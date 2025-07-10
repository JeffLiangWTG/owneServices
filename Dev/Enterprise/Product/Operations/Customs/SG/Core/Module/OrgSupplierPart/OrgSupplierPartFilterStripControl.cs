using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Module
{
	/// <summary>
	/// Filter control for SGPlaces.
	/// </summary>
	public partial class OrgSupplierPartFilterStripControl : Customs.Module.OrgSupplierPartFilterStripControl
	{
		public OrgSupplierPartFilterStripControl(IBusinessObjectCollection gridCollection, OrgSupplierPartFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
