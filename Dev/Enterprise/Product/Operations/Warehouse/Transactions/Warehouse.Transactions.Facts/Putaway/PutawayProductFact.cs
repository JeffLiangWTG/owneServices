using Enterprise.MasterFiles.Integration;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.ProductionRules.Core;
using WTG.ProductionRules.Core.Facts;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class PutawayProductFact : ProductFact, IPutawayProductFact
	{
		public PutawayProductFact(
			IOrgSupplierPart part,
			IOrgPartRelation relation,
			string abcCategory,
			string putawayGroupCode,
			string styleCode,
			string classificationCode,
			string colorCode,
			string sizeCode,
			IDangerousGoodsFact firstDG,
			bool hasMultipleDangerousGoods)
			: base(part, relation, styleCode, classificationCode, colorCode, sizeCode)
		{
			PutawayGroupCode = putawayGroupCode ?? string.Empty;
			ABCCategoryCode = abcCategory ?? string.Empty;
			StockUQ = part.OP_StockKeepingUnit;
			UnitPrice = new MoneyFact(relation.OU_UnitPrice, relation.OU_RX_NKUnitPriceCurrency);
			FirstDG = new FactLeftJoin<IDangerousGoodsFact>(firstDG);
			HasMultipleDangerousGoods = hasMultipleDangerousGoods;
		}

		public string PutawayGroupCode { get; }

		public string StockUQ { get; }

		public string ABCCategoryCode { get; }

		public MoneyFact UnitPrice { get; }

		public FactLeftJoin<IDangerousGoodsFact> FirstDG { get; }

		public bool HasMultipleDangerousGoods { get; }
	}
}
