using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	internal class OneOffQuoteDataObjectWriter : TopLevelDataObjectWriter<Quote, Shipment>
	{
		public OneOffQuoteDataObjectWriter(IDataWritingManager writeManager, QuotedBooking quotedBooking) : base(writeManager)
		{
			this.quotedBooking = quotedBooking;
		}

		QuotedBooking quotedBooking;

		protected override Quote GetTypedBusinessObject(BusinessObject sourceBO)
		{
			if (quotedBooking == null)
			{
				quotedBooking = ((QuotedBooking)sourceBO);
			}

			return quotedBooking.Quote;
		}

		protected override ZString GetEDIMessageSubType() => "XUS";

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.OneOffQuote;

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(Quote quote) => quotedBooking.GetUserDefinedValues();

		protected override IEnumerable<IPropertyValue> GetAllCustomPropertiesWithDefaultValue(Quote quote) => GetAllCustomPropertiesWithDefaultValue(quotedBooking);

		protected override IDataContextManager GetDataContextManager(BusinessObject sourceBO) => quotedBooking.GetUniversalDataContextManager();

		protected override void PopulateDataObject(Quote quote, Shipment shipmentData)
		{
			PopulateCommodities(shipmentData);
			PopulateOrganizations(shipmentData, quote);
			PopulateDates(shipmentData);
			PopulateGeneralFields(shipmentData);
			PopulateBrokerageDetails(shipmentData);
			PopulateGoodsDetails(shipmentData);
			PopulateMonetaryValues(shipmentData);
			PopulateCollection(shipmentData, quote);
			PopulateNotes(shipmentData);
			PopulateStatusFields(shipmentData);
			PopulateCO2eFields(shipmentData, quotedBooking);
		}

		void PopulateCO2eFields(Shipment shipmentData, QuotedBooking quotedBooking)
		{
			shipmentData.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = quotedBooking.GetTotalCO2e(),
				CO2eUnit = ListHelper.GetWithDescription<UnitOfWeight>(Constants.Weight.Kilograms, quotedBooking.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight)),
				CO2eDescriptiveStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(quotedBooking.GetCO2eStatus(), new CO2eStatusList())
					.AdditionalSetup(cdp => cdp.Description = CO2eHelper.GetCO2eStatusShortDescription(cdp.Code)),
			};
		}

		void PopulateOrganizations(Shipment shipmentData, Quote quote)
		{
			var addressList = new List<JobDocAddress> { quote.QuotationClientAddress, quotedBooking.ConsignorDocumentaryAddress, quotedBooking.ConsigneeDocumentaryAddress };
			shipmentData.SetOrganizationAddressCollection(() => ProcessCollection(addressList, new JobDocAddressDataObjectWriter(writeManager)));

			shipmentData.SetLocalClientAddress(writeManager, quotedBooking);
		}

		void PopulateDates(Shipment shipmentData)
		{
			shipmentData.SetDateCollection(() =>
			{
				var dates = new List<Date>();
				dates.Add(DateType.Start, ZBool.False, quotedBooking.StartDate);
				dates.Add(DateType.End, ZBool.False, quotedBooking.EndDate);

				dates.Add(DateType.Accepted, ZBool.False, quotedBooking.Quote.TH_Accepted);
				dates.Add(DateType.ClientAccepted, ZBool.False, quotedBooking.Quote.TH_ClientAccepted);
				dates.Add(DateType.FollowUp, ZBool.False, quotedBooking.Quote.TH_FollowUpDate);

				return dates;
			});
		}

		void PopulateCommodities(Shipment shipmentData)
		{
			shipmentData.RateCommodity = ListHelper.GetWithDescription<Commodity>(quotedBooking.Commodity, quotedBooking.CommodityCodeList);
			shipmentData.FMCTariffID = quotedBooking.FMCTariffID;
		}

		void PopulateGeneralFields(Shipment shipmentData)
		{
			shipmentData.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(quotedBooking.TransportMode, quotedBooking.TransportModes);
			shipmentData.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(quotedBooking.ContainerMode, quotedBooking.ContainerModes);

			shipmentData.IsDomesticFreight = quotedBooking.IsDomesticFreight;
			shipmentData.ShipmentIncoTerm = ListHelper.GetWithDescription<UniversalDataBuss.DataObjects.Universal.IncoTerm>(quotedBooking.PaymentTerms, quotedBooking.IncoTerms);
			shipmentData.AdditionalTerms = quotedBooking.AdditionalTerms;

			shipmentData.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(quotedBooking.ServiceLevel, quotedBooking.ServiceLevels);

			var hblDeliveryMode = ListHelper.GetWithDescription<CodeDescriptionPair>(quotedBooking.Quote.CurrentOneOffQuote.TT_HBLDeliveryMode, quotedBooking.HBLDeliveryModes);
			shipmentData.HBLContainerPackModeOverride = hblDeliveryMode.Code;

			shipmentData.PortOfOrigin = ListHelper.GetWithName(quotedBooking.Origin, quotedBooking.ReceivalLocations);
			shipmentData.PortOfDestination = ListHelper.GetWithName(quotedBooking.Destination, quotedBooking.DeliveryLocations);
			shipmentData.PortFirstForeign = ListHelper.GetWithName(quotedBooking.Via, quotedBooking.ViaLocations);

			shipmentData.SetCarrier(writeManager, quotedBooking);
			shipmentData.CarrierServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(quotedBooking.CarrierServiceLevel, quotedBooking.CarrierServiceLevels);

			shipmentData.SetCreditor(writeManager, quotedBooking);

			shipmentData.TransitTime = ListHelper.GetWithDescription<CodeDescriptionPair>(quotedBooking.TransitTime, quotedBooking.TransitTimesList);
			shipmentData.Frequency = quotedBooking.Frequency;
			shipmentData.FrequencyUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(quotedBooking.FrequencyUnit, quotedBooking.FrequencyUnits);

			shipmentData.QuoteKPI = ListHelper.GetWithDescription<CodeDescriptionPair>(quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteKPI, quotedBooking.Quote.CurrentOneOffQuote.Lookups.OneOffQuoteKPIList);
			shipmentData.QuoteSource = ListHelper.GetWithDescription<CodeDescriptionPair>(quotedBooking.Quote.CurrentOneOffQuote.TT_QuoteSource, quotedBooking.Quote.CurrentOneOffQuote.Lookups.OneOffQuoteSourceList);
			shipmentData.QuoteRevisionReason = ListHelper.GetWithDescription<CodeDescriptionPair>(quotedBooking.Quote.CurrentOneOffQuote.TT_RevisionReason, quotedBooking.Quote.CurrentOneOffQuote.Lookups.OneOffQuoteRevisionReasonList);
			shipmentData.CompanyTariffLevelOverride = quotedBooking.Quote?.CurrentOneOffQuote?.TT_CompanyTariffLevelOverride ?? ZByte.Zero;
		}

		void PopulateBrokerageDetails(Shipment shipmentData)
		{
			shipmentData.QuoteNumberOfEntries = quotedBooking.QuoteNumberOfEntries;
			shipmentData.QuoteNumberOfEntryLines = quotedBooking.QuoteNumberOfEntryLines;
		}

		void PopulateGoodsDetails(Shipment shipmentData)
		{
			shipmentData.TotalWeight = quotedBooking.Weight;
			shipmentData.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(quotedBooking.WeightUnit, quotedBooking.UnitOfWeightList);

			shipmentData.TotalVolume = quotedBooking.Volume;
			shipmentData.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(quotedBooking.VolumeUnit, quotedBooking.UnitOfVolumeList);

			shipmentData.ActualChargeable = quotedBooking.Chargeable;

			shipmentData.LocalProcessing = new LocalProcessing(writeManager.WriterStrategy)
			{
				PickupEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(quotedBooking.PickupEquipment, quotedBooking.Equipments),
				DeliveryEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(quotedBooking.DeliveryEquipment, quotedBooking.Equipments),
				Commodity = ListHelper.GetWithDescription<Commodity>(quotedBooking.Commodity, quotedBooking.Commodities)
			};
		}

		void PopulateMonetaryValues(Shipment shipmentData)
		{
			shipmentData.GoodsValue = quotedBooking.GoodsValue;
			shipmentData.GoodsValueCurrency = ListHelper.GetWithDescription<Currency>(quotedBooking.GoodsCurrency, quotedBooking.Currencies);

			shipmentData.InsuranceValue = quotedBooking.InsuranceValue;
			shipmentData.InsuranceValueCurrency = ListHelper.GetWithDescription<Currency>(quotedBooking.InsuranceCurrency, quotedBooking.Currencies);
		}

		void PopulateCollection(Shipment shipmentData, Quote quote)
		{
			shipmentData.SetPotentialCarrierCollection(() => ProcessCollection(quote.CurrentOneOffQuote.PossibleCarriers, new RateOneOffPotentialCarrierDataObjectWriter(writeManager)));
			shipmentData.SetContainerCollection(() => ProcessCollection(quote.CurrentOneOffQuote.Containers, new RateOneOffContainerDataObjectWriter(writeManager), CollectionContent.Complete, true));
			shipmentData.SetPackingLineCollection(() => ProcessCollection(quote.CurrentOneOffQuote.LooseCargo, new RateOneOffPackingLineDataObjectWriter(writeManager), CollectionContent.Complete, true));
			shipmentData.SetAdditionalReferenceCollection(() => ProcessCollection(quote.CurrentOneOffQuote.Numbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete));
		}

		void PopulateNotes(Shipment shipmentData)
		{
			var notes = quotedBooking.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			shipmentData.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
		}

		void PopulateStatusFields(Shipment shipmentData)
		{
			shipmentData.IsCancelled = quotedBooking.Quote.TH_IsCancelled;
			shipmentData.IsQuoteApprovedByManager = quotedBooking.OneOffQuoteApprovalStatus;
		}
	}
}
