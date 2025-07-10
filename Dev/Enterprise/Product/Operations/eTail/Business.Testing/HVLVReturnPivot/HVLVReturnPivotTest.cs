using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVReturnPivot))]
	class HVLVReturnPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateFromConsignments()
		{
			var formerConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var returnConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			formerConsignment.HVC_GoodsDescription = "CONS1";
			returnConsignment.HVC_GoodsDescription = "CONS2";
			Factory.Save();

			var returnPivot = HVLVReturnPivot.CreateFromConsignments(Factory, formerConsignment, returnConsignment);

			CombineAssertions(() =>
			{
				AssertEquals("HVP_HVC_Former", formerConsignment.PK, returnPivot.HVP_HVC_Former);
				AssertEquals("HVP_HVC_Return", returnConsignment.PK, returnPivot.HVP_HVC_Return);
				AssertEquals("FormerConsignment.HVC_GoodsDescription", formerConsignment.HVC_GoodsDescription, returnPivot.FormerConsignment.HVC_GoodsDescription);
				AssertEquals("ReturnConsignment.HVC_GoodsDescription", returnConsignment.HVC_GoodsDescription, returnPivot.ReturnConsignment.HVC_GoodsDescription);
			});
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() =>
			HVLVReturnPivot.CreateFromConsignments(Factory, Factory.NewWithValidTestData<HVLVConsignment>(), Factory.NewWithValidTestData<HVLVConsignment>());

		#endregion
	}
}
