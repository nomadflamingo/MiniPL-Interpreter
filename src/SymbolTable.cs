using interpreter.parser.ASTNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter
{
    internal static class SymbolTable
    {
        private static readonly Dictionary<string, STEntry> d = new();

        /// <summary>
        /// Returns whether or not the symbol table contains an entry for the variable
        /// </summary>
        /// <param name="varName">name of the variable</param>
        /// <returns></returns>
        public static bool Contains(string varName)
        {
            return d.ContainsKey(varName);
        }

        /// <summary>
        /// Adds an entry to a symbol table.
        /// Throws an error if a symbol table already contains the variable entry
        /// </summary>
        /// <param name="varName"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void Add(string varName)
        {
            if (d.ContainsKey(varName))
            {
                throw new ArgumentException($"Tried to redefine variable \"{varName}\"");
            }

            d.Add(varName, new STEntry());
        }

        /// <summary>
        /// Returns the type of the variable. 
        /// Throws an error if variable was not found in the symbol table.
        /// Returns null if its type was not set
        /// </summary>
        /// <param name="varName">variable name</param>
        /// <returns></returns>
        public static MiniPLType? GetType(string varName)
        {
            if (!d.ContainsKey(varName))
            {
                HandleTriedToAccessUndefined(varName);
            }

            return d[varName].Type;
        }

        /// <summary>
        /// Sets the type of the variable. 
        /// Throws an error if variable was not found in the symbol table or the variable is non-modifiable
        /// </summary>
        /// <returns></returns>
        public static void SetType(string varName, MiniPLType? type)
        {
            if (!d.ContainsKey(varName))
            {
                throw new ArgumentException($"Tried to assign type \"{type}\" to an undefined variable \"{varName}\"");
            }

            if (!IsModifiable(varName))
            {
                throw new InvalidOperationException($"Cannot modify variable \"{varName}\" because it was marked as non-modifiable");
            }
            d[varName].Type = type;
        }

        /// <summary>
        /// Returns the value of the variable. 
        /// Throws an error if variable was not found in the symbol table
        /// </summary>
        /// <returns></returns>
        public static object? GetValue(string varName)
        {
            if (!d.ContainsKey(varName))
            {
                HandleTriedToAccessUndefined(varName);
            }

            return d[varName].Value;
        }

        /// <summary>
        /// Sets the value of the variable. 
        /// Throws an error if variable was not found in the symbol table or the variable is non-modifiable
        /// </summary>
        /// <returns></returns>
        public static void SetValue(string varName, object? value)
        {
            if (!d.ContainsKey(varName))
            {
                throw new ArgumentException($"Tried to assign value \"{value}\" to an undefined variable \"{varName}\"");
            }

            if (!IsModifiable(varName))
            {
                throw new InvalidOperationException($"Cannot modify variable \"{varName}\" because it was marked as non-modifiable");
            }

            d[varName].Value = value;
            
        }

        /// <summary>
        /// Returns the IsModifiable flag of the variable, which defines whether the variable information can be modified right now
        /// Throws an error if variable was not found in the symbol table
        /// </summary>
        /// <returns></returns>
        public static bool IsModifiable(string varName)
        {
            if (!d.ContainsKey(varName))
            {
                HandleTriedToAccessUndefined(varName); 
            }

            return d[varName].IsModifiable;
        }

        /// <summary>
        /// Sets the IsModifiable flag of the variable. 
        /// Throws an error if variable was not found in the symbol table
        /// </summary>
        /// <returns></returns>
        public static void SetModifiable(string varName, bool isModifiable)
        {
            if (!d.ContainsKey(varName))
            {
                throw new ArgumentException($"Tried to assign isModifiable = \"{isModifiable}\" to an undefined variable \"{varName}\"");
            }

            d[varName].IsModifiable = isModifiable;
        }

        /// <summary>
        /// Resets the symbol table and removes all variables from it. Normally only used for debugging when interpreting multiple files in one folder.
        /// </summary>
        public static void Reset()
        {
            foreach (string varName in d.Keys)
            {
                d.Remove(varName);
            }
        }

        private static void HandleTriedToAccessUndefined(string varName)
        {
            throw new ArgumentException($"Tried to access undefined variable \"{varName}\"");
        }

        private class STEntry
        {
            public MiniPLType? Type { get; set; }
            public object? Value { get; set; }
            public bool IsModifiable { get; set; }

            public STEntry(MiniPLType? type = null, object? value = null, bool isModifiable = true)
            {
                Type = type;
                Value = value;
                IsModifiable = isModifiable;
            }
        }
    }
}
