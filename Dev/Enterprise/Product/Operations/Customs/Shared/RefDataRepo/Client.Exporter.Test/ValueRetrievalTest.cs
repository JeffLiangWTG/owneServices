using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter.Test
{
	class ValueRetrievalTest : TestCase
	{
		public void TestGetValue_BoolConvertToString()
		{
			var dbHelper = new Mock<IDBHelper>();
			var retrieval = new ValueRetrieval(dbHelper.Object);
			var code = new Mock<IDataRow>();
			code.Setup(x => x[nameof(IZZRefCusCodeListCombined.ZZD_IsAir)]).Returns(true);
			AssertEquals("Air", retrieval.GetValue(code.Object, typeof(IZZRefCusCodeListCombined),
				typeof(string), nameof(IZZRefCusCodeListCombined.ZZD_IsAir)));
		}

		public void TestGetStorageTypeFromName()
		{
			var dbHelper = new Mock<IDBHelper>();
			var retrieval = new ValueRetrieval(dbHelper.Object);
			var tableName = "ZZRefCusCodeListCombined";
			AssertNotNull(retrieval.GetStorageTypeFromName(tableName));
		}
	}
}
