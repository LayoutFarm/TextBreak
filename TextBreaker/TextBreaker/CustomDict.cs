//MIT, 2016, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LayoutFarm.TextBreaker.CustomBreaker
{
    /// <summary>
    /// my custom dic
    /// </summary>
    public class CustomDic
    {

        Dictionary<char, WordGroup> wordGroups = new Dictionary<char, WordGroup>();
        public void LoadFromTextfile(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            using (StreamReader reader = new StreamReader(fs))
            {
                string line = reader.ReadLine();
                while (line != null)
                {

                    line = line.Trim();
                    int lineLen = line.Length;
                    char c0;
                    if (lineLen > 0 && (c0 = line[0]) != '#')
                    {
                        //get first word
                        WordGroup found;
                        if (!wordGroups.TryGetValue(c0, out found))
                        {
                            found = new WordGroup(c0.ToString(), 1);
                            wordGroups.Add(c0, found);
                        }
                        found.AddWord(line);
                    }
                    //- next line
                    line = reader.ReadLine();
                }

                reader.Close();
                fs.Close();
            }
            //do index
            DoIndex();
        }
        void DoIndex()
        {
            foreach (WordGroup wordGroup in wordGroups.Values)
            {
                wordGroup.DoIndex();
            }
        }


        public WordGroup GetStateForFirstChar(char c)
        {
            WordGroup wordGroup;
            if (wordGroups.TryGetValue(c, out wordGroup))
            {
                return wordGroup;
            }
            return null;
        }

    }

    public enum BreakAction
    {
        SkipNextChar,
        FeedNextChar,
        Stop,
    }


    public class CustomBreaker
    {
        CustomDic _customDic;
        public void AddDic(CustomDic customDic)
        {
            _customDic = customDic;
        }
        public void BreakWords(string inputstr)
        {
            //conver to char buffer
            char[] charBuff = inputstr.ToCharArray();
            int j = charBuff.Length;
            WordVisitor visitor = new WordVisitor(charBuff, 0);

            for (int i = 0; i < j; )
            {
                //find proper start words;
                char c = charBuff[i];
                WordGroup state = _customDic.GetStateForFirstChar(c);
                if (state == null)
                {
                    //continue next char
                    ++i;
                }
                else
                {
                    //use this to find a proper word
                    int prevIndex = i;
                    visitor.SetCurrentIndex(i + 1);
                    state.FindBreak(visitor);
                    i = visitor.LatestBreakAt;
                    if(prevIndex ==i)
                    {
                    }
                }
            }
        }
    }


    public enum DataState
    {
        UnIndex,
        Indexed,
        TooLongPrefix,
        SmallAmountOfMembers
    }

    public class WordVisitor
    {
        List<string> possibleWords = new List<string>();
        List<int> breakAtList = new List<int>();
        char[] buffer;
        int bufferLen;
        int startIndex;
        int currentIndex;
        char currentChar;
        int latestBreakAt;

        public WordVisitor(char[] buffer, int index)
        {
            this.buffer = buffer;
            this.bufferLen = buffer.Length;
            this.startIndex = currentIndex = index;
            this.currentChar = buffer[currentIndex];
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
            get { return currentIndex >= bufferLen; }
        }

        public void AddWordBreakAt(int index)
        {
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
            this.currentChar = buffer[index];
        }
    }

    public class WordGroup
    {

        List<string> unIndexMemberWords = new List<string>();
        List<string> indexMemberWords;
        Dictionary<char, WordGroup> wordGroups;

        public WordGroup(string prefix, int prefixLen)
        {
            this.Prefix = prefix;
            this.PrefixLen = prefixLen;
        }
        public DataState DataState { get; private set; }
        public string Prefix
        {
            get;
            private set;
        }
        public int PrefixLen { get; private set; }
        public void AddWord(string word)
        {
            unIndexMemberWords.Add(word);
            this.DataState = DataState.UnIndex;
        }
        public int UnIndexMemberCount
        {
            get
            {
                if (unIndexMemberWords == null) return 0;
                return unIndexMemberWords.Count;
            }
        }
        public void DoIndex()
        {
            //recursive
            if (this.PrefixLen > 7)
            {
                this.DataState = DataState.TooLongPrefix;
                return;
            }
            if (indexMemberWords == null)
            {
                indexMemberWords = new List<string>();
            }
            if (wordGroups == null)
            {
                wordGroups = new Dictionary<char, WordGroup>();
            }
            //--------------------------------
            int j = unIndexMemberWords.Count;
            int thisPrefixLen = this.PrefixLen;
            int doSepAt = thisPrefixLen;
            for (int i = 0; i < j; ++i)
            {
                string w = unIndexMemberWords[i];
                if (w.Length > doSepAt)
                {
                    char c = w[doSepAt];
                    //get first word
                    WordGroup found;
                    if (!wordGroups.TryGetValue(c, out found))
                    {
                        found = new WordGroup(this.Prefix + c, doSepAt + 1);
                        wordGroups.Add(c, found);
                    }
                    found.AddWord(w);
                }
                else
                {
                    indexMemberWords.Add(w);
                }
            }
            this.DataState = DataState.Indexed;
            //clear unindex data
            unIndexMemberWords.Clear();
            unIndexMemberWords = null;
            //--------------------------------
            //do sup index
            foreach (WordGroup subgroup in this.wordGroups.Values)
            {
                if (subgroup.UnIndexMemberCount > 10)
                {
                    subgroup.DoIndex();
                }
                else
                {
                    subgroup.DataState = DataState.SmallAmountOfMembers;
                }
            }

            //--------------------------------
            this.DataState = DataState.Indexed;
            if (this.indexMemberWords.Count == 0)
            {
                //clear 
                this.indexMemberWords = null;
            }
            if (wordGroups.Count == 0)
            {
                //clear
                wordGroups = null;
            }
        }

        struct CandidateWord
        {
            public int w_index;
            public int w_len;
            public int max_match;
            public bool IsFullMatch()
            {
                return w_len == max_match;
            }
        }

        public void FindBreak(WordVisitor visitor)
        {
            //recursive
            char c = visitor.Char;
            bool foundInSubgroup = false;
            if (wordGroups != null)
            {
                WordGroup foundSubGroup;
                if (wordGroups.TryGetValue(c, out foundSubGroup))
                {
                    //found next group
                    foundInSubgroup = true;
                    if (!visitor.IsEnd)
                    {
                        visitor.SetCurrentIndex(visitor.CurrentIndex + 1);
                        foundSubGroup.FindBreak(visitor);
                    }
                }
            }
            //-------
            if (!foundInSubgroup)
            {
                if (indexMemberWords != null)
                {
                    //find word
                    visitor.AddWordBreakAt(visitor.CurrentIndex);
                }

                if (unIndexMemberWords != null)
                {
                    //at this wordgroup
                    //no subground anymore
                    //so we should find the word one by one
                    //start at prefix
                    //and select the one that 
                    int pos = visitor.CurrentIndex;
                    int latestBreak = visitor.LatestBreakAt;
                    int len = pos - latestBreak;
                    int n = unIndexMemberWords.Count;

                    List<CandidateWord> candidateWords = new List<CandidateWord>();

                    for (int i = 0; i < n; ++i)
                    {
                        //begin new word
                        string w = unIndexMemberWords[i];
                        int savedIndex = visitor.CurrentIndex;
                        int wordLen = w.Length;
                        int matchCharCount = 0;
                        if (wordLen > len)
                        {
                            char[] wbuff = w.ToCharArray();
                            //check if this word match or not
                            for (int p = len; p < wordLen; ++p)
                            {
                                char c2 = wbuff[p];
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
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        //reset
                        if (matchCharCount > 0)
                        {
                            CandidateWord candidate = new CandidateWord();
                            candidate.w_index = i;
                            candidate.w_len = wordLen;
                            candidate.max_match = len + matchCharCount;
                            candidateWords.Add(candidate);
                        }
                        visitor.SetCurrentIndex(savedIndex);
                    }

                    if (candidateWords.Count == 1)
                    {
                        CandidateWord candidate = candidateWords[0];
                        if (candidate.IsFullMatch())
                        {
                            visitor.AddWordBreakAt(
                                visitor.LatestBreakAt + candidate.max_match);
                        }
                        else
                        {

                        }
                    }
                    else if (candidateWords.Count > 0)
                    {

                    }
                }
            }
            else
            {
                if (indexMemberWords != null)
                {
                    //find word
                    visitor.AddWordBreakAt(visitor.CurrentIndex);


                }

                if (unIndexMemberWords != null)
                {
                    //at this wordgroup
                    //no subground anymore
                    //so we should find the word one by one
                    //start at prefix
                    //and select the one that 

                }
            }

        }
#if DEBUG
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.Append(Prefix);
            stbuilder.Append(" " + this.DataState);
            //---------  
            if (unIndexMemberWords != null)
            {
                stbuilder.Append(",u_index=" + unIndexMemberWords.Count + " ");
            }
            if (indexMemberWords != null)
            {
                stbuilder.Append(",index=" + indexMemberWords.Count + " ");
            }
            return stbuilder.ToString();
        }
#endif

    }


}