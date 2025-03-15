using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal class PrintASTNode : StatementASTNode
    {
        public ExpressionASTNode Expr { get; set; }

        public PrintASTNode(ExpressionASTNode expr)
        {
            Expr = expr;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitPrint(this);
    }
}
