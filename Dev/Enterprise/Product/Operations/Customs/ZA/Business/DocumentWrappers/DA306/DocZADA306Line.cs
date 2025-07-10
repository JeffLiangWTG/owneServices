using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocZADA306Line : DocumentWrapper
	{
		DocZADA306Line(IHVLVItemForDocument item, BusinessObjectFactory factoryToWrap) : base(item, factoryToWrap)
		{
			this.item = item;
			consignment = item.Consignment as IHVLVConsignmentForDocument;
			lines = item.Lines.Cast<IHVLVItemLine>().ToList();
			docShipment = DocForwardingShipment.New((ForwardingShipment)item.Shipment, Factory);
		}

		public static DocZADA306Line New(HVLVItem item, BusinessObjectFactory factoryToWrap)
		{
			return item == null ? null : new DocZADA306Line(item, factoryToWrap);
		}

		#region Fields

		public ZString FlightNumber => docShipment.FlightNo;
		public ZString MasterAirwayBillNumber => docShipment.MasterBillNumber;
		public ZString HouseAirwayBillNumber => consignment.HVC_WaybillNumber;
		public ZString CountryOfOrigin => string.Join(",", lines.Select(l => l.HVS_RN_NKOriginCountryCode).Distinct());
		public ZString Shipper => consignment.ShipperIsOrganisation ? consignment.ShipperAddress.Header.OH_FullName : consignment.HVC_ShipperName;
		public ZString DestinationCityTown => consignment.HVC_ConsigneeCity;
		public ZString ConsigneeFullNames => consignment.ConsigneeIsOrganisation ? consignment.ConsigneeAddress.Header.OH_FullName : consignment.HVC_ConsigneeName;
		public ZString ConsigneeID => GetConsigneeID();
		public ZInt NumberOfPieces => lines.Sum(x => x.HVS_Quantity);
		public ZDecimal WeightOfPackage => GetWeightOfPackage();
		public ZString DescriptionOfPackage => item.HVI_GoodsDescription;
		public ZString ClassificationHSCode => string.Join(",", lines.Where(l => !l.HVS_DestinationTariff.IsEmpty).Select(l => l.HVS_FormattedDestinationTariff));
		public ZDecimal ForeignValue => foreignValue ??= GetForeignValue();
		public ZString Currency => consignment.HVC_RX_NKGoodsValueCurrency;
		public ZDecimal ValueRand => ConvertToLocalCurrency(ForeignValue);
		public ZDecimal CustomsDuty => customsDuty ??= 0.20m * ConvertToLocalCurrency(lines.Sum(l => l.HVS_CustomsValue));
		public ZDecimal ATV => atv ??= GetATV();
		public ZDecimal VAT => 0.15m * ATV;
		public ZDecimal TotalPayable => CustomsDuty + VAT;
		public ZString DescriptionAsPerHSCode => string.Join(",", lines.Where(l => !l.HVS_GoodsDescription.IsEmpty).Select(l => l.HVS_GoodsDescription));

		#endregion

		ZString GetConsigneeID()
		{
			var consigneeCusEntryNumber = consignment.CustomsReferenceNumbers.GetFirstReferenceNumberByType("CID")
										?? consignment.CustomsReferenceNumbers.GetFirstReferenceNumberByType("CPA");
			return consigneeCusEntryNumber?.CE_EntryNum ?? ZString.Empty;
		}

		ZDecimal GetWeightOfPackage()
		{
			ZDecimal result;
			if (consignment.HVC_ItemCount == 1)
			{
				result = consignment.HVC_ActualWeight.IsEmpty ? consignment.HVC_ManifestedWeight : consignment.HVC_ActualWeight;
			}
			else
			{
				result = item.HVI_ActualWeight.IsEmpty ? item.HVI_ManifestedWeight : item.HVI_ActualWeight;
			}

			return result;
		}

		ZDecimal GetForeignValue()
		{
			ZDecimal foreignValue;
			if (consignment.HVC_ItemCount == 1)
			{
				foreignValue = consignment.HVC_GoodsValue;
			}
			else
			{
				foreignValue = lines.Sum(l => l.HVS_IntrinsicValue);
			}
			return foreignValue;
		}

		ZDecimal ConvertToLocalCurrency(ZDecimal foreignValue)
		{
			return CurrencyConverter.ConvertRounded(new Money(foreignValue, new Currency(Currency)), RefCurrency.LoadFromCurrencyCode(Factory, "ZAR")).Amount;
		}

		ZDecimal GetATV()
		{
			var atvInForeignCurrency = lines.Sum(l => l.HVS_CustomsValue * GetLineMultiplier(l.HVS_RN_NKOriginCountryCode));
			return ConvertToLocalCurrency(atvInForeignCurrency);

			ZDecimal GetLineMultiplier(ZString countryCode)
			{
				ZDecimal multiplier = 1;
				if (!countryCodesForATV.Contains(countryCode))
				{
					multiplier = 1.1;
				}

				return multiplier;
			}
		}

		readonly ZString[] countryCodesForATV = new ZString[]
		{
			Core.Constants.CountryCodes.Botswana,
			Core.Constants.CountryCodes.Swaziland,
			Core.Constants.CountryCodes.Lesotho,
			Core.Constants.CountryCodes.Namibia
		};
		readonly IHVLVItemForDocument item;
		readonly IHVLVConsignmentForDocument consignment;
		readonly DocForwardingShipment docShipment;
		readonly List<IHVLVItemLine> lines;
		ZDecimal? foreignValue;
		ZDecimal? customsDuty;
		ZDecimal? atv;

		CurrencyConverter CurrencyConverter => currencyConverter ??= CurrencyConverter.New(Factory, ZDateTime.Today, ZArchitecture.Core.ExchangeRateType.Customs, roundToTargetCurrencyDecimals: true);
		CurrencyConverter currencyConverter;
	}
}
