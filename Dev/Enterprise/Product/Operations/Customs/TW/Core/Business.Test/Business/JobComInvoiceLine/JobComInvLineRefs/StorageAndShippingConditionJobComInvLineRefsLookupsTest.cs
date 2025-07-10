using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(StorageAndShippingConditionJobComInvLineRefsLookups))]
	sealed class StorageAndShippingConditionJobComInvLineRefsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceTypeList()
		{
			CombineAssertions(() =>
			{
				var referenceTypeList = lookups.ReferenceTypeList;
				AssertEquals("Values", "1, 2, 3, 4, 5", referenceTypeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<CPT_122_StorageShippingConditionList>(), referenceTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var storageAndShippingConditionJobComInvLineRefs = Factory.New<StorageAndShippingConditionJobComInvLineRefs>();
			lookups = new StorageAndShippingConditionJobComInvLineRefsLookups(storageAndShippingConditionJobComInvLineRefs);
		}
		StorageAndShippingConditionJobComInvLineRefsLookups lookups;
	}
}
