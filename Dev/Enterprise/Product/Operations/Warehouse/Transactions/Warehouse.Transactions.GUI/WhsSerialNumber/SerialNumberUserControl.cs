using System.ComponentModel;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class SerialNumberUserControl : ZUserControl, IBindTo
	{
		public SerialNumberUserControl()
		{
			InitializeComponent();
		}

		#region Binding

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue("SerialNumbers")]
		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindToList")]
		public string BindTo
		{
			get { return SerialNumberGrid.BindTo; }
			set { SerialNumberGrid.BindTo = value; }
		}

		#endregion
	}
}
