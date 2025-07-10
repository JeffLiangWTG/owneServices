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

namespace Enterprise.Customs.US.InBond.Business
{
	public class OperationalActionBulkDepartureAddPerMovementMessageSender : OperationalActionBulkMessageSender<USInBondMoveHeader>
	{
		public OperationalActionBulkDepartureAddPerMovementMessageSender(USInBondMoveHeader movementHeader)
			: base(movementHeader)
		{
		}

		protected override string MessageTypeCore => "Departure Add";

		protected override LogControllerLink JobLink => job.GetInBondMovementIdLink();

		protected override SaveResult OperationalActionSendMessageCore(bool sendWithMessageErrors, IOperationalActionSectionLog log)
		{
			var result = SaveResult.Fail;

			if (job.Header is CusInBondHeader header)
			{
				var messageType = header.IsAir ? InBondMessageType.AirInBondAdd : InBondMessageType.DepartureAdd;
				header.RecalculateValidationModesOnHeader(messageType);
				header.RunPreSaveValidationWithFetchHints();

				if (header.HasErrors)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "In-Bond Movement {0}: Please fix following errors before sending Departure Add message.", new object[] { JobLink });
					log.Notify(OperationalActionLogErrorLevel.Warning, header.GetErrors().ToUniqueMessageListString());
					return result;
				}

				if (!header.HasMessageErrors || sendWithMessageErrors)
				{
					var sendingMovementObject = new InBondMessageSendingMovementObject(job, messageType, new SendsMessagesToCustomsShutterUpperer());
					if (sendingMovementObject.SendingObjects.Count > 0)
					{
						if (header.LockSendCustomsMessageMutex())
						{
							try
							{
								foreach (InBondMessageSendingObject sendingObject in sendingMovementObject.SendingObjects)
								{
									sendingObject.US_ShouldSend = !sendingObject.moveHeader.HasClearDepartureAdd;
								}

								result = sendingMovementObject.SendData() ? SaveResult.Success : SaveResult.Fail;
							}
							finally
							{
								header.UnlockSendCustomsMessageMutex();
							}
						}
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "In-Bond Movement {0}: This In-Bond Movement is not available for sending a 'Departure Add' message to Customs. It is possibly either pending Customs response or it has already been accepted by Customs.", new object[] { JobLink });
					}
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "In-Bond Movement {0}: Cannot send the Departure Add message because job has message errors.", new object[] { JobLink });
				}
			}

			return result;
		}
	}
}
