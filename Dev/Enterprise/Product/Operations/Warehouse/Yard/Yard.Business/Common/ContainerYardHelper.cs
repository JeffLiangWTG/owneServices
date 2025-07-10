using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public static class ContainerYardHelper
	{
		#region GetContainerYard

		public static WhsWarehouse GetContainerYard(BusinessObjectFactory factory, ZGuid addressPK)
		{
			return GetAllContainerYardForAddressPK(factory, addressPK).FirstOrDefault();
		}

		#endregion

		#region GetAllContainerYardForAddressPK

		public static IEnumerable<WhsWarehouse> GetAllContainerYardForAddressPK(BusinessObjectFactory factory, ZGuid addressPK)
		{
			var query = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, addressPK);
			query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.ContainerYard);
			return factory.Load<WhsWarehouse>(query).OrderBy(w => w.WW_WarehouseCode);
		}

		#endregion

		#region GetContainerYardInCurrentBranch

		public static WhsWarehouse GetContainerYardInCurrentBranch(BusinessObjectFactory factory)
		{
			var query = new ZQuery(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch, GlbBranch.CurrentBranch.PK);
			query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.ContainerYard);
			query.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
			return factory.LoadTop1<WhsWarehouse>(query);
		}

		#endregion

		#region IsContainerYardInCurrentBranch

		public static bool IsContainerYardInCurrentBranch(BusinessObjectFactory factory, ZGuid warehousePK)
		{
			return GetContainerYardInCurrentBranch(factory)?.PK == warehousePK;
		}

		#endregion

		#region GetOrgHeaderContact

		public static OrgHeaderContact GetOrgHeaderContact(this JobDocAddress jobDocAddress)
		{
			OrgHeaderContact orgHeaderContact = null;
			if (jobDocAddress != null && !jobDocAddress.E2_AddressOverride && jobDocAddress.Address != null)
			{
				orgHeaderContact = new OrgHeaderContact(jobDocAddress.Organisation, jobDocAddress.Address);
			}

			return orgHeaderContact;
		}

		#endregion

		#region GetNowInCurrentWarehouse

		public static ZDateTimeOffset GetNowInCurrentWarehouse(WhsWarehouse warehouse)
		{
			var result = ZDateTimeOffset.Now;
			if (warehouse != null)
			{
				var code = warehouse.RelatedCompanyBranch.HomePort.Code;
				var utc = ZDateTime.UtcNow.ToDateTime();
				var now = Env.Time.GetUnlocoTimeFromUtc(code, utc);
				var offset = Env.Time.GetUtcOffsetBasedOnUtc(code, utc);
				result = new ZDateTimeOffset(now, offset);
			}

			return result;
		}

		#endregion

		#region PopulateAddOnValue

		public static void PopulateAddOnValue(this BusinessObject parent, string name, string type, ZString value)
		{
			if (parent != null)
			{
				var addOnValue = parent.Factory.New<GenCustomAddOnValue>();
				addOnValue.XV_ParentID = parent.PK;
				addOnValue.XV_ParentTableCode = parent.TablePrefix;
				addOnValue.XV_Name = name;
				addOnValue.XV_Type = type;
				addOnValue.XV_Data = value;
			}
		}

		#endregion

		#region GetAddOnValues

		public static List<GenCustomAddOnValue> GetAddOnValues(this BusinessObject parent)
		{
			return parent.Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, parent.PK)).ToList();
		}

		public static List<GenCustomAddOnValue> GetAddOnValues(this BusinessObject parent, Func<GenCustomAddOnValue, bool> predicate)
		{
			return parent.GetAddOnValues().Where(predicate).ToList();
		}

		#endregion

	}
}
