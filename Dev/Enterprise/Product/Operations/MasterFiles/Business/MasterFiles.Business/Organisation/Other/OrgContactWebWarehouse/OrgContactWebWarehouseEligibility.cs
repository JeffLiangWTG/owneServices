using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactWebWarehouseEligibility : NonPersistentBusinessObject
	{
		public OrgContactWebWarehouseEligibility(BusinessObjectFactory factory, IWhsWarehouse warehouse, OrgContact contact, GenPivot pivot)
			: base(factory)
		{
			this.warehouse = Argument.NotNull(warehouse, "warehouse");
			Contact = Argument.NotNull(contact, "contact");
			prohibitedWarehousePivot = pivot;
			isGranted = pivot == null;
		}

		readonly IWhsWarehouse warehouse;
		readonly OrgContact Contact;

		#region Schema

		static class Schema
		{
			public const string WarehouseName = "WarehouseName";
			public const string IsGranted = "IsGranted";
		}

		#endregion

		#region WarehouseName

		public ZString WarehouseName
		{
			get
			{
				var result = warehouse.WW_WarehouseCode;
				if (!warehouse.WW_WarehouseName.IsEmpty)
				{
					result += " (" + warehouse.WW_WarehouseName + ")";
				}
				return result;
			}
		}

		public ZPropertyInfo WarehouseNameInfo
		{
			get { return GetZPropertyInfo(Schema.WarehouseName); }
		}

		#endregion

		#region WarehousePK

		public ZGuid WarehousePK
		{
			get { return warehouse.PK; }
		}

		#endregion

		#region IsGranted

		public ZBool IsGranted
		{
			get { return isGranted; }
			set
			{
				isGranted = value;
				if (value)
				{
					GrantAccess();
				}
				else
				{
					DenyAccess();
				}
				Contact.HasChanges = true;
				IsGrantedInfo.RefreshBinding();
			}
		}

		ZBool isGranted;

		public ZPropertyInfo IsGrantedInfo
		{
			get { return GetZPropertyInfo(Schema.IsGranted); }
		}

		#endregion

		#region Change access

		void GrantAccess()
		{
			if (prohibitedWarehousePivot != null)
			{
				prohibitedWarehousePivot.Delete(); // we have to delete it from DB
				prohibitedWarehousePivot = null; // we set it to null so DenyAccess method will create a new pivot
			}
		}

		void DenyAccess()
		{
			if (prohibitedWarehousePivot == null)
			{
				prohibitedWarehousePivot = Factory.New<GenPivot>();
				prohibitedWarehousePivot.XX_Relation1ID = Contact.PK;
				prohibitedWarehousePivot.XX_Relation2ID = WarehousePK;
				prohibitedWarehousePivot.XX_Relation1TableCode = OrgContactSchema.Constants.Prefix;
				prohibitedWarehousePivot.XX_Relation2TableCode = WhsWarehouseSchema.Constants.Prefix;
				prohibitedWarehousePivot.XX_RelationType = Constants.GenPivotTypes.OrgContactDeniedWarehouse;
			}
		}

		GenPivot prohibitedWarehousePivot;

		#endregion
	}
}
