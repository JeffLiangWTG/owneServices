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

namespace Enterprise.Customs.US.InBond.Messaging.MessageProcessor
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNoticeResponse)]
	public class InBondYTMessageProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			var linkedObject = OriginalMessageLinker.Link<BusinessObject>(Message);
			var header = linkedObject as IInBondWXHeader;

			var html = new StringBuilder();
			HtmlTableCreator wx10Table = null;
			HtmlTableCreator wx20Table = null;

			var emailReport = new HtmlTableCreator(new string[] { "Code", "Description", "Comments" });
			var isFailed = false;

			foreach (MessageBlock block in messageBlocks)
			{
				var wx10 = block as AIRWX10;
				if (wx10 != null)
				{
					wx10Table = new HtmlTableCreator(new string[] { "Column", "Value" });
					wx10Table.WriteRow("Action code", wx10.ActionCode);
					wx10Table.WriteRow("In-bond Number", wx10.InbondNumber);
					wx10Table.WriteRow("Master Bill Number", wx10.MasterBillNumber);
					wx10Table.WriteRow("House Bill Number", wx10.HouseBillNumber);
				}
				else
				{
					var wx20 = block as AIRWX20;
					if (wx20 != null)
					{
						wx20Table = new HtmlTableCreator(new string[] { "Column", "Value" });
						wx20Table.WriteRow("Date", wx20.Date);
						wx20Table.WriteRow("Time", wx20.Time);
						wx20Table.WriteRow("Port of Arrival, Departure or Export", wx20.PortOfArrivalDepartureOrExport);
						wx20Table.WriteRow("Export MOT", wx20.ExportMOT);
						wx20Table.WriteRow("Importing Carrier Code", wx20.ImportingCarrierCode);
						wx20Table.WriteRow("Flight Number", wx20.FlightNumber);
						wx20Table.WriteRow("Scheduled Arrival Date", wx20.ScheduledArrivalDate);
					}
					else
					{
						var yt95 = block as AIRYT95;
						if (yt95 != null)
						{
							isFailed = yt95.NarrativeMessageTypeCode == InBondAcceptanceRejectionList.Codes.Rejected;
							emailReport.WriteRow(yt95.NarrativeMessageIdentifier, yt95.NarrativeMessage, yt95.NarrativeNotes);
						}
					}
				}
			}

			html.Append(emailReport.ToHtml());
			html.Append("<br/>");

			if (wx10Table != null)
			{
				html.Append(wx10Table.ToHtml());
				html.Append("<br/>");
			}

			if (wx20Table != null)
			{
				html.Append(wx20Table.ToHtml());
				html.Append("<br/>");
			}

			string jobNumber = "Unknown";
			var uri = "";
			GlbBranch branch = null;
			if (header != null)
			{
				jobNumber = header.JobNumber;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(header);
				branch = header.Branch;

				var messageStatus = isFailed ? ABIResponseStatus.Rejected : ABIResponseStatus.Cleared;
				new InBondMessageStatusCalculator(header).CalculateStatus(Message, messageStatus);
			}

			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Air In-bond Arrival/Exportation", html.ToString(), isFailed, branch, linkedObject);
		}
	}
}
