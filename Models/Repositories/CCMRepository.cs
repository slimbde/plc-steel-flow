using Dapper;
using System;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using static PLCHandler.Delegates;
using System.Collections.Generic;
using System.Linq;
using System.Text;


#if RELEASE9I
using Oracle.DataAccess.Client;
#else
using Oracle.ManagedDataAccess.Client;
#endif


namespace PLCSteelFlow.Models.Repositories
{

    public class CCMRepository : ICCMRepository
    {
        static IDictionary<string, int> ccmStrandsNum = new Dictionary<string, int>
        {
            { "2", 1 },
            { "3", 6 },
            { "4", 6 },
            { "5", 5 },
        };

        string conString = ConfigurationManager.ConnectionStrings["oracle"].ConnectionString;
        int strandsNum = ccmStrandsNum[ConfigurationManager.AppSettings.Get("CcmNo").ToString()];
        public event LogHandler OnStatusLog;



        public void UpdateFlow(double flow)
        {
            try
            {
                using (IDbConnection db = new OracleConnection(conString))
                {
                    IEnumerable<dynamic> info = db.Query(@"SELECT 
                                                              STRAND_NO
                                                              ,IS_STOPPED
                                                            FROM REP_CCM_STRANDS_STATE");

                    var statuses = info.ToDictionary(rec => rec.STRAND_NO, rec => rec.IS_STOPPED.ToString());

                    for (decimal s = 1; s <= strandsNum; ++s)
                    {
                        StringBuilder sb = new StringBuilder("UPDATE RTDB_CCM_FLOW SET FLOW=:flow");
                        if (statuses[s] == "0") sb.Append(", LAST_UPDATE=SYSDATE");
                        sb.Append($" WHERE STRAND_NO={s}");

                        db.Execute(sb.ToString(), new { flow });
                    }
                }
            }
            catch (Exception ex) { OnStatusLog?.Invoke($"[CCMRepository UpdateFlow]: {ex.Message}", EventLogEntryType.Error); }
        }
    }
}
