using Electrum.Core;
using Electrum.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Client.ExternalProcesses
{
    public class GenericJobExecutor
    {

        public void ExecuteConfiguredJob(JobLogger logger, ElectrumJob job)
        {
            logger.Debug("Looking for command for job {Namespace}/{JobName}", job.Namespace.Name, job.JobName);
            var matchingJobs = Program.Configurations.Where(x => x.Namespace == job.Namespace.Name && x.Name == job.JobName);
            if(!matchingJobs.Any())
            {
                job.Status = Core.Enums.JobStatus.MissingExecutor;
                job.Error = "No configuration with " + job.Namespace.Name + "/" + job.JobName + " was found";
                logger.Error("No command for {Namespace}/{JobName} found", job.Namespace.Name, job.JobName);
                return;
            }

            var config = matchingJobs.First();
            // Create the process
            var proc = new Process();
            proc.StartInfo.FileName = config.ProcessPath;
            proc.StartInfo.Arguments = config.Args;
            // Allow capture of sysout and syserr
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            
            var timeoutInMs = (int) job.Timeout.TotalMilliseconds;
            var outStream = proc.StandardOutput;
            var errStream = proc.StandardError;
            var processWaiter = Task.Factory.StartNew(() => proc.WaitForExit(timeoutInMs));
            var outputReader = Task.Factory.StartNew((Action<object?>)ReadLines, Tuple.Create<Action<string>, StreamReader>((data) => logger.Info(data), outStream));
            var errorReader = Task.Factory.StartNew((Action<object?>)ReadLines, Tuple.Create<Action<string>, StreamReader>((data) => logger.Error(data), errStream));
            var didProcessFinish = processWaiter.Result;
            if(!didProcessFinish)
            {
                logger.Warning("Process did not complete in time, killing..");
                proc.Kill();
            }
            Task.WaitAll(outputReader, errorReader);
            job.Status = Core.Enums.JobStatus.TimedOut;
            job.Error = "The process timed out";
            logger.Info("Process exited with code: {ProcessExitCode}", proc.ExitCode);
        }

        private void ReadLines(object? packedParams)
        {
            if (packedParams == null) return;
            var paramsTuple = (Tuple<Action<string>, StreamReader>)packedParams;
            var reader = paramsTuple.Item2;
            var action = paramsTuple.Item1;
            string line;
            while((line = reader.ReadLine()) != null)
            {
                action(line);
            }
        }

    }
}
