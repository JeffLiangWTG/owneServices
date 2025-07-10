using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefTimeZoneSetFilterControl : ZFilterStripControl
	{
		public RefTimeZoneSetFilterControl(IBusinessObjectCollection gridCollection, RefTimeZoneSetFilterBusinessObject filterBusinessObject)
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
