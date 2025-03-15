using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal abstract class ASTNode
    {
        public abstract void Accept(IVisitor visitor);
    }
}
