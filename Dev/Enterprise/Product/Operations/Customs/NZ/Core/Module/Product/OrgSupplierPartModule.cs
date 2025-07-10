
namespace Enterprise.Customs.NZ.Module
{
	public class OrgSupplierPartModule : Customs.Module.OrgSupplierPartModule
	{
		public OrgSupplierPartModule()
		{
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgSupplierPartFilterStripBusinessObject();
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new OrgSupplierPartFilterStripControl(GridCollection, (OrgSupplierPartFilterStripBusinessObject)FilterBusinessObject);
		}
	}
}
