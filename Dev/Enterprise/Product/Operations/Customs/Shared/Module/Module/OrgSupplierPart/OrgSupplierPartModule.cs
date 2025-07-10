using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class OrgSupplierPartModule : MasterFiles.Module.OrgSupplierPartModule
	{
		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgSupplierPartFilterStripBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.SupplierPart);
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new OrgSupplierPartFilterStripControl(GridCollection, (OrgSupplierPartFilterStripBusinessObject)FilterBusinessObject);
		}
	}
}
