using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class CusRefRateCodeFilterControl : ZFilterStripControl
	{
		public CusRefRateCodeFilterControl()
		{
			InitializeComponent();
		}

		public CusRefRateCodeFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject filter) : base(collection, filter)
		{
			InitializeComponent();
		}
	}
}
