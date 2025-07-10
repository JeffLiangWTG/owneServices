using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffCostCentre : AutoGlbStaffCostCentre
	{
		public GlbStaffCostCentre(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
