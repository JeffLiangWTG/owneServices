using System.ComponentModel;
using System.Linq;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI
{
	public partial class WhsUNDGLimitControl : ZUserControl
	{
		public WhsUNDGLimitControl()
		{
			InitializeComponent();
		}

		#region Binding

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue("Warehouse")]
		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return UNDGLimitGrid.BindTo; }
			set { UNDGLimitGrid.BindTo = value; }
		}

		#endregion

		void UNDGLimitGrid_SelectedRowsChangedInMouseDown(object sender, System.EventArgs e)
		{
			((WarehouseEntryForm)TopLevelControl).MessageStatusBarPanel.Text = UNDGLimitGrid.GetSelectedElements<WhsUNDGLimit>().FirstOrDefault()?.UNDGLimitDescription;
		}
	}
}
