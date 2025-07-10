using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(SealNumberBusinessObjectCollection))]
	class SealNumberBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SealNumberBusinessObjectCollection>
	{
		public void TestLoad()
		{
			ZString longSealNumber = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
			var sealNumbersAsCommaSeperateString = "1,2,3,4" + "," + longSealNumber;
			var coll = new SealNumberBusinessObjectCollection(sealNumbersAsCommaSeperateString);
			AssertEquals(5, coll.Count);
			AssertNotNull(coll.Cast<SealNumberBusinessObject>().FirstOrDefault(x => x.SealNumber == "1"));
			AssertNotNull(coll.Cast<SealNumberBusinessObject>().FirstOrDefault(x => x.SealNumber == "2"));
			AssertNotNull(coll.Cast<SealNumberBusinessObject>().FirstOrDefault(x => x.SealNumber == "3"));
			AssertNotNull(coll.Cast<SealNumberBusinessObject>().FirstOrDefault(x => x.SealNumber == "4"));
			AssertNotNull(coll.Cast<SealNumberBusinessObject>().FirstOrDefault(x => x.SealNumber == longSealNumber.Left(SealNumberBusinessObject.Schema.SealNumberMaxLength)));
		}

		public void PopulateSealNumbersToParent()
		{
			var coll = new SealNumberBusinessObjectCollection(ZString.Empty);
			coll.AddNew().SealNumber = "1";
			coll.AddNew().SealNumber = "2";
			coll.AddNew().SealNumber = "3";
			coll.AddNew().SealNumber = "4";
			AssertEquals("1,2,3,4", coll.GetSealNumbersAsCommaSepereateString());
		}

		protected override SealNumberBusinessObjectCollection GetCollectionToTest()
		{
			return new SealNumberBusinessObjectCollection(ZString.Empty);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SealNumberBusinessObject(ZString.Empty);
		}
	}
}
