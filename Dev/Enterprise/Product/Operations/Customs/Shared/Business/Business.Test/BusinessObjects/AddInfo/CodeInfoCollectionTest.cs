using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CodeInfoCollection))]
	public abstract class CodeInfoCollectionTest<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : CodeInfoCollection
	{
		public void TestLoadFromString()
		{
			CodeInfoCollection collection = GetCollectionToTest();
			collection.RemoveAndDeleteAll();
			collection.LoadFromString("A=B^C=D");
			AssertEquals("There should be two elements in the collection", 2, collection.Count);
			AssertEquals("There should be an element with code A", true, collection.ContainsCode("A"));
			AssertEquals("There should be an element with code C", true, collection.ContainsCode("C"));
		}

		public void TestToString()
		{
			CodeInfoCollection collection = GetCollectionToTest();
			collection.RemoveAndDeleteAll();
			CodeInfo codeInfo = collection.AddNew();
			codeInfo.ZO_Code = "A";
			codeInfo.ZO_Data = "BB";

			CodeInfo codeInfo2 = collection.AddNew();
			codeInfo2.ZO_Code = "C";
			codeInfo2.ZO_Data = "DD";

			AssertEquals("A=BB^C=DD", collection.ToString());
		}

		public void TestContainsCode()
		{
			CodeInfoCollection collection = GetCollectionToTest();
			collection.RemoveAndDeleteAll();
			CodeInfo codeInfo = collection.AddNew();
			codeInfo.ZO_Code = "A";
			codeInfo.ZO_Data = "BB";

			CodeInfo codeInfo2 = collection.AddNew();
			codeInfo2.ZO_Code = "C";
			codeInfo2.ZO_Data = "DD";

			AssertEquals("ContainsCode", true, collection.ContainsCode("A"));
			AssertEquals("ContainsCode", false, collection.ContainsCode("B"));
		}

		public void TestGetElementWithThisCode()
		{
			CodeInfoCollection collection = GetCollectionToTest();
			collection.RemoveAndDeleteAll();
			CodeInfo codeInfo = collection.AddNew();
			codeInfo.ZO_Code = "A";
			codeInfo.ZO_Data = "BB";

			CodeInfo codeInfo2 = collection.AddNew();
			codeInfo2.ZO_Code = "C";
			codeInfo2.ZO_Data = "DD";

			AssertEquals(codeInfo, collection.GetElementWithThisCode("A"));
			AssertEquals(codeInfo2, collection.GetElementWithThisCode("C"));
			AssertEquals(null, collection.GetElementWithThisCode("B"));
		}

		public void TestUpdateAddInfoString()
		{
			CodeInfoCollection collection = GetCollectionToTest();
			collection.RemoveAndDeleteAll();
			CodeInfo codeInfo = collection.AddNew();
			codeInfo.ZO_Code = "A";
			codeInfo.ZO_Data = "BB";
			AssertEquals("AddInfoString should be updated", "A=BB", collection.addInfoStringInfo.Value);

			CodeInfo codeInfo2 = collection.AddNew();
			codeInfo2.ZO_Code = "C";
			codeInfo2.ZO_Data = "DD";
			AssertEquals("AddInfoString should be updated", "A=BB^C=DD", collection.addInfoStringInfo.Value);
		}

		public void TestAggregatedCodesAndDatas()
		{
			CodeInfoCollection collection = GetCollectionToTest();
			collection.RemoveAndDeleteAll();
			CodeInfo codeInfo = collection.AddNew();
			codeInfo.ZO_Code = "A";
			codeInfo.ZO_Data = "BB";

			CodeInfo codeInfo2 = collection.AddNew();
			codeInfo2.ZO_Code = "C";
			codeInfo2.ZO_Data = "DD";

			AssertEquals("AggregatedCodes", "A/C", collection.AggregatedCodes);
			AssertEquals("AggregatedDatas", "BB/DD", collection.AggregatedDatas);
		}
	}
}
