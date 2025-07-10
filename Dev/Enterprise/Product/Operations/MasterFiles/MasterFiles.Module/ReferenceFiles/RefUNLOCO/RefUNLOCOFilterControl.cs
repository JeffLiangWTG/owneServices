using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefUNLOCOFilterControl : ZFilterStripControl
	{
		public RefUNLOCOFilterControl(IBusinessObjectCollection gridCollection, RefUNLOCOFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new RefUNLOCOFilterStrip();
		}
	}
}
