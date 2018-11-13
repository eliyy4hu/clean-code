﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown
{
    public class MarkdownParser
    {
        private string markdownInput;
        public Token currentToken;
        public Token RootToken;
        private int index;
        private List<Delimiter> delimiters = new List<Delimiter>();

        public MarkdownParser(string markdownInput)
        {
            this.markdownInput = markdownInput;
            RootToken = new Token();
            currentToken = RootToken;
            currentToken.rootToken = RootToken;
            index = 0;
        }

        public bool IsCorrectToken(Token token)
        {
            return true;
        }


        public Token GetTokens()
        {
            var nextDelimeter = GetNextDelimiter();
            var text = markdownInput.Substring(0, nextDelimeter.index);
            currentToken.AddText(text);
            while (!(nextDelimeter is null))
            {
                bool closed;
                closed = nextDelimeter.canBeClosing && TryCloseToken(nextDelimeter);

                if (!closed && nextDelimeter.canBeStarting)
                {
                    var newTocken = new Token(nextDelimeter);
                    currentToken.AddToken(newTocken);
                    currentToken = newTocken;
                }
                else if (!closed && !nextDelimeter.canBeStarting)
                {
                    currentToken.AddText(nextDelimeter.delimiter);
                }

                if (closed)
                {
                    currentToken = currentToken.parentToken;
                }

                var prevIndex = nextDelimeter.index;
                var prevLength = nextDelimeter.delimiter.Length;
                nextDelimeter = GetNextDelimiter();
                if (nextDelimeter is null)
                {
                    text = markdownInput.Substring(prevIndex + prevLength);
                    currentToken.AddText(text);
                    break;
                }

                text = markdownInput.Substring
                    (prevIndex + prevLength, nextDelimeter.index - prevIndex - prevLength);
                currentToken.AddText(text);
            }

            return currentToken.rootToken;
        }

        private bool TryCloseToken(Delimiter closingDelimiter)
        {
            var token = this.currentToken;
            while (token.StartingDelimiter.delimiter != closingDelimiter.delimiter &&
                   token.ClosingDelimiter is null)
            {
                if (token.parentToken is null)
                    return false;
                token = token.parentToken;
            }

            if (token.StartingDelimiter.delimiter == closingDelimiter.delimiter)
            {
                token.ClosingDelimiter = closingDelimiter;
            }

            if (IsCorrectToken(token))
                return true;
            else
            {
                token.ClosingDelimiter = null;
                token.StartingDelimiter = null;
                token.InsertText(0, closingDelimiter.delimiter);

                return false;
            }
        }


        private Delimiter GetNextDelimiter()
        {
            if (index >= markdownInput.Length)
                return null;
            while (index < markdownInput.Length && markdownInput[index] != '_')
            {
                var e = markdownInput[index];
                index++;
            }


            var canBeClosing = false;
            var canBeStarting = false;
            if (index > 0 && markdownInput[index - 1] != ' ')
            {
                canBeClosing = true;
            }


            if (index < markdownInput.Length - 1 && markdownInput[index + 1] != ' ')
            {
                canBeStarting = true;
            }


            if (index >= markdownInput.Length)
                return null;
            var delimiter = new Delimiter("_", index, canBeClosing, canBeStarting);
            index++;
            return delimiter;
        }
    }
}