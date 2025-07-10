using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class TWGlbStaffCollection : GlbStaffCollection
	{
		public TWGlbStaffCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Filter
		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(GlbStaffSchema.GS_IsResource, false);
			return query;
		}
		#endregion

		protected override bool AllowNew => false;
	}
}
