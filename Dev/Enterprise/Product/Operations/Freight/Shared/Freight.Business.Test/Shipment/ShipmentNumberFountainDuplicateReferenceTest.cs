using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentNumberFountainDuplicateReferenceTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSavingSetJS_UniqueConsignRef_NoDuplicateReferenceException()
		{
			var oldValue = Env.Registry.AllowManualShipmentEntry;
			Env.Registry.AllowManualShipmentEntry = true;
			AssertPopulateBillAndShipmentNumberIfNeededCore();

			Env.Registry.AllowManualShipmentEntry = false;
			AssertPopulateBillAndShipmentNumberIfNeededCore();

			Env.Registry.AllowManualShipmentEntry = oldValue;
		}

		void AssertPopulateBillAndShipmentNumberIfNeededCore()
		{
			var factory1 = new BusinessObjectFactory();
			var shipment = factory1.NewWithValidTestData<CommonShipmentForTest>();
			shipment.JS_UniqueConsignRef = ZString.Empty;
			var dbConnection = ((IDbConnected)factory1).Connection;
			ZString uniqueConsignRef = AssignNumberFromFountainAndRollbackTransaction(dbConnection, shipment);

			var newFactory = new BusinessObjectFactory();
			var shipment2 = newFactory.NewWithValidTestData<CommonShipmentForTest>();
			shipment2.JS_UniqueConsignRef = ZString.Empty;
			newFactory.Save();
			var jsRef = shipment2.JS_UniqueConsignRef;
			Assert("JS_UniqueConsignRef is not empty", jsRef != ZString.Empty);
			AssertEquals("shipment2 should have the same uniqueConsignRef ", uniqueConsignRef, shipment2.JS_UniqueConsignRef);

			jsRef = shipment.JS_UniqueConsignRef;
			Assert("JS_UniqueConsignRef is not empty", jsRef != ZString.Empty);
			AssertEquals("shipment is assigned with the old uniqueConsignRef before saving", uniqueConsignRef, jsRef);
			factory1.Save();

			jsRef = shipment.JS_UniqueConsignRef;
			Assert("JS_UniqueConsignRef is not empty", jsRef != ZString.Empty);
			AssertNotEquals("shipment is assigned with a new uniqueConsignRef before saving", uniqueConsignRef, jsRef);
		}

		ZString AssignNumberFromFountainAndRollbackTransaction(DbConnection dbConnection, CommonShipmentForTest shipment)
		{
			ZString uniqueConsignRef;
			try
			{
				dbConnection.BeginTransaction();
				shipment.PopulateBillAndShipmentNumberIfNeeded();
				uniqueConsignRef = shipment.JS_UniqueConsignRef;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}
			return uniqueConsignRef;
		}
	}
}
