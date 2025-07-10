using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Filter control for USCTariff.
	/// </summary>
	public partial class USCTariffFilterControl : ZFilterStripControl
	{
		public USCTariffFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
