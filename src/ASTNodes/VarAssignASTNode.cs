using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal class VarAssignASTNode : StatementASTNode
    {
        public VarIdASTNode VarId { get; }
        public ExpressionASTNode Expr { get; }

        public VarAssignASTNode(VarIdASTNode varId, ExpressionASTNode expr)
        {
            VarId = varId;
            Expr = expr;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitVarAssign(this);
    }
}
