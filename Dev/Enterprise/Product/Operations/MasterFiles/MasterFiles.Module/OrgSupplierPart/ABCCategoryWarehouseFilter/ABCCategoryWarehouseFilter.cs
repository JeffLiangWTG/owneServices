using System;
using System.Xml;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ABCCategoryWarehouseFilter : ModuleFilter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name should not be localized")]
		public ABCCategoryWarehouseFilter()
			: base("ABC Category / Warehouse")
		{
			MultilingualDescription = ResString.GetMultilingualString("5febcd32-1514-4d76-86ab-a4c4fd2a224d", "ABC Category / Warehouse");
			this.SubGroup = new OrgSupplierPartSubGroup();
		}

		public ABCCategoryWarehouseFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
			this.SubGroup = new OrgSupplierPartSubGroup();
		}

		#region Schema

		public static class Schema
		{
			public const string WJ_Category = "WJ_Category";
			public const string WJ_WW_Warehouse = "WJ_WW_Warehouse";
		}

		#endregion

		#region WJ_Category

		[List("ABCAnalysisCategories")]
		[MaxLength(7)]
		public ZString WJ_Category
		{
			get { return wj_Category; }
			set
			{
				if (wj_Category != value)
				{
					SetNonPersistentPropertyValue(WJ_CategoryInfo, ref wj_Category, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateWJ_Category();
					}

					WJ_CategoryInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo WJ_CategoryInfo
		{
			get { return GetZPropertyInfo(Schema.WJ_Category, Res.GetString("168da0a4-4905-478a-8fdb-de88cb7cde2c", "ABC Category")); }
		}

		public ABCAnalysisCategoryCollection ABCAnalysisCategories
		{
			get { return abcAnalysisCategories ?? (abcAnalysisCategories = WarehouseDataRegistry.Instance.ABCAnalysisCategories.Value); }
		}

		ZString wj_Category;
		ABCAnalysisCategoryCollection abcAnalysisCategories;

		#endregion

		#region WJ_WW_Warehouse

		[List("Warehouses")]
		public ZGuid WJ_WW_Warehouse
		{
			get { return wj_WW_Warehouse; }
			set
			{
				if (wj_WW_Warehouse != value)
				{
					SetNonPersistentPropertyValue(WJ_WW_WarehouseInfo, ref wj_WW_Warehouse, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateWJ_WW_Warehouse();
					}

					WJ_WW_WarehouseInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo WJ_WW_WarehouseInfo
		{
			get { return GetZPropertyInfo(Schema.WJ_WW_Warehouse); }
		}

		public BusinessObjectCollection Warehouses
		{
			get { return warehouses ?? (warehouses = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IWhsWarehouseCollection>(), new BusinessObjectFactory())); }
		}

		ZGuid wj_WW_Warehouse;
		BusinessObjectCollection warehouses;

		#endregion

		#region Validation

		public new ABCCategoryWarehouseFilterValidation Validation
		{
			get { return (ABCCategoryWarehouseFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ABCCategoryWarehouseFilterValidation(this);
		}

		#endregion

		#region Query

		internal class OrgSupplierPartSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				query.AddToFilter(filter);
				return query;
			}
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = new ZQuery();

			if (!WJ_Category.IsEmpty)
			{
				var warehouseFilter = WJ_WW_Warehouse.IsEmpty ? "" : " WHERE WJ_WW_Warehouse = @WJ_WW_Warehouse";
				var sql = string.Format(@"
					OP_PK IN
					(
						SELECT WJ_OP_Product
						FROM
						(
							SELECT WJ_OP_Product, WJ_Category,
							ROW_NUMBER() OVER(PARTITION BY WJ_OP_Product, WJ_OH_Client, WJ_WW_Warehouse ORDER BY WJ_AnalysisDateTo DESC) as RowNumber
							FROM dbo.WhsABCCategory{0}
						) as CurrentCategories
						WHERE RowNumber = 1 AND WJ_Category = @WJ_Category
					)", warehouseFilter);

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@WJ_Category", WJ_Category, WhsABCCategorySchema.WJ_Category);
				if (!WJ_WW_Warehouse.IsEmpty)
				{
					sqlParams.Add("@WJ_WW_Warehouse", WJ_WW_Warehouse, WhsABCCategorySchema.WJ_WW_Warehouse);
				}

				query.AddFilterAndZSQLParameterCollection(sql, sqlParams);
			}

			return query;
		}

		protected override bool IsEmptyCore => WJ_Category.IsEmpty && WJ_WW_Warehouse.IsEmpty;

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { WJ_Category, WJ_WW_Warehouse }; }
		}

		#endregion

		#region Implementation

		protected override void ClearCore()
		{
			WJ_Category = "";
			WJ_WW_Warehouse = ZGuid.Empty;
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ABCCategoryWarehouseFilter)filterToCopyFrom;
			WJ_Category = filter.WJ_Category;
			WJ_WW_Warehouse = filter.WJ_WW_Warehouse;
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ABCCategoryWarehouseFilter(category, parentCollection);
		}

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			WJ_Category = RandomString(3);
			WJ_WW_Warehouse = ZGuid.NewZGuid();
		}

#endif
		#endregion

		#endregion

		#region IXmlSerializable Members

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == Schema.WJ_Category)
			{
				WJ_Category = reader.ReadElementString(Schema.WJ_Category);
			}

			if (reader.Name == Schema.WJ_WW_Warehouse)
			{
				ZGuid result;
				if (ZGuid.TryParse(reader.ReadElementString(Schema.WJ_WW_Warehouse), out result))
				{
					WJ_WW_Warehouse = result;
				}
			}
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString(Schema.WJ_Category, WJ_Category);
			writer.WriteElementString(Schema.WJ_WW_Warehouse, WJ_WW_Warehouse.ToString());
		}

		#endregion
	}
}
