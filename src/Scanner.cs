using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter
{
    internal class Scanner : IDisposable
    {
        private static readonly HashSet<int> specialSymbols = new() {  // special symbols don't have to be separated by spaces
            '+', '-', '*', '/',
            ';', ':', '&', '=', 
            '(', ')', '!', '<'
        };
        private static readonly HashSet<string> keywords = new() {  // keywords should be separated by spaces
            "var", "for", "end", "in", "do",
            "read", "print", "int", "string",
            "bool", "assert", "if", "else"
        };


        private readonly StreamReader r;
        private int line = 1;
        private int column = 1;

        public Scanner(StreamReader r)
        {
            this.r = r;
        }

        /// <summary>
        /// Returns the next token and consumes it or null if we reached the end of file
        /// </summary>
        /// <returns>The next token in the specified file or null if no more token found</returns>
        public Token? GetToken()
        {
            try
            {
                return GetTokenUnsafe();
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the next token and consumes it or throws an exception if we reached the end of file
        /// </summary>
        /// <returns>The next token in the specified file</returns>
        private Token GetTokenUnsafe()
        {
            // skip whitespaces
            SkipWhiteSpaces();

            // skip control characters
            if (char.IsControl((char)Peek()))
            {
                Consume();
                return GetTokenUnsafe();
            }

            // skip comments or detect '/'
            if (Peek() == '/')
            {
                Consume();
                if (Peek() == '*')  // we have multiple line comment
                {
                    SkipMultiLineComment();
                    return GetTokenUnsafe();

                }
                else if (Peek() == '/')  // we have one line comment
                {
                    SkipOneLineComment();
                    return GetTokenUnsafe();
                }
                else  // we have '/'
                {
                    return new Token(TokenType.Keyword, "/", line, column - 1);
                }
            }


            char c = (char)Peek();
            if (c == '\"')  // we're in a string 
            {
                return HandleString();
            }


            if (char.IsDigit(c)) // we have a number
            {

                int numberStartColumn = column;
                string digitString = ConsumeWhile(c => char.IsDigit((char)c));  // consume while it's not a digit

                return new Token(TokenType.Int, digitString, line, numberStartColumn);
            }

            // match the double dots
            if (c == '.')
            {
                Consume();
                if (Peek() == '.')
                {
                    Consume();
                    return new Token(TokenType.Keyword, "..", line, column - 2);
                }
                else
                {
                    throw new ArgumentException($"Line: {line}, Column: {column - 1}; Unrecognized token");
                }
            }

            // match the ':' or ':='
            if (c == ':')
            {
                Consume();
                if (Peek() == '=')
                {
                    Consume();
                    return new Token(TokenType.Keyword, ":=", line, column - 2);
                }
                else
                {
                    return new Token(TokenType.Keyword, ":", line, column - 1);
                }
            }

            // match any other one char symbol
            if (specialSymbols.Contains(c))
            {
                Consume();
                return new Token(TokenType.Keyword, c.ToString(), line, column - 1);
            }

            // scan the file until the next weird char (not a letter or underscore)
            string s = ConsumeWhile(c => char.IsLetterOrDigit((char)c) || c == '_');

            // if s is empty, then we reached some kind of very werid char
            // that is not allowed as an identifier, but is also not recognized
            // as a special character, which means that it's probably not a part of a language

            if (s == "")
            {
                // it could be an end of stream, so check for that first
                if (Peek() == -1)
                {
                    HandleEndOfStream();
                }
                else
                {
                    HandleUnrecognizedChar(line, column);
                }
            }


            // check for "true" or "false"
            if (s == "true" || s == "false")
            {
                return new Token(TokenType.Bool, s, line, column - s.Length);
            }

            // if found a keyword, mark as a keyword
            if (keywords.Contains(s))
            {
                return new Token(TokenType.Keyword, s, line, column - s.Length);
            }
            else  // if not, mark as identifier
            {
                return new Token(TokenType.Id, s, line, column - s.Length);
            }
        }

        private void HandleNewLine()
        {
            line++;
            column = 1;
        }

        private Token HandleString()
        {
            int startColumnNumber = column;
            StringBuilder s = new();
            Consume();

            while (true)
            {
                s.Append(ConsumeWhile(c => c != '\"' && c != '\\' && c != '\n'));

                if (Peek() == '\"')  // the string ended
                {
                    Consume();
                    return new Token(TokenType.String, s.ToString(), line, startColumnNumber);
                }
                else if (Peek() == '\\')  // escape char
                {
                    Consume();
                    char nextChar = (char)Peek();
                    char replaceChar = nextChar switch
                    {
                        'r' => '\r',
                        'n' => '\n',
                        'a' => '\a',
                        'b' => '\b',
                        'f' => '\f',
                        't' => '\t',
                        'v' => '\v',
                        '\\' => '\\',
                        '\"' => '\"',
                        _ => throw new ArgumentException($"Line: {line}, Column: {column}; Unrecognized escape character"),
                    };

                    s.Append(replaceChar);
                    Consume();
                }
                else if (Peek() == '\n')  // string wasn't closed before newline, so throw error
                {
                    int exceptionLine = line;
                    int exceptionColumn = column;
                    Consume();
                    throw new ArgumentException($"Line: {exceptionLine}, Column: {exceptionColumn}; String wasn't closed before newline");
                }
            }
        }


        private void SkipWhiteSpaces()
        {
            ConsumeWhile(c => c == ' ');
        }

        private void SkipMultiLineComment()
        {
            Consume();
            while (true)
            {
                ConsumeWhile(c => c != '*');
                Consume();  // consume the star
                if (Peek() == '/')
                {
                    // close the comment
                    Consume();
                    break;
                }
                else
                {
                    continue;
                }
            }
        }

        private void SkipOneLineComment()
        {
            Consume();
            r.ReadLine();
            HandleNewLine();
        }

        /// <summary>
        /// Consumes the stream while the condition is met. Puts the reader cursor position before the first element that doesn't satisfy the condition
        /// </summary>
        /// <param name="condition">The condition that the character should satisfy in order to be consumed</param>
        private string ConsumeWhile(Func<int, bool> condition)
        {
            StringBuilder s = new();
            int nextElem = Peek();

            while (condition(nextElem))
            {
                char c = (char)Consume();
                s.Append(c);
                nextElem = Peek();
            }

            return s.ToString();
        }

        private int Consume()
        {
            int c = r.Read();
            column++;

            if (c == '\n')
            {
                HandleNewLine();
            }

            if (c == -1)
            {
                HandleEndOfStream();
            }
            return c;
        }

        private int Peek()
        {
            return r.Peek();
        }

        private static void HandleUnrecognizedChar(int line, int column)
        {
            throw new ArgumentException($"Unrecognized character at line: {line}, column: {column}");
        }

        private static void HandleEndOfStream()
        {
            throw new EndOfStreamException();
        }


        public void Dispose()
        {
            r.Close();
        }
    }
}
