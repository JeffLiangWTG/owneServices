using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusVehicle))]
	public abstract class CusVehicleAbstractTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookupsType()
		{
			var vehicle = (CusVehicle)GetNewBusinessObject();
			AssertType(ExpectedLookupsType, vehicle.Lookups);
		}

		public void TestValidationType()
		{
			var vehicle = (CusVehicle)GetNewBusinessObject();
			AssertType(ExpectedValidationType, vehicle.Validation);
		}

		protected abstract Type ExpectedLookupsType { get; }
		protected abstract Type ExpectedValidationType { get; }
		protected virtual EngineRelationshipType ExpectedEngineRelationship => EngineRelationshipType.None;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);
		protected virtual BusinessObject GetVehicleParent(BusinessObjectFactory factory) => factory.New<BaseJobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();

		CusVehicle GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var parent = GetVehicleParent(factory);
			var vehicle = (CusVehicle)factory.NewWithValidTestData(ExpectedBusinessObjectType);

			vehicle.CVH_ParentID = parent.PK;
			vehicle.CVH_ParentTableCode = parent.TablePrefix;

			return vehicle;
		}

		public void TestEngineRelationship()
		{
			var vehicle = (CusVehicle)GetNewBusinessObject();
			AssertEquals(ExpectedEngineRelationship, vehicle.EngineRelationship);
		}

		public void TestEngineRelationship_One_HandleUniqueIndex()
		{
			Factory.RefreshEnabled = false;
			var vehicle = (CusVehicle)GetNewBusinessObject();

			if (vehicle.EngineRelationship == EngineRelationshipType.One)
			{
				Factory.Save();

				var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var vehicleInAnotherFactory = (CusVehicle)anotherFactory.Load(vehicle.GetType(), vehicle.PK);

				var engine = vehicle.Engines.AddNew();
				engine.CEG_EngineNumber = "ENGINE1";
				var engineInAnotherFactory = vehicleInAnotherFactory.Engines.AddNew();
				engineInAnotherFactory.CEG_EngineNumber = "ENGINE2";

				Factory.Save();

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					try
					{
						anotherFactory.Save();
						Fail($"First save should not have succeeded, please make sure that there is a unique index on CEG_ParentID for CEG_DataModel = '{engine.CEG_DataModel}' and CEG_ParentTableCode = 'CVH'.");
					}
					catch (Exception ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}

					AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals("Engine from another factory should be deleted", true, engineInAnotherFactory.IsDeleted);

					anotherFactory.Save();

					AssertEquals("Invoice line in another factory should have the saved engine", 1, vehicleInAnotherFactory.Engines.Count);
					AssertEquals("Engine in another factory should be using the existing engine now", engine.PK, vehicleInAnotherFactory.Engines[0].PK);
				});
			}
			else
			{
				Assert("All good", true);
			}
		}
	}
}
