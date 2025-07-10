using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class CommonDrawbackJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public void TestCheckJE_MessageTypeForDRW()
		{
			this.declaration.US_EntryFilerCode = "";
			this.declaration.Validation.ValidateJE_MessageType();
			AssertHasErrorContaining(this.declaration.JE_MessageTypeInfo, CommonDrawbackJobDeclarationValidation.EntryFilerCodeIsEmptyAndEntryNumberCannotBeGenerated);

			this.declaration.US_EntryFilerCode = "123";
			this.declaration.Validation.ValidateJE_MessageType();
			AssertNoError(this.declaration.JE_MessageTypeInfo, CommonDrawbackJobDeclarationValidation.EntryFilerCodeIsEmptyAndEntryNumberCannotBeGenerated);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "123";
			declaration.US_EntryFilerCode = "123";
			Factory.Save();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertHasErrorContaining(declaration.JE_MessageTypeInfo, CommonDrawbackJobDeclarationValidation.CannotChangeShipmentTypeToDrawback);
		}

		public void TestCheckJE_OH_Importer()
		{
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			OrgHeader claimant = Factory.New<OrgHeader>();
			claimant.OH_IsConsignee = true;

			declaration.JE_OH_Importer = claimant.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, CommonDrawbackJobDeclarationValidation.ClaimantRegNoRequired);

			OrgCusCode cusCode = claimant.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "A");
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			declaration.JE_OH_Importer = claimant.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, CommonDrawbackJobDeclarationValidation.ClaimantRegNoRequired);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			declaration.JE_OH_Importer = claimant.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, CommonDrawbackJobDeclarationValidation.ClaimantRegNoRequired);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			declaration.JE_OH_Importer = claimant.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, CommonDrawbackJobDeclarationValidation.ClaimantRegNoRequired);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			cusCode.OK_OA_PremisesAddress = claimant.MainAddress.PK;
			declaration.JE_OH_Importer = claimant.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, CommonDrawbackJobDeclarationValidation.ClaimantRegNoRequired);

			cusCode.OK_RN_NKCodeCountry = ZString.Empty;
			declaration.JE_OH_Importer = claimant.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, CommonDrawbackJobDeclarationValidation.ClaimantRegNoRequired);

			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			declaration.JE_OH_Importer = claimant.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, CommonDrawbackJobDeclarationValidation.ClaimantRegNoRequired);

			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			declaration.JE_OH_Importer = claimant.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, CommonDrawbackJobDeclarationValidation.ClaimantRegNoRequired);
		}

		public void TestCheckJE_OH_Importer_POAWithAttribute()
		{
			var validator = new AuthorityToActValidator();

			var ior = Factory.New<OrgHeader>();
			var poaDocument = ior.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
			poaDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_ValidToDate = ZDateTime.Today.AddYears(1);
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-1);

			var attribute1 = poaDocument.Attributes.AddNew();
			attribute1.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			attribute1.D0_AttribValue = ImportExportCodeList.Codes.Export;

			var notMatchErrorMessage = AuthorityToActValidator.GetPOANotValidForConditions(validator.CountrySpecificNameForPOA, "organization (eDocs > Document Tracking)", "US and/or this direction");
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(companyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_OH_Importer = ior.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, notMatchErrorMessage);

			attribute1.D0_AttribValue = ImportExportCodeList.Codes.Import;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, notMatchErrorMessage);

			var attribute2 = poaDocument.Attributes.AddNew();
			attribute2.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			attribute2.D0_AttribValue = ZString.Empty;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, notMatchErrorMessage);

			attribute2.D0_AttribValue = "2705";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, notMatchErrorMessage);

			attribute2.Delete();
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, notMatchErrorMessage);
		}

		public void TestCheckDeclarationNumber()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_EntryFilerCode = "XJ5";
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			companyStmNums.SetNextNumber(companyStmNums.SN_MaximumValue);
			companyStmNums.GenerateNextCustomsNumber(Factory);

			var companyMessage = string.Format(ACEEntryStmNumsSetting.NotEnoughAvailableEntryNumbersForCompany, GlbCompany.CurrentCompany.GC_Code, "XJ5");
			AssertEquals("PreCondition", companyMessage, declaration.DisallowAllocateImportEntryNumber);
			declaration.DeclarationNumber = ZString.Empty;
			AssertHasMessageError(declaration.DeclarationNumberInfo, companyMessage);
		}

		public void TestCheckJE_MessageSubType()
		{
			declaration.JE_MessageSubType = "FRM";
			AssertNoMessageErrors("JE_MessageSubType is not used for a Drawback Summary", declaration.JE_MessageSubTypeInfo);
		}

		public override void TestCheckJE_MasterBill()
		{
			Assert("Master Bill validation should not be tested for Drawback, because no Master Bill for Drawback declaration", true);
		}

		public override void TestCheckBrokerToPayIndicator()
		{
			Assert("Broker To Pay Indicator not relevant for Drawback", true);
		}

		public void TestCheckJE_MergeBy()
		{
			declaration.JE_MergeBy = ZString.Empty;
			AssertNoErrors(declaration.JE_MergeByInfo);
		}

		public new void TestUS_SchDArrivalForSea()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			AssertEquals("IsSea", true, declaration.IsSea);
		}

		#region Implementation

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
		}

		#endregion
	}
}
