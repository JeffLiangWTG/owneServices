using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageCustomFieldsControl : ZUserControl
	{
		public CartageCustomFieldsControl()
		{
			InitializeComponent();
			CaptionRenderingEnabled = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
