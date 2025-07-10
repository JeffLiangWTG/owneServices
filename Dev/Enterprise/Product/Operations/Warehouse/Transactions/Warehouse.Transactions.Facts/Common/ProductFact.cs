using System;
using Enterprise.MasterFiles.Integration;
using WTG.ProductionRules.Business.Common;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class ProductFact : IProductFact, IProductStyleFact
	{
		public ProductFact(IOrgSupplierPart part, IOrgPartRelation relation)
			: this(part, relation, string.Empty, string.Empty, string.Empty, string.Empty)
		{
		}

		public ProductFact(IOrgSupplierPart part, IOrgPartRelation relation, string styleCode, string classificationCode, string colorCode, string sizeCode)
		{
			Argument.NotNull(part, nameof(part));
			Argument.NotNull(relation, nameof(relation));

			PK = relation.PK.ToGuid();
			Code = part.OP_PartNum;
			CommodityCode = part.OP_RH_NKCommodityCode;
			CategoryCode = relation.CategoryCode;
			ProductStyleCode = styleCode;
			ProductStyleClassificationCode = classificationCode;
			ProductStyleColorCode = colorCode;
			ProductStyleSizeCode = sizeCode;
		}

		public Guid PK { get; }

		public string Code { get; }

		public string CategoryCode { get; }

		public string CommodityCode { get; }

		public string ProductStyleCode { get; }

		public string ProductStyleColorCode { get; }

		public string ProductStyleSizeCode { get; }

		public string ProductStyleClassificationCode { get; }
	}
}
