using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal class IfASTNode : StatementASTNode
    {
        public ExpressionASTNode Condition { get; set; }
        public IEnumerable<StatementASTNode> IfStmts { get; set; }
        public IEnumerable<StatementASTNode>? ElseStmts { get; set; }

        public IfASTNode(ExpressionASTNode condition, IEnumerable<StatementASTNode> ifStmts, IEnumerable<StatementASTNode>? elseStmts = null)
        {
            Condition = condition;
            IfStmts = ifStmts;
            ElseStmts = elseStmts;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitIf(this);
    }
}
