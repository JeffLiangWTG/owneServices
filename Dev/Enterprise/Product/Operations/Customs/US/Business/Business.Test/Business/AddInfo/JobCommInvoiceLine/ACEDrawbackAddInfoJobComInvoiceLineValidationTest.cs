using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEDrawbackAddInfoJobComInvoiceLineValidationTest : CommonDrawbackAddInfoJobComInvoiceLineValidationTest
	{
		public void TestCheckUS_DRWImpActInd()
		{
			InvoiceLine.US_DRWImpActInd = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImpActIndInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImpActIndInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImpActInd();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImpActIndInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWImpActInd = "~";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImpActIndInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImpActIndInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.Destroyed;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImpActIndInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DRWClaimBasis()
		{
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.US_DRWClaimBasis = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWClaimBasisInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.ClaimBasisRequired);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWClaimBasisInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWClaimBasis = "~";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWClaimBasisInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.ClaimBasisRequired);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWClaimBasisInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.Manufactured;
			InvoiceLine.US_DRWClaimBasis = ZString.Empty;
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWClaimBasisInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.ClaimBasisRequired);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWClaimBasisInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWClaimBasis = ACEDrawbackClaimBasisList.Codes._01;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWClaimBasisInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.ClaimBasisRequired);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWClaimBasisInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DRWImportQuantity()
		{
			InvoiceLine.US_DRWImportQuantity = ZDecimal.Zero;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportQuantity();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImportQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWImportQuantity = 100m;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportQuantityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWImportUQ()
		{
			InvoiceLine.US_DRWImportUQ = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportUQ();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImportUQInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWImportUQ = "XX";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImportUQInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWImportUQ = "KG";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DRWValuePerUQ()
		{
			InvoiceLine.US_DRWValuePerUQ = ZDecimal.Zero;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWValuePerUQInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWValuePerUQ();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWValuePerUQInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWValuePerUQ = 100m;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWValuePerUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWValuePerUQMisMatch()
		{
			InvoiceLine.US_DRWClaimAmountOverriden_New = true;
			InvoiceLine.DRWImportQuantity = 200m;
			InvoiceLine.DeclaredVFD = 400m;
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWValuePerUQ();
			var message = "doesn't match Entered Value/Import Quantity";
			AssertNoWarningContaining(InvoiceLine.US_DRWValuePerUQInfo, message);
			InvoiceLine.DRWGoodsValuePerUQ = 1m;
			AssertHasWarningContaining(InvoiceLine.US_DRWValuePerUQInfo, message);
			AssertHasWarning(InvoiceLine.US_DRWValuePerUQInfo, InvoiceLine.ValuePerUQNotMatchNotification);
		}

		public void TestCheckUS_DRWQuantityUsed()
		{
			InvoiceLine.US_DRWQuantityUsed = ZDecimal.Zero;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWQuantityUsedInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForManufacturerSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWQuantityUsed();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWQuantityUsedInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWQuantityUsed = 100m;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWQuantityUsedInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWUQUsed()
		{
			InvoiceLine.US_DRWUQUsed = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWUQUsedInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWUQUsedInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWIsForManufacturerSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWUQUsed();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWUQUsedInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWUQUsed = "XX";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWUQUsedInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWUQUsedInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWUQUsed = "KG";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWUQUsedInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DRWDateOfManufacture()
		{
			InvoiceLine.US_DRWDateOfManufacture = ZDateTime.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateOfManufactureInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForManufacturerSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateOfManufacture();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateOfManufactureInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWDateOfManufacture = ZDateTime.Today;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateOfManufactureInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWFactoryLocation()
		{
			InvoiceLine.US_DRWFactoryLocation = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWFactoryLocationInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForManufacturerSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWFactoryLocation();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWFactoryLocationInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWFactoryLocation = "XX";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWFactoryLocationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWDescrManufactured()
		{
			InvoiceLine.US_DRWDescrManufactured = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDescrManufacturedInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForManufacturerSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDescrManufactured();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDescrManufacturedInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWDescrManufactured = "XX";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDescrManufacturedInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWManufRuleNo()
		{
			InvoiceLine.US_DRWMafActInd = ACEDrawbackActionCodeList.Codes.Manufactured;
			InvoiceLine.US_DRWManufRuleNo = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWManufRuleNoInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForManufacturerSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWManufRuleNo();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWManufRuleNoInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWManufRuleNo = "XX";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWManufRuleNoInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWImpManufRuleNo = "YY";
			AssertNoMessageError(InvoiceLine.US_DRWManufRuleNoInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DuplicateManufRuleNo);
			InvoiceLine.US_DRWManufRuleNo = "YY";
			AssertHasMessageError(InvoiceLine.US_DRWManufRuleNoInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DuplicateManufRuleNo);
		}

		public void TestCheckUS_DRWExportQuantity()
		{
			InvoiceLine.US_DRWExportQuantity = ZDecimal.Zero;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForExportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportQuantity();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportQuantityInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWExportQuantity = 1m;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportQuantityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWExportQuantityForCalculatedAmount()
		{
			AssertEquals(0.0m, InvoiceLine.AdjClaimHMF);
			AssertEquals(0.0m, InvoiceLine.AdjClaimMPF);
			AssertEquals(0.0m, InvoiceLine.AdjClaimTax);
			AssertEquals(0.0m, InvoiceLine.AdjClaimDuty);
			AssertEquals(0.0m, InvoiceLine.ClaimedDuty);
			AssertEquals(0.0m, InvoiceLine.ClaimedHMF);
			AssertEquals(0.0m, InvoiceLine.ClaimedMPF);
			AssertEquals(0.0m, InvoiceLine.ClaimedTax);
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportQuantity();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportQuantityInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.CalculatedAmountShouldBeGreaterThanZero);
			var oneFee = InvoiceLine.DrawbackOtherFees.AddNew();
			oneFee.US_FeeType = Core.Constants.USCustoms.FeeCodes.Wines;
			var fee = InvoiceLine.DrawbackOtherFees[0];
			AssertEquals(0m, fee.ClaimedAmount);
			AssertEquals(0m, fee.AdjClaimAmount);
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportQuantity();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportQuantityInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.CalculatedAmountShouldBeGreaterThanZero);
			var twoFee = InvoiceLine.DrawbackOtherFees.AddNew();
			twoFee.US_FeeType = Core.Constants.USCustoms.FeeCodes.Cotton;
			twoFee.US_FeeAmount = 1m;
			twoFee.US_CalculatedAmount = 0.90m;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportQuantity();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportQuantityInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.CalculatedAmountShouldBeGreaterThanZero);
			InvoiceLine.DrawbackOtherFees.RemoveAll();
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportQuantity();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportQuantityInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.CalculatedAmountShouldBeGreaterThanZero);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.US_DRWClaimAmountOverriden_New = true;
			InvoiceLine.DRWImportQuantity = 1m;
			InvoiceLine.DRWExportQuantity = 1m;
			InvoiceLine.DeclaredTax = 200m;
			AssertEquals("Calculated tax amount", 200m, InvoiceLine.ClaimedTax);
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportQuantity();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportQuantityInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.CalculatedAmountShouldBeGreaterThanZero);
		}

		public void TestCheckUS_DRWExportUQ()
		{
			InvoiceLine.US_DRWExportUQ = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportUQInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForExportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportUQ();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportUQInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWExportUQ = "XX";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportUQInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWExportUQ = "KG";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DRWExportID()
		{
			InvoiceLine.US_DRWExportID = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportIDInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForExportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportID();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportIDInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWExportID = "XX";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWExportDest()
		{
			InvoiceLine.US_DRWExportDest = "XX";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDestInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
			InvoiceLine.US_DRWIsForExportSection = true;
			InvoiceLine.US_DRWExportAction = "D";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportDest();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportDestInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
			InvoiceLine.US_DRWExportAction = "E";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExportDest();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDestInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
			InvoiceLine.US_DRWExportAction = "D";
			InvoiceLine.US_DRWExportDest = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDestInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
		}

		public void TestCheckUS_DRWExpBOLInd()
		{
			InvoiceLine.US_DRWExpBOLInd = true;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExpBOLIndInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
			InvoiceLine.US_DRWIsForExportSection = true;
			InvoiceLine.US_DRWExportAction = "D";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExpBOLInd();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExpBOLIndInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
			InvoiceLine.US_DRWExportAction = "E";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExpBOLInd();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExpBOLIndInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
			InvoiceLine.US_DRWExportAction = "D";
			InvoiceLine.US_DRWExpBOLInd = false;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExpBOLIndInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
		}

		public void TestCheckUS_DRWExpBOLCarrier()
		{
			InvoiceLine.US_DRWExpBOLCarrier = "XX";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExpBOLCarrierInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
			AssertNoMessageError(InvoiceLine.US_DRWExpBOLCarrierInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.BOLCarrierCodeRequired);
			InvoiceLine.US_DRWIsForExportSection = true;
			InvoiceLine.US_DRWExportAction = "D";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExpBOLCarrier();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExpBOLCarrierInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
			AssertNoMessageError(InvoiceLine.US_DRWExpBOLCarrierInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.BOLCarrierCodeRequired);
			InvoiceLine.US_DRWExportAction = "E";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExpBOLCarrier();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExpBOLCarrierInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
			AssertNoMessageError(InvoiceLine.US_DRWExpBOLCarrierInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.BOLCarrierCodeRequired);
			InvoiceLine.US_DRWExportAction = "D";
			InvoiceLine.US_DRWExpBOLCarrier = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExpBOLCarrierInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.FieldIrrelevantForDestroy);
			AssertNoMessageError(InvoiceLine.US_DRWExpBOLCarrierInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.BOLCarrierCodeRequired);
			InvoiceLine.US_DRWExpBOLInd = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWExpBOLCarrier();
			AssertHasMessageError(InvoiceLine.US_DRWExpBOLCarrierInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.BOLCarrierCodeRequired);
			InvoiceLine.US_DRWExpBOLCarrier = "XX";
			AssertNoMessageError(InvoiceLine.US_DRWExpBOLCarrierInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.BOLCarrierCodeRequired);
		}

		public void TestCheckUS_DRWImportEntryLine()
		{
			InvoiceLine.US_ImportEntryNo = "SV934298738";
			InvoiceLine.US_DRWImportEntryLine = ZInt.Zero;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportEntryLine();
			AssertHasWarningContaining(InvoiceLine.US_DRWImportEntryLineInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.YouHaveNotEnteredEntryLineNumber);
			InvoiceLine.US_ImportEntryNo = "";
			InvoiceLine.US_DRWImportEntryLine = ZInt.Zero;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportEntryLine();
			AssertNoWarningContaining(InvoiceLine.US_DRWImportEntryLineInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.YouHaveNotEnteredEntryLineNumber);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportEntryLine();
			AssertHasMessageError(InvoiceLine.US_DRWImportEntryLineInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.LineNumberRequired);
			InvoiceLine.Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._58;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportEntryLine();
			AssertHasMessageError(InvoiceLine.US_DRWImportEntryLineInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.LineNumberRequired);
			InvoiceLine.Declaration.US_EntryType = "77";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportEntryLine();
			AssertHasMessageError(InvoiceLine.US_DRWImportEntryLineInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.LineNumberRequired);
			InvoiceLine.US_DRWImportEntryLine = 1;
			AssertNoMessageError(InvoiceLine.US_DRWImportEntryLineInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.LineNumberRequired);
			AssertNoMessageError(InvoiceLine.US_DRWImportEntryLineInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.LineNumberNotRequired);
			InvoiceLine.US_DRWImportEntryLine = ZInt.Zero;
			AssertHasMessageError(InvoiceLine.US_DRWImportEntryLineInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.LineNumberRequired);
			InvoiceLine.Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._57;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportEntryLine();
			AssertNoMessageError(InvoiceLine.US_DRWImportEntryLineInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.LineNumberRequired);
			InvoiceLine.US_DRWImportEntryLine = 1;
			AssertHasMessageError(InvoiceLine.US_DRWImportEntryLineInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.LineNumberNotRequired);
		}

		public void TestManufacturedRecordRequired()
		{
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.Destroyed;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWIsForManufacturerSectionInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.ManufacturedRequired);
			InvoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.Manufactured;
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWIsForManufacturerSectionInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.ManufacturedRequired);
			InvoiceLine.US_DRWIsForManufacturerSection = true;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWIsForManufacturerSectionInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.ManufacturedRequired);
			InvoiceLine.US_DRWIsForManufacturerSection = false;
			InvoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.ManuAndTrans;
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWIsForManufacturerSectionInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.ManufacturedRequired);
			InvoiceLine.US_DRWIsForManufacturerSection = true;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWIsForManufacturerSectionInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.ManufacturedRequired);
		}

		public void TestCheckUS_DRWImpManufRuleNo()
		{
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.Destroyed;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImpManufRuleNo();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImpManufRuleNoInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.Manufactured;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImpManufRuleNo();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImpManufRuleNoInfo, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImpManufRuleNo();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImpManufRuleNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWDateRcvFrom()
		{
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateRcvFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(InvoiceLine.US_DRWDateRcvFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredForTFTEAAnd1313A);
			InvoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.Manufactured;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateRcvFrom();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(InvoiceLine.US_DRWDateRcvFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredForTFTEAAnd1313A);
			InvoiceLine.US_DRWDateRcvFrom = ZDateTime.Today;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(InvoiceLine.US_DRWDateRcvFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredForTFTEAAnd1313A);
			InvoiceLine.Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateRcvFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(InvoiceLine.US_DRWDateRcvFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredForTFTEAAnd1313A);
			InvoiceLine.Declaration.US_EntryType = "77";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateRcvFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(InvoiceLine.US_DRWDateRcvFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredForTFTEAAnd1313A);
			InvoiceLine.Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateRcvFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(InvoiceLine.US_DRWDateRcvFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredForTFTEAAnd1313A);
			InvoiceLine.Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._02;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateRcvFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateRcvFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(InvoiceLine.US_DRWDateRcvFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredForTFTEAAnd1313A);
		}

		public void TestCheckUS_DRWDateUsedFrom()
		{
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateUsedFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(InvoiceLine.US_DRWDateUsedFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredFor1313A);
			InvoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.Manufactured;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateUsedFrom();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(InvoiceLine.US_DRWDateUsedFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredFor1313A);
			InvoiceLine.US_DRWDateUsedFrom = ZDateTime.Today;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(InvoiceLine.US_DRWDateUsedFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredFor1313A);
			InvoiceLine.Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateUsedFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(InvoiceLine.US_DRWDateUsedFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredFor1313A);
			InvoiceLine.Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._02;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWDateUsedFrom();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWDateUsedFromInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(InvoiceLine.US_DRWDateUsedFromInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.DateNotRequiredFor1313A);
		}

		public void TestCheckUS_DRWAccMethod()
		{
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWAccMethod();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWAccMethodInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWAccMethodInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.US_DRWAccMethod = ZString.Empty;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWAccMethod();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWAccMethodInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWAccMethodInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWAccMethod = "~";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWAccMethodInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWAccMethodInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			InvoiceLine.US_DRWAccMethod = DrawbackAccountingMethodCodeList.Codes._01;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWAccMethodInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWAccMethodInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			InvoiceLine.US_DRWAccMethod = DrawbackAccountingMethodCodeList.Codes._00;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWAccMethodInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWAccMethodInfo, ListValidation.InvalidCodeMessageError);
		}

		public override void TestCheckUS_ImportEntryNo()
		{
			base.TestCheckUS_ImportEntryNo();
			InvoiceLine.US_ImportEntryNo = "SV912345678";
			AssertNoMessageError(InvoiceLine.US_ImportEntryNoInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.EntryNumberNotRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._07;
			InvoiceLine.AddInfoValidation.ValidateUS_ImportEntryNo();
			AssertHasMessageError(InvoiceLine.US_ImportEntryNoInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.EntryNumberNotRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			InvoiceLine.AddInfoValidation.ValidateUS_ImportEntryNo();
			AssertNoMessageError(InvoiceLine.US_ImportEntryNoInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.EntryNumberNotRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._57;
			InvoiceLine.AddInfoValidation.ValidateUS_ImportEntryNo();
			AssertHasMessageError(InvoiceLine.US_ImportEntryNoInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.EntryNumberNotRequired);
		}

		public void TestCheckUS_DRWImportQuantity2()
		{
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.DRWImportUQ2 = "KG";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportQuantity2();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportQuantity2Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportQuantity2();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImportQuantity2Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.DRWImportQuantity2 = 10m;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportQuantity2Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWImportUQ2()
		{
			InvoiceLine.DRWImportUQ2 = "~";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImportUQ2Info, ListValidation.InvalidCodeMessageError);
			InvoiceLine.DRWImportUQ2 = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQ2Info, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.DRWImportQuantity2 = 10m;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportUQ2();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQ2Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportUQ2();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImportUQ2Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.DRWImportUQ2 = "KG";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQ2Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQ2Info, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DRWValuePerUQ2()
		{
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.US_DRWDeclaredVFD = 0;
			InvoiceLine.DRWImportQuantity2 = 10m;
			InvoiceLine.US_DRWValuePerUQ2 = ZDecimal.Zero;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWValuePerUQ2Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWValuePerUQ2();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWValuePerUQ2Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.DRWGoodsValuePerUQ2 = 100m;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWValuePerUQ2Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWImportQuantity3()
		{
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.DRWImportUQ3 = "KG";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportQuantity3();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportQuantity3Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportQuantity3();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImportQuantity3Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.DRWImportQuantity3 = 10m;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportQuantity3Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWImportUQ3()
		{
			InvoiceLine.DRWImportUQ3 = "~";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImportUQ3Info, ListValidation.InvalidCodeMessageError);
			InvoiceLine.DRWImportUQ3 = ZString.Empty;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQ3Info, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.DRWImportQuantity3 = 10m;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportUQ3();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQ3Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWImportUQ3();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWImportUQ3Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.DRWImportUQ3 = "KG";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQ3Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWImportUQ3Info, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DRWValuePerUQ3()
		{
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.US_DRWDeclaredVFD = 0;
			InvoiceLine.DRWImportQuantity3 = 10m;
			InvoiceLine.US_DRWValuePerUQ3 = ZDecimal.Zero;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWValuePerUQ3Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWValuePerUQ3();
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWValuePerUQ3Info, MandatoryValidation.YouHaveNotEntered);
			InvoiceLine.DRWGoodsValuePerUQ3 = 100m;
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWValuePerUQ3Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWSubstituted()
		{
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted();
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.US_DRWClaimAmountOverriden_New = true;
			InvoiceLine.DRWImportQuantity = 100m;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted();
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._52;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted();
			AssertHasMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			declaration.US_EntryType = "77";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted();
			AssertHasMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			InvoiceLine.SubstitutedValuePerUnit = 100m;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted();
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted();
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertHasMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			InvoiceLine.SubstitutedValuePerUnit = 0m;
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstitutedInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
		}

		public void TestCheckUS_DRWSubstituted2()
		{
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted2();
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.US_DRWClaimAmountOverriden_New = true;
			InvoiceLine.DRWImportQuantity2 = 100m;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted2();
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._52;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted2();
			AssertHasMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			declaration.US_EntryType = "77";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted2();
			AssertHasMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			InvoiceLine.SubstitutedValuePerUnit2 = 100m;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted2();
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted2();
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertHasMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			InvoiceLine.SubstitutedValuePerUnit2 = 0m;
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
		}

		public void TestCheckUS_DRWSubstituted3()
		{
			InvoiceLine.US_DRWIsForImportSection = false;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted3();
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.US_DRWClaimAmountOverriden_New = true;
			InvoiceLine.DRWImportQuantity3 = 100m;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted3();
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._52;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted3();
			AssertHasMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			declaration.US_EntryType = "77";
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted3();
			AssertHasMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			InvoiceLine.SubstitutedValuePerUnit3 = 100m;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted3();
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			InvoiceLine.AddInfoValidation.ValidateUS_DRWSubstituted3();
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertHasMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			InvoiceLine.SubstitutedValuePerUnit3 = 0m;
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(InvoiceLine.US_DRWSubstituted3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
		}

		public void TestUS_DRWExportDateGreaterThanClaimDateWhenTFTEA()
		{
			MakeExportLine();
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			declaration.US_EstimatedEntryDate = ZDateTime.Now;
			InvoiceLine.US_DRWExportDate = ZDateTime.Now.AddYears(-6);
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ExportDateGreaterThanClaimDateWhenTFTEA);
			InvoiceLine.US_DRWExportDate = ZDateTime.Now.AddYears(-4);
			InvoiceLine.AddInfoValidation.ValidateUS_EstimatedEntryDate();
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDateInfo, CommonDrawbackAddInfoJobComInvoiceLineValidation.ExportDateGreaterThanClaimDateWhenTFTEA);
		}

		public void TestACEDrawbackTrackingNumberLineOverflow()
		{
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._52;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForManufacturerSection = true;
			((IACEDrawbackTrackingNumberLine)invoiceLine).ArrangeTrackingNumbers(12345);
			AssertEquals("12346", invoiceLine.US_DRWImpTrkID);
			AssertEquals("62346", invoiceLine.US_DRWMafTrkID);
			invoiceLine.AddInfoValidation.ValidateUS_DRWImpTrkID();
			AssertNoMessageErrorContaining(invoiceLine.US_DRWImpTrkIDInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.MaxImportTrackingNumberOverflow);
			((IACEDrawbackTrackingNumberLine)invoiceLine).ArrangeTrackingNumbers(49998);
			AssertEquals("49999", invoiceLine.US_DRWImpTrkID);
			AssertEquals("99999", invoiceLine.US_DRWMafTrkID);
			invoiceLine.AddInfoValidation.ValidateUS_DRWImpTrkID();
			AssertNoMessageErrorContaining(invoiceLine.US_DRWImpTrkIDInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.MaxImportTrackingNumberOverflow);
			invoiceLine.US_DRWImpTrkID = "50000";
			AssertHasMessageErrorContaining(invoiceLine.US_DRWImpTrkIDInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.MaxImportTrackingNumberOverflow);
			invoiceLine.US_DRWImpTrkID = "5A00";
			AssertNoMessageErrorContaining(invoiceLine.US_DRWImpTrkIDInfo, ACEDrawbackAddInfoJobComInvoiceLineValidation.MaxImportTrackingNumberOverflow);
		}

		public void TestUSDRWExportDestErrorWherSetToOuterSpace()
		{
			InvoiceLine.US_DRWExportDest = "FN";
			AssertNoMessageErrorContaining(InvoiceLine.US_DRWExportDestInfo, ListValidation.InvalidCodeMessageError);
			InvoiceLine.US_DRWExportDest = "XQ";
			AssertHasMessageErrorContaining(InvoiceLine.US_DRWExportDestInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
		}
	}
}
