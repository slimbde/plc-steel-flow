using PLCSteelFlow.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static PLCHandler.Delegates;


namespace PLCSteelFlow.Models
{
    public class CcmFlowHandler
    {
        public LogHandler OnStatusLog;


        ICCMRepository repo;
        bool enableTrace;


        public CcmFlowHandler(LogHandler logger)
        {
            this.OnStatusLog += logger;
            this.repo = new CCMRepository();
            repo.OnStatusLog += logger;
            this.enableTrace = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("EnableTrace"));
        }


        public void Update(Dictionary<string, string> entries)
        {
            try
            {
                double flow = Convert.ToDouble(entries[$"pmFlow"].Replace("\0", "").Trim()) * 60;
                repo.UpdateFlow(flow);

                if (enableTrace) OnStatusLog?.Invoke($"[CcmFlowHandler]: Update Flow = {flow}", EventLogEntryType.Information);
            }
            catch (Exception ex) { OnStatusLog?.Invoke($"[CcmFlowHandler Update Exception]: {ex.Message}", EventLogEntryType.Error); }
        }
    }
}
