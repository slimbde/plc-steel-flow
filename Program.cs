using System;
using System.Diagnostics;
using System.ServiceProcess;
using static PLCHandler.Delegates;


namespace PLCSteelFlow
{
    static class Program
    {
        static LogHandler logger;


        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                logger = GetLogger();

                PLCSteelFlowSvc service = new PLCSteelFlowSvc(logger);

                if (Environment.UserInteractive)
                    service.RunAsConsole(args);
                else
                    ServiceBase.Run(service);
            }
            catch (Exception ex) { logger?.Invoke($"{ex.Message}\n{ex.StackTrace}", EventLogEntryType.Error); }
        }



        /// <summary>
        /// Initializes app logger
        /// </summary>
        /// <returns></returns>
        private static LogHandler GetLogger()
        {
            if (Environment.UserInteractive)
                return delegate (string message, EventLogEntryType msgType) { Console.WriteLine($"{DateTime.Now.ToString("G")} {message}"); };

            EventLog eventLog1 = new EventLog();

            if (!EventLog.SourceExists("PLCSteelFlowSvcSource"))
                EventLog.CreateEventSource("PLCSteelFlowSvcSource", "PLCSteelFlowSvcLog");

            eventLog1.Source = "PLCSteelFlowSvcSource";
            eventLog1.Log = "PLCSteelFlowSvcLog";

            return eventLog1.WriteEntry;
        }
    }
}
