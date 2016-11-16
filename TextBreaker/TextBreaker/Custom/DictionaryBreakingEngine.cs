//MIT, 2016, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LayoutFarm.TextBreaker.Custom
{
    public abstract class DictionaryBreakingEngine
    {
        public abstract bool CanbeStartChar(char c);
        public abstract void BreakWord(WordVisitor visitor, char[] charBuff, int startAt, int len);
    }
}