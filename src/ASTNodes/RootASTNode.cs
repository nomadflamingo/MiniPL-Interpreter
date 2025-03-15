using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal class RootASTNode : ASTNode
    {
        public IEnumerable<StatementASTNode> Stmts { get; set; }

        public RootASTNode(IEnumerable<StatementASTNode> stmts)
        {
            Stmts = stmts;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitRoot(this);
    }
}
