using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	partial class DeclarationSynchroniserTest : SynchroniserTestCase
	{
		public void TestTrimSCACCodeInFlightNoInAirMode()
		{
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "1234";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = declaration.TablePrefix;

			header.Synchroniser.Synchronise(true);
			AssertEquals("1234", header.BH_VoyageNumber);

			declaration.JE_VoyageFlightNo = "~~123";
			var refAirLine = Factory.New<RefAirline>();
			refAirLine.RM_TwoCharacterCode = "~~";

			header.Synchroniser.Synchronise(true);
			AssertEquals("123", header.BH_VoyageNumber);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VoyageFlightNo = "~~123";
			AssertEquals("~~123", header.BH_VoyageNumber);
		}

		public void TestDeletingMoveHeadersIfNeeded()
		{
			declaration.US_SchDEntry = "8659";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = declaration.TablePrefix;
			var bill = header.Bills.AddNew();
			var moveHeader1 = header.MovementHeader;
			AssertEquals(false, moveHeader1.CanDelete);

			var moveHeader2 = header.MovementHeaders.AddNew();
			var moveHeader2Message = moveHeader2.Messages.AddNew(typeof(MQEDIMessage));
			moveHeader2Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			AssertEquals(false, moveHeader2.CanDelete);

			var moveHeader3 = header.MovementHeaders.AddNew();
			AssertEquals(true, moveHeader3.CanDelete);

			header.BH_OverrideFreightDefaults = true;
			AssertEquals(true, moveHeader1.CanDelete);
			AssertEquals(false, moveHeader2.CanDelete);
			AssertEquals(true, moveHeader3.CanDelete);

			header.BH_OverrideFreightDefaults = false;
			header.Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(false, moveHeader1.IsDeleted);
			AssertEquals(false, moveHeader2.IsDeleted);
			AssertEquals(false, moveHeader3.IsDeleted);
		}

		public void TestDeletingBillsIfNeeded()
		{
			header.Synchroniser.SetEnabled(false, false);
			declaration.JE_MasterBillIssuerSCAC = "OTW1";
			declaration.JE_MasterBill = "25684495";
			declaration.PrimaryMasterBill.US_Weight = 46;
			Factory.Save();

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var moveHeader1 = header.MovementHeader;
			var moveHeader1Detail = moveHeader1.MovementDetails.AddNew(bill1.PK);
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			var moveHeader2Message = moveHeader2.Messages.AddNew(typeof(MQEDIMessage));
			moveHeader2Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var moveHeader2Detail = moveHeader2.MovementDetails.AddNew(bill2.PK);
			bill1.B0_IssuerCode = "OTW1";
			bill1.B0_MasterBillNumber = "25684495";
			Factory.Save();
			AssertEquals("bill1.CanDelete", true, bill1.CanDelete);
			AssertEquals("bill2.CanDelete", false, bill2.CanDelete);
			AssertEquals("bill3.CanDelete", true, bill3.CanDelete);

			header.BH_OverrideFreightDefaults = true;
			AssertEquals("bill1.CanDelete", true, bill1.CanDelete);
			AssertEquals("bill2.CanDelete", false, bill2.CanDelete);
			AssertEquals("bill3.CanDelete", true, bill3.CanDelete);

			header.BH_OverrideFreightDefaults = false;
			header.Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("bill1.IsDeleted", false, bill1.IsDeleted);
			AssertEquals("bill1.B0_Weight", 0m, bill1.B0_Weight);
			AssertEquals("bill2.IsDeleted", false, bill2.IsDeleted);
			AssertEquals("bill2.B0_Weight", 0m, bill2.B0_Weight);
			AssertEquals("bill3.IsDeleted", false, bill3.IsDeleted);
		}

		public void TestSynchronizeFromDeclaration()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_RL_NKClosestPort = "AUSYD";
			importer1.OH_Code = "IMP001";
			importer1.OH_FullName = "Importer One Co. Ltd";

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_RL_NKClosestPort = "USCHI";
			importer2.OH_Code = "IMP002";
			importer2.OH_FullName = "Importer Two Co. Ltd";

			var inBond = Factory.New<CusInBondHeader>();
			inBond.BH_ParentID = declaration.PK;
			inBond.BH_ParentTableCode = declaration.TablePrefix;
			inBond.Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));

			declaration.JE_OH_Importer = importer1.PK;
			AssertEquals("BH_OA_Importer", importer1.MainAddress.PK, inBond.BH_OA_Importer);
			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals("BH_OA_Importer", importer2.MainAddress.PK, inBond.BH_OA_Importer);
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("BH_OA_Importer", ZGuid.Empty, inBond.BH_OA_Importer);

			foreach (var tuple in new[] {
				new Tuple<ZString, ZString>(InBondTransportModeCodes.Codes.AirNonContainer, TransportTypeList.Codes.Air),
				new Tuple<ZString, ZString>(InBondTransportModeCodes.Codes.RailNonContainer, TransportTypeList.Codes.Rail),
				new Tuple<ZString, ZString>(InBondTransportModeCodes.Codes.TruckNonContainer, TransportTypeList.Codes.Truck)
			})
			{
				declaration.JE_TransportMode = tuple.Item2;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
				AssertEquals("BH_ImportTransportMode", tuple.Item1, inBond.BH_ImportTransportMode);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				AssertEquals("BH_ImportTransportMode", tuple.Item1, inBond.BH_ImportTransportMode);

				declaration.JE_TransportMode = "!@3";
				AssertNotEquals("BH_ImportTransportMode", tuple.Item1, inBond.BH_ImportTransportMode);
			}

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("BH_ImportTransportMode", ZString.Empty, inBond.BH_ImportTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("BH_ImportTransportMode", InBondTransportModeCodes.Codes.VesselContainer, inBond.BH_ImportTransportMode);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("BH_ImportTransportMode", InBondTransportModeCodes.Codes.VesselNonContainer, inBond.BH_ImportTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("BH_ImportTransportMode", InBondTransportModeCodes.Codes.FixedTransportInstallations, inBond.BH_ImportTransportMode);

			declaration.US_UI_NKCarrierSCAC = "QF";
			AssertEquals("BH_CarrierSCAC", "QF", inBond.BH_CarrierSCAC);
			declaration.US_UI_NKCarrierSCAC = "APCD";
			AssertEquals("BH_CarrierSCAC", "APCD", inBond.BH_CarrierSCAC);
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			AssertEquals("BH_CarrierSCAC", ZString.Empty, inBond.BH_CarrierSCAC);

			declaration.JE_VoyageFlightNo = "QF119";
			AssertEquals("BH_VoyageNumber", "QF119", inBond.BH_VoyageNumber);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF119";
			AssertEquals("BH_VoyageNumber", "119", inBond.BH_VoyageNumber);
			declaration.JE_VoyageFlightNo = "#@119";
			AssertEquals("BH_VoyageNumber", "#@119", inBond.BH_VoyageNumber);
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertEquals("BH_VoyageNumber", ZString.Empty, inBond.BH_VoyageNumber);

			declaration.JE_VesselName = "BOB'S BOAT";
			AssertEquals("BH_ImportConveyanceName", "BOB'S BOAT", inBond.BH_ImportConveyanceName);
			declaration.JE_VesselName = "WENDY'S BOAT";
			AssertEquals("BH_ImportConveyanceName", "WENDY'S BOAT", inBond.BH_ImportConveyanceName);
			declaration.JE_VesselName = ZString.Empty;
			AssertEquals("BH_ImportConveyanceName", ZString.Empty, inBond.BH_ImportConveyanceName);

			declaration.JE_OH_ShippingLine = importer1.PK;
			AssertEquals("BH_ImportConveyanceCountry not synchonized", ZString.Empty, inBond.BH_ImportConveyanceCountry);
			declaration.JE_OH_ShippingLine = ZGuid.Empty;
			AssertEquals("BH_ImportConveyanceCountry", ZString.Empty, inBond.BH_ImportConveyanceCountry);

			declaration.US_SchDLoading = "64234";
			AssertEquals("BH_ImportLoadPortKCode", "64234", inBond.BH_ImportLoadPortKCode);
			declaration.US_SchDLoading = "89654";
			AssertEquals("BH_ImportLoadPortKCode", "89654", inBond.BH_ImportLoadPortKCode);
			declaration.US_SchDLoading = ZString.Empty;
			AssertEquals("BH_ImportLoadPortKCode", ZString.Empty, inBond.BH_ImportLoadPortKCode);

			declaration.US_SchDArrival = "2704";
			AssertEquals("BH_PortUnladingDCode", "2704", inBond.BH_PortUnladingDCode);
			declaration.US_SchDArrival = "3905";
			AssertEquals("BH_PortUnladingDCode", "3905", inBond.BH_PortUnladingDCode);
			declaration.US_SchDArrival = ZString.Empty;
			AssertEquals("BH_PortUnladingDCode", ZString.Empty, inBond.BH_PortUnladingDCode);

			declaration.US_UC_NKCountryOfExport = "AU";
			AssertEquals("BH_RN_NKFirstExportCountry", "AU", inBond.BH_RN_NKFirstExportCountry);
			declaration.US_UC_NKCountryOfExport = "US";
			AssertEquals("BH_RN_NKFirstExportCountry", "US", inBond.BH_RN_NKFirstExportCountry);
			declaration.US_UC_NKCountryOfExport = ZString.Empty;
			AssertEquals("BH_RN_NKFirstExportCountry", ZString.Empty, inBond.BH_RN_NKFirstExportCountry);

			declaration.JE_DateOfArrival = new ZDateTime(2012, 1, 4);
			AssertEquals("BH_ETA", new ZDateTime(2012, 1, 4), inBond.BH_ETA);
			declaration.JE_DateOfArrival = new ZDateTime(2012, 4, 1);
			AssertEquals("BH_ETA", new ZDateTime(2012, 4, 1), inBond.BH_ETA);
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals("BH_ETA", ZDateTime.Empty, inBond.BH_ETA);

			declaration.JE_DateAtOrigin = new ZDateTime(2012, 1, 4);
			AssertEquals("BH_SailingDate", new ZDateTime(2012, 1, 4), inBond.BH_SailingDate);
			declaration.JE_DateAtOrigin = new ZDateTime(2012, 5, 3);
			AssertEquals("BH_SailingDate", new ZDateTime(2012, 5, 3), inBond.BH_SailingDate);
			declaration.JE_DateAtOrigin = ZDateTime.Empty;
			AssertEquals("BH_SailingDate", ZDateTime.Empty, inBond.BH_SailingDate);

			declaration.JE_ExportDate = ZDateTime.Empty;
			declaration.US_DateOfExport = new ZDateTime(2012, 1, 4);
			AssertEquals("BH_FirstExportDate", new ZDateTime(2012, 1, 4), inBond.BH_FirstExportDate);
			declaration.US_DateOfExport = new ZDateTime(2012, 5, 3);
			AssertEquals("BH_FirstExportDate", new ZDateTime(2012, 5, 3), inBond.BH_FirstExportDate);
			declaration.US_DateOfExport = ZDateTime.Empty;
			AssertEquals("BH_FirstExportDate", ZDateTime.Empty, inBond.BH_FirstExportDate);

			declaration.US_US_NKLocationOfGoods = "AD34";
			AssertEquals("BH_FIRMS", "AD34", inBond.BH_FIRMS);
			declaration.US_US_NKLocationOfGoods = "TD32";
			AssertEquals("BH_FIRMS", "TD32", inBond.BH_FIRMS);
			declaration.US_US_NKLocationOfGoods = ZString.Empty;
			AssertEquals("BH_FIRMS", ZString.Empty, inBond.BH_FIRMS);

			declaration.JE_OH_Supplier = importer1.PK;
			AssertEquals("BH_OH_Supplier", importer1.PK, inBond.BH_OH_Supplier);
			declaration.JE_OH_Supplier = importer2.PK;
			AssertEquals("BH_OH_Supplier", importer2.PK, inBond.BH_OH_Supplier);
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals("BH_OH_Supplier", ZGuid.Empty, inBond.BH_OH_Supplier);
		}

		public void TestDeleteContainersFromMovementIfNeeded()
		{
			SetupDeclarationAndContainers();

			var container = header.Bills[0].MovementDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CTN01234567");
			AssertNotNull(container);

			container.Delete();
			header.BH_OverrideFreightDefaults = true;
			AssertEquals(0, header.Bills[0].MovementDetail.Containers.Count(x => x.BC_ContainerNum == "CTN01234567"));
		}

		public void TestDeleteContainersFromDeclarationIfNeeded()
		{
			SetupDeclarationAndContainers();
			var container = declaration.CusContainers[0];
			container.Delete();
			AssertEquals(0, header.Bills[0].MovementDetail.Containers.Count(x => x.BC_ContainerNum == "CTN01234567"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = declaration.TablePrefix;
			synchronizer = (CusInBondHeaderDeclarationSynchronizer)header.Synchroniser;
			synchronizer.Synchronise(true);
		}
		JobDeclaration declaration;
		CusInBondHeader header;
		CusInBondHeaderDeclarationSynchronizer synchronizer;

		void SetupDeclarationAndContainers()
		{
			var containerForDeclaration = declaration.CusContainers.AddNew();
			containerForDeclaration.CO_ContainerNumber = "CTN01234567";

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB1";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBill.CU_BillNum = "HB1";

			declaration.Packages.RemoveAndDeleteAll();
			var package0 = declaration.Packages.AddNew();
			package0.CW_HouseBill = masterBill.CU_BillUniqueCode;
			package0.CW_ContainerNoOrEquipmentNo = "CTN01234567";
			package0.CW_PackQty = 200;
			package0.CW_PackType = "AE";

			var package1 = declaration.Packages.AddNew();
			package1.CW_HouseBill = houseBill.CU_BillUniqueCode;
			package1.CW_ContainerNoOrEquipmentNo = "CTN01234567";
			package1.CW_PackQty = 500;
			package1.CW_PackType = "BB";

			AssertEquals(1, header.Bills.Count);

			var bill = header.Bills[0];
			AssertEquals(1, bill.MovementDetail.Containers.Count(x => x.BC_ContainerNum == "CTN01234567"));
			AssertEquals(2, bill.MovementDetail.Containers[0].Commodities.Count);
		}
	}
}
