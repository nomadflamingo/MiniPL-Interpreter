using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpreter
{
    internal enum TokenType
    {
        Int,
        String,
        Bool,
        Id,
        Keyword  // this type refers to both keywords and special symbols
    }
}
