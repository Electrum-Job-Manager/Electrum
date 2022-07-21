using Electrum.Core.Store;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Electrum.Core.Logging
{
    public class JobLogger
    {

        public class JobLogRow
        {
            [ElectrumStoreKey]
            public Guid JobId { get; set; }
            public Guid RowId { get; set; }
            public DateTime UtcTimestamp { get; set; }
            public LogLevel Level { get; set; }
            public string Message { get; set; }
            public string? Template { get; set; }
            public Dictionary<string, string>? Properties { get; set; }
            public JobLogRowError? Error { get; set; }

            public JobLogRow(LogLevel level, string message)
            {
                RowId = Guid.NewGuid();
                UtcTimestamp = DateTime.UtcNow;
                Level = level;
                Message = message;
            }

            public JobLogRow(LogLevel level, string template, params object[] args)
            {
                RowId = Guid.NewGuid();
                UtcTimestamp = DateTime.UtcNow;
                Level = level;
                Template = template;
                FromTemplate(template, args);
            }

            public JobLogRow(Exception ex, LogLevel level, string message) : this(level, message)
            {
                Error = new JobLogRowError(ex);
            }

            public JobLogRow(Exception ex, LogLevel level, string template, params object[] args) : this(level, template, args)
            {
                Error = new JobLogRowError(ex);
            }

            private void FromTemplate(string template, params object[] args)
            {
                string pattern = @"\{([a-zA-Z\d]+)\}"; // Some text {NAME_OF_THE_PARAM} more text -> NAME_OF_THE_PARAM
                RegexOptions options = RegexOptions.Multiline;
                int i = 0;
                Properties = new Dictionary<string, string>();
                foreach (Match m in Regex.Matches(template, pattern, options))
                {
                    var paramName = m.Groups.Values.LastOrDefault()?.Value;
                    var paramValue = args[i++];
                    Properties.Add(paramName, paramValue?.ToString() ?? "-");
                }
                Message = template;
                foreach (var property in Properties)
                {
                    Message = Message.Replace($"{{{property.Key}}}", property.Value);
                }
            }

            public string FullMessage { get
                {
                    if(Error != null)
                    {
                        return Message + "\n" + Error?.ToString();
                    } else
                    {
                        return Message;
                    }
                }
            }

            public Microsoft.Extensions.Logging.LogLevel MELLevel
            {
                get
                {
                    switch(Level)
                    {
                        case LogLevel.Fine:
                        case LogLevel.Verbose:
                            return Microsoft.Extensions.Logging.LogLevel.Trace;
                        case LogLevel.Debug:
                            return Microsoft.Extensions.Logging.LogLevel.Debug;
                        case LogLevel.Info:
                            return Microsoft.Extensions.Logging.LogLevel.Information;
                        case LogLevel.Warning:
                            return Microsoft.Extensions.Logging.LogLevel.Warning;
                        case LogLevel.Error:
                            return Microsoft.Extensions.Logging.LogLevel.Error;
                        case LogLevel.Fatal:
                            return Microsoft.Extensions.Logging.LogLevel.Critical;
                        default:
                            return Microsoft.Extensions.Logging.LogLevel.Information;
                    }
                }
            }

            public void LogToMEL(ILogger logger)
            {
                if (Template != null)
                {
                    if (Error != null)
                    {
                        logger.Log(MELLevel, Error.ToException(), Template, Properties.Values.ToArray());
                    }
                    else
                    {
                        logger.Log(MELLevel, Template, Properties.Values.ToArray());
                    }
                }
                else
                {
                    if (Error != null)
                    {
                        logger.Log(MELLevel, Error.ToException(), Message);
                    }
                    else
                    {
                        logger.Log(MELLevel, Message);
                    }
                }
            }

            [Serializable]
            public class JobLogRowError
            {
                public DateTime TimeStamp { get; set; }
                public string? Message { get; set; }
                public string? StackTrace { get; set; }
                public string? TypeName { get; set; } = "Unknown error";

                public JobLogRowError()
                {
                    this.TimeStamp = DateTime.Now;
                }

                public JobLogRowError(string Message) : this()
                {
                    this.Message = Message;
                }

                public JobLogRowError(System.Exception ex) : this(ex.Message)
                {
                    this.StackTrace = ex.StackTrace;
                    this.TypeName = ex.GetType().Name;
                }

                public override string ToString()
                {
                    return this.TypeName + ": " + this.Message + "\n" + this.StackTrace;
                }

                public Exception ToException()
                {
                    return new JobLogRowErrorException(this);
                }

                [Serializable]
                public class JobLogRowErrorException : Exception
                {
                    private readonly JobLogRowError jobLogRowError;

                    public override string? StackTrace => jobLogRowError.StackTrace;
                    public override string Message => jobLogRowError.TypeName + ": " + jobLogRowError.Message ?? "";
                    public override string? Source => jobLogRowError.TypeName;

                    public JobLogRowErrorException(JobLogRowError jobLogRowError)
                    {
                        this.jobLogRowError = jobLogRowError;
                    }

                    public override string ToString()
                    {
                        return jobLogRowError.ToString();
                    }
                }
            }
        }

        internal JobLogger(ILogger<JobLogger> logger, Guid jobId, IJobLoggingClient jobLoggingClient, bool enableLiveLogging)
        {
            Logger = logger;
            JobId = jobId;
            JobLoggingClient = jobLoggingClient;
            LiveLoggingEnabled = enableLiveLogging;
        }

        private Task liveLoggingTask;
        private bool isShuttingDown;

        private void StartLiveLogging()
        {
            liveLoggingTask = Task.Run(async () =>
            {
                var allSentLogs = new List<JobLogRow>();
                while (!isShuttingDown)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    if (isShuttingDown) break;
                    var unsentLogs = logRows.Where(x => !allSentLogs.Any(y => y.RowId == x.RowId)).ToList();
                    if (unsentLogs.Any())
                    {
                        var logs = unsentLogs.ToArray();
                        allSentLogs.AddRange(logs);
                        JobLoggingClient.WriteRows(JobId, logs);
                    }
                }
            });
        }

        public bool LiveLoggingEnabled { get; set; }
        public Guid JobId { get; }
        public IJobLoggingClient JobLoggingClient { get; }
        public ILogger<JobLogger> Logger { get; }

        internal List<JobLogRow> logRows = new List<JobLogRow>();

        private void Log(JobLogRow row)
        {
            logRows.Add(row);
            if (LiveLoggingEnabled && liveLoggingTask == null)
            {
                StartLiveLogging();
            }
            row.LogToMEL(Logger);
        }

        public void SaveRows()
        {
            JobLoggingClient.WriteRows(JobId, logRows);
            if(liveLoggingTask != null)
            {
                isShuttingDown = true;
                Task.WaitAll(liveLoggingTask);
            }
        }

        #region Info
        public void Info(string message)
        {
            var row = new JobLogRow(LogLevel.Info, message);
            Log(row);
        }
        public void Info(string template, params object[] args)
        {
            var row = new JobLogRow(LogLevel.Info, template, args);
            Log(row);
        }
        public void Info(Exception ex, string message)
        {
            var row = new JobLogRow(ex, LogLevel.Info, message);
            Log(row);
        }
        public void Info(Exception ex, string template, params object[] args)
        {
            var row = new JobLogRow(ex, LogLevel.Info, template, args);
            Log(row);
        }
        #endregion

        #region Error
        public void Error(string message)
        {
            var row = new JobLogRow(LogLevel.Error, message);
            Log(row);
        }
        public void Error(string template, params object[] args)
        {
            var row = new JobLogRow(LogLevel.Error, template, args);
            Log(row);
        }
        public void Error(Exception ex, string message)
        {
            var row = new JobLogRow(ex, LogLevel.Error, message);
            Log(row);
        }
        public void Error(Exception ex, string template, params object[] args)
        {
            var row = new JobLogRow(ex, LogLevel.Error, template, args);
            Log(row);
        }
        #endregion

        #region Warning
        public void Warning(string message)
        {
            var row = new JobLogRow(LogLevel.Warning, message);
            Log(row);
        }
        public void Warning(string template, params object[] args)
        {
            var row = new JobLogRow(LogLevel.Warning, template, args);
            Log(row);
        }
        public void Warning(Exception ex, string message)
        {
            var row = new JobLogRow(ex, LogLevel.Warning, message);
            Log(row);
        }
        public void Warning(Exception ex, string template, params object[] args)
        {
            var row = new JobLogRow(ex, LogLevel.Warning, template, args);
            Log(row);
        }
        #endregion

        #region Fatal
        public void Fatal(string message)
        {
            var row = new JobLogRow(LogLevel.Fatal, message);
            Log(row);
        }
        public void Fatal(string template, params object[] args)
        {
            var row = new JobLogRow(LogLevel.Fatal, template, args);
            Log(row);
        }
        public void Fatal(Exception ex, string message)
        {
            var row = new JobLogRow(ex, LogLevel.Fatal, message);
            Log(row);
        }
        public void Fatal(Exception ex, string template, params object[] args)
        {
            var row = new JobLogRow(ex, LogLevel.Fatal, template, args);
            Log(row);
        }
        #endregion

        #region Verbose
        public void Verbose(string message)
        {
            var row = new JobLogRow(LogLevel.Verbose, message);
            Log(row);
        }
        public void Verbose(string template, params object[] args)
        {
            var row = new JobLogRow(LogLevel.Verbose, template, args);
            Log(row);
        }
        public void Verbose(Exception ex, string message)
        {
            var row = new JobLogRow(ex, LogLevel.Verbose, message);
            Log(row);
        }
        public void Verbose(Exception ex, string template, params object[] args)
        {
            var row = new JobLogRow(ex, LogLevel.Verbose, template, args);
            Log(row);
        }
        #endregion

        #region Debug
        public void Debug(string message)
        {
            var row = new JobLogRow(LogLevel.Debug, message);
            Log(row);
        }
        public void Debug(string template, params object[] args)
        {
            var row = new JobLogRow(LogLevel.Debug, template, args);
            Log(row);
        }
        public void Debug(Exception ex, string message)
        {
            var row = new JobLogRow(ex, LogLevel.Debug, message);
            Log(row);
        }
        public void Debug(Exception ex, string template, params object[] args)
        {
            var row = new JobLogRow(ex, LogLevel.Debug, template, args);
            Log(row);
        }
        #endregion

        #region Fine
        public void Fine(string message)
        {
            var row = new JobLogRow(LogLevel.Fine, message);
            Log(row);
        }
        public void Fine(string template, params object[] args)
        {
            var row = new JobLogRow(LogLevel.Fine, template, args);
            Log(row);
        }
        public void Fine(Exception ex, string message)
        {
            var row = new JobLogRow(ex, LogLevel.Fine, message);
            Log(row);
        }
        public void Fine(Exception ex, string template, params object[] args)
        {
            var row = new JobLogRow(ex, LogLevel.Fine, template, args);
            Log(row);
        }
        #endregion
    }
}
