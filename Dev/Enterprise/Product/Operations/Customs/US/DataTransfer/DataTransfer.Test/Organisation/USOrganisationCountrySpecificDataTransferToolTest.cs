using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	sealed class USOrganisationCountrySpecificDataTransferToolTest : OrganisationCountrySpecificDataTransferToolTest
	{
		public void TestExportNotifyPartyWhenUSNotifyPartyPointsToItself()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var wrapper = OrgHeaderWrapper.New(org);
			wrapper.ZO_OH_NP = org.PK;
			var xsdOrg = new Xsd.Organisation();
			var adapter = new OrganisationValueObjectDataAdapter();
			AssertNoExceptionThrown(delegate
			{
				adapter.ExportToValueObject(org, xsdOrg, new ValueObjectExportContext(Notification));
			});
			var notifyParty = (Xsd.Organisation)xsdOrg.OrganisationDetails.CountrySpecificDetails.USOrganisationSpecificDetails.ImporterOfRecordDetails.NotifyParty.Item;
			AssertEquals("Only the code is exported", org.OH_Code, notifyParty.EDICode);
		}

		public void TestExportImportUSSpecificDetails()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(org);
			wrapper.ZO_ProducerFirmType = ProducerFirmTypeList.Codes.G;
			wrapper.ZO_MFRRegExempt = FDAPriorNoticeExemptCodeList.Codes.D;
			wrapper.ZO_SubmitterFirmType = SubmitterFirmTypeList.Codes.S;
			wrapper.ZO_ENSPrintCustomAttrib1 = true;
			wrapper.ZO_ENSPrintCustomAttrib2 = false;
			wrapper.ZO_ENSPrintCustomAttrib3 = true;
			wrapper.ZO_ENSPrintProduct = true;
			wrapper.ZO_DoNotAutoGenerateSDCR = true;
			wrapper.ZO_AccountNo = "GT3456";
			wrapper.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			wrapper.ZO_Purchased = "Y";
			wrapper.ZO_SPDNumberOfDays = 2;
			wrapper.ZO_TaxDeferredInd = TaxDeferIndicatorList.Codes.DeferredTax;
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "HK1";
			wrapper.ZO_GB = branch.PK;
			wrapper.ZO_FileTheirOwnRecon = true;
			wrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			wrapper.ZO_NAFTAReconIndicator = true;
			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "NotifyParty";
			wrapper.ZO_OH_NP = notifyParty.PK;
			Factory.Save();
			Xsd.Organisation xsdOrg = new Xsd.Organisation();
			OrganisationValueObjectDataAdapter adapter = new OrganisationValueObjectDataAdapter();
			adapter.ExportToValueObject(org, xsdOrg, new ValueObjectExportContext(Notification));
			Xsd.USOrganisationSpecificDetails usOrgDetails = xsdOrg.OrganisationDetails.CountrySpecificDetails.USOrganisationSpecificDetails;
			AssertEquals(Xsd.USManufacturerProducerDataTypeProducerFirmType.G, usOrgDetails.FDA.ProducerFirmType);
			AssertEquals(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.D, usOrgDetails.FDA.FoodFacilityRegistrationExemption);
			AssertEquals(Xsd.USOrganisationSpecificDetailsFDASubmitterFirmType.S, usOrgDetails.FDA.SubmitterFirmType);
			AssertEquals(Xsd.TrueFalse.@true, usOrgDetails.EntryDocumentPrinting.CustomAttribute1);
			AssertEquals(Xsd.TrueFalse.@false, usOrgDetails.EntryDocumentPrinting.CustomAttribute2);
			AssertEquals(Xsd.TrueFalse.@true, usOrgDetails.EntryDocumentPrinting.CustomAttribute3);
			AssertEquals(Xsd.TrueFalse.@true, usOrgDetails.EntryDocumentPrinting.ProductCode);
			AssertEquals(Xsd.TrueFalse.@true, usOrgDetails.ImporterOfRecordDetails.DoNotAutoGenerateStmDayChangeRequest);
			AssertEquals("GT3456", usOrgDetails.ImporterOfRecordDetails.PayersUnitNo);
			AssertEquals(Xsd.USImporterOfRecordDetailsPaymentType.Item3, usOrgDetails.ImporterOfRecordDetails.PaymentType);
			AssertEquals(Xsd.TrueFalse.@true, usOrgDetails.ImporterOfRecordDetails.Purchased);
			AssertEquals(2, usOrgDetails.ImporterOfRecordDetails.StatementPrintDateWorkingDays);
			AssertEquals(Xsd.USImporterOfRecordDetailsTaxDeferredInd.Item1, usOrgDetails.ImporterOfRecordDetails.TaxDeferredInd);
			AssertNotEquals(ZString.Empty, usOrgDetails.Misc.BIRDDefaultBranch);
			AssertEquals(Xsd.TrueFalse.@true, usOrgDetails.Reconciliation.FileTheirOwnRecon);
			AssertEquals(Xsd.USReconciliationIssue.CL, usOrgDetails.Reconciliation.Issue);
			AssertEquals(Xsd.TrueFalse.@true, usOrgDetails.Reconciliation.NAFTA);
			AssertEquals("NotifyParty", ((Xsd.Organisation)usOrgDetails.ImporterOfRecordDetails.NotifyParty.Item).OrganisationDetails.Name);
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();
			adapter.ImportFromValueObject(newOrg, xsdOrg, new ValueObjectImportContext(Factory, Notification));
			wrapper = OrgHeaderWrapper.New(newOrg);
			Factory.Save();
			AssertEquals(wrapper.ZO_ProducerFirmType, ProducerFirmTypeList.Codes.G);
			AssertEquals(wrapper.ZO_MFRRegExempt, FDAPriorNoticeExemptCodeList.Codes.D);
			AssertEquals(wrapper.ZO_SubmitterFirmType, SubmitterFirmTypeList.Codes.S);
			AssertEquals(wrapper.ZO_ENSPrintCustomAttrib1, true);
			AssertEquals(wrapper.ZO_ENSPrintCustomAttrib2, false);
			AssertEquals(wrapper.ZO_ENSPrintCustomAttrib3, true);
			AssertEquals(wrapper.ZO_ENSPrintProduct, true);
			AssertEquals(wrapper.ZO_DoNotAutoGenerateSDCR, true);
			AssertEquals(wrapper.ZO_AccountNo, "GT3456");
			AssertEquals(wrapper.ZO_PaymentType, PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter);
			AssertEquals(wrapper.ZO_Purchased, "Y");
			AssertEquals(wrapper.ZO_SPDNumberOfDays, 2);
			AssertEquals(wrapper.ZO_TaxDeferredInd, TaxDeferIndicatorList.Codes.DeferredTax);
			AssertNotEquals(wrapper.ZO_GB, ZGuid.Empty);
			AssertEquals(wrapper.ZO_FileTheirOwnRecon, true);
			AssertEquals(wrapper.ZO_OtherReconIndicator, ReconIssueCodeList.Codes.ClassRecon);
			AssertEquals(wrapper.ZO_NAFTAReconIndicator, true);
			AssertEquals(notifyParty.PK, wrapper.ZO_OH_NP);
		}

		NotificationBuffer notify;
		NotificationBuffer Notification => notify ?? (notify = new NotificationBuffer());
	}
}
