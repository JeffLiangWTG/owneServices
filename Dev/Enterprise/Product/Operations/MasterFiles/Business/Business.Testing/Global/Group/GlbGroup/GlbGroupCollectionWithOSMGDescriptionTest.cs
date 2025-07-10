using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupCollectionWithOSMGDescription))]
	sealed class GlbGroupCollectionWithOSMGDescriptionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbGroupCollectionWithOSMGDescription(Factory);
		}

		public void TestOSMGDescription()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "GG_Code1";
			group1.GG_Desc = "GG_Desc2";

			var collection = new GlbGroupCollectionWithOSMGDescription(Factory);
			collection.Add(group1);

			var provider = (collection as IFindBoxListProvider);
			AssertEquals("GG_Desc2", provider.DescriptionFromPrimaryKey(group1.PK));
			AssertEquals("Unassigned", provider.DescriptionFromPrimaryKey(ZGuid.Empty));
		}
	}
}
