using System.Text;
using CargoWise.Application;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse)]
	[TopLevel(typeof(QTAU9))]
	public class NoVisaRequirementProcessor : ACSABIProcessor
	{
		public override void Process()
		{
			var jobDeclarationLinked = OriginalMessageLinker.Link<JobDeclaration>(Message);

			var html = new StringBuilder();
			var u9 = GetFirstMessageBlock<QTAU9>();
			if (u9 != null)
			{
				html.Append("<b>No Visa requirement for the Tariff Number/Country of Origin/Secondary Tariff Number</b>");
				html.Append("<br />");
				html.Append("<br />");

				var htmlTable = new HtmlTableCreator(new string[] { "Column", "Description" });
				htmlTable.WriteRow("Tariff Number/Category Number", u9.TariffNumberOrTextileCategoryNumber);
				htmlTable.WriteRow("Country of Origin", u9.CountryOfOrigin);
				htmlTable.WriteRow("Secondary Tariff Number", u9.SecondTariffNumber.IsEmpty ? "Not Applicable" : u9.SecondTariffNumber.ToString());
				htmlTable.WriteRow("Remarks", u9.ErrorMessage);

				var uri = "";
				var jobNumber = "Unknown";
				if (jobDeclarationLinked != null)
				{
					jobNumber = jobDeclarationLinked.JobNumber;
					uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(jobDeclarationLinked);
				}

				html.Append(htmlTable.ToHtml());
				html.Append("<br />");

				var originalMsg = Message.OriginalMessage;
				if (originalMsg != null)
				{
					if (jobDeclarationLinked == null)
					{
						jobNumber = originalMsg.EM_MessageNum;
					}
					html.Append("<b>Query was sent on: </b>" + originalMsg.EM_SystemCreateTimeUtc.ToString());
				}

				var branch = jobDeclarationLinked != null ? jobDeclarationLinked.Branch : null;
				GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Query for Visa Requirement", html.ToString(), false, branch, jobDeclarationLinked);
			}
		}
	}
}
