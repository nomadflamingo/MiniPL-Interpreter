# MiniPL-Interpreter

A simple C# interpreter for a toy programming language called MiniPL (Mini Pascal Language) implemented as part of the course project for the course "Compilers" at the University of Helsinki

## Table of contents

1. [How to Run](#how-to-run)
2. [Project Structure](#project-structure)
   - [AST Node Hierarchy](#ast-node-hierarchy)
   - [Visitors and Symbol Table](#visitors-and-symbol-table)
   - [Tokens Produced by Scanner](#tokens-produced-by-scanner)
   - [Interpreter and Program](#interpreter-and-program)
   - [Scanner and Parser](#scanner-and-parser)
3. [Language Specification](#language-specification)
   - [Token Patterns](#token-patterns)
   - [Modified Context-Free Grammar](#modified-context-free-grammar)
4. [Testing](#testing)
5. [Error Handling](#error-handling)
6. [Known Shortcomings](#known-shortcomings)

# How to run

To run the project, clone the repository, build the solution with Visual Studio or `msbuild`. You will then have a `bin/Release/net6.0/interpreter.exe` file. You can use this `.exe` file to execute `.mpl` files, by providing a path to the file to the interpreter:
```
interpreter.exe “full_path_to_your_file.mpl”
```
The interpreter also supports parsing a directory of scripts:
```
interpreter.exe “full_path_to_your_directory”
```
Alternatively, you can open the `.csproj` or `.sln` in visual studio (didn’t test on other ides) and click run

The project doesn’t force the use of `.mpl` extension for MiniPL files and would interpret any other extension without any warnings. 

# Project structure

## AST Node Hierarchy

![image](https://github.com/user-attachments/assets/52e0117d-2025-4319-9ea6-7a64805ecb6c)

## Visitors and Symbol Table

![image](https://github.com/user-attachments/assets/6f79d125-4eb4-43e3-8a08-480a49890554)

## Tokens Produced by Scanner

![image](https://github.com/user-attachments/assets/84eb2367-6718-4fed-9662-36e078428c53)

## Interpreter and Program
![image](https://github.com/user-attachments/assets/29e99759-98a1-4a62-81a8-7215e484004e)

## Scanner and Parser

![image](https://github.com/user-attachments/assets/886693a0-60cc-4cc6-a096-5ecff7c44193)


# Language specification
The defaukt language specification can be found in [`doc/MiniPL.pdf`](https://github.com/nomadflamingo/MiniPL-Interpreter/blob/master/doc/MiniPL.pdf). Below are my modifications to it for the purposes of this project.
## Token patterns

>[!NOTE]
> I didn't divide keywords and special symbols into separate token types because i didn't find any practical reasons to do it for my project

```
keyword := +|-|*|/|;|:|&|=|(|)|!|<|var|for|end|in|do|read|print|int|string|bool|if|else

bool := true|false

int := [0-9]*

string := ".*"

id := [a-z|A-Z|0-9|_]([a-z|A-Z|0-9|_])*
```

## Modified Context-Free Grammar:

Everything is the same as in the miniPL language definition file (doc folder), except for `<opnd>` and `<expr>`

```
<opnd> ::= <int>
         | <string>
         | <bool>
         | <var_ident>
         | (<expr>)

<expr> ::= <opnd> <op_or_empty>
         | <unary_opnd> <opnd>

<op_or_empty> ::= <op> <opnd>
               | ε
```

# Testing
The `tests` folder contains the `.mpl` files that were used to test the project:
![image](https://github.com/user-attachments/assets/7d4dce4d-d215-48b0-a6f1-61a29522d047)

For testing I used those files. In the project it is possible to pass a directory as an input and the interpreter will interpret all the files in this directory one by one. Sometimes I only passed one of those files as an argument

I chose to include only methods and fields that are part of an API of the class, because this is more important for an understanding the structure of my program. So I don’t show private methods and fields.

# Error handling
For error handling, I store lines and columns of tokens in the token class and then use this information to print error messages. Usually for parser errors I print the token that was expected, as well as the token that was encountered as well as on what line and on what column
It would also be better to store lines and columns of AST nodes but I didn’t have time to do it.
It would also be nice to indicate when an error was encountered (at runtime, at compile time, by scanner, by parser, by which visitor). This is pretty simple to do but again I’m bad at time management

# Known Shortcomings
As I mentioned, currently the code doesn’t say when an error occurred (at compile-time or at run-time). It also doesn’t tell the position of the error when the error happened during type-checking or during interpretation because AST nodes don’t have columns and lines.
Some error messages might not be very clear
It would be super nice to include some basic error recovery methods, such as panic mode recovery (e.g. find the next semicolon and continue as normal)
It would make a lot more sense to define all the keywords, types, symbols, operations, default values for every class, potentially error messages patterns in one class, because this is an information that should easily be able to change or extend
