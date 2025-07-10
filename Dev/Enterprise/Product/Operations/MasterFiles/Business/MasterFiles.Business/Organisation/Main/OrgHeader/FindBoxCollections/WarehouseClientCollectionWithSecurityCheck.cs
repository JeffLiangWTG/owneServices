using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class WarehouseClientCollectionWithSecurityCheck : WarehouseClientCollection
	{
		public WarehouseClientCollectionWithSecurityCheck(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region AllowedAccessTo

		public static bool AllowedAccessTo(OrgHeader client)
		{
			return Env.Security.WhsAllowedClients.IsAllowed
				|| AllowedAccessTo(GlbStaff.CurrentUser, client)
				|| AllowedAccessTo(GlbStaff.CurrentUser.Groups, client);
		}

		static bool AllowedAccessTo(BusinessObjectCollection groups, OrgHeader client)
		{
			foreach (GlbGroup group in groups)
			{
				if (AllowedAccessTo(group, client))
				{
					return true;
				}
			}
			return false;
		}

		static bool AllowedAccessTo(IOrgsAndWarehousesAccessProvider provider, OrgHeader client)
		{
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			foreach (GlbSecurity checkpoint in provider.SecurityAllowedOrgsAndWarehousesView)
			{
				if (checkpoint.GU_ItemGUID == client.PK)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Implementation

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery filter = base.CreateAdditionalFilter();

			if (!Env.Security.WhsAllowedClients.IsAllowed)
			{
				List<ZGuid> list = new List<ZGuid>();
				AddClientsFromProvider(list, GlbStaff.CurrentUser);
				foreach (GlbGroup group in GlbStaff.CurrentUser.Groups)
				{
					AddClientsFromProvider(list, group);
				}

				filter.AddToFilter(OrgHeaderSchema.PK, list);
			}

			return filter;
		}

		void AddClientsFromProvider(List<ZGuid> list, IOrgsAndWarehousesAccessProvider provider)
		{
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			foreach (GlbSecurity checkpoint in provider.SecurityAllowedOrgsAndWarehousesView)
			{
				if (!list.Contains(checkpoint.GU_ItemGUID))
				{
					list.Add(checkpoint.GU_ItemGUID);
				}
			}
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			OrgHeader org = (OrgHeader)selectedBusinessObject;

			if (IsValidClient(org) && !AllowedAccessTo(org))
			{
				errors.Add(Res.GetString("539b4bf8-654c-4b4b-bc93-20593e6bc0ba", "You are not authorized to work with this client."));
			}
		}

		#endregion
	}
}
