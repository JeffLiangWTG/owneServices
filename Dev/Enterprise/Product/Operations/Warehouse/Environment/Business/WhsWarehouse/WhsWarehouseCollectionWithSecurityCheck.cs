using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsWarehouseCollectionWithSecurityCheck : WhsWarehouseCollection, IWhsWarehouseCollectionWithSecurityCheck
	{
		public WhsWarehouseCollectionWithSecurityCheck(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsWarehouseCollectionWithSecurityCheck(BusinessObjectFactory factory, WarehouseCollectionType collectionType)
			: base(factory, collectionType)
		{
		}

		#region AllowedAccessTo

		public static bool AllowedAccessTo(WhsWarehouse warehouse)
		{
			return Env.Security.WhsAllowedWarehouses.IsAllowed
				|| AllowedAccessTo(GlbStaff.CurrentUser, warehouse)
				|| AllowedAccessTo(GlbStaff.CurrentUser.Groups, warehouse);
		}

		static bool AllowedAccessTo(BusinessObjectCollection groups, WhsWarehouse warehouse)
		{
			foreach (GlbGroup group in groups)
			{
				if (AllowedAccessTo(group, warehouse))
				{
					return true;
				}
			}
			return false;
		}

		static bool AllowedAccessTo(IOrgsAndWarehousesAccessProvider provider, WhsWarehouse warehouse)
		{
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			foreach (GlbSecurity checkpoint in provider.SecurityAllowedOrgsAndWarehousesView)
			{
				if (checkpoint.GU_ItemGUID == warehouse.PK)
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

			if (!Env.Security.WhsAllowedWarehouses.IsAllowed)
			{
				List<ZGuid> list = new List<ZGuid>();
				AddWarehousesFromProvider(list, GlbStaff.CurrentUser);
				foreach (GlbGroup group in GlbStaff.CurrentUser.Groups)
				{
					AddWarehousesFromProvider(list, group);
				}

				filter.AddToFilter(WhsWarehouseSchema.PK, list);
			}

			return filter;
		}

		void AddWarehousesFromProvider(List<ZGuid> list, IOrgsAndWarehousesAccessProvider provider)
		{
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
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
			WhsWarehouse whs = (WhsWarehouse)selectedBusinessObject;

			if (!AllowedAccessTo(whs))
			{
				errors.Add(Res.GetString("0b4376ff-2fda-42e7-9946-f7714156b9c4", "You are not authorized to work with this warehouse."));
			}
		}

		#endregion
	}
}
