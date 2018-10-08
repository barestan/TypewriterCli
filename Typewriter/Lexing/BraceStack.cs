using System.Collections.Generic;
using System.Linq;

namespace Typewriter.Lexing
{
    public class BraceStack
    {
        private readonly Stack<Token> _braceStack = new Stack<Token>();
        private readonly Stack<Token> _curlyBraceStack = new Stack<Token>();
        private readonly Stack<Token> _functionBraceStack = new Stack<Token>();

        public void Push(Token token, char brace)
        {
            if (brace == '{') _curlyBraceStack.Push(token);
            else if (brace == '[') _braceStack.Push(token);
            else if (brace == '(') _functionBraceStack.Push(token);
        }

        public Token Pop(char brace)
        {
            if (brace == '}' && _curlyBraceStack.Count > 0) return _curlyBraceStack.Pop();
            if (brace == ']' && _braceStack.Count > 0) return _braceStack.Pop();
            if (brace == ')' && _functionBraceStack.Count > 0) return _functionBraceStack.Pop();

            return null;
        }

        public bool IsBalanced(char brace)
        {
            if (brace == '}') return _curlyBraceStack.Any() == false;
            if (brace == ']') return _braceStack.Any() == false;
            if (brace == ')') return _functionBraceStack.Any() == false;

            return false;
        }
    }
}
