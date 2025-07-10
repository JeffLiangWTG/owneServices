using System;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;
using WTG.ProductionRules.Core.Facts;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class CycleCountProductFact : ICycleCountProductFact
	{
		public CycleCountProductFact(Guid pk, string code, string categoryCode, string commodityCode, string abcCategoryCode, decimal unitPrice, string unitPriceCurrency)
		{
			Argument.NotNullOrWhitespace(code, nameof(code));
			Argument.NotNull(categoryCode, nameof(categoryCode));
			Argument.NotNull(commodityCode, nameof(commodityCode));
			Argument.NotNull(abcCategoryCode, nameof(abcCategoryCode));
			Argument.NotNull(unitPriceCurrency, nameof(unitPriceCurrency));

			PK = pk;
			Code = code;
			CategoryCode = categoryCode;
			CommodityCode = commodityCode;
			ABCCategoryCode = abcCategoryCode;
			UnitPrice = new MoneyFact(unitPrice, unitPriceCurrency);
		}

		public string ABCCategoryCode { get; }

		public MoneyFact UnitPrice { get; }

		public Guid PK { get; }

		public string Code { get; }

		public string CategoryCode { get; }

		public string CommodityCode { get; }
	}
}
