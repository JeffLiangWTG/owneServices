using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace CargoWise.RefDataRepo.Ent.Client.NudgeServiceTask.Test
{
	public static class NudgeTestHelper
	{
		const string DefaultEDIIntermessageHeader = @"<SenderID>RefDbRepoDataPublishing</SenderID><RecipientID>HYEDAUUAT</RecipientID><InterchangeType>REN</InterchangeType><InterchangeNumber>f688d34b-02e4-4f29-8082-a5c3c2177c4e</InterchangeNumber>";

		public static EDIInterchange CreateEDIInterchange(BusinessObjectFactory factory, string bodyText)
		{
			var ediInterchange = factory.New<EDIInterchange>();
			ediInterchange.EI_ApplicationCode = EDIInterchangeTypeList.Codes.GenericMessageDelivery;
			ediInterchange.EI_From = "CUSTOMS_DATA_REPO_TEST";
			ediInterchange.EI_To = "XXX";
			ediInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			ediInterchange.EI_InterchangeType = ApplicationConfiguration.ServiceTask.Code;
			ediInterchange.EI_HeaderText = DefaultEDIIntermessageHeader;
			ediInterchange.EI_BodyText = bodyText;
			ediInterchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;

			factory.Save();
			return ediInterchange;
		}
	}
}
