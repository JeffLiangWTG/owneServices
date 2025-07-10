using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

[TestedType(typeof(CusEngine))]
class CusEngineBaseOnlyTest : CusEngineAbstractTest
{
	public void TestOnSaving()
	{
		var engine = (CusEngine)GetNewBusinessObject();
		var parent = (CusVehicle)engine.Parent;
		parent.CVH_DataModel = "AU";
		Factory.Save();
		AssertEquals(parent.PK, engine.CEG_ParentID);
		AssertNotNullOrEmpty(engine.CEG_DataModel);
		AssertEquals(parent.CVH_DataModel, engine.CEG_DataModel);
	}

	public void TestEngineSaving()
	{
		CombineAssertions(() =>
		{
			var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobDeclaration.JE_ClusterKey = 111;

			var vehicle = Factory.NewWithValidTestData<CusVehicle>();
			vehicle.CVH_ParentID = jobDeclaration.PK;
			vehicle.CVH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			vehicle.CVH_ClusterKey = 111;
			vehicle.CVH_DataModel = "AU";

			var engine = Factory.New<CusEngine>();
			engine.CEG_ParentID = vehicle.PK;
			engine.CEG_ParentTableCode = CusVehicleSchema.Constants.Prefix;
			engine.CEG_DataModel = "AU";
			engine.CEG_ClusterKey = 111;
			AssertEquals("Engine is empty", true, engine.IsEmpty);
			Factory.Save();
			AssertEquals("Engine is not saved when empty", false, engine.IsInDatabase);

			engine.CEG_EngineNumber = "ENGINE1";
			Factory.Save();
			AssertEquals("Engine is not empty", false, engine.IsEmpty);
			AssertEquals("Engine is saved when not empty", true, engine.IsInDatabase);

			engine.CEG_EngineNumber = "";
			Factory.Save();
			AssertEquals("Engine is empty again after clearing VIN", true, engine.IsEmpty);
			AssertEquals("Engine is still saved even when it is empty", true, engine.IsInDatabase);
			AssertEquals("Engine is not deleted when it is empty", false, engine.IsDeleted);
		});
	}

	public void TestParent()
	{
		var parent1 = Factory.New<CusVehicle>();
		var parent2 = Factory.New<CusVehicle>();
		var engine = Factory.New<CusEngine>();
		engine.CEG_ParentTableCode = parent1.TablePrefix;
		engine.CEG_ParentID = parent1.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Parent should be parent1", parent1, engine.Parent);

			engine.CEG_ParentID = parent2.PK;
			AssertEquals("Parent should be parent2", parent2, engine.Parent);

			engine.CEG_ParentTableCode = "XX";
			AssertNull("Parent", engine.Parent);
		});
	}

	public void TestIsEmpty()
	{
		var engine = Factory.New<CusEngine>();
		Assert(engine.IsEmpty);

		var propertiesToExclude = new[]
		{
			engine.CEG_ClusterKeyInfo,
			engine.CEG_DataModelInfo,
			engine.CEG_ParentIDInfo,
			engine.CEG_ParentTableCodeInfo,
			engine.CEG_SystemCreateTimeUtcInfo,
			engine.CEG_SystemCreateUserInfo,
			engine.CEG_SystemLastEditTimeUtcInfo,
			engine.CEG_SystemLastEditUserInfo
		};
		var propertiesToTest = engine.ZPropertyInfoHash.OfType<ZPropertyInfo>().Except(propertiesToExclude);

		CombineAssertions(() =>
		{
			foreach (var property in propertiesToTest)
			{
				var testValue = "1234567890";
				if (property.PropertyType == typeof(ZByte))
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
				AssertEquals($"After setting value for property '{property.Name}'", false, engine.IsEmpty);

				property.ClearValue();
				AssertEquals($"After clearing value for property '{property.Name}'", true, engine.IsEmpty);
			}
		});
	}

	public void TestCEG_DataModel_ReportErrorWhenSetToEmpty() => DataModelTestHelper.RunDataModelTest_ReportErrorWhenSetToEmpty<CusEngine>(Factory, GetCusEngine);

	public void TestCEG_DataModel_ReportErrorWhenUpdated() => DataModelTestHelper.RunDataModelTest_ReportErrorWhenUpdated<CusEngine>(Factory, GetCusEngine);

	public void TestCEG_DataModel_CanSaveTwice() => DataModelTestHelper.RunDataModelTest_CanSaveTwice<CusEngine>(Factory, GetCusEngine);

	CusEngine GetCusEngine(BusinessObjectFactory factory)
	{
		var engine = (CusEngine)GetNewBusinessObject();
		var parent = (CusVehicle)engine.Parent;
		parent.CVH_DataModel = "AU";
		return engine;
	}

	protected override Type ExpectedLookupsType => typeof(CusEngineLookups);
	protected override Type ExpectedValidationType => typeof(CusEngineValidation);
}
