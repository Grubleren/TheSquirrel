using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TheSquirrel
{
    // Line object is a wrapper around a text Line
    // Implements the IComparable interface ( int CompareTo(object obj) ) ake it possible to use C# List<Line>.Sort()
    // Members:
    // writer, used for writing the result to the output file
    // text, a text line from teh input file
    // tokens, a list of tokens, each line is split into a List<Token> 
    public class Line : IComparable
    {
        TheSquirrel theSquirrel;
        List<string> options;
        string text;
        int lineNumber;
        string fileNameIn;
        StreamWriter writer;
        StreamWriter logWriter;

        List<Token> tokens;

        // Constructor
        // Extracts tokens fom the input line
        public Line(TheSquirrel theSquirrel, List<string> options, string text, int lineNumber, string fileNameIn, StreamWriter writer, StreamWriter logWriter)
        {
            this.theSquirrel = theSquirrel;
            this.options = options;
            this.text = text;
            this.lineNumber = lineNumber;
            this.fileNameIn = fileNameIn;
            this.writer = writer;
            this.logWriter = logWriter;

            tokens = new List<Token>();

            ExtractTokens();
        }

        // Find the tokens in an input line
        // Tokens are separated by one or more white space (0 <= char <= 32)
        // Add the tokens to the List<Token> 
        void ExtractTokens()
        {
            Token token = null;
            int tokenNumber = 0;
            string ending = text;
            while (ending != "")
            {
                ending = FindToken(ending, out token);
                token.tokenNumber = tokenNumber;
                token.text = token.text.Replace("Aa", "Å");
                token.text = token.text.Replace("aa", "å");

                for (int i = 0; i < token.text.Length; i++)
                {
                    int x = token.text[i];
                    if (x < 0 || x > 255)
                    {
                        logWriter.WriteLine("\n{0}", DateTime.Now);
                        logWriter.WriteLine("Input file name: {0}", fileNameIn);
                        logWriter.WriteLine("Line: {0} Token: {1}", lineNumber, tokenNumber, true);
                        logWriter.WriteLine("Unexpected input value: {0}", x, true);
                        if (theSquirrel.uOption(options))
                            logWriter.WriteLine("Is this really an UTF8 file");
                        else
                            logWriter.WriteLine("This may be an UTF8 file");
                    }
                }

                tokens.Add(token);
                tokenNumber++;
            }
        }

        // Remove leading white space 
        // Find next token by searching for white space
        string FindToken(string text, out Token token)
        {
            int pos;
            text = ClearStart(text);
            bool err = FindW(text, out pos);
            if (err)
            {
                token = new Token(theSquirrel, options, text, lineNumber, logWriter);
                return "";
            }
            token = new Token(theSquirrel, options, text.Substring(0, pos), lineNumber, logWriter);
            return text.Substring(pos);
        }

        // Remove leading white space
        string ClearStart(string text)
        {
            int pos;
            bool err = FindNW(text, out pos);
            if (err)
                return "";
            return text.Substring(pos);
        }

        // Skip all white space until next non white space
        bool FindNW(string text, out int pos)
        {
            bool err = false;
            int i = 0;
            while (i < text.Length)
            {
                if (!IsWhite(text[i]))
                    break;
                i++;
            }
            pos = i;
            if (i == text.Length)
                err = true;
            return err;
        }

        // Find next white space
        bool FindW(string text, out int pos)
        {
            int i = 0;
            while (i < text.Length)
            {
                if (IsWhite(text[i]))
                    break;
                i++;
            }
            pos = i;
            return i == text.Length;

        }

        // Check for white space (0<= char <= 32 (could be a userdefined table stored in a .ini file)
        bool IsWhite(char ch)
        {
            return ch >= 0 && ch <= 32 || ch == 127 || ch == 129 || ch == 141 || ch == 143 || ch == 144 || ch == 157 || ch == 160 || ch == 173;

        }

        // Compare two Line objects by comparing each token, token
        public int CompareTo(object obj)
        {
            int startColumn = theSquirrel.cOption() - 1;
            Line line1 = (Line)obj;
            int length = Math.Min(tokens.Count, line1.tokens.Count);
            int compare = 0;
            for (int i = startColumn; i < length; i++)
            {
                Token token = tokens[i];
                Token token1 = line1.tokens[i];
                compare = token.CompareTo(token1);

                if (compare != 0)
                    break;
            }
            if (compare == 0)
                compare = tokens.Count > line1.tokens.Count ? 1 : tokens.Count == line1.tokens.Count ? 0 : -1;
            return compare;
        }

        public void WriteLine()
        {
            writer.WriteLine(text);
        }
    }
}
