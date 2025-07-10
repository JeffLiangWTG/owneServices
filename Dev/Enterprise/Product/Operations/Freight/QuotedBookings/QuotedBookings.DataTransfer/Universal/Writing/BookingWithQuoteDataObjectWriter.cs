using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	public class BookingWithQuoteDataObjectWriter : ForwardingBookingDataObjectWriter
	{
		public BookingWithQuoteDataObjectWriter(IDataWritingManager manager, QuotedBooking bookingBO) : base(manager, bookingBO)
		{
		}

		protected override void PopulateDataObject(ForwardingShipment shipmentBO, UniversalShipment shipmentData)
		{
			base.PopulateDataObject(shipmentBO, shipmentData);

			shipmentData.QuoteNumber = bookingBO.Quote.TH_QuoteNumber;
			shipmentData.QuoteKPI = ListHelper.GetWithDescription<CodeDescriptionPair>(bookingBO.Quote.CurrentOneOffQuote.TT_QuoteKPI, bookingBO.Quote.CurrentOneOffQuote.Lookups.OneOffQuoteKPIList);
			shipmentData.QuoteSource = ListHelper.GetWithDescription<CodeDescriptionPair>(bookingBO.Quote.CurrentOneOffQuote.TT_QuoteSource, bookingBO.Quote.CurrentOneOffQuote.Lookups.OneOffQuoteSourceList);
			shipmentData.QuoteRevisionReason = ListHelper.GetWithDescription<CodeDescriptionPair>(bookingBO.Quote.CurrentOneOffQuote.TT_RevisionReason, bookingBO.Quote.CurrentOneOffQuote.Lookups.OneOffQuoteRevisionReasonList);
			shipmentData.PortFirstForeign = ListHelper.GetWithName(bookingBO.Via, bookingBO.ViaLocations);

			shipmentData.IsDomesticFreight = bookingBO.IsDomesticFreight;
			shipmentData.ShipmentIncoTerm = ListHelper.GetWithDescription<IncoTerm>(bookingBO.PaymentTerms, bookingBO.IncoTerms);
			shipmentData.AdditionalTerms = bookingBO.AdditionalTerms;

			shipmentData.CarrierServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(bookingBO.CarrierServiceLevel, bookingBO.CarrierServiceLevels);

			shipmentData.TransitTime = ListHelper.GetWithDescription<CodeDescriptionPair>(bookingBO.TransitTime, bookingBO.TransitTimesList);
			shipmentData.Frequency = bookingBO.Frequency;
			shipmentData.FrequencyUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(bookingBO.FrequencyUnit, bookingBO.FrequencyUnits);

			shipmentData.AddOrgAddress(writeManager, bookingBO.Quote?.QuotationClientAddress, DocAddressType.QuotationClientAddress);

			shipmentData.QuoteNumberOfEntries = bookingBO.QuoteNumberOfEntries;
			shipmentData.QuoteNumberOfEntryLines = bookingBO.QuoteNumberOfEntryLines;

			shipmentData.IsQuoteApprovedByManager = bookingBO.OneOffQuoteApprovalStatus;

			PopulateDates(shipmentData);
			PopulateGoodsDetails(shipmentData);

			shipmentData.SetAdditionalReferenceCollection(() =>
			{
				var collection = shipmentData.AdditionalReferenceCollection ?? new DataObjectList<AdditionalReference>();
				collection.AddRange(ProcessCollection(bookingBO.Quote.CurrentOneOffQuote.Numbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete));
				return collection;
			});
		}

		void PopulateDates(UniversalShipment shipmentData)
		{
			shipmentData.DateCollection?.Add(DateType.Start, ZBool.False, bookingBO.StartDate);
			shipmentData.DateCollection?.Add(DateType.End, ZBool.False, bookingBO.EndDate);

			shipmentData.DateCollection?.Add(DateType.Accepted, ZBool.False, bookingBO.Quote.TH_Accepted);
			shipmentData.DateCollection?.Add(DateType.ClientAccepted, ZBool.False, bookingBO.Quote.TH_ClientAccepted);
			shipmentData.DateCollection?.Add(DateType.FollowUp, ZBool.False, bookingBO.Quote.TH_FollowUpDate);
		}

		void PopulateGoodsDetails(UniversalShipment shipmentData)
		{
			shipmentData.TotalWeight = bookingBO.Weight;
			shipmentData.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(bookingBO.WeightUnit, bookingBO.UnitOfWeightList);

			shipmentData.TotalVolume = bookingBO.Volume;
			shipmentData.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(bookingBO.VolumeUnit, bookingBO.UnitOfVolumeList);

			shipmentData.ActualChargeable = bookingBO.Chargeable;

			shipmentData.LocalProcessing.PickupEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(bookingBO.PickupEquipment, bookingBO.Equipments);
			shipmentData.LocalProcessing.DeliveryEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(bookingBO.DeliveryEquipment, bookingBO.Equipments);
			shipmentData.LocalProcessing.Commodity = ListHelper.GetWithDescription<Commodity>(bookingBO.Commodity, bookingBO.Commodities);
		}
	}
}
