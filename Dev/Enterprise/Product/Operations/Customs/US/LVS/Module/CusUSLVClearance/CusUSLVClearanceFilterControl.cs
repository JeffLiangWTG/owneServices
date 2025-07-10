using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.LVS.Module
{
	public partial class CusUSLVClearanceFilterControl : ZFilterStripControl
	{
		public CusUSLVClearanceFilterControl(IBusinessObjectCollection gridCollection, CusUSLVClearanceFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
