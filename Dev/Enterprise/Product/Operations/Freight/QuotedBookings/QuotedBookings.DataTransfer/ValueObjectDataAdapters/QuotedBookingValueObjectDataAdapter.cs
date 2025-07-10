using System;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using OrganisationTypes = Enterprise.MasterFiles.Integration.OrganisationTypes;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.QuotedBookings.DataTransfer
{
	public class QuotedBookingValueObjectDataAdapter : ValueObjectDataAdapter<QuotedBooking, Xsd.ShipmentBooking>
	{
		public QuotedBookingValueObjectDataAdapter()
			: this(EventsWithSourceType.Empty)
		{
		}

		public QuotedBookingValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			this.TriggeredByEvents = triggeredByEvents;
		}
		protected readonly EventsWithSourceType TriggeredByEvents;

		#region XML Schema Overrides

		public override string RootCollectionElementName
		{
			get { return "ShipmentBookings"; }
		}

		public override string RootElementName
		{
			get { return "ShipmentBooking"; }
		}

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleShipmentBookingSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.ShipmentBookingsSchema; }
		}

		#endregion

		#region Business Objects

		protected override QuotedBooking FindBusinessObject(Xsd.ShipmentBooking shipmentBookingValue, IValueObjectImportContext context)
		{
			ZQuery bookingQuery = new ZQuery(JobShipmentSchema.JS_IsBooking, true);
			bookingQuery.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, false);
			var shipmentLocator = new ShipmentLocator<CommonShipment>(context.Factory, bookingQuery);

			ViewQuotedBooking viewQuotedBooking = null;
			CommonShipment booking = shipmentLocator.Find(shipmentBookingValue.Shipment);

			if (booking != null)
			{
				viewQuotedBooking = context.Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_JS, booking.PK));
			}

			return viewQuotedBooking != null ? QuotedBooking.New(viewQuotedBooking.VB_TH, viewQuotedBooking.VB_JS, context.Factory) : null;
		}

		#endregion

		#region Import

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notify, BusinessObject bizObj)
		{
			// this notification will come from the shipment data adapter
		}

		protected override void ImportFromValueObjectCore(QuotedBooking quotedBooking, Xsd.ShipmentBooking shipmentBookingValue, IValueObjectImportContext context)
		{
			new ShipmentValueObjectDataAdapter<ForwardingShipment>().ImportFromValueObject(quotedBooking.Booking, shipmentBookingValue.Shipment, context);

			var dataImportSupporter = (ISupportDataImporting)quotedBooking.Booking;
			var previousIsImportingData = dataImportSupporter.IsImportingData;

			dataImportSupporter.IsImportingData = true;

			try
			{
				ImportShipmentBookingDetail(quotedBooking, shipmentBookingValue, context);
			}
			finally
			{
				dataImportSupporter.IsImportingData = previousIsImportingData;
			}
		}

		void ImportShipmentBookingDetail(QuotedBooking quotedBooking, Xsd.ShipmentBooking shipmentBookingValue, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(quotedBooking.LoadPortInfo, shipmentBookingValue.ShipmentBookingDetail.PortOfLoading.Port.Value, ForeignKeyType.PortNK, Res.GetString("d0168c86-13c9-4e5d-81ed-0b52c180ff3c", "Load Port"));
			context.SetPropertyInfoValue(quotedBooking.DischargePortInfo, shipmentBookingValue.ShipmentBookingDetail.PortOfDischarge.Port.Value, ForeignKeyType.PortNK, Res.GetString("ba87e05d-155d-46b9-8f31-b553bfa601fd", "Discharge Port"));

			context.SetPropertyInfoValue(quotedBooking.Booking.JS_CFSReferenceInfo, shipmentBookingValue.ShipmentBookingDetail.CFSReference, shipmentBookingValue.ShipmentBookingDetail.CFSReferenceSpecified);

			if (!shipmentBookingValue.ShipmentBookingDetail.ClientRequestedETA.IsEmpty)
			{
				quotedBooking.Booking.JS_ClientRequestedETA = shipmentBookingValue.ShipmentBookingDetail.ClientRequestedETA.ToSmallDateTime();
			}

			if (shipmentBookingValue.ShipmentBookingDetail.Carrier.IsSpecified)
			{
				quotedBooking.Booking.JS_OA_BookedShippingLineAddress = context.FindOrCreateTempOrganisation(shipmentBookingValue.ShipmentBookingDetail.Carrier, quotedBooking, OrganisationTypes.Carrier).MainAddress.PK;
			}
			Xsd.SailingBase sailingValue = shipmentBookingValue.ShipmentBookingDetail.Item;

			ZString vesselName = sailingValue != null ? sailingValue.GetVesselName(context) : ZString.Empty;
			ZString transportMode = TransportModeToXmlCodeMappings.Instance.GetEnterpriseCode(shipmentBookingValue.Shipment.ShipmentDetails.TransportMode, "", context);
			ZString voyage = XsdSailingBase.GetVoyage(sailingValue);

			bool otherOK = transportMode != Core.Constants.TransportModes.Sea && !voyage.IsEmpty;
			bool seaOK = transportMode == Core.Constants.TransportModes.Sea && !voyage.IsEmpty && !vesselName.IsEmpty;

			if (seaOK || otherOK)
			{
				var sailingWithVesselVoyage = sailingValue as Xsd.SailingWithVesselVoyage;

				ZGuid carrierPK = sailingWithVesselVoyage != null && sailingWithVesselVoyage.Carrier.IsSpecified
					? context.FindOrCreateTempOrganisationPK(sailingWithVesselVoyage.Carrier, quotedBooking, OrganisationTypes.Carrier)
					: ZGuid.Empty;

				SailingValueObjectDataAdapter sailingAdapter = new SailingValueObjectDataAdapter(
					transportMode,
					quotedBooking.ScheduleChooser.Sailing?.JX_JA_RL_NKPortOfLoading ?? quotedBooking.LoadPort,
					quotedBooking.ScheduleChooser.Sailing?.JX_JB_RL_NKPortOfDischarge ?? quotedBooking.DischargePort,
					vesselName,
					voyage,
					carrierPK);

				if (quotedBooking.ScheduleChooser.Sailing == null)
				{
					JobSailing sailing = sailingAdapter.CreateOrUpdateFromValueObjectWithSchedule(shipmentBookingValue.ShipmentBookingDetail.Item, context);
					if (sailing != null)
					{
						quotedBooking.Booking.JS_JX = sailing.PK;
					}
				}
				else
				{
					sailingAdapter.ImportFromValueObject(quotedBooking.ScheduleChooser.Sailing, shipmentBookingValue.ShipmentBookingDetail.Item, context);
				}
			}
			else
			{
				context.Notify(new WarningNotification(Res.GetString("09746dc6-8fe4-4999-9e01-deeaf6115831", "Not enough information provided to create or update sailing information.")));
			}

			if (shipmentBookingValue.ShipmentBookingDetail.IsDirectBookingSpecified)
			{
				quotedBooking.Booking.JS_IsDirectBooking = shipmentBookingValue.ShipmentBookingDetail.IsDirectBooking == Xsd.TrueFalse.@true;
			}

			ShipmentOrdersDataAdapterHelper ordersDataAdapterHelper = new ShipmentOrdersDataAdapterHelper(TriggeredByEvents);
			ordersDataAdapterHelper.ImportOrdersAndOrderReferences(new ShipmentOrdersDataAdapterHelper.ImportArgs()
			{
				Shipment = quotedBooking.Booking,
				ShipmentValue = shipmentBookingValue.Shipment,
				Context = context
			});

			ImportContainers(quotedBooking, shipmentBookingValue, context);

			context.SetPropertyInfoValueIfValueNotEmpty(quotedBooking.Booking.JS_HouseBillInfo, shipmentBookingValue.Shipment.Housebill);
		}

		void ImportContainers(QuotedBooking quotedBooking, Xsd.ShipmentBooking value, IValueObjectImportContext context)
		{
			if (value.ShipmentBookingDetail.Containers != null)
			{
				var adapter = new QuotedBookingContainerValueObjectDataAdapter<ForwardingContainer, Xsd.Container>(quotedBooking);

				foreach (Xsd.Container containerValue in value.ShipmentBookingDetail.Containers)
				{
					adapter.CreateOrUpdateFromValueObject(containerValue, context);
				}
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(QuotedBooking quotedBooking, Xsd.ShipmentBooking result, IValueObjectExportContext context)
		{
			if (quotedBooking.ObjectState == QuotedBookingState.QuoteOnly)
			{
				throw new NotSupportedException("Not available for Spot Quotes.");
			}

			result.Shipment = GetNewShipmentValueObjectDataAdapter(TriggeredByEvents).ExportToValueObject(quotedBooking.Booking, context);
			if (!ShouldPopulatedAgentReference())
			{
				result.Shipment.ShipmentDetails.AgentReference = "";
			}

			ExportShipmentBookingDetail(quotedBooking, result, context);
		}

		void ExportShipmentBookingDetail(QuotedBooking quotedBooking, Xsd.ShipmentBooking shipmentBookingValue, IValueObjectExportContext context)
		{
			shipmentBookingValue.ShipmentBookingDetail = new Xsd.ShipmentBookingShipmentBookingDetail();
			shipmentBookingValue.ShipmentBookingDetail.CFSReference = quotedBooking.Booking.JS_CFSReference;
			shipmentBookingValue.ShipmentBookingDetail.ClientRequestedETA = quotedBooking.Booking.JS_ClientRequestedETA;

			shipmentBookingValue.ShipmentBookingDetail.Carrier = new OrganisationValueObjectDataAdapter().ExportToValueObject(quotedBooking.Booking.BookedShippingLine, context);

			shipmentBookingValue.ShipmentBookingDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(quotedBooking.Factory, quotedBooking.LoadPort);
			shipmentBookingValue.ShipmentBookingDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(quotedBooking.Factory, quotedBooking.DischargePort);

			shipmentBookingValue.Shipment.ShipmentDetails.Custom.IsSpecified = false;

			if (quotedBooking.Booking.JS_IsDirectBooking)
			{
				shipmentBookingValue.ShipmentBookingDetail.IsDirectBooking = Xsd.TrueFalse.@true;
				shipmentBookingValue.ShipmentBookingDetail.IsDirectBookingSpecified = true;
			}

			PopulateSailing(quotedBooking, shipmentBookingValue, context);
			ExportContainers(quotedBooking, shipmentBookingValue, context);

			var ordersDataAdapterHelper = new ShipmentOrdersDataAdapterHelper(TriggeredByEvents);
			ordersDataAdapterHelper.ExportOrdersAndOrderReferences(new ShipmentOrdersDataAdapterHelper.ExportArgs()
			{
				Shipment = quotedBooking.Booking,
				ShipmentValue = shipmentBookingValue.Shipment,
				Context = context
			});
		}

		void ExportContainers(QuotedBooking quotedBooking, Xsd.ShipmentBooking value, IValueObjectExportContext context)
		{
			if (quotedBooking.QuotedBookingContainers.Count > 0)
			{
				var adapter = new QuotedBookingContainerValueObjectDataAdapter<ForwardingContainer, Xsd.Container>(quotedBooking);

				value.ShipmentBookingDetail.Containers = new Xsd.ContainerCollection();
				foreach (ForwardingContainer container in quotedBooking.QuotedBookingContainers)
				{
					value.ShipmentBookingDetail.Containers.Add(adapter.ExportToValueObject(container, context));
				}
			}
		}

		protected virtual ShipmentValueObjectDataAdapter<ForwardingShipment> GetNewShipmentValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			return new ShipmentValueObjectDataAdapter<ForwardingShipment>(triggeredByEvents);
		}

		void PopulateSailing(QuotedBooking quotedBooking, Xsd.ShipmentBooking shipmentValue, IValueObjectExportContext context)
		{
			Xsd.SailingBase sailingValue;
			if (quotedBooking.Booking.IsSea)
			{
				var sailingWithVesselVoyage = new Xsd.SailingWithVesselVoyage();
				sailingWithVesselVoyage.VesselName = quotedBooking.Booking.JS_Calc_CurrentVessel;
				sailingWithVesselVoyage.VoyageNo = quotedBooking.Booking.JS_Calc_CurrentVoyageFlight;

				var sailing = quotedBooking.ScheduleChooser.Sailing;
				if (sailing != null
					&& sailing.JX_JV_NKVessel == quotedBooking.Booking.JS_Calc_CurrentVessel
					&& sailing.JX_JV_VoyageFlight == quotedBooking.Booking.JS_Calc_CurrentVoyageFlight
					&& sailing.Line != null)
				{
					sailingWithVesselVoyage.Carrier = new OrganisationValueObjectDataAdapter().ExportToValueObject(quotedBooking.ScheduleChooser.Sailing.Line, context);
				}

				sailingValue = sailingWithVesselVoyage;
			}
			else
			{
				sailingValue = new Xsd.FlightWithFlightNumber();
				((Xsd.FlightWithFlightNumber)sailingValue).FlightNoJourneyNoTruckRegNo = quotedBooking.Booking.JS_Calc_CurrentVoyageFlight;
			}

			if ((!quotedBooking.ScheduleChooser.Sailing?.JX_JA_RL_NKPortOfLoading.IsEmpty ?? false) &&
				(!quotedBooking.ScheduleChooser.Sailing?.JX_JB_RL_NKPortOfDischarge.IsEmpty ?? false))
			{
				SailingValueObjectDataAdapter.RunExport(quotedBooking.Booking.Sailing, sailingValue, context);
				shipmentValue.ShipmentBookingDetail.Item = sailingValue;
			}
		}

		protected virtual bool ShouldPopulatedAgentReference()
		{
			return true;
		}

		#endregion

		#region Implementation

		protected override QuotedBooking NewBusinessObject(Xsd.ShipmentBooking value, IValueObjectImportContext context)
		{
			return QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, context.Factory);
		}

		protected ZDBOnlyQuery GetWithBookingQuery(SchemaColumn bookingColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			ZDBOnlySubQuery bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			bookingSubQuery.AddToFilter_PossiblyCommaSeparated(bookingColumn, comparisonOperator, value);

			viewFilter.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);
			return viewFilter;
		}

		#endregion
	}
}
