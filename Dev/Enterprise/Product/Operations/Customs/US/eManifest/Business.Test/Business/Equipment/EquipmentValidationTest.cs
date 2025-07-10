using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class EquipmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBJ_RQ_Equipment()
		{
			var refEquipment = Factory.New<RefEquipment>();
			ValidationTestHelper.AssertErrorIfInvalidPK(equipment.BJ_RQ_EquipmentInfo, ZGuid.Invalid, refEquipment.PK, "Enter a valid Equipment.");
			ValidationTestHelper.AssertErrorIfInvalidPK(conveyance.BJ_RQ_EquipmentInfo, ZGuid.Invalid, refEquipment.PK, "Enter a valid Conveyance.");
		}

		public void TestCheckBJ_IsConveyance()
		{
			AssertNoMessageErrorContaining(conveyance.BJ_IsConveyanceInfo, "Only one");
			AssertNoMessageErrorContaining(equipment.BJ_IsConveyanceInfo, "Only one");
			equipment.BJ_IsConveyance = true;
			AssertHasMessageErrorContaining(equipment.BJ_IsConveyanceInfo, "Only one");
		}

		public void TestCheckBJ_InsuranceAmount()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(conveyance.BJ_InsuranceAmountInfo, HazmatErrorMessage);
			trip.Shipments.AddNew().Commodities.AddNew().UNDGs.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(conveyance.BJ_InsuranceAmountInfo, HazmatErrorMessage);
			ValidationTestHelper.AssertFieldIsNotMandatory(equipment.BJ_InsuranceAmountInfo, HazmatErrorMessage);
		}

		public void TestCheckBJ_InsuranceName()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(conveyance.BJ_InsuranceNameInfo, HazmatErrorMessage);
			trip.Shipments.AddNew().Commodities.AddNew().UNDGs.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(conveyance.BJ_InsuranceNameInfo, HazmatErrorMessage);
			ValidationTestHelper.AssertFieldIsNotMandatory(equipment.BJ_InsuranceNameInfo, HazmatErrorMessage);
		}

		public void TestCheckBJ_InsurancePolicyNumber()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(conveyance.BJ_InsurancePolicyNumberInfo, HazmatErrorMessage);
			trip.Shipments.AddNew().Commodities.AddNew().UNDGs.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(conveyance.BJ_InsurancePolicyNumberInfo, HazmatErrorMessage);
			ValidationTestHelper.AssertFieldIsNotMandatory(equipment.BJ_InsurancePolicyNumberInfo, HazmatErrorMessage);
		}

		public void TestCheckBJ_InsuranceYearPolicyIssue()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(conveyance.BJ_InsuranceYearPolicyIssueInfo, HazmatErrorMessage);
			trip.Shipments.AddNew().Commodities.AddNew().UNDGs.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(conveyance.BJ_InsuranceYearPolicyIssueInfo, HazmatErrorMessage);
			ValidationTestHelper.AssertFieldIsNotMandatory(equipment.BJ_InsuranceYearPolicyIssueInfo, HazmatErrorMessage);
		}

		public void TestCheckBJ_RC_RoadContainerType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(conveyance.BJ_RC_RoadContainerTypeInfo, "not entered");
			var refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
			AssertNoNotifications(conveyance.BJ_RC_RoadContainerTypeInfo);
		}

		public void TestCheckBJ_RegistrationNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(conveyance.BJ_RegistrationNumberInfo, "not entered");
			var refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
			AssertNoNotifications(conveyance.BJ_RegistrationNumberInfo);
		}

		public void TestCheckRW_NKRegistrationStateInfo()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(conveyance.BJ_RW_NKRegistrationStateInfo, "not entered");
			var refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
			AssertNoNotifications(conveyance.BJ_RW_NKRegistrationStateInfo);
		}

		public void TestCheckBJ_RN_NKRegistrationCountry()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(conveyance.BJ_RN_NKRegistrationCountryInfo, "not entered");
			var refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
			AssertNoNotifications(conveyance.BJ_RN_NKRegistrationCountryInfo);
		}

		public void TestCheckBJ_VIN()
		{
			conveyance.BJ_IsConveyance = false;
			var refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
			conveyance.Validation.ValidateBJ_VIN();
			AssertNoMessageErrorContaining(conveyance.BJ_VINInfo, "not entered");
			conveyance.BJ_IsConveyance = true;
			conveyance.BJ_RQ_Equipment = ZGuid.Empty;
			conveyance.BJ_VIN = "";
			AssertHasMessageErrorContaining(conveyance.BJ_VINInfo, "not entered");
		}

		public void TestCheckBJ_ACEID()
		{
			conveyance.BJ_ACEID = "12345678901";
			AssertHasNotifications("exceeds the maximum", conveyance.BJ_ACEIDInfo);
			var refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
			AssertNoNotifications(conveyance.BJ_ACEIDInfo);
		}

		public void TestValidateAll()
		{
			trip.Shipments.AddNew().Commodities.AddNew().UNDGs.AddNew();
			conveyance.Validation.ValidateAll();
			AssertHasNotifications(conveyance.BJ_InsuranceAmountInfo);
			AssertHasNotifications(conveyance.BJ_InsuranceNameInfo);
			AssertHasNotifications(conveyance.BJ_InsurancePolicyNumberInfo);
			AssertHasNotifications(conveyance.BJ_InsuranceYearPolicyIssueInfo);
			AssertHasNotifications(conveyance.BJ_RC_RoadContainerTypeInfo);
			AssertHasNotifications(conveyance.BJ_RegistrationNumberInfo);
			AssertHasNotifications(conveyance.BJ_RW_NKRegistrationStateInfo);
			AssertHasNotifications(conveyance.BJ_RN_NKRegistrationCountryInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			trip = Factory.New<Trip>();
			conveyance = trip.Conveyance;
			equipment = trip.Equipment.AddNew();
		}

		Equipment conveyance;
		Equipment equipment;
		Trip trip;
		const string HazmatErrorMessage = "is required for hazardous shipments";
	}
}
