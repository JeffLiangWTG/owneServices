using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	internal class OneOffQuoteDataObjectReader : ShipmentDataObjectReader<QuotedBooking>
	{
		public OneOffQuoteDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.OneOffQuote;

		protected override QuotedBooking GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		protected override IMatchingBusinessEntityFinder<QuotedBooking> GetCombinedReferenceMatcher() => null;

		protected override QuotedBooking GetNewBusinessObject()
			=> QuotedBooking.New(QuoteBookingType.SpotQuote, factory.BOFactory);

		protected override void PopulateBusinessObject(QuotedBooking targetBO)
		{
			if (dataObject.IsCO2eResponse())
			{
				var previousCO2eValue = (TotalCO2e: targetBO.GetTotalCO2e(),
										Transports: targetBO.Booking?.Transports ?? Enumerable.Empty<BusinessObject>());
				targetBO.GetResponseImporter(targetBO.Factory, logger).ImportGreenHouseGasEmission(dataObject, targetBO, GetBusinessObjectHumanReadableName(targetBO), previousCO2eValue);
				return;
			}

			targetBO.IsImportingData = true;

			var quoteIndexer = GetColumnIndexer(targetBO.Quote);
			var oneOffQuoteBO = targetBO.Quote.CurrentOneOffQuote;
			var oneOffQuoteIndexer = GetColumnIndexer(oneOffQuoteBO);

			SetAddressOrOverride(nameof(DocAddressType.QuotationClientAddress), targetBO.Quote);
			SetAddressOrOverride(nameof(DocAddressType.OneOffQuotePickupAddress), oneOffQuoteBO);
			SetAddressOrOverride(nameof(DocAddressType.OneOffQuoteDeliveryAddress), oneOffQuoteBO);
			SetLocalClientAddress(targetBO);

			SetCommodity(oneOffQuoteBO);
			SetDates(quoteIndexer);
			SetGeneralFields(oneOffQuoteIndexer);
			SetBrokerageDetails(oneOffQuoteIndexer);
			SetGoodsDetails(oneOffQuoteIndexer);
			SetMonetaryValues(oneOffQuoteIndexer);

			SetCollection(oneOffQuoteBO);

			SetNotes(targetBO);

			PopulateWorkflowCustomFields(targetBO, dataObject);
		}

		#region Organization Address

		void SetAddressOrOverride(string requiredAddressType, IDocAddresses addressParent)
		{
			var universalAddressToMatch = dataObject.OrganizationAddressCollection?.FirstOrDefault(requiredAddressType);
			if (universalAddressToMatch != null)
			{
				new OrganisationDataObjectReader(universalAddressToMatch, logger, factory).GetMatchedOrNew(addressParent);
			}
		}

		void SetValue(IColumnIndexer columnIndexer, SchemaGuidColumn column, string addressType, Func<OrgAddress, ZGuid> getValue)
		{
			var addressBO = GetAddress(addressType);

			if (addressBO != null)
			{
				SetValue(columnIndexer, column, getValue(addressBO));
			}
		}

		void SetLocalClientAddress(QuotedBooking targetBO)
		{
			var clientAddressBO = GetAddress(AddressTypes.SendersLocalClient);

			if (clientAddressBO != null)
			{
				targetBO.TryLoadOrCreateJob();

				using (var job = targetBO.Job)
				{
					if (job != null)
					{
						SetValue(targetBO.Job, JobHeaderSchema.JH_OA_LocalChargesAddr, clientAddressBO.PK);
					}
				}
			}
		}

		OrgAddress GetAddress(string addressType)
		{
			return GetAddress(dataObject.OrganizationAddressCollection?.FirstOrDefault(x => x.AddressType.HasValue && x.AddressType.Value == addressType));
		}

		OrgAddress GetAddress(OrganizationAddress address)
		{
			if (address == null)
			{
				return null;
			}

			return new OrganisationDataObjectReader(address, logger, factory).GetMatched(canUseUnmatchedOrgNote: true);
		}

		#endregion

		void SetDates(IColumnIndexer quoteIndexer)
		{
			if (dataObject.DateCollection?.Any() ?? false)
			{
				FillDates(quoteIndexer, dataObject.DateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(RatingHeaderSchema.TH_QuoteDate, new[] { DateType.Start }),
					new DateTypeSchemaColumnMap(RatingHeaderSchema.TH_QuoteEndDate, new[] { DateType.End }));
			}
		}

		void SetGeneralFields(IColumnIndexer oneOffQuoteIndexer)
		{
			var transportMode = dataObject.TransportMode?.Code;
			var containerMode = dataObject.ContainerMode?.Code;

			var hasValidContainerModeCode = dataObject.ContainerMode?.Code.HasValue ?? false;

			if (!hasValidContainerModeCode)
			{
				transportMode = RatingConstants.GetTransportModeFromMode(dataObject.TransportMode?.Code ?? ZString.Empty);
				containerMode = RatingConstants.GetOneOffQuoteContainerModeFromMode(dataObject.TransportMode?.Code ?? ZString.Empty);
			}

			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_TransportMode, transportMode);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_ContainerMode, containerMode);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_IncoTerm, dataObject.ShipmentIncoTerm);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_AdditionalTerms, dataObject.AdditionalTerms);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_RS_NKServiceLevel, dataObject.ServiceLevel);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_HBLDeliveryMode, dataObject.HBLContainerPackModeOverride);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_RL_NKReceivalLocation, dataObject.PortOfOrigin);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_RL_NKDeliveryLocation, dataObject.PortOfDestination);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_RL_NKViaLocation, dataObject.PortFirstForeign);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_OH_Carrier, nameof(DocAddressType.ShippingLineAddress), (orgAddress) => orgAddress.Header.PK);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_OH_Creditor, nameof(DocAddressType.Creditor), (orgAddress) => orgAddress.Header.PK);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_PL_NKCarrierServiceLevel, dataObject.CarrierServiceLevel);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_TransitTime, dataObject.TransitTime);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_FrequencyUnit, dataObject.FrequencyUnit);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_Frequency, dataObject.Frequency);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_QuoteSource, dataObject.QuoteSource?.Code);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_CompanyTariffLevelOverride, dataObject.CompanyTariffLevelOverride);
		}

		void SetBrokerageDetails(IColumnIndexer oneOffQuoteIndexer)
		{
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_NumberOfEntries, dataObject.QuoteNumberOfEntries);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_NumberOfEntryLines, dataObject.QuoteNumberOfEntryLines);
		}

		void SetGoodsDetails(IColumnIndexer oneOffQuoteIndexer)
		{
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_UnitOfWeight, dataObject.TotalWeightUnit);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_ActualWeight, dataObject.TotalWeight);

			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_UnitOfVolume, dataObject.TotalVolumeUnit);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_ActualVolume, dataObject.TotalVolume);

			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_Chargeable, dataObject.ActualChargeable);

			if (dataObject.LocalProcessing != null)
			{
				SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_PickupEquipment, dataObject.LocalProcessing.PickupEquipmentNeeded);
				SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_DeliveryEquipment, dataObject.LocalProcessing.DeliveryEquipmentNeeded);
				SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_RH_NKCommodity, dataObject.LocalProcessing.Commodity);
			}
		}

		void SetMonetaryValues(IColumnIndexer oneOffQuoteIndexer)
		{
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_RX_NKGoodsCurrency, dataObject.GoodsValueCurrency?.Code);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_ValueOfGoods, dataObject.GoodsValue);

			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_RX_NKInsureValCurr, dataObject.InsuranceValueCurrency?.Code);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_InsureVal, dataObject.InsuranceValue);
		}

		void SetCommodity(RateOneOffShipment oneOffQuote)
		{
			oneOffQuote.TT_RH_NKCommodity = dataObject.RateCommodity?.Code ?? string.Empty;
			oneOffQuote.TT_FMCTariffID = dataObject.FMCTariffID ?? string.Empty;
		}

		void SetCollection(RateOneOffShipment oneOffQuote)
		{
			SetPotentialCarrierCollection(oneOffQuote);
			SetContainerCollection(oneOffQuote);
			SetPackingLineCollection(oneOffQuote);
			SetAdditionalReferenceCollection(oneOffQuote);
		}

		void SetPotentialCarrierCollection(RateOneOffShipment oneOffQuote)
		{
			var localCodeMatcher = new OrgLocalCodeMatcher(factory.BOFactory);

			dataObject.PotentialCarrierCollection?.ForEach(carrier =>
			{
				var carrierBO = localCodeMatcher.Match(carrier.Code.Value, null);
				var creditorBO = localCodeMatcher.Match(carrier.Creditor?.Code.Value ?? "", null);

				if (carrierBO is not null)
				{
					var rateOneOffCarrier = oneOffQuote.PossibleCarriers.AddNew();
					SetValue(rateOneOffCarrier, RateOneOffCarrierSchema.TTC_OH_Carrier, carrierBO.PK);

					if (creditorBO is not null)
					{
						SetValue(rateOneOffCarrier, RateOneOffCarrierSchema.TTC_OH_Creditor, creditorBO.PK);
					}
				}
			});
		}

		void SetContainerCollection(RateOneOffShipment oneOffQuote)
		{
			dataObject.ContainerCollection?.ForEach(container =>
			{
				if (container.ContainerType?.Code.HasValue ?? false)
				{
					var containerBO = factory.BOFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, container.ContainerType.Code.Value);

					if (containerBO != null)
					{
						var rateOneOffContainers = oneOffQuote.Containers.AddNew();

						SetValue(rateOneOffContainers, RateOneOffContainersSchema.TC_RC, containerBO.PK);
						SetValue(rateOneOffContainers, RateOneOffContainersSchema.TC_ContainerCount, container.ContainerCount);
					}
				}
			});
		}

		void SetPackingLineCollection(RateOneOffShipment oneOffQuote)
		{
			dataObject.PackingLineCollection?.ForEach(packingLineData =>
			{
				var rateOneOffPackLine = oneOffQuote.LooseCargo.AddNew();
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_PackLineCount, packingLineData.PackQty);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_F3_NKPackType, packingLineData.PackType?.Code);

				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_DimensionUQ, packingLineData.LengthUnit?.Code);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_Length, packingLineData.Length);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_Width, packingLineData.Width);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_Height, packingLineData.Height);

				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_WeightUQ, packingLineData.WeightUnit?.Code);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_Weight, packingLineData.Weight);

				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_VolumeUQ, packingLineData.VolumeUnit?.Code);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_Volume, packingLineData.Volume);
			});
		}

		void SetAdditionalReferenceCollection(RateOneOffShipment oneOffQuote)
		{
			if (dataObject.AdditionalReferenceCollection != null)
			{
				var additionalReferenceCollectionReader = new RateOneOffShipmentAdditionalReferenceCollectionReader(
					dataObject.AdditionalReferenceCollection,
					logger,
					factory,
					oneOffQuote);
				additionalReferenceCollectionReader.ReadIntoCollection();
			}
		}

		void SetNotes(QuotedBooking quotedBooking)
		{
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, quotedBooking.Quote).ReadIntoCollection();
			}
		}
	}
}
