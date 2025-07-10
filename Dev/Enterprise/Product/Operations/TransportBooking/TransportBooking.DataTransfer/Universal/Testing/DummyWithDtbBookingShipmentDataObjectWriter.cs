#if DEBUG

using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	class DummyWithDtbBookingShipmentDataObjectWriter : DummyWithWorkflowShipmentDataObjectWriter
	{
		public DummyWithDtbBookingShipmentDataObjectWriter(bool isFCL, bool populateActualDates)
			: base()
		{
			this.IsFCL = isFCL;
			this.PopulateActualDates = populateActualDates;
		}
		readonly bool IsFCL;
		readonly bool PopulateActualDates;

		public override ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, sourceBO));

			var day = ZDateTime.Today;
			var oneDay = new TimeSpan(1, 0, 0, 0);

			var transportOrg = Helper.CreateOrLoadOrganisation("SYDTCO");
			var cfsOrg = Helper.CreateOrLoadOrganisation("SYDCFS");
			var cydOrg = Helper.CreateOrLoadOrganisation("SYDCYD");
			var cneOrg = Helper.CreateOrLoadOrganisation("SYDCNE");
			var cnrOrg = Helper.CreateOrLoadOrganisation("SYDCNR");
			transportOrg.MainAddress.OA_CompanyNameOverride = "Company123";
			cfsOrg.MainAddress.OA_CompanyNameOverride = "Hello";
			cydOrg.MainAddress.OA_CompanyNameOverride = "cyd Company";
			cneOrg.MainAddress.OA_CompanyNameOverride = "cne Company";
			cnrOrg.MainAddress.OA_CompanyNameOverride = "cnr Company";
			cfsOrg.MainAddress.OA_FCLEquipmentNeeded = "SDL";
			cydOrg.MainAddress.OA_FCLEquipmentNeeded = "SDL";
			cneOrg.MainAddress.OA_FCLEquipmentNeeded = "SDL";
			cnrOrg.MainAddress.OA_FCLEquipmentNeeded = "SDL";

			cfsOrg.OH_IsMiscFreightServices = true;
			cfsOrg.OH_IsPackDepot = true;

			Helper.Factory.Save();

			var ref20GP = Helper.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var sourceDescription = ((DummyWithDtbBooking)sourceBO).Z0_Description;
			if (sourceDescription == "DUM123" || sourceDescription == "Default")
			{
				sourceDescription = "DUM456";
			}

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");
			consol.DataContext.AddDataSource(DataContextType.DummyBusinessObject, sourceDescription);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, sourceDescription);

			if (IsFCL)
			{
				shipment.ContainerMode = new ContainerMode() { Code = "FCL" };
			}

			shipment.ServiceLevel = new ServiceLevel() { Code = "456" };
			shipment.CarrierServiceLevel = new ServiceLevel() { Code = "123" };

			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.SubShipmentCollection.Add(shipment);

			shipment.AddOrgAddress(writeManager, cfsOrg.MainAddress, DocAddressType.LocalCartageCFS);
			shipment.AddOrgAddress(writeManager, transportOrg.MainAddress, AddressTypes.PickupLocalCartage);
			shipment.AddOrgAddress(writeManager, transportOrg.MainAddress, AddressTypes.DeliveryLocalCartage);

			shipment.AddOrgAddress(writeManager, cydOrg, DocAddressType.ArrivalCYDAddress);
			shipment.AddOrgAddress(writeManager, cneOrg, DocAddressType.ConsigneeDocumentaryAddress);
			shipment.AddOrgAddress(writeManager, cnrOrg, DocAddressType.ConsignorDocumentaryAddress);

			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var containerIgnore = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);
			consol.ContainerCollection.Add(containerIgnore);

			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(container1); // add to Shipment too
			shipment.ContainerCollection.Add(container2); // add to Shipment too

			container1.ContainerNumber = "CONT123";
			container2.ContainerNumber = "CONT456";
			containerIgnore.ContainerNumber = "CONTIGNORE";
			container1.ContainerType = ContainerType.New(ref20GP);
			container2.ContainerType = ContainerType.New(ref20GP);
			containerIgnore.ContainerType = ContainerType.New(ref20GP);

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);

			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			var firstLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			var secondLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			consol.TransportLegCollection.Add(firstLeg);
			consol.TransportLegCollection.Add(secondLeg);

			firstLeg.LegOrder = 0;
			secondLeg.LegOrder = 1;

			var data = TransportBookingTestCache.Instance;

			// pickup

			data.AddData("firstLeg.DocumentCutOff", firstLeg.DocumentCutOff = day += oneDay);
			data.AddData("firstLeg.LCLReceivalCommences", firstLeg.LCLReceivalCommences = day += oneDay); // CFS -> Dlv -> Req. From
			data.AddData("firstLeg.LCLCutOff", firstLeg.LCLCutOff = day += oneDay); // CFS -> Dlv -> Req. To
			data.AddData("firstLeg.FCLReceivalCommences", firstLeg.FCLReceivalCommences = day += oneDay); // CTO -> Dlv -> Req. From
			data.AddData("firstLeg.FCLCutOff", firstLeg.FCLCutOff = day += oneDay); // CTO -> Dlv -> Req. To
			data.AddData("firstLeg.EstimatedDeparture", firstLeg.EstimatedDeparture = day += oneDay);
			data.AddData("firstLeg.EstimatedArrival", firstLeg.EstimatedArrival = day += oneDay);
			data.AddData("firstLeg.FCLAvailability", firstLeg.FCLAvailability = day += oneDay);
			data.AddData("firstLeg.FCLStorage", firstLeg.FCLStorage = day += oneDay);
			data.AddData("firstLeg.LCLAvailability", firstLeg.LCLAvailability = day += oneDay);
			data.AddData("firstLeg.LCLStorageDate", firstLeg.LCLStorageDate = day += oneDay);

			// delivery

			data.AddData("secondLeg.DocumentCutOff", secondLeg.DocumentCutOff = day += oneDay);
			data.AddData("secondLeg.LCLReceivalCommences", secondLeg.LCLReceivalCommences = day += oneDay);
			data.AddData("secondLeg.LCLCutOff", secondLeg.LCLCutOff = day += oneDay);
			data.AddData("secondLeg.FCLReceivalCommences", secondLeg.FCLReceivalCommences = day += oneDay);
			data.AddData("secondLeg.FCLCutOff", secondLeg.FCLCutOff = day += oneDay);
			data.AddData("secondLeg.EstimatedDeparture", secondLeg.EstimatedDeparture = day += oneDay);
			data.AddData("secondLeg.EstimatedArrival", secondLeg.EstimatedArrival = day += oneDay);
			data.AddData("secondLeg.FCLAvailability", secondLeg.FCLAvailability = day += oneDay);
			data.AddData("secondLeg.FCLStorage", secondLeg.FCLStorage = day += oneDay);
			data.AddData("secondLeg.LCLAvailability", secondLeg.LCLAvailability = day += oneDay);
			data.AddData("secondLeg.LCLStorageDate", secondLeg.LCLStorageDate = day += oneDay);

			var localProcessing = shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);

			// pickup
			localProcessing.FCLPickupEquipmentNeeded = new CodeDescriptionPair() { Code = "N12" };
			data.AddData("localProcessing.EstimatedPickup", localProcessing.EstimatedPickup = day += oneDay); // CNR -> Est
			data.AddData("localProcessing.PickupRequiredBy", localProcessing.PickupRequiredBy = day += oneDay); // CNR -> Req. To
			if (PopulateActualDates)
			{
				data.AddData("localProcessing.PickupCartageCompleted", localProcessing.PickupCartageCompleted = day += oneDay); // CNR -> Act
			}

			// delivery
			localProcessing.ArrivalCartageRef = "Trans Ref";
			localProcessing.FCLDeliveryEquipmentNeeded = new CodeDescriptionPair() { Code = "N34" };
			data.AddData("localProcessing.EstimatedDelivery", localProcessing.EstimatedDelivery = day += oneDay); // CNE -> Est
			data.AddData("localProcessing.DeliveryRequiredBy", localProcessing.DeliveryRequiredBy = day += oneDay); // CNE -> Req. To
			if (PopulateActualDates)
			{
				data.AddData("localProcessing.DeliveryCartageCompleted", localProcessing.DeliveryCartageCompleted = day += oneDay); // CNE -> Act
			}

			data.AddData("localProcessing.FCLAvailable", localProcessing.FCLAvailable = day += oneDay); // CTO -> Req. From
			data.AddData("localProcessing.FCLStorageCommences", localProcessing.FCLStorageCommences = day += oneDay); // CTO -> Req. To
			data.AddData("localProcessing.LCLAvailable", localProcessing.LCLAvailable = day += oneDay); // CFS -> Req. To
			data.AddData("localProcessing.LCLStorageCommences", localProcessing.LCLStorageCommences = day += oneDay); // CFS -> Req. To

			// pickup

			data.AddData("container1.ReleaseNum", container1.ReleaseNum = "EmptyRelease123"); //CYD -> Est
			data.AddData("container1.ContainerParkEmptyPickupGateOut", container1.ContainerParkEmptyPickupGateOut = day += oneDay); // CYD -> Pic -> Act

			data.AddData("container1.EmptyRequired", container1.EmptyRequired = day += oneDay); // CNR or CFS -> DLV -> Req. To
			data.AddData("container1.DepartureEstimatedPickup", container1.DepartureEstimatedPickup = day += oneDay); //CNR or CFS -> Est pic

			data.AddData("container1.DepartureSlotReference", container1.DepartureSlotReference = "DepSlot123");
			data.AddData("container1.DepartureSlotDateTime", container1.DepartureSlotDateTime = day += oneDay);
			if (PopulateActualDates)
			{
				data.AddData("container1.FCLWharfGateIn", container1.FCLWharfGateIn = day += oneDay); // CTO -> Del -> Act
			}

			// delivery

			data.AddData("container1.FCLAvailable", container1.FCLAvailable = day += oneDay);
			data.AddData("container1.FCLStorageCommences", container1.FCLStorageCommences = day += oneDay);
			data.AddData("container1.LCLAvailable", container1.LCLAvailable = day += oneDay);
			data.AddData("container1.LCLStorageCommences", container1.LCLStorageCommences = day += oneDay);

			data.AddData("container1.ArrivalSlotReference", container1.ArrivalSlotReference = "ArvSlot123");
			data.AddData("container1.ArrivalSlotDateTime", container1.ArrivalSlotDateTime = day += oneDay);

			data.AddData("container1.ContainerImportDORelease", container1.ContainerImportDORelease = "ContImpDORel123");
			if (PopulateActualDates)
			{
				data.AddData("container1.FCLWharfGateOut", container1.FCLWharfGateOut = day += oneDay); // CTO -> pic -> Act
			}

			data.AddData("container1.ArrivalEstimatedDelivery", container1.ArrivalEstimatedDelivery = day += oneDay); // CFS or CNE -> pic -> Act
			data.AddData("container1.EmptyReadyForReturn", container1.EmptyReadyForReturn = day += oneDay); // CFS or CNE -> Pic -> Req From
			data.AddData("container1.EmptyReturnedBy", container1.EmptyReturnedBy = day += oneDay); // CYD -> Del -> Req. To
			if (PopulateActualDates)
			{
				data.AddData("container1.ContainerParkEmptyReturnGateIn", container1.ContainerParkEmptyReturnGateIn = day += oneDay); // CYD -> Del -> Act
			}

			// pickup

			data.AddData("container2.ReleaseNum", container2.ReleaseNum = "EmptyRelease456"); //CYD -> Est
			data.AddData("container2.ContainerParkEmptyPickupGateOut", container2.ContainerParkEmptyPickupGateOut = day += oneDay); // CYD -> Pic -> Act

			data.AddData("container2.EmptyRequired", container2.EmptyRequired = day += oneDay); // CNR or CFS -> DLV -> Req. To
			data.AddData("container2.DepartureEstimatedPickup", container2.DepartureEstimatedPickup = day += oneDay); //CNR or CFS -> Est pic

			data.AddData("container2.DepartureSlotReference", container2.DepartureSlotReference = "DepSlot456");
			data.AddData("container2.DepartureSlotDateTime", container2.DepartureSlotDateTime = day += oneDay);
			if (PopulateActualDates)
			{
				data.AddData("container2.FCLWharfGateIn", container2.FCLWharfGateIn = day += oneDay); // CTO -> Del -> Act
			}

			// delivery		   

			data.AddData("container2.FCLAvailable", container2.FCLAvailable = day += oneDay);
			data.AddData("container2.FCLStorageCommences", container2.FCLStorageCommences = day += oneDay);
			data.AddData("container2.LCLAvailable", container2.LCLAvailable = day += oneDay);
			data.AddData("container2.LCLStorageCommences", container2.LCLStorageCommences = day += oneDay);

			data.AddData("container2.ArrivalSlotReference", container2.ArrivalSlotReference = "ArvSlot456");
			data.AddData("container2.ArrivalSlotDateTime", container2.ArrivalSlotDateTime = day += oneDay);

			data.AddData("container2.ContainerImportDORelease", container2.ContainerImportDORelease = "ContImpDORel456");
			if (PopulateActualDates)
			{
				data.AddData("container2.FCLWharfGateOut", container2.FCLWharfGateOut = day += oneDay); // CTO -> pic -> Act
			}

			data.AddData("container2.ArrivalEstimatedDelivery", container2.ArrivalEstimatedDelivery = day += oneDay); // CFS or CNE -> pic -> Act
			data.AddData("container2.EmptyReadyForReturn", container2.EmptyReadyForReturn = day += oneDay); // CFS or CNE -> Pic -> Req From
			data.AddData("container2.EmptyReturnedBy", container2.EmptyReturnedBy = day += oneDay); // CYD -> Del -> Req. To
			if (PopulateActualDates)
			{
				data.AddData("container2.ContainerParkEmptyReturnGateIn", container2.ContainerParkEmptyReturnGateIn = day += oneDay); // CYD -> Del -> Act
			}

			return consol;
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(new BusinessObjectFactory())); }
		}

		TransportBookingTestHelper helper;
	}
}

#endif
