using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefPackTypeFilterControl : ZFilterStripControl
	{
		public RefPackTypeFilterControl(IBusinessObjectCollection gridCollection, RefPackTypeFilterBusinessObject filterBusinessObject)
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
