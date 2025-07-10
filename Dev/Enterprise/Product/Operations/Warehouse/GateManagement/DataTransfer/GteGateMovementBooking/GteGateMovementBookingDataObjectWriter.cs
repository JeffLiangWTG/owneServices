using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteGateMovementBookingDataObjectWriter : DataObjectWriter<GteGateMovementBooking, UniversalShipment>
	{
		public GteGateMovementBookingDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		#region Populate

		protected override UniversalShipment PopulateDataObject(GteGateMovementBooking gateMovementBooking)
		{
			var dataObject = new UniversalShipment(writeManager.WriterStrategy);
			dataObject.BookingConfirmationReference = gateMovementBooking.GBM_BookingReferenceNumber;

			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataSource(DataContextType.GateMovementBooking, gateMovementBooking.GBM_MovementBookingNumber);

			PopulateBookingSlotTimes(gateMovementBooking, dataObject);
			PopulatePackingLine(gateMovementBooking, dataObject);
			PopulateContainerCollection(gateMovementBooking, dataObject);
			PopulateTransportBookingDirection(gateMovementBooking, dataObject);
			PopulateAdditionalReferenceCollection(gateMovementBooking, dataObject);

			return dataObject;
		}

		void PopulateBookingSlotTimes(GteGateMovementBooking gateMovementBooking, UniversalShipment dataObject)
		{
			if (!gateMovementBooking.GBM_SlotStartTime.IsValid && !gateMovementBooking.GBM_SlotEndTime.IsValid)
			{
				return;
			}

			dataObject.SetDateCollection(() => new List<Date>());
			if (gateMovementBooking.GBM_SlotStartTime.IsValid)
			{
				dataObject.DateCollection.Add(Date.New(DateType.Start, ZBool.False, gateMovementBooking.GBM_SlotStartTime));
			}

			if (gateMovementBooking.GBM_SlotEndTime.IsValid)
			{
				dataObject.DateCollection.Add(Date.New(DateType.End, ZBool.False, gateMovementBooking.GBM_SlotEndTime));
			}
		}

		void PopulatePackingLine(GteGateMovementBooking gateMovementBooking, UniversalShipment dataObject)
		{
			dataObject.SetPackingLineCollection(() =>
			{
				var commodity = ListHelper.GetWithDescription<Commodity>(gateMovementBooking.GBM_RH_NKCargoType, gateMovementBooking.Lookups.CargoTypes);
				var packageType = ListHelper.GetWithDescription<PackageType>(gateMovementBooking.GBM_F3_NKPackageType, gateMovementBooking.Lookups.PackageTypes);

				var packingLine = new PackingLine();
				packingLine.PackType = new PackageType();
				packingLine.PackType.Code = packageType.Code;
				packingLine.PackType.Description = packageType.Description;
				packingLine.Commodity = commodity;
				packingLine.PackQty = ZLong.Parse(gateMovementBooking.GBM_Quantity.ToString());

				return new DataObjectList<PackingLine>() { packingLine };
			});
		}

		void PopulateContainerCollection(GteGateMovementBooking gateMovementBooking, UniversalShipment dataObject)
		{
			if (gateMovementBooking.UnitType != null && !string.IsNullOrEmpty(gateMovementBooking.GBM_UnitNumber))
			{
				dataObject.FacilityJobType = new CodeDescriptionPair()
				{
					Code = FacilityJobType.Codes.Container,
					Description = FacilityJobType.Descriptions.Container
				};
			}
			else
			{
				dataObject.FacilityJobType = new CodeDescriptionPair()
				{
					Code = FacilityJobType.Codes.Cargo,
					Description = FacilityJobType.Descriptions.Cargo
				};
			}

			var container = new Container();
			container.ContainerNumber = gateMovementBooking.GBM_UnitNumber;
			container.ContainerType = new ContainerType();

			var refContainer = gateMovementBooking.Factory.Load<RefContainer>(gateMovementBooking.GBM_RC_UnitType);
			if (refContainer != null)
			{
				container.ContainerType.Code = refContainer.RC_Code;
			}

			dataObject.SetContainerCollection(() => new DataObjectList<Container>() { container });
			dataObject.ContainerCollection.Content = CollectionContent.Complete;
		}

		void PopulateTransportBookingDirection(GteGateMovementBooking gateMovementBooking, UniversalShipment dataObject)
		{
			if (gateMovementBooking.GBM_IsPickup)
			{
				dataObject.TransportBookingDirection = new TransportBookingDirection()
				{
					Code = GateManagementConstants.TransportBookingDirections.Codes.Pickup,
					Description = GateManagementConstants.TransportBookingDirections.Descriptions.Pickup,
				};
			}
			else
			{
				dataObject.TransportBookingDirection = new TransportBookingDirection()
				{
					Code = GateManagementConstants.TransportBookingDirections.Codes.Delivery,
					Description = GateManagementConstants.TransportBookingDirections.Descriptions.Delivery,
				};
			}
		}

		void PopulateAdditionalReferenceCollection(GteGateMovementBooking gateMovementBooking, UniversalShipment dataObject)
		{
			if (!gateMovementBooking.GBM_SourceReferenceNumber.IsDefault)
			{
				dataObject.AddAdditionalReference(
					AdditionalReferenceTypes.Codes.BookingPartyReference,
					AdditionalReferenceTypes.Descriptions.BookingPartyReference,
					gateMovementBooking.GBM_SourceReferenceNumber);
			}

			if (!gateMovementBooking.GBM_TransportReference.IsEmpty)
			{
				dataObject.AddAdditionalReference(
					AdditionalReferenceTypes.Codes.TransportReference,
					AdditionalReferenceTypes.Descriptions.TransportReference,
					gateMovementBooking.GBM_TransportReference);
			}
		}

		#endregion Populate
	}
}
