using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ItemIdentityNumbersMultipleCodeCollection))]
	class ItemIdentityNumbersMultipleCodeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ItemIdentityNumbersMultipleCodeCollection>
	{
		public void TestLoadAndPopulate()
		{
			var codesAsCommaSeparatedString = "AS,BD,GT,2012," + "87845";
			var coll = new ItemIdentityNumbersMultipleCodeCollection(codesAsCommaSeparatedString, Factory);
			AssertEquals(5, coll.Count);
			AssertNotNull(coll.Cast<CommaSeparatedNumber>().FirstOrDefault(x => x.Number == "AS"));
			AssertNotNull(coll.Cast<CommaSeparatedNumber>().FirstOrDefault(x => x.Number == "BD"));
			AssertNotNull(coll.Cast<CommaSeparatedNumber>().FirstOrDefault(x => x.Number == "GT"));
			AssertNotNull(coll.Cast<CommaSeparatedNumber>().FirstOrDefault(x => x.Number == "2012"));
			AssertNotNull(coll.Cast<CommaSeparatedNumber>().FirstOrDefault(x => x.Number == "87845"));

			coll = new ItemIdentityNumbersMultipleCodeCollection(ZString.Empty, Factory);
			coll.AddNew().Number = "FR";
			coll.AddNew().Number = "IT";
			coll.AddNew().Number = "US";
			coll.AddNew().Number = "CA";
			AssertEquals("FR,IT,US,CA", coll.GetCodesAsCommaSeparatedString());
		}

		protected override ItemIdentityNumbersMultipleCodeCollection GetCollectionToTest()
		{
			return new ItemIdentityNumbersMultipleCodeCollection(ZString.Empty, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ItemIdentityNumbersMultipleCode();
		}
	}
}
