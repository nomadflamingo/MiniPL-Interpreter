using interpreter;
using interpreter.parser;
using interpreter.parser.ASTNodes;
using System.Reflection.Metadata.Ecma335;
using System.Text;


for (int i = 0; i < args.Length; ++i)
{
    FileAttributes attr = File.GetAttributes(args[i]);
    if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
    {
        PrettyInterpret(args[i]);
    }
    else
    {
        // it's a directory

        // scan for all mpl files
        IEnumerable<string> mplFiles = Directory.EnumerateFiles(args[i]);

        foreach (string currentFile in mplFiles)
        {
            PrettyInterpret(currentFile);
        }
    }
}

void TryInterpret(string currentFile)
{
    try
    {
        SymbolTable.Reset();
        Interpreter.Interpret(currentFile);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}

void PrettyInterpret(string currentFile)
{
    Console.WriteLine($"Interpreting file: \"{Path.GetFileName(currentFile)}\"");
    TryInterpret(currentFile);
    Console.WriteLine($"\nInterpreter finished");
    Console.WriteLine();
}

