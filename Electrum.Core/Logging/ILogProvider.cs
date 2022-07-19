using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core.Logging
{
    public interface ILogProvider<T>
    {

        public void SetContext(string key, string value);
        public void PushContext();
        public void PopContext();

        #region Info
        public void Info(string message);
        public void Info(string template, params object[] args);
        public void Info(Exception ex, string message);
        public void Info(Exception ex, string template, params object[] args);
        #endregion

        #region Error
        public void Error(string message);
        public void Error(string template, params object[] args);
        public void Error(Exception ex, string message);
        public void Error(Exception ex, string template, params object[] args);
        #endregion

        #region Warning
        public void Warning(string message);
        public void Warning(string template, params object[] args);
        public void Warning(Exception ex, string message);
        public void Warning(Exception ex, string template, params object[] args);
        #endregion

        #region Fatal
        public void Fatal(string message);
        public void Fatal(string template, params object[] args);
        public void Fatal(Exception ex, string message);
        public void Fatal(Exception ex, string template, params object[] args);
        #endregion

        #region Verbose
        public void Verbose(string message);
        public void Verbose(string template, params object[] args);
        public void Verbose(Exception ex, string message);
        public void Verbose(Exception ex, string template, params object[] args);
        #endregion

        #region Debug
        public void Debug(string message);
        public void Debug(string template, params object[] args);
        public void Debug(Exception ex, string message);
        public void Debug(Exception ex, string template, params object[] args);
        #endregion


    }
}
