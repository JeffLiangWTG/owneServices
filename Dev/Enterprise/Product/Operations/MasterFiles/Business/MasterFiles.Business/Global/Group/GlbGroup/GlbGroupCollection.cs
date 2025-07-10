using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbGroup)]
	public class GlbGroupCollection : BusinessObjectCollection<GlbGroup>, Integration.IGlbGroupCollection
	{
		public GlbGroupCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlbGroupCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public GlbGroupCollection(BusinessObjectFactory factory, ZQuery filter, ZQuery aRelationshipFilter) : base(factory, filter)
		{
			this.ARelationshipFilter = aRelationshipFilter;
		}

		public GlbGroupCollection(BusinessObjectFactory factory, bool staffGroupOnly) : base(factory, staffGroupOnly ? new ZQuery(GlbGroupSchema.GG_Type, GlbGroupTypeList.Codes.Staff) : new ZQuery())
		{
		}

		readonly ZQuery ARelationshipFilter;

		protected override ZQuery CreateRelationshipFilter()
		{
			return ARelationshipFilter ?? base.CreateRelationshipFilter();
		}

		public bool AddScimFilter { get; set; }

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			if (query.IsEmpty)
			{
				query.AddToFilter(GlbGroupSchema.PK, SQLComparisonOperator.NotEqual, GlbGroup.DbDeveloperGroupPK);
				query.AddToFilter(JoinCondition.And, GlbGroupSchema.PK, SQLComparisonOperator.NotEqual, GlbGroup.DbReaderGroupPK);
				query.AddToFilter(JoinCondition.And, GlbGroupSchema.PK, SQLComparisonOperator.NotEqual, GlbGroup.BackupOperatorGroupPK);
			}

			if (AddScimFilter)
			{
				AddScimSubQuery(query);
			}

			return query;
		}

		void AddScimSubQuery(ZQuery query)
		{
			if (SystemDataRegistry.Instance.EnableScimService.Value)
			{
				if (!SystemDataRegistry.Instance.ScimAllowLocalEditing.Value)
				{
					query.AddToFilter(JoinCondition.And, GlbGroupSchema.GG_ExternalId, SQLComparisonOperator.IsBlank, ZString.Empty);
				}

				var collection = SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.Value;
				var groupDescriptions = collection.Cast<StaffColumnToGroupDescriptionScimMapping>().Select(m => m.GroupDescriptionMapping);
				query.AddToFilter(GlbGroupSchema.GG_Desc, SQLComparisonOperator.NotEqual, groupDescriptions);
			}
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX notifications, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(notifications, selectedBusinessObject);

			var group = selectedBusinessObject as GlbGroup;
			if (group == null)
			{
				return;
			}

			if (group.IsControlledByScim)
			{
				notifications.Add(Res.GetString("4E53AEA7-ED6B-462D-9CA8-F6A347442937", "Cannot select an externally controlled group."));
			}

			if (group.IsScimMappedGroup)
			{
				notifications.Add(Res.GetString("0174DD64-883B-4445-B134-29585DFE7D43", "Cannot select an SCIM-mapped group. Set the checkbox directly instead."));
			}
		}
	}

	public class GlbGroupCollectionWithOSMGDescription : GlbGroupCollection
	{
		public GlbGroupCollectionWithOSMGDescription(BusinessObjectFactory factory) : base(factory)
		{ }

		public GlbGroupCollectionWithOSMGDescription(BusinessObjectFactory factory, ZQuery filter, ZQuery aRelationshipFilter) : base(factory, filter, aRelationshipFilter)
		{ }

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new FindBoxListProviderWithOSMGDescription(this); }
		}

		class FindBoxListProviderWithOSMGDescription : FindBoxListProvider
		{
			public FindBoxListProviderWithOSMGDescription(IBusinessObjectCollection coll) : base(coll)
			{ }

			public override string DescriptionFromPrimaryKey(ZGuid pk)
			{
				return (pk.IsEmpty) ? Res.GetString("065bbd23-f722-4607-b62c-7fae58f1112a", "Unassigned") : base.DescriptionFromPrimaryKey(pk);
			}
		}
	}
}
