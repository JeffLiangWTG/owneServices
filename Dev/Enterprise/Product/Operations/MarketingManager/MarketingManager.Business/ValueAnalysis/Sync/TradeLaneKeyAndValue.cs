using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradeLaneKey : IEquatable<TradeLaneKey>
	{
		public enum TradeType
		{
			Actual,
			Prospect
		}

		public TradeLaneKey(
				TradeType tradeType,
				TradeLine line)
		{
			TradeStatusType = tradeType;
			ProductPk = line.Product.PK;

			OriginPk = line.OriginPk;
			OriginTableCode = line.OriginTableCode;
			DestinationPk = line.DestinationPk;
			DestinationTableCode = line.DestinationTableCode;

			WarehousePk = line.WarehousePk;
			Service = line.TradeService;

			JobCompanyPk = line.JobCompanyPk;

			MainOrg = line.MainOrg;
			Supplier = line.Supplier;
			Buyer = line.Buyer;
		}

		public TradeLaneKey(OrgSales sales)
		{
			TradeStatusType = TradeLaneKey.TradeType.Actual;
			ProductPk = sales.OW_MP_Product;

			OriginPk = sales.OW_OriginID;
			OriginTableCode = sales.OW_OriginTableCode;
			DestinationPk = sales.OW_DestinationID;
			DestinationTableCode = sales.OW_DestinationTableCode;

			WarehousePk = sales.OW_WW;
			Service = sales.OW_Service;

			JobCompanyPk = sales.OW_GC;

			MainOrg = ZGuid.Empty;
			Supplier = sales.OW_OH_Supplier;
			Buyer = sales.OW_OH_Buyer;
		}

		public TradeType TradeStatusType { get; private set; }
		public ZGuid ProductPk { get; private set; }
		public ZGuid OriginPk { get; private set; }
		public ZString OriginTableCode { get; private set; }
		public ZGuid DestinationPk { get; private set; }
		public ZString DestinationTableCode { get; private set; }

		//Prospect Owner
		public ZGuid MainOrg { get; private set; }

		//Buyer-Supplier
		public ZGuid Supplier { get; private set; }
		public ZGuid Buyer { get; private set; }

		//Warehouse
		public ZGuid WarehousePk { get; private set; }
		public ZString Service { get; private set; }

		//Customs
		public ZGuid JobCompanyPk { get; private set; }

		#region System.Object Overrides

		public override bool Equals(object obj)
		{
			return Equals(obj as TradeLaneKey);
		}

		public override int GetHashCode()
		{
			int result =
						TradeStatusType.GetHashCode()
						^ ProductPk.GetHashCode()
						^ Service.GetHashCode()
						^ OriginPk.GetHashCode()
						^ DestinationPk.GetHashCode()
						^ WarehousePk.GetHashCode()
						^ Supplier.GetHashCode()
						^ Buyer.GetHashCode()
						^ JobCompanyPk.GetHashCode();

			if (TradeStatusType == TradeType.Prospect)
			{
				result ^= MainOrg.GetHashCode();
			}

			return result;
		}

		#endregion

		#region IEquatable<TradeStatusKey> Members

		public bool Equals(TradeLaneKey other)
		{
			if (other == null)
			{ return false; }

			bool result =
					TradeStatusType.Equals(other.TradeStatusType)
					&& ProductPk.Equals(other.ProductPk)
					&& Service.Equals(other.Service)
					&& OriginPk.Equals(other.OriginPk)
					&& OriginTableCode.Equals(other.OriginTableCode)
					&& DestinationPk.Equals(other.DestinationPk)
					&& DestinationTableCode.Equals(other.DestinationTableCode)
					&& WarehousePk.Equals(other.WarehousePk)
					&& Supplier.Equals(other.Supplier)
					&& Buyer.Equals(other.Buyer)
					&& JobCompanyPk.Equals(other.JobCompanyPk);

			if (TradeStatusType == TradeType.Prospect)
			{
				result &= MainOrg.Equals(other.MainOrg);
			}

			return result;
		}

		#endregion
	}

	public class TradeLaneValue
	{
		public TradeLaneValue()
		{
			TradeDetails = new Dictionary<TradeDetailKey, TradeDetailValue>(1);
		}
		public IDictionary<TradeDetailKey, TradeDetailValue> TradeDetails { get; private set; }

		public void AddData(ZGuid viewPointOrgPk, TradeLine line)
		{
			var key = new TradeDetailKey(line);
			if (!TradeDetails.TryGetValue(key, out TradeDetailValue tradeDetailValue))
			{
				tradeDetailValue = new TradeDetailValue();
				TradeDetails.Add(key, tradeDetailValue);
			}

			tradeDetailValue.AddData(viewPointOrgPk, line);
		}
	}
}
