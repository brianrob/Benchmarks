using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks.UI.App.Services
{
    public class JobsService
    {
        static string _driverPath = @"C:\Users\sebros\Documents\Projects\benchmarks\src\BenchmarksDriver\bin\Debug\netcoreapp2.1";

        private const string _plaintextJobs = "-j https://raw.githubusercontent.com/aspnet/Benchmarks/master/src/Benchmarks/benchmarks.plaintext.json";
        private const string _htmlJobs = "-j https://raw.githubusercontent.com/aspnet/Benchmarks/master/src/Benchmarks/benchmarks.html.json";
        private const string _jsonJobs = "-j https://raw.githubusercontent.com/aspnet/Benchmarks/master/src/Benchmarks/benchmarks.json.json";
        private const string _multiQueryJobs = "-j https://raw.githubusercontent.com/aspnet/Benchmarks/master/src/Benchmarks/benchmarks.multiquery.json";
        private const string _httpClientJobs = "-j https://raw.githubusercontent.com/aspnet/Benchmarks/master/src/Benchmarks/benchmarks.httpclient.json";
        private const string _signalRJobs = "-j https://raw.githubusercontent.com/aspnet/AspNetCore/master/src/SignalR/benchmarkapps/BenchmarkServer/signalr.json -t SignalR";
        private const string _plaintextPlatformJobs = "-j https://raw.githubusercontent.com/aspnet/AspNetCore/master/src/Servers/Kestrel/perf/PlatformBenchmarks/benchmarks.plaintext.json";
        private const string _jsonPlatformJobs = "-j https://raw.githubusercontent.com/aspnet/AspNetCore/master/src/Servers/Kestrel/perf/PlatformBenchmarks/benchmarks.json.json";
        private const string _routingJobs = "-j https://raw.githubusercontent.com/aspnet/AspNetCore/master/src/Routing/benchmarkapps/Benchmarks/benchmarks.json";
        //private static const string _basicApiJobs = "--database MySql --jobs https://raw.githubusercontent.com/aspnet/AspNetCore/master/src/Mvc/benchmarkapps/BasicApi/benchmarks.json";
        //private static const string _basicViewsJobs = "--database MySql --jobs https://raw.githubusercontent.com/aspnet/AspNetCore/master/src/Mvc/benchmarkapps/BasicViews/benchmarks.json";
        //private static const string _http2Jobs = "--clientName H2Load -p Streams=70 --headers None --connections $CPU_COUNT --clientThreads $CPU_COUNT";

        private ConcurrentDictionary<string, Process> _processes = new ConcurrentDictionary<string, Process>();

        public Process GetProcessById(string id)
        {
            _processes.TryGetValue(id, out var process);

            return process;
        }

        public async Task StopProcess(string id)
        {
            if (String.IsNullOrEmpty(id) || !_processes.TryGetValue(id, out var process))
            {
                return;
            }

            var processId = process.Id;

            if (!process.HasExited)
            {
                process.CloseMainWindow();

                if (!process.HasExited)
                {
                    process.Kill();
                }

                process.Dispose();

                do
                {
                    await Task.Delay(1000);

                    try
                    {
                        process = Process.GetProcessById(processId);
                        process.Refresh();
                    }
                    catch
                    {
                        process = null;
                    }

                } while (process != null && !process.HasExited);
            }
        }

        public string StartDriver(string arguments, StringBuilder output, Action outputDataReceived, Func<Task> onJobFinished)
        {
            var id = Guid.NewGuid().ToString("n");

            var process = new Process()
            {
                StartInfo = {
                FileName = "dotnet",
                Arguments = "BenchmarksDriver.dll " + arguments,
                WorkingDirectory = _driverPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            },
                EnableRaisingEvents = true
            };

            _processes.TryAdd(id, process);

            process.OutputDataReceived += (_, e) =>
            {
                if (e != null && e.Data != null)
                {
                    Console.WriteLine(e.Data);
                    output.AppendLine(e.Data);

                    outputDataReceived?.Invoke();
                }
            };

            process.Exited += (_, e) =>
            {
                if (onJobFinished != null)
                {
                    onJobFinished().GetAwaiter().GetResult();
                }
            };

            process.Start();
            process.BeginOutputReadLine();

            return id;
        }

        public IEnumerable<JobDefinition> GetJobDescriptions()
        {
            // TODO: Instead of using the raw arguments we could assign all the properties to the local 
            // parameters so they could be tweaked

            return new JobDefinition[]
            {
                new JobDefinition("Plaintext", $"-n Plaintext {_plaintextJobs}"),
                new JobDefinition("Plaintext Platform", $"-n PlaintextPlatform {_plaintextPlatformJobs}"),
                new JobDefinition("Plaintext Non-Pipelined", $"-n PlaintextNonPipelined {_plaintextJobs}"),
                new JobDefinition("Plaintext MVC", $"-n MvcPlaintext {_plaintextJobs}"),
                new JobDefinition("Plaintext Routing", $"-n PlaintextRouting {_routingJobs}"),

                new JobDefinition("Json", $"-n Json {_jsonJobs}"),
                new JobDefinition("Json Platform", $"-n Json {_jsonPlatformJobs}"),
                new JobDefinition("Json MVC", $"-n MvcJson {_jsonJobs}"),
            };
        }

        public string GetDriverFileName(string filename)
        {
            return System.IO.Path.Combine(_driverPath, filename);
        }

    }
}
