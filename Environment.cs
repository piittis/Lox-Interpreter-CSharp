﻿using System;
using System.Collections.Generic;

namespace Lox
{
    class Environment
    {
        private readonly Environment enclosing;
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        // All variables have this value before they are initialized or assigned.
        public static readonly object unAssigned = new object();
        public Environment Enclosing => enclosing;

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string name, Object value)
        {
            values.Add(name, value);
        }

        public object Get(Token name)
        {
            if (values.TryGetValue(name.lexeme, out object val))
            {
                if (ReferenceEquals(val, unAssigned))
                {
                    throw new RuntimeError(name, $"Use of unassigned variable '{name.lexeme}'.");
                }
                return val;
            }
            if (enclosing != null)
            {
                return enclosing.Get(name);
            }
            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
            }
            else if (enclosing != null)
            {
                enclosing.Assign(name, value);
            }
            else
            {
                throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
            }
        }

        public object GetAt(int distance, Token name)
        {
            var env = Ancestor(distance);
            var value = Ancestor(distance).values[name.lexeme];

            if (ReferenceEquals(value, unAssigned))
            {
                throw new RuntimeError(name, $"Use of unassigned variable '{name.lexeme}'.");
            }
            return value;
        }

        public object GetAt(int distance, string name)
        {
            var env = Ancestor(distance);
            var value = Ancestor(distance).values[name];
            return value;
        }

        public void AssignAt(int distance, Token name, object value)
        {
            var env = Ancestor(distance);
            env.values[name.lexeme] = value;
        }

        private Environment Ancestor(int distance)
        {
            // Walk up the distance to the correct enclosing environment.
            var environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.enclosing;
            }
            return environment;
        }
    }
}
