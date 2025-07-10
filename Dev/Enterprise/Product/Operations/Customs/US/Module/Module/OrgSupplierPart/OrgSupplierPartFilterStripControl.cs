using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Module
{
	public partial class OrgSupplierPartFilterStripControl : Customs.Module.OrgSupplierPartFilterStripControl
	{
		public OrgSupplierPartFilterStripControl(IBusinessObjectCollection gridCollection, OrgSupplierPartFilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZArchitecture.GUI.ZFilterStrip NewZFilterStrip() => new OrgSupplierPartFilterStrip();
	}
}
