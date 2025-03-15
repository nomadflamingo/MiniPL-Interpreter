using interpreter.parser.ASTNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace interpreter.Visitors
{
    internal class InterpreterVisitor : IVisitor
    {
        public void VisitRoot(RootASTNode node)
        {
            foreach (StatementASTNode stmt in node.Stmts)
            {
                stmt.Accept(this);
            }
        }

        // statemenets
        public void VisitIf(IfASTNode node)
        {
            node.Condition.Accept(this);  // visit the condition

            bool condition = (bool)node.Condition.Value;  // get condition value
            if (condition)
            {
                foreach (StatementASTNode stmt in node.IfStmts)
                {
                    stmt.Accept(this);
                }
            }
            else if (node.ElseStmts != null)
            {
                foreach (StatementASTNode stmt in node.ElseStmts)
                {
                    stmt.Accept(this);
                }
            }
        }

        public void VisitFor(ForASTNode node)
        {
            node.ControlVar.Accept(this);
            node.RangeBeginning.Accept(this);
            node.RangeEnd.Accept(this);

            int begin = (int)node.RangeBeginning.Value;
            int end = (int)node.RangeEnd.Value;

            node.ControlVar.Value = begin;
            for (int i = begin; i <= end; i++)
            {
                // iterate through all statements
                foreach (StatementASTNode stmt in node.Stmts)
                {
                    stmt.Accept(this);
                }
                

                // unlock control variable in the symbol table
                SymbolTable.SetModifiable(node.ControlVar.Name, true);

                // increment the control variable
                node.ControlVar.Value = (int)node.ControlVar.Value + 1;

                // lock back the control variable
                SymbolTable.SetModifiable(node.ControlVar.Name, false);
            }

            // unlock control variable in the end of loop
            SymbolTable.SetModifiable(node.ControlVar.Name, true);
        }

        public void VisitRead(ReadASTNode node)
        {
            node.VarId.Accept(this);
            string? userInput = Console.ReadLine();
            userInput ??= "";

            if (node.VarId.Type == MiniPLType.Int)
            {
                if (int.TryParse(userInput, out int res))
                {
                    node.VarId.Value = res;
                }
                else
                {
                    throw new ArgumentException($"Could not convert user input \"{userInput}\" to int");
                }
            }

            else if (node.VarId.Type == MiniPLType.String)
            {
                node.VarId.Value = userInput;
            }
        }

        public void VisitPrint(PrintASTNode node)
        {
            node.Expr.Accept(this);

            if (node.Expr.Value == null)
            {
                throw new NullReferenceException("Value for expression wasn't set");
            }

            Console.Write(node.Expr.Value);
        }

        public void VisitVarDef(VarDefASTNode node)
        {
            if (!IsVarDefined(node.VarId))
            {
                throw new ArgumentException($"Variable \"{node.VarId.Name}\" was not set in the symbol table before interpretation");
            }

            if (node.Expr == null)
            {
                // set default values
                switch (node.VarType)
                {
                    case MiniPLType.Bool:
                        node.VarId.Value = false;
                        break;
                    case MiniPLType.String:
                        node.VarId.Value = "";
                        break;
                    case MiniPLType.Int:
                        node.VarId.Value = 0;
                        break;
                    default:
                        throw new InvalidOperationException($"Default value for type \"{node.VarType}\" was not defined");
                }
                return;
            }

            // node.Expr != null
            node.Expr.Accept(this);
            if (node.Expr.Value == null)
            {
                throw new NullReferenceException("Value for expression wasn't set");
            }
            else
            {
                node.VarId.Value = node.Expr.Value;
            }

        }

        public void VisitVarAssign(VarAssignASTNode node)
        {
            node.VarId.Accept(this);
            node.Expr.Accept(this);

            if (node.Expr.Value == null)
            {
                throw new NullReferenceException("Value for expression wasn't set");
            }

            node.VarId.Value = node.Expr.Value;
        }

        // expressions
        public void VisitVarId(VarIdASTNode node)
        {
            // skip because nothing to run
        }

        public void VisitStringLiteral(StringLiteralASTNode node)
        {
            // we only set value here, despite it being known earlier, cause maybe later we'll need to check if the node was visited by the interpreter or not, idk
            

            node.Value = node.StringValue;
        }

        public void VisitIntLiteral(IntLiteralASTNode node)
        {
            node.Value = node.IntValue;
        }

        public void VisitBoolLiteral(BoolLiteralASTNode node)
        {
            node.Value = node.BoolValue;
        }

        public void VisitPlus(PlusASTNode node)
        {
            ExpressionASTNode lhs = node.Operands.ElementAt(0);
            ExpressionASTNode rhs = node.Operands.ElementAt(1);

            VisitExprsForBinaryOpAndCheckForNullValues(lhs, rhs, "+");

            switch (node.Type)
            {
                case MiniPLType.Int:
                    node.Value = (int)lhs.Value + (int)rhs.Value;
                    break;
                case MiniPLType.String:
                    node.Value = (string)lhs.Value + (string)rhs.Value;
                    break;
                default:
                    throw new NotImplementedException($"The \"+\" operation is not implemented for type \"{node.Type}\"");
            }
        }

        public void VisitMinus(MinusASTNode node)
        {
            ExpressionASTNode lhs = node.Operands.ElementAt(0);
            ExpressionASTNode rhs = node.Operands.ElementAt(1);

            VisitExprsForBinaryOpAndCheckForNullValues(lhs, rhs, "-");

            switch (node.Type)
            {
                case MiniPLType.Int:
                    node.Value = (int)lhs.Value - (int)rhs.Value;
                    break;
                default:
                    throw new NotImplementedException($"The \"-\" operation is not implemented for type \"{node.Type}\"");
            }
        }

        public void VisitMultiply(MultiplyASTNode node)
        {
            ExpressionASTNode lhs = node.Operands.ElementAt(0);
            ExpressionASTNode rhs = node.Operands.ElementAt(1);

            VisitExprsForBinaryOpAndCheckForNullValues(lhs, rhs, "*");

            switch (node.Type)
            {
                case MiniPLType.Int:
                    node.Value = (int)lhs.Value * (int)rhs.Value;
                    break;
                default:
                    throw new NotImplementedException($"The \"*\" operation is not implemented for type \"{node.Type}\"");
            }
        }

        public void VisitDivide(DivideASTNode node)
        {
            ExpressionASTNode lhs = node.Operands.ElementAt(0);
            ExpressionASTNode rhs = node.Operands.ElementAt(1);

            VisitExprsForBinaryOpAndCheckForNullValues(lhs, rhs, "/");

            switch (node.Type)
            {
                case MiniPLType.Int:
                    node.Value = (int)lhs.Value / (int)rhs.Value;
                    break;
                default:
                    throw new NotImplementedException($"The \"/\" operation is not implemented for type \"{node.Type}\"");
            }
        }

        public void VisitLessThan(LessThanASTNode node)
        {
            ExpressionASTNode lhs = node.Operands.ElementAt(0);
            ExpressionASTNode rhs = node.Operands.ElementAt(1);

            VisitExprsForBinaryOpAndCheckForNullValues(lhs, rhs, "<");

            switch (lhs.Type)
            {
                case MiniPLType.Int:
                    node.Value = (int)lhs.Value < (int)rhs.Value;
                    break;
                default:
                    throw new NotImplementedException($"The \"<\" operation is not implemented for type \"{node.Type}\"");
            }
        }

        public void VisitEquals(EqualsASTNode node)
        {
            ExpressionASTNode lhs = node.Operands.ElementAt(0);
            ExpressionASTNode rhs = node.Operands.ElementAt(1);

            VisitExprsForBinaryOpAndCheckForNullValues(lhs, rhs, "=");

            switch (lhs.Type)
            {
                case MiniPLType.Int:
                    node.Value = (int)lhs.Value == (int)rhs.Value;
                    break;
                case MiniPLType.Bool:
                    node.Value = (bool)lhs.Value == (bool)rhs.Value;
                    break;
                case MiniPLType.String:
                    node.Value = (string)lhs.Value == (string)rhs.Value;
                    break;
                default:
                    throw new NotImplementedException($"The \"=\" operation is not implemented for type \"{node.Type}\"");
            }
        }

        public void VisitLAnd(LAndASTNode node)
        {
            ExpressionASTNode lhs = node.Operands.ElementAt(0);
            ExpressionASTNode rhs = node.Operands.ElementAt(1);

            VisitExprsForBinaryOpAndCheckForNullValues(lhs, rhs, "&");

            node.Value = (bool)lhs.Value && (bool)rhs.Value;
        }

        public void VisitLNot(LNotASTNode node)
        {
            ExpressionASTNode opnd = node.Operands.ElementAt(0);

            opnd.Accept(this);
            if (opnd.Value == null)
            {
                throw new NullReferenceException("The value of the expression in \"!\" operation was not set");
            }

            node.Value = !(bool)opnd.Value;
        }

        private void VisitExprsForBinaryOpAndCheckForNullValues(ExpressionASTNode lhs, ExpressionASTNode rhs, string opName)
        {
            lhs.Accept(this);
            rhs.Accept(this);

            if (lhs.Value == null)
            {
                throw new NullReferenceException($"The value for the lhs of the \"{opName}\" operator wasn't set");
            }

            if (rhs.Value == null)
            {
                throw new NullReferenceException($"The value for the rhs of the \"{opName}\" operator wasn't set");
            }
        }

        private static bool IsVarDefined(VarIdASTNode var)
        {
            return SymbolTable.Contains(var.Name);
        }
    }
}
