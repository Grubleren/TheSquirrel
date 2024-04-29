using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
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
            //          option /i       do not ignore uppercase

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
            else if (nargs > 0)
            {
                int nOptionParams = 0;
                if (args[0][0] == '/')
                {
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
                        string[] optionSplit = args[i].Split(' ');
                        AddOptions(options, optionSplit);
                    }
                    AddFileNames(options, args, nOptionParams, nargs - nOptionParams);
                }
                else
                {
                    AddFileNames(options, args, nOptionParams, nargs);
                }
            }
        }
        void AddOptions(List<string> options, string[] optionSplit)
        {
            foreach (string option in optionSplit)
            {
                if (!option.StartsWith("/"))
                    throw new Exception("Error");
                else
                {
                    string opt = option.Substring(1);
                    int nc = 0;
                    if (opt.Length == 1)
                    {
                        if (opt[0] == 'u' || opt[0] == 'i')
                        {
                            if (!OptionInList(options, opt[0]))
                            {
                                options.Add(opt);
                            }
                        }
                        else
                            throw new Exception("Error");
                    }
                    else if (opt.Length >= 3 && opt[0] == 'c' && opt[1] == '=' && int.TryParse(opt.Substring(2), out nc))
                    {
                        if (!OptionInList(options, opt[0]))
                        {
                            options.Add(opt);
                        }

                    }
                }
            }
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
        public bool iOption(List<string> options)
        {
            foreach (string option in options)
            {
                if (option[0] == 'i')
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