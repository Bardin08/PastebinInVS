namespace PastebinInVS.Structs
{
    /// <summary>
    /// Store selected text and info about it
    /// </summary>
    public struct TextViewSelection
    {
        /// <summary>
        /// Selected text start position
        /// </summary>
        public TextViewPosition StartPosition { get; set; }
        /// <summary>
        /// Selected text end position
        /// </summary>
        public TextViewPosition EndPosition { get; set; }
        /// <summary>
        /// Selected text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Create new <seealso cref="TextViewSelection"/>
        /// </summary>
        /// <param name="a">Selected text start position</param>
        /// <param name="b">Selected text end position</param>
        /// <param name="text">Selected text</param>
        public TextViewSelection(TextViewPosition a, TextViewPosition b, string text)
        {
            StartPosition = TextViewPosition.Min(a, b);
            EndPosition = TextViewPosition.Max(a, b);
            Text = text;
        }
    }
}