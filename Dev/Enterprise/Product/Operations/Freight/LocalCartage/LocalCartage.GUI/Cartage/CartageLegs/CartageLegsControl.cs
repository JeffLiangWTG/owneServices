using System;
using Enterprise.Freight.GUI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageLegsControl : ZUserControl
	{
		public CartageLegsControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				CartageLegsGrid.InnerGrid.ColorContextKey = LegGridColourScheme.LegColourKey;
				CartageLegsGrid.InnerGrid.ShareActiveColorScheme = false;

				new UNDGDataItemFormManager(CartageLegsGrid.InnerGrid, "BookedCtgMove").Initialize(CartageLegWithDetailsPanel.DGLinkLabel, CartageLegWithDetailsPanel.DGSubstanceGuidFindBox, CartageLegWithDetailsPanel.FlashPointCalcEdit, CartageLegWithDetailsPanel.DGContactGuidFindBox);
			}

			FreightCalculateDistanceMenuHelper.SetupCalculateDistanceContextMenu(CartageLegsGrid.InnerGrid);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentException("dataMember parameter not supported", nameof(dataMember));
			}

			Unhook();

			CommonCartage cartage = dataSource != null ? (CommonCartage)dataSource : null;
			base.SetDataBinding(cartage, "");

			Hook();
		}

		void Hook()
		{
			if (Cartage != null)
			{
				workflowCustomFields = WorkflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(CartageLegsGrid.InnerGrid, Cartage.CartageLegs, true);
			}
		}

		void Unhook()
		{
			if (workflowCustomFields != null)
			{
				workflowCustomFields.Dispose();
				workflowCustomFields = null;
			}
		}

		IDisposable workflowCustomFields;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Unhook();

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public CommonCartage Cartage
		{
			get { return (CommonCartage)DataSource; }
		}
	}

	public static class LegGridColourScheme
	{
		public static readonly string LegColourKey = "{1BF5B00D-488E-42cb-B99A-F1B02681EE5F}";
	}
}
