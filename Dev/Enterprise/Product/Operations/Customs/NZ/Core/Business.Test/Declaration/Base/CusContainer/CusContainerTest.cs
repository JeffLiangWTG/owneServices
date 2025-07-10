using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System.Linq;
	using NUnit.Framework;

	[TestedType(typeof(CusContainer))]
	class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		public void TestNewContainerAttachesToMasterBillWhenNoHouseBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = "ECI";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MasterBill = "MB1";

			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(1, declaration.PackingGroups.Count);
			AssertEquals(1, declaration.PackingGroups[0].Packages.Count);

			var cont1 = declaration.CusContainers.AddNew();
			cont1.CO_ContainerNumber = "CN001";
			Factory.Save();

			var mBill = declaration.Bills.Cast<Bill>().First(b => b.CU_BillNum == "MB1");
			AssertEquals(1, mBill.PackingGroups.Count);
			var mBillPackGrp = mBill.PackingGroups[0];
			AssertEquals(cont1.PK, mBillPackGrp.CR_CO_Container);
			AssertEquals(1, mBillPackGrp.Packages.Count);
		}

		public void TestNewContainerAttachesToHouseBillWhenExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = "ECI";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MasterBill = "MB1";

			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(1, declaration.PackingGroups.Count);
			AssertEquals(1, declaration.PackingGroups[0].Packages.Count);

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HB1";

			var cont1 = declaration.CusContainers.AddNew();
			cont1.CO_ContainerNumber = "CN001";
			Factory.Save();

			var masterBill = declaration.Bills.Cast<Bill>().First(b => b.CU_BillNum == "MB1");
			AssertEquals(1, masterBill.PackingGroups.Count);
			AssertEquals("Not on Master Bill", ZGuid.Empty, masterBill.PackingGroups[0].CR_CO_Container);

			AssertEquals(1, houseBill.PackingGroups.Count);
			var hBillPackGrp = houseBill.PackingGroups[0];
			AssertEquals("Container is on House Bill", cont1.PK, hBillPackGrp.CR_CO_Container);
			AssertEquals(1, hBillPackGrp.Packages.Count);
		}

		public void TestNewContainerAttachesToStandAloneHouseBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_HouseBill = "HB1";
			var cont1 = declaration.CusContainers.AddNew();
			cont1.CO_ContainerNumber = "CN001";
			var packGrp1 = declaration.PackingGroups[0];
			AssertEquals(cont1.PK, packGrp1.CR_CO_Container);
			AssertEquals("CN001", packGrp1.Packages[0].CW_ContainerNoOrEquipmentNo);
		}

		public void TestNewContainerCreatesNewPackGroupOnHouseBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = "ECI";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";

			var cont1 = declaration.CusContainers.AddNew();
			cont1.CO_ContainerNumber = "CN001";
			Factory.Save();

			AssertEquals(2, declaration.Bills.Count);
			AssertEquals("Master group is removed when house is created by dec", 1, declaration.PackingGroups.Count);
			AssertEquals(1, declaration.PackingGroups[0].Packages.Count);

			var cont2 = declaration.CusContainers.AddNew();
			cont2.CO_ContainerNumber = "CN002";

			AssertEquals(2, declaration.Bills.Count);
			AssertEquals(2, declaration.PackingGroups.Count);

			// Master is ignored
			var mBill = declaration.Bills.Cast<Bill>().First(b => b.CU_BillNum == "MB1");
			AssertEquals(0, mBill.PackingGroups.Count);

			// added container and created package on House
			var hBill = declaration.Bills.Cast<Bill>().First(b => b.CU_BillNum == "HB1");
			AssertEquals(2, hBill.PackingGroups.Count);
			var hBillPackGrpCN1 = hBill.PackingGroups[0];
			AssertEquals(cont1.PK, hBillPackGrpCN1.CR_CO_Container);
			AssertEquals(1, hBillPackGrpCN1.Packages.Count);
			var hBillPackGrpCN2 = hBill.PackingGroups[1];
			AssertEquals(cont2.PK, hBillPackGrpCN2.CR_CO_Container);
			AssertEquals(1, hBillPackGrpCN2.Packages.Count);
		}

		public void TestNewContainerCreatesNewPackOnPacklessGroup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MasterBill = "MB1";

			var masterBill = declaration.Bills[0];
			masterBill.PackingGroups[0].Packages.RemoveAndDeleteAll();

			AssertEquals(1, masterBill.PackingGroups.Count);
			var packGrp = masterBill.PackingGroups[0];
			AssertEquals(0, packGrp.Packages.Count);

			var cont1 = declaration.CusContainers.AddNew();
			cont1.CO_ContainerNumber = "CN001";

			AssertEquals(cont1.PK, packGrp.CR_CO_Container);
			AssertEquals(1, packGrp.Packages.Count);
			AssertEquals("CN001", packGrp.Packages[0].CW_ContainerNoOrEquipmentNo);
		}

		public void TestChangeOfContainerNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MasterBill = "MB1";

			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			bill1.CU_HouseBill = "HB1";

			var cont1 = declaration.CusContainers.AddNew();
			cont1.CO_ContainerNumber = "CN001";

			AssertEquals(1, bill1.PackingGroups.Count);
			var packGrp1 = bill1.PackingGroups[0];
			AssertEquals(cont1.PK, packGrp1.CR_CO_Container);
			AssertEquals("CN001", packGrp1.Packages[0].CW_ContainerNoOrEquipmentNo);

			cont1.CO_ContainerNumber = "CN123";
			AssertEquals(1, bill1.PackingGroups.Count);
			AssertEquals("CN123", packGrp1.Packages[0].CW_ContainerNoOrEquipmentNo);
		}

		public void TestWI00201248_ContainerOnGroupBeingReplaced()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "MB1";

			var bill1 = declaration.Bills[0];
			AssertEquals(1, bill1.PackingGroups.Count);
			Factory.Save();
			// creates a valid containerless packing group on the bill.
			var packGrp1 = bill1.PackingGroups[0];
			AssertEquals(ZGuid.Empty, packGrp1.CR_CO_Container);

			var cont1 = declaration.CusContainers.AddNew();
			cont1.CO_ContainerNumber = "CN001";
			Factory.Save();
			// adds container with packages on the group
			AssertEquals(cont1.PK, packGrp1.CR_CO_Container);
			AssertEquals(1, packGrp1.Packages.Count);

			var cont2 = declaration.CusContainers.AddNew();
			cont2.CO_ContainerNumber = "CN002";
			Factory.Save();
			// adds a second container.  New container gets linked to bill through new packGroup. Container 1 is not affected.
			AssertEquals(2, bill1.PackingGroups.Count);
			var packGrp2 = bill1.PackingGroups[1];
			AssertEquals(cont2.PK, packGrp2.CR_CO_Container);
			AssertEquals(1, packGrp2.Packages.Count);
			AssertNotEquals(packGrp1.PK, packGrp2.PK);
			AssertEquals(1, packGrp1.Packages.Count);
		}

		public void TestIsEmptyContainer()
		{
			AssertEquals("Container.IsEmptyContainer", false, Container.IsEmptyContainer);
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertEquals("Container.IsEmptyContainer", true, Container.IsEmptyContainer);
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertEquals("Container.IsEmptyContainer", false, Container.IsEmptyContainer);
		}

		public void TestCO_FCL_LCL_AIR()
		{
			AssertEquals("JC_IsEmptyContainer", false, Container.JobContainer.JC_IsEmptyContainer);
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertEquals("JC_IsEmptyContainer should be true when CO_FCL_LCL_AIR is set to empty", true, Container.JobContainer.JC_IsEmptyContainer);
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertEquals("JC_IsEmptyContainer", false, Container.JobContainer.JC_IsEmptyContainer);
		}

		public void TestFCXContainerModeIsConvertedToFCLForBuyersConsol()
		{
			var bcnLeadShipment = Freight.Business.CommonShipment.New(Factory);
			var bcnSubShipment = Freight.Business.CommonShipment.New(Factory);

			bcnLeadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			bcnSubShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var bcnConsol = Factory.New<Freight.Business.CommonConsol>();
			bcnConsol.Shipments.Add(bcnLeadShipment);
			bcnLeadShipment.CoLoadShipments.Add(bcnSubShipment);

			var consolContainer = bcnConsol.Containers.AddNew();
			consolContainer.JC_ContainerMode = BaseCusContainer.ContainerModes.FCX;

			Declaration.DisableDefaultPackingInformation = false;
			Declaration.JE_JS = bcnLeadShipment.PK;
			var defaultedCustomsContainer = Declaration.CusContainers.AddNew();

			defaultedCustomsContainer.CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FCX;
			AssertEquals("CO_FCL_LCL_AIR container mode of FCX on BCN Lead Shiment should be converted to FCL - FCX does not exist in NZ Customs", ContainerModeList.Codes.FCL, defaultedCustomsContainer.CO_FCL_LCL_AIR);

			var bcnSubShipmentDeclaration = Factory.New<JobDeclaration>();
			bcnSubShipmentDeclaration.JE_JS = bcnSubShipment.PK;
			var jobDeclarationCustomsContainer = bcnSubShipmentDeclaration.CusContainers.AddNew();
			jobDeclarationCustomsContainer.CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FCX;
			AssertEquals("CO_FCL_LCL_AIR container mode of FCX on BCN sub-shipments should be converted to FCL - FCX does not exist in NZ Customs", ContainerModeList.Codes.FCL, jobDeclarationCustomsContainer.CO_FCL_LCL_AIR);
		}

		public void TestApportionedContainerGoodsValue()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TotalWeight = 300.00m;
			Declaration.JE_ECI_InvoiceAmount = 299.45m;
			Container.CO_Weight = 300.00m;
			AssertEquals(299.45m, Container.ECI_ApportionedContainerisedGoodsValue);
		}

		public void TestTSWSeaJobSealingPartySetByDepotOrg()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_FullName = "Test Depot Auckland NZ";
			var depotAddress1 = depot.Addresses.AddNew();
			depotAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			var depotSEPCode = depot.CustomsCodes.AddNew();
			depotSEPCode.OK_CodeType = OrgCusCode.NZCodeTypes.SecureExportPartner;
			depotSEPCode.OK_RN_NKCodeCountry = "NZ";
			depotSEPCode.OK_CustomsRegNo = "SEP1234";

			Declaration.DepotDocAddress.OrganisationPK = depot.PK;
			AssertEquals("JE_SEPOtherInfoValue should have been populated", "SEP1234", Declaration.JE_SEPOtherInfoValue);
			AssertNotNull("GetOtherInfo(SecureExportPartnership)", Declaration.OtherInfos.GetElementWithThisCode(HeaderOtherInfoList.Codes.SecureExportPartnership));
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertEquals("Sealing Party Name should default", "Test Depot Auckland NZ", Container.CO_SealingParty);

			var depotAddress2 = depot.Addresses.AddNew();
			depotAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			Container.CO_SealingParty = "Another Depot";
			Declaration.DepotDocAddress.E2_OA_Address = depotAddress2.PK;
			AssertEquals("Overriden Sealing Party should not be re-overriden from Depot Organization when simply changing Depot address", "Another Depot", Container.CO_SealingParty);

			var secondDepot = Factory.NewWithValidTestData<OrgHeader>();
			secondDepot.OH_FullName = "KELVIN PACKING AND STORAGE";
			var secondDepotAddress1 = secondDepot.Addresses.AddNew();
			secondDepotAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			var secondDepotSEPCode = secondDepot.CustomsCodes.AddNew();
			secondDepotSEPCode.OK_CodeType = OrgCusCode.NZCodeTypes.SecureExportPartner;
			secondDepotSEPCode.OK_RN_NKCodeCountry = "NZ";
			secondDepotSEPCode.OK_CustomsRegNo = "8938273";
			Declaration.DepotDocAddress.OrganisationPK = secondDepot.PK;
			AssertEquals("Sealing Party Name should re-default when new depot entered", "KELVIN PACKING AND STORAGE", Container.CO_SealingParty);
		}

		#region TestContainerValidation
		public override void TestContainerValidation()
		{
			AssertEquals("Checking CusContainer.Validation", typeof(CusContainerValidation), Container.Validation.GetType());
		}
		#endregion

		#region TestLookupsIsRightType
		public virtual void TestLookupsIsRightType()
		{
			AssertEquals("Checking CusContainer.Lookups", typeof(CusContainerLookups), Container.Lookups.GetType());
		}
		#endregion

		#region TestIsFullContainer
		public void TestIsFullContainer()
		{
			CusContainer container = Factory.New<CusContainer>();
			AssertEquals("Container.IsFullContainer", false, container.IsFullContainer);
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Bulk;
			AssertEquals("Container.IsFullContainer", false, container.IsFullContainer);
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertEquals("Container.IsFullContainer", false, container.IsFullContainer);
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertEquals("Container.IsFullContainer", true, container.IsFullContainer);
			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.LCL;
			AssertEquals("Container.IsFullContainer", false, container.IsFullContainer);
		}
		#endregion

		#region TestContainerNumberIsPalletNumber
		public void TestContainerNumberIsPalletNumber()
		{
			CusContainer container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "";
			AssertEquals("Container Number " + container.CO_ContainerNumber + " Is Pallet Number", false, container.ContainerNumberIsValidPalletNumber());
			container.CO_ContainerNumber = "P";
			AssertEquals("Container Number " + container.CO_ContainerNumber + " Is Pallet Number", false, container.ContainerNumberIsValidPalletNumber());
			container.CO_ContainerNumber = "P1";
			AssertEquals("Container Number " + container.CO_ContainerNumber + " Is Pallet Number", true, container.ContainerNumberIsValidPalletNumber());
			container.CO_ContainerNumber = "P23";
			AssertEquals("Container Number " + container.CO_ContainerNumber + " Is Pallet Number", true, container.ContainerNumberIsValidPalletNumber());
			container.CO_ContainerNumber = "P2D";
			AssertEquals("Container Number " + container.CO_ContainerNumber + " Is Pallet Number", false, container.ContainerNumberIsValidPalletNumber());
		}
		#endregion

		public void TestStuffingEstablishmentAddress()
		{
			AssertEquals("StuffingEstablishment - default container details", null, Container.StuffingEstablishmentAddress);
			var packingOrg = Factory.NewWithValidTestData<OrgAddress>();
			Container.CO_OA_PackingLocation = packingOrg.PK;
			AssertEquals("StuffingEstablishment", packingOrg, Container.StuffingEstablishmentAddress);
		}

		public override void TestDeleteRemovesLinkedPackingGroups()
		{
			var testDec = GetJobDeclaration();
			var container1 = testDec.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;
			var container2 = testDec.CusContainers.AddNew();
			var container3 = testDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OCLU7654321";
			var container4 = testDec.CusContainers.AddNew();

			var houseBill1 = testDec.Bills.AddNew();
			var packGroup = houseBill1.PackingGroups.AddNew();
			packGroup.CR_CO_Container = container1.PK;

			var houseBill2 = testDec.Bills.AddNew();
			var packGroup2 = houseBill2.PackingGroups.AddNew();
			packGroup2.CR_CO_Container = container2.PK;

			var houseBill3 = testDec.Bills.AddNew();
			var packGroup3 = houseBill3.PackingGroups.AddNew();
			packGroup3.CR_CO_Container = container1.PK;

			var houseBill4 = testDec.Bills.AddNew();
			var packGroup4 = houseBill4.PackingGroups.AddNew();
			packGroup4.CR_CO_Container = container3.PK;

			var houseBill5 = testDec.Bills.AddNew();
			var packGroup5 = houseBill5.PackingGroups.AddNew();
			packGroup5.CR_CO_Container = container4.PK;

			container1.Delete();
			container3.Delete();
			container4.Delete();
			AssertEquals("PackGroup should have been detached", ZGuid.Empty, packGroup.CR_CO_Container);
			AssertEquals("PackGroup2 should not be deleted", container2.PK, packGroup2.CR_CO_Container);
			AssertEquals("PackGroup3 should have been deleted", ZGuid.Empty, packGroup3.CR_CO_Container);
			AssertEquals("PackGroup4 should have been deleted", ZGuid.Empty, packGroup4.CR_CO_Container);
			AssertEquals("PackGroup5 should have been deleted", ZGuid.Empty, packGroup5.CR_CO_Container);
		}

		public override void TestDeleteRemovesLinkedPackingGroupsWithJobContainer()
		{
			var consol = Factory.New<Freight.Forwarding.Business.ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234567";

			//Set up CusContainer with same data
			var shipment = (Freight.Business.CommonShipment)consol.Shipments.AddNew();
			var testDec = GetJobDeclaration();
			testDec.JE_TransportMode = testDec.TransportModeSeaCodeForTesting;
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			testDec.JE_JS = shipment.PK;

			var cusContainer = testDec.CusContainers.AddNew();
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			cusContainer.CO_ContainerNumber = "CONT1234567";
			cusContainer.CO_JC = container.PK;

			var houseBill1 = testDec.Bills.AddNew();
			var packGroup = houseBill1.PackingGroups.AddNew();
			packGroup.CR_CO_Container = cusContainer.PK;

			var houseBill2 = testDec.Bills.AddNew();
			var packGroup2 = houseBill2.PackingGroups.AddNew();
			packGroup2.CR_CO_Container = cusContainer.PK;

			Factory.Save();
			cusContainer.Delete();

			AssertEquals(true, cusContainer.IsDeleted);
			AssertEquals("Deleting CusContainer should *not* delete a JobContainer that has a Consol attached.", false, container.IsDeleted);
			AssertEquals(false, packGroup.IsDeleted);
			AssertEquals(false, packGroup2.IsDeleted);
			AssertEquals(ZGuid.Empty, packGroup.CR_CO_Container);
			AssertEquals(ZGuid.Empty, packGroup2.CR_CO_Container);
		}

		public void TestDefaultValuesFromContainerType()
		{
			Container.CO_RC = ZGuid.NewZGuid();
			AssertEquals(ZString.Empty, Container.CO_MAF_ContainerType);
			AssertEquals(ZString.Empty, Container.CO_ContainerSize);

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ISOEquipmentSizeTypeCode = "12345";
			var refContainerCodeMap = refContainer.CodeMapCollection.AddNew();
			refContainerCodeMap.RCM_RN_NKCountry = "NZ";
			refContainerCodeMap.RCM_Code = "54321";
			Container.CO_RC = refContainer.PK;
			AssertEquals("5432", Container.CO_MAF_ContainerType);
			AssertEquals("54", Container.CO_ContainerSize);
		}

		#region Implementation
		#region Container
		protected CusContainer Container
		{
			get
			{
				if (fContainer == null)
				{
					fContainer = Declaration.CusContainers.AddNew();
				}
				return fContainer;
			}
		}
		CusContainer fContainer;
		#endregion

		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = (JobDeclaration)GetJobDeclaration();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion

		#region GetJobDeclaration
		protected override BaseJobDeclaration GetJobDeclaration(BusinessObjectFactory factory)
		{
			JobDeclaration result = factory.New<JobDeclaration>();
			result.DisableDefaultPackingInformation = true;
			return result;
		}
		#endregion
		#endregion
	}
}
