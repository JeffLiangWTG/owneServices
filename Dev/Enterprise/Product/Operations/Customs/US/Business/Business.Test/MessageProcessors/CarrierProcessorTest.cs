using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business.Internal;
using Enterprise.Customs.US.Business.RefDbEntUS;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class CarrierProcessorTest : TestCaseWithFactory
	{
		public void TestAddNew()
		{
			var generator = new ABIOutputBlockControlGenerator<AABIOutputB, AABIOutputY>();
			generator.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			var f106 = new ERFF106();
			f106.CarrierCode = "XX";
			f106.CarrierName = "NAME";
			f106.CarrierModeOfTransportation = "10";
			generator.AddMessageBlock(f106);

			ProcessMessage(generator);

			var query = new ZQuery(USCarrierSchema.USC_Code, "XX");
			var port = Factory.LoadTop1<USCarrier>(query);
			AssertEquals("XX", port.USC_Code);
			AssertEquals("NAME", port.USC_Name);
			AssertEquals("ModeOfTransportation", "10", port.USC_ModeOfTransportation);
		}

		public void TestUpdateExistingCarrierForCode()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			CreateUserEnterableCarrier("KKLU", "");

			Factory.Save();
			CreateMockMessage("800", "B018888XJ5FO                                               800                  " +
				"F106KKLU'K' LINE AMERICA, INC              10                                   " +
				"F106KKLU'K' LINE AMERICA, INC              10TOKYO                              " +
				"F106KKLU'K' LINE AMERICA, INC              10          JP                       " +
				"Y  8888XJ5FR00003                                                               ");

			DeclarationTestHelper.SetupForSendMessage();
			new USRIncomingMessageProcessor().ExecuteBatch();

			var query = new ZQuery(USCarrierSchema.USC_Code, "KKLU");
			AssertUpdateResults(query, "KKLU", "'K' LINE AMERICA, INC");
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertContains("Carrier Code Request Response", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<ZArchitecture.Environment.AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		public void TestUpdateExistingCarrierForName()
		{
			CreateUserEnterableCarrier("CVI1", "CASTLE AVIATION INC");
			Factory.Save();
			CreateMockMessage("801", "B018888XJ5FO                                               801                  " +
				"F106CVI~CASTLE AVIATION INC                405430 LAUBY RD                      " +
				"F106CVI~CASTLE AVIATION INC                40NORTH CANTON                 OH    " +
				"F106CVI~CASTLE AVIATION INC                4044720                              " +
				"Y  8888XJ5FR00003                                                               ");

			new USRIncomingMessageProcessor().ExecuteBatch();

			var factoryForLoad = new BusinessObjectFactory();
			var carriers = factoryForLoad.Load<USCarrier>(new ZQuery(USCarrierSchema.USC_Code, "CVI1"));
			AssertEquals("Carrier updated", 1, carriers.Length);
			AssertEquals("Carrier code", "CASTLE AVIATION INC", carriers[0].USC_Name);

			carriers = factoryForLoad.Load<USCarrier>(new ZQuery(USCarrierSchema.USC_Code, "CVI~"));
			AssertEquals("Carrier updated", 1, carriers.Length);
			AssertEquals("Carrier code", "CASTLE AVIATION INC", carriers[0].USC_Name);
		}

		public void TestACECarrierProcessor_ErrorResponse()
		{
			CreateMockMessage("8789",
				"B00       FO                                                                    " +
				"F106XX  APL4                                                                    " +
				"F906                            E01INVALID CARRIER CODE                         " +
				"F106XX  APL3                                                                    " +
				"F906                            E02INVALID CARRIER CODE                         " +
				"F106XX  APL8                                                                    " +
				"F906                            E03INVALID CARRIER CODE                         " +
				"Y         FO00002                                                               ",
				ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse);

			DeclarationTestHelper.SetupForSendMessage();
			new USRIncomingMessageProcessor().ExecuteBatch();

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			AssertEquals("Carrier Code Request Response (Failure) for Carrier Code Request", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("XX"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("APL4"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("APL3"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("APL8"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("E01"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("E02"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("E03"));
			Assert(Env.OutgoingCustomsMailManager.EmailsCreated[0].Body.Contains("INVALID CARRIER CODE"));
		}

		public void TestResponseMessageProcessing_MultipleAirCarrierCodes()
		{
			var query = new ZQuery(USCCarrierSchema.UI_Code, "CC");
			query.ReLoadExistingRows = true;
			var uscCarriers = Factory.Load<USCCarrier>(query);
			var uscCarrier = uscCarriers.Length > 0 ? uscCarriers[0] : Factory.New<USCCarrier>();
			uscCarrier.UI_Code = "CC";
			uscCarrier.UI_Name = "AIR 1";
			uscCarrier.UI_ModeOfTransportation = "41";
			uscCarrier.UI_Address = "BOB";
			uscCarrier.UI_AirwayBillPrefix = "320";
			uscCarriers.Where(x => x != uscCarrier).DeleteAll();
			Factory.Save();
			var messageProcessorFactory = new USRMessageProcessorFactory(new LoggingInformation());

			var ediMessage = Factory.New<MQEDIMessage>();
			ediMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			ediMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			ediMessage.EM_MessageText =
				  "B018888XJ5FO                                               17076                " +
				  "F106    ALLIANCE AIR CHARTER               40                                000" +
				  "F106    AIR AMERICA CHARTER COMPANY        40                                000" +
				  "F106ZI  AIGLE AZUR                         40                                439" +
				  "F106    ALTO AIR FREIGHT                   40                                000" +
				  "F106    AMERICAN AIRLINES INC (AIR CODE AA)40                                000" +
				  "F106    ATLANTIC AIR TRANSPORT             40                                000" +
				  "F106AQ  ALOHA AIRLINES INC.                40                                327" +
				  "F106    ALG ADMIRAL INC                    40                                000" +
				  "F106    ACTIVE AIR FREIGHT LLC             40                                000" +
				  "F106AA  AMERICAN AIRLINES                  40                                001" +
				  "F106    ASIAN AMERICAN TRANSPORTATION      40                                000" +
				  "F106    AIR ALLIANCE INC.                  40                                000" +
				  "F106OZ  ASIANA AIRLINES                    40                                988" +
				  "F106    ALAS AIRLINES                      40                                000" +
				  "F106    ALASKA AIR TOURS INC               40                                000" +
				  "F106    ATLANTIC AVIATION                  40                                000" +
				  "F1068U  AFRIQIYAH AIRWAYS                  40                                546" +
				  "F106    AIRTRAN AIRWAYS                    40                                000" +
				  "F106    ADVANTAGE AIR FORWARDING INC       40                                000" +
				  "F106G4  ALLEGIANT AIR INC                  40                                000" +
				  "F106    AIR BELGIUM INTL                   40                                000" +
				  "F106CC  AIR ATLANTA ICELANDIC              40                                318" +
				  "F106K5  ABAN AIR                           40                                710" +
				  "F106DF  AEROLINEAS DE BALEARES AEBAL       40                                059" +
				  "F106    AIR BC LTD                         40                                000" +
				  "F106    ABI LOGISTICS INC                  40                                000" +
				  "Y  8888XJ5FR01607";

			messageProcessorFactory.ProcessMessage(ediMessage);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			uscCarriers = Factory.Load<USCCarrier>(query);
			AssertEquals(1, uscCarriers.Length);
			AssertEquals(uscCarrier, uscCarriers[0]);
			uscCarrier.Reload();
			AssertEquals("AIR 1", uscCarrier.UI_Name);
			AssertEquals("41", uscCarrier.UI_ModeOfTransportation);
			AssertEquals("BOB", uscCarrier.UI_Address);
			AssertEquals("320", uscCarrier.UI_AirwayBillPrefix);

			query = new ZQuery(USCarrierSchema.USC_Code, "CC");
			var carriers = Factory.Load<USCarrier>(query);
			AssertEquals(1, carriers.Length);

			var carrier = carriers[0];
			AssertEquals("AIR ATLANTA ICELANDIC", carrier.USC_Name);
			AssertEquals("40", carrier.USC_ModeOfTransportation);
			AssertEquals("", carrier.USC_Address);
			AssertEquals("318", carrier.USC_AirwayBillPrefix);

			query = new ZQuery(USCarrierSchema.USC_Name, "ALASKA AIR TOURS INC");
			carriers = Factory.Load<USCarrier>(query);
			AssertEquals("Empty Carrier Code should not be added", 0, carriers.Length);

			query = new ZQuery(USCarrierSchema.USC_Name, "AFRIQIYAH AIRWAYS");
			carriers = Factory.Load<USCarrier>(query);
			AssertEquals(1, carriers.Length);

			carrier = carriers[0];
			AssertEquals("8U", carrier.USC_Code);
			AssertEquals("40", carrier.USC_ModeOfTransportation);
			AssertEquals("", carrier.USC_Address);
			AssertEquals("546", carrier.USC_AirwayBillPrefix);

			query = new ZQuery(USCarrierSchema.USC_Name, "SHIT AIR LLC");
			carriers = Factory.Load<USCarrier>(query);
			AssertEquals(0, carriers.Length);
		}

		public void TestExistingUserEnterableDataIsUpdated()
		{
			var carrier = CreateUserEnterableCarrier("KKLU", "carrier 2");
			Factory.Save();
			CreateMockMessage("800", "B018888XJ5FO                                               800                  " +
				"F106KKLU'K' LINE AMERICA, INC              10                                   " +
				"F106KKLU'K' LINE AMERICA, INC              10TOKYO                              " +
				"F106KKLU'K' LINE AMERICA, INC              10          JP                       " +
				"Y  8888XJ5FR00003                                                               ");

			new USRIncomingMessageProcessor().ExecuteBatch();

			AssertUpdateResults(new ZQuery(USCarrierSchema.USC_Code, "KKLU"), "KKLU", "'K' LINE AMERICA, INC");

			carrier.Reload();
			AssertEquals("Carrier code", "KKLU", carrier.USC_Code);
			AssertEquals("Carrier name", "'K' LINE AMERICA, INC", carrier.USC_Name);
		}

		USCarrier CreateUserEnterableCarrier(ZString carrierCode, ZString carrierName)
		{
			var carrier = Factory.New<USCarrier>();
			carrier.USC_Code = carrierCode;
			carrier.USC_Name = carrierName;
			return carrier;
		}

		MQEDIMessage CreateMockMessage(ZString messageNumber, ZString messageText, string messageType = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)
		{
			var messageResponse = Factory.New<MQEDIMessage>();
			messageResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageResponse.EM_MessageType = messageType;
			messageResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageResponse.EM_MessageNum = messageNumber;
			messageResponse.EM_MessageText = messageText;

			Factory.Save();
			return messageResponse;
		}

		void AssertUpdateResults(ZQuery query, ZString carrierCode, ZString carrierName)
		{
			var factoryForLoad = new BusinessObjectFactory();
			var carriers = factoryForLoad.Load<USCarrier>(query);
			AssertEquals("Carrier updated", 1, carriers.Length);
			AssertEquals("Carrier code", carrierCode, carriers[0].USC_Code);
			AssertEquals("Carrier name", carrierName, carriers[0].USC_Name);
		}

		void ProcessMessage(BlockControlGenerator generator)
		{
			var ediMessage = Factory.New<MQEDIMessage>();
			ediMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = generator.Serialise();
			ediMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse;
			new USRMessageProcessorFactory(new LoggingInformation()).ProcessMessage(ediMessage);
		}
	}
}
