using System.Globalization;
using System.Threading;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2
{
	abstract class CargoIMPPhase2InterchangeSender : BaseInterchangeSender
	{
		#region New

		protected CargoIMPPhase2InterchangeSender(ILogger logger)
			: base(new ServiceTaskLoggingInformation(logger))
		{
			this.Logger = logger;
		}

		protected new readonly ILogger Logger;

		public static CargoIMPPhase2InterchangeSender New(ILogger logger)
		{
			CargoIMPPhase2InterchangeSender result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(logger);
			}
			else
			{
				result = new TraxonCargoIMPPhase2InterchangeSender(logger);
			}

			return result;
		}

		protected delegate CargoIMPPhase2InterchangeSender NewDelegate(ILogger logger);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Implementation

		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			SendOutboundInterchanges(ApplicationCodeList.Codes.CargoIMPPhase2, token);
		}

		protected override bool SendInt(EDIInterchange interchange)
		{
			if (interchange.ContainedMessages.Count > 0 &&
					interchange.EI_ApplicationCode == ApplicationCodeList.Codes.CargoIMPPhase2)
			{
				return SendInterchange(interchange);
			}
			else
			{
				Logger.Log(LogType.Warning, "Only Cargo2000 Phase 2 Interchanges containing messages can be sent using this sender.");
				return false;
			}
		}

		protected abstract bool SendInterchange(EDIInterchange interchange);

		protected override void OnInterchangeSendFailed(EDIInterchange interchange)
		{
			bool failed = false;

			if (interchange.EI_RetryCount + 1 >= 10)
			{
				Logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Interchange #{0} has exceeded its maximum resend count.", interchange.EI_InterchangeNum));
				failed = true;
			}
			else if (interchange.ContainedMessages.Count == 0 || interchange.EI_ApplicationCode != ApplicationCodeList.Codes.CargoIMPPhase2)
			{
				Logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Interchange #{0} is malformed and cannot be sent.", interchange.EI_InterchangeNum));
				failed = true;
			}

			if (failed)
			{
				interchange.EI_Status = EDIInterchange.Status.Failed;
				foreach (EDIMessage message in interchange.ContainedMessages)
				{
					message.EM_Status = EDIInterchange.Status.Failed;
				}
			}
			else
			{
				interchange.EI_RetryCount++;
			}
		}

		protected override bool SendMoreInterchanges()
		{
			return false;
		}

		protected void LogMessageOutcome(EDIInterchange interchange, bool interchangeSucceeded)
		{
			if (interchangeSucceeded)
			{
				interchange.EI_Status = EDIInterchange.Status.Sent;
				Logger.Log(LogType.Information, "Interchange #" + interchange.EI_InterchangeNum + " successfully sent.");
			}
			else
			{
				Logger.Log(LogType.Information, "Interchange #" + interchange.EI_InterchangeNum + " failed.");
			}
		}

		#endregion
	}
}
