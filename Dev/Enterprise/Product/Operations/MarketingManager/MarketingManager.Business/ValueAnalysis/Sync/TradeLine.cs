using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradeLine
	{
		public TradeLine(DynamicBusinessObject rawLine, Dictionary<ZString, OrgSalesProduct> products)
		{
			if (rawLine != null)
			{ Init(rawLine); }
			this.products = products;
		}
		protected readonly Dictionary<ZString, OrgSalesProduct> products;

		void Init(DynamicBusinessObject rawLine)
		{
			MainOrg = (ZGuid)rawLine[Schema.MainOrg];
			Buyer = (ZGuid)rawLine[Schema.Buyer];
			Supplier = (ZGuid)rawLine[Schema.Supplier];

			OriginPk = (ZGuid)rawLine[Schema.Origin];
			OriginTableCode = (ZString)rawLine[Schema.OriginTableCode];
			DestinationPk = (ZGuid)rawLine[Schema.Destination];
			DestinationTableCode = (ZString)rawLine[Schema.DestinationTableCode];

			WarehousePk = (ZGuid)rawLine[Schema.Warehouse];
			ProductCode = (ZString)rawLine[Schema.Product];
			TradeService = (ZString)rawLine[Schema.Service];
			TradeMode = (ZString)rawLine[Schema.Mode];
			TradeType = (ZString)rawLine[Schema.TradeLaneType];

			var status = (ZByte)rawLine[Schema.Status];
			StatusCode = ConvertTradeStatusToCode(status);

			PeriodStart = ((ZDateTime)rawLine[Schema.PeriodStart]).Date;
			PeriodLastTrade = (ZDateTime)rawLine[Schema.PeriodLastTrade];

			JobCount = (ZInt)rawLine[Schema.JobCount];
			PalletCount = (ZInt)rawLine[Schema.PalletCount];
			LineCount = (ZInt)rawLine[Schema.LineCount];
			WeightVolume = (ZDecimal)rawLine[Schema.WeightVolume];
			WeightAmount = (ZDecimal)rawLine[Schema.WeightAmount];
			VolumeM3 = (ZDecimal)rawLine[Schema.VolumeM3];
			ChargeableAmount = (ZDecimal)rawLine[Schema.ChargeableAmount];
			ChargeableUnits = (ZString)rawLine[Schema.ChargeableUnits];
			TEU = (ZDecimal)rawLine[Schema.TEU];

			SupplierPartPk = (ZGuid)rawLine[Schema.SupplierPart];
			JobCompanyPk = (ZGuid)rawLine[Schema.JobComapny];

			Currency = (ZString)rawLine[Schema.Currency];

			JobRevenue = (ZDecimal)rawLine[Schema.JobRevenue];
			JobCost = (ZDecimal)rawLine[Schema.JobCost];
			MainOrgRevenue = (ZDecimal)rawLine[Schema.MainOrgRevenue];
			MainOrgCost = (ZDecimal)rawLine[Schema.MainOrgCost];

			RelatedRateEntryPk = (ZGuid)rawLine[Schema.RelatedRateEntryPk];
			ChargeCompanyPk = (ZGuid)rawLine[Schema.ChargeCompany];
		}

		#region Properties

		public ZString ProductCode { get; protected set; }
		public OrgSalesProduct Product
		{
			get
			{
				if (products.TryGetValue(ProductCode, out OrgSalesProduct product))
				{
					return product;
				}
				return null;
			}
		}
		public ZGuid OriginPk { get; protected set; }
		public ZString OriginTableCode { get; protected set; }
		public ZGuid DestinationPk { get; protected set; }
		public ZString DestinationTableCode { get; protected set; }
		public ZGuid MainOrg { get; protected set; }
		public ZGuid Supplier { get; protected set; }
		public ZGuid Buyer { get; protected set; }

		public ZGuid WarehousePk { get; protected set; }
		public ZString TradeService { get; protected set; }
		public ZString StatusCode { get; protected set; }

		public ZDateTime PeriodLastTrade { get; protected set; }
		public ZInt JobCount { get; protected set; }
		public ZInt PalletCount { get; protected set; }
		public ZInt LineCount { get; protected set; }
		public ZDecimal WeightVolume { get; protected set; }
		public ZDecimal WeightAmount { get; protected set; }
		public ZDecimal VolumeM3 { get; protected set; }
		public ZDecimal ChargeableAmount { get; protected set; }
		public ZString ChargeableUnits { get; protected set; }
		public ZDecimal TEU { get; protected set; }

		public ZGuid JobCompanyPk { get; protected set; }
		public ZGuid RelatedRateEntryPk { get; protected set; }

		public ZString TradeMode { get; protected set; }
		public ZString TradeType { get; protected set; }
		public ZGuid SupplierPartPk { get; protected set; }

		public ZDateTime PeriodStart { get; protected set; }

		public ZString Currency { get; protected set; }
		public ZGuid ChargeCompanyPk { get; protected set; }
		public ZDecimal MainOrgRevenue { get; protected set; }
		public ZDecimal MainOrgCost { get; protected set; }
		public ZDecimal JobRevenue { get; protected set; }
		public ZDecimal JobCost { get; protected set; }

		#endregion

		public bool IsValid
		{
			get
			{
				var product = Product;
				if (product == null
					|| (product.ServiceIsMandatory && TradeService.IsEmpty)
					|| (OriginPk.IsEmpty && DestinationPk.IsEmpty && WarehousePk.IsEmpty)
					|| StatusCode.IsEmpty)
				{
					return false;
				}

				return true;
			}
		}

		public bool IsTraded
		{
			get { return StatusCode == OrgTradeDetail.TradeLaneStatus.Shipped; }
		}

		static protected string ConvertTradeStatusToCode(byte status)
		{
			switch (status)
			{
				case 1:
					return OrgTradeDetail.TradeLaneStatus.Quoted;
				case 2:
					return OrgTradeDetail.TradeLaneStatus.Confirmed;
				case 3:
					return OrgTradeDetail.TradeLaneStatus.Shipped;
				default:
					return string.Empty;
			}
		}

		class Schema
		{
			public const string MainOrg = "MainOrg";
			public const string Buyer = "Buyer";
			public const string Supplier = "Supplier";
			public const string Origin = "Origin";
			public const string OriginTableCode = "OriginTableCode";
			public const string Destination = "Destination";
			public const string DestinationTableCode = "DestinationTableCode";
			public const string Warehouse = "Warehouse";
			public const string Status = "Status";
			public const string Product = "Product";
			public const string Service = "Service";
			public const string Mode = "Mode";
			public const string TradeLaneType = "TradeLaneType";
			public const string PeriodStart = "PeriodStart";
			public const string PeriodLastTrade = "PeriodLastTrade";
			public const string JobCount = "JobCount";
			public const string PalletCount = "PalletCount";
			public const string LineCount = "LineCount";
			public const string WeightVolume = "WeightVolume";
			public const string WeightAmount = "WeightAmount";
			public const string VolumeM3 = "VolumeM3";
			public const string ChargeableAmount = "ChargeableAmount";
			public const string ChargeableUnits = "ChargeableUnits";
			public const string TEU = "TEU";
			public const string SupplierPart = "SupplierPart";
			public const string JobComapny = "JobCompany";
			public const string Currency = "Currency";
			public const string JobRevenue = "JobRevenue";
			public const string JobCost = "JobCost";
			public const string MainOrgRevenue = "MainOrgRevenue";
			public const string MainOrgCost = "MainOrgCost";
			public const string ChargeCompany = "ChargeCompany";
			public const string RelatedRateEntryPk = "RelatedJobID";
		}
	}
}
