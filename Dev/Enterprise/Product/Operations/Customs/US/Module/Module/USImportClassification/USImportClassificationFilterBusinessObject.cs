using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USImportClassificationFilterBusinessObject : Customs.Module.CusClassificationFilterBusinessObject
	{
		public USImportClassificationFilterBusinessObject()
		{
		}

		protected override void AddTariffFilters(ModuleFilterCollection filters)
		{
			fTariffFilter = filters.AddTextFilter("Tariff No", TariffNumQuery);

			fTariffFilter.PropertyInfo.ValueChanged += new EventHandler(TariffFilterPropertyInfo_ValueChanged);
		}

		protected override void TariffFilterPropertyInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!bIsInFormatting)
			{
				bIsInFormatting = true;
				fTariffFilter.Property = TariffFormatter.DisplayFormat(fTariffFilter.Property);
				bIsInFormatting = false;
			}
		}

		protected override Customs.Business.TariffFormatter GetTariffFormatter() => new TariffFormatter();

		ZQuery TariffNumQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();

			if (!value.IsEmpty)
			{
				query.AddToFilter(CusClassificationSchema.CC_TariffNum, comparisonOperator, TariffFormatter.Format(value));
			}

			return query;
		}
	}
}
