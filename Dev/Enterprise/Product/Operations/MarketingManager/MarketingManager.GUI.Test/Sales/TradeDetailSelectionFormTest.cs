using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailSelectionForm))]
	class TradeDetailSelectionFormTest : ZFormBasherTest
	{
		#region Form Caption & control text

		public void TestFormCaption()
		{
			using (var form = GetNewForm())
			{
				AssertEquals("Trade Detail Selection", form.FormCaption);
			}
		}

		public void TestReasonLabelText()
		{
			using (var form = GetNewForm())
			{
				form.Show();
				var reasonLabels = form.Controls.Find("reasonLabel", true);
				AssertEquals(1, reasonLabels.Length);
				AssertEquals("Please select a trade lane detail that you wish to create an one off quote for.", reasonLabels[0].Text);
			}
		}

		#endregion

		#region Columns

		public void TestShowTradeLaneColumns()
		{
			var org = Factory.New<OrgHeader>();
			var collection = new EntityTradeDetailWrapperCollection(org);

			using (var form = new TradeDetailSelectionForm(collection))
			{
				AssertEquals("Should show by default", true, form.ShowTradeLaneColumns);

				var grid = form.TradeDetailsGrid;
				var unavailableColumnNames = new HashSet<string>(grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsUnavailable).Select(x => x.ColumnName));
				AssertContainsExactElementsInAnyOrder(
					Enumerable.Empty<string>(),
					unavailableColumnNames);
				AssertEquals("18839db7-2572-4e42-830b-fff5b04f92a2", grid.GridId);
				AssertEquals("tradeDetailsGrid", grid.LayoutKey);

				form.ShowTradeLaneColumns = false;
				unavailableColumnNames = new HashSet<string>(grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsUnavailable).Select(x => x.ColumnName));
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"Parent+OW_OriginID",
						"Parent+OW_DestinationID",
						"Parent+OW_OH_Buyer",
						"Parent+OW_OH_Supplier",
					},
					unavailableColumnNames);
				AssertEquals("18839db7-2572-4e42-830b-fff5b04f92a2.HideTradeLaneColumns", grid.GridId);
				AssertEquals("tradeDetailsGrid|HideTradeLaneColumns", grid.LayoutKey);
			}
		}

		#endregion

		#region Select

		public void TestSelect()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var sales = salesHeader.EntitySalesCollectionProductView.AddNew();
			var collection = sales.EntityTradeDetailsCollection;
			var tradeDetail1 = collection.AddNew();
			var tradeDetail2 = collection.AddNew();

			using (var form = new TradeDetailSelectionForm(collection))
			{
				form.Show();

				form.TradeDetailsGrid.SelectSingleElement(tradeDetail1);
				form.SelectButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(true, form.IsDisposed);
			}
		}

		#endregion

		#region Overrides

		TradeDetailSelectionForm GetNewForm()
		{
			var sales = Factory.New<EntitySalesWrapper>();
			return new TradeDetailSelectionForm(sales.EntityTradeDetailsCollection);
		}

		protected override Form GetFormToBashCore()
		{
			return GetNewForm();
		}

		#endregion
	}
}
