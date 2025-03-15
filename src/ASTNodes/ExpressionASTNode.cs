using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter.parser.ASTNodes
{
    internal abstract class ExpressionASTNode : ASTNode
    {
        public virtual object? Value { get; set; }  // set by interpreterVisitor

        public virtual MiniPLType? Type { get; set; } // set by typeCheckVisitor
    }
}
