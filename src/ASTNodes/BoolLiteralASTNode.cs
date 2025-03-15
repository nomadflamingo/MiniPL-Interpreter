using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal class BoolLiteralASTNode : LiteralASTNode
    {
        public bool BoolValue { get; init; }
        public BoolLiteralASTNode(bool value)
        {
            BoolValue = value;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitBoolLiteral(this);
    }
}
