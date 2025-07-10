using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TransportTradeDetailsControl))]
	class TransportTradeDetailsControlTest : TradeDetailsControlBaseTest
	{
		public void TestTransportTradeDetailsGrid_ShouldPopulateNewDetailWithOpportunityStatus()
		{
			var transportProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);
			var org = Factory.New<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			opp1.P8_Status = "WON";
			AssertEquals("Precondition", OpportunityTradeStatus.Codes.Successful, OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetTradeStatusFromCode(opp1.P8_Status));
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = transportProduct.PK;
			var entitySales = EntitySalesWrapper.Get(sales, opp1);

			using (var form = new TradeDetailsControlFormForTest(entitySales))
			using (var control = new TransportTradeDetailsControlForTest(transportProduct))
			{
				form.Controls.Add(control);
				control.SetDataBinding(entitySales, ZString.Empty);
				control.OnCurrentDataItemChangedExposed();
				form.Show();

				AssertEquals("One new detail should be added", 1, entitySales.EntityTradeDetailsCollection.Count);
				AssertEquals("This detail should adopt the status of the opportunity", OpportunityTradeStatus.Codes.Successful, entitySales.EntityTradeDetailsCollection[0].PA_Status);
			}
		}

		protected override TradeDetailsControl GetNewControlForTest()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);
			return new TransportTradeDetailsControl(product);
		}

		public void TestTopSplitContainer()
		{
			var transportProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);

			using (var form = new ZForm())
			using (var control = new TransportTradeDetailsControl(transportProduct))
			{
				form.Controls.Add(control);
				form.Show();

				Assert(control.topSplitContainer.IsSplitterFixed);
				AssertEquals(FixedPanel.Panel2, control.topSplitContainer.FixedPanel);
			}
		}

		//class TransportTradeDetailsControlForTest : TransportTradeDetailsControl
		//{
		//	public TransportTradeDetailsControlForTest(OrgSalesProduct salesProduct)
		//		: base(salesProduct)
		//	{
		//		grid = new ZGrid();
		//		this.BindingSource.SetBindingMember(grid, "EntityTradeDetailsCollection");
		//		zDropEditColumnStyleInfo1.ColumnName = "PA_TradeMode";
		//		this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);

		//		Controls.Add(grid);
		//	}

		//	public override ZGrid TradeDetailsGrid
		//	{
		//		get { return grid; }
		//	}

		//	public void OnCurrentDataItemChangedExposed()
		//	{
		//		OnCurrentDataItemChanged(null);
		//	}

		//	Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
		//	Enterprise.ZArchitecture.ZGrid grid;
		//}

		class TransportTradeDetailsControlForTest : TransportTradeDetailsControl
		{
			public TransportTradeDetailsControlForTest(OrgSalesProduct salesProduct)
				: base(salesProduct)
			{
			}

			public void OnCurrentDataItemChangedExposed()
			{
				OnCurrentDataItemChanged(null);
			}
		}
	}
}
