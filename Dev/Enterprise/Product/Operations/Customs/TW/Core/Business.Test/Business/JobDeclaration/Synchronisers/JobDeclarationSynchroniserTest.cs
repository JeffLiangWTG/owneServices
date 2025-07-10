using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobDeclarationSynchroniser))]
	sealed class JobDeclarationSynchroniserTest : Customs.Business.Testing.JobDeclarationSynchroniserTest
	{
		public void TestLoadAndDischargeSyncroniseToCorrectTransport_WithoutConsol_Export()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "TWTPE";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_E_DEP = new ZDateTime(2023, 7, 5, 16, 12, 0);
			shipment.JS_E_ARV = new ZDateTime(2023, 7, 5, 16, 12, 0);

			var declaration = (JobDeclaration)GetJobDeclaration();
			var firstInternationalLegTransport = shipment.Transports.AddNew();
			firstInternationalLegTransport.JW_RL_NKLoadPort = "TWTPE";
			firstInternationalLegTransport.JW_RL_NKDiscPort = "JPHIU";
			firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
			firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
			firstInternationalLegTransport.JW_LegOrder = 1;

			var secondInternationalLegTransport = shipment.Transports.AddNew();
			secondInternationalLegTransport.JW_RL_NKLoadPort = "JPHIU";
			secondInternationalLegTransport.JW_RL_NKDiscPort = "AUSYD";
			secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(7);
			secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(8);
			secondInternationalLegTransport.JW_LegOrder = 2;

			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Precondition: Declaration.JE_RL_NKPortOfLoading", declaration.JE_RL_NKPortOfLoading);
				AssertNullOrEmpty("Precondition: Declaration.JE_RL_NKPortOfArrival", declaration.JE_RL_NKPortOfArrival);
			});

			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(true);

			CombineAssertions(() =>
			{
				AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", "TWTPE", declaration.JE_RL_NKPortOfLoading);
				AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", "JPHIU", declaration.JE_RL_NKPortOfArrival);
				AssertEquals("JE_ExportDate is that of the first international leg's departure", ZDateTime.BrettsBirthday, declaration.JE_ExportDate);
				AssertEquals("JE_DateOfArrival is that of the last international leg's arrival", ZDateTime.BrettsBirthday.AddDays(1), declaration.JE_DateOfArrival);
			});
		}

		public void TestLoadAndDischargeSyncroniseToCorrectTransport_WithoutConsol_Import()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "TWKEL";
			shipment.JS_E_DEP = new ZDateTime(2023, 7, 5, 16, 12, 0);
			shipment.JS_E_ARV = new ZDateTime(2023, 7, 5, 16, 12, 0);

			var declaration = (JobDeclaration)GetJobDeclaration();
			var firstInternationalLegTransport = shipment.Transports.AddNew();
			firstInternationalLegTransport.JW_RL_NKLoadPort = "AUSYD";
			firstInternationalLegTransport.JW_RL_NKDiscPort = "JPHIU";
			firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
			firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
			firstInternationalLegTransport.JW_LegOrder = 1;

			var secondInternationalLegTransport = shipment.Transports.AddNew();
			secondInternationalLegTransport.JW_RL_NKLoadPort = "JPHIU";
			secondInternationalLegTransport.JW_RL_NKDiscPort = "TWKEL";
			secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(7);
			secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(8);
			secondInternationalLegTransport.JW_LegOrder = 2;

			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			declaration.JE_RL_NKPortOfLoading = "";
			declaration.JE_RL_NKPortOfArrival = "";

			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(true);

			CombineAssertions(() =>
			{
				AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", "JPHIU", declaration.JE_RL_NKPortOfLoading);
				AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", "TWKEL", declaration.JE_RL_NKPortOfArrival);
				AssertEquals("JE_ExportDate is that of the first international leg's departure", ZDateTime.BrettsBirthday.AddDays(7), declaration.JE_ExportDate);
				AssertEquals("JE_DateOfArrival is that of the last international leg's arrival", ZDateTime.BrettsBirthday.AddDays(8), declaration.JE_DateOfArrival);
			});
		}

		public void TestHookSupplierAndImporter()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorDocumentaryAddress = Shipment.ConsignorDocumentaryAddress;
			consignorDocumentaryAddress.OrganisationPK = consignor.PK;
			consignorDocumentaryAddress.E2_AddressOverride = true;
			consignorDocumentaryAddress.E2_Address2 = "consignor address 2";
			consignorDocumentaryAddress.E2_AdditionalAddressInformation = "consignor additional address information";
			consignorDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			consignorDocumentaryAddress.E2_City = "C1";
			consignorDocumentaryAddress.E2_State = "S1";
			consignorDocumentaryAddress.E2_Phone = "+86987324348";
			consignorDocumentaryAddress.E2_Email = "xx1@gmail.com";
			consignorDocumentaryAddress.E2_CompanyName = "wisetech global 1";
			consignorDocumentaryAddress.E2_Contact = "contact 1";
			consignorDocumentaryAddress.E2_Postcode = "105";
			var supplierDocumentaryAddress = Declaration.SupplierDocumentaryAddress;
			AssertEquals(true, supplierDocumentaryAddress.E2_AddressOverride);
			AssertEquals("consignor address 2", supplierDocumentaryAddress.E2_Address2);
			AssertEquals("consignor additional address information", supplierDocumentaryAddress.E2_AdditionalAddressInformation);
			AssertEquals("TW", supplierDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("C1", supplierDocumentaryAddress.E2_City);
			AssertEquals("S1", supplierDocumentaryAddress.E2_State);
			AssertEquals("+86987324348", supplierDocumentaryAddress.E2_Phone);
			AssertEquals("xx1@gmail.com", supplierDocumentaryAddress.E2_Email);
			AssertEquals("wisetech global 1", supplierDocumentaryAddress.E2_CompanyName);
			AssertEquals("contact 1", supplierDocumentaryAddress.E2_Contact);
			AssertEquals("105", supplierDocumentaryAddress.E2_Postcode);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeDocumentaryAddress = Shipment.ConsigneeDocumentaryAddress;
			consigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			consigneeDocumentaryAddress.E2_AddressOverride = true;
			consigneeDocumentaryAddress.E2_Address2 = "consignee address 2";
			consigneeDocumentaryAddress.E2_AdditionalAddressInformation = "consignee additional address information";
			consigneeDocumentaryAddress.E2_RN_NKCountryCode = "US";
			consigneeDocumentaryAddress.E2_City = "C2";
			consigneeDocumentaryAddress.E2_State = "S2";
			consigneeDocumentaryAddress.E2_Phone = "+86987324349";
			consigneeDocumentaryAddress.E2_Email = "xx2@gmail.com";
			consigneeDocumentaryAddress.E2_CompanyName = "wisetech global 2";
			consigneeDocumentaryAddress.E2_Contact = "contact 2";
			consigneeDocumentaryAddress.E2_Postcode = "108";
			var importerDocumentaryAddress = Declaration.ImporterDocumentaryAddress;
			AssertEquals(true, importerDocumentaryAddress.E2_AddressOverride);
			AssertEquals("consignee address 2", importerDocumentaryAddress.E2_Address2);
			AssertEquals("consignee additional address information", importerDocumentaryAddress.E2_AdditionalAddressInformation);
			AssertEquals("US", importerDocumentaryAddress.E2_RN_NKCountryCode);
			AssertEquals("C2", importerDocumentaryAddress.E2_City);
			AssertEquals("S2", importerDocumentaryAddress.E2_State);
			AssertEquals("+86987324349", importerDocumentaryAddress.E2_Phone);
			AssertEquals("xx2@gmail.com", importerDocumentaryAddress.E2_Email);
			AssertEquals("wisetech global 2", importerDocumentaryAddress.E2_CompanyName);
			AssertEquals("contact 2", importerDocumentaryAddress.E2_Contact);
			AssertEquals("108", importerDocumentaryAddress.E2_Postcode);
		}

		protected override bool ShouldIgnoreInfoForDetection(ZPropertyInfo info)
		{
			switch (info.Name)
			{
				case JobDocAddress.Schema.E2_AddressOverride:
				case JobDocAddress.Schema.E2_CompanyName:
				case JobDocAddress.Schema.E2_Address1:
				case JobDocAddress.Schema.E2_Address2:
				case JobDocAddress.Schema.E2_AdditionalAddressInformation:
				case JobDocAddress.Schema.E2_RN_NKCountryCode:
				case JobDocAddress.Schema.E2_City:
				case JobDocAddress.Schema.E2_Postcode:
				case JobDocAddress.Schema.E2_State:
				case JobDocAddress.Schema.E2_Contact:
				case JobDocAddress.Schema.E2_Email:
				case JobDocAddress.Schema.E2_Phone:
					return true;
				default:
					return base.ShouldIgnoreInfoForDetection(info);
			}
		}

		public override void TestJobDeclarationSynchroniser()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			Shipment.JS_RS_NKServiceLevel = "XYZ";
			AssertEquals("Declaration.JE_RS_NKServiceLevel", "XYZ", Declaration.JE_RS_NKServiceLevel);
			Shipment.JS_TransportMode = "";
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Declaration.JE_TransportMode", Enterprise.Core.Constants.TransportModes.Sea, Declaration.JE_TransportMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertNotEquals("Declaration.JE_ContainerMode", Core.Constants.ContainerModes.NonContainerised, Declaration.JE_ContainerMode);
			var testConsignor = GetOverseasConsignor();
			Shipment.ConsignorPK = testConsignor.PK;
			AssertEquals("Declaration.JE_OH_Supplier", testConsignor.PK, Declaration.JE_OH_Supplier);
			var testConsignee = GetLocalConsignee();
			Shipment.ConsigneePK = testConsignee.PK;
			AssertEquals("Declaration.JE_OH_Importer", testConsignee.PK, Declaration.JE_OH_Importer);
			Transport.JW_ATA = ZDateTime.Today.AddDays(3);
			AssertEquals("Declaration.JE_DateOfArrival", Consol.JK_JX_JB_A_ARV, Declaration.JE_DateOfArrival);
			Transport.JW_ATD = ZDateTime.Today.AddDays(1);
			AssertEquals("Declaration.JE_ExportDate", Consol.JK_JX_JA_A_DEP, Declaration.JE_ExportDate);
			Transport.JW_Vessel = "12345";
			AssertEquals("Declaration.JE_VesselName", Consol.JK_JX_JV_NKVessel, Declaration.JE_VesselName);
			Shipment.JS_HouseBill = TestHouseBillNumber;
			AssertEquals("Declaration.JE_HouseBill", TestHouseBillNumber, Declaration.JE_HouseBill);
			Shipment.JS_ActualWeight = 3210.0m;
			AssertEquals("Declaration.JE_TotalWeight", 3210.0m, Declaration.JE_TotalWeight);
			Shipment.JS_UnitOfWeight = Enterprise.Core.Constants.Weight.Pounds;
			AssertEquals("Declaration.JE_TotalWeightUnit", Enterprise.Core.Constants.Weight.Pounds, Declaration.JE_TotalWeightUnit);
			Shipment.JS_ActualVolume = 13.2m;
			AssertEquals("Declaration.JE_TotalVolume", 13.2m, Declaration.JE_TotalVolume);
			Shipment.JS_UnitOfVolume = Enterprise.Core.Constants.Volume.CubicFeet;
			AssertEquals("Declaration.JE_TotalVolumeUnit", Enterprise.Core.Constants.Volume.CubicFeet, Declaration.JE_TotalVolumeUnit);
			Shipment.JS_OuterPacks = 12;
			AssertEquals("Declaration.JE_TotalNoOfPacks", 12, Declaration.JE_TotalNoOfPacks);
			Shipment.JS_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Box;
			Shipment.JS_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Pallet;
			AssertEquals("Declaration.JE_TotalNoOfPacksPackType", Enterprise.Core.Constants.PkgUnit.Pallet, Declaration.JE_TotalNoOfPacksPackType);
			Shipment.JS_GoodsDescription = TestGoodsDescription;
			AssertEquals("Declaration.JE_GoodsDescription", TestGoodsDescription, Declaration.JE_GoodsDescription);
			Shipment.JS_E_ARV = ZDateTime.Today.AddDays(1);
			AssertEquals("Declaration.JE_DateOfFirstArrival", Shipment.JS_E_ARV, Declaration.JE_DateAtFinalDestination);
			Shipment.JS_E_DEP = ZDateTime.Today.AddDays(4);
			AssertEquals("Declaration.JE_DateAtOrigin", Shipment.JS_E_DEP, Declaration.JE_DateAtOrigin);
		}

		public override void TestSeaTransportModeFormatting()
		{
			Assert("Should not Synchronise JE_ContainerMode with JS_PackingMode in TW", true);
		}

		public override void TestSeaTransportModeFormattingForImport()
		{
			Assert("Should not Synchronise JE_ContainerMode with JS_PackingMode in TW", true);
		}

		public override void TestShouldSynchroniseContainerModeWithPackingMode()
		{
			var synchroniser = new JobDeclarationSynchroniserForTest(Declaration);
			Assert("ShouldSynchroniseContainerModeWithPackingMode should be false", !synchroniser.ShouldSynchroniseContainerModeWithPackingMode);
		}

		public void TestShouldNotSynchroniseContainerModeWithPackingMode()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			Declaration.JE_ContainerMode = "X";
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			AssertEquals("JE_ContainerMode", "X", Declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Liquid;
			AssertEquals("JE_ContainerMode", "X", Declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("JE_ContainerMode", "X", Declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("JE_ContainerMode", "X", Declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("JE_ContainerMode", "X", Declaration.JE_ContainerMode);
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Loose;
			AssertEquals("JE_ContainerMode", "CNT", Declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.ULD;
			AssertEquals("JE_ContainerMode", "CNT", Declaration.JE_ContainerMode);
			Assert("ContainerMode should be editable", !Declaration.JE_ContainerModeInfo.ReadOnly);
		}

		public void TestPackingSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRBUS";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.Shipments.Add(shipment);
			consol.JK_MasterBillNum = "M1";
			var forwardingContainer1 = consol.Containers.AddNew();
			forwardingContainer1.JC_ContainerNum = "C1";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_JC = forwardingContainer1.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			var synchroniser = new JobDeclarationSynchroniserForTest(declaration);
			AssertType<PackingSynchroniser>(synchroniser.PackingSynchroniser);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWPUM", "Taiwan Packing Units of Measurement");
			helper.CreateCusCodeList("TW", "TWPUM", "PLT", "PLT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", "TWPUM", "BOX", "BOX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		public JobDeclaration Declaration => base.declaration as JobDeclaration;
		class JobDeclarationSynchroniserForTest : JobDeclarationSynchroniser
		{
			public JobDeclarationSynchroniserForTest(JobDeclaration destination) : base(destination)
			{
			}

			public Customs.Business.PackingSynchroniser PackingSynchroniser => base.GetPackingSynchroniser();
			public new ZBool ShouldSynchroniseContainerModeWithPackingMode => base.ShouldSynchroniseContainerModeWithPackingMode;
		}
	}
}
