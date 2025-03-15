using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal class ForASTNode : StatementASTNode
    {
        public VarIdASTNode ControlVar { get; set; }
        public IEnumerable<StatementASTNode> Stmts { get; }
        public ExpressionASTNode RangeBeginning { get; set; } 
        public ExpressionASTNode RangeEnd { get; set; }


        public ForASTNode(VarIdASTNode controlVar, ExpressionASTNode rangeBeginning, ExpressionASTNode rangeEnd, IEnumerable<StatementASTNode> stmts)
        {
            ControlVar = controlVar;
            RangeBeginning = rangeBeginning;
            RangeEnd = rangeEnd;
            Stmts = stmts;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitFor(this);
    }
}
