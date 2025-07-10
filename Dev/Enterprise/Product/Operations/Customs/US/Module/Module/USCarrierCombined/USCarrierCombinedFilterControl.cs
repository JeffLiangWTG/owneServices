using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Filter Control for US Carrier
	/// </summary>
	public partial class USCarrierCombinedFilterControl : ZFilterStripControl
	{
		public USCarrierCombinedFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
