using System;
using System.Drawing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class ReceiveProductSummaryGridUserControl : ZUserControl
	{
		public ReceiveProductSummaryGridUserControl()
		{
			InitializeComponent();
		}

		#region OnCurrentDataItemChanged

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (SummaryLinesGrid != null)
			{
				SummaryLinesGrid.ColourDeciding -= Grid_ColourDeciding;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (SummaryLinesGrid != null)
			{
				SummaryLinesGrid.ColourDeciding += Grid_ColourDeciding;
			}
		}

		#region Grid_ColourDeciding

		void Grid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var summary = e.ObjectAtRow as WhsReceiveProductSummary;
			if (summary != null)
			{
				if (summary.ReceivedQuantity > summary.ExpectedQuantity)
				{
					e.Colour = Color.LightGreen;
				}
				else if (summary.ReceivedQuantity < summary.ExpectedQuantity)
				{
					e.Colour = Color.LightSalmon;
				}
			}
		}

		#endregion

		#endregion

		#region ReleaseLinesGrid

		public class ProductSummaryGrid : ZGrid
		{
			// WhsProductSummaryCollection rebuilds itself when you access either its Count property or you Enumerate it.
			// When Disposing the Receive Form, the Count property is accessed which rebuilds the Collection
			// unnecessarily. To prevent this we suspend rebuilding the collection during Dispose.
			public override void SetDataBinding(object dataSource, string dataMember, string tableName)
			{
				var isDisposing = (dataSource == null);
				var productSummaries = isDisposing && ListManager != null ? ListManager.List as WhsReceiveProductSummaryCollection : null;

				using (productSummaries?.SuspendRebuild())
				{
					base.SetDataBinding(dataSource, dataMember, tableName);
				}
			}
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class ReceiveProductSummaryGridUserControl
	{
		public void OnColourDecidingForTest(ColourDecidingEventArgs args)
		{
			Grid_ColourDeciding(null, args);
		}
	}
}

#endif
#endregion
