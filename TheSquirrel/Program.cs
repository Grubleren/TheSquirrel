using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TheSquirrel
{
    internal class Program
    {

        static void Main(string[] args)
        {
            //          input   := <options>*[<fileIn>[<fileOut>]]
            //          options := (/<char>[=<number>])*
            //          fileIn  := <string>
            //          fileOut := <string>
            //
            //          option /u       file is in UTF8 format
            //          option /c=n     sorting from column n (n >= 1)
            //          option /v       do not ignore uppercase

            new TheSquirrel().ExtractOptions(args);
        }
    }

    public class TheSquirrel
    {
        // Main class
        // Reads option and filenames
        string fileNameIn = "";
        string fileNameOut = "";
        List<string> options;

        public void ExtractOptions(string[] args)
        {
            options = new List<string>();

            int nargs = args.Length;
            if (nargs == 0)
            {
                bool flag = false;
                while (!flag)
                {
                    Console.Write("Enter input file ");
                    fileNameIn = Console.ReadLine();
                    if (File.Exists(fileNameIn))
                    {
                        flag = true;
                    }
                    else
                        Console.WriteLine("No such file '{0}'", fileNameIn);
                }

                Console.Write("Enter output file ");
                fileNameOut = Console.ReadLine();

                ProgramStart(options, fileNameIn, fileNameOut);


            }
            else
            {
                bool optionsOk = true;
                int nOptionParams = 0;
                for (int i = 0; i < nargs; i++)
                {
                    if (args[i][0] != '/')
                    {
                        break;
                    }
                    nOptionParams++;
                }
                for (int i = 0; i < nOptionParams; i++)
                {
                    string[] optionSplit = args[i].Split('/');
                    optionsOk = AddOptions(options, optionSplit);
                }
                if (!optionsOk)
                    Console.WriteLine("Unknown option");
                else
                {
                    if (!qOption(options))
                        AddFileNames(options, args, nOptionParams, nargs - nOptionParams);
                }
            }
        }
        bool AddOptions(List<string> options, string[] optionSplit)
        {
            foreach (string option in optionSplit)
            {
                int nc;
                if (option.Length == 1)
                {
                    if (option[0] == 'u' || option[0] == 'v')
                    {
                        options.Add(option);
                    }
                    else if (option[0] == '?')
                    {
                        Console.WriteLine();
                        Console.WriteLine("Command line syntax");
                        Console.WriteLine("input        := <options>*[<fileIn>[<fileOut>]]");
                        Console.WriteLine("options      := (/<char>[=<number>])*");
                        Console.WriteLine("fileIn       := <string>");
                        Console.WriteLine("fileOut      := <string>>");
                        Console.WriteLine();
                        Console.WriteLine("Options:");
                        Console.WriteLine("option /?    Help");
                        Console.WriteLine("option /v    Case sensitive");
                        Console.WriteLine("option /u    File is in UTF8 format");
                        Console.WriteLine("option /c=n  Sorting starting from column n (n >= 1)");
                        Console.WriteLine();
                        Console.WriteLine("Example:");
                        Console.WriteLine("TheSquirrel.exe /u /c=10 /v unsorted.txt sorted.txt");
                        Console.WriteLine("Input file has UTF8 format");
                        Console.WriteLine("Sorting starts in column 10");
                        Console.WriteLine("Sorting is casesensitive");
                        Console.WriteLine("Input file unsorteed.txt");
                        Console.WriteLine("Output file sorted.txt");

                    }
                    else
                        return false;
                }
                else if (option.Length >= 3 && option[0] == 'c' && option[1] == '=' && int.TryParse(option.Substring(2), out nc))
                {
                    options.Add(option);

                }
                else if (option.Length != 0)
                    return false;
            }

            return true;
        }


        bool OptionInList(List<string> options, char ch)
        {
            if (options.Count == 0)
                return false;

            foreach (string option in options)
            {
                if (option[0] == ch)
                    return true;
            }
            return false;
        }
        void AddFileNames(List<string> options, string[] args, int start, int n)
        {
            if (n == 0)
            {
                bool flag = false;
                while (!flag)
                {
                    Console.Write("Enter input file ");
                    fileNameIn = Console.ReadLine();
                    fileNameIn = fileNameIn.Replace("\"", "");
                    if (File.Exists(fileNameIn))
                    {
                        flag = true;
                    }
                    else
                        Console.WriteLine("No such file '{0}'", fileNameIn);
                }

                Console.Write("Enter output file ");
                fileNameOut = Console.ReadLine();
                fileNameOut = fileNameOut.Replace("\"", "");

                ProgramStart(options, fileNameIn, fileNameOut);

            }
            else if (n == 1)
            {
                bool flag = false;
                fileNameIn = args[start];
                fileNameIn = fileNameIn.Replace("\"", "");

                if (!File.Exists(fileNameIn))
                    Console.WriteLine("No such file '{0}'", fileNameIn);
                else
                    flag = true;

                while (!flag)
                {
                    Console.Write("Enter input file ");
                    fileNameIn = Console.ReadLine();
                    fileNameIn = fileNameIn.Replace("\"", "");
                    if (File.Exists(fileNameIn))
                    {
                        flag = true;
                    }
                    else
                        Console.WriteLine("No such file '{0}'", fileNameIn);
                }

                Console.Write("Enter output file ");
                fileNameOut = Console.ReadLine();
                fileNameOut = fileNameOut.Replace("\"", "");

                ProgramStart(options, fileNameIn, fileNameOut);
            }
            else if (n == 2)
            {
                fileNameIn = args[start];
                fileNameIn = fileNameIn.Replace("\"", "");
                fileNameOut = args[start + 1];
                fileNameOut = fileNameOut.Replace("\"", "");

                ProgramStart(options, fileNameIn, fileNameOut);
            }
            else
                throw new Exception("Error");

        }
        // Reads one line at a time from the input file
        // Input each line to a Line object
        // Adds each Line object to a List<Line>
        // Sort the List<Line>
        // Write each sorted Line to the output file
        void ProgramStart(List<string> options, string fileNameIn, string fileNameOut)
        {
            StreamReader reader = null;
            StreamWriter writer = null;
            StreamWriter logWriter = null;

            if (fileNameIn == fileNameOut)
            {
                Console.WriteLine("Input and output file names identiclal");

                return;
            }

            try
            {
                if (uOption(options))
                {
                    reader = new StreamReader(fileNameIn, Encoding.UTF8);
                    writer = new StreamWriter(fileNameOut, false, Encoding.UTF8);
                }
                else
                {
                    reader = new StreamReader(fileNameIn, Encoding.Default);
                    writer = new StreamWriter(fileNameOut, false, Encoding.Default);
                }

                logWriter = new StreamWriter("TheSquirrel.log", true);

                List<Line> list = new List<Line>();

                int lineNumber = 1;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    list.Add(new Line(this, options, line, lineNumber, fileNameIn, writer, logWriter));
                    lineNumber++;
                }

                list.Sort();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                logWriter.WriteLine(DateTime.Now);
                logWriter.WriteLine("ERROR: {0}", e.Message, true);
                logWriter.WriteLine("ERROR: {0}", e.StackTrace, true);

            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (writer != null)
                    writer.Close();
                if (logWriter != null)
                    logWriter.Close();
            }
        }
        public bool qOption(List<string> options)
        {
            foreach (string option in options)
            {
                if (option[0] == '?')
                {
                    return true;
                }

            }
            return false;
        }
        public bool uOption(List<string> options)
        {
            foreach (string option in options)
            {
                if (option[0] == 'u')
                {
                    return true;
                }

            }
            return false;
        }
        public bool vOption(List<string> options)
        {
            foreach (string option in options)
            {
                if (option[0] == 'v')
                {
                    return true;
                }

            }
            return false;
        }
        public int cOption()
        {
            int startColumn = 1;
            foreach (string option in options)
            {
                if (option[0] == 'c')
                {
                    startColumn = int.Parse(option.Substring(2));
                    break;
                }

            }
            return startColumn;
        }
    }
}