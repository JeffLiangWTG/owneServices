using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class CusCodeDataExtensionsTest : TestCase
	{
		public static void AssertPropertyIsStringRepresentationOfCusCodeDataCollection<T>(ZPropertyInfo info, CusCodeDataCollection<T> collection)
			where T : CusCodeData
		{
			info.Value = new ZString("123");
			AssertEquals(1, collection.Count);
			AssertEquals("123", collection[0].CY_Data);
			info.Value = new ZString("123, 456 ,789");
			AssertEquals("123, 456, 789", info.Value);
			AssertEquals(3, collection.Count);
			AssertEquals("123", collection[0].CY_Data);
			AssertEquals("456", collection[1].CY_Data);
			AssertEquals("789", collection[2].CY_Data);
			info.Value = new ZString("123,,789");
			AssertEquals("123, 789", info.Value);
			AssertEquals(2, collection.Count);
			AssertEquals("123", collection[0].CY_Data);
			AssertEquals("789", collection[1].CY_Data);
			collection.AddNew().CY_Data = "456";
			AssertEquals("123, 789, 456", info.Value);
			collection[1].Delete();
			AssertEquals("123, 456", info.Value);
			collection[0].CY_Data = ZString.Empty;
			AssertEquals(", 456", info.Value);
			info.Value = new ZString().PadRight(CusCodeData.Schema.CY_DataMaxLength + 2, '1');
			AssertEquals(CusCodeData.Schema.CY_DataMaxLength, ((ZString)info.Value).Length);
		}
	}
}
