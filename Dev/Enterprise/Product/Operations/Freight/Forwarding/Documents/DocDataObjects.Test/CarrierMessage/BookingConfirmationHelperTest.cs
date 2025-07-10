using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class BookingConfirmationHelperTest : TestCaseWithFactory
	{
		public void TestNullConsol()
		{
			AssertNoExceptionThrown("no exception thrown when calling method on null consol",
				() => ((ForwardingConsol)null).GetBookingConfirmationUniversalXml());
		}

		public void TestGetUSXml()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.DataLinkedCode;
				log.SL_Parent = consol.PK;
				log.SL_Table = JobConsolSchema.Constants.TableName;
			}

			var message = Factory.CreateBookingConfirmationMessage();
			message.AddUniversalDataLink(log);

			Factory.Save();

			var usxml = consol.GetBookingConfirmationUniversalXml();

			AssertNotNull("found booking confirmation USXml", usxml);
		}

		public void TestMessageWithBadUSXml()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.DataLinkedCode;
				log.SL_Parent = consol.PK;
				log.SL_Table = JobConsolSchema.Constants.TableName;
			}

			const string badUSxml = "<I'm bad xml";

			var interchange = Factory.New<IXmlEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_From = "eHubsky";
			interchange.EI_To = Env.CurrentCompany.GetLicenceCode();
			interchange.EI_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
			interchange.EI_InterchangeType = EDIMessageTypeList.Codes.XMS;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_BodyText = badUSxml;

			var message = interchange.AddNeweHubMessage();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Linked;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_MessageText = badUSxml;

			message.AddUniversalDataLink(log);

			Factory.Save();

			var usxml = consol.GetBookingConfirmationUniversalXml();
			AssertNull("no booking confirmation USXml was found", usxml);
		}
	}
}
