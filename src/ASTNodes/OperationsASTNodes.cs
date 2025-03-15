using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using interpreter.Visitors;

namespace interpreter.parser.ASTNodes
{
    internal abstract class OperationASTNode : ExpressionASTNode
    {
        public IEnumerable<ExpressionASTNode> Operands { get; set; }

        public OperationASTNode(IEnumerable<ExpressionASTNode> operands)
        {
            Operands = operands;
        }
    }

    internal class PlusASTNode : OperationASTNode
    {
        public PlusASTNode(IEnumerable<ExpressionASTNode> operands) : base(operands)
        {
            Operands = operands;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitPlus(this);
    }

    internal class MinusASTNode : OperationASTNode
    {
        public MinusASTNode(IEnumerable<ExpressionASTNode> operands) : base(operands)
        {
            Operands = operands;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitMinus(this);
    }

    internal class MultiplyASTNode : OperationASTNode
    {
        public MultiplyASTNode(IEnumerable<ExpressionASTNode> operands) : base(operands)
        {
            Operands = operands;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitMultiply(this);
    }

    internal class DivideASTNode : OperationASTNode
    {
        public DivideASTNode(IEnumerable<ExpressionASTNode> operands) : base(operands)
        {
            Operands = operands;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitDivide(this);
    }

    internal class LessThanASTNode : OperationASTNode
    {
        public LessThanASTNode(IEnumerable<ExpressionASTNode> operands) : base(operands)
        {
            Operands = operands;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitLessThan(this);
    }

    internal class EqualsASTNode : OperationASTNode
    {
        public EqualsASTNode(IEnumerable<ExpressionASTNode> operands) : base(operands)
        {
            Operands = operands;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitEquals(this);
    }

    internal class LAndASTNode : OperationASTNode
    {
        public LAndASTNode(IEnumerable<ExpressionASTNode> operands) : base(operands)
        {
            Operands = operands;
        }
        
        public override void Accept(IVisitor visitor) => visitor.VisitLAnd(this);
    }

    internal class LNotASTNode : OperationASTNode
    {
        public LNotASTNode(IEnumerable<ExpressionASTNode> operands) : base(operands)
        {
            Operands = operands;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitLNot(this);
    }
}
