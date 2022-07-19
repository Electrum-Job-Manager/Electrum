using Electrum.Core.Logging;
using Serilog;
using Serilog.Context;
using System.Runtime.CompilerServices;

namespace Electrum.Logging.SeriLog
{
    public class ElectrumSerilogLogger<T> : ILogProvider<T>
    {

        public ILogger Log { get; }

        public ElectrumSerilogLogger()
        {
            Log = Serilog.Log.ForContext<T>();
        }
        
        public void Debug(string message)
        {
            Log.Debug(message);
        }

        public void Debug(string template, params object[] args)
        {
            Log.Debug(template, args);
        }

        public void Debug(Exception ex, string message)
        {
            Log.Debug(ex, message);
        }

        public void Debug(Exception ex, string template, params object[] args)
        {
            Log.Debug(ex, template, args);
        }

        public void Error(string message)
        {
            Log.Error(message);
        }

        public void Error(string template, params object[] args)
        {
            Log.Error(template, args);
        }

        public void Error(Exception ex, string message)
        {
            Log.Error(ex, message);
        }

        public void Error(Exception ex, string template, params object[] args)
        {
            Log.Error(ex, template, args);
        }

        public void Fatal(string message)
        {
            Log.Fatal(message);
        }

        public void Fatal(string template, params object[] args)
        {
            Log.Fatal(template, args);
        }

        public void Fatal(Exception ex, string message)
        {
            Log.Fatal(ex, message);
        }

        public void Fatal(Exception ex, string template, params object[] args)
        {
            Log.Fatal(ex, template, args);
        }

        public void Info(string message)
        {
            Log.Information(message);
        }

        public void Info(string template, params object[] args)
        {
            Log.Information(template, args);
        }

        public void Info(Exception ex, string message)
        {
            Log.Information(ex, message);
        }

        public void Info(Exception ex, string template, params object[] args)
        {
            Log.Information(ex, template, args);
        }

        public void PopContext()
        {
            LogContext.
        }

        public void PushContext()
        {
            LogContext.Pu
        }

        public void SetContext(string key, string value)
        {
            throw new NotImplementedException();
        }

        public void Verbose(string message)
        {
            throw new NotImplementedException();
        }

        public void Verbose(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Verbose(Exception ex, string message)
        {
            throw new NotImplementedException();
        }

        public void Verbose(Exception ex, string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warning(string message)
        {
            throw new NotImplementedException();
        }

        public void Warning(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warning(Exception ex, string message)
        {
            throw new NotImplementedException();
        }

        public void Warning(Exception ex, string template, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}