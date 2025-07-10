using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.Module
{
	/// <summary>
	/// Module Controller for SGPlaces.
	/// </summary>
	public class OrgSupplierPartModule : Customs.Module.OrgSupplierPartModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgSupplierPartFilterStripControl(GridCollection, (OrgSupplierPartFilterStripBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgSupplierPartFilterStripBusinessObject();
		}
	}
}
