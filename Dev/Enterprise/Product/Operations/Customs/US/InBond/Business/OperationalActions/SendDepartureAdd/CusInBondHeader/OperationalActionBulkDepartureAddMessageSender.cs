using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.OperationalAction;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.OperationalActions
{
	public class OperationalActionBulkDepartureAddMessageSender : OperationalActionBulkMessageSender<CusInBondHeader>
	{
		public OperationalActionBulkDepartureAddMessageSender(CusInBondHeader job)
			: base(job)
		{
		}

		protected override string MessageTypeCore => "Departure Add";

		protected override LogControllerLink JobLink
		{
			get
			{
				return jobLink ?? (jobLink = job.GetInBondHeaderIdLink());
			}
		}
		LogControllerLink jobLink;

		protected override SaveResult OperationalActionSendMessageCore(bool sendWithMessageErrors, IOperationalActionSectionLog log)
		{
			var result = SaveResult.Fail;

			var messageType = job.IsAir ? InBondMessageType.AirInBondAdd : InBondMessageType.DepartureAdd;
			job.RecalculateValidationModesOnHeader(messageType);
			job.RunPreSaveValidationWithFetchHints();

			if (job.HasErrors)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Please fix following errors before sending Departure Add message.", new object[] { JobLink });
				log.Notify(OperationalActionLogErrorLevel.Warning, job.GetErrors().ToUniqueMessageListString());
				return result;
			}

			if (!job.HasMessageErrors || sendWithMessageErrors)
			{
				var sendingHeaderObject = new InBondMessageSendingHeaderObject(job.PK, messageType, new SendsMessagesToCustomsShutterUpperer());
				if (sendingHeaderObject.SendingObjects.Count > 0)
				{
					if (job.LockSendCustomsMessageMutex())
					{
						try
						{
							foreach (InBondMessageSendingObject sendingObject in sendingHeaderObject.SendingObjects)
							{
								sendingObject.US_ShouldSend = !sendingObject.moveHeader.HasClearDepartureAdd;
							}

							result = sendingHeaderObject.SendData() ? SaveResult.Success : SaveResult.Fail;
						}
						finally
						{
							job.UnlockSendCustomsMessageMutex();
						}
					}
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: There is no In-Bond Movement available for sending a 'Departure Add' message to Customs. Possibly all In-Bond Movements are either pending Customs response or have already been accepted by Customs.", new object[] { JobLink });
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cannot send the Departure Add message because job has message errors.", new object[] { JobLink });
			}

			return result;
		}
	}
}
