using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCarrierCombinedCollection))]
	public class USCarrierCombinedCollectionTest : ActiveBusinessObjectCollectionTestCase<USCarrierCombinedCollection>
	{
		protected override USCarrierCombinedCollection GetCollectionToTest()
		{
			return new USCarrierCombinedCollection(Factory, new ZQuery(USCarrierCombinedSchema.UI_Code, ""));
		}

		public void TestFilterBusinessObjectDefaults()
		{
			var carrierCollection = new USCarrierCombinedCollection(Factory);
			var filter = carrierCollection.FilterBusinessObjectDefaults["Mode Of Transportation:Property"];
			AssertNotNull(filter);
			AssertEquals("Mode Of Transportation", filter.FilterName);
			AssertEquals(TransportModeCodes.Codes.AirNonContainer, filter.Value);
		}
	}
}
