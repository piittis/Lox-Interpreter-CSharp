﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    class Lox
    {
        private static readonly Interpreter interpreter = new Interpreter();
        private static bool hadError = false;
        private static bool hadRuntimeError = false;
        private static List<String> Errors = new List<String>();

        static void Main(string [] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }
           

        private static void RunFile(string path)
        {
            string text = System.IO.File.ReadAllText(path);
            Run(text);

            Console.Write("Finished, press any key...");
            Console.ReadKey();

            if (hadError) System.Environment.Exit(65);
            if (hadRuntimeError) System.Environment.Exit(70);
        }

        /// <summary>
        /// Runs a REPL
        /// </summary>
        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line != null)
                {
                    List<Token> tokens = new Scanner(line).ScanTokens();
                    List<Stmt> statements = new Parser(tokens).Parse();
                    if (!hadError)
                    {
                        interpreter.Interpret(statements);
                        continue;
                    }

                    // Normal statement parsing failed, try to parse as an expression.
                    hadError = false;
                    Expr expr = new Parser(tokens).ParseExpression();
                    if (!hadError)
                    {
                        Console.WriteLine(interpreter.EvaluateExpr(expr));
                        continue;
                    }
                    else
                    {
                        // Both parse attempts failed.
                        ReportErrors();
                        ClearErrors();
                        hadError = false;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private static void Run(string source)
        {
            if (source == null)
            {
                return;
            }

            string sourceWithLineNumbers = source.Split("\n").Select((line, index) => $"{index+1}\t{line}").Join("\n");

            Console.WriteLine($"Running code:\n----------\n{sourceWithLineNumbers}\n----------");

            var scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            var parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();

            if (hadErrors()) return;

            var resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            if (hadErrors()) return;

            interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            AddError(line, "", message);
        }

        public static void Error(Token token, String message)
        {
            if (token.type == TokenType.EOF)
            {
                AddError(token.line, " at end", message);
            }
            else
            {
                AddError(token.line, $" at '{token.lexeme}'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message}\n[line {error.token.line}]");
            hadRuntimeError = true;
        }

        private static void AddError(int line, string where, string message)
        {
            Errors.Add($"[line {line}] Error{where}: {message}");
            hadError = true;
        }

        private static bool hadErrors()
        {
            if (hadError)
            {
                ReportErrors();
                return true;
            }
            return false;
        }

        private static void ReportErrors()
        {
            foreach(var errorString in Errors)
            {
                Console.Error.WriteLine(errorString);
            }
        }

        private static void ClearErrors()
        {
            Errors.Clear();
        }
    }
}
