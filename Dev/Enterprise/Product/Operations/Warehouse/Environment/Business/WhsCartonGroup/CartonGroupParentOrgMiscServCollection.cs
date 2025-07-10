using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class CartonGroupParentOrgMiscServCollection : ActiveBusinessObjectCollection<OrgMiscServ>
	{
		public CartonGroupParentOrgMiscServCollection(WhsCartonGroup group)
			: base(GetFactoryWithNullCheck(group), group, null, OrgMiscServSchema.OM_WCG_CartonGroup)
		{
		}

		#region GetFactoryWithNullCheck

		static BusinessObjectFactory GetFactoryWithNullCheck(WhsCartonGroup group)
		{
			return Argument.NotNull(group, "group").Factory;
		}

		#endregion
	}
}

