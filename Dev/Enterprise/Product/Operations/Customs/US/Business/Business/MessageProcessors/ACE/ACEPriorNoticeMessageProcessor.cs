using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	public class ACEPriorNoticeMessageProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.FDAPriorNoticeResponse;

			var emailBody = new StringBuilder();
			emailBody.Append("<b>ACE Prior Notice submission result</b>");
			emailBody.Append("<br />");
			emailBody.Append("<br />");

			var isFailure = ProcessCore(emailBody);

			var jobNumber = "Unknown";
			var uri = "";
			var branch = GlbBranch.CurrentBranch;

			var jobDeclarationLinked = OriginalMessageLinker.Link<JobDeclaration>(Message);
			if (jobDeclarationLinked != null)
			{
				jobNumber = jobDeclarationLinked.JobNumber;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(jobDeclarationLinked);
				branch = jobDeclarationLinked.Branch;
			}

			emailBody.Append("<br />");

			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "ACE Prior Notice", emailBody.ToString(), isFailure, branch, jobDeclarationLinked);
		}

		protected bool ProcessCore(StringBuilder emailBody)
		{
			var isFailure = false;
			var p90Table = new HtmlTableCreator(new string[] { "Code", "Message Identifier", "Message", "Envelope Number" });

			foreach (MessageBlock block in messageBlocks)
			{
				if (block.MandatoryCharacters == "PE10")
				{
					var p10 = (SEPAPE10)block;
					var headerTable = new HtmlTableCreator(new string[] { "Action Code", "Reference Qualifier", "Filer/Issuer code", "Number", "Carrier SCAC/IATA", "Entry Type", "Mode of Transportation" });
					headerTable.WriteRow(GetActionCodeDescription(p10.ActionCode), GetReferenceQualifierDescription(p10.ReferenceQualifierCode), p10.FilerOrIssuerCodeForReferenceIdentifier, p10.ReferenceIdentifierNumber, p10.Carrier, p10.EntryType, GetModeOfTransportationDescr(p10.ModeOfTransportationMOTCode));

					emailBody.Append(headerTable.ToHtml());
				}
				else if (block.MandatoryCharacters == "PE90")
				{
					var p90 = (SEPAPE90)block;

					isFailure |= p90.NarrativeMessageTypeCode == MessageRejected || p90.NarrativeMessageTypeCode == RecordRejected;
					p90Table.WriteRow(GetNarrativeCodeDescription(p90.NarrativeMessageTypeCode), p90.NarrativeMessageIdentifier, p90.NarrativeMessage, p90.EnvelopeNumber);
				}
			}

			emailBody.Append("<br />");
			emailBody.Append(p90Table.ToHtml());

			return isFailure;
		}
		const string MessageRejected = "01";
		const string RecordRejected = "11";

		ZString GetActionCodeDescription(ZString code)
		{
			switch (code)
			{
				case "A":
					return "Add";
				case "D":
					return "Delete";
				case "R":
					return "Replace";
				default:
					return code;
			}
		}

		ZString GetReferenceQualifierDescription(ZString status)
		{
			switch (status)
			{
				case "ENT":
					return "Entry Number";
				case "BOL":
					return "Bill of Lading Number (Ocean, Rail or Truck)";
				case "AWB":
					return "Air waybill";
				case "FTZ":
					return "FTZ admission number";
				case "INB":
					return "In-bond number";
				default:
					return "";
			}
		}

		ZString GetModeOfTransportationDescr(ZString code)
		{
			var result = code;
			if (!code.IsEmpty)
			{
				ZString description = Factory.GetCachedValue<TransportModeCodes>().GetDescriptionFromCode(code);
				result = !description.IsEmpty ? description + " (" + code + ")" : code.ToString();
			}
			return result;
		}

		ZString GetNarrativeCodeDescription(ZString status)
		{
			switch (status)
			{
				case "01":
					return "Message Rejected";
				case "02":
					return "Message Accepted";
				case "03":
					return "Message Accepted with Warning(s)";
				case "04":
					return "Message Referred to Human Review";
				case "11":
					return "Record Rejected";
				case "13":
					return "Record Accepted with a Warning";
				default:
					return "";
			}
		}
	}
}
