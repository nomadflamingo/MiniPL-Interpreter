using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter.parser.ASTNodes
{
    internal abstract class LiteralASTNode : ExpressionASTNode
    {
        // for bool values, we don't immediately set the value in the value field in expression,
        // but instead we still set it in the interpreterVisitor, as any other expressions
        // maybe later we'll need to determine if the node was visited by the interpreterVisitor
        // and this will tell us that, cause the value will be null
    }
}
