using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace connect.Service.ibm.cm.filter
{
    public class HangFireCustomAutoRetryJobFilterAttribute : JobFilterAttribute, IElectStateFilter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HangFireCustomAutoRetryJobFilterAttribute));
        private const int DefaultRetryAttempts = 10;
        private int _attempts;
        public HangFireCustomAutoRetryJobFilterAttribute()
        {
            Attempts = DefaultRetryAttempts;
            LogEvents = true;
            OnAttemptsExceeded = AttemptsExceededAction.Fail;
        }
        public int Attempts
        {
            get { return _attempts; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Attempts value must be equal or greater than zero.");
                }
                _attempts = value;
            }
        }
        public AttemptsExceededAction OnAttemptsExceeded { get; set; }
        public bool LogEvents { get; set; }
        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState == null)
            {
                // This filter accepts only failed job state.
                return;
            }
            var retryAttempt = context.GetJobParameter<int>("RetryCount") + 1;


            if (retryAttempt <= Attempts)
            {
                switch (context.Job.Method.Name)
                {
                    case "test":
                        break;
                }
                ScheduleAgainLater(context, retryAttempt, failedState);
            }
            else if (retryAttempt > Attempts && OnAttemptsExceeded == AttemptsExceededAction.Delete)
            {
                TransitionToDeleted(context, failedState);
            }
            else
            {
                if (LogEvents)
                {
                    Logger.ErrorFormat(
                    "Failed to process the job '{0}': an exception occurred.",
                    failedState.Exception,
                    context.JobId);
                }
            }
        }
        /// <summary>
        /// Schedules the job to run again later. See <see cref="SecondsToDelay"/>.
        /// </summary>
        /// <param name="context">The state context.</param>
        /// <param name="retryAttempt">The count of retry attempts made so far.</param>
        /// <param name="failedState">Object which contains details about the current failed state.</param>
        private void ScheduleAgainLater(ElectStateContext context, int retryAttempt, FailedState failedState)
        {
            var delay = TimeSpan.FromSeconds(SecondsToDelay(retryAttempt));
            context.SetJobParameter("RetryCount", retryAttempt);
            // If attempt number is less than max attempts, we should
            // schedule the job to run again later.
            context.CandidateState = new ScheduledState(delay)
            {
                Reason = String.Format("Retry attempt {0} of {1}", retryAttempt, Attempts)
            };
            if (LogEvents)
            {
                Logger.ErrorFormat(
                "Failed to process the job '{0}': an exception occurred. Retry attempt {1} of {2} will be performed in {3}.",
                failedState.Exception,
                context.JobId,
                retryAttempt,
                Attempts,
                delay);
            }
        }
        /// <summary>
        /// Transition the candidate state to the deleted state.
        /// </summary>
        /// <param name="context">The state context.</param>
        /// <param name="failedState">Object which contains details about the current failed state.</param>
        private void TransitionToDeleted(ElectStateContext context, FailedState failedState)
        {
            context.CandidateState = new DeletedState
            {
                Reason = string.Format("Automatic deletion after retry count exceeded {0}", Attempts)
            };
            if (LogEvents)
            {
                Logger.ErrorFormat(
                "Failed to process the job '{0}': an exception occured. Job was automatically deleted because the retry attempt count exceeded {1}",
                failedState.Exception,
                context.JobId,
                Attempts);
            }
        }
        // delayed_job uses the same basic formula
        private static int SecondsToDelay(long retryCount)
        {
            var random = new Random();
            double pow = 4;
            if (retryCount > 5)
            {
                switch (retryCount)
                {
                    case 6:
                        retryCount = 8;
                        break;
                    case 7:
                        retryCount = 9;
                        break;
                    case 8:
                        retryCount = 11;
                        break;
                    case 9:
                        retryCount = 12; // 
                        break;
                    case 10:
                        retryCount = 19; // 36 hrs
                        break;


                }
                retryCount = retryCount + retryCount;
            }


            return (int)Math.Round(
            Math.Pow(retryCount - 1, pow) + 15 + (random.Next(30) * (retryCount)));
        }
    }

}