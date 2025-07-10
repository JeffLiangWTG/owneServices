using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ExchangeRateCurrencyConfigurationCollection : DependentBusinessObjectCollection<ExchangeRateCurrencyConfiguration, AccExchangeRateConfiguration>
	{
		public ExchangeRateCurrencyConfigurationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ExchangeRateCurrencyConfigurationCollection(AccExchangeRateConfiguration parent) : base(parent)
		{
			Parent = parent;
		}

		protected override string FkColumnName => AccJobConfigPivotSchema.JCT_JCF_JobConfig.Name;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((ExchangeRateCurrencyConfiguration)child).JCT_JCF_JobConfig = Parent.PK;
			((ExchangeRateCurrencyConfiguration)child).JCT_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(AccJobConfigPivotSchema.JCT_ParentId, DBNull.Value);
			return query;
		}

		public ExchangeRateCurrencyConfiguration GetRecord(string currencyCode, ZDate date)
		{
			var currencyCollection = Find(c => c.JCT_Code.EqualsIgnoringCase(currencyCode));

			return currencyCollection.SingleOrDefault(config => config.HasDateRange && config.JCT_StartDate <= date && config.JCT_ExpiryDate >= date)
				?? currencyCollection.SingleOrDefault(config => !config.HasDateRange);
		}

		readonly AccExchangeRateConfiguration Parent;
	}
}
