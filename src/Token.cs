using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter
{
    internal class Token
    {
        

        private readonly TokenType tokenType;
        private readonly string val;
        private readonly int line;
        private readonly int column;

        public TokenType TokenType => tokenType;
        public string Val => val;
        public int Line => line;
        public int Column => column;

        public Token(TokenType type, string val, int line, int column)
        {
            this.tokenType = type;
            this.val = val;
            this.line = line;
            this.column = column;
        }

        

        public override string ToString()
        {
            return $"{{{TokenType}, {Val}, {Line}, {Column}}}";
        }
    }
}
