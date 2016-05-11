using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace ConsoleApplication12
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var generator = new PhatomJsChartGenerator())
            {
                generator.Initialize();

                Stopwatch sw = new Stopwatch();
                sw.Start();
                const int loop = 1;
                for (int i = 0; i < loop; i++)
                {
                    var config = $@"{{
                        title: ""My title"",
                        width: {((i % 4) + 1) * 400},
                        height: {((i % 4) + 1) * 300},
                        chartType: ""Pie"",
                        columns: [
                            {{ type: ""string"", label: ""Topping"" }},
                            {{ type: ""number"", label: ""Slices"" }}
                        ],
                        Rows: [
                            [""Mushrooms"", 3],
                            [""Onions"", 1],
                            [""Olives"", 1],
                            [""Zucchini"", 1],
                            [""Pepperoni"", 2]
                        ]
                    }}";
                    var chartConfiguration = JsonConvert.DeserializeObject<ChartConfiguration>(config);
                    //string path = Path.GetFullPath("Generated\\" + DateTime.Now.Ticks + "_" + Guid.NewGuid().ToString("N") + ".png");
                    string path = Path.GetFullPath("Generated\\" + chartConfiguration.ComputeFileName());

                    generator.GenerateImage(chartConfiguration, path, false).Wait();
                    //if (!File.Exists(path))
                    //{
                    //    throw new Exception("File not generated");   
                    //}
                }
                sw.Stop();
                Console.WriteLine("total: " + sw.ElapsedMilliseconds + "ms");
                Console.WriteLine("average:" + sw.ElapsedMilliseconds / loop + "ms");

                string command;
                while (!string.IsNullOrEmpty(command = Console.ReadLine()))
                {
                    generator.SendCommand(command);
                    if (!generator.IsStarted)
                        break;
                }
            }
        }
    }
}
