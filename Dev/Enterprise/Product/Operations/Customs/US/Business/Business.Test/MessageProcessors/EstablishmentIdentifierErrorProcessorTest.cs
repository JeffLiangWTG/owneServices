using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class EstablishmentIdentifierErrorProcessorTest : TestCaseWithFactory
	{
		public void TestFailedResponse()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse,
				"FD20AEAGLE DATAMATION INTERNATIONAL PTY LT104 BOURKE RD",
	"FD21ALEXANDRIA           IL2015",
	"FDER             008INVALID ZIP CODE",
	"FD22",
	"FDER             VGXINVALID FORMAT",
	"FDER             524TRANSACTION DATA REJECTED");
			BlockProcessor.ProcessBlock<EstablishmentIdentifierErrorProcessor>(Factory, block, EstablishmentIdentifierErrorProcessor.IdentifierFailedSubject, "INVALID ZIP CODE");
		}

		public void TestFailedQueryResponse()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse,
	"FD10Q001234569845                                                               ",
	"FDER             VGZFEI NOT FOUND ON ACS                                        ");
			BlockProcessor.ProcessBlock<EstablishmentIdentifierErrorProcessor>(Factory, block, EstablishmentIdentifierErrorProcessor.IdentifierFailedSubject, "FEI NOT FOUND ON ACS");
		}

		public void TestSuccessfulResponseSent1Email()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			MQEDIMessage message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "35963";
			message.EM_MessageText = @"B018888XJ5PP                                               35963                
FD20ADEMO PHOENIX IMPORTER                225 RAINTREE                          
FD21SCOTTSDALE           AZ85260                                                
Y  8888XJ5PP00002";
			message.EM_LinkedObject = organisation;

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_EmailAddress = "hello@hello";
			message.EM_SystemCreateUser = staff.GS_Code;

			MQEDIMessage message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse;
			message2.EM_MessageNum = "35963";

			message2.EM_MessageText =
				"B018888XJ5PT                                               35963                " +
				"FD20ADEMO PHOENIX IMPORTER                225 RAINTREE                          " +
				"FD21SCOTTSDALE           AZ85260                                                " +
				"FD301003007932447                                                               " +
				"Y  8888XJ5PT00003";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			message2.Reload();
			NUnit.Framework.TestCase.AssertEquals(organisation.PK, message2.EM_LinkUniqueID);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("FDA Establishment Identifier created", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
		}
	}
}
