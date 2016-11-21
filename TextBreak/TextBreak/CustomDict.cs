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
    /// <summary>
    /// my custom dic
    /// </summary>
    public class CustomDic
    {
        TextBuffer textBuffer;
        WordGroup[] wordGroups;
        char firstChar, lastChar;

        internal TextBuffer TextBuffer { get { return textBuffer; } }
        public void SetCharRange(char firstChar, char lastChar)
        {
            this.firstChar = firstChar;
            this.lastChar = lastChar;
        }
        public char FirstChar { get { return firstChar; } }
        public char LastChar { get { return lastChar; } }
        public void LoadFromTextfile(string filename)
        {
            //once only            
            if (textBuffer != null)
            {
                return;
            }
            if (firstChar == '\0' || lastChar == '\0')
            {
                throw new NotSupportedException();
            }

            //---------------
            Dictionary<char, WordGroup> wordGroups = new Dictionary<char, WordGroup>();
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            using (StreamReader reader = new StreamReader(fs))
            {
                //init with filesize
                textBuffer = new TextBuffer((int)fs.Length);
                string line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Trim();
                    char[] lineBuffer = line.ToCharArray();
                    int lineLen = lineBuffer.Length;
                    char c0;
                    if (lineLen > 0 && (c0 = lineBuffer[0]) != '#')
                    {
                        int startAt = textBuffer.CurrentPosition;
                        textBuffer.AddWord(lineBuffer);

#if DEBUG
                        if (lineLen > byte.MaxValue)
                        {
                            throw new NotSupportedException();
                        }
#endif

                        WordSpan wordspan = new WordSpan(startAt, (byte)lineLen);
                        //each wordgroup contains text span

                        WordGroup found;
                        if (!wordGroups.TryGetValue(c0, out found))
                        {
                            found = new WordGroup(new WordSpan(startAt, 1));
                            wordGroups.Add(c0, found);
                        }
                        found.AddWordSpan(wordspan);

                    }
                    //- next line
                    line = reader.ReadLine();
                }

                reader.Close();
                fs.Close();
            }
            //------------------------------------------------------------------
            textBuffer.Freeze();
            //------------------------------------------------------------------
            this.wordGroups = new WordGroup[this.lastChar - this.firstChar + 1];
            foreach (var kp in wordGroups)
            {
                int index = TransformCharToIndex(kp.Key);
                this.wordGroups[index] = kp.Value;
            }

            //do index
            DoIndex();
        }
        int TransformCharToIndex(char c)
        {
            return c - this.firstChar;
        }
        void DoIndex()
        {
            for (int i = wordGroups.Length - 1; i >= 0; --i)
            {
                WordGroup wordGroup = wordGroups[i];
                if (wordGroup != null)
                {
                    wordGroup.DoIndex(this.textBuffer, this);
                }
            }
        }
        public void GetWordList(char startWithChar, List<string> output)
        {
            if (startWithChar >= firstChar && startWithChar <= lastChar)
            {
                //in range 
                WordGroup found = this.wordGroups[TransformCharToIndex(startWithChar)];
                if (found != null)
                {//iterate and collect into 
                    found.CollectAllWords(this.textBuffer, output);
                }
            }
        }
        internal WordGroup GetWordGroupForFirstChar(char c)
        {
            if (c >= firstChar && c <= lastChar)
            {
                //in range
                return this.wordGroups[TransformCharToIndex(c)];
            }
            return null;
        }
    }


    struct WordSpan
    {
        public readonly int startAt;
        public readonly byte len;

        public WordSpan(int startAt, byte len)
        {
            this.startAt = startAt;
            this.len = len;
        }
        public char GetChar(int index, TextBuffer textBuffer)
        {
            return textBuffer.GetChar(startAt + index);
        }
        public string GetString(TextBuffer textBuffer)
        {
            return textBuffer.GetString(startAt, len);
        }
        public bool SameTextContent(WordSpan another, TextBuffer textBuffer)
        {
            if (another.len == this.len)
            {
                for (int i = another.len - 1; i >= 0; --i)
                {
                    if (this.GetChar(i, textBuffer) != another.GetChar(i, textBuffer))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
            //return this.startAt == another.startAt && this.len == another.len;
        }
    }

    class TextBuffer
    {
        List<char> _tmpCharList;
        int position;
        char[] charBuffer;
        public TextBuffer(int initCapacity)
        {
            _tmpCharList = new List<char>(initCapacity);
        }
        public void AddWord(char[] wordBuffer)
        {
            _tmpCharList.AddRange(wordBuffer);
            //append with  ' ' 
            _tmpCharList.Add(' ');
            position += wordBuffer.Length + 1;
        }
        public void Freeze()
        {
            charBuffer = _tmpCharList.ToArray();
            _tmpCharList = null;
        }
        public int CurrentPosition
        {
            get { return position; }
        }
        public char GetChar(int index)
        {
            return charBuffer[index];
        }
        public string GetString(int index, int len)
        {
            return new string(this.charBuffer, index, len);
        }
    }



    public struct BreakSpan
    {
        public int startAt;
        public int len;
    }


    public class WordGroup
    {
        List<WordSpan> wordSpanList = new List<WordSpan>();
        WordGroup[] subGroups;
        WordSpan prefixSpan;
        internal WordGroup(WordSpan prefixSpan)
        {
            this.prefixSpan = prefixSpan;
        }


#if DEBUG
        public enum debugDataState : byte
        {
            UnIndex,
            Indexed,
            TooLongPrefix,
            SmallAmountOfMembers
        }
        static int debugTotalId;
        int debugId = debugTotalId++;
        public static int DebugTotalId { get { return debugTotalId; } }
        debugDataState dbugDataState;
#endif

        internal string GetPrefix(TextBuffer buffer)
        {
            return prefixSpan.GetString(buffer);
        }
        internal bool PrefixIsWord
        {
            get;
            private set;
        }
        internal void CollectAllWords(TextBuffer textBuffer, List<string> output)
        {
            if (this.PrefixIsWord)
            {
                output.Add(GetPrefix(textBuffer));
            }
            if (subGroups != null)
            {
                foreach (WordGroup wordGroup in subGroups)
                {
                    if (wordGroup != null)
                    {
                        wordGroup.CollectAllWords(textBuffer, output);
                    }
                }
            }
            if (wordSpanList != null)
            {
                foreach (var span in wordSpanList)
                {
                    output.Add(span.GetString(textBuffer));
                }
            }
        }
        public int PrefixLen { get { return this.prefixSpan.len; } }

        internal void AddWordSpan(WordSpan span)
        {
            wordSpanList.Add(span);
#if DEBUG
            dbugDataState = debugDataState.UnIndex;
#endif
        }
        public int WordSpanListCount
        {
            get
            {

                if (wordSpanList == null) return 0;
                return wordSpanList.Count;
            }
        }
        internal void DoIndex(TextBuffer textBuffer, CustomDic owner)
        {

            //recursive
            if (this.PrefixLen > 7)
            {
                DoIndexOfSmallAmount(textBuffer);
#if DEBUG
                dbugDataState = debugDataState.TooLongPrefix;
#endif
                return;
            }
            bool hasEvalPrefix = false;
            if (subGroups == null)
            {
                subGroups = new WordGroup[owner.LastChar - owner.FirstChar + 1];
            }
            //--------------------------------
            int j = wordSpanList.Count;
            int thisPrefixLen = this.PrefixLen;
            int doSepAt = thisPrefixLen;
            for (int i = 0; i < j; ++i)
            {
                WordSpan sp = wordSpanList[i];
                //string dbugStr = sp.GetString(textBuffer);

                if (sp.len > doSepAt)
                {
                    char c = sp.GetChar(doSepAt, textBuffer);
                    int c_index = c - owner.FirstChar;
                    WordGroup found = subGroups[c_index];
                    if (found == null)
                    {
                        //not found
                        found = new WordGroup(new WordSpan(sp.startAt, (byte)(doSepAt + 1)));
                        subGroups[c_index] = found;
                    }
                    found.AddWordSpan(sp);
                }
                else
                {
                    if (!hasEvalPrefix)
                    {
                        if (sp.SameTextContent(this.prefixSpan, textBuffer))
                        {
                            hasEvalPrefix = true;
                            this.PrefixIsWord = true;
                        }
                    }
                }

            }
#if DEBUG
            this.dbugDataState = debugDataState.Indexed;
#endif
            wordSpanList.Clear();
            wordSpanList = null;
            //--------------------------------
            //do sup index
            //foreach (WordGroup subgroup in this.wordGroups.Values)
            bool hasSomeSubGroup = false;
            foreach (WordGroup subgroup in this.subGroups)
            {
                if (subgroup != null)
                {
                    hasSomeSubGroup = true;

                    //****
                    //performance factor here,****
                    //in this current version 
                    //if we not call DoIndex(),
                    //this subgroup need linear search-> so it slow                   
                    //so we call DoIndex until member count in the group <=3
                    //then it search faster, 
                    //but dictionary-building time may increase.

                    if (subgroup.WordSpanListCount > 2)
                    {
                        subgroup.DoIndex(textBuffer, owner);
                    }
                    else
                    {
#if DEBUG
                        subgroup.dbugDataState = debugDataState.SmallAmountOfMembers;
#endif
                        subgroup.DoIndexOfSmallAmount(textBuffer);
                    }
                }
            }
            //--------------------------------
#if DEBUG
            this.dbugDataState = debugDataState.Indexed;
#endif
            if (!hasSomeSubGroup)
            {
                //clear
                subGroups = null;
            }
        }
        void DoIndexOfSmallAmount(TextBuffer textBuffer)
        {

            //convention...
            //data must me sorted (ascending) before use with the wordSpanList 

            for (int i = wordSpanList.Count - 1; i >= 0; --i)
            {
                WordSpan sp = wordSpanList[i];
#if DEBUG
                //string dbugStr = sp.GetString(textBuffer);
#endif

                if (sp.SameTextContent(this.prefixSpan, textBuffer))
                {
                    this.PrefixIsWord = true;
                    break;
                }
            }
        }
        internal WordGroup GetSubGroup(WordVisitor visitor)
        {
            char c = visitor.Char;
            if (!visitor.CanHandle(c))
            {
                //can't handle
                //then no furtur sub group
                visitor.State = VisitorState.OutOfRangeChar;
                return null;
            }
            //-----------------
            //can handle 
            if (subGroups != null)
            {
                return subGroups[c - visitor.CurrentCustomDic.FirstChar];
            }
            return null;
        }

        internal int FindInUnIndexMember(WordVisitor visitor)
        {
            if (wordSpanList == null)
            {
                throw new NotSupportedException();
            }

            //at this wordgroup
            //no subground anymore
            //so we should find the word one by one
            //start at prefix
            //and select the one that 

            int readLen = visitor.CurrentIndex - visitor.LatestBreakAt;
            int nwords = wordSpanList.Count;
            //only 1 that match 

            TextBuffer currentTextBuffer = visitor.CurrentCustomDic.TextBuffer;

            //we sort unindex string ***
            //so we find from longest one( last) to begin 
            for (int i = nwords - 1; i >= 0; --i)
            {
                //loop test on each word
                WordSpan w = wordSpanList[i];
#if DEBUG
                //string dbugstr = w.GetString(currentTextBuffer);
#endif

                int savedIndex = visitor.CurrentIndex;
                char c = visitor.Char;
                int wordLen = w.len;
                int matchCharCount = 0;
                if (wordLen > readLen)
                {
                    for (int p = readLen; p < wordLen; ++p)
                    {
                        char c2 = w.GetChar(p, currentTextBuffer);
                        if (c2 == c)
                        {
                            matchCharCount++;
                            //match 
                            //read next
                            if (!visitor.IsEnd)
                            {
                                visitor.SetCurrentIndex(visitor.CurrentIndex + 1);
                                c = visitor.Char;
                            }
                            else
                            {
                                //no more data in visitor

                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                //reset
                if (readLen + matchCharCount == wordLen)
                {
                    int newBreakAt = visitor.LatestBreakAt + wordLen;
                    visitor.SetCurrentIndex(newBreakAt);
                    //-------------------------------------------- 
                    if (visitor.State == VisitorState.End)
                    {
                        return newBreakAt;
                    }
                    //check next char can be the char of new word or not
                    //this depends on each lang 
                    char canBeStartChar = visitor.Char;
                    if (visitor.CanHandle(canBeStartChar))
                    {
                        if (visitor.CanbeStartChar(canBeStartChar))
                        {
                            return newBreakAt;
                        }
                        else
                        {
                            //back to savedIndex
                            visitor.SetCurrentIndex(savedIndex);
                            return savedIndex;
                        }
                    }
                    else
                    {
                        visitor.State = VisitorState.OutOfRangeChar;
                        return newBreakAt;
                    }
                }
                visitor.SetCurrentIndex(savedIndex);
            }
            return 0;
            //int candidateCount;
            //switch (candidateCount = candidateWords.Count)
            //{
            //    case 0: return 0;
            //    case 1:
            //        {
            //            CandidateWord candidate = candidateWords[0];
            //            int savedIndex = visitor.CurrentIndex;
            //            int newBreakAt = visitor.LatestBreakAt + candidate.max_match;
            //            visitor.SetCurrentIndex(newBreakAt);
            //            if (visitor.State == VisitorState.End)
            //            {
            //                return newBreakAt;
            //            }
            //            //check next char can be the char of new word or not
            //            //this depends on each lang 
            //            char canBeStartChar = visitor.Char;
            //            if (visitor.CanHandle(canBeStartChar))
            //            {
            //                if (visitor.CanbeStartChar(canBeStartChar))
            //                {
            //                    return newBreakAt;
            //                }
            //                else
            //                {
            //                    //back to savedIndex
            //                    visitor.SetCurrentIndex(savedIndex);
            //                    return savedIndex;
            //                }
            //            }
            //            else
            //            {
            //                visitor.State = VisitorState.OutOfRangeChar;
            //                return visitor.CurrentIndex;
            //            }
            //        }
            //    default:
            //        {
            //            throw new NotSupportedException();
            //            //for (int cn = 0; cn < candidateCount; ++cn)
            //            //{
            //            //    CandidateWord candidate = candidateWords[cn];
            //            //    int savedIndex = visitor.CurrentIndex;
            //            //    int newBreakAt = visitor.LatestBreakAt + candidate.max_match;
            //            //    visitor.SetCurrentIndex(newBreakAt);
            //            //    if (visitor.State == VisitorState.End)
            //            //    {
            //            //        return newBreakAt;
            //            //    }
            //            //    //check next char can be the char of new word or not
            //            //    //this depends on each lang 
            //            //    char canBeStartChar = visitor.Char;
            //            //    if (visitor.CanHandle(canBeStartChar))
            //            //    {
            //            //        if (visitor.CanbeStartChar(canBeStartChar))
            //            //        {
            //            //            return newBreakAt;
            //            //        }
            //            //        else
            //            //        {
            //            //            //back to savedIndex
            //            //            visitor.SetCurrentIndex(savedIndex);
            //            //            return savedIndex;
            //            //        }
            //            //    }
            //            //    else
            //            //    {
            //            //        visitor.State = VisitorState.OutOfRangeChar;
            //            //        return visitor.CurrentIndex;
            //            //    }
            //            //} 
            //        }
            //}
        }

#if DEBUG
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.Append(this.prefixSpan.startAt + " " + this.prefixSpan.len);
            stbuilder.Append(" " + this.dbugDataState);
            //---------  

            if (wordSpanList != null)
            {
                stbuilder.Append(",u_index=" + wordSpanList.Count + " ");
            }
            return stbuilder.ToString();
        }
#endif

    }


}