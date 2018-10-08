using System.Text;

namespace Typewriter.Lexing
{
    internal class Stream
    {
        private readonly int _offset;
        private readonly string _template;
        private int _position = -1;
        private char _current = char.MinValue;

        public Stream(string template, int offset = 0)
        {
            _offset = offset;
            _template = template ?? string.Empty;
        }

        public int Position
        {
            get { return _position + _offset; }
        }

        public char Current
        {
            get { return _current; }
        }

        public bool Advance(int offset = 1)
        {
            for (int i = 0; i < offset; i++)
            {
                _position ++;

                if (_position >= _template.Length)
                {
                    _current = char.MinValue;
                    return false;
                }

                _current = _template[_position];
            }

            return true;
        }

        public char Peek(int offset = 1)
        {
            var index = _position + offset;

            if (index > -1 && index < _template.Length)
            {
                return _template[index];
            }

            return char.MinValue;
        }

        public string PeekWord(int start = 0)
        {
            if (char.IsLetter(Peek(start)) == false) return null;

            var identifier = new StringBuilder();
            var i = start;
            while (char.IsLetterOrDigit(Peek(i)))
            {
                identifier.Append(Peek(i));
                i++;
            }

            return identifier.ToString();
        }

        public string PeekLine(int start = 0)
        {
            var line = new StringBuilder();
            var i = start;
            do
            {
                line.Append(Peek(i));
                i++;
            } while (Peek(i) != '\n' && i + _position < _template.Length);

            line.Append('\n');

            return line.ToString();
        }

        public string PeekBlock(int start, char open, char close)
        {
            var i = start;
            var depth = 1;
            var identifier = new StringBuilder();
            
            while (depth > 0)
            {
                var letter = Peek(i);

                if (letter == char.MinValue) break;
                //if (letter == close) depth--;
                if (IsMatch(i, letter, close)) depth--;
                if (depth > 0)
                {
                    identifier.Append(letter);
                    //if (letter == open) depth++;
                    if (IsMatch(i, letter, open)) depth++;

                    i++;

                    if (letter != open && (letter == '"' || letter == '\''))
                    {
                        var block = PeekBlock(i, letter, letter);
                        identifier.Append(block);
                        i += block.Length;

                        if (letter == Peek(i))
                        {
                            identifier.Append(letter);
                            i++;
                        }
                    }
                }
            }

            return identifier.ToString();
        }

        private bool IsMatch(int index, char letter, char match)
        {
            if (letter == match)
            {
                var isString = match == '"' || match == '\'';
                if (isString)
                {
                    if (Peek(index - 1) == '\\' && Peek(index - 2) != '\\') return false;
                }

                return true;
            }

            return false;
        }

        public bool SkipWhitespace()
        {
            if (_position < 0) Advance();

            while (char.IsWhiteSpace(Current))
            {
                Advance();
            }

            return _position < _template.Length;
        }
    }
}