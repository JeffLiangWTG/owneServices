using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	[TopLevel(typeof(QTAU0))]
	public class VisaRequirementProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			QTAU0 u0 = GetFirstMessageBlock<QTAU0>();
			MQEDIMessage originalMessage = Message.OriginalMessage;

			OriginalMessageLinker.Link<JobDeclaration>(Message);

			if (u0 != null)
			{
				HtmlTableCreator htmlTable = new HtmlTableCreator(new string[] { "Column", "Description" });

				if (originalMessage != null)
				{
					IVisaQuery visaQuery = originalMessage;
					htmlTable.WriteRow("Tariff Number", visaQuery.TariffNumber);
					htmlTable.WriteRow("Country of Origin", visaQuery.OriginCountry);

					if (!visaQuery.SecondTariffNumber.IsEmpty)
					{
						htmlTable.WriteRow("Second Tariff Number", visaQuery.SecondTariffNumber);
					}
				}

				if (VisaStatusList.IsOnFile(u0.VisaNumberStatusIndicator))
				{
					htmlTable.WriteRow("Visa Document Year Last Digit", u0.VisaDocumentNumber.Left(1));
					htmlTable.WriteRow("Visa Document Country", u0.VisaDocumentNumber.SubstringSafe(1, 2));
					htmlTable.WriteRow("Visa Document Number Number", u0.VisaDocumentNumber.SubstringSafe(3));
				}

				string statusDesc = new VisaStatusList().GetDescriptionFromCode(u0.VisaNumberStatusIndicator);
				htmlTable.WriteRow("Visa Number Status", statusDesc ?? u0.VisaNumberStatusIndicator.ToString());

				ZString date = u0.VisaDocumentStatusDate.ToString();

				if (date.Length == 8 && VisaStatusList.IsStatusDateLastStateUpdateChangeDate(u0.VisaNumberStatusIndicator))
				{
					htmlTable.WriteRow("Date Visa Status Changed (YYYYMMDD)", date);
				}

				EmailDef email;
				GlbBranch branch = originalMessage != null ? originalMessage.Branch : GlbBranch.CurrentBranch;
				GenerateHtmlEmail(
					"Query for Visa Requirement",
					"Visa requirement for the tariff number",
					"",
					htmlTable.ToHtml(),
					"",
					out email, branch);

				SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, branch, false);
			}
		}
	}
}
