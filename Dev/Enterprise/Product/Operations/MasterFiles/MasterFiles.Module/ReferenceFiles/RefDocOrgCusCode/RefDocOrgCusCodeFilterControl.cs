using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefDocOrgCusCodeFilterControl : ZFilterStripControl
	{
		public RefDocOrgCusCodeFilterControl(IBusinessObjectCollection gridCollection, RefDocOrgCusCodeFilterBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new RefDocOrgCusCodeFilterStrip();
		}
	}
}
