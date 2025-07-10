using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UniqueIndexHandlerTest : TestCaseWithFactory
	{
		public void TestLoadFromConsol()
		{
			var consolTarget = new DummyGeneratorTarget();
			consolTarget.FountainUsedForGeneration = Env.NumberFountains.JobConsolNumber;
			consolTarget.ValuePrefix = "ABC";
			consolTarget.ValueSuffix = "YZ";
			consolTarget.FountainValue = "005";
			consolTarget.Value = "ABC005YZ";

			var handler = new UniqueIndexHandler(consolTarget);
			var command = handler.FindMaxValueInDatabase(Db.Connection, JobConsolSchema.JK_UniqueConsignRef);
			var expectedQuery = "SELECT max(substring(JK_UniqueConsignRef, 4, 3)) FROM dbo.JobConsol WHERE isnumeric(substring(JK_UniqueConsignRef, 4, 3)) = 1 AND len(JK_UniqueConsignRef) = 8 AND left(JK_UniqueConsignRef, 3) = 'ABC' AND substring(JK_UniqueConsignRef, 7, 2) = 'YZ'";
			AssertEquals(expectedQuery, command.CommandText);
		}

		public void TestLoadFromShipment()
		{
			var shipmentTarget = new DummyGeneratorTarget();
			shipmentTarget.FountainUsedForGeneration = Env.NumberFountains.JobShipmentNumber;
			shipmentTarget.ValuePrefix = "SS";
			shipmentTarget.ValueSuffix = "ABCD";
			shipmentTarget.FountainValue = "016";
			shipmentTarget.Value = "SS016ABCD";

			var handler = new UniqueIndexHandler(shipmentTarget);
			var command = handler.FindMaxValueInDatabase(Db.Connection, JobShipmentSchema.JS_UniqueConsignRef);
			var expectedQuery = "SELECT max(substring(JS_UniqueConsignRef, 3, 3)) FROM dbo.JobShipment WHERE isnumeric(substring(JS_UniqueConsignRef, 3, 3)) = 1 AND len(JS_UniqueConsignRef) = 9 AND left(JS_UniqueConsignRef, 2) = 'SS' AND substring(JS_UniqueConsignRef, 6, 4) = 'ABCD'";
			AssertEquals(expectedQuery, command.CommandText);
		}

		class DummyGeneratorTarget : NumberGeneratorTarget
		{
			public DummyGeneratorTarget()
				: base()
			{
			}

			public override string NumberCustomisationLocation
			{
				get { return ""; }
			}

			protected override Registry.Business.BillOfLadingNumberCustomisation GetNumberCustomisationCore()
			{
				return null;
			}

			protected override int GetMaxLengthCore()
			{
				return 20;
			}

			protected override ZString GetNameCore()
			{
				return "DUMMY";
			}
		}
	}
}
