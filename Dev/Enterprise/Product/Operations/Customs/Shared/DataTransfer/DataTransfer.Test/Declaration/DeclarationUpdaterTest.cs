using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using CartageBookingCodes = Enterprise.Freight.Business.FreightConstants.LocalCartageBookingStatus.Codes;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class DeclarationUpdaterTest : TestCaseWithFactory
	{
		public void TestIDeclarationUpdater_Empty()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CusContainers.Add(CusContainer);
			Factory.Save();

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = CreateXsdCartage(CartageBookingCodes.WorkCommenced);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCT, DocAddressAddressType.LCI, ZDateTime.Empty, "", true);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCI, DocAddressAddressType.LCY, ZDateTime.Empty, "", true);

			AssertEquals("Precondition", declaration.JE_CartageCompleted, ZDateTime.Empty);

			Updater.Update(declaration, xsdCartage, context);
			AssertEquals(ZDateTime.Empty, declaration.JE_CartageCompleted);
			AssertExportFields(CusContainer, "", ZDateTime.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, "", ZDateTime.Empty);
			AssertImportFields(CusContainer, "T00001357", ZDateTime.Empty, "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZGuid.Empty);
		}

		public void TestIDeclarationUpdater_Export_Container()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CusContainers.Add(CusContainer);

			Factory.Save();

			var today = ZDateTime.Today;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = CreateXsdCartage(CartageBookingCodes.WorkCommenced);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCY, DocAddressAddressType.LCE, today, ContainerYard.OH_Code, true);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCE, DocAddressAddressType.LCT, today.AddHours(10), "", true);

			AssertEquals("Precondition", declaration.JE_CartageCompleted, ZDateTime.Empty);

			Updater.Update(declaration, xsdCartage, context);

			var totalDem = ZeroTime.AddHours(6).AddMinutes(14);
			var cydOut = today.AddHours(2).AddMinutes(6);
			var cnrIn = today.AddHours(11).AddMinutes(5);
			var ctoIn = today.AddHours(13).AddMinutes(7);
			var slot = today.AddDays(2).AddHours(10);

			Factory.Save();

			AssertEquals(cnrIn, declaration.JE_CartageCompleted);
			AssertExportFields(CusContainer, "T00001357", totalDem, ContainerYard.PK, cydOut, cnrIn, ctoIn, "DepSlotRef", slot);
			AssertImportFields(CusContainer, "", ZDateTime.Empty, "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZGuid.Empty);
		}

		public void TestIDeclarationUpdater_Export_Container_Multiple()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CusContainers.Add(CusContainer);

			var secondContainer = declaration.CusContainers.AddNew();
			secondContainer.CO_ContainerNumber = "DEFG123456";
			secondContainer.CO_Seal = "SEAL";
			secondContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			secondContainer.CO_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			Factory.Save();

			var today = ZDateTime.Today;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = CreateXsdCartage(CartageBookingCodes.WorkCommenced);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCY, DocAddressAddressType.LCE, today, ContainerYard.OH_Code, true);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCE, DocAddressAddressType.LCT, today.AddHours(10), "", true);

			AssertEquals("Precondition", declaration.JE_CartageCompleted, ZDateTime.Empty);

			Updater.Update(declaration, xsdCartage, context);

			var totalDem = ZeroTime.AddHours(6).AddMinutes(14);
			var cydOut = today.AddHours(2).AddMinutes(6);
			var cnrIn = today.AddHours(11).AddMinutes(5);
			var ctoIn = today.AddHours(13).AddMinutes(7);
			var slot = today.AddDays(2).AddHours(10);

			Factory.Save();

			AssertEquals(ZDateTime.Empty, declaration.JE_CartageCompleted);
			AssertExportFields(CusContainer, "T00001357", totalDem, ContainerYard.PK, cydOut, cnrIn, ctoIn, "DepSlotRef", slot);
			AssertImportFields(CusContainer, "", ZDateTime.Empty, "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZGuid.Empty);

			xsdCartage = CreateXsdCartage(CartageBookingCodes.WorkCommenced);
			var legCYD_CNRXsd = CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCY, DocAddressAddressType.LCE, today, ContainerYard.OH_Code, true);
			var legCNR_CTOXsd = CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCE, DocAddressAddressType.LCT, today.AddHours(10), "", true);

			((CartageLegContainer)legCYD_CNRXsd.Item).ContainerNumber = "DEFG123456";
			((CartageLegContainer)legCNR_CTOXsd.Item).ContainerNumber = "DEFG123456";

			Updater.Update(declaration, xsdCartage, context);
			Factory.Save();

			totalDem = ZeroTime.AddHours(6).AddMinutes(14);
			cydOut = today.AddHours(2).AddMinutes(6);
			cnrIn = today.AddHours(11).AddMinutes(5);
			ctoIn = today.AddHours(13).AddMinutes(7);
			slot = today.AddDays(2).AddHours(10);

			AssertEquals(cnrIn, declaration.JE_CartageCompleted);
			AssertExportFields(secondContainer, "T00001357", totalDem, ContainerYard.PK, cydOut, cnrIn, ctoIn, "DepSlotRef", slot);
			AssertImportFields(secondContainer, "", ZDateTime.Empty, "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZGuid.Empty);

			AssertEquals(ZeroTime.AddHours(12).AddMinutes(28), declaration.JE_PickupOrDeliveryTruckWaitTime);
		}

		public void TestIDeclarationUpdater_Import_Container()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CusContainers.Add(CusContainer);
			Factory.Save();

			var today = ZDateTime.Today;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = CreateXsdCartage(CartageBookingCodes.WorkCommenced);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCT, DocAddressAddressType.LCI, today, "", true);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCI, DocAddressAddressType.LCY, today.AddHours(10), ContainerYard.OH_Code, true);

			AssertEquals("Precondition", declaration.JE_CartageCompleted, ZDateTime.Empty);

			Updater.Update(declaration, xsdCartage, context);

			var totalDem = ZeroTime.AddHours(6).AddMinutes(14);
			var slot = today.AddDays(3);
			var ctoOut = today.AddHours(2).AddMinutes(6);
			var cneIn = today.AddHours(3).AddMinutes(7);
			var cydIn = today.AddHours(13).AddMinutes(7);

			Factory.Save();

			AssertEquals(cneIn, declaration.JE_CartageCompleted);
			AssertExportFields(CusContainer, "", ZDateTime.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, "", ZDateTime.Empty);
			AssertImportFields(CusContainer, "T00001357", totalDem, "ArrivalSlotRef", slot, ctoOut, cneIn, cydIn, ContainerYard.PK);
		}

		public void TestIDeclarationUpdater_Export_Loose()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Loose;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();

			var today = ZDateTime.Today;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = CreateXsdCartage(CartageBookingCodes.WorkCommenced);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCE, DocAddressAddressType.LCF, today, "", false);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCE, DocAddressAddressType.LCF, today.AddHours(10), "", false);

			AssertEquals("Precondition", declaration.JE_CartageCompleted, ZDateTime.Empty);

			Updater.Update(declaration, xsdCartage, context);

			var totalDem = ZeroTime.AddHours(6).AddMinutes(14);
			var lastCNRin = today.AddHours(11).AddMinutes(5);

			AssertEquals(totalDem, declaration.JE_PickupOrDeliveryTruckWaitTime);
			AssertEquals(lastCNRin, declaration.JE_CartageCompleted);
		}

		public void TestIDeclarationUpdater_Import_Loose()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Loose;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			var today = ZDateTime.Today;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = CreateXsdCartage(CartageBookingCodes.WorkCommenced);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCF, DocAddressAddressType.LCI, today, "", false);
			CreateXsdCartageLeg(xsdCartage, DocAddressAddressType.LCF, DocAddressAddressType.LCI, today.AddHours(10), "", false);

			AssertEquals("Precondition", declaration.JE_CartageCompleted, ZDateTime.Empty);

			Updater.Update(declaration, xsdCartage, context);

			var totalDem = ZeroTime.AddHours(6).AddMinutes(14);
			var lastCNRin = today.AddHours(13).AddMinutes(7);

			AssertEquals(totalDem, declaration.JE_PickupOrDeliveryTruckWaitTime);
			AssertEquals(lastCNRin, declaration.JE_CartageCompleted);
		}

		void AssertExportFields(BaseCusContainer container, ZString refNumber, ZDateTime dem, ZGuid cydOrg, ZDateTime cydOut, ZDateTime cnrIn, ZDateTime ctoIn, ZString slotRef, ZDateTime slotDate)
		{
			AssertEquals(refNumber, container.JobContainer.JC_DepartureCartageRef);
			AssertEquals(dem, container.JobContainer.DepartureTruckWaitTime);
			AssertEquals(cydOrg, container.JobContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(cydOut, container.JobContainer.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals(cnrIn, container.JobContainer.JC_DepartureCartageComplete);
			AssertEquals(ctoIn, container.JobContainer.JC_FCLWharfGateIn);
			AssertEquals(slotRef, CusContainer.JobContainer.JC_DepartureSlotReference);
			AssertEquals(slotDate, CusContainer.JobContainer.JC_DepartureSlotDateTime);
		}

		void AssertImportFields(BaseCusContainer container, ZString refNumber, ZDateTime dem, ZString slotRef, ZDateTime slotDate, ZDateTime ctoOut, ZDateTime cneIn, ZDateTime cydIn, ZGuid cydOrg)
		{
			AssertEquals(refNumber, container.JobContainer.JC_ArrivalCartageRef);
			AssertEquals(dem, container.JobContainer.ArrivalTruckWaitTime);
			AssertEquals(slotRef, CusContainer.JobContainer.JC_ArrivalSlotReference);
			AssertEquals(slotDate, CusContainer.JobContainer.JC_ArrivalSlotDateTime);
			AssertEquals(ctoOut, container.JobContainer.JC_FCLWharfGateOut);
			AssertEquals(cneIn, container.JobContainer.JC_ArrivalCartageComplete);
			AssertEquals(cydIn, container.JobContainer.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals(cydOrg, container.JobContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK);
		}

		CartageJob CreateXsdCartage(ZString status)
		{
			if (status.IsEmpty)
			{
				status = FreightConstants.LocalCartageBookingStatus.Codes.WorkCompleted;
			}

			var cartage = new CartageJob();
			cartage.Action = FreightConstants.CartageStatusExport;
			cartage.ActionType = status;
			cartage.JobNumber = "T00001357";
			cartage.ClientJobReference = "ClientJobReference";
			return cartage;
		}

		CartageLeg CreateXsdCartageLeg(CartageJob cartage, DocAddressAddressType picType, DocAddressAddressType dlvType, ZDateTime date, ZString ediCode, bool isContainerLeg)
		{
			var cartageLeg = cartage.CartageLegs.AddNew();

			if (!date.IsEmpty)
			{
				cartageLeg.CartageLegDates.PickupDemurrage = ZeroTime.AddHours(1).AddMinutes(3);
				cartageLeg.CartageLegDates.DeliveryDemurrage = ZeroTime.AddHours(2).AddMinutes(4);

				cartageLeg.CartageLegDates.PickupTimeInDate = date.AddHours(1).AddMinutes(5);
				cartageLeg.CartageLegDates.PickupTimeOutDate = date.AddHours(2).AddMinutes(6);
				cartageLeg.CartageLegDates.DeliverTimeInDate = date.AddHours(3).AddMinutes(7);
				cartageLeg.CartageLegDates.DeliverTimeOutDate = date.AddHours(4).AddMinutes(8);
			}

			cartageLeg.Pickup.DocAddress.AddressType = picType;
			cartageLeg.Pickup.DocAddress.AddressReference.Organisation.EDICode = ediCode;

			cartageLeg.Delivery.DocAddress.AddressType = dlvType;
			cartageLeg.Delivery.DocAddress.AddressReference.Organisation.EDICode = ediCode;

			if (isContainerLeg)
			{
				CreateXsdContainerLeg(cartageLeg, date);
			}

			return cartageLeg;
		}

		void CreateXsdContainerLeg(CartageLeg xsdCartageLeg, ZDateTime date)
		{
			var xsdContainerLeg = new CartageLegContainer();
			xsdContainerLeg.ContainerNumber = CusContainer.CO_ContainerNumber;

			if (!date.IsEmpty)
			{
				xsdContainerLeg.ContainerAdditionalInfo.PackDate = date.AddDays(1);
				xsdContainerLeg.ContainerAdditionalInfo.DepartureSlotDate = date.AddDays(2);
				xsdContainerLeg.ContainerAdditionalInfo.DepartureSlotRef = "DepSlotRef";
				xsdContainerLeg.ContainerAdditionalInfo.ArrivalSlotRef = "ArrivalSlotRef";
				xsdContainerLeg.ContainerAdditionalInfo.ArrivalSlotDate = date.AddDays(3);
				xsdContainerLeg.ContainerAdditionalInfo.UnpackDate = date.AddDays(4);
				xsdContainerLeg.ContainerAdditionalInfo.EmptyReturnedByDate = date.AddDays(5);
			}

			xsdCartageLeg.Item = xsdContainerLeg;
		}

		BaseJobDeclaration GetJobDeclaration()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Local Consignee 1";
			consignee.OH_Code = "LOCCON1";
			consignee.MainAddress.OA_Address1 = "Test Address Line";
			consignee.OH_RL_NKClosestPort = "AUSYD";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_FullName = "Overseas Consignor 1";
			consignor.MainAddress.OA_Address1 = "Test Address Line";
			consignor.OH_RL_NKClosestPort = "USLAX";

			var result = Factory.New<BaseJobDeclaration>();
			result.JE_OH_Importer = consignee.PK;
			result.JE_OH_Supplier = consignor.PK;

			return result;
		}

		OrgHeader ContainerYard
		{
			get
			{
				if (fContainerYard == null)
				{
					fContainerYard = Factory.New<BaseFreightTest.TestLocalContainerYard>();
				}
				return fContainerYard;
			}
		}
		OrgHeader fContainerYard;

		BaseCusContainer CusContainer
		{
			get
			{
				if (fContainer == null)
				{
					fContainer = Factory.New<BaseCusContainer>();
					fContainer.CO_ContainerNumber = "ABCD1234560";
					fContainer.CO_Seal = "SEAL";
					fContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
					fContainer.CO_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
				}
				return fContainer;
			}
		}
		BaseCusContainer fContainer;

		ZDateTime ZeroTime => ZDateTime.DefaultDurationEpoch;

		IDeclarationUpdater Updater => updater ?? (updater = new DeclarationUpdater());
		IDeclarationUpdater updater;
	}
}
