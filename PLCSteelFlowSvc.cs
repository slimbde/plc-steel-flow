using PLCHandler;
using PLCSteelFlow.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.ServiceProcess;
using System.Timers;
using static PLCHandler.Delegates;


namespace PLCSteelFlow
{
    public partial class PLCSteelFlowSvc : ServiceBase
    {
        Timer timer;                                        // polling timer
        LogHandler logger;                                  // output handler
        IPLCHandlerFactory plcFactory = new PLCFactory();
        bool enableTrace;                                   // enables full trace to output

        public PLCSteelFlowSvc(LogHandler logger)
        {
            try
            {
                InitializeComponent();

                this.logger = logger;
                this.CanStop = true;
                this.CanPauseAndContinue = true;
            }
            catch (Exception ex) { logger?.Invoke($"{ex.Message}\n{ex.StackTrace}", EventLogEntryType.Error); }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                logger?.Invoke("Service started", EventLogEntryType.Information);

                IEnumerable<ISyncPLCHandler> plcHandlers = plcFactory.Create();

                foreach (ISyncPLCHandler plc in plcHandlers)
                {
                    CcmFlowHandler fHandler = new CcmFlowHandler(logger);

                    plc.OnDataReceived += fHandler.Update;
                    plc.OnStatusLog += logger;
                }

                enableTrace = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("EnableTrace"));

                timer?.Dispose();
                timer = new Timer();
                double timeScale = double.Parse(ConfigurationManager.AppSettings["TimeScale"], CultureInfo.InvariantCulture);
                timer.Interval = 1000 * timeScale;
                timer.AutoReset = bool.Parse(ConfigurationManager.AppSettings["InfinitePolling"]);        // true - timer should fire its event infinitely (false - once)
                timer.Elapsed += (sndr, evargs) =>
                {
                    try
                    {
                        foreach (ISyncPLCHandler plc in plcHandlers)
                            plc.Read();
                    }
                    catch (Exception ex) { logger?.Invoke($"[Service OnElapsed]: {ex.Message}\n{ex.StackTrace}", EventLogEntryType.Error); }
                };
                timer.Start();

                if (enableTrace) logger?.Invoke($"[Service initialization]: success", EventLogEntryType.Information);
            }
            catch (Exception ex) { logger?.Invoke($"[OnStart]: {ex.Message}\n{ex.StackTrace}", EventLogEntryType.Error); }
        }

        public void RunAsConsole(string[] args)
        {
            OnStart(args);
            Console.ReadLine();
            OnStop();
        }

        protected override void OnStop()
        {
            timer.Stop();
            logger?.Invoke("Service stopped", EventLogEntryType.Information);
        }
    }
}
