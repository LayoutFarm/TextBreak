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
    public class CustomBreaker
    {
        DictionaryBreakingEngine breakingEngine;
        WordVisitor visitor;
        int textLength;
        public CustomBreaker()
        {
            visitor = new WordVisitor(this);
        }
        public void AddBreakingEngine(DictionaryBreakingEngine engine)
        {
            //TODO: make this accept more than 1 engine
            breakingEngine = engine;
        }
        public void BreakWords(char[] charBuff, int startAt)
        {
            //conver to char buffer 
            int j = charBuff.Length;
            textLength = j;
            visitor.LoadText(charBuff, 0);
            breakingEngine.BreakWord(visitor, charBuff, startAt, charBuff.Length - startAt);


        }
        public void BreakWords(string inputstr)
        {
            BreakWords(inputstr.ToCharArray(), 0);

        }

        public bool CanBeStartChar(char c)
        {
            return breakingEngine.CanbeStartChar(c);
        }
        public IEnumerable<BreakSpan> GetBreakSpanIter()
        {
            List<int> breakAtList = visitor.GetBreakList();
            int c_index = 0;
            int i = 0;
            foreach (int breakAt in breakAtList)
            {
                BreakSpan sp = new BreakSpan();
                sp.startAt = c_index;
                sp.len = breakAtList[i] - c_index;
                c_index += sp.len;
                i++;
                yield return sp;
            }
            //-------------------
            if (c_index < textLength)
            {
                BreakSpan sp = new BreakSpan();
                sp.startAt = c_index;
                sp.len = textLength - c_index;
                yield return sp;
            }
        }
    }
  
    
}