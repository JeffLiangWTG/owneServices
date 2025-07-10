using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public abstract class NCTSMessageProcessor : TRBranchCustomsApplicationTypeMessageProcessor<NCTSMessage>
	{
		protected NCTSMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}
		protected HtmlTableCreator tableCreator;

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.TRCustoms;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => linkedObject is Integration.Customs.TR.ICusInBondHeader nctsHeader ? nctsHeader.BH_GB : ZGuid.Empty;

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, NCTSMessage message, bool isSuccess)
		{
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				htmlBody.Append(Res.GetString("CF678562-3885-47AF-9824-AEF58DCA17EE", "NCTS Message for job {0} has been cleared. For details please follow the Link to the Manifest", messageAttacheeBO.JobReference));
			}
			else
			{
				htmlBody.Append(Res.GetString("FDA5DA60-1147-4B8F-87C8-7C9A9C3399D2", "NCTS Message for job {0} has been rejected. For details please follow the Link to the Manifest", messageAttacheeBO.JobReference));
			}
			htmlBody.Append("<br /><br />");
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, NCTSMessage message) => Res.GetString("F9B20377-8E90-4513-AC94-75F83F405FB1", "NCTS Status Message");
	}
}
