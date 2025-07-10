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
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	public class PGACorrectionMessageProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.PGADataCorrection;

			var emailBody = new StringBuilder();
			var entryHeaderLinked = OriginalMessageLinker.Link<CusEntryHeader>(Message);
			var messageTypeCode = ProcessCore(emailBody);
			var pgaCorrectionStatus = GetPGACorrectionStatusCode(messageTypeCode);
			var isFailure = pgaCorrectionStatus == PGACorrectionStatusList.Codes.ErrorPGADataCorrection;

			var jobNumber = "Unknown";
			var uri = "";
			var branch = GlbBranch.CurrentBranch;

			if (entryHeaderLinked != null)
			{
				jobNumber = entryHeaderLinked.Declaration.JobNumber;
				uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entryHeaderLinked.Declaration);
				branch = entryHeaderLinked.Declaration.Branch;
				entryHeaderLinked.Declaration.US_PGACorrectionStatus = pgaCorrectionStatus;

				var pgaLinesDataCorrectionManager = new PGALinesDataCorrectionManager(Message, entryHeaderLinked.Declaration);
				pgaLinesDataCorrectionManager.UpdatePGALines(isFailure, isPGADataCorrectionMessage: true);
			}

			emailBody.Append("<br />");

			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "PGA Correction", emailBody.ToString(), isFailure, branch, entryHeaderLinked);
		}

		protected string ProcessCore(StringBuilder emailBody)
		{
			var messageTypeCode = ZString.Empty;
			var ca90Table = new HtmlTableCreator(new string[] { "Entry Number", "Entry Line #", "Tariff", "Code", "Narrative Message" });
			var entryFiler = ZString.Empty;
			var entryNumber = ZString.Empty;
			var entryLineNo = ZInt.Zero;
			var tariffNo = ZString.Empty;

			foreach (MessageBlock block in messageBlocks)
			{
				if (block.MandatoryCharacters == "CA10")
				{
					var ca10 = block as APDCCA10;
					entryFiler = ca10.EntryFilerCode;
					entryNumber = ca10.EntryNumber;
				}
				else if (block.MandatoryCharacters == "CA40")
				{
					var ca40 = block as APDCCA40;
					entryLineNo = ca40.LineItemIdentifier;
				}
				else if (block.MandatoryCharacters == "CA60")
				{
					var ca60 = block as APDCCA60;
					tariffNo = ca60.TariffNumber;
				}
				else if (block.MandatoryCharacters == "CA61")
				{
					var ca61 = block as APDCCA61;
					tariffNo = ca61.CurrentHTSCode;
				}
				else if (block.MandatoryCharacters == "CA90")
				{
					var ca90 = block as APDCCA90;
					messageTypeCode = ca90.NarrativeMessageTypeCode;
					var entryNoWithEntryFilerCode = CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(entryFiler, entryNumber);
					var narrativeMessage = (!ca90.NarrativeMessageIdentifier.IsEmpty ? ca90.NarrativeMessageIdentifier + " - " : "") + ca90.NarrativeMessage;
					ca90Table.WriteRow(entryNoWithEntryFilerCode, entryLineNo.ToString(), tariffNo, GetDescrFromStatusCode(messageTypeCode), narrativeMessage);
				}
			}

			emailBody.Append("<br />");
			emailBody.Append(ca90Table.ToHtml());

			return messageTypeCode;
		}

		static string GetDescrFromStatusCode(string code)
		{
			switch (code)
			{
				case "01":
					return "01 - Message Rejected";
				case "02":
					return "02 - Message Accepted";
				case "03":
					return "03 - Message Accepted with Warning(s)";
				case "04":
					return "04 - Message Referred to Human Review";
				case "11":
					return "11 - Record Rejected";
				case "13":
					return "13 - Record Accepted with a Warning";
				default:
					return code;
			}
		}

		static string GetPGACorrectionStatusCode(string messageTypeCode)
		{
			switch (messageTypeCode)
			{
				case "01":
				case "11":
					return PGACorrectionStatusList.Codes.ErrorPGADataCorrection;
				case "02":
					return PGACorrectionStatusList.Codes.ClearPGADataCorrection;
				case "03":
				case "13":
					return PGACorrectionStatusList.Codes.PGADataCorrectionAcceptedwithWarning;
				case "04":
					return PGACorrectionStatusList.Codes.PGADataCorrectionReferredtoHumanReview;
				default:
					return string.Empty;
			}
		}
	}
}
