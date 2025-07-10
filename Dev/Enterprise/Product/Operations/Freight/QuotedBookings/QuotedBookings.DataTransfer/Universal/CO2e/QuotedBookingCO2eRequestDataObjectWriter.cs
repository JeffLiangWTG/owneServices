using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.DataTransfer
{
	public sealed class QuotedBookingCO2eRequestDataObjectWriter : CO2eLegBasedRequestDataObjectWriter<QuotedBooking, JobSailing>
	{
		public QuotedBookingCO2eRequestDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
			quotedBooking = writeManager.Action?.ParentBO as QuotedBooking;
		}

		readonly QuotedBooking quotedBooking;

		protected override void PopulateDataObject(QuotedBooking quotedBooking, Shipment shipment)
		{
			base.PopulateDataObject(quotedBooking, shipment);

			var carrierBO = quotedBooking.Carrier;
			if (carrierBO != null)
			{
				shipment.AddOrgAddress(new CO2eOrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ShippingLineAddress)).GetDataObject(carrierBO.MainAddress));
			}
		}

		protected override DataObjectWriter<JobSailing, TransportLeg> GetLegDataObjectWriter()
		{
			return new CO2eScheduleTransportLegDataObjectWriter(writeManager);
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return quotedBooking == null || quotedBooking.Booking != null ? DataContextType.ForwardingBooking : DataContextType.Quotation;
		}

		protected override bool UseRoadForFirstAndLastLegs => false;

		protected override void PopulateLegs(Shipment shipmentData, QuotedBooking sourceBO)
		{
			if (sourceBO.ObjectState == QuotedBookingState.QuoteOnly)
			{
				PopulateLegsForOneOffQuote(shipmentData, sourceBO);
			}
			else
			{
				base.PopulateLegs(shipmentData, sourceBO);
			}
		}

		void PopulateLegsForOneOffQuote(Shipment shipmentData, QuotedBooking sourceBO)
		{
			shipmentData.SetTransportLegCollection(() =>
			{
				var result = new DataObjectList<TransportLeg> { Content = CollectionContent.Complete };

				if (!string.IsNullOrEmpty(sourceBO.Via))
				{
					var viaUNLOCO = sourceBO.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, sourceBO.Via);
					var transportMode = new TransportModeConverter().ToEnumValue(sourceBO.ConvertTransportModeForCO2eCalculation());

					var transportLeg1 = new TransportLeg(writeManager.WriterStrategy);
					transportLeg1.LegOrder = 0;
					transportLeg1.TransportMode = transportMode;
					transportLeg1.PortOfLoading = GetUNLOCOByPortAndTransportMode(sourceBO.OriginUNLOCO, sourceBO.ConvertTransportModeForCO2eCalculation());
					transportLeg1.PortOfDischarge = GetUNLOCOByPortAndTransportMode(viaUNLOCO, sourceBO.ConvertTransportModeForCO2eCalculation());
					result.Add(transportLeg1);

					var transportLeg2 = new TransportLeg(writeManager.WriterStrategy);
					transportLeg2.LegOrder = 1;
					transportLeg2.TransportMode = transportMode;
					transportLeg2.PortOfLoading = GetUNLOCOByPortAndTransportMode(viaUNLOCO, sourceBO.ConvertTransportModeForCO2eCalculation());
					transportLeg2.PortOfDischarge = GetUNLOCOByPortAndTransportMode(sourceBO.DestinationUNLOCO, sourceBO.ConvertTransportModeForCO2eCalculation());
					result.Add(transportLeg2);
				}

				return result;
			});
		}
	}
}
