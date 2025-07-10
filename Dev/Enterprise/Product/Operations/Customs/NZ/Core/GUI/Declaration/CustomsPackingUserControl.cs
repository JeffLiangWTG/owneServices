
#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsPackingUserControl : Customs.GUI.BaseCustomsPackingUserControl
	{
		private readonly System.ComponentModel.Container components;

		public CustomsPackingUserControl()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}


