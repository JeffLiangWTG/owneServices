using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ImportOrgAddressMessageDataFromLastMessageTest : TestCaseWithFactory
	{
		public void TestImportFromMessage()
		{
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var organisation = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(organisation);
			organisation.MainAddress.OA_Address1 = "TEST MAILING ADDRESS";
			organisation.MainAddress.OA_City = "CHICAGO";
			organisation.MainAddress.OA_State = "IL";
			organisation.MainAddress.OA_PostCode = "60091";
			organisation.MainAddress.OA_RN_NKCountryCode = "US";
			var address2 = organisation.Addresses.AddNew();
			address2.OA_Address1 = "TEST PHYSICAL ADDRESS";
			address2.OA_Address2 = "SECONDARY ADDRESS";
			address2.OA_City = "LONDON";
			address2.OA_PostCode = "1234";
			address2.OA_RN_NKCountryCode = "GB";

			var piiContact = organisation.Contacts.AddNew();
			piiContact.OC_ContactName = "LONGPERSONIDENTIFYFIRSTNAME LONGPERSONIDENTIFYLASTNAME";
			piiContact.OC_Title = "PRESIDENT";
			piiContact.OC_Phone = "+6423456001";
			var individualContact = organisation.Contacts.AddNew();
			individualContact.OC_ContactName = "LONGINDIVIDUALFIRSTNAME LONGINDIVIDUALLASTNAME";
			individualContact.OC_Title = "MANAGER";
			individualContact.OC_Phone = "+6423456011";
			var brokerContact = organisation.Contacts.AddNew();
			brokerContact.OC_ContactName = "THIS IS A LONG BROKER NAME WHICH LENGTH EXCEED";
			brokerContact.OC_Title = "BROKER";
			brokerContact.OC_Phone = "+6423456012";

			var outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.AddNewCBPFormCBPF5106DatatotheImporterFile;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_148588";
			outgoingMessage.EM_MessageText = "B  8888XJ5TP                                               <<MSGNO PLACEHOLDER>>T1A            THIS IS A LONG IMPORTER NAME WITTEST MAILING ADDRESS            CTADIVA&S FURNISHING BEDDING DIVISION                                            T2                                            CHICAGO              IL60091    UST3THIS IS A LONG IMPORTER NAME W                                                TNIN1ITH LENGTH GREATER THAN 32                                                 TBTEST PHYSICAL ADDRESS                                                         TCLONDON               FN1234     GB                                            TD2X   XOTHER DESC     CTPATISA            +6423456789    001    X X            TE1               8               I SELL PRODUCTS                               TFTHISISAVERYLONGEMAILADDRESS@ABHTTPS://WWW.ABCDEFGHIJKLMNOPQR                  TNCE1C.COM                                                                      TNCW1ST12345.COM                                                                TG123456987654321XJ52018AKUS123ABC321CBA                                        THTHIS IS A LONG BANK NAME THAT 1234567890ATHIS IS A LONG CITY NAME THAT ABCA   TNBN1EXCEED 30 CHARACTERS                                                       TNBC1LENGTH EXCEED 30 CHARACTERS                                                TI01LONGPERSONIDENTIFYLASTNAME, LOPRESIDENT             123125436               TNCN1NGPERSONIDENTIFYFIRSTNAME                                                  TJ01E512031578   12312018CN16423456001     201   THIS IS A LONG PERSON IDENTIFY TNCE2EMAIL                                                                      TK011THIS IS A LONG RELATED BUSINES321-12-8542                                  TNNE1S NAME                                                                     TK022CURRENT BUSINESS              181101-02342                                 TLXLONGINDIVIDUALLASTNAME, LONGINMANAGER                                        TNIN2DIVIDUALFIRSTNAME                                                          TMCargoWise Support             23456011                                        Y  8888XJ5TP";
			wrapper.Messages.Add(outgoingMessage);

			var messageData = new OrgAddressMessageData(wrapper);
			messageData.US_AcknowledgeAndSign = true;
			var message = new ImporterADDBuilder(messageData).Generate();
			AssertMultilineASCIIEquals(outgoingMessage.EM_FormattedMessageText, message.EM_FormattedMessageText);
		}
	}
}
