using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentProcessTaskCollection))]
	public class HVLVConsignmentProcessTaskCollectionTest : ProcessTaskCollectionTest<HVLVConsignmentProcessTaskCollection>
	{
		public void TestCountryConditions()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var taskCollection = new HVLVConsignmentProcessTaskCollection(consignment);

			AssertEquals(ZString.Empty, taskCollection.OriginCountry);
			AssertEquals(ZString.Empty, taskCollection.DestinationCountry);

			consignment.HVC_RN_NKShipperCountryCode = "AU";
			consignment.HVC_RN_NKConsigneeCountryCode = "NZ";

			AssertEquals("AU", taskCollection.OriginCountry);
			AssertEquals("NZ", taskCollection.DestinationCountry);
		}

		protected override HVLVConsignmentProcessTaskCollection GetCollectionToTestCore() =>
			new HVLVConsignmentProcessTaskCollection(Factory.NewWithValidTestData<HVLVConsignment>());
	}
}
