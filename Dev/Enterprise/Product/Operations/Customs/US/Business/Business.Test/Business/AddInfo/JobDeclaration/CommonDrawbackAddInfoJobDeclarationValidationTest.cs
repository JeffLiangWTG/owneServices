using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class CommonDrawbackAddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_EntryTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "Drawback Provision Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "01", "1313(A) - Direct Identification Manufacturing Drawback (Articles made from imported merchandise)", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "75", "TFTEA 1313(b) - SOUGHT CHEMICALS TFTEA Substitution Manufacturing Drawback SOUGHT CHEMICALS", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();
			var message = "Please enter a valid Entry Type Code";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = ZString.Empty;
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EntryType = "99";
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.US_EntryType = "01";
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, "Only TFTEA Drawback Provisions are allowed.");
			declaration.US_EntryType = "75";
			AssertNoMessageError(declaration.US_EntryTypeInfo, "Only TFTEA Drawback Provisions are allowed.");
		}

		public void TestCheckUS_ClaimPort()
		{
			declaration.US_ClaimPort = ZString.Empty;
			declaration.AddInfoValidation.ValidateUS_ClaimPort();
			AssertHasMessageErrorContaining(declaration.US_ClaimPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_ClaimPortInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_ClaimPort = "9999";
			AssertNoMessageErrorContaining(declaration.US_ClaimPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_ClaimPortInfo, ListValidation.InvalidCodeMessageError);
			foreach (ZString claimPort in declaration.AddInfoLookups.ValidClaimPortTeamNos.Keys)
			{
				declaration.US_ClaimPort = claimPort;
				AssertNoMessageErrorContaining(declaration.US_ClaimPortInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.US_ClaimPortInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_TeamNo()
		{
			declaration.US_TeamNo = ZString.Empty;
			declaration.AddInfoValidation.ValidateUS_TeamNo();
			AssertHasMessageErrorContaining(declaration.US_TeamNoInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_TeamNoInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_TeamNo = "999";
			AssertNoMessageErrorContaining(declaration.US_TeamNoInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_TeamNoInfo, ListValidation.InvalidCodeMessageError);
			foreach (ZString teamNo in declaration.AddInfoLookups.ValidClaimPortTeamNos.Values)
			{
				declaration.US_TeamNo = teamNo;
				AssertNoMessageErrorContaining(declaration.US_TeamNoInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.US_TeamNoInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestValidateClaimPortAndTeamNo()
		{
			RunOneValidateClaimPortAndTeamNoTest(ZString.Empty, ZString.Empty, false);
			RunOneValidateClaimPortAndTeamNoTest("1001", ZString.Empty, false);
			RunOneValidateClaimPortAndTeamNoTest(ZString.Empty, "2DB", false);
			RunOneValidateClaimPortAndTeamNoTest("1001", "2DB", false);
			RunOneValidateClaimPortAndTeamNoTest("1001", "3DR", true);
			RunOneValidateClaimPortAndTeamNoTest("3901", "2DB", true);
			RunOneValidateClaimPortAndTeamNoTest("9999", "2DB", true);
		}

		public virtual void TestCheckUS_BondType()
		{
			declaration.US_AcceleratedClaimInd = false;
			declaration.US_ExporterSummaryInd = false;
			declaration.US_BondType = "";
			declaration.AddInfoValidation.ValidateUS_BondType();
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType = "X";
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_AcceleratedClaimInd = true;
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, CommonDrawbackAddInfoJobDeclarationValidation.BondTypeRequiredMessage);
			declaration.US_AcceleratedClaimInd = false;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, CommonDrawbackAddInfoJobDeclarationValidation.BondTypeRequiredMessage);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_ExporterSummaryInd = true;
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, CommonDrawbackAddInfoJobDeclarationValidation.BondTypeRequiredMessage);
			declaration.US_ExporterSummaryInd = false;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, CommonDrawbackAddInfoJobDeclarationValidation.BondTypeRequiredMessage);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_AcceleratedClaimInd = true;
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, CommonDrawbackAddInfoJobDeclarationValidation.BondTypeRequiredMessage);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, CommonDrawbackAddInfoJobDeclarationValidation.BondTypeRequiredMessage);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, CommonDrawbackAddInfoJobDeclarationValidation.BondTypeRequiredMessage);
		}

		public void TestCheckUS_SuretyCode()
		{
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_SuretyCode = "123";
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_SuretyCode = "123";
			AssertHasMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_SuretyCode = "1Z3";
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.DoNotEntered);
			AssertHasMessageErrorContaining(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_SuretyCode = "1Z3";
			AssertHasMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_SuretyCode = "1Z3";
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.DoNotEntered);
			AssertHasMessageErrorContaining(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_SuretyCode = "";
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
		}

		[TestDate(2008, 4, 1)]
		public void TestCheckUS_EstimatedEntryDate()
		{
			declaration.US_EstimatedEntryDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(declaration.US_EstimatedEntryDateInfo, CommonDrawbackAddInfoJobDeclarationValidation.EstimatedClaimDateMessage);
			declaration.US_EstimatedEntryDate = ZDateTime.Now.AddDays(-31);
			AssertHasMessageErrorContaining(declaration.US_EstimatedEntryDateInfo, CommonDrawbackAddInfoJobDeclarationValidation.EstimatedClaimDateMessage);
			declaration.US_EstimatedEntryDate = ZDateTime.Now.AddDays(-29);
			AssertNoMessageErrorContaining(declaration.US_EstimatedEntryDateInfo, CommonDrawbackAddInfoJobDeclarationValidation.EstimatedClaimDateMessage);
		}

		public void TestCheckUS_EarliestExportDate()
		{
			declaration.US_EstimatedEntryDate = new ZDateTime(2008, 4, 1);
			declaration.US_EarliestExportDate = new ZDateTime(2005, 3, 29);
			AssertNoMessageErrorContaining(declaration.US_EarliestExportDateInfo, ACSDrawbackAddInfoJobDeclarationValidation.EarliestExportDateMessage);
			declaration.US_EstimatedEntryDate = new ZDateTime(2010, 3, 25);
			AssertNoMessageErrorContaining(declaration.US_EarliestExportDateInfo, ACSDrawbackAddInfoJobDeclarationValidation.EarliestExportDateMessage);
			declaration.US_EarliestExportDate = new ZDateTime(2005, 3, 20);
			AssertHasMessageErrorContaining(declaration.US_EarliestExportDateInfo, ACSDrawbackAddInfoJobDeclarationValidation.EarliestExportDateMessage);
			declaration.US_EarliestExportDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(declaration.US_EarliestExportDateInfo, ACSDrawbackAddInfoJobDeclarationValidation.EarliestExportDateMessage);
		}

		public void TestCheckUS_DRWDatePeriod()
		{
			declaration.US_DRWDatePeriodFrom = ZDateTime.Empty;
			AssertNoMessageErrorContaining(declaration.US_DRWDatePeriodFromInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			AssertNoMessageErrorContaining(declaration.US_DRWDatePeriodToInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			declaration.US_DRWDatePeriodFrom = new ZDateTime(2008, 11, 30);
			AssertHasMessageErrorContaining(declaration.US_DRWDatePeriodFromInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			AssertHasMessageErrorContaining(declaration.US_DRWDatePeriodToInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			declaration.US_DRWDatePeriodTo = new ZDateTime(2008, 12, 1);
			AssertNoMessageErrorContaining(declaration.US_DRWDatePeriodFromInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			AssertNoMessageErrorContaining(declaration.US_DRWDatePeriodToInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			declaration.US_DRWDatePeriodFrom = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.US_DRWDatePeriodFromInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			AssertHasMessageErrorContaining(declaration.US_DRWDatePeriodToInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			declaration.US_DRWDatePeriodFrom = new ZDateTime(2008, 12, 2);
			AssertHasMessageErrorContaining(declaration.US_DRWDatePeriodFromInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			AssertHasMessageErrorContaining(declaration.US_DRWDatePeriodToInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			declaration.US_DRWDatePeriodFrom = new ZDateTime(2008, 11, 29);
			AssertNoMessageErrorContaining(declaration.US_DRWDatePeriodFromInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
			AssertNoMessageErrorContaining(declaration.US_DRWDatePeriodToInfo, ACSDrawbackAddInfoJobDeclarationValidation.DrawbackPeriodDatesMessage);
		}

		protected JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
		}

		void RunOneValidateClaimPortAndTeamNoTest(ZString claimPort, ZString teamNo, bool expectedResult)
		{
			declaration.US_ClaimPort = "9";
			declaration.US_TeamNo = "9";
			declaration.US_ClaimPort = claimPort;
			declaration.US_TeamNo = teamNo;
			if (expectedResult)
			{
				AssertHasMessageError(declaration.US_ClaimPortInfo, CommonDrawbackAddInfoJobDeclarationValidation.InconsistentClaimPortAndTeamNoMessage);
				AssertHasMessageError(declaration.US_TeamNoInfo, CommonDrawbackAddInfoJobDeclarationValidation.InconsistentClaimPortAndTeamNoMessage);
				declaration.US_TeamNo = "";
			}

			AssertNoMessageError(declaration.US_ClaimPortInfo, CommonDrawbackAddInfoJobDeclarationValidation.InconsistentClaimPortAndTeamNoMessage);
			AssertNoMessageError(declaration.US_TeamNoInfo, CommonDrawbackAddInfoJobDeclarationValidation.InconsistentClaimPortAndTeamNoMessage);
			declaration.US_ClaimPort = "9";
			declaration.US_TeamNo = "9";
			declaration.US_ClaimPort = claimPort;
			declaration.US_TeamNo = teamNo;
			if (expectedResult)
			{
				AssertHasMessageError(declaration.US_ClaimPortInfo, CommonDrawbackAddInfoJobDeclarationValidation.InconsistentClaimPortAndTeamNoMessage);
				AssertHasMessageError(declaration.US_TeamNoInfo, CommonDrawbackAddInfoJobDeclarationValidation.InconsistentClaimPortAndTeamNoMessage);
				declaration.US_ClaimPort = "";
			}

			AssertNoMessageError(declaration.US_ClaimPortInfo, CommonDrawbackAddInfoJobDeclarationValidation.InconsistentClaimPortAndTeamNoMessage);
			AssertNoMessageError(declaration.US_TeamNoInfo, CommonDrawbackAddInfoJobDeclarationValidation.InconsistentClaimPortAndTeamNoMessage);
		}
	}
}
