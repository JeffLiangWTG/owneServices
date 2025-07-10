using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module
{
	public class AssignCartonGroupLookups : ZLookups
	{
		public AssignCartonGroupLookups(BusinessObject parent)
			: base(parent)
		{
		}

		#region CartonGroups

		public IWhsCartonGroupCollection CartonGroups
		{
			get { return Factory.GetCachedValue("AssignCartonGroupLookups|CartonGroups", () => ObjectFactory.New<IWhsCartonGroupCollection>(Factory)); }
		}

		#endregion

		#region Organisations

		public OrgHeaderCollection Organisations
		{
			get { return Factory.GetCachedValue("AssignCartonGroupLookups|Organisations", () => new OrgHeaderCollection(Factory)); }
		}

		#endregion
	}
}
