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


    public class ThaiDictionaryBreakingEngine : DictionaryBreakingEngine
    {
        CustomDic _customDic;
        public void SetDictionaryData(CustomDic customDic)
        {
            this._customDic = customDic;
        }
        public override bool CanbeStartChar(char c)
        {
            for (int i = cantBeStartChars.Length - 1; i >= 0; --i)
            {
                if (c == cantBeStartChars[i])
                {
                    return false;
                }
            }
            return true;
        }
        public override void BreakWord(WordVisitor visitor, char[] charBuff, int startAt, int len)
        {
            for (int i = startAt; i < len; )
            {
                //find proper start words;
                char c = charBuff[i];

                WordGroup state = _customDic.GetStateForFirstChar(c);
                if (state == null)
                {
                    //continue next char
                    ++i;
                    visitor.AddWordBreakAt(i);
                }
                else
                {
                    //use this to find a proper word
                    int prevIndex = i;
                    visitor.SetCurrentIndex(i + 1);
                    state.FindBreak(visitor);
                    i = visitor.LatestBreakAt;
                    if (prevIndex == i)
                    {
                        if (visitor.CurrentIndex >= len - 1)
                        {
                            //the last one 
                            break;
                        }
                        else
                        {
                            //skip this 
                            i++;
                            visitor.AddWordBreakAt(i);
                            visitor.SetCurrentIndex(visitor.LatestBreakAt);
                        }
                    }
                }
            }
        }
        //eg thai sara
        static char[] cantBeStartChars = "ะาิีึืุู่้๊๋็ฺ์ํั".ToCharArray();
    }


}