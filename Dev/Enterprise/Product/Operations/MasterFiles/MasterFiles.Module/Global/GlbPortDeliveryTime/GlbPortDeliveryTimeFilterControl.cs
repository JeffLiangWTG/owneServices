using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Filter control for GlbPortDeliveryTime.
	/// </summary>
	public partial class GlbPortDeliveryTimeFilterControl : ZFilterStripControl
	{
		public GlbPortDeliveryTimeFilterControl(IBusinessObjectCollection gridCollection, GlbPortDeliveryTimeFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
