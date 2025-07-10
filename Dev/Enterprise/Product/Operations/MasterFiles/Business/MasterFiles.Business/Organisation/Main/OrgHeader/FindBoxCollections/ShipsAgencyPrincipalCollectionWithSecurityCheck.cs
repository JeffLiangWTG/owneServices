using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ShipsAgencyPrincipalCollectionWithSecurityCheck : ShipsAgencyPrincipalCollection
	{
		public ShipsAgencyPrincipalCollectionWithSecurityCheck(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region AllowedAccessTo

		public static bool AllowedAccessTo(OrgHeader principal)
		{
			return Env.Security.AgencyPrincipalAccess.IsAllowed
				|| AllowedAccessTo(GlbStaff.CurrentUser, principal)
				|| AllowedAccessTo(GlbStaff.CurrentUser.Groups, principal);
		}

		static bool AllowedAccessTo(BusinessObjectCollection groups, OrgHeader principal)
		{
			foreach (GlbGroup group in groups)
			{
				if (AllowedAccessTo(group, principal))
				{
					return true;
				}
			}
			return false;
		}

		static bool AllowedAccessTo(IOrgsAndWarehousesAccessProvider provider, OrgHeader principal)
		{
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			foreach (GlbSecurity checkpoint in provider.SecurityAllowedOrgsAndWarehousesView)
			{
				if (checkpoint.GU_ItemGUID == principal.PK)
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

			if (!Env.Security.AgencyPrincipalAccess.IsAllowed)
			{
				List<ZGuid> list = new List<ZGuid>();
				AddPrincipalsFromProvider(list, GlbStaff.CurrentUser);
				foreach (GlbGroup group in GlbStaff.CurrentUser.Groups)
				{
					AddPrincipalsFromProvider(list, group);
				}

				filter.AddToFilter(OrgHeaderSchema.PK, list);
			}

			return filter;
		}

		void AddPrincipalsFromProvider(List<ZGuid> list, IOrgsAndWarehousesAccessProvider provider)
		{
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
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

			if (IsValidPrincipal(org) && !AllowedAccessTo(org))
			{
				errors.Add(Res.GetString("d9fb0b6e-9a91-490f-ae2a-b9b3a79819c7", "You are not authorized to work with this principal."));
			}
		}

		#endregion
	}
}
