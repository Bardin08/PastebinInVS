namespace PastebinInVS.Structs
{
    /// <summary>
    /// Helps to get selected text
    /// </summary>
    public struct TextViewPosition
    {
        private readonly int _column;
        private readonly int _line;

        /// <summary>
        /// Create new <seealso cref="TextViewPosition"/>
        /// </summary>
        /// <param name="line">Text line</param>
        /// <param name="column">Text column</param>
        public TextViewPosition(int line, int column)
        {
            _line = line;
            _column = column;
        }

        /// <summary>
        /// Text line
        /// </summary>
        public int Line => _line;
        /// <summary>
        /// Text column
        /// </summary>
        public int Column => _column;


        public static bool operator <(TextViewPosition a, TextViewPosition b)
        {
            if (a.Line < b.Line)
            {
                return true;
            }
            else if (a.Line == b.Line)
            {
                return a.Column < b.Column;
            }
            else
            {
                return false;
            }
        }

        public static bool operator >(TextViewPosition a, TextViewPosition b)
        {
            if (a.Line > b.Line)
            {
                return true;
            }
            else if (a.Line == b.Line)
            {
                return a.Column > b.Column;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Compare two <seealso cref="TextViewPosition"/>
        /// </summary>
        public static TextViewPosition Min(TextViewPosition a, TextViewPosition b)
        {
            return a > b ? b : a;
        }

        /// <summary>
        /// Compare two <seealso cref="TextViewPosition"/>
        /// </summary>
        public static TextViewPosition Max(TextViewPosition a, TextViewPosition b)
        {
            return a > b ? a : b;
        }
    }
}