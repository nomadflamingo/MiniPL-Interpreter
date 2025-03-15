using interpreter.parser.ASTNodes;
using interpreter.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter
{
    internal static class Interpreter
    {
        public static void Interpret(string filePath, bool debugScanner = false)
        {
            if (debugScanner)  // if debugging the scanner, scan the file twice
            {
                Scanner scDb = new Scanner(new StreamReader(filePath));
                Token? t = scDb.GetToken();
                while (t != null)
                {
                    Console.WriteLine(t);
                    t = scDb.GetToken();
                }
            }

            Scanner sc = new Scanner(new StreamReader(filePath));
            Parser parser = new Parser(sc);

            RootASTNode root = parser.Parse();
            
            TypeCheckVisitor typeCheckVisitor = new();
            
            typeCheckVisitor.VisitRoot(root);
            
            InterpreterVisitor interpreterVisitor = new();
            
            interpreterVisitor.VisitRoot(root);
        }
    }
}
