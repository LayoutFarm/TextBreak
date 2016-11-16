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


   
    
    public struct BreakSpan
    {
        public int startAt;
        public int len;
    }

     
    public enum DataState
    {
        UnIndex,
        Indexed,
        TooLongPrefix,
        SmallAmountOfMembers
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
        bool PrefixIsWord
        {
            get;
            set;
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
                    if (w == this.Prefix)
                    {
                        this.PrefixIsWord = true;
                    }
                    else
                    {
                        indexMemberWords.Add(w);
                    }
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
                    subgroup.DoIndexOfSmallAmount();
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
        void DoIndexOfSmallAmount()
        {
            //check ext
            int j = unIndexMemberWords.Count;
            int thisPrefixLen = this.PrefixLen;
            int doSepAt = thisPrefixLen;
            for (int i = 0; i < j; ++i)
            {
                string w = unIndexMemberWords[i];
                if (w == this.Prefix)
                {
                    this.PrefixIsWord = true;
                    break;
                }
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


        internal void FindBreak(WordVisitor visitor)
        {
            //recursive
            char c = visitor.Char;
            visitor.FoundWord = false;
            if (wordGroups != null)
            {
                WordGroup foundSubGroup;
                if (wordGroups.TryGetValue(c, out foundSubGroup))
                {
                    //found next group

                    if (!visitor.IsEnd)
                    {
                        int index = visitor.CurrentIndex;
                        visitor.SetCurrentIndex(index + 1);


                        foundSubGroup.FindBreak(visitor);

                        if (!visitor.FoundWord)
                        {
                            //not found in deeper level
                            visitor.SetCurrentIndex(index);
                        }
                    }
                }
            }
            //-------
            if (!visitor.FoundWord)
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
                    int len = (pos - latestBreak);
                    int n = unIndexMemberWords.Count;

                    List<CandidateWord> candidateWords = new List<CandidateWord>();

                    for (int i = 0; i < n; ++i)
                    {
                        //begin new word
                        string w = unIndexMemberWords[i];
                        int savedIndex = visitor.CurrentIndex;
                        c = visitor.Char;
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
                            if (candidate.IsFullMatch())
                            {
                                candidateWords.Add(candidate);
                            }
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
                            visitor.SetCurrentIndex(visitor.LatestBreakAt);
                        }
                        else
                        {

                        }
                    }
                    else if (candidateWords.Count > 0)
                    {
                        //match more than 1 
                        //select only full match
                        //choose the longest 

                        CandidateWord candidate = candidateWords[candidateWords.Count - 1];
                        visitor.AddWordBreakAt(
                               visitor.LatestBreakAt + candidate.max_match);
                        visitor.SetCurrentIndex(visitor.LatestBreakAt);


                    }
                    else
                    {

                    }
                }
                if (!visitor.FoundWord)
                {
                    if (this.PrefixIsWord)
                    {
                        int savedIndex = visitor.CurrentIndex;
                        int newBerakAt = visitor.LatestBreakAt + this.PrefixLen;
                        visitor.SetCurrentIndex(newBerakAt);
                        //check next char can be the char of new word or not
                        //this depends on each lang 
                        char canBeStartChar = visitor.Char;
                        if (visitor.CanbeStartChar(canBeStartChar))
                        {
                            visitor.AddWordBreakAt(newBerakAt);
                        }
                        else
                        {
                            visitor.SetCurrentIndex(savedIndex);
                        }
                    }
                }
            }
            else
            {
                if (indexMemberWords != null)
                {
                    //find word
                    if (!visitor.FoundWord)
                    {
                        visitor.AddWordBreakAt(visitor.LatestBreakAt + this.PrefixLen);
                        visitor.SetCurrentIndex(visitor.LatestBreakAt);
                    }
                    else
                    {
                    }

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