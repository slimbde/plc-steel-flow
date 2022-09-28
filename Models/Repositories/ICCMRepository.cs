using System;
using static PLCHandler.Delegates;


namespace PLCSteelFlow.Models.Repositories
{
    /// <summary>
    /// Handles Oracle database
    /// </summary>
    public interface ICCMRepository
    {
        /// <summary>
        /// The delegate for repo to complain through
        /// </summary>
        event LogHandler OnStatusLog;



        /// <summary>
        /// Updates flow value in the database
        /// </summary>
        void UpdateFlow(double flow);
    }
}
