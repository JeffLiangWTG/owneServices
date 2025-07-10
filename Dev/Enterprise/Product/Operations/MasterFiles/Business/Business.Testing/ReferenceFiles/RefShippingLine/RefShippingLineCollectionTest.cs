using System.Collections;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefShippingLineCollection))]
	sealed class RefShippingLineCollectionTest : ActiveBusinessObjectCollectionTestCase<RefShippingLineCollection>
	{
		public void TestUserFilterDefaults()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var refShippingLineCollection = new RefShippingLineCollection(Factory, org);
			(refShippingLineCollection.FilterBusinessObjectDefaults as IEnumerable).GetEnumerator(); // Mock Add FilterBusinessObjectDefault to filter
			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals("OH_IsShippingLine is false.", false, org.OH_IsShippingLine);
				AssertEquals("OH_IsSeaWholesaler is false.", false, org.OH_IsSeaWholesaler);
				AssertEquals("No default filters are set.", false, refShippingLineCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("NVO Status:Property"));
			});

			org.OH_IsShippingLine = true;
			(refShippingLineCollection.FilterBusinessObjectDefaults as IEnumerable).GetEnumerator(); // Mock Add FilterBusinessObjectDefault to filter
			AssertEquals("A default filter is set.", true, refShippingLineCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("NVO Status:Property"));
			AssertEquals("Default NVO Status should be Not NVO.", "Not NVO", refShippingLineCollection.FilterBusinessObjectDefaults["NVO Status:Property"].Value);

			org.OH_IsShippingLine = false;
			org.OH_IsSeaWholesaler = true;
			(refShippingLineCollection.FilterBusinessObjectDefaults as IEnumerable).GetEnumerator(); // Mock Add FilterBusinessObjectDefault to filter
			AssertEquals("A default filter is set.", true, refShippingLineCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("NVO Status:Property"));
			AssertEquals("Default NVO Status should be NVO.", "NVO", refShippingLineCollection.FilterBusinessObjectDefaults["NVO Status:Property"].Value);

			org.OH_IsSeaWholesaler = false;
			(refShippingLineCollection.FilterBusinessObjectDefaults as IEnumerable).GetEnumerator(); // Mock Add FilterBusinessObjectDefault to filter
			AssertEquals("Default filters are removed.", false, refShippingLineCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("NVO Status:Property"));
		}
	}
}
