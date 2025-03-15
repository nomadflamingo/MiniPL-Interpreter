using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal class ReadASTNode : StatementASTNode
    {
        public VarIdASTNode VarId { get; set; }

        public ReadASTNode(VarIdASTNode varId)
        {
            VarId = varId;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitRead(this);
    }
}
