﻿//MIT, 2016, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LayoutFarm.TextBreak
{
    public enum VisitorState
    {
        Init,
        Parsing,
        OutOfRangeChar,
        End,

    }
    public class WordVisitor
    {
        CustomBreaker ownerBreak;
        List<int> breakAtList = new List<int>();
        char[] buffer;
        int bufferLen;
        int startIndex;
        int currentIndex;
        char currentChar;
        int latestBreakAt;

        List<CandidateWord> tempCandidateWords = new List<CandidateWord>();
        Stack<int> tempCandidateBreaks = new Stack<int>();

        public WordVisitor(CustomBreaker ownerBreak)
        {
            this.ownerBreak = ownerBreak;
        }
        public void LoadText(char[] buffer, int index)
        {
            this.buffer = buffer;
            this.bufferLen = buffer.Length;
            this.startIndex = currentIndex = index;
            this.currentChar = buffer[currentIndex];
            breakAtList.Clear();
            latestBreakAt = 0;
        }
        public VisitorState State
        {
            get;
            set;
        }
        public int CurrentIndex
        {
            get { return this.currentIndex; }
        }
        public char Char
        {
            get { return currentChar; }
        }
        public bool ReadNext()
        {
            if (currentIndex < bufferLen + 1)
            {
                currentIndex++;
                currentChar = buffer[currentIndex];
                return true;
            }
            return false;
        }

        public bool IsEnd
        {
            get { return currentIndex >= bufferLen - 1; }
        }


        public void AddWordBreakAt(int index)
        {

#if DEBUG
            if (index == latestBreakAt)
            {
                throw new NotSupportedException();
            }
#endif
            this.latestBreakAt = index;
            breakAtList.Add(index);
        }
        public int LatestBreakAt
        {
            get { return this.latestBreakAt; }
        }
        public void SetCurrentIndex(int index)
        {
            this.currentIndex = index;
            if (index < buffer.Length)
            {
                this.currentChar = buffer[index];
            }
            else
            {
                this.State = VisitorState.End;
            }
        }
        public bool CanbeStartChar(char c)
        {
            return ownerBreak.CanBeStartChar(c);
        }
        public bool CanHandle(char c)
        {
            CustomDic dic = CurrentCustomDic;
            return c >= dic.FirstChar && c <= dic.LastChar;
        }
        public List<int> GetBreakList()
        {
            return breakAtList;
        }
        internal List<CandidateWord> GetTempCandidateWords()
        {
            return this.tempCandidateWords;
        }
        internal Stack<int> GetTempCandidateBreaks()
        {
            return this.tempCandidateBreaks;
        }
        internal CustomDic CurrentCustomDic
        {
            get;
            set;
        }
    }

}