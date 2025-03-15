using interpreter.parser.ASTNodes;
using System.Linq;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace interpreter.Visitors
{
    internal class TypeCheckVisitor : IVisitor
    {

        public void VisitRoot(RootASTNode node)
        {
            foreach (StatementASTNode stmt in node.Stmts)
            {
                stmt.Accept(this);
            }
        }

        // statements
        public void VisitIf(IfASTNode node)
        {
            node.Condition.Accept(this);

            foreach (StatementASTNode stmt in node.IfStmts)
            {
                stmt.Accept(this);
            }

            if (node.ElseStmts != null)
            {
                foreach (StatementASTNode stmt in node.ElseStmts)
                {
                    stmt.Accept(this);
                }
            }
        }

        public void VisitFor(ForASTNode node)
        {
            node.ControlVar.Accept(this);
            MatchType(node.ControlVar, MiniPLType.Int);

            // lock the control variable to make sure it was not modified
            SetVarModifiable(node.ControlVar, false);

            node.RangeBeginning.Accept(this);
            MatchType(node.RangeBeginning, MiniPLType.Int);

            node.RangeEnd.Accept(this);
            MatchType(node.RangeEnd, MiniPLType.Int);

            // type check every statement
            foreach (StatementASTNode stmt in node.Stmts)
            {
                stmt.Accept(this);
            }

            // unlock the control variable
            SetVarModifiable(node.ControlVar, true);
        }

        public void VisitRead(ReadASTNode node)
        {
            // could read either int or string
            node.VarId.Accept(this);
            MatchType(node.VarId, MiniPLType.String, MiniPLType.Int);
        }

        public void VisitPrint(PrintASTNode node)
        {
            // could print any type (string, int, bool)
            node.Expr.Accept(this);
            MatchType(node.Expr, MiniPLType.String, MiniPLType.Int, MiniPLType.Bool);
        }

        public void VisitVarDef(VarDefASTNode node)
        {
            // check if the variable was already defined, if so, throw an error
            if (IsVarDefined(node.VarId))
            {
                throw new ArgumentException($"Cannot redefine an already defined variable \"{node.VarId.Name}\"");
            }

            // it doesn't make sense to visit var id here,
            // because it's being defined for the first time here, and obviously
            // there will be no symbol table entry so no need to check it

            AddVar(node.VarId);
            node.VarId.Type = node.VarType;

            if (node.Expr != null)
            {
                node.Expr.Accept(this);
                MatchType(node.Expr, node.VarType);  // throw error if expression is not of the same type
            }
        }

        public void VisitVarAssign(VarAssignASTNode node)
        {
            // check that the variable has been defined
            if (!IsVarDefined(node.VarId))
            {
                throw new InvalidOperationException($"Use of uninitialized variable \"{node.VarId.Name}\"");
            }

            // if found, get its type
            MiniPLType? varType = node.VarId.Type;
            if (varType == null)
            {
                throw new InvalidOperationException($"Cannot assign value to a variable \"{node.VarId.Name}\" because its type was not specified");
            }

            // figure out the expression type
            node.Expr.Accept(this);
            if (node.Expr.Type != varType)
            {
                throw new InvalidOperationException($"Cannot assign value of type \"{node.Expr.Type}\" " +
                    $"to a variable \"{node.VarId.Name}\" of type \"{varType}\"");
            }

            MiniPLType exprType = (MiniPLType)node.Expr.Type;
            node.VarId.Type = exprType;
        }

        // expressions

        public void VisitVarId(VarIdASTNode node)
        {
            if (!IsVarDefined(node))
            {
                throw new ArgumentException($"Tried to use an undefined variable \"{node.Name}\"");
            }

            // no need to update the type in ast node, because it's alredy referred from the symbol table
        }

        public void VisitStringLiteral(StringLiteralASTNode node)
        {
            node.Type = MiniPLType.String;
        }

        public void VisitIntLiteral(IntLiteralASTNode node)
        {
            node.Type = MiniPLType.Int;
        }

        public void VisitBoolLiteral(BoolLiteralASTNode node)
        {
            node.Type = MiniPLType.Bool;
        }

        // operations
        public void VisitPlus(PlusASTNode node)
        {
            // check operands count
            CheckOperandsCount(node, 2);

            // visit every operand
            foreach (ExpressionASTNode expr in node.Operands)
            {
                expr.Accept(this);
            }

            // check operands types
            MiniPLType[] allowedTypes = { MiniPLType.Int, MiniPLType.String };

            CheckOperandsTypesForBinaryOp(node, allowedTypes, allowDifferentTypes: false);

            // set operation type
            node.Type = node.Operands.ElementAt(0).Type;  // first operand (we know that it's the second one as well)
        }
        public void VisitMinus(MinusASTNode node)
        {
            // check operands count
            CheckOperandsCount(node, 2);

            // visit every operand
            foreach (ExpressionASTNode expr in node.Operands)
            {
                expr.Accept(this);
            }

            // check operands types
            MiniPLType[] allowedTypes = { MiniPLType.Int };

            CheckOperandsTypesForBinaryOp(node, allowedTypes, allowDifferentTypes: false);

            // set operation type
            node.Type = node.Operands.ElementAt(0).Type;  // first operand (we know that it's the second one as well)
        }
        public void VisitMultiply(MultiplyASTNode node)
        {
            // check operands count
            CheckOperandsCount(node, 2);

            // visit every operand
            foreach (ExpressionASTNode expr in node.Operands)
            {
                expr.Accept(this);
            }

            // check operands types
            MiniPLType[] allowedTypes = { MiniPLType.Int };

            CheckOperandsTypesForBinaryOp(node, allowedTypes, allowDifferentTypes: false);

            // set operation type
            node.Type = node.Operands.ElementAt(0).Type;  // first operand (we know that it's the second one as well)
        }
        public void VisitDivide(DivideASTNode node)
        {
            // check operands count
            CheckOperandsCount(node, 2);

            // visit every operand
            foreach (ExpressionASTNode expr in node.Operands)
            {
                expr.Accept(this);
            }

            // check operands types
            MiniPLType[] allowedTypes = { MiniPLType.Int };

            CheckOperandsTypesForBinaryOp(node, allowedTypes, allowDifferentTypes: false);

            // set operation type
            node.Type = node.Operands.ElementAt(0).Type;  // first operand (we know that it's the second one as well)
        }
        public void VisitLessThan(LessThanASTNode node)
        {
            // check operands count
            CheckOperandsCount(node, 2);

            // visit every operand
            foreach (ExpressionASTNode expr in node.Operands)
            {
                expr.Accept(this);
            }

            // check operands types
            MiniPLType[] allowedTypes = { MiniPLType.Int };

            CheckOperandsTypesForBinaryOp(node, allowedTypes, allowDifferentTypes: false);

            // set operation type
            node.Type = MiniPLType.Bool;
        }
        public void VisitEquals(EqualsASTNode node)
        {
            // check operands count
            CheckOperandsCount(node, 2);

            // visit every operand
            foreach (ExpressionASTNode expr in node.Operands)
            {
                expr.Accept(this);
            }

            // check operands types
            MiniPLType[] allowedTypes = { MiniPLType.Int, MiniPLType.String, MiniPLType.Bool };

            CheckOperandsTypesForBinaryOp(node, allowedTypes, allowDifferentTypes: false);

            // set operation type
            node.Type = MiniPLType.Bool;
        }

        public void VisitLAnd(LAndASTNode node)
        {
            // check operands count
            CheckOperandsCount(node, 2);

            // visit every operand
            foreach (ExpressionASTNode expr in node.Operands)
            {
                expr.Accept(this);
            }

            // check operands types
            MiniPLType[] allowedTypes = { MiniPLType.Bool };

            CheckOperandsTypesForBinaryOp(node, allowedTypes, allowDifferentTypes: false);

            // set operation type
            node.Type = MiniPLType.Bool;
        }

        public void VisitLNot(LNotASTNode node)
        {
            // check operands count
            CheckOperandsCount(node, 1);

            // visit the only operand
            ExpressionASTNode opnd = node.Operands.ElementAt(0);
            opnd.Accept(this);

            // check operand type
            if (opnd.Type != MiniPLType.Bool)
            {
                throw new ArgumentException($"\"Logical not\" operation is supported only for boolean types. Found type \"{opnd.Type}\"");
            }

            // set operation type
            node.Type = MiniPLType.Bool;
        }

        /// <summary>
        /// Makes sure that the expression is of any of the specified types
        /// </summary>
        /// <param name="expr">expression ast node</param>
        /// <param name="type">types that the expression can hold</param>
        /// <exception cref="ArgumentException"></exception>
        private static void MatchType(ExpressionASTNode expr, params MiniPLType[] types)
        {
            MiniPLType? exprType = expr.Type;

            if (exprType == null)
            {
                throw new ArgumentException($"Type of expression \"{expr}\" was not specified");
            }

            bool found = false;
            foreach (MiniPLType type in types)
            {
                if (exprType == type)
                {
                    found = true;
                }
            }

            if (!found)
            {
                // check if we used uninitialized variable for better error message
                if (expr is VarIdASTNode)
                {
                    VarIdASTNode id = (VarIdASTNode)expr;
                    if (id.Type == null)
                    {
                        throw new ArgumentException($"Use of uninitialized variable \"{id.Name}\"");
                    }
                }

                string expected = string.Join(", ", types);
                throw new ArgumentException($"Unexpected type. Expected any of {expected}. Was {exprType}");
            }
        }

        /// <summary>
        /// Checks that the expression is of any of the specified types. 
        /// Returns false if type of the expression is null
        /// </summary>
        /// <param name="var">variable identifier ast node</param>
        /// <param name="types">types that the variable can hold</param>
        /// <returns>the result of the check</returns>
        private bool CheckType(ExpressionASTNode expr, params MiniPLType[] types)
        {
            MiniPLType? exprType = expr.Type;

            if (exprType == null)
            {
                return false;
            }

            bool found = false;
            foreach (MiniPLType type in types)
            {
                if (exprType == type)
                {
                    found = true;
                }
            }
            return found;
        }

        private static void CheckOperandsTypesForBinaryOp(OperationASTNode node, IEnumerable<MiniPLType> allowedTypes, bool allowDifferentTypes)
        {
            string expected = string.Join(", ", allowedTypes);  // for debugging

            // get lhs, rhs
            ExpressionASTNode lhs = node.Operands.ElementAt(0);
            ExpressionASTNode rhs = node.Operands.ElementAt(1);

            if (!allowDifferentTypes)
            {
                // check that both sides are of the same type
                if (lhs.Type != rhs.Type)
                {
                    throw new ArgumentException("Type mismatch between two operands");
                }

                // now we know that rhs type is the same, so we only check for lhs type
                MiniPLType t = (MiniPLType)lhs.Type;  // type is not null cause operands were hopefully visited
                if (!allowedTypes.Contains(t))
                {
                    throw new ArgumentException($"Binary operation is not supported for operands of type {lhs.Type}. " +
                        $"Expected any of {expected}");
                }
            }
            else
            {
                MiniPLType tLeft = (MiniPLType)lhs.Type;
                MiniPLType tRight = (MiniPLType)rhs.Type;

                if (!allowedTypes.Contains(tLeft))
                {
                    throw new ArgumentException($"Binary operation is not supported for an operand of type {lhs.Type}. " +
                        $"Expected any of {expected}");
                }

                if (!allowedTypes.Contains(tRight))
                {
                    throw new ArgumentException($"Binary operation is not supported for an operand of type {rhs.Type}. " +
                        $"Expected any of {expected}");
                }
            }
        }

        private static void CheckOperandsCount(OperationASTNode node, int expectedCount)
        {
            int operandsCount = node.Operands.Count();
            if (operandsCount != expectedCount)
            {
                throw new ArgumentException($"Invalid number of operands an operation. Expected \"{expectedCount}\", found \"{operandsCount}\"");
            }
        }

        // methods to work with the symbol table

        private static void AddVar(VarIdASTNode var)
        {
            SymbolTable.Add(var.Name);
        }

        private static bool IsVarDefined(VarIdASTNode var)
        {
            return SymbolTable.Contains(var.Name);
        }

        private static void SetVarModifiable(VarIdASTNode var, bool isModifiable)
        {
            SymbolTable.SetModifiable(var.Name, isModifiable);
        }
    }
}
