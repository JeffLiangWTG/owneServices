using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbStaffResourceTimeCollection<TGlbStaffResourceTime> : DependentBusinessObjectCollection<TGlbStaffResourceTime, GlbStaff>
		where TGlbStaffResourceTime : GlbStaffResourceTime
	{
		public GlbStaffResourceTimeCollection(GlbStaff staff)
			: base(staff)
		{
		}

		#region Filter

		protected abstract IEnumerable<string> RecordTypes { get; }

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(GlbStaffHolidaySchema.GA_RecordType, RecordTypes);
			return query;
		}

		#endregion
	}
}
