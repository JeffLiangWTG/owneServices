using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefCountryFilterControl : ZFilterStripControl
	{
		public RefCountryFilterControl(IBusinessObjectCollection gridCollection, RefCountryFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
