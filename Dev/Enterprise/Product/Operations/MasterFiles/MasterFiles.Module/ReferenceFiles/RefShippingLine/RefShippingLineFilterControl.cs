using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefShippingLineFilterControl : ZFilterStripControl
	{
		public RefShippingLineFilterControl(IBusinessObjectCollection gridCollection, RefShippingLineFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		public RefShippingLineFilterControl()
		{
			InitializeComponent();
		}
	}
}
