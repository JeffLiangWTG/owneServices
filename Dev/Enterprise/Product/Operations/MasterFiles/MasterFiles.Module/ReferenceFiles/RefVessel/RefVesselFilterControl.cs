using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefVesselFilterControl : ZFilterStripControl
	{
		public RefVesselFilterControl(IBusinessObjectCollection gridCollection, RefVesselFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZFilterStrip();
		}
	}
}
