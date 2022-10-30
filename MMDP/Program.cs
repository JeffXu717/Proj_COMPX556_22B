using System;
using CommandLine;
using MMDP.Gen;
using MMDP.Solve;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace MMDP
{
    /// <summary>
    ///  Example: 
    ///  (gen 2 instance with n equal to 100, then write to file) gen -o 100 2 "D:\Temp\instances.csv"
    /// </summary>
    [Verb("gen", HelpText = "Generate instances")]
    class GenerateOptions
    {
        /// <summary>
        /// number of elements in one instance
        /// </summary>
        [Value(0)]
        public int N { get; set; }

        /// <summary>
        /// number of instances
        /// </summary>
        [Value(1)]
        public int NOfInstances { get; set; }

        /// <summary>
        /// output file path
        /// </summary>
        [Value(2)]
        public string OutputFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Write matrices to file")]
        public bool OutputFlag { get; set; }

        [Option('i', "intVal", Required = false, HelpText = "Values of coordinate should be integers.")]
        public bool IntFlag { get; set; }
    }

    /// <summary>
    /// Example:
    /// (solve first instance with m equal to 0.3n, max iteration 1000, 20 ants) sSolve "D:\Temp\instances.csv" 0.3 1000 20
    /// (solve first instance 10 times with m equal to 0.3n, max iteration 1000, 20 ants) sSolve "D:\Temp\instances.csv" 0.3 1000 20 -n 10
    /// (solve instances 10 times with m equal to 0.3n, max iteration 1000, 20 ants) sSolve "D:\Temp\instances.csv" 0.3 1000 20 -a -n 10
    /// (solve instances 10 times with m equal to 0.3n, max iteration 1000, 20 ants. Write result to file.) sSolve "D:\Temp\instances.csv" 0.3 1000 20 -a -n 10 -w "D:\Temp\result.txt"
    /// </summary>
    [Verb("sSolve", HelpText = "Solve instances serially")]
    class SSolveOptions
    {
        /// <summary>
        /// input file path
        /// </summary>
        [Value(0)]
        public string inputFile { get; set; }

        [Option('a', "all", Required = false, HelpText = "Solve all instaces")]
        public bool AllFlag { get; set; }

        [Value(1)]
        public float MRatio { get; set; }

        [Value(2)]
        public int MaxIter { get; set; }

        [Value(2)]
        public int NAnts { get; set; }

        [Option('n', "nTimes", Required = false, HelpText = "Execution times for one instance")]
        public int NTimes { get; set; }

        [Option('w', "writeToFile", Required = false, HelpText = "Write result to file")]
        public string desFile { get; set; }
    }

    /// <summary>
    /// (solve first instance with m equal to 0.3n, max iteration 1000, 2 colonies, each 10 ants) pSolve "D:\Temp\instances.csv" 0.3 1000 10 2
    /// (solve first instance 10 times with m equal to 0.3n, max iteration 1000, 2 colonies, each 10 ants) pSolve "D:\Temp\instances.csv" 0.3 1000 10 2 -n 10
    /// (solve instances 10 times with m equal to 0.3n, max iteration 1000, 2 colonies, each 10 ants) pSolve "D:\Temp\instances.csv" 0.3 1000 10 2 -a -n 10
    /// (solve instances 10 times with m equal to 0.3n, max iteration 1000, 20 ants. Write result to file.) pSolve "D:\Temp\instances.csv" 0.3 1000 10 2 -a -n 10 -w "D:\Temp\result.txt"
    /// </summary>
    [Verb("pSolve", HelpText = "Solve instances parallelly")]
    class PSolveOptions
    {
        /// <summary>
        /// input file path
        /// </summary>
        [Value(0)]
        public string inputFile { get; set; }

        [Option('a', "all", Required = false, HelpText = "Solve all instaces")]
        public bool AllFlag { get; set; }

        [Value(1)]
        public float MRatio { get; set; }

        [Value(2)]
        public int MaxIter { get; set; }
        
        [Value(3)]
        public int NAnts { get; set; }

        [Value(4)]
        public int NColonies { get; set; }

        [Option('n', "nTimes", Required = false, HelpText = "Execution times for one instance")]
        public int NTimes { get; set; }

        [Option('w', "writeToFile", Required = false, HelpText = "Write result to file")]
        public string desFile { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            var exitCode = CommandLine.Parser.Default.ParseArguments<GenerateOptions, SSolveOptions, PSolveOptions>(args)
                .MapResult(
                    (GenerateOptions opts) => RunGenAndReturnExitCode(opts),
                    (SSolveOptions opts) => RunSSolveAndReturnExitCode(opts),
                    (PSolveOptions opts) => RunPSolveAndReturnExitCode(opts),
                    errs => 1);
            return exitCode;
        }

        private static int RunSSolveAndReturnExitCode(SSolveOptions opts)
        {
            System.Console.WriteLine("Run serial Solver");
            Solver solver = new Solver();

            // read instances
            readInstances(solver, opts.inputFile);

            //printMatricesToConsole(solver.matriceList);
            
            string result = string.Empty; ;
            if (opts.AllFlag)
            {
                if (opts.NTimes > 0)
                {
                    result = solver.RunSerialACOForInstacnesNTimes(opts.MRatio, opts.MaxIter, opts.NAnts, opts.NTimes).ToString(); 
                }
            }
            else
            {
                if (opts.NTimes > 0)
                {
                    result = solver.RunSerialACOForFirstInstacneNTimes(opts.MRatio, opts.MaxIter, opts.NAnts, opts.NTimes).ToString();
                }
                else
                {
                    result = solver.RunSerialACOForFirstInstacne(opts.MRatio, opts.MaxIter, opts.NAnts).ToString();
                }
            }
            System.Console.WriteLine(result);

            if (opts.desFile != null)
            {
                File.WriteAllText(opts.desFile, result);
            }
            return 0;
        }

        private static int RunPSolveAndReturnExitCode(PSolveOptions opts)
        {
            System.Console.WriteLine("Run parallel Solver");
            Solver solver = new Solver();

            // read instances
            readInstances(solver, opts.inputFile);

            //printMatricesToConsole(solver.matriceList);

            string result = string.Empty; ;
            if (opts.AllFlag)
            {
                if (opts.NTimes > 0)
                {
                    result = solver.RunParallelACOForInstacnesNTimes(opts.MRatio, opts.MaxIter, opts.NAnts, opts.NColonies, opts.NTimes).ToString();
                }
            }
            else
            {
                if (opts.NTimes > 0)
                {
                    result = solver.RunParallelACOForFirstInstacneNTimes(opts.MRatio, opts.MaxIter, opts.NAnts, opts.NColonies, opts.NTimes).ToString();
                }
                else
                {
                    result = solver.RunParallelACOForFirstInstance(opts.MRatio, opts.MaxIter, opts.NAnts, opts.NColonies).ToString();
                }
            }
            System.Console.WriteLine(result);

            if (opts.desFile.Length > 0)
            {
                File.WriteAllText(opts.desFile, result);
            }
            return 0;
        }

        private static int RunGenAndReturnExitCode(GenerateOptions opts)
        {
            System.Console.WriteLine("Run Gen");
            System.Console.WriteLine("Gen {0} instances with n = {1}", opts.NOfInstances, opts.N);
            Generater generater = new Generater(opts.N, opts.NOfInstances, opts.IntFlag);
            generater.Gen();
            
            printMatricesToConsole(generater.matriceList);

            //Write matrices to file
            if (opts.OutputFlag)
            {
                string strSeperator = ",";
                StringBuilder sbOutput = new StringBuilder();

                for (int i = 0; i < generater.matriceList.Count; i++)
                {
                    var matrice = generater.matriceList[i];
                    for (int index = 0; index < matrice.Count; index++)
                    {
                        if (index % generater.N == 0)
                        {
                            sbOutput.AppendLine("");
                        }
                        sbOutput.Append(matrice[index]);
                        sbOutput.Append(strSeperator);
                    }
                    sbOutput.AppendLine("");
                }

                // To append more lines to the csv file
                File.AppendAllText(opts.OutputFile, sbOutput.ToString());
            }
            return 0;
        }

        private static void printMatricesToConsole(List<List<float>> matriceList)
        {
            for (int i = 0; i < matriceList.Count; i++)
            {
                var matrice = matriceList[i];
                int n = (int)Math.Sqrt(matrice.Count);
                for (int index = 0; index < matrice.Count; index++)
                {
                    if (index % n == 0)
                    {
                        System.Console.WriteLine();
                    }
                    System.Console.Write(string.Format("{0, -10}", matrice[index].ToString("0.00")));
                }
                System.Console.WriteLine();
            }
        }

        private static void readInstances(Solver solver, string inputFile)
        {
            StreamReader sr = new StreamReader(inputFile, System.Text.Encoding.Default);
            string strLine = string.Empty;
            List<float> matrice = new List<float>();
            while ((strLine = sr.ReadLine()) != null)
            {
                if (strLine == string.Empty)
                {
                    if (matrice.Count > 0)
                    {
                        solver.matriceList.Add(matrice);
                    }
                    matrice = new List<float>();
                }
                else
                {
                    string[] values = strLine.Split(",");
                    for (int i = 0; i < values.Length - 1; i++)
                    {
                        matrice.Add(float.Parse(values[i]));
                    }
                }
            }
            solver.matriceList.Add(matrice);
        }
    }
}
