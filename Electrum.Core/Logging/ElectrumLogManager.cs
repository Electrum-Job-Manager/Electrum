using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Logging
{
    public class ElectrumLogManager
    {

        public List<ILogProvider<object>> Loggers { get; set; } = new List<ILogProvider<object>> ();

        public void SetContext(string key, string value)
        {
            Loggers.ForEach(x => x.SetContext(key, value));
        }
        public void PushContext()
        {
            Loggers.ForEach(x => x.PushContext());
        }
        public void PopContext()
        {
            Loggers.ForEach(x => x.PopContext());
        }

        #region Info
        public void Info(string message)
        {
            Loggers.ForEach(x => x.Info(message));
        }
        public void Info(string template, params object[] args)
        {
            Loggers.ForEach(x => x.Info(template, args));
        }
        public void Info(Exception ex, string message)
        {
            Loggers.ForEach(x => x.Info(ex, message));
        }
        public void Info(Exception ex, string template, params object[] args)
        {
            Loggers.ForEach(x => x.Info(ex, template, args));
        }
        #endregion

        #region Error
        public void Error(string message)
        {
            Loggers.ForEach(x => x.Error(message));
        }
        public void Error(string template, params object[] args)
        {
            Loggers.ForEach(x => x.Error(template, args));
        }
        public void Error(Exception ex, string message)
        {
            Loggers.ForEach(x => x.Error(ex, message));
        }
        public void Error(Exception ex, string template, params object[] args)
        {
            Loggers.ForEach(x => x.Error(ex, template, args));
        }
        #endregion

        #region Warning
        public void Warning(string message)
        {
            Loggers.ForEach(x => x.Warning(message));
        }
        public void Warning(string template, params object[] args)
        {
            Loggers.ForEach(x => x.Warning(template, args));
        }
        public void Warning(Exception ex, string message)
        {
            Loggers.ForEach(x => x.Warning(ex, message));
        }
        public void Warning(Exception ex, string template, params object[] args)
        {
            Loggers.ForEach(x => x.Warning(ex, template, args));
        }
        #endregion

        #region Fatal
        public void Fatal(string message)
        {
            Loggers.ForEach(x => x.Fatal(message));
        }
        public void Fatal(string template, params object[] args)
        {
            Loggers.ForEach(x => x.Fatal(template, args));
        }
        public void Fatal(Exception ex, string message)
        {
            Loggers.ForEach(x => x.Fatal(ex, message));
        }
        public void Fatal(Exception ex, string template, params object[] args)
        {
            Loggers.ForEach(x => x.Fatal(ex, template, args));
        }
        #endregion

        #region Verbose
        public void Verbose(string message)
        {
            Loggers.ForEach(x => x.Verbose(message));
        }
        public void Verbose(string template, params object[] args)
        {
            Loggers.ForEach(x => x.Verbose(template, args));
        }
        public void Verbose(Exception ex, string message)
        {
            Loggers.ForEach(x => x.Verbose(ex, message));
        }
        public void Verbose(Exception ex, string template, params object[] args)
        {
            Loggers.ForEach(x => x.Verbose(ex, template, args));
        }
        #endregion

        #region Debug
        public void Debug(string message)
        {
            Loggers.ForEach(x => x.Debug(message));
        }
        public void Debug(string template, params object[] args)
        {
            Loggers.ForEach(x => x.Debug(template, args));
        }
        public void Debug(Exception ex, string message)
        {
            Loggers.ForEach(x => x.Debug(ex, message));
        }
        public void Debug(Exception ex, string template, params object[] args)
        {
            Loggers.ForEach(x => x.Debug(ex, template, args));
        }
        #endregion



    }
}
