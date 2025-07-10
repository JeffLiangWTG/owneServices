using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	[CargoWise.Data.Testing.UseSnapshotProtection]
	sealed class ValidationModesCalculatorTest : TestCaseWithFactory
	{
#if NETFRAMEWORK
		[NUnit.Framework.ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: declaration")]
#else
		[NUnit.Framework.ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null. (Parameter 'declaration')")]
#endif
		public void TestContsructor()
		{
			new ValidationModesCalculator(null);
		}

		public void TestUpdateForCargoReleaseCertification()
		{
			declaration.US_EnableCRL = true;
			AssertEquals(ValidationModes.CargoRelease, declaration.ValidationModes & ValidationModes.CargoRelease);

			declaration.US_EnableCRL = false;
			Assert(!declaration.US_CertifyCargoRelease);
			AssertNotEquals(ValidationModes.CargoRelease, declaration.ValidationModes & ValidationModes.CargoRelease);

			var ensEntryBeingCertified = declaration.CustomsEntryHeaders.AddNew();
			ensEntryBeingCertified.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
			AssertEquals("IsCargoReleaseBeingCertified", true, ensEntryBeingCertified.IsCargoReleaseBeingCertified);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.ValidationModes = ValidationModes.None;
			declaration.RecalculateValidationModesOnDeclaration();

			AssertEquals("ValidationModes", ValidationModes.EntrySummary, ValidationModes.EntrySummary & declaration.ValidationModes);
		}

		public void TestUpdateDeclarationValidationModesWhenAMessagingModeChanges()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2904", "TEST NAME", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "TEST NAME", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeUnlading = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameUnlading.ZXE_Name, "Y");
			Factory.Save();

			declaration.US_EnableCRL = true;
			AssertEquals("When CRL is enabled, Declaration.ValidationModes should contain the info", ValidationModes.CargoRelease, ValidationModes.CargoRelease & declaration.ValidationModes);

			declaration.US_EnableINB = true;
			AssertEquals("Declaration.ValidationModes should still contain CRL enabled", ValidationModes.CargoRelease, ValidationModes.CargoRelease & declaration.ValidationModes);

			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			AssertNotEquals("Declaration.ValidationModes should not contain CRL enabled", ValidationModes.CargoRelease, ValidationModes.CargoRelease & declaration.ValidationModes);

			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, "1101");
			SetUpValidationPassingDeclaration(declaration);
			declaration.US_EnableENS = false;
			Factory.Save(); // will have JE_DeclarationReference assigned and it will be marked as invalid

			declaration.RunPreSaveValidation();
			AssertEquals("Errors", "", declaration.Notifications.GetErrors().ToUniqueMessageListString());
			AssertEquals("MessageErrors", "", declaration.Notifications.GetMessageErrors().ToUniqueMessageListString());
			AssertEquals("Warnings", "", declaration.Notifications.GetWarnings().ToUniqueMessageListString());

			Factory.Save();
			AssertEquals(true, declaration.LightValidationIsValid);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableINB = false;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			AssertEquals("Declaration.ValidationModes should be EntrySummary | CargoRelease", ValidationModes.EntrySummary | ValidationModes.CargoRelease, declaration.ValidationModes);
		}

		public void TestUpdatingValidationModesOnLoadedDoesNotMarkAsNeedingValidationIfSame()
		{
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, "1101");
			SetUpValidationPassingDeclaration(declaration);
			Factory.Save();//will have JE_DeclarationReference assigned and it will be marked as invalid
			AssertEquals("pre-condition", false, declaration.LightValidationIsValid);

			var factory1 = new BusinessObjectFactory();
			var declaration1 = factory1.Load<JobDeclaration>(declaration.PK);
			declaration1.US_EnableINB = true;
			declaration1.RunPreSaveValidation();
			declaration1.US_EnableENS = false;
			factory1.Save();
			AssertEquals("should be true", true, declaration1.LightValidationIsValid);

			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("US_ValidationModesAtLastSave and current validation modes are the same", true, declaration2.LightValidationIsValid);
		}

		public void TestIsThisValidationOn()
		{
			SetUpValidationPassingDeclaration(declaration);
			declaration.US_EnableINB = false;//EnableINB is not intended for this test
			declaration.US_EntryType = "";
			declaration.US_EnableENS = false;
			Factory.Save();//will have JE_DeclarationReference assigned and it will be marked as invalid

			ValidationModesCalculator calculator = new ValidationModesCalculator(declaration);

			AssertEquals("PreCondition:ValidationModes NotSetYet", ValidationModes.None, declaration.ValidationModes);
			AssertEquals("PreCondition:US_EnableINB is not on", false, declaration.US_EnableINB);

			declaration.US_EnableINB = true;

			AssertEquals("ENS validation is off now", false, calculator.IsThisValidationOn(declaration.ValidationModes, ValidationModes.EntrySummary));
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertEquals("ENS validation is ON now", true, calculator.IsThisValidationOn(declaration.ValidationModes, ValidationModes.EntrySummary));
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60267", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();

			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TotalNoOfPacksPackType = "";
		}

		void SetUpValidationPassingDeclaration(JobDeclaration declaration)
		{
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.JE_HouseBillIssuerSCAC = "";
			declaration.JE_MasterBillIssuerSCAC = "";
			USCarrierCombined usCarrier = Factory.New<USCarrierCombined>();
			usCarrier.UI_Code = "AAAS";
			usCarrier.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;
			declaration.US_UI_NKCarrierSCAC = usCarrier.UI_Code;

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;

			OrgHeader shippingLine = Factory.New<OrgHeader>();
			shippingLine.FillWithValidTestData();
			shippingLine.OH_IsShippingLine = true;
			shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, usCarrier.UI_Code, GlbCompany.CurrentCompany.Country);
			shippingLine.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "11111", GlbCompany.CurrentCompany.Country);

			declaration.JE_OH_ShippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsShippingLine, ZBool.True)).PK;
			declaration.JE_VoyageFlightNo = "US123";
			declaration.JE_MasterBill = "1";
			declaration.US_SchDUSDestination = "3124";
			declaration.US_SchDLoading = "60267";
			declaration.US_SchDEntry = "8888";
			declaration.US_SchDArrival = "2904";
			declaration.US_InbondType = EntryTypeList.Codes.ImmediateTransportation;
			declaration.PrimaryMasterBill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.Yes;
			declaration.PrimaryMasterBill.US_GoodsValueInLocalCurrency = 10m;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_HouseBill = "2";
			declaration.JE_MasterBillIssuerSCAC = usCarrier.UI_Code;
			declaration.JE_HouseBillIssuerSCAC = usCarrier.UI_Code;
		}
	}
}
