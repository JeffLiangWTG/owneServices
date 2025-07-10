using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using IDocAddresses = Enterprise.MasterFiles.Business.IDocAddresses;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	public class BookingWithQuoteDataObjectReader : ForwardingBookingDataObjectReader
	{
		public BookingWithQuoteDataObjectReader(Shipment bookingDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IUniversalFreightHelper helper = null) : base(bookingDataObject, logger, factory, helper)
		{
		}

		protected override IMatchingBusinessEntityFinder<QuotedBooking> GetCombinedReferenceMatcher()
			=> null;

		protected override QuotedBooking GetNewBusinessObject()
			=> QuotedBooking.New(QuoteBookingType.BookingWithQuote, factory.BOFactory);

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(QuotedBooking targetBO)
		{
			if (!string.IsNullOrEmpty(dataObject.QuoteNumber))
			{
				var existingQuote = factory.LoadTop1<RatingHeader>(new ZQuery(RatingHeaderSchema.TH_QuoteNumber, dataObject.QuoteNumber));

				if (existingQuote != null)
				{
					return Res.GetString("42e8ad48-88eb-4062-af87-60dea5308b0c", "[*Provided Quote Number is already in use by an existing quote.*]");
				}
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		protected override void PopulateBusinessObject(QuotedBooking targetBO)
		{
			try
			{
				var complianceUniversalDataObjectReader = new ComplianceUniversalDataObjectReader(targetBO);
				complianceUniversalDataObjectReader.InitializeComplianceMaterialChangesSnapshotIfNeeded();

				base.PopulateBusinessObject(targetBO);

				targetBO.IsImportingData = true;

				var bookingIndexer = GetColumnIndexerFromRow(targetBO.Booking);
				var quoteIndexer = GetColumnIndexer(targetBO.Quote);
				var oneOffQuoteBO = targetBO.Quote.CurrentOneOffQuote;
				var oneOffQuoteIndexer = GetColumnIndexer(oneOffQuoteBO);

				if (!string.IsNullOrWhiteSpace(dataObject.QuoteNumber))
				{
					SetValue(quoteIndexer, RatingHeaderSchema.TH_QuoteNumber, dataObject.QuoteNumber);
					targetBO.Quote.QuoteNumberAlreadySet = true;
				}

				SetAddressOrOverride(nameof(DocAddressType.QuotationClientAddress), targetBO.Quote);
				SetLocalClientAddress(targetBO);

				SetDates(quoteIndexer);
				SetGeneralFields(bookingIndexer, oneOffQuoteIndexer);
				SetBrokerageDetails(oneOffQuoteIndexer);
				SetGoodsDetails(oneOffQuoteIndexer);
				SetMonetaryValues(oneOffQuoteIndexer);
				SetCollection(oneOffQuoteBO);

				complianceUniversalDataObjectReader.SynchronizeComplianceRiskStatusIfNeeded();
			}
			catch (NullReferenceException ex2)
			{
				// WI00728583, Intended to provide more info next time error occurs to identify problem
				targetBO.AddErrorInformation(ex2);
				ExceptionDispatchInfo.Capture(ex2).Throw();
			}
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
			=> GetAddress(dataObject.OrganizationAddressCollection?.FirstOrDefault(x => x.AddressType.HasValue && x.AddressType.Value == addressType));

		OrgAddress GetAddress(OrganizationAddress address)
		{
			return address == null
				? null
				: new OrganisationDataObjectReader(address, logger, factory).GetMatched(canUseUnmatchedOrgNote: true);
		}

		#endregion

		void SetDates(IColumnIndexer quoteIndexer)
		{
			if (dataObject.DateCollection?.Any() ?? false)
			{
				FillDates(quoteIndexer, dataObject.DateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(RatingHeaderSchema.TH_QuoteDate, new[] { DateType.Start }),
					new DateTypeSchemaColumnMap(RatingHeaderSchema.TH_QuoteEndDate, new[] { DateType.End }),
					new DateTypeSchemaColumnMap(RatingHeaderSchema.TH_Accepted, new[] { DateType.Accepted }),
					new DateTypeSchemaColumnMap(RatingHeaderSchema.TH_ClientAccepted, new[] { DateType.ClientAccepted }),
					new DateTypeSchemaColumnMap(RatingHeaderSchema.TH_FollowUpDate, new[] { DateType.FollowUp }));
			}
		}

		void SetGeneralFields(IColumnIndexer bookingIndexer, IColumnIndexer oneOffQuoteIndexer)
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
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_RL_NKReceivalLocation, dataObject.PortOfOrigin);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_RL_NKDeliveryLocation, dataObject.PortOfDestination);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_RL_NKViaLocation, dataObject.PortFirstForeign);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_OH_Carrier, nameof(DocAddressType.ShippingLineAddress), (orgAddress) => orgAddress.Header.PK);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_PL_NKCarrierServiceLevel, dataObject.CarrierServiceLevel);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_TransitTime, dataObject.TransitTime);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_FrequencyUnit, dataObject.FrequencyUnit);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_Frequency, dataObject.Frequency);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_QuoteSource, dataObject.QuoteSource?.Code);
			SetValue(oneOffQuoteIndexer, RateOneOffShipmentSchema.TT_CompanyTariffLevelOverride, dataObject.CompanyTariffLevelOverride);
			SetValue(bookingIndexer, JobShipmentSchema.JS_OH_Creditor, nameof(DocAddressType.Creditor), (orgAddress) => orgAddress.Header.PK);
			SetValue(bookingIndexer, JobShipmentSchema.JS_RS_NKServiceLevel, dataObject.ServiceLevel?.Code);
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

				if (carrierBO != null)
				{
					var rateOneOffCarrier = oneOffQuote.PossibleCarriers.AddNew();
					SetValue(rateOneOffCarrier, RateOneOffCarrierSchema.TTC_OH_Carrier, carrierBO.PK);
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

				rateOneOffPackLine.Parent.CalculatedVolumeIsNotWithinSqlPrecisionAndScale += Parent_CalculatedVolumeIsNotWithinSqlPrecisionAndScale;
				rateOneOffPackLine.Parent.CalculatedWeightIsNotWithinSqlPrecisionAndScale += Parent_CalculatedWeightIsNotWithinSqlPrecisionAndScale;
				rateOneOffPackLine.Parent.CalculatedChargeableIsNotWithinSqlPrecisionAndScale += Parent_CalculatedChargeableIsNotWithinSqlPrecisionAndScale;
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_DimensionUQ, packingLineData.LengthUnit?.Code);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_Length, packingLineData.Length);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_Width, packingLineData.Width);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_Height, packingLineData.Height);

				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_WeightUQ, packingLineData.WeightUnit?.Code);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_Weight, packingLineData.Weight);

				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_VolumeUQ, packingLineData.VolumeUnit?.Code);
				SetValue(rateOneOffPackLine, RateOneOffPackLineSchema.TPL_Volume, packingLineData.Volume);

				rateOneOffPackLine.Parent.CalculatedVolumeIsNotWithinSqlPrecisionAndScale -= Parent_CalculatedVolumeIsNotWithinSqlPrecisionAndScale;
				rateOneOffPackLine.Parent.CalculatedWeightIsNotWithinSqlPrecisionAndScale -= Parent_CalculatedWeightIsNotWithinSqlPrecisionAndScale;
			});
		}

		void Parent_CalculatedChargeableIsNotWithinSqlPrecisionAndScale(object sender, EventArgs e)
		{
			if (sender is RateOneOffShipment rateOneOffShipment)
			{
				logger.Log(LogType.Warning, MaxValueWarning(rateOneOffShipment.CalculatedChargeable.ToString(), RateOneOffShipmentSchema.TT_Chargeable.Name));
			}
		}

		void Parent_CalculatedVolumeIsNotWithinSqlPrecisionAndScale(object sender, EventArgs e)
		{
			if (sender is RateOneOffShipment rateOneOffShipment)
			{
				logger.Log(LogType.Warning, MaxValueWarning(rateOneOffShipment.TotalLooseVolume.ToString(), RateOneOffShipmentSchema.TT_ActualVolume.Name));
			}
		}

		void Parent_CalculatedWeightIsNotWithinSqlPrecisionAndScale(object sender, EventArgs e)
		{
			if (sender is RateOneOffShipment rateOneOffShipment)
			{
				logger.Log(LogType.Warning, MaxValueWarning(rateOneOffShipment.TotalLooseWeight.ToString(), RateOneOffShipmentSchema.TT_ActualVolume.Name));
			}
		}

		static ZString MaxValueWarning(ZString value, ZString column) =>
			Res.GetString("4c4d52bc-e463-43ff-ab05-00ce730b0c6f", "Attempted to insert '{0}' into Field [{1}] in calculating total. Calculated total has been ignored.", value.ToString(), column);

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
	}
}
