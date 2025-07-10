using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class DynamicTradeLaneWithDetailsControlTest : TestCaseWithFactory
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

			using (var form = new ZForm(salesHeaderAAA))
			using (var control = new DynamicTradeLaneWithDetailsControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(salesHeaderAAA, control.CurrentlyVisibleInnerControl_Exposed.CurrentDataItem);

				control.SetDataBinding(salesHeaderSHP, null);
				AssertEquals(salesHeaderSHP, control.CurrentlyVisibleInnerControl_Exposed.CurrentDataItem);

				control.SetDataBinding(salesHeaderBBB, null);
				AssertEquals(salesHeaderBBB, control.CurrentlyVisibleInnerControl_Exposed.CurrentDataItem);

				control.SetDataBinding(salesHeaderSHP, null);
				AssertEquals(salesHeaderSHP, control.CurrentlyVisibleInnerControl_Exposed.CurrentDataItem);
			}
		}

		public void TestShowControlForCurrentDataItem_ShouldNotBindToIncorrectHeaderWhenHidden()
		{
			var productAAA = Factory.New<OrgSalesProduct>();
			productAAA.MP_Code = "AAA";
			var productSHP = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = new SalesHeaderCollection(org);
			var salesHeaderAAA = salesHeaderCollection.AddNew(productAAA);
			var salesHeaderSHP = salesHeaderCollection.AddNew(productSHP);

			using (var form = new ZForm(salesHeaderAAA))
			using (var control = new DynamicTradeLaneWithDetailsControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var salesHeaderAAAControl = control.CurrentlyVisibleInnerControl_Exposed;
				AssertEquals(salesHeaderAAA, salesHeaderAAAControl.CurrentDataItem);

				control.SetDataBinding(salesHeaderSHP, null);
				AssertEquals(salesHeaderSHP, control.CurrentlyVisibleInnerControl_Exposed.CurrentDataItem);
				AssertEquals("Should not have changed binding of salesHeaderAAAControl", salesHeaderAAA, salesHeaderAAAControl.CurrentDataItem);
			}
		}
	}
}
