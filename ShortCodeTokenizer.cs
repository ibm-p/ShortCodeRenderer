using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShortCodeRenderer
{
    public class ShortCodeTokenizeValue
    {
        public int Index { get; set; }
        public int EndIndex { get; set; }
        //public string Content { get; set; }
        public bool IsSlashUsed { get; set; }
        public bool InTag { get; set; }
        public int Length => EndIndex - Index + 1;
        public string Content { get; set; }
        public string TagName { get; set; }
        public bool Unclosed { get; set; }
        public ShortCodeAttributes Attributes { get; set; }
        public ShortCodeTokenizeValue Linked { get; set; } // For linking tags, e.g. [/tag] to [tag]
    }
    public class ShortCodeTokenizer
    {
        private void SkipSpace(ref string input, ref int index)
        {
            while (index < input.Length && char.IsWhiteSpace(input[index]))
            {
                index++;
            }
        }
        private void ProcessTag(ref string input, ref int index, ref ShortCodeTokenizeValue shortCode)
        {
            StringBuilder tagName = new StringBuilder();
            while (index < input.Length && !char.IsWhiteSpace(input[index]) && input[index] != ']')
            {
                tagName.Append(input[index]);
                index++;
            }
            ShortCodeAttributes attributes = new ShortCodeAttributes();
            if (tagName.Length > 0)
            {
                bool inQuote = false;
                char quoteChar = '\0';
                bool equalsFound = false;
                StringBuilder key = new StringBuilder();
                StringBuilder value = new StringBuilder();
                SkipSpace(ref input, ref index);
                while (index < input.Length)
                {
                    char cur = input[index];

                    if (cur == '"' || cur == '\'')
                    {
                        if (inQuote && cur == quoteChar)
                        {
                            inQuote = false;
                        }
                        else if (!inQuote)
                        {
                            inQuote = true;
                            quoteChar = cur;
                        }
                        index++;
                        continue;
                    }
                    if (!inQuote)
                    {
                        if (cur == '=')
                        {
                            equalsFound = true;
                            index++;
                            continue;
                        }
                        else if (cur == ']')
                        {
                            if (key.Length > 0)
                            {
                                attributes[key.ToString()] = new ShortCodeAttribute()
                                {
                                    IsInnerAttribute = false,
                                    Name = key.ToString(),
                                    Value = value.ToString()
                                };
                                key.Clear();
                                value.Clear();
                                equalsFound = false;
                            }
                            break;
                        }
                        else if (char.IsWhiteSpace(cur))
                        {
                            if (key.Length > 0)
                            {
                                attributes[key.ToString()] = new ShortCodeAttribute()
                                {
                                    IsInnerAttribute = false,
                                    Name = key.ToString(),
                                    Value = value.ToString()
                                };
                                key.Clear();
                                value.Clear();
                                equalsFound = false;
                            }
                            index++;
                            continue;
                        }
                    }
                    if (equalsFound)
                        value.Append(cur);
                    else
                        key.Append(cur);
                    index++;
                }
            }
            shortCode.TagName = tagName.ToString();
            shortCode.Attributes = attributes;


        }

        public void LinkTags(List<ShortCodeTokenizeValue> tokens)
        {

            //Performans uyarısı,
            //Çok büyük veri kümelerinde tarama işlemi önemli ölçüde yavaşlatabilir.
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                var token = tokens[i];
                if (!token.IsSlashUsed || !token.InTag)
                    continue;
                for (int j = 0; j < tokens.Count; j++)
                {
                    if (i == j)
                        continue;
                    var otherToken = tokens[j];
                    if (otherToken.IsSlashUsed || !otherToken.InTag || otherToken.TagName != token.TagName || otherToken.Linked != null)
                        continue;
                    otherToken.Linked = token;
                    break;
                }
            }
        }
        public List<ShortCodeTokenizeValue> Tokenize(ref string input, bool linkClosed = false)
        {
            char open = '[';
            char close = ']';

            int tLen = input.Length;
            StringBuilder content = new StringBuilder();
            StringBuilder tagContent = new StringBuilder();
            bool isCLoseUsed = false; //[/TEST] gibi durumlarda close kullanımı kontrolü
            bool inTag = false; // Etiket içinde olma durumu [TEST]
            char specialChar = '\0'; // Özel karakter kontrolü, örneğin [[[]]] gibi durumlar için
            int startIndex = 0;
            List<ShortCodeTokenizeValue> values = new List<ShortCodeTokenizeValue>();
            Dictionary<string, ShortCodeTokenizeValue> tagsForLinking = linkClosed ? new  Dictionary<string, ShortCodeTokenizeValue>() : null;
            for (int i = 0; i < tLen; i++)
            {
                char cur = input[i];
                char prev = i > 0 ? input[i - 1] : '\0';
                char next = i < tLen - 1 ? input[i + 1] : '\0';
                if (specialChar == '\0' && !inTag && cur == open && next == open)
                {
                    specialChar = cur;
                    continue;
                }
                if (specialChar == '\0' && cur == close && next == close)
                {
                    specialChar = cur;
                    continue;
                }

                if (specialChar == '\0')
                {
                    if (!inTag && cur == open && next != open && next != close && prev != open)
                    {
                        if (content.Length > 0)
                        {
                            values.Add(new ShortCodeTokenizeValue
                            {
                                Index = startIndex,
                                EndIndex = i - 1,
                                Content = content.ToString(),
                                InTag = false
                            });
                            content.Clear();
                        }
                        inTag = true;
                        startIndex = i;
                        SkipSpace(ref input, ref i);
                        if (next != '/')
                        {
                            var tokenizeVal = new ShortCodeTokenizeValue
                            {
                                Index = startIndex,
                                //EndIndex = i,
                                //Content = tagContent.ToString(),
                                TagName = tagContent.ToString(),
                                IsSlashUsed = false,
                                InTag = true
                            };
                            i++;
                            ProcessTag(ref input, ref i, ref tokenizeVal);
                            inTag = false;
                            tokenizeVal.EndIndex = i;
                            values.Add(tokenizeVal);
                            if (linkClosed)
                                tagsForLinking[tokenizeVal.TagName] = tokenizeVal;
                            startIndex = i + 1;
                        }

                        continue;
                    }
                    if (inTag)
                    {
                        if (cur == '/' && tagContent.Length == 0)
                        {
                            //Örnek [/TEST]
                            isCLoseUsed = true;
                            continue;
                        }
                        //Attribute kontrolü
                        if (!isCLoseUsed && cur == ' ')
                        {

                        }
                        else if (cur == close)
                        {
                            var last = new ShortCodeTokenizeValue
                            {
                                Index = startIndex,
                                EndIndex = i,
                                TagName = tagContent.ToString(),
                                IsSlashUsed = isCLoseUsed,
                                InTag = true
                            };
                            values.Add(last);
                            //Bir sonraki karakterden başlamalı
                            startIndex = i + 1;
                            inTag = false;
                            if (isCLoseUsed && linkClosed)
                            {
                                //Eğer linkleme yapılacaksa, mevcut tagı aynı isimli kapatılmamış tag ile ilişkilendir
                                if(tagsForLinking.TryGetValue(last.TagName, out var target))
                                {
                                    target.Linked = last;
                                    tagsForLinking.Remove(last.TagName);
                                }
                                else
                                {
                                    last.Unclosed = true;
                                }
                            }
                            tagContent.Clear();
                            isCLoseUsed = false;

                            continue;
                        }
                    }
                }
                else
                {
                    if (cur != specialChar)
                        specialChar = '\0';
                }

                if (inTag)
                {
                    tagContent.Append(cur);
                }
                else
                {
                    content.Append(cur);
                }
            }
            if (content.Length > 0)
            {
                values.Add(new ShortCodeTokenizeValue
                {
                    Index = startIndex,
                    EndIndex = input.Length - 1,
                    Content = content.ToString(),
                    InTag = false
                });
                //content.Clear();
            }
            return values;
        }


    }
}
