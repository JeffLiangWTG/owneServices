using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyUniversalTestHelper
	{
		#region CreateReferences
		public static AgencyShipmentReferences CreateReferences(ZString oceanBill, SailingReference sailingReference)
		{
			return CreateReferences(oceanBill, ZString.Empty, sailingReference, o => new AgencyShipmentReferences(o));
		}

		public static AgencyShipmentReferences CreateReferences(ZString oceanBill, ZString carriersBookingReference, SailingReference sailingReference)
		{
			return CreateReferences(oceanBill, carriersBookingReference, sailingReference, o => new AgencyShipmentReferences(o));
		}

		public static AgencyShipmentReferences CreateReferences(ZString oceanBill, SailingReference[] sailingReferences)
		{
			return CreateReferences(oceanBill, ZString.Empty, ZString.Empty, ZString.Empty, null, sailingReferences, o => new AgencyShipmentReferences(o, ZGuid.Empty, ZString.Empty));
		}

		public static AgencyShipmentReferences CreateReferences(ZString oceanBill, ZString carriersBookingReference, ZString agentsReference, IOrgHeader bookingParty, ZString bookingPartyName, SailingReference[] sailingReferences, string purpose = null)
		{
			var bookingPartyPk = bookingParty?.PK ?? ZGuid.Empty;
			return CreateReferences(oceanBill, carriersBookingReference, agentsReference, ZString.Empty, bookingParty, sailingReferences, o => new AgencyShipmentReferences(o, bookingPartyPk, bookingPartyName), purpose);
		}

		public static AgencyShipmentReferences CreateReferences(ZString oceanBill, ZString carriersBookingReference, ZString containerNumber, ZBool isCarrierVGM)
		{
			return CreateReferences(oceanBill, carriersBookingReference, ZString.Empty, containerNumber, null, null, o => new AgencyShipmentReferences(o, ZGuid.Empty, ZString.Empty), null, isCarrierVGM);
		}

		public static T CreateReferences<T>(ZString oceanBill, SailingReference sailingReference, Func<IXmlEventValueObjectContextValueList, T> createNew)
			where T : AgencyShipmentReferences
		{
			return CreateReferences(oceanBill, ZString.Empty, sailingReference, createNew);
		}

		public static T CreateReferences<T>(ZString oceanBill, ZString carriersBookingReference, SailingReference sailingReference, Func<IXmlEventValueObjectContextValueList, T> createNew)
			where T : AgencyShipmentReferences
		{
			var mockContextList = new Mock<IXmlEventValueObjectContextValueList>();
			mockContextList.Setup(context => context.MBOLNumber).Returns(oceanBill);
			mockContextList.Setup(context => context.CarriersBookingReference).Returns(carriersBookingReference);
			mockContextList.Setup(context => context.LloydsNumber).Returns(sailingReference.LloydsNumber);
			mockContextList.Setup(context => context.VesselName).Returns(sailingReference.VesselName);
			mockContextList.Setup(context => context.VoyageNumber).Returns(sailingReference.VoyageNumber);
			mockContextList.Setup(context => context.LegOriginUNLOCO).Returns(sailingReference.PortOfLoading);
			mockContextList.Setup(context => context.LegDestinationUNLOCO).Returns(sailingReference.PortOfDischarge);
			return createNew(mockContextList.Object);
		}

		public static T CreateReferences<T>(ZString oceanBill, ZString carriersBookingReference, ZString agentsReference, ZString containerNumber, IOrgHeader bookingParty, SailingReference[] sailingReferences, Func<UniversalShipment, T> createNew, string purpose = null
			, bool isCarrierVGM = false)
			where T : AgencyShipmentReferences
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{ WayBillNumber = oceanBill, AgentsReference = agentsReference, BookingConfirmationReference = carriersBookingReference };
			if (sailingReferences != null && sailingReferences.Length > 0)
			{
				dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(sailingReferences.Select(GetTransportLeg)));
			}

			if (!containerNumber.IsEmpty)
			{
				dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = containerNumber } });
			}

			if (bookingParty != null)
			{
				var bookingPartyOrgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{ AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress), CompanyName = bookingParty.OH_FullName, OrganizationCode = bookingParty.OH_Code, Address1 = "Booking Party Address 1" };
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
				{ bookingPartyOrgAddress });
			}

			dataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			if (!string.IsNullOrEmpty(purpose))
			{
				dataObject.DataContext.SetDocumentaryOverride("", purpose, null, true, 1, 1);
			}

			if (isCarrierVGM)
			{
				dataObject.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAR, ServiceCode = ServiceCodeType.VGM } };
			}

			return createNew(dataObject);
		}

		static TransportLeg GetTransportLeg(SailingReference sailingReference)
		{
			var transportLeg = new TransportLeg { TransportMode = TransportMode.Sea, VesselName = sailingReference.VesselName, VoyageFlightNo = sailingReference.VoyageNumber };
			return transportLeg;
		}
		#endregion
	}
}
