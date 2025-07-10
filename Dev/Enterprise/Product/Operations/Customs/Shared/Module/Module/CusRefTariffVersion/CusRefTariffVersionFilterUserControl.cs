using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class RefCusTariffVersionFilterUserControl : ZFilterStripControl
	{
		public RefCusTariffVersionFilterUserControl()
		{
			InitializeComponent();
		}

		public RefCusTariffVersionFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
