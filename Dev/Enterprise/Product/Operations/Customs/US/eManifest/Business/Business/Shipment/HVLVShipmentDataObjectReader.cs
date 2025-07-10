using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class HVLVShipmentDataObjectReader : ShipmentDataObjectReader<Shipment>
	{
		public HVLVShipmentDataObjectReader(UniversalXml.Shipment dataObject,
			Trip trip,
			UniversalXml.Shipment shipmentLevelDataObject,
			UniversalXml.Shipment consolLevelDataObject,
			IXmlImportLogger logger,
			UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			this.shipmentLevelDataObject = Argument.NotNull(shipmentLevelDataObject, nameof(shipmentLevelDataObject));
			this.consolLevelDataObject = Argument.NotNull(consolLevelDataObject, nameof(consolLevelDataObject));
			currencyConverter = CurrencyConverter.New(factory.BOFactory, ZDateTime.Now, ZArchitecture.Core.ExchangeRateType.Customs, true);
			this.trip = trip;
		}

		readonly UniversalXml.Shipment shipmentLevelDataObject;
		readonly UniversalXml.Shipment consolLevelDataObject;
		readonly CurrencyConverter currencyConverter;
		readonly Trip trip;

		protected override CharacterCase StringValueCharacterCase => CharacterCase.Upper;

		public override DataContextType DataContextType => DataContextType.USeManifestShipment;

		protected override IMatchingBusinessEntityFinder<Shipment> GetCombinedReferenceMatcher() => null;

		protected override Shipment GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			if (dataObject.WayBillNumber.HasValue)
			{
				var waybillNumber = dataObject.WayBillNumber.Value;
				return trip.GetShipmentFromLookup(waybillNumber);
			}

			return null;
		}

		protected override void PopulateBusinessObject(Shipment shipmentBO)
		{
			using (new CopyingOperation(shipmentBO))
			using (shipmentBO.SuspendPopulateCommodityValue())
			{
				SetValue(shipmentBO, CusInBondBillSchema.B0_ShipmentType, ShipmentTypes.Codes.LowValue);
				SetValue(shipmentBO, CusInBondBillSchema.B0_MasterBillNumber, dataObject.WayBillNumber);
				SetValue(shipmentBO, CusInBondBillSchema.B0_ReferenceID, dataObject.OwnerRef);
				if (consolLevelDataObject.PortOfLoading?.Code is ZString portOfLoadingCode)
				{
					SetValue(shipmentBO, CusInBondBillSchema.B0_RL_NKPortOfLading, portOfLoadingCode);
					var ladingPortScheduleKCode = USScheduleResolver.GetScheduleCode(MasterFiles.Business.Schedule.K, portOfLoadingCode, USLocoMapSystemUsageList.Codes.SCK, factory.BOFactory);
					SetValue(shipmentBO, CusInBondBillSchema.B0_PortOfLadingKCode, ladingPortScheduleKCode);
				}

				SetValue(shipmentBO, CusInBondBillSchema.B0_DateOfExport, shipmentLevelDataObject.DateCollection?.FirstOrDefault(dt => dt.Type == DateType.Departure)?.Value);
				SetValue(shipmentBO, CusInBondBillSchema.B0_PlaceOfReceipt, dataObject.PlaceOfReceipt?.Code ?? ZString.Empty);
				SetValue(shipmentBO, CusInBondBillSchema.B0_ManifestQty, dataObject.OuterPacks);
				SetValue(shipmentBO, CusInBondBillSchema.B0_ManifestUQ, PackageTypes.Codes.Pieces);
				SetValue(shipmentBO, CusInBondBillSchema.B0_Weight, dataObject.TotalWeight);
				SetValue(shipmentBO, CusInBondBillSchema.B0_WeightUQ, dataObject.TotalWeightUnit?.Code ?? ZString.Empty);
				SetValue(shipmentBO, CusInBondBillSchema.B0_Volume, dataObject.TotalVolume);
				SetValue(shipmentBO, CusInBondBillSchema.B0_VolumeUQ, dataObject.TotalVolumeUnit?.Code ?? ZString.Empty);

				PopulateGoodsValue(shipmentBO);
				PopulateCountryOfOrigin(shipmentBO);
				PopulateCarrierSCAC(shipmentBO);
				PopulateConsignee(shipmentBO);
				PopulateShipper(shipmentBO);
				PopulateCommodities(shipmentBO);
			}
		}

		ZDecimal ConvertToUSD(ZDecimal amount, ZString currencyCode)
		{
			if (currencyCode.IsEmpty)
			{
				return amount;
			}

			var currency = GetCurrency(currencyCode);
			var usd = GetCurrency(CurrencyCodes.UnitedStates);
			return currency != null ? currencyConverter.ConvertExact(new Money(amount, currency), usd).Amount.Round(0) : amount;
		}

		ZDecimal GetValueInUSD(ZDecimal? amount)
		{
			var valueInUSD = ZDecimal.Zero;
			if (amount.HasValue && !amount.Value.IsEmpty)
			{
				valueInUSD = ConvertToUSD(amount.Value, dataObject.GoodsValueCurrency?.Code ?? ZString.Empty);
			}
			return valueInUSD;
		}

		RefCurrency GetCurrency(ZString code)
		{
			RefCurrency result = null;
			if (currencyList == null)
			{
				currencyList = factory.GetCachedValue("Customs|US.eManifest|HVLVShipmentDataObjectReader|CurrencyCache", () => new Dictionary<ZString, RefCurrency>());
			}
			if (!currencyList.TryGetValue(code, out result) || result == null)
			{
				result = RefCurrency.LoadFromCurrencyCode(factory.BOFactory, code);
				if (result != null)
				{
					currencyList.Add(code, result);
				}
			}

			return result;
		}
		Dictionary<ZString, RefCurrency> currencyList;

		void PopulateCarrierSCAC(Shipment shipmentBO)
		{
			if (!trip.BH_CarrierSCAC.IsEmpty)
			{
				SetValue(shipmentBO, CusInBondBillSchema.B0_IssuerSCAC, trip.BH_CarrierSCAC);
			}
		}

		#region Populate Methods

		void PopulateGoodsValue(Shipment shipmentBO)
		{
			var commercialInvoiceLines = dataObject.CommercialInfo?.CommercialInvoiceCollection?.FirstOrDefault()?.CommercialInvoiceLineCollection;
			if (commercialInvoiceLines != null && commercialInvoiceLines.Count > 0)
			{
				var totalLinesValue = commercialInvoiceLines.Sum(x => GetValueInUSD(x.CustomsValue));
				SetValue(shipmentBO, CusInBondBillSchema.B0_GoodsValue, totalLinesValue);
			}
		}

		void PopulateCountryOfOrigin(Shipment shipmentBO)
		{
			var commercialInvoiceHeader = dataObject.CommercialInfo?.CommercialInvoiceCollection?.FirstOrDefault();

			if (commercialInvoiceHeader != null)
			{
				var countryCode = commercialInvoiceHeader?.CommercialInvoiceLineCollection?.FirstOrDefault(l => !string.IsNullOrEmpty(l.CountryOfOrigin?.Code))?.CountryOfOrigin?.Code ?? ZString.Empty;
				if (!countryCode.IsEmpty)
				{
					SetValue(shipmentBO, CusInBondBillSchema.B0_RN_NKCountryOfExport, countryCode);
				}
			}

			if (shipmentBO.B0_RN_NKCountryOfExport.IsEmpty)
			{
				SetValue(shipmentBO, CusInBondBillSchema.B0_RN_NKCountryOfExport, consolLevelDataObject.PortOfLoading?.Code ?? ZString.Empty);
			}
		}

		void PopulateCountryOfOrigin(Commodity commodityBO, CommercialInvoiceLine commercialInvoiceLine)
		{
			var itemLineCountryOfOrigin = commercialInvoiceLine.CountryOfOrigin?.Code ?? ZString.Empty;

			if (!itemLineCountryOfOrigin.IsEmpty)
			{
				commodityBO.BY_RN_NKCountryOfOrigin = itemLineCountryOfOrigin;
			}
			else
			{
				var unloco = consolLevelDataObject.PortOfLoading?.Code ?? ZString.Empty;
				if (!unloco.IsEmpty)
				{
					var countryCode = unloco.Substring(0, 2);
					commodityBO.BY_RN_NKCountryOfOrigin = countryCode;
				}
			}
		}

		void PopulateCommodities(Shipment shipmentBO)
		{
			DeleteExistingCommoditiesIfRequired(shipmentBO);

			var commercialInvoiceLines = dataObject.CommercialInfo?.CommercialInvoiceCollection?.FirstOrDefault()?.CommercialInvoiceLineCollection;
			if (commercialInvoiceLines != null && commercialInvoiceLines.Count > 0)
			{
				foreach (var line in commercialInvoiceLines)
				{
					var commodity = factory.New<Commodity>();
					using (new CopyingOperation(commodity))
					{
						commodity.BY_ParentID = shipmentBO.PK;
						commodity.BY_ParentTableCode = shipmentBO.TablePrefix;
						commodity.Shipment = shipmentBO;

						commodity.BY_Description = GetCommodityDescriptionCandidates(line)
							.FirstOrDefault(d => !string.IsNullOrEmpty(d));

						new CommodityDefaultsManager(commodity).SetDefaults();

						commodity.BY_MonetaryValue = GetValueInUSD(line.CustomsValue);
						commodity.BY_GrossWeight = line.Weight ?? ZDecimal.Zero;
						commodity.BY_GrossWeightUnit = line.WeightUnit?.Code ?? ZString.Empty;

						commodity.BY_HarmonizedNumbers = line.HarmonisedCode.GetValueOrDefault();
						commodity.BY_PieceCount = line.InvoiceQuantity.Value.ToZInt();
						commodity.BY_ManifestUnitCode = PackageTypes.Codes.Pieces;
						PopulateCountryOfOrigin(commodity, line);
					}
				}
			}
		}

		void DeleteExistingCommoditiesIfRequired(Shipment shipmentBO)
		{
			if (shipmentBO.IsInDatabase)
			{
				var query = new ZQuery();
				query.AddToFilter(CusInBondCargoDescSchema.BY_ParentID, shipmentBO.PK);
				query.AddToFilter(CusInBondCargoDescSchema.BY_ParentTableCode, shipmentBO.TablePrefix);

				var existingCommodities = factory.Load<Commodity>(query);
				if (existingCommodities.Length > 0)
				{
					existingCommodities.ForEach(x => x.Delete());
				}
			}
		}

		IEnumerable<ZString> GetCommodityDescriptionCandidates(CommercialInvoiceLine invoiceLine)
		{
			yield return invoiceLine.Description.GetValueOrDefault();
			if (dataObject.PackingLineCollection != null)
			{
				var matchingPackingLine = dataObject.PackingLineCollection.FirstOrDefault(
					line =>
					{
						if (line.PackedItemCollection != null)
						{
							return line.PackedItemCollection.Any(item => invoiceLine.Link.Equals(item.CommercialInvoiceLineLink));
						}

						return false;
					});

				if (matchingPackingLine != null)
				{
					yield return matchingPackingLine.GoodsDescription.GetValueOrDefault();
				}
			}

			yield return dataObject.GoodsDescription.GetValueOrDefault();
		}

		void PopulateConsignee(Shipment shipmentBO)
		{
			var consigneeDocumentaryAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			if (consigneeDocumentaryAddress != null)
			{
				var isAddressOverride = consigneeDocumentaryAddress.AddressOverride ?? false;
				var consignee = Party.LoadOrCreate(shipmentBO, PartyTypes.Codes.Consignee);
				using (new CopyingOperation(consignee))
				{
					if (isAddressOverride)
					{
						PopulatePartyFromAddressOverride(consignee, consigneeDocumentaryAddress);
					}
					else
					{
						var reader = new OrganisationDataObjectReader(consigneeDocumentaryAddress, logger, factory);
						var consigneeAddress = reader.GetMatched();
						if (consigneeAddress != null)
						{
							consignee.OrganisationPK = consigneeAddress.OA_OH;
							consignee.ContactPK = consigneeAddress.Header.Contacts.FirstOrDefault()?.PK ?? ZGuid.Empty;
							reader.PopulateJobDocAddress(consigneeAddress, shipmentBO.Consignee);
						}
					}
				}
			}
		}

		void PopulateShipper(Shipment shipmentBO)
		{
			var consignorDocumentaryAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress));
			if (consignorDocumentaryAddress != null)
			{
				var isAddressOverride = consignorDocumentaryAddress.AddressOverride ?? false;
				var shipper = Party.LoadOrCreate(shipmentBO, PartyTypes.Codes.Shipper);
				using (new CopyingOperation(shipper))
				{
					if (isAddressOverride)
					{
						PopulatePartyFromAddressOverride(shipper, consignorDocumentaryAddress);
					}
					else
					{
						var reader = new OrganisationDataObjectReader(consignorDocumentaryAddress, logger, factory);
						var consignorAddress = reader.GetMatched();
						if (consignorAddress != null)
						{
							shipper.OrganisationPK = consignorAddress.OA_OH;
							shipper.ContactPK = consignorAddress.Header.Contacts.FirstOrDefault()?.PK ?? ZGuid.Empty;
							reader.PopulateJobDocAddress(consignorAddress, shipmentBO.Shipper);
						}
					}
				}
			}
		}

		void PopulatePartyFromAddressOverride(Party party, OrganizationAddress addressDataObject)
		{
			using (party.SuspendSettingHasChangesIncludingChildren())
			{
				party.BypassFireEventBeforeChange = true;
				party.E2_SuppressAddressValidationError = true;
				party.E2_CompanyName = addressDataObject.CompanyName ?? ZString.Empty;
				party.E2_Address1 = addressDataObject.Address1 ?? ZString.Empty;
				party.E2_Address2 = addressDataObject.Address2 ?? ZString.Empty;
				party.E2_City = addressDataObject.City ?? ZString.Empty;
				party.E2_Postcode = addressDataObject.Postcode ?? ZString.Empty;
				party.E2_State = addressDataObject.State?.Code ?? ZString.Empty;
				party.E2_RN_NKCountryCode = addressDataObject.Country?.Code ?? ZString.Empty;
				party.E2_Contact = addressDataObject.Contact ?? ZString.Empty;
				party.E2_Phone = addressDataObject.Phone ?? ZString.Empty;
				party.E2_Mobile = addressDataObject.Mobile ?? ZString.Empty;
				party.E2_Fax = addressDataObject.Fax ?? ZString.Empty;
				party.E2_Email = addressDataObject.Email ?? ZString.Empty;
				party.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
				party.E2_AddressOverride = true;
			}
		}

		#endregion
	}
}
