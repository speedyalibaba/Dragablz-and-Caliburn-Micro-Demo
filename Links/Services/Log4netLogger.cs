using Caliburn.Micro;
using System;

namespace Links.Services
{
    public class Log4netLogger : ILog
    {
        #region Fields

        private readonly log4net.ILog _innerLogger;

        #endregion Fields

        #region Constructors

        public Log4netLogger(Type type)
        {
            _innerLogger = log4net.LogManager.GetLogger(type);
        }

        #endregion Constructors

        #region Methods

        public void Error(Exception exception)
        {
            _innerLogger.Error(exception.Message, exception);
        }

        public void Error(string message, Exception exception)
        {
            _innerLogger.Error(message, exception);
        }

        public void Info(string format, params object[] args)
        {
            _innerLogger.InfoFormat(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            _innerLogger.WarnFormat(format, args);
        }

        #endregion Methods
    }
}