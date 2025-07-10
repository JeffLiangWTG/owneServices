using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.InBond.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

[assembly: CBPMessageProcessorProvider(typeof(Enterprise.Customs.US.InBond.Messaging.MessageProcessor.InBondXTMessageProcessor))]
namespace Enterprise.Customs.US.InBond.Messaging.MessageProcessor
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondResponse)]
	public class InBondXTMessageProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			var linkedObject = OriginalMessageLinker.Link<BusinessObject>(Message);
			var header = linkedObject as IInBondQXHeader;

			var html = new StringBuilder();
			HtmlTableCreator qx10Table = null;
			HtmlTableCreator qx20Table = null;
			HtmlTableCreator lastQX30Table = null;

			var emailReport = new HtmlTableCreator(new string[] { "Code", "Description" });
			var isFailed = false;

			foreach (MessageBlock block in messageBlocks)
			{
				var qx10 = block as AIRQX10;
				if (qx10 != null)
				{
					qx10Table = new HtmlTableCreator(new string[] { "Column", "Value" });
					qx10Table.WriteRow("Action code", qx10.ActionCode);
					qx10Table.WriteRow("Entry Type", qx10.InbondEntryType);
					qx10Table.WriteRow("InBond Number", qx10.InbondNumber);
					qx10Table.WriteRow("Carrier code", qx10.CarrierCode);
					qx10Table.WriteRow("U.S Port of Destination", qx10.USPortOfDestination);
					qx10Table.WriteRow("Foreign Destination", qx10.ForeignDestination);
					qx10Table.WriteRow("Value", qx10.Value);
					qx10Table.WriteRow("InBond Carrier ID", qx10.InbondCarrierID);
					qx10Table.WriteRow("Foreign Entry Number", qx10.ForeignEntryNumber);
				}
				else
				{
					var qx20 = block as AIRQX20;
					if (qx20 != null)
					{
						qx20Table = new HtmlTableCreator(new string[] { "Column", "Value" });
						qx20Table.WriteRow("Carrier Code", qx20.CarrierCode);
						qx20Table.WriteRow("Flight Number", qx20.VoyageTripNumberFlightNumber);
						qx20Table.WriteRow("District/Port of Importing Conveyance Arrival", qx20.DistrictPortOfImportingConveyanceArrival);
						qx20Table.WriteRow("Estimated Date of Arrival", qx20.EstimatedDateOfArrival);
					}
					else
					{
						var qx30 = block as AIRQX30;
						if (qx30 != null)
						{
							lastQX30Table = new HtmlTableCreator(new string[] { "Column", "Value" });
							lastQX30Table.WriteRow("Master Bill Number", qx30.MasterBillNumber);
							lastQX30Table.WriteRow("House Bill Number", qx30.HouseBillNumber);
							lastQX30Table.WriteRow("Previous In-bond Number", qx30.PreviousInbondNumber);
						}
						else
						{
							var xt95 = block as AIRXT95;
							if (xt95 != null)
							{
								isFailed = xt95.NarrativeMessageTypeCode == InBondAcceptanceRejectionList.Codes.Rejected;
								emailReport.WriteRow(xt95.NarrativeMessageIdentifier, xt95.NarrativeMessage);
							}
						}
					}
				}
			}

			html.Append(emailReport.ToHtml());
			html.Append("<br/>");

			if (qx10Table != null)
			{
				html.Append(qx10Table.ToHtml());
				html.Append("<br/>");
			}

			if (qx20Table != null)
			{
				html.Append(qx20Table.ToHtml());
				html.Append("<br/>");
			}

			if (lastQX30Table != null)
			{
				html.Append(lastQX30Table.ToHtml());
				html.Append("<br/>");
			}

			string jobNumber = "Unknown";
			var uri = "";
			GlbBranch branch = null;

			if (header != null)
			{
				var messageStatus = isFailed ? ABIResponseStatus.Rejected : ABIResponseStatus.Cleared;
				new InBondMessageStatusCalculator(header).CalculateStatus(Message, messageStatus);

				var pendingMessageProcessingResult = new InBondPendingMessageManager().Process(header, messageStatus);
				if (!string.IsNullOrEmpty(pendingMessageProcessingResult))
				{
					html.Append(pendingMessageProcessingResult);
				}

				jobNumber = header.JobNumber;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(header);
				branch = header.Branch;
			}

			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Air In-bond Departure", html.ToString(), isFailed, branch, linkedObject);
		}
	}
}
