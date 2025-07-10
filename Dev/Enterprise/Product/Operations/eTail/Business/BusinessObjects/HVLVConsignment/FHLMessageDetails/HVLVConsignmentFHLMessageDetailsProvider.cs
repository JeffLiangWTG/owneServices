using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentFHLMessageDetailsProvider : IFHLMessageDetailsProvider
	{
		public HVLVConsignmentFHLMessageDetailsProvider(HVLVConsignment consignment)
		{
			this.consignment = consignment;
		}

		readonly HVLVConsignment consignment;

		#region IFHLMessageDetailsProvider Members

		public ZString HouseBill => consignment.HVC_WaybillNumber;

		public ZInt ShippingLoadAndCount => consignment.HVC_ItemCount;

		public ZString ManifestDescriptionOfGoods => consignment.HVC_GoodsDescription;

		public ZString DetailedGoodsDescription
		{
			get
			{
				var volume = consignment.HVC_ActualVolume.IsDefault ? consignment.HVC_ManifestedVolume : consignment.HVC_ActualVolume;
				var goodsDescriptionText = consignment.HVC_GoodsDescription.IsEmpty ? ZString.Empty : ZString.Format("{0} ", consignment.HVC_GoodsDescription);
				return ZString.Format((NoResString)"{0}{1} VOL {2} {3}", goodsDescriptionText, consignment.HVC_ItemCount, volume, consignment.HVC_VolumeUQ); // For US FHL Messaging only
			}
		}

		public ZString NatureAndQtyOfGoods => AWBHeaderFromManifestedOnShipment?.NatureAndQtyOfGoods ?? ZString.Empty;

		public ZString Currency => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		public ZString ChargesCode => AWBHeaderFromManifestedOnShipment?.EH_ChargesCode ?? ZString.Empty;

		public ZString WeightVPPDCOL => AWBHeaderFromManifestedOnShipment?.EH_WeightVPPDCOL ?? ZString.Empty;

		public ZString OtherPPDCOL => AWBHeaderFromManifestedOnShipment?.EH_OtherPPDCOL ?? ZString.Empty;

		public ZDecimal DeclaredValue => ZDecimal.Zero;

		public ZDecimal CustomsValue => consignment.HVC_GoodsValue;

		public ZDecimal InsuranceValue => ZDecimal.Zero;

		public ZString HouseCustomsValueCurrency => consignment.HVC_RX_NKGoodsValueCurrency;

		public ZString HouseInsuranceValueCurrency => ZString.Empty;

		public ZString HouseDeclaredValueCurrency => ZString.Empty;

		public ZString ShipperTraderNo => AWBHeaderFromManifestedOnShipment?.EH_ShipperTraderNo ?? ZString.Empty;

		public ZString ShipperTraderNoType => AWBHeaderFromManifestedOnShipment?.EH_ShipperTraderNoType ?? ZString.Empty;

		ZString IFBaseMessageDetailsProvider.ShipperCountryCode => consignment.HVC_RN_NKShipperCountryCode;

		public ZString ShipperContactName => consignment.HVC_ShipperContact;

		public ZString ShipperContactCode => !consignment.HVC_ShipperPhone.IsEmpty ? AWB.ContactCodes.TELEPHONE : string.Empty;

		public ZString ShipperContactDetail => consignment.HVC_ShipperPhone;

		public ZString ShipperAccount => consignment.ShipperIsOrganisation ? consignment.ShipperAddress.Header.OH_Code : ZString.Empty;

		public ZString ShipperName => consignment.HVC_ShipperName;

		ZString IFBaseMessageDetailsProvider.ShipperAddress => consignment.HVC_ShipperAddress1;

		public ZString ShipperAddress2 => consignment.HVC_ShipperAddress2;

		public ZString ShipperPlace => consignment.HVC_ShipperCity;

		public ZString ShipperState => consignment.HVC_ShipperState;

		public ZString ShipperPostCode => consignment.HVC_ShipperPostcode;

		public ZString DestinationShipperComment => ZString.Empty;

		public ZString ConsigneeTraderNo => AWBHeaderFromManifestedOnShipment?.EH_ConsigneeTraderNo ?? ZString.Empty;

		public ZString ConsigneeTraderNoType => AWBHeaderFromManifestedOnShipment?.EH_ConsigneeTraderNoType ?? ZString.Empty;

		ZString IFBaseMessageDetailsProvider.ConsigneeCountryCode => consignment.HVC_RN_NKConsigneeCountryCode;

		public ZString ConsigneeContactName => consignment.HVC_ConsigneeContact;

		public ZString ConsigneeContactCode => !consignment.HVC_ConsigneePhone.IsEmpty ? AWB.ContactCodes.TELEPHONE : string.Empty;

		public ZString ConsigneeContactDetail => consignment.HVC_ConsigneePhone;

		public ZString ConsigneeAccount => consignment.ConsigneeIsOrganisation ? consignment.ConsigneeAddress.Header.OH_Code : ZString.Empty;

		public ZString ConsigneeName => consignment.HVC_ConsigneeName;

		ZString IFBaseMessageDetailsProvider.ConsigneeAddress => consignment.HVC_ConsigneeAddress1;

		public ZString ConsigneeAddress2 => consignment.HVC_ConsigneeAddress2;

		public ZString ConsigneePlace => consignment.HVC_ConsigneeCity;

		public ZString ConsigneeState => consignment.HVC_ConsigneeState;

		public ZString ConsigneePostCode => consignment.HVC_ConsigneePostcode;

		public ZString AlsoNotifyTraderNo => AWBHeaderFromManifestedOnShipment?.EH_AlsoNotifyTraderNo ?? ZString.Empty;

		public ZString AlsoNotifyTraderNoType => AWBHeaderFromManifestedOnShipment?.EH_AlsoNotifyTraderNoType ?? ZString.Empty;

		public ZString AlsoNotifyCountryCode => consignment.ManifestedOnShipment?.NotifyParty?.CountryCode ?? ZString.Empty;

		public ZString AlsoNotifyContactName => consignment.ManifestedOnShipment?.NotifyPartyDocumentaryAddress?.E2_Contact ?? ZString.Empty;

		public ZString AlsoNotifyContactCode => !AlsoNotifyContactDetail.IsEmpty ? AWB.ContactCodes.TELEPHONE : string.Empty;

		public ZString AlsoNotifyContactDetail => consignment.ManifestedOnShipment?.NotifyPartyDocumentaryAddress?.E2_Phone ?? string.Empty;

		public ZString FreightForwarderOrCarrierCode => ZString.Empty;

		public ZString AWBOriginCode => consignment.ManifestedOnShipment?.Origin?.RL_IATA ?? ZString.Empty;

		public ZString AirportOfDestinationCode => consignment.ManifestedOnShipment?.Destination?.RL_IATA ?? ZString.Empty;

		public ZInt TotalNoOfPieces => consignment.ActiveItems.Sum(x => x.Lines.Cast<HVLVItemLine>().Sum(line => line.HVS_Quantity));

		public ZDecimal TotalGrossWeight => consignment.HVC_ActualWeight.IsDefault ? consignment.HVC_ManifestedWeight : consignment.HVC_ActualWeight;

		public ZString? AirlineContactNameOCIIdentifier => Airline?.RM_ContactNameOCIIdentifier;

		public ZString? AirlineContactPhoneOCIIdentifier => Airline?.RM_ContactPhoneOCIIdentifier;

		public ZString HandlingInformation => AWBHeaderFromManifestedOnShipment?.EH_HandlingInformation ?? ZString.Empty;

		public ZString ParentTable => AutoHVLVConsignment.Schema.TableName;

		public ZGuid ParentID => consignment.PK;

		public ZString UniqueReference => consignment.HVC_ConsignmentId;

		public ZBool IsDeclarantForAdvancedCargoReporting => AWBHeaderFromManifestedOnShipment?.EH_IsConsigneeDeclarantForAdvanceCargoReporting ?? false;

		public IReadOnlyCollection<ZString> DGCodes
		{
			get => GetSubstances().Select(s => s.DG_Code).ToList().AsReadOnly();
		}

		public IReadOnlyCollection<ZString> DGUNNOValues()
		{
			return GetSubstances().Select(s => s.DG_UNNO).ToList().AsReadOnly();
		}

		IReadOnlyCollection<UNDGSubstance> GetSubstances()
		{
			var items = consignment.Items.OfType<HVLVItem>();
			var query = new ZQuery();
			query.AddToFilter(UNDGDataItemSchema.DI_ParentTableCode, HVLVItemSchema.Constants.Prefix);
			query.AddToFilter(UNDGDataItemSchema.DI_ParentID, items.Select(i => i.PK));

			var uNDGDataItems = consignment.Factory.Load<UNDGDataItem>(query);
			return consignment.Factory.Load<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.PK, uNDGDataItems.Select(d => d.DI_DG))).ToList();
		}

		public IReadOnlyCollection<IAWBRateLineMessageDetailsProvider> AWBRateLines
		{
			get
			{
				var rateLineMessageDetails = new HVLVItemFHLMessageDetailsProvider(consignment.ActiveItems.First());
				return new[] { rateLineMessageDetails }.ToList().AsReadOnly();
			}
		}

		public StringCollectionX GetAvailableHarmonisedCodes()
		{
			var result = new StringCollectionX();
			var hsCodes = new HashSet<string>();

			var itemLines = consignment.Items.OfType<HVLVItem>().SelectMany(item => item.Lines).OfType<HVLVItemLine>();

			foreach (var itemLine in itemLines)
			{
				if (itemLine.ParentItem.HVI_IsActive)
				{
					hsCodes.Add(itemLine.HVS_DestinationTariff);
				}
			}

			result.AddRange(hsCodes.ToArray());
			return result;
		}

		public ZDecimal GetConvertedCurrencyValue(ZDecimal value, ZString valueCurrency, ZString newCurrency)
		{
			if (value <= 0d || valueCurrency.IsEmpty || newCurrency.IsEmpty || valueCurrency == newCurrency)
			{
				return value;
			}

			var valueRefCurrency = RefCurrency.LoadFromCurrencyCode(consignment.Factory, valueCurrency);
			var money = new Money(value, valueRefCurrency);
			var newRefCurrency = RefCurrency.LoadFromCurrencyCode(consignment.Factory, newCurrency);
			var converter = CurrencyConverter.New(consignment.Factory);

			return converter.ConvertExact(money, newRefCurrency).Amount;
		}

		ShipmentExportAWBHeader AWBHeaderFromManifestedOnShipment => consignment.ManifestedOnShipment.AWBHeader as ShipmentExportAWBHeader;

		RefAirline Airline
		{
			get
			{
				if (airline == null && AWBHeaderFromManifestedOnShipment != null)
				{
					if (!AWBHeaderFromManifestedOnShipment.EH_By1st.IsEmpty)
					{
						airline = RefAirline.LoadFromAirline2LetterCode(AWBHeaderFromManifestedOnShipment.Factory, AWBHeaderFromManifestedOnShipment.EH_By1st);
					}

					if (airline == null && !AWBHeaderFromManifestedOnShipment.EH_AirlinePrefix.IsEmpty)
					{
						airline = RefAirline.LoadFromAirlinePrefix(AWBHeaderFromManifestedOnShipment.Factory, AWBHeaderFromManifestedOnShipment.EH_AirlinePrefix);
					}
				}

				return airline;
			}
		}

		public ZString ShipperTraderNoCountryCode => ZString.Empty;

		public ZString ConsigneeTraderNoCountryCode => ZString.Empty;

		public ZString AlsoNotifyTraderNoCountryCode => ZString.Empty;

		public IReadOnlyCollection<IAWBEntryNumberMessageDetailsProvider> CustomsEntryNumbers => Array.Empty<IAWBEntryNumberMessageDetailsProvider>();

		public IReadOnlyCollection<IAWBMovementReferenceNumberMessageDetailsProvider> MovementReferenceNumbers => Array.Empty<IAWBMovementReferenceNumberMessageDetailsProvider>();

		public IReadOnlyCollection<IVATCountryHandler> VATCountryHandlers => AWBHeaderFromManifestedOnShipment?.GetVATCountryHandlers(this) ?? new List<IVATCountryHandler>();

		public IReadOnlyCollection<IContactNumberCountryHandler> ContactNumberCountryHandlers => AWBHeaderFromManifestedOnShipment?.GetContactNumberCountryHandlers(this) ?? new List<IContactNumberCountryHandler>();

		public IReadOnlyCollection<IAWBGoodsDeclarationReferenceNumberMessageDetailsProvider> GoodsDeclarationReferenceNumbers => Array.Empty<IAWBGoodsDeclarationReferenceNumberMessageDetailsProvider>();

		public IReadOnlyCollection<IAWBExportStatementDetailsProvider> ExportStatements => Array.Empty<IAWBExportStatementDetailsProvider>();

		public ZString ShipperContactEmail => consignment.HVC_ShipperEmail;

		public ZString ConsigneeContactEmail => consignment.HVC_ConsigneeEmail;

		public IACASCountryHandler ACASCountryHandler => AWBHeaderFromManifestedOnShipment.GetACASCountryHandler();

		RefAirline airline;

		#endregion
	}
}
