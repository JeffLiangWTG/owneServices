using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCollectionNoteCollection : BusinessObjectCollection<OrgCollectionNote>, IBusiness
	{
		public OrgCollectionNoteCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgCollectionNoteCollection(OrgHeader header)
			: base(header.Factory)
		{
			ParentOrg = header;
		}

		public OrgCollectionNoteCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		#region Related Parent OrgHeader

		public OrgHeader ParentOrg;

		internal ZGuid ParentOrgOB_PK
		{
			get
			{
				if (ParentOrg != null)
				{
					return ParentOrg.CompanyData.PK;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
		}

		#endregion

		#region Overrides

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = new ZQuery();
			if (ParentOrg != null)
			{
				ZQuery additionalFilter = new ZQuery(OrgCollectionNoteSchema.PN_OB, ParentOrgOB_PK);
				filter.AddToFilter(additionalFilter, JoinCondition.And);
			}
			else
			{
				filter.AddToFilter(OrgCollectionNoteSchema.PK, Guid.Empty);
			}
			return filter;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgCollectionNote)child).PN_OB = ParentOrgOB_PK;
			((OrgCollectionNote)child).Header = ParentOrg;
		}

		#endregion
	}
}
