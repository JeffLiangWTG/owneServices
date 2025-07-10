using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class InternalContainerUpdaterTest : BaseFreightTest
	{
		#region Empty

		public void TestUpdateContainerFromXsd_Empty()
		{
			var today = ZDateTime.Today;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var cartageXsd = CreateXsdCartage();
			var legCTO_CNEXsd = CreateXsdCartageLeg(cartageXsd, DocAddressAddressType.LCT, DocAddressAddressType.LCI, ZDateTime.Empty, "");
			var legCNE_CYDXsd = CreateXsdCartageLeg(cartageXsd, DocAddressAddressType.LCI, DocAddressAddressType.LCY, ZDateTime.Empty, "");

			var container = Factory.New<CommonContainer>();
			InternalContainerUpdater.UpdateContainerFromXsd(false, cartageXsd, container, new List<CartageLeg>() { legCTO_CNEXsd, legCNE_CYDXsd }, context);

			AssertExportFields(container, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZGuid.Empty);
			AssertImportFields(container, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZGuid.Empty);
		}

		#endregion

		#region Export

		public void TestUpdateContainerFromXsd_Export()
		{
			var today = ZDateTime.Today;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var cartageXsd = CreateXsdCartage();
			var legCYD_CNRXsd = CreateXsdCartageLeg(cartageXsd, DocAddressAddressType.LCY, DocAddressAddressType.LCE, today, ContainerYard.OH_Code);
			var legCNR_CTOXsd = CreateXsdCartageLeg(cartageXsd, DocAddressAddressType.LCE, DocAddressAddressType.LCT, today.AddHours(10), "");

			var container = Factory.New<CommonContainer>();
			InternalContainerUpdater.UpdateContainerFromXsd(true, cartageXsd, container, new List<CartageLeg>() { legCYD_CNRXsd, legCNR_CTOXsd }, context);

			var totalDem = (ZDateTime)new TimeSpan(6, 14, 0);
			var cydOut = today.AddHours(2).AddMinutes(6);
			var cnrIn = today.AddHours(11).AddMinutes(5);
			var ctoIn = today.AddHours(13).AddMinutes(7);

			AssertEquals("T00001357", container.JC_DepartureCartageRef);
			AssertEquals("", container.JC_ArrivalCartageRef);

			AssertExportFields(container, totalDem, cydOut, cnrIn, ctoIn, ContainerYard.PK);
			AssertImportFields(container, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZGuid.Empty);
		}

		#endregion

		#region Import

		public void TestUpdateContainerFromXsd_Import()
		{
			var today = ZDateTime.Today;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var cartageXsd = CreateXsdCartage();
			var legCTO_CNEXsd = CreateXsdCartageLeg(cartageXsd, DocAddressAddressType.LCT, DocAddressAddressType.LCI, today, "");
			var legCNE_CYDXsd = CreateXsdCartageLeg(cartageXsd, DocAddressAddressType.LCI, DocAddressAddressType.LCY, today.AddHours(10), ContainerYard.OH_Code);

			var container = Factory.New<CommonContainer>();
			InternalContainerUpdater.UpdateContainerFromXsd(false, cartageXsd, container, new List<CartageLeg>() { legCTO_CNEXsd, legCNE_CYDXsd }, context);

			var totalDem = (ZDateTime)new TimeSpan(6, 14, 0);
			var ctoOut = today.AddHours(2).AddMinutes(6);
			var cneIn = today.AddHours(3).AddMinutes(7);
			var cydIn = today.AddHours(13).AddMinutes(7);

			AssertEquals("", container.JC_DepartureCartageRef);
			AssertEquals("T00001357", container.JC_ArrivalCartageRef);

			AssertExportFields(container, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZGuid.Empty);
			AssertImportFields(container, totalDem, ctoOut, cneIn, cydIn, ContainerYard.PK);
		}

		#endregion

		#region Asserts

		void AssertExportFields(CommonContainer container, ZDateTime dem, ZDateTime cydOut, ZDateTime cnrIn, ZDateTime ctoIn, ZGuid cydOrg)
		{
			AssertEquals(dem, container.DepartureTruckWaitTime);
			AssertEquals(cydOut, container.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals(cnrIn, container.JC_DepartureCartageComplete);
			AssertEquals(ctoIn, container.JC_FCLWharfGateIn);
			AssertEquals(cydOrg, container.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK);
		}

		void AssertImportFields(CommonContainer container, ZDateTime dem, ZDateTime ctoOut, ZDateTime cneIn, ZDateTime cydIn, ZGuid cydOrg)
		{
			AssertEquals(dem, container.ArrivalTruckWaitTime);
			AssertEquals(ctoOut, container.JC_FCLWharfGateOut);
			AssertEquals(cneIn, container.JC_ArrivalCartageComplete);
			AssertEquals(cydIn, container.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals(cydOrg, container.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK);
		}

		#endregion

		#region Implementation

		#region GetXsdCartage

		CartageJob CreateXsdCartage()
		{
			CartageJob cartage = new CartageJob();
			cartage.Action = FreightConstants.CartageStatusExport;
			cartage.ActionType = FreightConstants.LocalCartageBookingStatus.Codes.WorkCompleted;
			cartage.JobNumber = "T00001357";
			cartage.ClientJobReference = "ClientJobReference";
			return cartage;
		}

		#endregion

		#region CreateXsdCartageLeg

		CartageLeg CreateXsdCartageLeg(CartageJob cartage, DocAddressAddressType picType, DocAddressAddressType dlvType, ZDateTime date, ZString ediCode)
		{
			CartageLeg cartageLeg = cartage.CartageLegs.AddNew();

			if (!date.IsEmpty)
			{
				cartageLeg.CartageLegDates.PickupDemurrage = new TimeSpan(1, 3, 0);
				cartageLeg.CartageLegDates.DeliveryDemurrage = new TimeSpan(2, 4, 0);

				cartageLeg.CartageLegDates.PickupTimeInDate = date.AddHours(1).AddMinutes(5);
				cartageLeg.CartageLegDates.PickupTimeOutDate = date.AddHours(2).AddMinutes(6);
				cartageLeg.CartageLegDates.DeliverTimeInDate = date.AddHours(3).AddMinutes(7);
				cartageLeg.CartageLegDates.DeliverTimeOutDate = date.AddHours(4).AddMinutes(8);
			}

			cartageLeg.Pickup.DocAddress.AddressType = picType;
			cartageLeg.Pickup.DocAddress.AddressReference.Organisation.EDICode = ediCode;

			cartageLeg.Delivery.DocAddress.AddressType = dlvType;
			cartageLeg.Delivery.DocAddress.AddressReference.Organisation.EDICode = ediCode;

			CreateXsdContainerOnLeg(cartageLeg, date);

			return cartageLeg;
		}

		#endregion

		#region CreateXsdContainerOnLeg

		void CreateXsdContainerOnLeg(CartageLeg xsdCartageLeg, ZDateTime date)
		{
			var xsdContainerLeg = new CartageLegContainer();
			xsdContainerLeg.ContainerNumber = "CONTAINER1";
			xsdContainerLeg.ContainerAdditionalInfo.ArrivalSlotRef = "ArrivalSlotRef";
			xsdContainerLeg.ContainerAdditionalInfo.DepartureSlotRef = "DepSlotRef";

			if (!date.IsEmpty)
			{
				xsdContainerLeg.ContainerAdditionalInfo.ArrivalSlotDate = date.AddDays(1);
				xsdContainerLeg.ContainerAdditionalInfo.DepartureSlotDate = date.AddDays(2);
				xsdContainerLeg.ContainerAdditionalInfo.EmptyReturnedByDate = date.AddDays(3);
				xsdContainerLeg.ContainerAdditionalInfo.PackDate = date.AddDays(4);
				xsdContainerLeg.ContainerAdditionalInfo.UnpackDate = date.AddDays(5);
			}

			xsdCartageLeg.Item = xsdContainerLeg;
		}

		#endregion

		#region ContainerYard

		OrgHeader ContainerYard
		{
			get { return containerYard ?? (containerYard = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader containerYard;

		#endregion

		#endregion
	}
}
