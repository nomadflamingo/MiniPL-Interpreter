using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal class IntLiteralASTNode : LiteralASTNode
    {
        public int IntValue { get; init; }

        public IntLiteralASTNode(int value)
        {
            IntValue = value;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitIntLiteral(this);
    }
}
