using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgCusCodeFilterControl : ZFilterStripControl
	{
		public OrgCusCodeFilterControl(IBusinessObjectCollection gridCollection, OrgCusCodeFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		void FilteredGrid_Navigate(object sender, System.Windows.Forms.NavigateEventArgs ne)
		{
		}
	}
}
