using interpreter.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter.parser.ASTNodes
{
    internal class VarDefASTNode : StatementASTNode
    {
        public VarIdASTNode VarId { get; set; }
        public MiniPLType VarType { get; set; } 
        public ExpressionASTNode? Expr { get; set; }  // if set to null, will be marked as not initialized in symbol table until it's not initialized

        public VarDefASTNode(VarIdASTNode varId, MiniPLType type, ExpressionASTNode? expr = null)
        {
            VarId = varId;
            VarType = type;
            Expr = expr;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitVarDef(this);
    }
}
