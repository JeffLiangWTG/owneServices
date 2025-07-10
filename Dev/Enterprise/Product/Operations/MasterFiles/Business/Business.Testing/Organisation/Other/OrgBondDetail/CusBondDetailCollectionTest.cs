using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CusBondDetailCollection))]
	sealed class CusBondDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultValuesForNewChild()
		{
			CusBondDetailCollection coll = (CusBondDetailCollection)GetCollectionToTest();
			CusBondDetail element = coll.AddNew();
			AssertEquals(coll.Master.PK, element.Parent.PK);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return new CusBondDetailCollection(org);
		}
	}
}
