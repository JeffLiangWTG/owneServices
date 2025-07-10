using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefCarrierConsortiumFilterControl : ZFilterStripControl
	{
		public RefCarrierConsortiumFilterControl(IBusinessObjectCollection gridCollection, RefCarrierConsortiumFilterBusinessObject filterBusinessObject)
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
