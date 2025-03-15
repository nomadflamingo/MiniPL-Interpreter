using interpreter.parser.ASTNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter.Visitors
{
    internal interface IVisitor
    {
        public void VisitRoot(RootASTNode node);

        // statemenets
        public void VisitIf(IfASTNode node);
        public void VisitFor(ForASTNode node);
        public void VisitRead(ReadASTNode node);
        public void VisitPrint(PrintASTNode node);
        public void VisitVarDef(VarDefASTNode node);
        public void VisitVarAssign(VarAssignASTNode node);

        // expressions
        public void VisitVarId(VarIdASTNode node);
        public void VisitStringLiteral(StringLiteralASTNode node);
        public void VisitIntLiteral(IntLiteralASTNode node);
        public void VisitBoolLiteral(BoolLiteralASTNode node);
        public void VisitPlus(PlusASTNode node);
        public void VisitMinus(MinusASTNode node);
        public void VisitMultiply(MultiplyASTNode node);
        public void VisitDivide(DivideASTNode node);
        public void VisitLessThan(LessThanASTNode node);
        public void VisitEquals(EqualsASTNode node);
        public void VisitLAnd(LAndASTNode node);
        public void VisitLNot(LNotASTNode node);
    }
}
