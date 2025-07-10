using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Filter Control for US FIRMS
	/// </summary>
	public partial class USCFIRMSFilterControl : ZFilterStripControl
	{
		public USCFIRMSFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
