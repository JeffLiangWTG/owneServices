using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	class AgencyShipmentCarrierVGMTargetHelperTest : TestCaseWithFactory
	{
		public void TestAlwaysTrueWhenIsNotValidCarrierVGM()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAR, ServiceCode = ServiceCodeType.VGM } }, null));

				Assert(AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsBillOfLading(new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAR, ServiceCode = ServiceCodeType.VGM } }, null));
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAP, ServiceCode = ServiceCodeType.VGM } }, null));
				Assert(AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAR, ServiceCode = ServiceCodeType.BRQ } }, null));

				Assert(AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsBillOfLading(new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAP, ServiceCode = ServiceCodeType.VGM } }, null));
				Assert(AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsBillOfLading(new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAR, ServiceCode = ServiceCodeType.BRQ } }, null));
			}
		}

		public void TestIsProcessingAsBillOfLading()
		{
			var recipientRoles = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAR, ServiceCode = ServiceCodeType.VGM, } };

			var logger = new TestErrorLogger();
			logger.TopLevelDataObject = CreateUniversalShipment("C00001194", "C00001195", "C00000001");

			var bol = CreateBillOfLading("C00001194", "C00001195", "C00000001");
			Factory.Save();

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("Matching bill, booking number and Real container", AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsBillOfLading(recipientRoles, logger));

				bol.JS_CFSReference = "C00001196";
				Factory.Save();
				Assert("Matching BOL - bill number and Real container", AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsBillOfLading(recipientRoles, logger));

				bol.JS_HouseBill = "C00001195";
				bol.JS_CFSReference = "C00001195";
				Factory.Save();
				Assert("Matching BOL - booking number and Real container", AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsBillOfLading(recipientRoles, logger));

				bol.JS_HouseBill = "C00001196";
				bol.JS_CFSReference = "C00001196";
				Factory.Save();
				Assert("Cannot match BOL - no WayBillNumber and BookingConfirmationReference", !AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsBillOfLading(recipientRoles, logger));

				bol.JS_HouseBill = "C00001194";
				bol.JS_CFSReference = "C00001195";
				bol.RealContainers.OfType<BillOfLadingContainer>().First().JC_ContainerNum = "C00000002";
				Factory.Save();
				Assert("Cannot match BOL - no Real container", !AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsBillOfLading(recipientRoles, logger));

				var bookedContainer = bol.BookedContainers.AddNew();
				bookedContainer.JC_ContainerNum = "C00000001";

				Factory.Save();
				Assert("Cannot match BOL - no Real container", !AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsBillOfLading(recipientRoles, logger));
			}
		}

		public void TestIsProcessingAsAgencyBooking()
		{
			var recipientRoles = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CAR, ServiceCode = ServiceCodeType.VGM, } };

			var logger = new TestErrorLogger();
			var universalShipment = CreateUniversalShipment("C00001194", "C00001195", "C00000001");
			logger.TopLevelDataObject = universalShipment;

			var bol = CreateBillOfLading("C00001194", "C00001195", "C00000001");
			Factory.Save();

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("Matching BOL - bill, booking number and container", !AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(recipientRoles, logger));

				bol.JS_CFSReference = "C00001196";
				Factory.Save();
				Assert("Matching BOL - bill number and container", !AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(recipientRoles, logger));

				bol.JS_HouseBill = "C00001195";
				bol.JS_CFSReference = "C00001195";
				Factory.Save();
				Assert("Matching BOL - booking number and container", !AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(recipientRoles, logger));

				bol.JS_HouseBill = "C00001196";
				bol.JS_CFSReference = "C00001196";
				Factory.Save();
				Assert("Cannot match BOL", AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(recipientRoles, logger));

				bol.JS_HouseBill = "C00001195";
				bol.JS_CFSReference = "C00001195";
				bol.RealContainers.OfType<BillOfLadingContainer>().First().JC_ContainerNum = "C00000002";
				Factory.Save();
				Assert("Cannot match BOL", AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(recipientRoles, logger));

				var bookedContainer = bol.BookedContainers.AddNew();
				bookedContainer.JC_ContainerNum = "C00000001";
				Factory.Save();
				Assert("Cannot match BOL - no Real container", AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(recipientRoles, logger));

				bol.RealContainers.OfType<BillOfLadingContainer>().First().JC_ContainerNum = "C00000001";
				Factory.Save();
				Assert("Matching BOL - booking number and container", !AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(recipientRoles, logger));

				universalShipment.WayBillNumber = ZString.Empty;
				Assert("Matching BOL - booking number and container", !AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(recipientRoles, logger));

				universalShipment.BookingConfirmationReference = ZString.Empty;
				Assert("WayBillNumber and BookingConfirmationReference are empty, Unable to match any valid BillOfLading", AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(recipientRoles, logger));

				universalShipment.WayBillNumber = "C00001194";
				universalShipment.BookingConfirmationReference = "C00001195";
				universalShipment.ContainerCollection.First().ContainerNumber = ZString.Empty;

				universalShipment.BookingConfirmationReference = ZString.Empty;
				Assert("ContainerNumber is empty, Unable to match any valid BillOfLading", AgencyShipmentCarrierVGMTargetHelper.IsProcessingAsAgencyBooking(recipientRoles, logger));
			}
		}

		BillOfLading CreateBillOfLading(ZString wayBillNumber, ZString bookingConfirmationReference, ZString containerNumber)
		{
			var bol = Factory.NewWithValidTestData<BillOfLading>();
			bol.JS_HouseBill = wayBillNumber;
			bol.JS_CFSReference = bookingConfirmationReference;
			bol.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;

			var container = bol.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;

			return bol;
		}

		Shipment CreateUniversalShipment(ZString wayBillNumber, ZString bookingConfirmationReference, ZString containerNumber)
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();

			universalShipment.WayBillNumber = wayBillNumber;
			universalShipment.BookingConfirmationReference = bookingConfirmationReference;

			universalShipment.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container
				{
					ContainerNumber = containerNumber
				}
			});

			return universalShipment;
		}
	}
}
