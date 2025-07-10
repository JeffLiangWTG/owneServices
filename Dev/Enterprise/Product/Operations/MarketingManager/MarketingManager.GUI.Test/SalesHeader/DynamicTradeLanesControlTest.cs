using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class DynamicTradeLanesControlTest : TestCaseWithFactory
	{
		public void TestShowControlForCurrentDataItem()
		{
			var productAAA = Factory.New<OrgSalesProduct>();
			productAAA.MP_Code = "AAA";
			var productBBB = Factory.New<OrgSalesProduct>();
			productBBB.MP_Code = "BBB";
			var productSHP = Factory.New<OrgSalesProduct>();
			productSHP.MP_Code = SystemDefinedSalesProductList.Codes.ForwardingShipment;

			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = new SalesHeaderCollection(org);
			var salesHeaderAAA = salesHeaderCollection.AddNew(productAAA);
			var salesHeaderBBB = salesHeaderCollection.AddNew(productBBB);
			var salesHeaderSHP = salesHeaderCollection.AddNew(productSHP);

			using (var form = new ZForm(new OpportunitySalesValueAnalysis(salesHeaderAAA)))
			using (var control = new DynamicTradeLanesControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(new OpportunitySalesValueAnalysis(salesHeaderAAA), null);
				AssertType(typeof(GenericTradeLanesControl), control.CurrentlyVisibleInnerControl_Exposed);

				control.SetDataBinding(new OpportunitySalesValueAnalysis(salesHeaderSHP), null);
				AssertType(typeof(ForwardingShipmentTradeLanesControl), control.CurrentlyVisibleInnerControl_Exposed);

				control.SetDataBinding(new OpportunitySalesValueAnalysis(salesHeaderBBB), null);
				AssertType(typeof(GenericTradeLanesControl), control.CurrentlyVisibleInnerControl_Exposed);

				control.SetDataBinding(new OpportunitySalesValueAnalysis(salesHeaderSHP), null);
				AssertType(typeof(ForwardingShipmentTradeLanesControl), control.CurrentlyVisibleInnerControl_Exposed);
			}
		}
	}
}
