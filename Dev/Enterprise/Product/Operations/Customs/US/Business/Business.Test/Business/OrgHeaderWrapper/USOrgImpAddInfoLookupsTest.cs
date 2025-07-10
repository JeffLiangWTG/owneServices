using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USOrgImpAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOtherReconIssueList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("OtherReconIssueList", typeof(ReconIssueCodeList), declaration.AddInfoLookups.OtherReconIssueList.GetType());
			AssertEquals("not existing ReconIssueCodeList.Codes.NAFTA", false, declaration.AddInfoLookups.OtherReconIssueList.ContainsCode(ReconIssueCodeList.Codes.FTA));
		}

		public void TestZO_ImportSourceList()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgCountryData countryData = organisation.CountryData;
			OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			AssertEquals(Factory.GetCachedValue<ReconciliationImportEntrySourceList>(), addInfo.Lookups.ZO_ImportSourceList);
		}

		public void TestReconPorts()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgCountryData countryData = organisation.CountryData;
			OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), addInfo.Lookups.ReconPorts.GetType());
		}

		public void TestReconPaymentTypes()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgCountryData countryData = organisation.CountryData;
			OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			AssertEquals(PaymentTypeList.GetCachedReconPaymentTypeList(Factory), addInfo.Lookups.ReconPaymentTypes);
		}

		public void TestList()
		{
			var organisation = Factory.New<OrgHeader>();
			var countryData = organisation.CountryData;
			var addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			AssertEquals(typeof(PaymentTypeList), addInfo.Lookups.PaymentTypes.GetType());
			AssertEquals("should have been sorted on code", PaymentTypeList.Codes.IndividualBasis, addInfo.Lookups.PaymentTypes[0].Code);
			AssertEquals(false, addInfo.Lookups.ZO_YesNoList.ContainsCode(YesNoDefaultList.Codes.Default));
			AssertEquals(typeof(OrgMessageStatusList), addInfo.Lookups.MessageStatusList.GetType());
			AssertEquals(typeof(ImporterTypeList), addInfo.Lookups.ZO_ImporterTypeList.GetType());
			AssertNotNull(addInfo.Lookups.ProducerFirmTypes);
			AssertNotNull(addInfo.Lookups.SubmitterFirmTypes);
			AssertEquals(typeof(TaxDeferIndicatorList), addInfo.Lookups.TaxDeferredIndicators.GetType());
			AssertEquals(typeof(FDAPriorNoticeExemptCodeList), addInfo.Lookups.FDAPriorNoticeExemptCodeList.GetType());
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), addInfo.Lookups.FIRMSList.GetType());
			AssertEquals(typeof(OrgHeaderCollection), addInfo.Lookups.NotifyParties.GetType());
			AssertEquals(typeof(DefTaxDueDateCalculationOptionList), addInfo.Lookups.DefTaxDueDateCalculationOptionList.GetType());
			AssertEquals(typeof(ConsigneeCollection), addInfo.Lookups.Consignees.GetType());
			AssertEquals(8, addInfo.Lookups.ZO_ImporterTypeList.Count);
			AssertEquals(true, addInfo.Lookups.ZO_ImporterTypeList.ContainsCode(ImporterTypeList.Codes.LLC));
		}
	}
}
