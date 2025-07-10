using Enterprise.Customs.US.Messaging.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondHeaderDocumentSupporter))]
	sealed class CusInBondHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals("In-bond Header cannot be found.", header.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusInBondHeader), null));
			AssertEquals("In-bond Header cannot be found.", header.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.Notes), null));
		}

		public void TestShowReasonForNotPrinting()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals(true, header.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.CusInBondHeader, null));
			AssertEquals(true, header.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.Notes, null));
			AssertEquals(false, header.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, null));
		}

		public void TestSupportedDataContexts()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals("DataContext.CusInBondHeader is Supported", true, header.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CusInBondHeader)));
			AssertEquals("DataContext.Notes is Supported", true, header.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.Notes)));
		}

		public void TestGetFilterValue()
		{
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "Z!Z";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "Z!Z";
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = branch2.PK;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirContainer;
			var supporter = header.DocumentSupporter;
			AssertEquals(Core.Constants.CountryCodes.PuertoRico, supporter.GetFilterValue(DocumentFilters.CTY));
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, supporter.GetFilterValue(DocumentFilters.BKRCTY));
			AssertEquals("OTH", supporter.GetFilterValue(DocumentFilters.MSC));
			AssertEquals(TransportModeCodes.Codes.AirContainer, supporter.GetFilterValue(DocumentFilters.MOD));
			AssertNull(supporter.GetFilterValue(DocumentFilters.LGR));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			AssertEquals(Env.Security.USInBondEditCustomiseDocuments, header.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetBODocDataProviders()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "INB234234";
			CusInBondMoveHeader moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "INB636985";
			var providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusInBondHeaderDocumentSupporter.CBPForm7512DataContextValue), null);
			AssertEquals(2, providers.Length);
			CBP7512Document print = (CBP7512Document)providers[0].ParentBusinessObject;
			AssertEquals("INB234234", print.FormattedEntryNumber);
			print = (CBP7512Document)providers[1].ParentBusinessObject;
			AssertEquals("INB636985", print.FormattedEntryNumber);
		}

		public void TestGetContactOrganisation()
		{
			var importOrg = Factory.New<OrgHeader>();
			importOrg.OH_Code = "IMPTEST";
			var importContact = importOrg.Contacts.AddNew();
			importContact.OC_ContactName = "AAA";
			importContact.OC_Email = "test1@wisetechglobal.com";
			var supplierOrg = Factory.New<OrgHeader>();
			supplierOrg.OH_Code = "SUPTEST";
			var supplierContact = supplierOrg.Contacts.AddNew();
			supplierContact.OC_ContactName = "BBB";
			supplierContact.OC_Email = "test2@wisetechglobal.com";
			var carrierOrg = Factory.New<OrgHeader>();
			carrierOrg.OH_Code = "CARTEST";
			var carrierContact = carrierOrg.Contacts.AddNew();
			carrierContact.OC_ContactName = "CCC";
			carrierContact.OC_Email = "test3@wisetech.com";
			var header = Factory.New<CusInBondHeader>();
			header.ImporterOrgPK = importOrg.PK;
			header.BH_OH_Supplier = supplierOrg.PK;
			var moveHeader = header.MovementHeader;
			moveHeader.BM_OA_InBondCarrier = carrierOrg.MainAddress.PK;
			var supporter = header.DocumentSupporter;
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "7512Tmplate";
			menuItem.SU_PreventAutoDelivery = true;
			menuItem.SU_ContactType = ContactType.Consignee.Code;
			var documentDeliveryContact = supporter.GetContactOrganisation(menuItem.SU_MenuName, ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("If contact type is Consignee, then use Importer organization.", "IMPTEST", documentDeliveryContact.OrgHeader.OH_Code);
			documentDeliveryContact = supporter.GetContactOrganisation(menuItem.SU_MenuName, ContactType.Consignor, DocumentDirection.ANY);
			AssertEquals("If contact type is Consignor, then use Supplier organization.", "SUPTEST", documentDeliveryContact.OrgHeader.OH_Code);
			documentDeliveryContact = supporter.GetContactOrganisation(menuItem.SU_MenuName, ContactType.ShippingLine, DocumentDirection.ANY);
			AssertEquals("If contact type is ShippingLine, then use Carrier organization.", "CARTEST", documentDeliveryContact.OrgHeader.OH_Code);
			documentDeliveryContact = supporter.GetContactOrganisation(menuItem.SU_MenuName, ContactType.ExportBroker, DocumentDirection.ANY);
			AssertNull("For contact type other than Consignee or Consignor, documentDeliveryContact is null.", documentDeliveryContact);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MWB123";
			CusInbondBillAddRef billRef1 = bill1.AdditionalReferences.AddNew();
			billRef1.BR_Qualifier = ReferenceQualifierList.Codes.CR;
			billRef1.BR_ReferenceNum = "CR123";
			CusInBondBill bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MWB456";
			CusInbondBillAddRef billRef2 = bill2.AdditionalReferences.AddNew();
			billRef2.BR_Qualifier = ReferenceQualifierList.Codes.CG;
			billRef2.BR_ReferenceNum = "CG456";
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			CusInBondMoveDetail moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill2.PK;
			return header;
		}
	}
}
