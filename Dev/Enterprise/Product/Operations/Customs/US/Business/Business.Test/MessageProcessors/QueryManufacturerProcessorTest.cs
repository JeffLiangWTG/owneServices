using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.US;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class QueryManufacturerProcessorTest : ABIProcessorTest<ACEQueryManufacturerProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			var generator = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse,
				"$1123" +
				"$7B12 ISO COUNTRY CODE NOT FOUND");

			var processor = new ACEQueryManufacturerProcessor();
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
			{ return emailToMatched.Subject.Contains("Query Manufacturer File"); }));
			Assert(sentMail.Body.Contains("Query Manufacturer File Response for Unknown"));
			var banner = sentMail.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		public void TestCalculateStatus()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			MQEDIMessage message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B01       $N                                                                     $5U00001AUBRETT SHEARER                                                          $6                               AUBRESHE59SYD                                   $7B29 TEST MODE - FILE NOT UPDATED                                              Y         $I00014000000000000000000000000";
			message.EM_LinkedObject = organisation;

			MQEDIMessage message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse;
			message2.EM_MessageNum = "~15000";

			message2.EM_MessageText =
"B01       $S                                               ~15000               " +
"$1123                                                                           " +
"$7B12 ISO COUNTRY CODE NOT FOUND                                                " +
"Y  8888XJ5$S00012000000189600                                                   ";

			Factory.Save();

			AssertNoExceptionThrown(delegate
			{ new ABIIncomingMessageProcessor().ExecuteBatch(); });
		}

		[TestDate(2016, 3, 11)]
		public void TestCreateNewOrgWithoutDetailsAndSendMessage()
		{
			OrgHeader newOrg = Factory.New<OrgHeader>();
			newOrg.OH_IsConsignee = true;
			ZString mID = "CHCHATEC810MEY";
			newOrg.OH_FullName = AutocreatefromMID.FullName;
			newOrg.MainAddress.OA_Address1 = AutocreatefromMID.Address1;
			newOrg.MainAddress.OA_Address2 = mID;
			newOrg.MainAddress.OA_City = "MEY";

			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.FillWithValidTestData();
			unloco.RL_Code = "CHMT~";
			newOrg.OH_RL_NKClosestPort = unloco.RL_Code;

			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(newOrg);
			var messageData = new USMIDQuery(Factory);
			messageData.US_MID = mID;
			wrapper.Messages.Add(ManufacturerIdentifierQueryBuilder.Generate(messageData));

			Factory.Save();

			BlockControlGenerator generator = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse,
"$1CHCHATEC810MEY                                                                ",
"$2    CHCHARMILLES TECHNOLOGIES SA                                              ",
"$3                              8-10 RUE DU PRE-DELA-FONTAINE                   ",
"$4                                                   MEYRINABCDEFG              ",
"$5SOMELONGPRO                                 1217                              ");

			var processor = new ACEQueryManufacturerProcessor();
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_LinkedObject = newOrg;
			message.EM_MessageNum = wrapper.Messages[0].EM_MessageNum;
			processor.Message = message;

			foreach (MessageBlock messageBlock in generator.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}

			processor.Process();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedOrg = newFactory.Load<OrgHeader>(newOrg.PK);
			wrapper = OrgHeaderWrapper.New(reloadedOrg);
			AssertEquals(2, wrapper.Messages.Count);
			AssertEquals("CHARMILLES TECHNOLOGIES SA", newOrg.OH_FullName);
			AssertEquals("8-10 RUE DU PRE-DELA-FONTAINE", newOrg.MainAddress.OA_Address1);
			AssertEquals("", newOrg.MainAddress.OA_Address2);
			AssertEquals("MEYRINABCDEFG SOMELONGPRO", newOrg.MainAddress.OA_City);
			AssertEquals("1217", newOrg.MainAddress.OA_PostCode);
		}

		[TestDate(2016, 3, 11)]
		public void TestCreateOrganizationIfAutoCreateOption()
		{
			var messageData = new USMIDQuery(Factory);
			messageData.US_MID = "CHCHATEC810MEY";
			messageData.US_AutoCreateOrganization = true;
			MQEDIMessage generatedMessage = ManufacturerIdentifierQueryBuilder.Generate(messageData);

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "LOCO";
			unloco.RL_PortName = "SOME CITY";
			unloco.RL_RN_NKCountryCode = "US";

			Factory.Save();

			AssertEquals(ZGuid.Empty, generatedMessage.EM_LinkUniqueID);

			BlockControlGenerator generator = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse,
"$1CHCHATEC810MEY                                                                ",
"$2    USTEST ORGANIZATION  LOGIES SA                                            ",
"$3                              8-10 RUE DU PRE-DELA-FONTAINE                   ",
"$4                                                   SOME CITY                  ",
"$5                                            2018                              ");

			var processor = new ACEQueryManufacturerProcessor();
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = generatedMessage.EM_MessageNum;
			processor.Message = message;

			foreach (MessageBlock messageBlock in generator.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}

			processor.Process();
			Factory.Save();

			OrgHeader createdOrganization = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "TESORG"));

			generatedMessage.Reload();
			AssertEquals(createdOrganization.PK, generatedMessage.EM_LinkUniqueID);
			AssertEquals("TEST ORGANIZATION  LOGIES SA", createdOrganization.OH_FullName);
			AssertEquals("8-10 RUE DU PRE-DELA-FONTAINE", createdOrganization.MainAddress.OA_Address1);
			AssertEquals("SOME CITY", createdOrganization.MainAddress.OA_City);
			AssertEquals("2018", createdOrganization.MainAddress.OA_PostCode);
			AssertEquals(1, createdOrganization.MainAddress.CustomsCodes.Count);
			AssertEquals("CHCHATEC810MEY", createdOrganization.MainAddress.CustomsCodes[0].OK_CustomsRegNo);
			AssertEquals("LOCO", createdOrganization.UNLOCO.Code);

			EmailDef sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Manufacturer File"); }));
			var emailBody = sentMail.Body;
			AssertContains("Email Body", "Auto create Organization option has been selected for this message", emailBody);
			AssertContains("Email Body", "UNLOCO : LOCO<br />", emailBody);
		}

		[TestDate(2016, 3, 11)]
		public void TestCreateOrganizationIfAutoCreateOptionForCanada()
		{
			var messageData = new USMIDQuery(Factory);
			messageData.US_MID = "XOPHIINC63ETO";
			messageData.US_AutoCreateOrganization = true;
			MQEDIMessage generatedMessage = ManufacturerIdentifierQueryBuilder.Generate(messageData);

			Factory.Save();
			AssertEquals(ZGuid.Empty, generatedMessage.EM_LinkUniqueID);

			BlockControlGenerator generator = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse,
"B013910SV9$S                                               HYEDUSCMT_147230     ",
"$1XOPHIINC63ETO                                                                 ",
"$2    XOPHILAIR INC                                                             ",
"$3                              63 GALAXY BLVD UNIT 1 2                         ",
"$4                                                   ETOBICOKE                  ",
"$5                                            M9W5R7                            ");

			var processor = new ACEQueryManufacturerProcessor();
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = generatedMessage.EM_MessageNum;
			processor.Message = message;

			foreach (MessageBlock messageBlock in generator.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}

			processor.Process();
			Factory.Save();

			var createdOrganization = OrgHeader.LoadFromCode(Factory, "PHILAIETC");
			AssertEquals("PHILAIR INC", createdOrganization.OH_FullName);
			AssertEquals("63 GALAXY BLVD UNIT 1 2", createdOrganization.MainAddress.OA_Address1);
			AssertEquals("ETOBICOKE", createdOrganization.MainAddress.OA_City);
			AssertEquals("M9W5R7", createdOrganization.MainAddress.OA_PostCode);
			AssertEquals(1, createdOrganization.MainAddress.CustomsCodes.Count);
			AssertEquals("XOPHIINC63ETO", createdOrganization.MainAddress.CustomsCodes[0].OK_CustomsRegNo);
			AssertEquals("CAETC", createdOrganization.UNLOCO.Code);

			EmailDef sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Query Manufacturer File"); }));
			var emailBody = sentMail.Body;
			AssertContains("Email Body", "Auto create Organization option has been selected for this message", emailBody);
			AssertContains("Email Body", "UNLOCO : CAETC<br />", emailBody);
		}

		public void TestExistingOrgWithEmptyOrgCodeGeneration()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "CNYANPERYAN";
			organisation.MainAddress.OA_Address1 = "CNYANPERYAN";
			organisation.MainAddress.OA_City = "YAN";
			organisation.OH_RL_NKClosestPort = "CN";
			organisation.OH_IsConsignor = true;

			organisation.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CNYANPERYAN");

			organisation.OH_Code = "CNYANP45";
			Factory.Save();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			MQEDIMessage message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "6175";
			message.EM_MessageText = "B0139019M3$N                                               6175                 $ CNYANPERYAN                                                                   Y  39019M3$N00001";
			message.EM_LinkedObject = organisation;

			MQEDIMessage message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse;
			message2.EM_MessageNum = "6175";
			message2.EM_MessageText = "B0139019M3$S                                               6175                 $1CNYANPERYAN                                                                   $2    CNYANTAI PERZINA PIANO MFG. CO., LTD                                      $3                              INDUSTRY ZONE OF XIE JIA ZHUNG, LAI SHAN        $4DISTRICT                                           YANTAI CITY, SHANDONG      $5P                                           225111                            Y  39019M3$S00005";

			Factory.Save();

			Globals.IsUserInteractive = false;

			ABIIncomingMessageProcessor msgProcessor = new ABIIncomingMessageProcessor();
			ArrayList logs = new ArrayList();
			msgProcessor.Logger.OnLogInfoAdded += (string msg, LogType logType) => logs.Add(msg);
			msgProcessor.ExecuteBatch();
			foreach (string log in logs)
			{
				Assert(log, !log.Contains("** Error Saving Record **"));
			}
		}

		public void TestUpdateOrganizationDetails()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = AutocreatefromMID.FullName;
			organisation.MainAddress.OA_Address1 = AutocreatefromMID.Address1;
			organisation.MainAddress.OA_City = "YAN";
			organisation.OH_RL_NKClosestPort = "CN";
			organisation.OH_IsConsignor = true;
			organisation.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CNYANPERYAN");
			organisation.OH_Code = "CNYANP45";
			Factory.Save();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAdd;
			message.EM_MessageNum = "6175";
			message.EM_MessageText = "B0139019M3$N                                               6175                 $ CNYANPERYAN                                                                   Y  39019M3$N00001";
			message.EM_LinkedObject = organisation;

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse;
			response.EM_MessageNum = "6175";
			Factory.Save();

			var processor = new ACEQueryManufacturerProcessor();
			processor.Message = response;
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse,
				"$1CNYANPERYAN                                                                   ",
				"$2      SOCIETA AZIONARIA LABORATORI ALCALOIDI RIFORNIMENTI                     ",
				"$3SANITARI INDUSTRY ZONE                                                        ",
				"$4DISTRICT                                           YANTAI CITY, SHANDONG      ",
				"$5P                                           225111                            ");

			AssertNoExceptionThrown(delegate
			{ processor.Process(); });
			AssertEquals("SOCIETA AZIONARIA LABORATORI ALCALOIDI RIFORNIMENTI SANITARI INDUSTRY ZONE", organisation.OH_FullName);
		}

		public void TestUpdateOrganizationDetailsAssertingMaxLengthOfAddress1NotExceeded()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = AutocreatefromMID.FullName;
			organisation.MainAddress.OA_Address1 = AutocreatefromMID.Address1;
			organisation.MainAddress.OA_City = "YAN";
			organisation.OH_RL_NKClosestPort = "CN";
			organisation.OH_IsConsignor = true;
			organisation.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CNYANPERYAN");
			organisation.OH_Code = "CNYANP45";
			Factory.Save();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAdd;
			message.EM_MessageNum = "6175";
			message.EM_MessageText = "B0139019M3$N                                               6175                 $ CNYANPERYAN                                                                   Y  39019M3$N00001";
			message.EM_LinkedObject = organisation;

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse;
			response.EM_MessageNum = "6175";
			Factory.Save();

			var processor = new ACEQueryManufacturerProcessor();
			processor.Message = response;
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse,
				"$1CNYANPERYAN                                                                   ",
				"$2      SOCIETA AZIONARIA LABORATORI ALCALOIDI RIFORNIMENTI                     ",
				"$3SANITARI INDUSTRY ZONE                  DISTRICTDISTRICTDISTRICT              ",
				"$4DISTRICTDISTRICTDISTRICTDISTRICTDISTRICTDISTRICT   YANTAI CITY, SHANDONG      ",
				"$5P                                           225111                            ");

			AssertNoExceptionThrown(delegate
			{ processor.Process(); });
			AssertEquals("SOCIETA AZIONARIA LABORATORI ALCALOIDI RIFORNIMENTI SANITARI INDUSTRY ZONE", organisation.OH_FullName);
		}

		[TestDate(2017, 12, 31)]
		public void TestProcessACEManufacturerQuery()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CNYANPERYAN");
			org.OH_IsConsignee = true;

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var transmitMessage = mock.Object;
			transmitMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmitMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmitMessage.EM_MessageNum = "6175";
			transmitMessage.EM_MessageText = "B0139019M3MA                                               6175                 $ CNYANPERYAN                                                                   Y  39019M3MA00001";
			transmitMessage.EM_LinkedObject = org;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse;
			responseMessage.EM_MessageNum = "6175";
			responseMessage.EM_MessageText = "B0139019M3MY                                               6175                 $1CNYANPERYAN                                                                   $2    CNYANTAI PERZINA PIANO MFG. CO., LTD                                      $3                              INDUSTRY ZONE OF XIE JIA ZHUNG, LAI SHAN        $4DISTRICT                                           YANTAI CITY, SHANDONG      $5P                                           225111                            Y  39019M3MY00005";

			Factory.Save();

			AssertNoExceptionThrown(delegate
			{ new ABIIncomingMessageProcessor().ExecuteBatch(); });
		}
	}
}
