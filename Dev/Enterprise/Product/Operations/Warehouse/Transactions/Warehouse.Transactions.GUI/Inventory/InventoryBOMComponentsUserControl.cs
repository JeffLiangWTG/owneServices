using System;
using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class InventoryBOMComponentsUserControl : ZUserControl
	{
		#region Constructor

		public InventoryBOMComponentsUserControl()
		{
			InitializeComponent();
		}

		#endregion

		#region ComponentInfoGrid_MouseDoubleClick

		void ComponentInfoGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var currentComponentLine = (WhsDocketLine)ComponentDetailsGrid.ListManager.GetCurrent();

			if (currentComponentLine != null)
			{
				InventoryHelper.ViewReceipt(currentComponentLine);
			}
		}

		#endregion

		#region Column Control

		WhsDocketLine DocketLine => BindingSource.DataSource as WhsDocketLine;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (DocketLine != null)
			{
				CustomsDataGridHelper.ShowHideCustomsData(ComponentDetailsGrid, DocketLine.Docket, Array.Empty<string>());

				PartAttributeColumnManager.SetColumns(DocketLine.Docket.Client);
			}
		}

		#endregion

		#region PartAttributeColumnManager

		PartAttributeColumnManager PartAttributeColumnManager
		{
			get
			{
				if (partAttributeColumnManager == null)
				{
					partAttributeColumnManager = new PartAttributeColumnManager(ComponentDetailsGrid,
						WhsDocketLineSchema.WE_ExpiryDate.Name,
						WhsDocketLineSchema.WE_PackingDate.Name,
						WhsDocketLineSchema.WE_PartAttrib1.Name,
						WhsDocketLineSchema.WE_PartAttrib2.Name,
						WhsDocketLineSchema.WE_PartAttrib3.Name,
						WhsDocketLineSchema.WE_SerialNumber.Name);
				}
				return partAttributeColumnManager;
			}
		}
		PartAttributeColumnManager partAttributeColumnManager;

		#endregion

	}
}
