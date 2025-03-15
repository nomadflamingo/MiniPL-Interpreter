using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using interpreter.parser.ASTNodes;

namespace interpreter
{
    internal class Parser
    {
        private uint depth = 0;  // increases with each for or if statement we encounter. decreases when we reach see "end" keyword
        private bool fileEnded = false;
        private readonly Scanner sc;
        private Token currToken;


        public Parser(Scanner sc)
        {
            this.sc = sc;
            NextToken();
        }

        public RootASTNode Parse()
        {
            return Program();
        }

        private RootASTNode Program()
        {
            StatementASTNode stmt = Statement();  // at least one statement is required
            MatchKeyword(";");

            List<StatementASTNode> stmts = new() { stmt };  //  convert it to list
            return new RootASTNode(stmts.Concat(Statements()));  // add other possible statements to it and create a root ast node for all of them
        }

        /// <summary>
        /// Produces zero or more statements
        /// </summary>
        /// <returns>IEnumerable containing zero or more statements</returns>
        private IEnumerable<StatementASTNode> Statements()
        {
            List<StatementASTNode> stmts = new();

            while (!fileEnded)
            {
                // check for "end" keyword at the beginning of the sentence
                // if depth is 0, we are in a global scope and should throw an error because "end" is not a statement
                // the error is thrown automatically after we try to match for a statement
                // if depth > 0, return the statements (we exited "if" or "for")
                if (CheckKeyword("end") && !InGlobalScope()) return stmts;

                stmts.Add(Statement());
                MatchKeyword(";");
            }
            return stmts;
        }

        private StatementASTNode Statement()
        {
            // Explicit variable initialization with "var"
            if (CheckKeyword("var"))
            {
                // not allowed to declare vars in local scope
                if (!InGlobalScope())
                {
                    throw new ArgumentException(
                        $"Line: {currToken.Line}, " +
                        $"Col: {currToken.Column}; " +
                        $"Not allowed to declare variables inside local scope");
                }

                NextToken();
                VarIdASTNode varId = VarIdent();  // name of the new variable
                MatchKeyword(":");
                MiniPLType type = Type();  // type of the new variable

                ExpressionASTNode? expr = null;
                if (CheckKeyword(":="))  // currToken could be null here
                {
                    NextToken();
                    expr = Expr();
                }
                return new VarDefASTNode(varId, type, expr);
            }

            // Variable assignment
            else if (CheckTokenType(TokenType.Id))
            {
                VarIdASTNode varId = VarIdent();
                MatchKeyword(":=");
                ExpressionASTNode expr = Expr();

                return new VarAssignASTNode(varId, expr);
            }

            // For loop
            else if (CheckKeyword("for"))
            {
                NextToken();
                VarIdASTNode varId = VarIdent();
                MatchKeyword("in");
                ExpressionASTNode rangeBeginning = Expr();
                MatchKeyword("..");
                ExpressionASTNode rangeEnd = Expr();
                MatchKeyword("do");

                EnterScope();
                IEnumerable<StatementASTNode> stmts = Statements();
                ExitScope();

                MatchKeyword("end");
                MatchKeyword("for");

                return new ForASTNode(varId, rangeBeginning, rangeEnd, stmts);
            }

            // Read statement
            else if (CheckKeyword("read"))
            {
                NextToken();
                VarIdASTNode varId = VarIdent();
                return new ReadASTNode(varId);
            }

            // Print statement
            else if (CheckKeyword("print"))
            {
                NextToken();
                ExpressionASTNode expr = Expr();
                return new PrintASTNode(expr);
            }

            // If statement
            else if (CheckKeyword("if"))
            {
                NextToken();
                ExpressionASTNode condition = Expr();
                MatchKeyword("do");

                EnterScope();
                IEnumerable<StatementASTNode> ifStmts = Statements();
                ExitScope();

                IEnumerable<StatementASTNode>? elseStmts = null;
                if (CheckKeyword("else"))
                {
                    NextToken();

                    EnterScope();
                    elseStmts = Statements();
                    ExitScope();
                }

                MatchKeyword("end");
                MatchKeyword("if");

                return new IfASTNode(condition, ifStmts, elseStmts);
            }

            else
            {
                string expected = "a statement (either an \"if\", \"for\", \"print\", \"read\", or \"var\")"; // TODO: separate error messages from code

                throw new ArgumentException(
                    $"Line: {currToken.Line}, " +
                    $"Col: {currToken.Column}; " +
                    $"Unexpected token. Expected {expected} " +
                    $"Found \"{currToken.TokenType}\" \"{currToken.Val}\"");
            }
        }


        private ExpressionASTNode Expr()
        {
            List<ExpressionASTNode> opnds = new();

            // "!". The only unary operation
            if (CheckKeyword("!"))
            {
                NextToken();
                opnds.Add(Opnd());
                return new LNotASTNode(opnds);
            }

            // Operand with a possible binary operation
            else if (CheckTokenType(TokenType.Id, TokenType.String, TokenType.Int, TokenType.Bool))
            {
                opnds.Add(Opnd());

                // if we have a binary op after operand
                if (CheckKeyword("+", "-", "*", "/", "<", "=", "&"))
                {
                    string symbol = currToken.Val;  // save the symbol we got for the operation
                    NextToken();
                    opnds.Add(Opnd());  // match the second operand
                    OperationASTNode? op =
                        symbol == "+" ? new PlusASTNode(opnds) :
                        symbol == "-" ? new MinusASTNode(opnds) :
                        symbol == "*" ? new MultiplyASTNode(opnds) :
                        symbol == "/" ? new DivideASTNode(opnds) :
                        symbol == "<" ? new LessThanASTNode(opnds) :
                        symbol == "=" ? new EqualsASTNode(opnds) :
                        symbol == "&" ? new LAndASTNode(opnds) :
                        null;

                    if (op == null)  // should never happen, but just in case we forget something
                    {
                        throw new ArgumentException(
                            $"Line: {currToken.Line}, " +
                            $"Col: {currToken.Column}; " +
                            $"Unexpected token. Expected a binary operation " +
                            $"Found \"{currToken.TokenType}\" \"{currToken.Val}\"");
                    }

                    return op!;
                }
                else  // we only have one operand
                {
                    return opnds[0];
                }

            }

            // Expression in parentheses
            else if (CheckKeyword("("))
            {
                NextToken();
                ExpressionASTNode expr = Expr();
                MatchKeyword(")");
                return expr;
            }

            else
            {
                throw new ArgumentException(
                    $"Line: {currToken.Line}, " +
                    $"Col: {currToken.Column}; " +
                    $"Unexpected token. Expected an operand or an operation " +
                    $"Found \"{currToken.TokenType}\" \"{currToken.Val}\"");
            }
        }

        private ExpressionASTNode Opnd()
        {
            ExpressionASTNode expr;

            // current token is integer
            if (CheckTokenType(TokenType.Int))
            {
                int val = int.Parse(currToken.Val);
                expr = new IntLiteralASTNode(val);
                NextToken();
            }

            // current token is a string
            else if (CheckTokenType(TokenType.String))
            {
                expr = new StringLiteralASTNode(currToken.Val);
                NextToken();
            }

            // current token is a bool (true or false)
            else if (CheckTokenType(TokenType.Bool))
            {
                // capitalize, cause c# doesn't recognize lowercase bools for some reason
                // (but now both lowercase and uppercase works)
                bool val = bool.Parse(Capitalize(currToken.Val));  
                
                expr = new BoolLiteralASTNode(val);
                NextToken();
            }

            // current token is an identifier
            else if (CheckTokenType(TokenType.Id))
            {
                expr = new VarIdASTNode(currToken.Val);
                NextToken();
            }

            // another expression
            else if (CheckKeyword("("))
            {
                NextToken();
                expr = Expr();
                MatchKeyword(")");
            }

            else
            {
                throw new ArgumentException(
                    $"Line: {currToken.Line}, " +
                    $"Col: {currToken.Column}; " +
                    $"Unexpected token. Expected an operand " +
                    $"Found \"{currToken.TokenType}\" \"{currToken.Val}\"");
            }

            return expr;
        }

        private VarIdASTNode VarIdent()
        {
            VarIdASTNode node = new(currToken.Val);
            MatchTokenType(TokenType.Id);
            return node;
        }

        private MiniPLType Type()
        {
            if (CheckKeyword("int"))
            {
                NextToken();
                return MiniPLType.Int;
            }

            else if (CheckKeyword("string"))
            {
                NextToken();
                return MiniPLType.String;
            }

            else if (CheckKeyword("bool"))
            {
                NextToken();
                return MiniPLType.Bool;
            }

            else
            {
                throw new ArgumentException(
                    $"Line: {currToken.Line}, " +
                    $"Col: {currToken.Column}; " +
                    $"Unexpected token. Expected a valid type name " +
                    $"Found \"{currToken.TokenType}\" \"{currToken.Val}\"");
            }
        }

        /// <summary>
        /// Checks if the current token is a keyword containing any of the specified values.
        /// Returns the result of the check. Returns false if the token is null
        /// </summary>
        /// <param name="keywordToMatch">Value of token to match</param>
        private bool CheckKeyword(params string[] keywordsToCheck)
        {
            bool found = false;
            foreach (string keyword in keywordsToCheck)
            {
                if (keyword == currToken.Val && currToken.TokenType == TokenType.Keyword)
                {
                    found = true; break;
                }
            }
            return found;
        }

        /// <summary>
        /// Makes sure that the current token is a keyword containing a specified value. 
        /// If so, consumes it and sets the current token to point to the next token, else throws ArgumentException
        /// </summary>
        /// <param name="keywordToMatch">Value of token to match</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="EndOfStreamException">Thrown if current token is null</exception>
        private void MatchKeyword(string keywordToMatch)
        {
            if (keywordToMatch == currToken.Val && currToken.TokenType == TokenType.Keyword)
            {
                NextToken();  // TODO: catch exception and specify what was expected to match
            }
            else
            {
                throw new ArgumentException(
                    $"Line: {currToken.Line}, " +
                    $"Col: {currToken.Column}; " +
                    $"Unexpected token. Expected keyword \"{keywordToMatch}\". " +
                    $"Found \"{currToken.TokenType}\" \"{currToken.Val}\"");
            }
        }


        /// <summary>
        /// Checks if the current token is of any of the specified types.
        /// Returns the result of the check. Returns false if the token is null
        /// </summary>
        /// <param name="typesToCheck">TokenTypes to match</param>
        private bool CheckTokenType(params TokenType[] typesToCheck)
        {
            bool found = false;
            foreach (TokenType type in typesToCheck)
            {
                if (type == currToken.TokenType)
                {
                    found = true; break;
                }
            }
            return found;
        }

        /// <summary>
        /// Makes sure that the current token is of specified type. 
        /// If so, consumes it and proceeds to the next token, else throws ArgumentException
        /// </summary>
        /// <param name="typeToMatch">Type of token to match</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="EndOfStreamException">Thrown if current token is null</exception>
        private void MatchTokenType(TokenType typeToMatch)
        {

            if (currToken.TokenType == typeToMatch)
            {
                NextToken();  // TODO: catch exception and specify what was expected to match
            }
            else
            {
                throw new ArgumentException(
                    $"Line: {currToken.Line}, " +
                    $"Col: {currToken.Column}; " +
                    $"Unexpected token. Expected type \"{typeToMatch}\". " +
                    $"Found \"{currToken.TokenType}\"");
            }
        }

        /// <summary>
        /// Consumes the next token in the scanner token stream 
        /// and makes the parser point to the consumed token.
        /// If there are no more token and the file has ended correctly 
        /// (in global scope and the last character was a semicolon), 
        /// then it marks the file as ended and returns, 
        /// if the file ended incorrectly, throws end of stream exception
        /// </summary>
        private void NextToken()
        {
            Token? nextToken = sc.GetToken();

            // if file ended, check if we're in global scope and the statement has ended, else throw EndOfStreamException
            if (nextToken == null)
            {
                // there should be at least one statement in the file,
                // and because we can't check for the semicolon if currToken is null,
                // we have check that it is not null
                if (currToken == null)
                {
                    throw new EndOfStreamException("Unexpected end of file");
                }


                if (InGlobalScope() && CheckKeyword(";"))
                {
                    fileEnded = true;
                    return;
                }
                else
                {
                    throw new EndOfStreamException("Unexpected end of file");
                }

            }

            currToken = nextToken;
        }


        private void EnterScope()
        {
            depth++;
        }

        private void ExitScope()
        {
            depth--;
        }

        private bool InGlobalScope()
        {
            return depth == 0;
        }

        private static string Capitalize(string input) =>
            input switch
            {
                "" => throw new ArgumentException($"Tried to capitalize an empty string"),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
    }
}
