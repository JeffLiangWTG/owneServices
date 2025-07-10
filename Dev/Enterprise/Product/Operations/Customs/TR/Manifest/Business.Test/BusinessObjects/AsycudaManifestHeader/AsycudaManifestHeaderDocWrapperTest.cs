using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderDocWrapperTest : TestCaseWithFactory
	{
		public const string TRManifestBills = "TR Manifest Bills";
		public const string TRManifestBillsLines = "TR Manifest Bills Lines";
		public const string TRManifestBillsforEMANIF = "TR Manifest Bills for EMANIF";
		public void TestPropertiesForDocWrapper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_Carrier = CarrierAgent.PK;
			var glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			glbCompany.CompanyName = "TestName";
			glbCompany.GC_BusinessRegNo = "test567890";
			glbCompany.Branches.Add(glbBranch);
			header.AMA_GB = glbBranch.PK;
			Factory.Save();
			var testWrapper = new AsycudaManifestHeaderDocWrapper(header);
			CombineAssertions("Empty Data", () =>
			{
				AssertEquals("TotalContainers", (ZInt)0, testWrapper.TotalContainers);
				AssertEquals("TotalABLManifestQty", (ZDecimal)0, testWrapper.TotalABLManifestQty);
			});
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			var container2 = header.Containers.AddNew();
			var pack1 = bill1.Packs.AddNew();
			var pack2 = bill1.Packs.AddNew();
			var packedItem1 = pack1.PackedItems.AddNewPackedItem();
			var packedItem2 = pack1.PackedItems.AddNewPackedItem();
			var packedItem3 = pack2.PackedItems.AddNewPackedItem();
			bill1.ABL_ManifestQty = 5;
			bill2.ABL_ManifestQty = 10;
			bill1.ABL_GrossWeight = 50;
			bill2.ABL_GrossWeight = 200;
			bill1.ABL_GrossWeightUQ = Constants.Weight.Kilograms;
			bill2.ABL_GrossWeightUQ = Constants.Weight.Kilograms;
			pack1.APA_Weight = 50;
			pack2.APA_Weight = 200;
			pack1.APA_WeightUQ = Constants.Weight.Kilograms;
			pack2.APA_WeightUQ = Constants.Weight.Kilograms;
			packedItem1.API_GrossWeight = 20;
			packedItem2.API_GrossWeight = 30;
			packedItem3.API_GrossWeight = 20;
			packedItem1.API_GrossWeightUQ = Constants.Weight.Kilograms;
			packedItem2.API_GrossWeightUQ = Constants.Weight.Kilograms;
			packedItem3.API_GrossWeightUQ = Constants.Weight.Kilograms;
			Factory.Save();
			CombineAssertions("With Data", () =>
			{
				AssertEquals("TST0001", testWrapper.RegNoOfAgentCarrier);
				AssertEquals(GlbCompany.CurrentCompany.GC_BusinessRegNo, testWrapper.RegNoOfDeclarant);
				AssertEquals("TotalContainers", 2, testWrapper.TotalContainers);
				AssertEquals("TotalABLManifestQty", 15m, testWrapper.TotalABLManifestQty);
				AssertEquals("TotalGrossWeight", "70.00", testWrapper.TotalGrossWeight);
			});
		}

		public void TestNew()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var wrapper = AsycudaManifestHeaderDocWrapper.New(manifest, TRManifestBills, "");
			AssertEquals(manifest, wrapper.Manifest);
			var wrapper2 = AsycudaManifestHeaderDocWrapper.New(manifest, TRManifestBillsLines, "");
			AssertEquals(manifest, wrapper2.Manifest);
			var wrapper3 = AsycudaManifestHeaderDocWrapper.New(manifest, TRManifestBillsforEMANIF, "");
			AssertEquals(manifest, wrapper3.Manifest);
		}

		public void TestTotalWeightOfPackedItemsInKGANDPackWeightInKG()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = "VARONC";
			manifest.AMA_RN_NKCountry = Constants.CountryCodes.Turkey;
			manifest.AMA_TransportMode = Constants.TransportModes.Sea;
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_Weight = 5000;
			pack.APA_WeightUQ = Constants.Weight.Grams;
			var packedItem1 = pack.PackedItems.AddNewPackedItem();
			var packedItem2 = pack.PackedItems.AddNewPackedItem();
			packedItem1.API_NetWeight = 49;
			packedItem1.API_NetWeightUQ = Constants.Weight.Kilograms;
			packedItem2.API_NetWeight = 20;
			packedItem2.API_NetWeightUQ = Constants.Weight.Kilograms;
			Factory.Save();
			AssertEquals((ZShort)1, bill.ABL_SequenceNumber);
			var apaWeightInkg = Core.Constants.Weight.ConvertSafe(pack.APA_Weight, pack.APA_WeightUQ, Constants.Weight.Kilograms);
			AssertEquals(apaWeightInkg, pack.PackWeightInKG);
			var packedItems = manifest.Bills.Cast<AsycudaBill>().SelectMany(b => b.Packs.Cast<AsycudaPack>().SelectMany(p => p.PackedItems.Cast<ManifestBase.AsycudaPackPackedItemPivot>().Select(x => x.PackedItem)));
			ZDecimal total = 0;
			foreach (var packedItemsLine in packedItems)
			{
				total += Core.Constants.Weight.ConvertSafe(packedItemsLine.API_NetWeight, packedItemsLine.API_NetWeightUQ, Constants.Weight.Kilograms);
			}

			AssertEquals(total, pack.TotalWeightOfPackedItemsInKG);
			var invalidWeightConvert = Core.Constants.Weight.ConvertSafe(pack.APA_Weight, "2B", Constants.Weight.Kilograms);
			AssertEquals(ZDecimal.Zero, invalidWeightConvert);
			invalidWeightConvert = Core.Constants.Weight.ConvertSafe(pack.APA_Weight, Constants.Weight.Kilograms, "2B");
			AssertEquals(ZDecimal.Zero, invalidWeightConvert);
		}

		public void TestContainerInformationAND()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = "VARONC";
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			var container = manifest.Containers.AddNew();
			container.ACN_ContainerNumber = "ABCD 123456-7";
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			Factory.Save();
			AssertEquals("ABCD 123456-7", container.ACN_ContainerNumber);
			AssertEquals("E", bill.ContainerInformation);
			pack.Container.Delete();
			AssertEquals("H", bill.ContainerInformation);
		}

		public void TestRegNoOfAgentANDCompanyNameOfAgent()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = "VARONC";
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			var container = manifest.Containers.AddNew();
			container.ACN_ContainerNumber = "ABCD 123456-7";
			var bill = manifest.Bills.AddNew();
			bill.ABL_OA_ContainerAgent = ContainerAgent.PK;
			AssertEquals("ULUKOM", bill.CompanyNameOfAgent);
			AssertEquals("TST0001", bill.RegNoOfAgent);
		}

		public void TestPresentationCustomsOfficeDescription()
		{
			SetData();
			var wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals("İstanbul Gümrük Dairesi", wrapper.PresentationCustomsOfficeDescription);
		}

		public void TestLoadAndDischargePorts()
		{
			SetData();
			header.AMA_Nature = "IMP";
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "TRIST";
			header.AMA_CustomsDischargePort = "TRIST-001";
			var wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals(wrapper.LoadPortNameForCarrierManifest, "Singapore");
			AssertEquals(wrapper.DischargePortNameForCarrierManifest, "İstanbul Limanı 1");
			header.AMA_Nature = "EXP";
			header.AMA_RL_NKPortOfLoading = "TRIZM";
			header.AMA_CustomsLoadPort = "TRIZM-001";
			header.AMA_RL_NKPortOfDischarge = "SGSIN";
			header.AMA_CustomsDischargePort = "";
			wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals(wrapper.LoadPortNameForCarrierManifest, "İzmir Limanı 1");
			AssertEquals(wrapper.DischargePortNameForCarrierManifest, "Singapore");
		}

		public void TestLoadAndDischargePortsUnlocoDescriptions()
		{
			SetData();
			header.AMA_Nature = "EXP";
			header.AMA_CustomsDischargePort = "TRIST";
			header.AMA_CustomsLoadPort = "TRIZM-001";
			header.AMA_RL_NKPortOfDischarge = ZString.Empty;
			header.AMA_RL_NKPortOfLoading = ZString.Empty;
			var wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals(wrapper.DischargePortNameForCarrierManifest, "Istanbul");
			AssertEquals(wrapper.LoadPortNameForCarrierManifest, "İzmir Limanı 1");
			header.AMA_CustomsDischargePort = "TRISX";
			header.AMA_CustomsLoadPort = "TRIZM-00X";
			wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals(wrapper.DischargePortNameForCarrierManifest, ZString.Empty);
			AssertEquals(wrapper.LoadPortNameForCarrierManifest, ZString.Empty);
			header.AMA_CustomsDischargePort = ZString.Empty;
			header.AMA_CustomsLoadPort = ZString.Empty;
			header.AMA_RL_NKPortOfDischarge = "TRIST";
			header.AMA_RL_NKPortOfLoading = "TRIZM";
			wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals(wrapper.DischargePortNameForCarrierManifest, "Istanbul");
			AssertEquals(wrapper.LoadPortNameForCarrierManifest, "Izmir");
			header.AMA_Nature = "IMP";
			header.AMA_CustomsDischargePort = "TRIST-001";
			header.AMA_CustomsLoadPort = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = ZString.Empty;
			header.AMA_RL_NKPortOfLoading = ZString.Empty;
			header.AMA_Nature = "IMP";
			wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals(wrapper.DischargePortNameForCarrierManifest, "İstanbul Limanı 1");
			AssertEquals(wrapper.LoadPortNameForCarrierManifest, "Singapore");
			header.AMA_CustomsDischargePort = "TRIST-00X";
			header.AMA_CustomsLoadPort = "SGSIX";
			wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals(wrapper.DischargePortNameForCarrierManifest, ZString.Empty);
			AssertEquals(wrapper.LoadPortNameForCarrierManifest, ZString.Empty);
			header.AMA_CustomsDischargePort = ZString.Empty;
			header.AMA_CustomsLoadPort = ZString.Empty;
			header.AMA_RL_NKPortOfDischarge = "TRIST";
			header.AMA_RL_NKPortOfLoading = "TRIZM";
			wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals(wrapper.DischargePortNameForCarrierManifest, "Istanbul");
			AssertEquals(wrapper.LoadPortNameForCarrierManifest, "Izmir");
		}

		public void TestTRConveyanceNationalityAndName()
		{
			SetData();
			var wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals("004", wrapper.ConveyanceTRMappedNationality);
			AssertEquals("Germany", wrapper.ConveyanceTRMappedNationalityName);
		}

		public void TestNoExceptionPackedItemDocWrappers()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = "VARONC";
			manifest.AMA_RN_NKCountry = Constants.CountryCodes.Turkey;
			manifest.AMA_TransportMode = Constants.TransportModes.Sea;
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_Weight = 5000;
			pack.APA_WeightUQ = Constants.Weight.Grams;
			var packedItem1 = pack.PackedItems.AddNewPackedItem();
			var packedItem2 = pack.PackedItems.AddNewPackedItem();
			packedItem1.API_NetWeight = 49;
			packedItem1.API_NetWeightUQ = Constants.Weight.Kilograms;
			packedItem2.API_NetWeight = 20;
			packedItem2.API_NetWeightUQ = Constants.Weight.Kilograms;

			var wrapper = new AsycudaManifestHeaderDocWrapper(manifest);
			AssertNoExceptionThrown(() => GetPackedItemDocWrappers(wrapper));
			var packedItemDocWrappers = wrapper.PackedItemDocWrappers;
			AssertEquals(2, packedItemDocWrappers.Count);
		}

		public void TestNoExceptionBillDocWrappers()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_ManifestType = "VARONC";
			manifest.AMA_RN_NKCountry = Constants.CountryCodes.Turkey;
			manifest.AMA_TransportMode = Constants.TransportModes.Sea;
			var bill1 = manifest.Bills.AddNew();
			var bill2 = manifest.Bills.AddNew();

			var wrapper = new AsycudaManifestHeaderDocWrapper(manifest);
			AssertNoExceptionThrown(() => GetBillDocWrappers(wrapper));
			var packedItemDocWrappers = wrapper.PackedItemDocWrappers;
			AssertEquals(2, packedItemDocWrappers.Count);
		}

		Customs.Business.DocumentWrappers.BusinessObjectCollectionWrapper<TRCarrierManifestItemWrapper> GetPackedItemDocWrappers(AsycudaManifestHeaderDocWrapper wrapper) => wrapper.PackedItemDocWrappers;

		Customs.Business.DocumentWrappers.BusinessObjectCollectionWrapper<AsycudaBillDocWrapper> GetBillDocWrappers(AsycudaManifestHeaderDocWrapper wrapper) => wrapper.BillDocWrappers;

		public void TestTestESignatureOwnerWhoSignsMessageWithEmptyRegNo()
		{
			SetData();
			SetUserInfo();

			var wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");

			AssertEquals(ZString.Empty, wrapper.DeclarationOwnerFullName);
		}

		public void TestESignatureOwnerWhoSignsMessage()
		{
			SetData();
			SetUserInfo();
			header.RegistrationNumber = "23066666IM000070";
			header.RegistrationDate = ZDateTime.Now;

			var outgoingTROMessage = CreateEdiMessage(TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit);
			outgoingTROMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 24, 13, 30, 38);
			outgoingTROMessage.EM_MessageOwner = "BP";

			var outgoingTROMessage1 = CreateEdiMessage(TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit);
			outgoingTROMessage1.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 24, 14, 02, 51);
			outgoingTROMessage1.EM_MessageOwner = "WZG";

			var outgoingTROMessage2 = CreateEdiMessage(TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit);
			outgoingTROMessage2.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 24, 15, 43, 34);
			outgoingTROMessage2.EM_MessageOwner = "KNZ";

			var outgoingT1OMessage = CreateEdiMessage(TRMessageTypes.Codes.T1O, EDIMessage.Direction.Transmit);
			outgoingT1OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 17, 04);
			outgoingT1OMessage.EM_MessageOwner = "KNZ";
			var incomingT1OMessage = CreateEdiMessage(TRMessageTypes.Codes.T1O, EDIMessage.Direction.Receive);
			incomingT1OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 24, 36);

			var outgoingT3OMessage = CreateEdiMessage(TRMessageTypes.Codes.T3O, EDIMessage.Direction.Transmit);
			outgoingT3OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 45, 21);
			outgoingT3OMessage.EM_MessageOwner = "AAA";
			var incomingT3OMessage = CreateEdiMessage(TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive);
			incomingT3OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 51, 56);

			var wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");

			AssertEquals("KNZ Testing Signed User", wrapper.DeclarationOwnerFullName);
		}

		public void TestIssuingCarrierAgentName()
		{
			SetData();
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "KUEHNE";
			var wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals("IssuingCarrierAgentName", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName, wrapper.IssuingCarrierAgentName);
		}

		public void TestIssuingCarrierAgentIATACode()
		{
			SetData();

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/999";
			var wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals("IssuingCarrierAgentIATACode", string.Empty, wrapper.IssuingCarrierAgentIATACode);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			AssertEquals("IssuingCarrierAgentIATACode", "1234567/999", wrapper.IssuingCarrierAgentIATACode);
		}
		public void TestShortRegistrationNumber()
		{
			SetData();
			header.RegistrationNumber = "22340300IM12345678";
			var wrapper = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, Menu, "");
			CombineAssertions(() =>
			{
				AssertEquals("Valid Registration", "12345678", wrapper.ShortRegistrationNumber);

				header.RegistrationNumber = ZString.Empty;
				AssertEquals("Empty Registration", ZString.Empty, wrapper.ShortRegistrationNumber);
			});
		}

		EDIMessage CreateEdiMessage(string messageType, string direction)
		{
			var message = Factory.New<TRManifestMessage>();
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkedObject = header;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = messageType;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message.EM_IsTestMessage = true;
			message.EM_SystemCreateUser = "KNZ";
			message.EM_ReceiveTransmit = direction;
			return message;
		}

		void SetUserInfo()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			staff.GS_LoginName = "KNZ";
			staff.GS_FullName = "KNZ Testing Signed User";

			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "BP";
			staff1.GS_LoginName = "BP";
			staff1.GS_FullName = "BP Testing Signed User";

			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "WZG";
			staff2.GS_LoginName = "WZG";
			staff2.GS_FullName = "WZG Testing Signed User";

			var staff3 = group.Staff.AddNew();
			staff3.GS_Code = "AAA";
			staff3.GS_LoginName = "AAA";
			staff3.GS_FullName = "AAA Testing Signed User";

			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var user1 = TRGlbStaffWrapper.Get(staff3).TRBPassword;
			user1.GP_UserID = "20201224199";
			user1.CurrentDecryptedPassword = "12345699";
		}

		protected override void SetUp()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0044", "İstanbul Gümrük Dairesi", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "TRIZM-001", "İzmir Limanı 1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "TRIST-001", "İstanbul Limanı 1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusMapType("CNTRY", "OUT", "CNTRY", true);
			helper.CreateCusMap("CNTRY", Core.Constants.CountryCodes.Germany, "004", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), Core.Constants.CountryCodes.Turkey);
			Factory.Save();
			base.SetUp();
		}

		AsycudaManifestHeader header;
		void SetData()
		{
			var testWeight = new ZDecimal(5.15);
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();
			organization.Addresses[0].Address1 = "test line 1";
			organization.Addresses[0].Address2 = "test line 2";
			organization.Addresses[0].City = "ISTANBUL";
			organization.Addresses[0].Postcode = "34344";
			organization.Addresses[0].OA_RN_NKCountryCode = "TR";
			organization.Addresses[0].State = "34";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			company.CompanyName = "TestName";
			company.Address1 = "Test adress 1";
			company.Address2 = "Test adress 2";
			company.GC_OH_OrgProxy = organization.PK;
			company.Branches.Add(branch);
			vessel.RV_Name = "TestVessel";
			vessel.RV_Code = "TestCode";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Germany;
			header.AMA_GB = branch.PK;
			header.TR_GM_PresentationCustomsOffice = "0044";
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "TRIST";
			header.AMA_CustomsDischargePort = "TRIST-001";
			header.AMA_VesselName = "TestCode";
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill1 = header.Bills.AddNew();
			var pack11 = bill1.Packs.AddNew();
			var pack12 = bill1.Packs.AddNew();
			var item111 = pack11.PackedItems.AddNewPackedItem();
			var item112 = pack11.PackedItems.AddNewPackedItem();
			var item121 = pack12.PackedItems.AddNewPackedItem();
			var item122 = pack12.PackedItems.AddNewPackedItem();
			pack11.APA_PackQty = 1;
			pack12.APA_PackQty = 2;
			item111.API_GrossWeight = testWeight;
			item112.API_GrossWeight = testWeight;
			item121.API_GrossWeight = testWeight;
			item122.API_GrossWeight = testWeight;
			item111.API_GrossWeightUQ = "KG";
			item112.API_GrossWeightUQ = "KG";
			item121.API_GrossWeightUQ = "KG";
			item122.API_GrossWeightUQ = "KG";
			var bill2 = header.Bills.AddNew();
			var pack21 = bill2.Packs.AddNew();
			var pack22 = bill2.Packs.AddNew();
			var item211 = pack21.PackedItems.AddNewPackedItem();
			var item212 = pack21.PackedItems.AddNewPackedItem();
			var item221 = pack22.PackedItems.AddNewPackedItem();
			var item222 = pack22.PackedItems.AddNewPackedItem();
			pack21.APA_PackQty = 3;
			pack22.APA_PackQty = 4;
			item222.API_GrossWeight = testWeight;
			item221.API_GrossWeight = testWeight;
			item212.API_GrossWeight = testWeight;
			item211.API_GrossWeight = testWeight;
			item211.API_GrossWeightUQ = "KG";
			item212.API_GrossWeightUQ = "KG";
			item221.API_GrossWeightUQ = "KG";
			item222.API_GrossWeightUQ = "KG";
			Factory.Save();
		}

		OrgAddress containerAgent;
		OrgAddress ContainerAgent
		{
			get
			{
				if (containerAgent == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_Code = "ORG01";
					containerAgent = org.MainAddress;
					containerAgent.OA_Code = "TC01";
					containerAgent.CompanyName = "ULUKOM";
					containerAgent.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "TST0001");
				}

				return containerAgent;
			}
		}

		OrgAddress carrierAgent;
		OrgAddress CarrierAgent
		{
			get
			{
				if (carrierAgent == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_Code = "ORG01";
					carrierAgent = org.MainAddress;
					carrierAgent.OA_Code = "TC01";
					carrierAgent.CompanyName = "ULUKOM";
					carrierAgent.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "TST0001");
				}

				return carrierAgent;
			}
		}

		const string Menu = "Global Manifest";
	}
}
