using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseWaveCreation;
using WTG.ProductionRules.Core;
using WTG.ProductionRules.Core.Facts;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class OrderFact : InputFactWithUserDefinedProperties, IOrderFact
	{
		public OrderFact(
			IWhsOrder whsOrder,
			IOrganisationFact client,
			IOrganisationFact consignee,
			IOrganisationFact transportCompany,
			IOrganisationFact carrierBookingAgent,
			string consigneeDeliveryRoute,
			CurrencyConverter currencyConverter,
			IDocAddressFact consigneeAddress,
			IDocAddressFact distributionCentreAddress)
		{
			Argument.NotNull(whsOrder, nameof(whsOrder));
			Argument.NotNull(client, nameof(client));
			Argument.NotNull(currencyConverter, nameof(currencyConverter));
			CurrencyConverter = currencyConverter;

			Client = new FactJoin<IOrganisationFact>(client);
			Consignee = new FactLeftJoin<IOrganisationFact>(consignee);

			TransportCompany = new FactLeftJoin<IOrganisationFact>(transportCompany);
			CarrierBookingAgent = new FactLeftJoin<IOrganisationFact>(carrierBookingAgent);
			ConsigneeAddress = new FactJoin<IDocAddressFact>(consigneeAddress);
			DistributionCentreAddress = new FactLeftJoin<IDocAddressFact>(distributionCentreAddress);

			PK = whsOrder.PK.ToGuid();
			var orderPickPriority = whsOrder.WD_PickPriority;
			PickPriority = orderPickPriority == 0 ? int.MaxValue : orderPickPriority;
			DocketID = whsOrder.WD_DocketID;
			OrderNumber = whsOrder.WD_ExternalReference;
			OrderType = whsOrder.WD_DocketSubType;
			SalesChannelCode = whsOrder.SalesChannelCode;
			CustomerReference = whsOrder.WD_CustomerReference;
			TransportZone = whsOrder.TransportZoneName;
			ServiceLevel = whsOrder.WD_RS_NKServiceLevel;
			CarrierServiceLevel = whsOrder.WD_PL_NKCarrierServiceLevel;
			ConsigneeDeliveryRoute = consigneeDeliveryRoute;
			PackingRequired = whsOrder.WD_PackingAfterPickingRequired;
			AuthorisedToLeave = whsOrder.WD_IsAuthorisedToLeave;
			QualityAuditRequired = whsOrder.WD_QualityAuditRequired;
			HasDangerousGoods = whsOrder.HasDangerousGoods;
			RequiredDate = whsOrder.WD_RequiredDate.Date.ToDateTime();
			CreatedDate = whsOrder.CreateDate.ToDateTime();
			TotalLineUnits = whsOrder.WD_TotalUnitsFromLines;
			TotalOrderLines = whsOrder.TotalOrderLines;
			TotalWeight = new MeasureFact(whsOrder.WD_TotalWeight, whsOrder.WD_TotalWeightUnit);
			TotalVolume = new MeasureFact(whsOrder.WD_TotalCubic, whsOrder.WD_TotalCubicUnit);

			OrderValue = new Money(whsOrder.WD_TotalOrderValue, RefCurrency.LoadFromCurrencyCode(currencyConverter.Factory, whsOrder.WD_RX_NKTotalOrderCurrency));
		}

		CurrencyConverter CurrencyConverter { get; }

		Money OrderValue { get; }

		public Guid PK { get; }

		public Guid WaveFactPK { get; set; }

		public FactJoin<IOrganisationFact> Client { get; }

		public FactLeftJoin<IOrganisationFact> Consignee { get; }

		public FactJoin<IDocAddressFact> ConsigneeAddress { get; }

		public FactLeftJoin<IOrganisationFact> TransportCompany { get; }

		public FactLeftJoin<IOrganisationFact> CarrierBookingAgent { get; }

		public FactLeftJoin<IDocAddressFact> DistributionCentreAddress { get; }

		public int PickPriority { get; }

		public string DocketID { get; }

		public string OrderNumber { get; }

		public string OrderType { get; }

		public string SalesChannelCode { get; }

		public string CustomerReference { get; }

		public string TransportZone { get; }

		public string ServiceLevel { get; }

		public string CarrierServiceLevel { get; }

		public string ConsigneeDeliveryRoute { get; }

		public bool PackingRequired { get; }

		public bool AuthorisedToLeave { get; }

		public bool QualityAuditRequired { get; }

		public bool HasDangerousGoods { get; }

		public DateTime RequiredDate { get; }

		public DateTime CreatedDate { get; }

		public decimal TotalLineUnits { get; }

		public int TotalOrderLines { get; }

		public MeasureFact TotalWeight { get; }

		public MeasureFact TotalVolume { get; }

		public decimal GetTotalOrderValue(string currencyCode)
		{
			var result = ZDecimal.Zero;
			if (OrderValue.IsValid)
			{
				if (OrderValue.Currency.Code == currencyCode)
				{
					result = OrderValue.Amount;
				}
				else
				{
					CurrencyConverter.DateForRate = ZDateTime.Now;
					var currency = RefCurrency.LoadFromCurrencyCode(CurrencyConverter.Factory, currencyCode);
					var valueByCurrency = CurrencyConverter.ConvertExact(OrderValue, currency);
					if (valueByCurrency.IsValid)
					{
						result = valueByCurrency.Amount;
					}
				}
			}
			return result;
		}
	}
}
