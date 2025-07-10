using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemLookups : AutoWorkItemLookups
	{
		public WorkItemLookups(AutoWorkItem parent)
			: base(parent)
		{
		}

		public WorkItemLookups(BusinessObjectFactory factory)
			: base(null)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		protected override BusinessObjectFactory Factory => factory ?? base.Factory;

		#region Work Item Details

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("WorkItemLookups.Locations", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#endregion

		#region Staff

		public GlbStaffCollection Staff
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion
	}
}
