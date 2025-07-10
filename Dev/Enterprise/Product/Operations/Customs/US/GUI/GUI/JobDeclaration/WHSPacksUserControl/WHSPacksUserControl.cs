using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class WHSPacksUserControl : ZUserControl
	{
		public WHSPacksUserControl()
		{
			InitializeComponent();
		}

		void ResetButton_Click(object sender, System.EventArgs e)
		{
			var declaration = this.CurrentDataItem as JobDeclaration;
			if (declaration != null)
			{
				declaration.WHSInvLineFilter = ZGuid.Empty;
				declaration.WHSPackageFilter = ZGuid.Empty;
				declaration.WHSProductFilter = ZString.Empty;
			}
		}
	}
}
