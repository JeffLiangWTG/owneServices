using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI
{
	public partial class ProductStyleForm : ZTemplateForm
	{
		public ProductStyleForm() { /* for designer */ }

		public ProductStyleForm(WhsProductStyle productStyle)
			: base(productStyle)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		public override string FormCaption
		{
			get
			{
				var style = Style;
				return Res.GetString("ProductStyleForm|FormCaption", "Product Style {0}", style != null ? style.WST_Code : ZString.Empty);
			}
		}

		protected WhsProductStyle Style
		{
			get { return (WhsProductStyle)BusinessEntity; }
		}

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
