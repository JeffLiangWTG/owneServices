using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(Equipment))]
	sealed class EquipmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCalculatedPropertiesAndDefaultingOfBjFromRq()
		{
			var refEquipment = Factory.New<RefEquipment>();
			refEquipment.RQ_Registration = "123456";
			refEquipment.RQ_Description = "Car";
			refEquipment.RQ_VIN = "VINTest";
			var equipment = Factory.New<Equipment>();
			equipment.BJ_RQ_Equipment = refEquipment.PK;
			AssertEquals("BJ_RegistrationNumber", "123456", equipment.BJ_RegistrationNumber);
			AssertEquals("BJ_EquipmentDescription", "Car", equipment.BJ_EquipmentDescription);
			AssertEquals("BJ_VIN", "VINTest", equipment.BJ_VIN);
			AssertEquals("EQ_InsuranceCurrency", Core.Constants.CurrencyCodes.UnitedStates, equipment.EQ_InsuranceCurrency);
			CusCodeDataExtensionsTest.AssertPropertyIsStringRepresentationOfCusCodeDataCollection(equipment.BJ_SealNumbersInfo, equipment.SealNumbers);
		}

		public void TestLoadOrCreateConveyance()
		{
			var trip = Factory.New<Trip>();
			var equipment = Factory.New<Equipment>();
			equipment.BJ_BH_Header = trip.PK;
			equipment.BJ_IsConveyance = false;
			var conveyance = Equipment.LoadOrCreateConveyance(trip);
			AssertNotNull("Conveyance should be created", conveyance);
			AssertEquals("No validation should be run", false, conveyance.HasNotifications());
			AssertNotEquals("Should not be equipment", equipment.PK, conveyance.PK);
			AssertEquals("BJ_BH_Header", trip.PK, conveyance.BJ_BH_Header);
			AssertEquals("BJ_IsConveyance", true, conveyance.BJ_IsConveyance);
			var conveyance2 = Equipment.LoadOrCreateConveyance(trip);
			AssertEquals("Same conveyance should be loaded", conveyance2.PK, conveyance.PK);
		}

		public void TestIITEntityIndicators()
		{
			var equipment = Factory.New<Equipment>();
			AssertEquals("Pre-condition: IITEntityIndicators should not be set", ZString.Empty, equipment.BJ_IITEntityIndicators);
			equipment.BJ_EmptyIITsCoveredByCarrier = true;
			AssertEquals("BJ_EmptyIITsCoveredByCarrier is added", "EC", equipment.BJ_IITEntityIndicators);
			equipment.BJ_EmptyIITsCoveredByImporter = true;
			AssertEquals("BJ_EmptyIITsCoveredByImporter is added", "ECEI", equipment.BJ_IITEntityIndicators);
			equipment.BJ_MerchandiseAndIITsCoveredByCarrier = true;
			AssertEquals("BJ_MerchandiseAndIITsCoveredByCarrier is added", "ECEIMC", equipment.BJ_IITEntityIndicators);
			equipment.BJ_MerchandiseAndIITsCoveredByImporter = true;
			AssertEquals("BJ_MerchandiseAndIITsCoveredByImporter is added", "ECEIMCMI", equipment.BJ_IITEntityIndicators);
			equipment.BJ_EmptyIITsCoveredByImporter = false;
			AssertEquals("BJ_EmptyIITsCoveredByImporter is removed", "ECMCMI", equipment.BJ_IITEntityIndicators);
			equipment.BJ_MerchandiseAndIITsCoveredByCarrier = false;
			AssertEquals("BJ_MerchandiseAndIITsCoveredByCarrier is removed", "ECMI", equipment.BJ_IITEntityIndicators);
			equipment.BJ_EmptyIITsCoveredByCarrier = false;
			AssertEquals("BJ_EmptyIITsCoveredByCarrier is removed", "MI", equipment.BJ_IITEntityIndicators);
			equipment.BJ_MerchandiseAndIITsCoveredByImporter = false;
			AssertEquals("BJ_MerchandiseAndIITsCoveredByImporter is removed", ZString.Empty, equipment.BJ_IITEntityIndicators);
		}

		public void TestDelete()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			var equipment = trip.Equipment.AddNew();
			var sealNumber = equipment.SealNumbers.AddNew();
			equipment.Delete();
			Assert("SealNumbers should be deleted", sealNumber.IsDeleted);
			var refEquipment = Factory.New<RefEquipment>();
			refEquipment.RQ_ShortCode = "DJC";
			refEquipment.RQ_Registration = "123456";
			refEquipment.RQ_Description = "Car";
			refEquipment.RQ_EquipmentType = "DC";
			equipment = trip.Equipment.AddNew();
			equipment.BJ_RQ_Equipment = refEquipment.PK;
			equipment.BJ_RegistrationNumber = ZString.Empty;
			Factory.Save();
			AssertEquals(false, equipment.IsDeleted);
			equipment.BJ_RQ_Equipment = ZGuid.Empty;
			Factory.Save();
			AssertEquals(false, equipment.IsDeleted);
			equipment = trip.Equipment.AddNew();
			equipment.BJ_RegistrationNumber = "A";
			Factory.Save();
			AssertEquals(false, equipment.IsDeleted);
			equipment.BJ_RegistrationNumber = ZString.Empty;
			Factory.Save();
			AssertEquals(true, equipment.IsDeleted);
			equipment = trip.Equipment.AddNew();
			equipment.BJ_ACEID = "A";
			Factory.Save();
			AssertEquals(false, equipment.IsDeleted);
			equipment.BJ_ACEID = ZString.Empty;
			Factory.Save();
			AssertEquals(true, equipment.IsDeleted);
			equipment = trip.Equipment.AddNew();
			equipment.BJ_ContainerType = "A";
			Factory.Save();
			AssertEquals(false, equipment.IsDeleted);
			equipment.BJ_ContainerType = ZString.Empty;
			Factory.Save();
			AssertEquals(true, equipment.IsDeleted);
		}

		public void TestInsuranceReadOnly()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			var equipment = trip.Equipment.AddNew();
			equipment.BJ_IsConveyance = true;
			Assert("BJ_IsConveyance is true", equipment.BJ_IsConveyance);
			Assert("Not ReadOnly", !equipment.BJ_InsuranceAmountInfo.ReadOnly);
			Assert("Not ReadOnly", !equipment.BJ_InsuranceNameInfo.ReadOnly);
			Assert("Not ReadOnly", !equipment.BJ_InsurancePolicyNumberInfo.ReadOnly);
			Assert("Not ReadOnly", !equipment.BJ_InsuranceYearPolicyIssueInfo.ReadOnly);

			equipment.BJ_IsConveyance = false;
			Assert("BJ_IsConveyance is false", !equipment.BJ_IsConveyance);
			Assert("ReadOnly", equipment.BJ_InsuranceAmountInfo.ReadOnly);
			Assert("ReadOnly", equipment.BJ_InsuranceNameInfo.ReadOnly);
			Assert("ReadOnly", equipment.BJ_InsurancePolicyNumberInfo.ReadOnly);
			Assert("ReadOnly", equipment.BJ_InsuranceYearPolicyIssueInfo.ReadOnly);
		}

		public void TestReadOnlyWhenEquipmentSet()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			var equipment = trip.Equipment.AddNew();
			Assert("BJ_RQ_Equipment Not Set", equipment.BJ_RQ_Equipment.IsEmpty);
			Assert("Not ReadOnly", !equipment.BJ_RegistrationNumberInfo.ReadOnly);
			Assert("Not ReadOnly", !equipment.BJ_ACEIDInfo.ReadOnly);
			Assert("Not ReadOnly", !equipment.BJ_RC_RoadContainerTypeInfo.ReadOnly);
			Assert("Not ReadOnly", !equipment.BJ_RN_NKRegistrationCountryInfo.ReadOnly);
			Assert("Not ReadOnly", !equipment.BJ_RW_NKRegistrationStateInfo.ReadOnly);
			Assert("Not ReadOnly", !equipment.BJ_VINInfo.ReadOnly);
			equipment.BJ_RQ_Equipment = System.Guid.NewGuid();
			Assert("BJ_RQ_Equipment Set", !equipment.BJ_RQ_Equipment.IsEmpty);
			Assert("ReadOnly", equipment.BJ_RegistrationNumberInfo.ReadOnly);
			Assert("ReadOnly", equipment.BJ_ACEIDInfo.ReadOnly);
			Assert("ReadOnly", equipment.BJ_RC_RoadContainerTypeInfo.ReadOnly);
			Assert("ReadOnly", equipment.BJ_RN_NKRegistrationCountryInfo.ReadOnly);
			Assert("ReadOnly", equipment.BJ_RW_NKRegistrationStateInfo.ReadOnly);
			Assert("ReadOnly", equipment.BJ_VINInfo.ReadOnly);
		}
	}
}
