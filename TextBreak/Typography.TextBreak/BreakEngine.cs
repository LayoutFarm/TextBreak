//MIT, 2016-2017, WinterDev
//some code from ICU project with BSD license

namespace Typography.TextBreak
{

    delegate void OnBreak(BreakBounds breakBounds);

    class BreakBounds
    {
        public int startIndex;
        public int length;
        public bool stopNext;
        public WorkKind kind;
    }

    enum WorkKind : byte
    {
        Whitespace,
        NewLine,
        Text,
        Number,
        Punc
    }

    public struct SplitBound
    {
        public readonly int startIndex;
        public readonly int length;
        public SplitBound(int startIndex, int length)
        {
            this.startIndex = startIndex;
            this.length = length;
        }
#if DEBUG
        public override string ToString()
        {
            return startIndex + ":" + length;
        }
#endif
    }

}