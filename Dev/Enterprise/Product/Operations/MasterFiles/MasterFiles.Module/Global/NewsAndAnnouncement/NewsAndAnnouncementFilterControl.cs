using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class NewsAndAnnouncementFilterControl : ZFilterStripControl
	{
		public NewsAndAnnouncementFilterControl()
		{
			InitializeComponent();
		}

		public NewsAndAnnouncementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
