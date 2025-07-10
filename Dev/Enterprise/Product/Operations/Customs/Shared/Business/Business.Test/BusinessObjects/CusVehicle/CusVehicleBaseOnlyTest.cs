using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusVehicle))]
	class CusVehicleBaseOnlyTest : CusVehicleAbstractTest
	{
		public void TestOnSaving()
		{
			var vehicle = (CusVehicle)base.GetNewBusinessObject();
			var parent = (BaseJobComInvoiceLine)vehicle.Parent;
			Factory.Save();
			AssertEquals(parent.PK, vehicle.CVH_ParentID);
			AssertNotNullOrEmpty(vehicle.CVH_DataModel);
			AssertEquals(parent.JI_DataModel, vehicle.CVH_DataModel);
		}

		public void TestVehicleSaving()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var vehicle = invoiceLine.Vehicles.AddNew();
				AssertEquals("Vehicle is empty", true, vehicle.IsEmpty);
				Factory.Save();
				AssertEquals("Vehicle is not saved when empty", false, vehicle.IsInDatabase);

				vehicle.CVH_VehicleIdentificationNumber = "VIN1";
				Factory.Save();
				AssertEquals("Vehicle is not empty", false, vehicle.IsEmpty);
				AssertEquals("Vehicle is saved when not empty", true, vehicle.IsInDatabase);

				vehicle.CVH_VehicleIdentificationNumber = "";
				Factory.Save();
				AssertEquals("Vehicle is empty again after clearing VIN", true, vehicle.IsEmpty);
				AssertEquals("Vehicle is still saved even when it is empty", true, vehicle.IsInDatabase);
				AssertEquals("Vehicle is not deleted when it is empty", false, vehicle.IsDeleted);
			});
		}

		public void TestParent()
		{
			var parent1 = Factory.New<BaseJobComInvoiceLine>();
			var parent2 = Factory.New<BaseJobComInvoiceLine>();
			var vehicle = Factory.New<CusVehicle>();
			vehicle.CVH_ParentTableCode = parent1.TablePrefix;
			vehicle.CVH_ParentID = parent1.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Parent should be parent1", parent1, vehicle.Parent);

				vehicle.CVH_ParentID = parent2.PK;
				AssertEquals("Parent should be parent2", parent2, vehicle.Parent);

				vehicle.CVH_ParentTableCode = "XX";
				AssertNull("Parent", vehicle.Parent);
			});
		}

		public void TestIsEmpty()
		{
			var vehicle = Factory.New<CusVehicleForTesting>();
			AssertEquals(true, vehicle.IsEmpty);

			var propertiesToExclude = new[]
			{
				vehicle.CVH_ClusterKeyInfo,
				vehicle.CVH_DataModelInfo,
				vehicle.CVH_ParentIDInfo,
				vehicle.CVH_ParentTableCodeInfo,
				vehicle.CVH_SystemCreateTimeUtcInfo,
				vehicle.CVH_SystemCreateUserInfo,
				vehicle.CVH_SystemLastEditTimeUtcInfo,
				vehicle.CVH_SystemLastEditUserInfo,
				vehicle.CVH_IsDamagedInfo,
				vehicle.CVH_IsUsedInfo
			};
			var propertiesToTest = vehicle.ZPropertyInfoHash.OfType<ZPropertyInfo>().Except(propertiesToExclude);

			CombineAssertions(() =>
			{
				foreach (var property in propertiesToTest)
				{
					var testValue = "1234567890";
					if (property.PropertyType == typeof(ZByte) || property.PropertyType == typeof(ZShort))
					{
						testValue = "123";
					}
					else if (property.PropertyType == typeof(ZDate))
					{
						testValue = "22123";
					}
					else if (property.SupportsMaxLength && property.MaxLength < testValue.Length)
					{
						testValue = testValue.Substring(0, property.MaxLength);
					}

					property.SetValueFromString(testValue);
					AssertEquals($"After setting value for property '{property.Name}'", false, vehicle.IsEmpty);

					property.ClearValue();
					AssertEquals($"After clearing value for property '{property.Name}'", true, vehicle.IsEmpty);
				}
			});

			vehicle.EngineRelationshipForTesting = EngineRelationshipType.One;
			vehicle.FirstEngine.CEG_EngineNumber = "ENG1234";
			AssertEquals("When engine is not empty", false, vehicle.IsEmpty);

			vehicle.FirstEngine.CEG_EngineNumber = "";
			AssertEquals("When engine is empty", true, vehicle.IsEmpty);
		}

		public void TestCVH_ModelYear()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertEquals("CVH_ModelYear.MaxLength", 4, vehicle.CVH_ModelYearInfo.MaxLength);
		}

		public void TestCVH_VehicleIdentificationNumber()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertEquals("CVH_VehicleIdentificationNumber.MaxLength", 17, vehicle.CVH_VehicleIdentificationNumberInfo.MaxLength);
		}

		public void TestCVH_DataModel_ReportErrorWhenSetToEmpty() => DataModelTestHelper.RunDataModelTest_ReportErrorWhenSetToEmpty<CusVehicle>(Factory, GetCusVehicle);

		public void TestCVH_DataModel_ReportErrorWhenUpdated() => DataModelTestHelper.RunDataModelTest_ReportErrorWhenUpdated<CusVehicle>(Factory, GetCusVehicle);

		public void TestCVH_DataModel_CanSaveTwice() => DataModelTestHelper.RunDataModelTest_CanSaveTwice<CusVehicle>(Factory, GetCusVehicle);

		CusVehicle GetCusVehicle(BusinessObjectFactory factory)
		{
			var declaration = factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var vehicle =  invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = "VIN1";
			return vehicle;
		}

		protected override Type ExpectedLookupsType => typeof(CusVehicleLookups);
		protected override Type ExpectedValidationType => typeof(CusVehicleValidation);
	}
}
