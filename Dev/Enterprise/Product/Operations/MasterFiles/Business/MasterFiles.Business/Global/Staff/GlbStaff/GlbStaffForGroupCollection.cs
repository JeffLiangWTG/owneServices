using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffForGroupCollection : GlbStaffCollection
	{
		public GlbStaffForGroupCollection(BusinessObjectFactory factory) : base(factory)
		{
			this.GroupPK = ZGuid.Empty;
		}

		public GlbStaffForGroupCollection(BusinessObjectFactory factory, ZGuid groupPK) : base(factory)
		{
			this.GroupPK = groupPK;
		}

		#region Filter

		public ZGuid GroupPK
		{
			get { return fGroupPK; }
			set { fGroupPK = value; }
		}

		ZGuid fGroupPK;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();

			if (!GroupPK.IsEmpty)
			{
				ZDBOnlyQuery dbQuery = new ZDBOnlyQuery(typeof(GlbStaff));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), Enterprise.ZArchitecture.Schema.GlbGroupLinkSchema.GK_GS);
				subQuery.AddToFilter(Enterprise.ZArchitecture.Schema.GlbGroupLinkSchema.GK_GG, GroupPK);
				dbQuery.AddSubQuery(subQuery, JoinCondition.And);
				query.AddToFilter(dbQuery);
			}

			return query;
		}

		#endregion

		protected override object[] GetCollectionState()
		{
			return new object[] { GroupPK };
		}
	}
}
