using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentDocManagerInfo))]
	public class HVLVItemDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestDocManagerInfo()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			AssertEquals("Should return doc manager info from parent consignment", consignment.DocManagerInfo(), item.DocManagerInfo);
		}

		#region Implementations

		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<HVLVConsignment>();

		public override BusinessObject GetPopulatedParentBusinessObject() => Factory.NewWithValidTestData<HVLVConsignment>();

		#endregion
	}
}
