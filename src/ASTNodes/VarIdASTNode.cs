using interpreter.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter.parser.ASTNodes
{
    // TODO: make all fields have a logical access modifier
    internal class VarIdASTNode : ExpressionASTNode
    {

        public override object? Value { 
            get => SymbolTable.GetValue(Name); 
            set => SymbolTable.SetValue(Name, value);
        }

        public override MiniPLType? Type
        {
            get => SymbolTable.GetType(Name);
            set => SymbolTable.SetType(Name, value);
        }

        public string Name { get; set; }

        public VarIdASTNode(string name)
        {
            Name = name;
        }

        public override void Accept(IVisitor visitor) => visitor.VisitVarId(this);
    }
}
