using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.MAFeBACCa;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	internal class IPIMessageProcessor : DeclarationMessageProcessor<IM1Response>
	{
		public IPIMessageProcessor(LoggingInformation logger)
			: base(logger, "Import")
		{
		}

		#region Overrides

		protected override ZGuid DocumentParentPK => Response.ManifestConsol?.Identifier ?? ZGuid.Empty;

		protected override ZString DocumentParentType => Response.ManifestConsol?.DocumentParentType ?? ZString.Empty;

		protected override bool IsWarningAboutTotalAmountInconsistencyRequired
		{
			get { return true; }
		}

		protected override ZString ApplicationCodeForGetUserToNotify => EDIInterchange.ApplicationCodes.NewZealandCustoms;

		protected override void ProcessCore()
		{
			Report.AddLine(Response.MessageTypeDescription);
			Report.AddSeparator();
			Report.AddLine("Job Number", Response.JobID);
			Report.AddLine("Master Bill", Consol.JK_MasterBillNum);
			Report.AddLine("Entry Type", "TSW IPI " + Consol.JK_EntryType);
			Report.AddLine("Entry Number", Response.DeclarationID);
			Report.AddLine("Message No", Response.MessageNumber);
			Report.AddLine();
			OutputResponseStatus();

			ProcessDeliveryInstruction();
			ProcessCustomsInstructions();

			if (!Response.ErrorsWithPointers.IsNullOrEmpty())
			{
				Report.AddHeader("Message Errors");
				foreach (var error in Response.ErrorsWithPointers)
				{
					Report.AddLine("**Error** in {" + error.Pointer + "}:-");
					Report.AddLine("  " + error.Value);
				}
			}
		}

		protected override void WriteEntryStatus()
		{
			var consolMafMessaging = GetMAFMessaging(Consol);
			if (consolMafMessaging != null)
			{
				consolMafMessaging.ZX_ConsignmentNumber = Response.DeclarationID;
				ZString currentStatus = (consolMafMessaging.ZX_MessagingStatus.IsEmpty || consolMafMessaging.ZX_MessagingStatus == TSWEntryStatusList.Codes.STC) ? ConsolIPIStatusList.Codes.PP : consolMafMessaging.ZX_MessagingStatus.ToString();
				if (Response.IsTSWAcknowledgementResponse && currentStatus.Length == 3)
				{
					currentStatus = ConsolIPIStatusList.Codes.PP;
				}

				if (Response.IsTSWResponse && consolMafMessaging.ZX_ConsignmentNumber.IsEmpty)
				{
					consolMafMessaging.ZX_MessagingStatus = ConsolIPIStatusList.Codes.EntryRejected;
				}
				else
				{
					var mpiFoodStatus = currentStatus.SubstringSafe(1, 1);
					var mpiBioStatus = currentStatus.SubstringSafe(0, 1);
					if (Response.IsMPIFoodResponse)
					{
						mpiFoodStatus = Response.EnterpriseStatus;
					}
					else if (Response.IsMPIBiosecurityResponse)
					{
						mpiBioStatus = Response.EnterpriseStatus;
					}

					consolMafMessaging.ZX_MessagingStatus = mpiBioStatus + mpiFoodStatus;
				}
			}
		}

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendAcknowledgements.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendImpedimentsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendImpediments.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendErrors.Value; }
		}

		protected override DeclarationResponse PreviousResponsesWithSameMessageType
		{
			get
			{
				if (fPreviousResponsesWithSameMessageType == null)
				{
					var responseMessages = Consol.Messages.OfType<TSWMessage>()
											.Where(message => message.EM_ReceiveTransmit == Enterprise.Messaging.Business.EDIInterchange.Direction.Receive && message != Response.IncomingTSWMessage)
											.OrderByDescending(message => message.EM_SystemCreateTimeUtc);

					var lastestMessageWithSameSubType = responseMessages.FirstOrDefault(message => message.EM_MessageSubType == Response.IncomingTSWMessage.EM_MessageSubType);
					BaseTSWResponse baseTSWResponse = null;

					if (BaseTSWResponse.TryParse(lastestMessageWithSameSubType, out baseTSWResponse))
					{
						fPreviousResponsesWithSameMessageType = new IM1Response(baseTSWResponse);
					}
				}

				return fPreviousResponsesWithSameMessageType;
			}
		}
		DeclarationResponse fPreviousResponsesWithSameMessageType;

		#endregion // Overrides

		public MAFMessagingBO GetMAFMessaging(ForwardingConsol consol)
		{
			return new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(consol));
		}
	}
}
