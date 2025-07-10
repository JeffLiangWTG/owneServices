using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.TransportCommon.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefEquipmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRQ_GeoProviderID_IsEmpty()
		{
			var truck1 = Factory.New<RefEquipment>();
			var truck2 = Factory.New<RefEquipment>();
			truck2.RunPreSaveValidation();
			AssertNoErrors(truck2.RQ_GeoProviderIDInfo);
		}

		public void TestValidateRQ_GeoProviderID()
		{
			var truck1 = Factory.New<RefEquipment>();
			var truck2 = Factory.New<RefEquipment>();

			truck1.RQ_GeoProviderType = "NAV";
			truck1.RQ_GeoProviderID = "100";

			truck2.RQ_GeoProviderType = "NAV";
			truck2.RQ_GeoProviderID = "100";
			AssertHasErrors(truck2.RQ_GeoProviderIDInfo);

			truck2.RQ_GeoProviderID = "200";
			AssertNoErrors(truck2.RQ_GeoProviderIDInfo);

			truck2.RQ_GeoProviderID = "100";
			AssertHasErrors(truck2.RQ_GeoProviderIDInfo);

			truck2.RQ_GeoProviderType = "NV2";
			truck2.RQ_GeoProviderID = "100";
			AssertNoErrors(truck2.RQ_GeoProviderIDInfo);
		}

		public void TestCheckRQ_GS_NKPreferredDriver_ValidCode()
		{
			GlbGroup driversGroup = Factory.New<GlbGroup>();
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());

			GlbStaff bob = Factory.New<GlbStaff>();
			bob.GS_LoginName = "bob";
			bob.GS_Code = "bob";

			GlbStaff mob = Factory.New<GlbStaff>();
			mob.GS_LoginName = "mob";
			mob.GS_Code = "mob";

			bob.Groups.Add(driversGroup);
			Factory.Save();

			RefEquipment vehicle = Factory.New<RefEquipment>();
			vehicle.RQ_GS_NKPreferredDriver = bob.GS_Code;
			AssertNoErrors(vehicle.RQ_GS_NKPreferredDriverInfo);
			vehicle.RQ_GS_NKPreferredDriver = mob.GS_Code;
			AssertHasErrors(vehicle.RQ_GS_NKPreferredDriverInfo);
		}

		public void TestCheckRQ_GS_NKPreferredDriver_OtherCarWithSameDriver()
		{
			RefEquipment otherCar = Factory.New<RefEquipment>();
			otherCar.RQ_IsVehicle = true;
			otherCar.RQ_Registration = "abc";
			otherCar.RQ_ShortCode = "abc";

			GlbStaff driver = Factory.New<GlbStaff>();
			driver.GS_Code = "aaa";

			Factory.Save();

			otherCar.RQ_GS_NKPreferredDriver = driver.GS_Code;
			AssertNoWarnings(otherCar.RQ_GS_NKPreferredDriverInfo);

			RefEquipment anotherCar = Factory.New<RefEquipment>();
			anotherCar.RQ_IsVehicle = true;
			anotherCar.RQ_Registration = "def";
			anotherCar.RQ_ShortCode = "def";

			RefEquipment newCar = Factory.New<RefEquipment>();
			newCar.RQ_IsVehicle = true;
			newCar.RQ_Registration = "ghi";
			newCar.RQ_ShortCode = "ghi";

			Factory.Save();

			anotherCar.RQ_GS_NKPreferredDriver = driver.GS_Code;
			AssertHasWarning(anotherCar.RQ_GS_NKPreferredDriverInfo, "This Driver is already allocated to another vehicle abc");

			newCar.RQ_GS_NKPreferredDriver = driver.GS_Code;
			AssertHasWarning(newCar.RQ_GS_NKPreferredDriverInfo, "This Driver is already allocated to another vehicle abc, def");
		}

		public void TestCheckRQ_Registration()
		{
			var bO = Factory.New<RefEquipment>();
			bO.RQ_Registration = ZString.Empty;
			Assert("Expecting RQ_Registration to be empty and have errors.", bO.RQ_RegistrationInfo.HasErrors());
			bO.RQ_Registration = new ZString("1111111111");
			Assert("RQ_Registration should be correct, not expecting errors.", !bO.RQ_RegistrationInfo.HasNotifications());
		}

		public void TestCheckRQ_AddFlag1()
		{
			var bO = Factory.New<RefEquipment>();
			bO.RQ_RN_NKRegistrationCountry = "AU";
			bO.RQ_AddFlag1 = false;
			bO.RQ_ShortCode = "ZZZ";
			Factory.Save();

			AssertNoError(bO.RQ_AddFlag1Info, "No TruckSafe number has been specified in the registry.");

			bO.RQ_AddFlag1 = true;
			Factory.Save();

			AssertHasError(bO.RQ_AddFlag1Info, "No TruckSafe number has been specified in the registry.");

			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.EquipmentTruckSafe.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "BLAH");
			bO.RQ_AddFlag1 = true;
			Factory.Save();

			AssertNoError(bO.RQ_AddFlag1Info, "No TruckSafe number has been specified in the registry.");
		}

		public void TestCheckRQ_RegState()
		{
			var bO = Factory.New<RefEquipment>();
			bO.RQ_RN_NKRegistrationCountry = "AU";
			bO.RQ_ShortCode = "ZZZ";
			Factory.Save();

			bO.RQ_RegState = "";
			bO.Validation.ValidateRQ_RegState();
			Assert("State. Error not expected.", !bO.RQ_RegStateInfo.HasErrors());

			bO.RQ_RegState = "XXX";
			bO.Validation.ValidateRQ_RegState();
			Assert("State. Error expected.", bO.RQ_RegStateInfo.HasErrors());
		}

		public void TestCheckRQ_Description()
		{
			var bO = Factory.New<RefEquipment>();
			bO.RQ_Description = ZString.Empty;
			Assert("Expecting RQ_Description to be empty and have errors.", bO.RQ_DescriptionInfo.HasErrors());
			bO.RQ_Description = new ZString("45 Pan Refrigerator");
			Assert("RQ_Description should be correct, not expecting errors.", !bO.RQ_DescriptionInfo.HasNotifications());
		}

		public void TestCheckRQ_PackCountAndType()
		{
			var bO = Factory.New<RefEquipment>();
			bO.RQ_PackCapacity = -2;
			bO.RQ_F3_NKPackType = "";

			Assert("Package Count. Error expected.", bO.RQ_PackCapacityInfo.HasErrors());

			bO.RQ_PackCapacity = 1;
			Assert("Package Type. Error expected.", bO.RQ_F3_NKPackTypeInfo.HasErrors());

			bO.RQ_PackCapacity = 0;
			bO.RQ_F3_NKPackType = "XXX";
			Assert("Package Type. Error expected.", bO.RQ_F3_NKPackTypeInfo.HasErrors());
		}

		public void TestCheckRQ_WeightCapacityAndUnit()
		{
			var bO = Factory.New<RefEquipment>();
			bO.RQ_WeightCapacity = -2;
			bO.RQ_WeightUnit = "";

			Assert("Weight. Error expected.", bO.RQ_WeightCapacityInfo.HasErrors());

			bO.RQ_WeightCapacity = 1;
			Assert("Weight Unit. Error expected.", bO.RQ_WeightUnitInfo.HasErrors());

			bO.RQ_WeightCapacity = 0;
			bO.RQ_WeightUnit = "XX";
			Assert("Weight Unit. Error expected.", bO.RQ_WeightUnitInfo.HasErrors());

			bO.RQ_WeightCapacity = 5;
			bO.RQ_WeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			AssertNoErrors(bO.RQ_WeightUnitInfo);
		}

		public void TestCheckRQ_TareWeightAndUnit()
		{
			var bO = Factory.New<RefEquipment>();
			bO.RQ_TareWeight = -2;
			bO.RQ_WeightUnit = "";

			Assert("Tare. Error expected.", bO.RQ_TareWeightInfo.HasErrors());

			bO.RQ_TareWeight = 1;
			Assert("Tare Unit. Error expected.", bO.RQ_WeightUnitInfo.HasErrors());

			bO.RQ_TareWeight = 0;
			bO.RQ_WeightUnit = "XX";
			Assert("Tare Unit. Error expected.", bO.RQ_WeightUnitInfo.HasErrors());

			bO.RQ_TareWeight = 5;
			bO.RQ_WeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			AssertNoErrors(bO.RQ_TareWeightInfo);
		}

		public void TestCheckRQ_CubicCapacityAndUnit()
		{
			var bO = Factory.New<RefEquipment>();
			bO.RQ_CubicCapacity = -2;
			bO.RQ_CubicUnit = "";

			Assert("Cubic. Error expected.", bO.RQ_CubicCapacityInfo.HasErrors());

			bO.RQ_CubicCapacity = 1;
			Assert("Cubic Unit. Error expected.", bO.RQ_CubicUnitInfo.HasErrors());

			bO.RQ_CubicCapacity = 0;
			bO.RQ_CubicUnit = "XX";
			Assert("Cubic Unit. Error expected.", bO.RQ_CubicUnitInfo.HasErrors());

			bO.RQ_CubicCapacity = 5;
			bO.RQ_CubicUnit = Enterprise.Core.Constants.Volume.CubicFeet;
			AssertNoErrors(bO.RQ_CubicUnitInfo);
		}

		public void TestRQ_ShortCode()
		{
			var equipment = Factory.New<RefEquipment>();
			equipment.Validation.ValidateRQ_ShortCode();
			AssertHasErrorContaining(equipment.RQ_ShortCodeInfo, MandatoryValidation.MustBeEntered);

			equipment.RQ_ShortCode = "XXXX";
			AssertNoErrorContaining(equipment.RQ_ShortCodeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckRQ_PurchaseDateIsValidZDateTimeRange()
		{
			var equipment = Factory.New<RefEquipment>();
			var pastDate = DateTime.UtcNow.AddYears(-49);
			equipment.RQ_PurchaseDate = pastDate;
			Assert("Purchase Date can be within 50 years.", !equipment.RQ_PurchaseDateInfo.HasErrors());
			pastDate = DateTime.UtcNow.AddYears(-51);
			equipment.RQ_PurchaseDate = pastDate;
			Assert("Purchase Date can be within 50 years.", equipment.RQ_PurchaseDateInfo.HasErrors());
		}

		public void TestWeightCapacity_ValidatesUnits()
		{
			var b0 = Factory.New<RefEquipment>();
			b0.RQ_WeightCapacity = 1;

			Assert(b0.RQ_WeightUnitInfo.HasErrors());
		}

		public void TestTareWeight_ValidatesUnits()
		{
			var b0 = Factory.New<RefEquipment>();
			b0.RQ_TareWeight = 1;

			Assert(b0.RQ_WeightUnitInfo.HasErrors());
		}
	}
}
