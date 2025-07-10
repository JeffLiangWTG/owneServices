using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class ServiceLevelFilterControl : ZFilterStripControl
	{
		public ServiceLevelFilterControl(IBusinessObjectCollection gridCollection, ServiceLevelFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
			=> new ServiceLevelFilterStrip();
	}
}
