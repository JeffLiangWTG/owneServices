using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class CusRefPreferenceFilterControl : ZFilterStripControl
	{
		public CusRefPreferenceFilterControl()
		{
			InitializeComponent();
		}

		public CusRefPreferenceFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
