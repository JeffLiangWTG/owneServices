using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusUSClassification))]
	sealed class CusUSClassificationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var details = pivot.Details;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertType<CusUSClassificationValidation>(details.Validation);
			pivot.CI_ChildType = "#@2";
			AssertType<CusUSClassificationValidation>(details.Validation);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertType<HTICusUSClassificationValidation>(details.Validation);
			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertType<CusUSClassificationValidation>(details.Validation);
		}

		public void TestCD_SPI()
		{
			var classification = Factory.New<CusUSClassification>();
			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = product.PK.ToString().Replace("-", "");
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			classification.CD_ParentID = pivot.PK;
			classification.CD_SPI = "10";
			classification.CD_SPI = "3";
			AssertEquals("3", pivot.CD_SPI);
			AssertContainsLog(pivot.Logs, "SPI '10' changed to '3'", true);
			AssertContainsLog(pivot.Part.Logs, "SPI '10' changed to '3'", false);
		}

		public void TestCompartibleMaxLength()
		{
			// This test ensures that transformation will not fall. Any changes in US AddInfo Schema must be reflected in CusUSClassification and vice versa.
			AssertEquals(USAddInfoSchema.US_ADDDepositRateIndicator.MaxLength, CusUSClassificationSchema.CD_ADDDepositRateInd.MaxLength);
			AssertEquals(USAddInfoSchema.US_ADDCaseNo.MaxLength, CusUSClassificationSchema.CD_ADDCaseNo.MaxLength);
			AssertEquals(USAddInfoSchema.US_ADCVDStat.MaxLength, CusUSClassificationSchema.CD_ADCVDStat.MaxLength);
			AssertEquals(USAddInfoSchema.US_CVDDepositRateIndicator.MaxLength, CusUSClassificationSchema.CD_CVDDepositRateInd.MaxLength);
			AssertEquals(USAddInfoSchema.US_CVDCaseNo.MaxLength, CusUSClassificationSchema.CD_CVDCaseNo.MaxLength);
			AssertEquals(USAddInfoSchema.US_SecondarySPI.MaxLength, CusUSClassificationSchema.CD_ProductClaim.MaxLength);
			AssertEquals(USAddInfoSchema.US_SPI.MaxLength, CusUSClassificationSchema.CD_SPI.MaxLength);
			AssertEquals(USAddInfoSchema.US_CBTPACertificateNo.MaxLength, CusUSClassificationSchema.CD_CBTPACertificate.MaxLength);
			AssertEquals(USAddInfoSchema.US_CottonCertificateNo.MaxLength, CusUSClassificationSchema.CD_CottonCertificate.MaxLength);
			AssertEquals(USAddInfoSchema.US_CottonFeeExempt.MaxLength, CusUSClassificationSchema.CD_CottonFeeExempt.MaxLength);
			AssertEquals(USAddInfoSchema.US_PIRPRulingNo.MaxLength, CusUSClassificationSchema.CD_RulingNumber.MaxLength);
			AssertEquals(USAddInfoSchema.US_PIRPRulingType.MaxLength, CusUSClassificationSchema.CD_RulingType.MaxLength);
			AssertEquals(USAddInfoSchema.US_WoolLicenceNo.MaxLength, CusUSClassificationSchema.CD_WoolLicenceNo.MaxLength);
			AssertEquals(USAddInfoSchema.US_CAExportCertificate.MaxLength, CusUSClassificationSchema.CD_SugarCertificate.MaxLength);
			AssertEquals(USAddInfoSchema.US_AgricultureLicNo.MaxLength, CusUSClassificationSchema.CD_AgricultureLicenceNo.MaxLength);
			AssertEquals(USAddInfoSchema.US_ZoneStatus.MaxLength, CusUSClassificationSchema.CD_ZoneStatus.MaxLength);
			AssertEquals(USAddInfoSchema.US_TSCAIndicator.MaxLength, CusUSClassificationSchema.CD_TSCAIndicator.MaxLength);
			AssertEquals(USAddInfoSchema.US_OtherReconIndicator.MaxLength, CusUSClassificationSchema.CD_ReconIssue.MaxLength);
			AssertEquals(USAddInfoSchema.US_UC_NKCountryOfExport.MaxLength, CusUSClassificationSchema.CD_UC_NKCountryOfExport.MaxLength);
			AssertEquals(USAddInfoSchema.US_UC_NKCountryOfOrigin.MaxLength, CusUSClassificationSchema.CD_UC_NKCountryOfOrigin.MaxLength);
			AssertEquals(USAddInfoSchema.US_TaxCode.MaxLength, CusUSClassificationSchema.CD_TaxCode.MaxLength);
			AssertEquals(USAddInfoSchema.US_TaxRateS.MaxLength, CusUSClassificationSchema.CD_TaxRateDesc.MaxLength);
			AssertEquals(USAddInfoSchema.US_TaxRateT.MaxLength, CusUSClassificationSchema.CD_TaxRateType.MaxLength);
			AssertEquals(USAddInfoSchema.US_RX_NK98InvCurrPerUnitCurr.MaxLength, CusUSClassificationSchema.CD_RX_NK9802ValuePerUnitCurr.MaxLength);
			AssertEquals(USAddInfoSchema.US_RX_NKPerUnitCostCurr.MaxLength, CusUSClassificationSchema.CD_RX_NKPerUnitCostCurr.MaxLength);
			AssertEquals(USAddInfoSchema.US_WeightUQ.MaxLength, CusUSClassificationSchema.CD_WeightUQ.MaxLength + 1);
			AssertEquals(USAddInfoSchema.US_AESOriginIndicator.MaxLength, CusUSClassificationSchema.CD_OriginIndicator.MaxLength);
			AssertEquals(USAddInfoSchema.US_ECCN.MaxLength, CusUSClassificationSchema.CD_ECCN.MaxLength);
			AssertEquals(USAddInfoSchema.US_ExportCode.MaxLength, CusUSClassificationSchema.CD_ExportCode.MaxLength);
			AssertEquals(USAddInfoSchema.US_LicenseType.MaxLength, CusUSClassificationSchema.CD_LicenceType.MaxLength);
			AssertEquals(USAddInfoSchema.US_DDTCITARExemptionNo.MaxLength, CusUSClassificationSchema.CD_ITARExemptionNo.MaxLength);
			AssertEquals(USAddInfoSchema.US_DDTCMilitaryEquipmentIndicator.MaxLength, CusUSClassificationSchema.CD_MilitaryEquipInd.MaxLength);
			AssertEquals(USAddInfoSchema.US_JurisdictionNumber.MaxLength, CusUSClassificationSchema.CD_DDTCJurisdictionNumber.MaxLength);
			AssertEquals(USAddInfoSchema.US_DDTCRegistrationNo.MaxLength, CusUSClassificationSchema.CD_DDTCRegoNo.MaxLength);
			AssertEquals(USAddInfoSchema.US_DDTCUSMLCategoryCode.MaxLength, CusUSClassificationSchema.CD_DDTCUSMLCategoryCode.MaxLength);
			AssertEquals(USAddInfoSchema.US_DDTCUnit.MaxLength, CusUSClassificationSchema.CD_DDTCUnit.MaxLength);
			AssertEquals(USAddInfoSchema.US_TaxApply.MaxLength, CusUSClassificationSchema.CD_TaxApplicability.MaxLength);
			AssertEquals(USAddInfoSchema.US_NAFTAReconIndicator.MaxLength, CusUSClassificationSchema.CD_NAFTARecon.MaxLength);
			AssertEquals(USAddInfoSchema.US_ADDDecID.MaxLength, CusUSClassificationSchema.CD_ADDDecID.MaxLength);
			AssertEquals(USAddInfoSchema.US_DDTCPartyCertificationIndicator.MaxLength, CusUSClassificationSchema.CD_PartyCertInd.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var lookup = Factory.NewWithValidTestData<CusClassification>();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;
			return pivot.Details;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		void AssertContainsLog(Logs logs, string expected, bool result)
		{
			var hasLog = logs.GetAllLogs().ToList().Exists(o => expected == (o as StmALog).SL_Reference);
			AssertEquals(result, hasLog);
		}
	}
}
