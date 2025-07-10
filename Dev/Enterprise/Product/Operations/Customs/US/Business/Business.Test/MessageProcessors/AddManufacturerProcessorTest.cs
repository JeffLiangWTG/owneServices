using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class AddManufacturerProcessorTest : ABIProcessorTest<AddManufacturerProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var generator = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse,
				"$5U00001AUBRETT SHEARER",
				"$6                               AUBRESHE59SYD",
				"$7B29 TEST MODE - FILE NOT UPDATED");

			var processor = new AddManufacturerProcessor();
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.Message = responseMessage;

			foreach (var messageBlock in generator.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Body.Contains("Add Manufacturer File Response (Warning) for Unknown"); }));
			AssertNotNull(sentMail);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		public void TestUpdatingManufacturerID()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var address = organisation.Addresses.AddNew();
			address.OA_Address1 = "Address 1";
			var cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID12322");

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B01       $I                                               ~15000               " +
				"$A" + address.PK.ToString().PadRight(78) +
				"$1A00001AUHAIL MARKETING WORKS                                                  " +
				"$2                              2/2-6 ORION ROAD LANE COVE                      " +
				"$3                                                   LANE COVE                  " +
				"$4                                            2066      AUBRESHE59SYD           " +
				"Y  1101SV9$I00005";
			message.EM_LinkedObject = organisation;

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse;
			message2.EM_MessageNum = "~15000";

			message2.EM_MessageText =
				"B01       $R                                               ~15000               " +
				"$A" + address.PK.ToString().PadRight(78) +
				"$5U00001AUHAIL MARKETING WORKS                                                  " +
				"$6                               AUBRESHE59SYD                                  " +
				"Y  8888XJ5$R00002000000189600                                                   ";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			cusCode.Reload();
			AssertEquals("MID should have been updated", "AUBRESHE59SYD", cusCode.OK_CustomsRegNo);
		}

		public void TestIfAcceptedAddMessageButErrorFoundInResults()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.OH_Code = "ORG";
			var address = organisation.Addresses.AddNew();
			address.OA_Address1 = "Address 1";
			var cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID12322");

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B01       $I                                               ~15000               " +
				"$A" + address.PK.ToString().PadRight(78) +
				"$1A00001AUHAIL MARKETING WORKS                                                  " +
				"$2                              2/2-6 ORION ROAD LANE COVE                      " +
				"$3                                                   LANE COVE                  " +
				"$4                                            2066      AUBRESHE59SYD           " +
				"Y  1101SV9$I00005";
			message.EM_LinkedObject = organisation;

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse;
			message2.EM_MessageNum = "~15000";

			message2.EM_MessageText =
				"B01       $R                                               ~15000               " +
				"$A" + address.PK.ToString().PadRight(78) +
				"$5U00001AUHAIL MARKETING WORKS                                                  " +
				"$6                               MID12322                                       " +
				"$7B38 UFLPA FLAG XUAR POSTAL CODE                                               " +
				"Y  8888XJ5$R00002000000189600                                                   ";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var subject = "Add Manufacturer File Response (Warning) for Organization: ORG, MID: AUBRESHE59SYD";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == subject);
			AssertNotNull("Email subject should contain (warning)", email);
		}

		public void TestIfAcceptedUpdateMessageButErrorFoundInResults()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.OH_Code = "ORG";
			var address = organisation.Addresses.AddNew();
			address.OA_Address1 = "Address 1";
			var cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID12322");

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B01       $I                                               ~15000               " +
				"$A" + address.PK.ToString().PadRight(78) +
				"$1U00001AUHAIL                                                                  " +
				"$2                              2/2-6 ORION ROAD LANE COVE                      " +
				"$3                                                   LANE COVE                  " +
				"$4                                            2066      AUBRESHE59SYD           " +
				"Y  1101SV9$I00005";
			message.EM_LinkedObject = organisation;

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse;
			message2.EM_MessageNum = "~15000";

			message2.EM_MessageText =
				"B01       $R                                               ~15000               " +
				"$A" + address.PK.ToString().PadRight(78) +
				"$5U00001AUHAIL MARKETING WORKS                                                  " +
				"$6                               MID12322                                       " +
				"$7B38 UFLPA FLAG XUAR POSTAL CODE                                               " +
				"Y  8888XJ5$R00002000000189600                                                   ";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var subject = "Update Manufacturer File Response (Warning) for Organization: ORG, MID: AUBRESHE59SYD";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == subject);
			AssertNotNull("Email subject should contain (warning)", email);
		}

		public void TestUpdatingManufacturerIDFailed_EmailSubjectShouldContainTheMIDSentToUSC()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.OH_Code = "ORG";
			var address = organisation.Addresses.AddNew();
			address.OA_Address1 = "Address 1";
			var cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID12322");

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B01       $I                                               ~15000               " +
				"$A" + address.PK.ToString().PadRight(78) +
				"$1A00001AUHAIL MARKETING WORKS                                                  " +
				"$2                              2/2-6 ORION ROAD LANE COVE                      " +
				"$3                                                   LANE COVE                  " +
				"$4                                            2066      AUHAIMAR226LAN          " +
				"Y  1101SV9$I00005";
			message.EM_LinkedObject = organisation;

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse;
			message2.EM_MessageNum = "~15000";

			message2.EM_MessageText =
				"B01       $R                                               ~15000               " +
				"$A" + address.PK.ToString().PadRight(78) +
				"$5U00001AUHAIL MARKETING WORKS                                                  " +
				"$6                               MID12322                                       " +
				"$7B13 WARNING - TRANSMITTED MID INVALID                                         " +
				"Y  8888XJ5$R00002000000189600                                                   ";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			cusCode.Reload();
			AssertEquals("MID should NOT have been updated", "MID12322", cusCode.OK_CustomsRegNo);

			var subject = "Add Manufacturer File Response (Warning) for Organization: ORG, MID: AUHAIMAR226LAN";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == subject);
			AssertNotNull("Email subject should contain the MID sent to Customs.", email);
		}

		public void TestUpdatingPostCode()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			var address = organisation.Addresses.AddNew();
			address.OA_Address1 = "Address 1";

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B01       $I                                               ~15000               " +
				"$A" + address.PK.ToString().PadRight(78) +
				"$1U00001                                                                        " +
				"$2                              2/2-6 ORION ROAD LANE COVE                      " +
				"$3                                                   LANE COVE                  " +
				"$4                                            2066      AUBRESHE59SYD           " +
				"Y  1101SV9$I00005";
			message.EM_LinkedObject = organisation;

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse;
			message2.EM_MessageNum = "~15000";

			message2.EM_MessageText =
				"B01       $R                                               ~15000               " +
				"$A" + address.PK.ToString().PadRight(78) +
				"$5U00001AUHAIL MARKETING WORKS                                                  " +
				"$6                               AUBRESHE59SYD    2066                          " +
				"Y  8888XJ5$R00002000000189600                                                   ";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			address.Reload();
			AssertEquals("PostCode should have been updated", "2066", address.OA_PostCode);
		}

		public void TestProcessingErrorB22DoesNotReportToUs()
		{
			var responseData =
				"$A638D0116-7A63-446F-8551-74AB709D8BB9                                          " +
				"$5E00001CNQINHUANGDAO HIGH TECH ENERGY                                          " +
				"$6                               CNQINHIG1QIN                                   " +
				"$7B22 MID ALREADY EXISTS                                                        ";
			var expectedBodyMessage = @"<br />
Manufacturer ID Code : CNQINHIG1QIN<br /><br />Firm Name : QINHUANGDAO HIGH TECH ENERGY <br />ISO Country Code : CN<br /><br /><b style=""color:red"">Error Code : B22<br />Description : MID ALREADY EXISTS<br /></b>
<br />";
			AssertErrorCodeNotReportedToUs(responseData, true, expectedBodyMessage);
		}

		void AssertErrorCodeNotReportedToUs(string responseData, bool isFailure, string expectedBodyMessage)
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~15000";
			outgoingMessage.EM_MessageText = "B018888XJ5$I                                                                    Y         SF00001000000000000000000000000";
			outgoingMessage.EM_LinkedObject = organisation;

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAddResponse;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText =
				"B00                                                        B".PadRight(80) +
				responseData +
				"Y           00003";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals(EDIMessage.Status.Received, incomingMessage.EM_Status);

			var subject = string.Format("Add Manufacturer File Response {0}for Organization: {1}", isFailure ? "(Failure) " : "", organisation.OH_Code);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == subject; }));
			Assert("Send To Notify Group", email.Recipients.Contains("dong2@pretend.email.com"));
			AssertContains(expectedBodyMessage, email.Body);
			AssertEquals("", ErrorReporter.LastKeyReported);
		}
	}
}
