using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVClearanceDocManagerInfo))]
	class CusUSLVClearanceDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestRelatedObjects()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			var consignment3 = Factory.NewWithValidTestData<CusUSLVClearance>();

			AssertContainsExactElementsInAnyOrder(new[] { consignment1, consignment2 }, ((IEDocsProvider)clearance).DocManagerInfo.RelatedObjects);
		}

		#region Implementation

		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<CusUSLVClearance>();

		public override BusinessObject GetPopulatedParentBusinessObject() => Factory.NewWithValidTestData<CusUSLVClearance>();

		#endregion
	}
}
