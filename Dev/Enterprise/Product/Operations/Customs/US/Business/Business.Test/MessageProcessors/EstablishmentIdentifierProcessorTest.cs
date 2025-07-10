using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class EstablishmentIdentifierProcessorTest : TestCaseWithFactory
	{
		public void TestResponseFEIResponse()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse, "FD302FDAESTIDENTF");
			BlockProcessor.ProcessBlock<EstablishmentIdentifierProcessor>(Factory, block, EstablishmentIdentifierProcessor.IdentifierCreatedSubject, "succeeded");
		}

		public void TestInvalidBrokerRequestFEIResponse()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse, "FD303FDAESTIDENTF");
			BlockProcessor.ProcessBlock<EstablishmentIdentifierProcessor>(Factory, block, EstablishmentIdentifierProcessor.IdentifierFailedCreateSubject, "Error Code 3");
		}

		public void TestRequestDeniedRequestFEIResponse()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse, "FD304FDAESTIDENTF");
			BlockProcessor.ProcessBlock<EstablishmentIdentifierProcessor>(Factory, block, EstablishmentIdentifierProcessor.IdentifierFailedCreateSubject, "Error Code 4");
		}

		public void TestAddFEINoBrokerNotificationFEIResponse()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse, "FD305FDAESTIDENTF");
			BlockProcessor.ProcessBlock<EstablishmentIdentifierProcessor>(Factory, block, EstablishmentIdentifierProcessor.IdentifierCreatedSubject, "succeeded");
		}

		public void TestDeleteFEIFromACSFEIResponse()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse, "FD306FDAESTIDENTF");
			BlockProcessor.ProcessBlock<EstablishmentIdentifierProcessor>(Factory, block, EstablishmentIdentifierProcessor.IdentifierFailedCreateSubject, "Error Code 6");
		}

		public void TestQueryResponse()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse, "FD30Q003008038672VGYFEI FOUND ON ACS");
			BlockProcessor.ProcessBlock<EstablishmentIdentifierProcessor>(Factory, block, EstablishmentIdentifierProcessor.QuerySubject, "Response Message:");
		}

		public void TestQueryResponseIfLinkedObjectIsNull()
		{
			BlockControlGenerator block = OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse, "FD30Q003008038672VGYFEI FOUND ON ACS");
			BlockProcessor.ProcessBlock<EstablishmentIdentifierProcessor>(Factory, block, EstablishmentIdentifierProcessor.QuerySubject, "Response Message:", true);
		}

		public void TestInterimResponse()
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
				"FD30ADEMO PHOENIXVG1REQUEST SENT TO FDA, PROC PENDING                           " +
				"Y  8888XJ5PT00001";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			message2.Reload();
			NUnit.Framework.TestCase.AssertEquals(organisation.PK, message2.EM_LinkUniqueID);
			AssertEquals("No Emails for interim response", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestFEIReturnedFromFDA()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.OH_FullName = "PATTERSON COMPANIES";
			organisation.OH_RL_NKClosestPort = "USBOO";
			organisation.MainAddress.OA_Address1 = "ADDRESS 1";
			organisation.MainAddress.OA_City = "BOONEVILLE";
			organisation.MainAddress.OA_State = "IA";
			organisation.MainAddress.OA_PostCode = "50032";

			var address2 = organisation.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "USBNW";
			address2.OA_Address1 = "1905 LAKEWOOD DRIVE";
			address2.OA_City = "BOONE";
			address2.OA_State = "IA";
			address2.OA_PostCode = "50036";

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			MQEDIMessage message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "27242";
			message.EM_MessageText = "B013901AYFPP                                               27242                " +
"FD20APATTERSON COMPANIES                  1905 LAKEWOOD DR                      " +
"FD21BOONE                IA50036                                                " +
"Y  3901AYFPP00002";
			message.EM_LinkedObject = address2;

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_EmailAddress = "hello@hello";
			message.EM_SystemCreateUser = staff.GS_Code;

			MQEDIMessage message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse;
			message2.EM_MessageNum = "27242";
			message2.EM_MessageText = "B013901AYFPT                                               27242                " +
"FD20APATTERSON COMPANIES                  1905 LAKEWOOD DR                      " +
"FD21BOONE                IA50036                                                " +
"FD301003009433739                                                               " +
"Y  3901AYFPT00003                                                               ";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			message2.Reload();
			NUnit.Framework.TestCase.AssertEquals(address2.PK, message2.EM_LinkUniqueID);
			var cusCode = address2.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("FEI", "003009433739", cusCode.OK_CustomsRegNo);
			AssertEquals("No Emails for FEI returned from FDA", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("BODY", @"    <tr>
      <td class=""content"">
<p style=""margin: 1em"">Your request for an FDA Establishment Identifier succeeded.<br />This identifier has been automatically attached to the organisation.<br /><br />Organisation : PATTERSON COMPANIES<br />Address : 1905 LAKEWOOD DRIVE<br />City : BOONE<br />State : IA<br />Post Code : 50036<br />Establishment Identifier : 003009433739<br /></p>      </td>
    </tr>", email.Body);
		}
	}
}
