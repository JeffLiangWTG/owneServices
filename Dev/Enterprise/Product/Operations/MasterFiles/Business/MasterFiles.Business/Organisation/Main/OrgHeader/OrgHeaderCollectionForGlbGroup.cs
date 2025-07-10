using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderCollectionForGlbGroup : BusinessObjectCollection<OrgHeader>
	{
		public OrgHeaderCollectionForGlbGroup(GlbGroup masterGroup) : base(masterGroup.Factory)
		{
			MasterPK = masterGroup.PK;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var org = bizOAdded as OrgHeader;

			if (org?.MiscServ != null && org.MiscServ.OM_GG_OrgSecurityGroup != MasterPK)
			{
				org.MiscServ.OM_GG_OrgSecurityGroup = MasterPK;
			}

			if (org != null)
			{
				org.ValidationSuspendedForOrgHeader = true;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			var org = bizO as OrgHeader;

			if (org?.MiscServ != null && org.MiscServ.OM_GG_OrgSecurityGroup == MasterPK)
			{
				var originalValue = (ZGuid)org.MiscServ.OM_GG_OrgSecurityGroupInfo.OriginalValue;

				if (originalValue == MasterPK)
				{
					org.MiscServ.OM_GG_OrgSecurityGroup = ZGuid.Empty;
				}
				else
				{
					org.MiscServ.OM_GG_OrgSecurityGroup = originalValue;
				}
			}
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			subQuery.AddToFilter(OrgMiscServSchema.OM_GG_OrgSecurityGroup, MasterPK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return false;
			}
		}

		protected override bool ElementCanBeAdded(BusinessObject bizO)
		{
			var group = (bizO as OrgHeader)?.MiscServ?.OrgSecurityGroup;

			if (group != null && group.PK != MasterPK)
			{
				bizO.AddRowError(Res.GetString("05777641-51fb-4791-b09b-abf7fd72ed2f", "This record has already been added to the Group ({0}).", group.GG_Code));
			}

			return base.ElementCanBeAdded(bizO);
		}

		readonly ZGuid MasterPK;
	}

	public class OrgHeaderCollectionForGlbGroupLookup : OrgHeaderCollection, IFilterModuleExtraNotificationProvider
	{
		public OrgHeaderCollectionForGlbGroupLookup(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgHeaderCollectionForGlbGroupLookup(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			var group = (businessObject as OrgHeader)?.MiscServ?.OrgSecurityGroup;

			if (group != null)
			{
				return new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("05777641-51fb-4791-b09b-abf7fd72ed2f", "This record has already been added to the Group ({0}).", group.GG_Code));
			}

			return null;
		}
	}
}
