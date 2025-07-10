using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbStaff)]
	public class GlbStaffCollection : ActiveBusinessObjectCollection<GlbStaff>, Integration.IGlbStaffCollection
	{
		public GlbStaffCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbStaffCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		readonly GlbPerson master;
		public GlbStaffCollection(GlbPerson master, ZQuery filter)
			: base(master.Factory, master, filter, GlbStaffSchema.GS_PER)
		{
			this.master = master;
		}

		protected override void SetDefaultsForNewElementCore(GlbStaff newStaff)
		{
			base.SetDefaultsForNewElementCore(newStaff);

			if (master != null)
			{
				newStaff.SetFromPerson(master);
			}
		}

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(GlbStaffSchema.GS_IsResource, false);

			return query;
		}

		#endregion
	}
}
