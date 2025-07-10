using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_DestinationState()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_DestinationState = "~";
			AssertHasMessageError(declaration.US_DestinationStateInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);
			declaration.US_DestinationState = USStatesList.Codes.Alabama;
			AssertNoMessageError(declaration.US_DestinationStateInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);
		}

		public void TestDestinationStateIsNotValidatedForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_DestinationState = "XX";
			AssertNoNotifications(declaration.US_DestinationStateInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_DestinationState = "YY";
			AssertHasNotifications(declaration.US_DestinationStateInfo);
		}

		public void TestCheckUS_InBondTOLState()
		{
			addInfoDummy.US_InBondTOLState = "~";
			AssertHasMessageError(addInfoDummy.US_InBondTOLStateInfo, USAddInfoValidation.InBondTOLStateShouldBeInList);
			addInfoDummy.US_InBondTOLState = USStatesList.Codes.Alabama;
			AssertNoMessageError(addInfoDummy.US_InBondTOLStateInfo, USAddInfoValidation.InBondTOLStateShouldBeInList);
		}

		public void TestCheckUS_InBondExportTransMode()
		{
			addInfoDummy.US_InBondExportTransMode = "~";
			AssertHasMessageError(addInfoDummy.US_InBondExportTransModeInfo, USAddInfoValidation.InBondExportTransModeShouldBeInList);
			addInfoDummy.US_InBondExportTransMode = addInfoDummy.Lookups.ExportingTransportTypeList[0].Code;
			AssertNoMessageError(addInfoDummy.US_InBondExportTransModeInfo, USAddInfoValidation.InBondExportTransModeShouldBeInList);
		}

		public void TestCheckUS_SecondarySPI()
		{
			var message = "Please enter a valid Secondary SPI Code. The code you have selected is not in the Secondary SPI codes List.";
			addInfoDummy.US_SecondarySPI = "~";
			AssertHasMessageError(addInfoDummy.US_SecondarySPIInfo, message);
			addInfoDummy.US_SecondarySPI = addInfoDummy.Lookups.ProductClaimList[0].Code;
			AssertNoMessageError(addInfoDummy.US_SecondarySPIInfo, message);
		}

		public void TestCheckUS_PercentageActiveIngredient()
		{
			ClassificationValidatorHelper.CheckPercentageActiveIngredient(addInfoDummy.US_PercentageActiveIngredientInfo);
		}

		public void TestUS_PrivilegedStatusDate()
		{
			addInfoDummy.US_PrivilegedStatusDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrors(addInfoDummy.US_PrivilegedStatusDateInfo);
		}

		public void TestCountryOfExportRestriction()
		{
			addInfoDummy.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Cuba;
			AssertHasMessageError(addInfoDummy.US_UC_NKCountryOfExportInfo, "CUBA is a restricted country");
			addInfoDummy.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			AssertNoMessageError(addInfoDummy.US_UC_NKCountryOfExportInfo, "CUBA is a restricted country");
		}

		public void TestCountryOfOriginRestriction()
		{
			addInfoDummy.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Cuba;
			AssertHasMessageError(addInfoDummy.US_UC_NKCountryOfOriginInfo, "CUBA is a restricted country");
			addInfoDummy.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertNoMessageError(addInfoDummy.US_UC_NKCountryOfOriginInfo, "CUBA is a restricted country");
		}

		public void TestMissingDocs1()
		{
			addInfoDummy.US_MissingDocument1 = MissingDocumentList.Codes.Lots;
			AssertHasMessageError(addInfoDummy.US_MissingDocument1Info, "Record the code number for the first missing document and insert code '99' in Missing Document 2 field, to indicate more than one additional document is missing.");
			addInfoDummy.US_MissingDocument1 = MissingDocumentList.Codes.OAF;
			AssertNoMessageErrors(addInfoDummy.US_MissingDocument1Info);
			addInfoDummy.US_MissingDocument1 = "05";
			AssertHasMessageErrors("Invalid code list error", addInfoDummy.US_MissingDocument1Info);
		}

		public void TestMissingDocs2()
		{
			addInfoDummy.US_MissingDocument2 = MissingDocumentList.Codes.OAF;
			AssertNoMessageErrors(addInfoDummy.US_MissingDocument2Info);
			addInfoDummy.US_MissingDocument2 = "05";
			AssertHasMessageErrors("Invalid code list error", addInfoDummy.US_MissingDocument2Info);
		}

		public void TestCheckUS_UC_NKCountryOfOrigin()
		{
			addInfoDummy.US_UC_NKCountryOfOrigin = USCCountry.Unknown;
			AssertNoMessageErrors(addInfoDummy.US_UC_NKCountryOfOriginInfo);
			addInfoDummy.US_UC_NKCountryOfOrigin = "XX";
			AssertHasMessageErrors(addInfoDummy.US_UC_NKCountryOfOriginInfo);
		}

		public void TestCheckUS_CottonFeeExempt()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CD_CottonFeeExempt = "Z";
			AssertHasMessageErrorContaining(pivot.CD_CottonFeeExemptInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_CottonFeeExempt = YesNoDefaultList.Codes.No;
			AssertNoMessageErrorContaining(pivot.CD_CottonFeeExemptInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(pivot.CD_CottonFeeExemptInfo, ListValidation.InvalidCodeMessageError);
			pivot.CD_CottonFeeExempt = ZString.Empty;
			AssertNoMessageErrorContaining(pivot.CD_CottonFeeExemptInfo, ListValidation.InvalidCodeMessageError);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104220040";
			invoiceLine.US_CottonFeeExempt = "Z";
			AssertHasMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_CottonFeeExempt = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_9802PerUnit_MaxValue()
		{
			addInfoDummy.US_9802PerUnit = 999999999999999.9999m;
			AssertNoErrors(addInfoDummy.US_9802PerUnitInfo);
			addInfoDummy.US_9802PerUnit = 9999999999999999.9999m;
			AssertHasErrors(addInfoDummy.US_9802PerUnitInfo);
		}

		public void TestCheckUS_98InvCurrPerUnit_MaxValue()
		{
			addInfoDummy.US_98InvCurrPerUnit = 999999999999999.9999m;
			AssertNoErrors(addInfoDummy.US_98InvCurrPerUnitInfo);
			addInfoDummy.US_98InvCurrPerUnit = 9999999999999999.9999m;
			AssertHasErrors(addInfoDummy.US_98InvCurrPerUnitInfo);
		}

		public void TestCheckUS_AMMVPerUnit_MaxValue()
		{
			addInfoDummy.US_AMMVPerUnit = 999999999999999.9999m;
			AssertNoErrors(addInfoDummy.US_AMMVPerUnitInfo);
			addInfoDummy.US_AMMVPerUnit = 9999999999999999.9999m;
			AssertHasErrors(addInfoDummy.US_AMMVPerUnitInfo);
		}

		public void TestCheckUS_98GoodsValue_MaxValue()
		{
			addInfoDummy.US_98GoodsValue = 999999999999999.9999m;
			AssertNoErrors(addInfoDummy.US_98GoodsValueInfo);
			addInfoDummy.US_98GoodsValue = 9999999999999999.9999m;
			AssertHasErrors(addInfoDummy.US_98GoodsValueInfo);
		}

		public void TestCheckUS_98ValueInvCurr_MaxValue()
		{
			addInfoDummy.US_98ValueInvCurr = 999999999999999.9999m;
			AssertNoErrors(addInfoDummy.US_98ValueInvCurrInfo);
			addInfoDummy.US_98ValueInvCurr = 9999999999999999.9999m;
			AssertHasErrors(addInfoDummy.US_98ValueInvCurrInfo);
		}

		public void TestCheckUS_PerUnitCost_MaxValue()
		{
			addInfoDummy.US_PerUnitCost = 999999999999999.9999m;
			AssertNoErrors(addInfoDummy.US_PerUnitCostInfo);
			addInfoDummy.US_PerUnitCost = 9999999999999999.9999m;
			AssertHasErrors(addInfoDummy.US_PerUnitCostInfo);
		}

		public void TestCheckUS_TaxRate_MaxValue()
		{
			addInfoDummy.US_TaxRate = 999999999.999999999m;
			AssertNoErrors(addInfoDummy.US_TaxRateInfo);
			addInfoDummy.US_TaxRate = 9999999999.999999999m;
			AssertHasErrors(addInfoDummy.US_TaxRateInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			classification = Factory.New<CusClassification>();
			addInfoDummy = new AddInfoForTest(classification.CC_AddInfoInfo);
		}

		CusClassification classification;
		AddInfoForTest addInfoDummy;
	}
}
