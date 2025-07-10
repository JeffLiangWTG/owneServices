using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefDomesticCartageZoneFilterControl : ZFilterStripControl
	{
		public RefDomesticCartageZoneFilterControl(IBusinessObjectCollection gridCollection, RefDomesticCartageZoneFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
