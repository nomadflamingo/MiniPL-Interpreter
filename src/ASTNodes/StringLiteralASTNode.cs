using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal class StringLiteralASTNode : LiteralASTNode
    {
        public string StringValue { get; init; }
        public StringLiteralASTNode(string value)
        {
            StringValue = value;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitStringLiteral(this);
    }
}
