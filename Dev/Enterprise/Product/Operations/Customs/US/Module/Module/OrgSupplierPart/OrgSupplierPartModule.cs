namespace Enterprise.Customs.US.Module
{
	public class OrgSupplierPartModule : Customs.Module.OrgSupplierPartModule
	{
		public OrgSupplierPartModule()
		{
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject() => new OrgSupplierPartFilterStripBusinessObject();

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl() => new OrgSupplierPartFilterStripControl(GridCollection, (OrgSupplierPartFilterStripBusinessObject)FilterBusinessObject);
	}
}
