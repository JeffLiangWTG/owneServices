using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobComInvoiceLineCOValidationTest : AddInfoJobComInvoiceLineValidationTest
	{
		protected override string MessageType => MessageTypeCodeList.Codes.COO;

		public void TestSG_CertItemValue()
		{
			Declaration.SG_Cert1Type = "9";
			Validation.ValidateSG_CertItemValue();
			AssertEquals("Cert Item Value is optional for this certificate type", false, AddInfoJobComInvoiceLine.SG_CertItemValueInfo.HasMessageErrors());
			Declaration.SG_Cert1Type = "4";
			AddInfoJobComInvoiceLine.SG_CertItemValue = 35750;
			Validation.ValidateSG_CertItemValue();
			AssertEquals("Cert Item Value should not be entered for this certificate type", true, AddInfoJobComInvoiceLine.SG_CertItemValueInfo.HasMessageErrors());
			Declaration.SG_Cert1Type = "9";
			AddInfoJobComInvoiceLine.SG_CertItemValue = 10;
			Validation.ValidateSG_CertItemValue();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_CertItemValueInfo.HasMessageErrors());
		}

		public void TestOriginCriterion()
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			Declaration.SG_Cert1Type = "1";
			Validation.ValidateSG_CertOriginCriterion1();
			AssertEquals("Origin Criterion details are mandatory for this certificate type", true, AddInfoJobComInvoiceLine.SG_CertOriginCriterion1Info.HasMessageErrors());
			Declaration.SG_Cert1Type = "9";
			Validation.ValidateSG_CertOriginCriterion1();
			AssertEquals("Origin Criterion details are not required for this certificate type", false, AddInfoJobComInvoiceLine.SG_CertOriginCriterion1Info.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_CertOriginCriterion1 = "CWC";
			Validation.ValidateSG_CertOriginCriterion1();
			AssertEquals("Origin Criterion details are not required for this certificate type", true, AddInfoJobComInvoiceLine.SG_CertOriginCriterion1Info.HasMessageErrors());
			Declaration.SG_Cert1Type = "1";
			AddInfoJobComInvoiceLine.SG_CertOriginCriterion1 = "TEST";
			Validation.ValidateSG_CertOriginCriterion1();
			AssertEquals("Origin Criterion details are mandatory for this certificate type", false, AddInfoJobComInvoiceLine.SG_CertOriginCriterion1Info.HasMessageErrors());
		}

		public void TestTextileQuotaQuantity_CO()
		{
			Validation.ValidateSG_TextileQuotaQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityInfo.HasMessageErrors());
			Declaration.SG_Cert1Type = "9";
			Validation.ValidateSG_TextileQuotaQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityInfo.HasMessageErrors());
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			Validation.ValidateSG_TextileQuotaQuantity();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_TextileQuotaQuantity = 100;
			Validation.ValidateSG_TextileQuotaQuantity();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityInfo.HasMessageErrors());
		}

		public void TestTextileUnitCode()
		{
			Validation.ValidateSG_TextileQuotaQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityUnitInfo.HasMessageErrors());
			Declaration.SG_Cert1Type = "9";
			Validation.ValidateSG_TextileQuotaQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityUnitInfo.HasMessageErrors());
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			Validation.ValidateSG_TextileQuotaQuantityUnit();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityUnitInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityUnit = UnitOfQuantityCodeList.Codes.KGM;
			Validation.ValidateSG_TextileQuotaQuantityUnit();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TextileQuotaQuantityUnitInfo.HasMessageErrors());
		}

		public void TestTextileCatCode()
		{
			Validation.ValidateSG_TextileCatCode();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TextileCatCodeInfo.HasMessageErrors());
			Declaration.SG_Cert1Type = "9";
			Validation.ValidateSG_TextileCatCode();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TextileCatCodeInfo.HasMessageErrors());
			Declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			Validation.ValidateSG_TextileCatCode();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_TextileCatCodeInfo.HasMessageErrors());
			AddInfoJobComInvoiceLine.SG_TextileCatCode = "CODE1";
			Validation.ValidateSG_TextileCatCode();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_TextileCatCodeInfo.HasMessageErrors());
		}

		public void TestCheckSG_CertHSCodeForStandAloneCOO()
		{
			Declaration.SG_Cert1Type = "9";
			AddInfoJobComInvoiceLine.SG_CertOriginCriterion1 = OriginCriterionCodeList.Codes.GSPFormA_W;
			Validation.ValidateSG_CertHSCode();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_CertHSCodeInfo.HasMessageError("When using this Origin Criterion, HS Code is required."));
			AddInfoJobComInvoiceLine.SG_CertHSCode = "837012";
			Validation.ValidateSG_CertHSCode();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_CertHSCodeInfo.HasMessageError("When using this Origin Criterion, HS Code is required."));
		}

		public void TestCheckSG_PercContentForStandAloneCOO()
		{
			Declaration.SG_Cert1Type = "16";
			AddInfoJobComInvoiceLine.SG_CertOriginCriterion1 = "ACFTA";
			Validation.ValidateSG_PercContent();
			AssertEquals(true, AddInfoJobComInvoiceLine.SG_PercContentInfo.HasWarning("When using this Origin Criterion, % Content may be required."));
			AddInfoJobComInvoiceLine.SG_PercContent = 45;
			Validation.ValidateSG_PercContent();
			AssertEquals(false, AddInfoJobComInvoiceLine.SG_PercContentInfo.HasWarning("When using this Origin Criterion, % Content may be required."));
		}
	}
}
