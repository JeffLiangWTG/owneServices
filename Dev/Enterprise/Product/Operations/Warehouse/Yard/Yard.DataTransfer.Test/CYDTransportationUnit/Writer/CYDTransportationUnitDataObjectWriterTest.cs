using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(CYDTransportationUnitDataObjectWriter))]
	public class CYDTransportationUnitDataObjectWriterTest : TestCaseWithUniversalObjectFactory
	{
		public void TestWriteToDataObjectForDelivery()
		{
			SetUpForDelivery();

			var transportationUnit = Factory.LoadTop1<CYDTransportationUnit>(new ZQuery(CYDTransportationUnitSchema.YTU_TransportationReference, "V051502"));
			var shipment = new CYDTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CYD, transportationUnit))).GetDataObject(transportationUnit);

			var dataSource = shipment.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDTransportationUnit", dataSource.Type);
			AssertEquals(transportationUnit.YTU_TransportationUnitID, dataSource.Key);

			var preCarriageShipment = shipment.PreCarriageShipmentCollection.Single();
			AssertEquals("V051502", preCarriageShipment.VehicleRun.Vehicle.Registration.Number);

			AssertEquals(2, shipment.OrganizationAddressCollection.Count);
			AssertEquals("TransportCompanyDocumentaryAddress", shipment.OrganizationAddressCollection[0].AddressType);
			AssertEquals("ACE", shipment.OrganizationAddressCollection[0].OrganizationCode);
			AssertEquals("LocalCartageYard", shipment.OrganizationAddressCollection[1].AddressType);
			AssertEquals("WUFSHIJNB", shipment.OrganizationAddressCollection[1].OrganizationCode);

			AssertEquals(3, shipment.SubShipmentCollection.Count);

			var orderedSubShipments = shipment.SubShipmentCollection.OrderBy(s => s.ContainerCollection[0].ContainerNumber).ToList();

			var subShipment1 = orderedSubShipments[0];
			var subShipment1DataSource = subShipment1.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDDelivery", subShipment1DataSource.Type);
			Assert(subShipment1DataSource.Key.HasValue);
			AssertEquals("BKR01", subShipment1.BookingConfirmationReference);
			AssertEquals("DLV", subShipment1.TransportBookingDirection.Code);
			AssertEquals("Delivery", subShipment1.TransportBookingDirection.Description);
			AssertEquals(1, subShipment1.ContainerCollection.Count);
			AssertEquals("20GP", subShipment1.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL", subShipment1.ContainerCollection[0].ContainerNumber);

			var subShipment2 = orderedSubShipments[1];
			var subShipment2DataSource = subShipment2.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDDelivery", subShipment2DataSource.Type);
			Assert(subShipment2DataSource.Key.HasValue);
			AssertEquals("BKR01", subShipment2.BookingConfirmationReference);
			AssertEquals("DLV", subShipment2.TransportBookingDirection.Code);
			AssertEquals("Delivery", subShipment2.TransportBookingDirection.Description);
			AssertEquals(1, subShipment2.ContainerCollection.Count);
			AssertEquals("20GP", subShipment2.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL2", subShipment2.ContainerCollection[0].ContainerNumber);

			var subShipment3 = orderedSubShipments[2];
			var subShipment3DataSource = subShipment3.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDDelivery", subShipment3DataSource.Type);
			Assert(subShipment3DataSource.Key.HasValue);
			AssertEquals("BKR01", subShipment3.BookingConfirmationReference);
			AssertEquals("DLV", subShipment3.TransportBookingDirection.Code);
			AssertEquals("Delivery", subShipment3.TransportBookingDirection.Description);
			AssertEquals(1, subShipment3.ContainerCollection.Count);
			AssertEquals("20GP", subShipment3.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL3", subShipment3.ContainerCollection[0].ContainerNumber);
		}

		public void TestWriteToDataObjectForDelivery_MultiplePRA()
		{
			SetUpForDelivery_MultiplePRA();

			var transportationUnit = Factory.LoadTop1<CYDTransportationUnit>(new ZQuery(CYDTransportationUnitSchema.YTU_TransportationReference, "V051502"));
			var shipment = new CYDTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CYD, transportationUnit))).GetDataObject(transportationUnit);

			var dataSource = shipment.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDTransportationUnit", dataSource.Type);
			AssertEquals(transportationUnit.YTU_TransportationUnitID, dataSource.Key);

			var preCarriageShipment = shipment.PreCarriageShipmentCollection.Single();
			AssertEquals("V051502", preCarriageShipment.VehicleRun.Vehicle.Registration.Number);

			AssertEquals(2, shipment.OrganizationAddressCollection.Count);
			AssertEquals("TransportCompanyDocumentaryAddress", shipment.OrganizationAddressCollection[0].AddressType);
			AssertEquals("ACE", shipment.OrganizationAddressCollection[0].OrganizationCode);
			AssertEquals("LocalCartageYard", shipment.OrganizationAddressCollection[1].AddressType);
			AssertEquals("WUFSHIJNB", shipment.OrganizationAddressCollection[1].OrganizationCode);

			AssertEquals(3, shipment.SubShipmentCollection.Count);

			var orderedSubShipments = shipment.SubShipmentCollection.OrderBy(s => s.ContainerCollection[0].ContainerNumber).ToList();

			var subShipment1 = orderedSubShipments[0];
			var subShipment1DataSource = subShipment1.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDDelivery", subShipment1DataSource.Type);
			Assert(subShipment1DataSource.Key.HasValue);
			AssertEquals("BKR01", subShipment1.BookingConfirmationReference);
			AssertEquals("DLV", subShipment1.TransportBookingDirection.Code);
			AssertEquals("Delivery", subShipment1.TransportBookingDirection.Description);
			AssertEquals(1, subShipment1.ContainerCollection.Count);
			AssertEquals("20GP", subShipment1.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL", subShipment1.ContainerCollection[0].ContainerNumber);

			var subShipment2 = orderedSubShipments[1];
			var subShipment2DataSource = subShipment2.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDDelivery", subShipment2DataSource.Type);
			Assert(subShipment2DataSource.Key.HasValue);
			AssertEquals("BKR03", subShipment2.BookingConfirmationReference);
			AssertEquals("DLV", subShipment2.TransportBookingDirection.Code);
			AssertEquals("Delivery", subShipment2.TransportBookingDirection.Description);
			AssertEquals(1, subShipment2.ContainerCollection.Count);
			AssertEquals("20GP", subShipment2.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL2", subShipment2.ContainerCollection[0].ContainerNumber);

			var subShipment3 = orderedSubShipments[2];
			var subShipment3DataSource = subShipment3.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDDelivery", subShipment3DataSource.Type);
			Assert(subShipment3DataSource.Key.HasValue);
			AssertEquals("BKR01", subShipment3.BookingConfirmationReference);
			AssertEquals("DLV", subShipment3.TransportBookingDirection.Code);
			AssertEquals("Delivery", subShipment3.TransportBookingDirection.Description);
			AssertEquals(1, subShipment3.ContainerCollection.Count);
			AssertEquals("20GP", subShipment3.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL3", subShipment3.ContainerCollection[0].ContainerNumber);
		}

		public void TestWriteToDataObjectForPickup()
		{
			SetUpForPickup();

			var transportationUnit = Factory.LoadTop1<CYDTransportationUnit>(new ZQuery(CYDTransportationUnitSchema.YTU_TransportationReference, "V051502"));
			var shipment = new CYDTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CYD, transportationUnit))).GetDataObject(transportationUnit);

			var dataSource = shipment.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDTransportationUnit", dataSource.Type);
			AssertEquals(transportationUnit.YTU_TransportationUnitID, dataSource.Key);

			var preCarriageShipment = shipment.PreCarriageShipmentCollection.Single();
			AssertEquals("V051502", preCarriageShipment.VehicleRun.Vehicle.Registration.Number);

			AssertEquals(2, shipment.OrganizationAddressCollection.Count);
			AssertEquals("TransportCompanyDocumentaryAddress", shipment.OrganizationAddressCollection[0].AddressType);
			AssertEquals("ACE", shipment.OrganizationAddressCollection[0].OrganizationCode);
			AssertEquals("LocalCartageYard", shipment.OrganizationAddressCollection[1].AddressType);
			AssertEquals("WUFSHIJNB", shipment.OrganizationAddressCollection[1].OrganizationCode);

			var orderedSubShipments = shipment.SubShipmentCollection.OrderBy(s => s.ContainerCollection[0].ContainerNumber).ToList();

			var subShipment1 = orderedSubShipments[0];
			var subShipment1DataSource = subShipment1.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDPickup", subShipment1DataSource.Type);
			Assert(subShipment1DataSource.Key.HasValue);
			AssertEquals("BKR02", subShipment1.BookingConfirmationReference);
			AssertEquals("PIC", subShipment1.TransportBookingDirection.Code);
			AssertEquals("Pickup", subShipment1.TransportBookingDirection.Description);
			AssertEquals(1, subShipment1.ContainerCollection.Count);
			AssertEquals("20GP", subShipment1.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL", subShipment1.ContainerCollection[0].ContainerNumber);

			var subShipment2 = orderedSubShipments[1];
			var subShipment2DataSource = subShipment2.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDPickup", subShipment2DataSource.Type);
			Assert(subShipment2DataSource.Key.HasValue);
			AssertEquals("BKR02", subShipment2.BookingConfirmationReference);
			AssertEquals("PIC", subShipment2.TransportBookingDirection.Code);
			AssertEquals("Pickup", subShipment2.TransportBookingDirection.Description);
			AssertEquals(1, subShipment2.ContainerCollection.Count);
			AssertEquals("20GP", subShipment2.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL2", subShipment2.ContainerCollection[0].ContainerNumber);

			var subShipment3 = orderedSubShipments[2];
			var subShipment3DataSource = subShipment3.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDPickup", subShipment3DataSource.Type);
			Assert(subShipment3DataSource.Key.HasValue);
			AssertEquals("BKR02", subShipment3.BookingConfirmationReference);
			AssertEquals("PIC", subShipment3.TransportBookingDirection.Code);
			AssertEquals("Pickup", subShipment3.TransportBookingDirection.Description);
			AssertEquals(1, subShipment3.ContainerCollection.Count);
			AssertEquals("20GP", subShipment3.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL3", subShipment3.ContainerCollection[0].ContainerNumber);
		}

		public void TestWriteToDataObjectForPickup_MultipleRO()
		{
			SetUpForPickup_MultipleRO();

			var transportationUnit = Factory.LoadTop1<CYDTransportationUnit>(new ZQuery(CYDTransportationUnitSchema.YTU_TransportationReference, "V051502"));
			var shipment = new CYDTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CYD, transportationUnit))).GetDataObject(transportationUnit);

			var dataSource = shipment.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDTransportationUnit", dataSource.Type);
			AssertEquals(transportationUnit.YTU_TransportationUnitID, dataSource.Key);

			var preCarriageShipment = shipment.PreCarriageShipmentCollection.Single();
			AssertEquals("V051502", preCarriageShipment.VehicleRun.Vehicle.Registration.Number);

			AssertEquals(2, shipment.OrganizationAddressCollection.Count);
			AssertEquals("TransportCompanyDocumentaryAddress", shipment.OrganizationAddressCollection[0].AddressType);
			AssertEquals("ACE", shipment.OrganizationAddressCollection[0].OrganizationCode);
			AssertEquals("LocalCartageYard", shipment.OrganizationAddressCollection[1].AddressType);
			AssertEquals("WUFSHIJNB", shipment.OrganizationAddressCollection[1].OrganizationCode);

			var orderedSubShipments = shipment.SubShipmentCollection.OrderBy(s => s.ContainerCollection[0].ContainerNumber).ToList();

			var subShipment1 = orderedSubShipments[0];
			var subShipment1DataSource = subShipment1.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDPickup", subShipment1DataSource.Type);
			Assert(subShipment1DataSource.Key.HasValue);
			AssertEquals("BKR02", subShipment1.BookingConfirmationReference);
			AssertEquals("PIC", subShipment1.TransportBookingDirection.Code);
			AssertEquals("Pickup", subShipment1.TransportBookingDirection.Description);
			AssertEquals(1, subShipment1.ContainerCollection.Count);
			AssertEquals("20GP", subShipment1.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL", subShipment1.ContainerCollection[0].ContainerNumber);

			var subShipment2 = orderedSubShipments[1];
			var subShipment2DataSource = subShipment2.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDPickup", subShipment2DataSource.Type);
			Assert(subShipment2DataSource.Key.HasValue);
			AssertEquals("BKR04", subShipment2.BookingConfirmationReference);
			AssertEquals("PIC", subShipment2.TransportBookingDirection.Code);
			AssertEquals("Pickup", subShipment2.TransportBookingDirection.Description);
			AssertEquals(1, subShipment2.ContainerCollection.Count);
			AssertEquals("20GP", subShipment2.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL2", subShipment2.ContainerCollection[0].ContainerNumber);

			var subShipment3 = orderedSubShipments[2];
			var subShipment3DataSource = subShipment3.DataContext.DataSourceCollection.Single();
			AssertEquals("CYDPickup", subShipment3DataSource.Type);
			Assert(subShipment3DataSource.Key.HasValue);
			AssertEquals("BKR02", subShipment3.BookingConfirmationReference);
			AssertEquals("PIC", subShipment3.TransportBookingDirection.Code);
			AssertEquals("Pickup", subShipment3.TransportBookingDirection.Description);
			AssertEquals(1, subShipment3.ContainerCollection.Count);
			AssertEquals("20GP", subShipment3.ContainerCollection[0].ContainerType.Code);
			AssertEquals("GENL3", subShipment3.ContainerCollection[0].ContainerNumber);
		}

		UniversalTestData Data;
		static readonly string[] containerNumbers = new string[] { "GENL", "GENL2", "GENL3" };

		protected override void SetUp()
		{
			base.SetUp();
			Data = new UniversalTestData(Factory, new TestErrorLogger());
		}

		void SetUpForDelivery()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			Data.SetupDataForTransportationUnitDataObjectWriterForDelivery("BKR01", today, tomorrow, "ACE", "PO BOX 201", "V051502", "TREF008", "20GP", containerNumbers);
		}

		void SetUpForDelivery_MultiplePRA()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			Data.SetupDataForTransportationUnitDataObjectWriterForDelivery_MultiplePRA("BKR01", "BKR03", today, tomorrow, "ACE", "PO BOX 201", "V051502", "TREF008", "20GP", containerNumbers);
		}

		void SetUpForPickup()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			Data.SetupDataForTransportationUnitDataObjectWriterForPickup("BKR02", today, tomorrow, "ACE", "PO BOX 201", "V051502", "TREF008", "20GP", containerNumbers);
		}

		void SetUpForPickup_MultipleRO()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			Data.SetupDataForTransportationUnitDataObjectWriterForPickup_MultipleRO("BKR02", "BKR04", today, tomorrow, "ACE", "PO BOX 201", "V051502", "TREF008", "20GP", containerNumbers);
		}
	}
}
