using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class CusClassificationFilterBusinessObject : FilterStripBusinessObject
	{
		public CusClassificationFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddTariffFilters(filters);
			AddCustomFilters(filters);
			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Lookup Code", CusClassificationSchema.CC_LookupCode).MultilingualDescription = ResString.GetMultilingualString("Customs|CusClassificationFilter|LookupCode", "Lookup Code");
			filters.AddTextFilter("Description", CusClassificationSchema.CC_Description).MultilingualDescription = ResString.GetMultilingualString("Customs|CusClassificationFilter|Description", "Description");
		}

		#endregion

		#region Tariff

		// override this method in descendants to replace text filter with custom tariff filters
		protected virtual void AddTariffFilters(ModuleFilterCollection filters)
		{
			fTariffFilter = filters.AddTextFilter("Tariff No", CusClassificationSchema.CC_TariffNum);
			fTariffFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusClassificationFilter|TariffNo", "Tariff No");
			fTariffFilter.PropertyInfo.ValueChanged += new EventHandler(TariffFilterPropertyInfo_ValueChanged);
		}

		protected virtual void TariffFilterPropertyInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!bIsInFormatting)
			{
				bIsInFormatting = true;
				fTariffFilter.Property = TariffFormatter.Format(fTariffFilter.Property);
				bIsInFormatting = false;
			}
		}

		protected ModuleTextFilter fTariffFilter;
		protected bool bIsInFormatting;

		#endregion

		#region Custom

		// override this method in descendants to add more filters
		protected virtual void AddCustomFilters(ModuleFilterCollection filters)
		{
		}

		#endregion

		#endregion

		#region Tariff Formatter

		protected TariffFormatter TariffFormatter
		{
			get
			{
				if (fTariffFormatter == null)
				{
					fTariffFormatter = GetTariffFormatter();
				}

				return fTariffFormatter;
			}
		}

		TariffFormatter fTariffFormatter;

		protected virtual TariffFormatter GetTariffFormatter()
		{
			return new TariffFormatter();
		}

		#endregion

		#region Query Helpers

		protected void AddInfoQuery(ZQuery query, ZString value, string columnName)
		{
			query.AddToFilter(JoinCondition.And, CusClassificationSchema.CC_AddInfo, SQLComparisonOperator.Contains, columnName);
			query.AddToFilter(JoinCondition.And, CusClassificationSchema.CC_AddInfo, SQLComparisonOperator.Contains, value);
		}

		protected ModuleTextFilter GetAddInfoTextFilter(ZString description, string addInfoPropertyName)
		{
			return new AddInfoModuleTextFilter(description, CusClassificationSchema.CC_AddInfo, addInfoPropertyName);
		}

		protected ModuleNkFilter GetAddInfoNkExactFilter(ZString description, ModuleIdentifier id, IBusinessObjectCollection list, string addInfoPropertyName)
		{
			return new AddInfoModuleNkFilter(description, id, list, CusClassificationSchema.CC_AddInfo, addInfoPropertyName);
		}

		#endregion
	}
}
