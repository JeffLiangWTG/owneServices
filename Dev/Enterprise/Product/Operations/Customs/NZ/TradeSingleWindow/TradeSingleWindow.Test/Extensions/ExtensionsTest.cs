using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class ExtensionsTest : TestCaseWithFactory
	{
		public void TestGetSenderReferenceNumber()
		{
			var sendersReference = new ZString("TEST*(&$/*@&(*@#@123/");
			AssertEquals("TEST123", sendersReference.GetSenderReferenceNumber());
			sendersReference = new ZString("B123456789012345");
			AssertEquals("B1234567890123", sendersReference.GetSenderReferenceNumber());
			sendersReference = ZString.Empty;
			AssertEquals(TSWConstants.SendersReferencePlaceHolder, sendersReference.GetSenderReferenceNumber());
		}

		public void TestGetCustomsClientCode()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var ccdCode = organisation.CustomsCodes.AddNew();
			ccdCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			ccdCode.OK_CustomsRegNo = "372845965J";
			AssertEquals("372845965", organisation.GetCustomsClientCode());
			ccdCode.OK_CustomsRegNo = ZString.Empty;
			AssertEquals(ZString.Empty, organisation.GetCustomsClientCode());
			ccdCode.OK_CustomsRegNo = "845965J";
			AssertEquals("00845965J", organisation.GetCustomsClientCode());
			organisation.CustomsCodes.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, organisation.GetCustomsClientCode());
		}

		public void TestGetCCPOrATFCode_GetFromAddressFirst_ThenOrganisation()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.Addresses.AddNew();
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.OrganisationPK = organisation.PK;
			docAddress.E2_OA_Address = address.PK;
			organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "11111A");
			AssertEquals("We get the code from the organisation.", "11111A", docAddress.GetCCPOrATFCode());
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "22222B");
			AssertEquals("If there is a code defined on the address, we use it.", "22222B", docAddress.GetCCPOrATFCode());
		}

		public void TestGetCCPOrATFCode_IfHasBothCCPAndAFT_WeSendCCP()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.Addresses.AddNew();
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.OrganisationPK = organisation.PK;
			docAddress.E2_OA_Address = address.PK;
			organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "11111A");
			organisation.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "22222B");
			AssertEquals("If the organisation has both CCP and AFT, we send the CCP.", "11111A", docAddress.GetCCPOrATFCode());
		}

		public void TestGetSupplierCode()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var supCode = organisation.CustomsCodes.AddNew();
			supCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			supCode.OK_CustomsRegNo = "SUPCODE123";
			AssertEquals("SUPCODE12", organisation.GetSupplierCode());
			supCode.OK_CustomsRegNo = ZString.Empty;
			AssertEquals(ZString.Empty, organisation.GetSupplierCode());
			supCode.OK_CustomsRegNo = "SUPCODE";
			AssertEquals("00SUPCODE", organisation.GetSupplierCode());
			organisation.CustomsCodes.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, organisation.GetSupplierCode());
		}

		public void TestGetAllocatedContact()
		{
			OrgHeader nullHeader = null;
			AssertNull(nullHeader.GetAllocatedNZCustomsContact());
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "CONTACT1";
			var contact1Allocations = contact1.Allocations.AddNew();
			contact1Allocations.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			var contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "CONTACT2";
			var contact2Allocations = contact1.Allocations.AddNew();
			contact2Allocations.PC_Type = OrgConstants.ContactAllocationType.CNCUS;
			AssertEquals("CONTACT1", organisation.GetAllocatedNZCustomsContact().ContactName);
			contact1Allocations.PC_Type = OrgConstants.ContactAllocationType.NZBiosecurity;
			AssertNull(organisation.GetAllocatedNZCustomsContact());
		}

		public void TestGetCommunications()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "NZCONTACT";
			contact.OC_Email = "test@nztest.co.nz";
			var contactAllocations = contact.Allocations.AddNew();
			contactAllocations.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			var communications = organisation.GetCommunications();
			AssertEquals(1, communications.Count());
			var communique = communications.First();
			AssertEquals("test@nztest.co.nz", communique.ContactDetail);
			AssertEquals(CommunicationTypeList.Codes.EM, communique.ContactType);
			contact.Allocations.RemoveAndDeleteAll();
			organisation.MainAddress.OA_Email = "testfallback@nztest.co.nz";
			organisation.MainAddress.OA_Phone = "9001001";
			organisation.MainAddress.OA_Mobile = "0478611058";
			organisation.MainAddress.OA_Fax = "9001002";
			communications = organisation.GetCommunications();
			AssertEquals(4, communications.Count());
			AssertEquals("testfallback@nztest.co.nz", communications.First(x => x.ContactType == CommunicationTypeList.Codes.EM).ContactDetail);
			AssertEquals("9001001", communications.First(x => x.ContactType == CommunicationTypeList.Codes.TE).ContactDetail);
			AssertEquals("0478611058", communications.First(x => x.ContactType == CommunicationTypeList.Codes.AL).ContactDetail);
			AssertEquals("9001002", communications.First(x => x.ContactType == CommunicationTypeList.Codes.FX).ContactDetail);
		}

		public void TestNoCommsWhenValueStrippedToBlank()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "NZCONTACT";
			contact.OC_Email = "test@nztest.co.nz";
			contact.OC_Phone = "NOPHONE";
			contact.OC_Mobile = "0478611058";
			var contactAllocations = contact.Allocations.AddNew();
			contactAllocations.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			var communications = organisation.GetCommunications();
			AssertEquals("Phone contact detail above should not produce a communications interface", 2, communications.Count());
			AssertEquals("test@nztest.co.nz", communications.First(x => x.ContactType == CommunicationTypeList.Codes.EM).ContactDetail);
			AssertEquals("0478611058", communications.First(x => x.ContactType == CommunicationTypeList.Codes.AL).ContactDetail);
		}

		public void TestNoCommsWhenStrippedOnFallbackOrg()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.MainAddress.OA_Email = "testfallback@nztest.co.nz";
			organisation.MainAddress.OA_Phone = "No Phone";
			organisation.MainAddress.OA_Mobile = "No Mobile";
			organisation.MainAddress.OA_Fax = "NO FAX";
			var communications = organisation.GetCommunications();
			AssertEquals("Only email comms should have been generated", 1, communications.Count());
			AssertEquals("testfallback@nztest.co.nz", communications.First(x => x.ContactType == CommunicationTypeList.Codes.EM).ContactDetail);
		}
	}
}
