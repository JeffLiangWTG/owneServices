using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class DataSetUpdaterHelperTest : TestCase
	{
		public void TestGetUpdaterName()
		{
			var tableName = nameof(RefUNLOCO);
			AssertEquals("REF-RefUNLOCO", DataSetUpdaterHelper.GetUpdaterName(tableName, UpdaterType.REF));
			AssertEquals("RDU-RefUNLOCO", DataSetUpdaterHelper.GetUpdaterName(tableName, UpdaterType.RDU));
		}

		public void TestGetTableName()
		{
			var updaterName = "REF-RefUNLOCO";
			AssertEquals("RefUNLOCO", DataSetUpdaterHelper.GetTableName(updaterName));

			updaterName = "RDU-RefUNLOCO";
			AssertEquals("RefUNLOCO", DataSetUpdaterHelper.GetTableName(updaterName));
		}

		public void TestGetUpdaterType()
		{
			var updaterName = "REF-RefUNLOCO";
			AssertEquals(UpdaterType.REF, DataSetUpdaterHelper.GetUpdaterType(updaterName));

			updaterName = "RDU-RefUNLOCO";
			AssertEquals(UpdaterType.RDU, DataSetUpdaterHelper.GetUpdaterType(updaterName));

			updaterName = "RefUNLOCO";
			AssertExceptionThrown<InvalidOperationException>(() => DataSetUpdaterHelper.GetUpdaterType(updaterName));
		}
	}
}
