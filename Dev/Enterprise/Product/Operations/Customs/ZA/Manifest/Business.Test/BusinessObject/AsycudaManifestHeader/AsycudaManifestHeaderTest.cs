using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillCollection>(header.Bills);
		}

		public void TestAsycudaPackedItemIsNotSupported()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(true, bill.IsNonePackedItemRelationship);
		}

		public void TestGetMasterZZCarrier()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "VWG";
			carrier.ZZ4_CountryOrGrouping = "ZA";
			carrier.ZZ4_Description = "Daniel";
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.MASTER, ZString.Empty);
			carrier.Attributes.AddNew("SEA", "SEA");
			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_Code = "00505655";
			carrier2.ZZ4_CountryOrGrouping = "ZA";
			carrier2.ZZ4_Description = "Daniel";
			carrier2.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, ZString.Empty);
			carrier2.Attributes.AddNew("SEA", "SEA");
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			AssertEquals(carrier.PK, header.GetMasterZZCarrier("VWG", false).PK);
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			AssertEquals(carrier.PK, header.GetMasterZZCarrier("VWG", false).PK);
			header.AMA_AgentType = AgentType.CoLoad;
			AssertEquals(carrier2.PK, header.GetMasterZZCarrier("00505655", false).PK);

			AssertNull("Should only return MASTER carrier", header.GetMasterZZCarrier("00505655", true));
			AssertEquals(carrier.PK, header.GetMasterZZCarrier("VWG", true).PK);
		}

		public void TestCarrierCodeAutoUpdate()
		{
			var carrierOrgAddress = Factory.Load<OrgAddress>(new ZGuid("0EA85FB9-EC2E-4713-8A96-85B6290E97BB"));
			carrierOrgAddress.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "1234", "ZA");
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_CarrierCode = "XXX";
				header.AMA_OA_Carrier = carrierOrgAddress.PK;
				AssertEquals("update to 1234", "1234", header.AMA_CarrierCode);
				header.AMA_CarrierCode = "XXX";
				header.AMA_OA_Carrier = ZGuid.Empty;
				AssertEquals("update to Empty", ZString.Empty, header.AMA_CarrierCode);
			});
		}

		public void TestMasterCarrierCodeAutoUpdate()
		{
			var carrierOrgAddress = Factory.Load<OrgAddress>(new ZGuid("0EA85FB9-EC2E-4713-8A96-85B6290E97BB"));
			carrierOrgAddress.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "1234", "ZA");
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.MasterCarrierCode = "XXX";
				header.AMA_OA_Carrier = carrierOrgAddress.PK;
				AssertEquals("update to 1234", "1234", header.MasterCarrierCode);
				header.MasterCarrierCode = "XXX";
				header.AMA_OA_Carrier = ZGuid.Empty;
				AssertEquals("update to Empty", ZString.Empty, header.MasterCarrierCode);

				header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
				header.AMA_AgentType = ZString.Empty;
				header.MasterCarrierCode = "XXX";
				header.AMA_OA_Carrier = carrierOrgAddress.PK;
				AssertEquals("No update", "1234", header.MasterCarrierCode);

				header.AMA_OA_Carrier = ZGuid.Empty;
				header.MasterCarrierCode = "XXX";
				header.AMA_AgentType = AgentType.CoLoad;
				header.AMA_OA_Carrier = carrierOrgAddress.PK;
				AssertEquals("update to Empty", "", header.MasterCarrierCode);

				header.AMA_OA_Carrier = ZGuid.Empty;
				header.MasterCarrierCode = "XXX";
				header.AMA_OA_ShippingAgent = carrierOrgAddress.PK;
				header.AMA_OA_Carrier = carrierOrgAddress.PK;
				AssertEquals("update to 1234", "1234", header.MasterCarrierCode);

				header.AMA_OA_ShippingAgent = ZGuid.Empty;
				AssertEquals("update to Empty", ZString.Empty, header.MasterCarrierCode);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_UniqueConsignRef = "Con001";
				header.AMA_ParentTableCode = "JK";
				header.AMA_ParentId = consol.PK;
				header.MasterCarrierCode = "XXX";
				header.AMA_OA_ShippingAgent = carrierOrgAddress.PK;
				AssertEquals("update to 1234", "1234", header.MasterCarrierCode);
			});
		}

		public void TestShippingAgentCCCCode()
		{
			var carrierOrgAddress = Factory.Load<OrgAddress>(new ZGuid("0EA85FB9-EC2E-4713-8A96-85B6290E97BB"));
			carrierOrgAddress.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OrgAGT", "ZA");
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			header.AMA_RN_NKCountry = CountryCodes.SouthAfrica;
			header.AMA_OA_ShippingAgent = carrierOrgAddress.PK;

			AssertEquals("OrgAGT", header.ShippingAgentCCCCode);

			carrierOrgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "AddressAGTTW", "TW");
			carrierOrgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "AddressAGTZA", "ZA");

			AssertEquals("AddressAGTZA", header.ShippingAgentCCCCode);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = (AsycudaManifestHeader)GetNewBusinessObject();
			bizObj.AMA_ManifestType = ZaManifestTypes.Codes.HAB;
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestDeptCode()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "VWG", CountryCodes.SouthAfrica);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_DeconsolidateAddress = org.PK;
			AssertEquals("VWG", header.DepotCode);
		}

		public void TestTerminalCode()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "VWG", CountryCodes.SouthAfrica);
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_OA_DischargeTerminalAddress = org.PK;
			AssertEquals("VWG", header.TerminalCode);
		}

		public void TestDefaultGetTypes()
		{
			var header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, CountryCodes.SouthAfrica, ZaManifestTypes.Codes.ALM, ApplicationCodeTypeList.Codes.ShippingLine);
			CombineAssertions(() =>
			{
				AssertEquals("Default Bill Type", typeof(AsycudaBill), header.GetBillType());
				AssertEquals("Default Container Type", typeof(AsycudaContainer), header.GetContainerType());
				AssertEquals("Default Person Type", typeof(CusPerson), header.GetPersonType());
			});
		}

		public void TestDefaultGetConsolSynchronizerCore()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			AssertType<AsycudaManifestHeaderSynchroniser>(manifestHeader.Synchroniser);
		}

		public void TestCaseNumbers()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			for (var i = 1; i < 4; i++)
			{
				var caseNumber = Factory.New<CaseNumber>();
				caseNumber.CY_ParentID = header.PK;
				caseNumber.CY_ParentTableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
			}

			CombineAssertions(() =>
			{
				AssertEquals("Header has 3 linked case numbers loaded", 3, header.CaseNumbers.Count);
				AssertEquals("CaseNumbers Collection is child editable", true, header.IsRegisteredEditableChildObject(header.CaseNumbers));
			});
		}

		public void TestFetchStrategy()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<AsycudaManifestHeaderFetchStrategy>(header.FetchStrategy);
		}
		public void TestGetExtraMessageSendingNotification()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var company1 = Factory.New<GlbCompany>();
				company1.GC_RN_NKCountryCode = CountryCodes.SouthAfrica;
				company1.GC_Code = "DZA";
				var proxy1 = Factory.New<OrgHeader>();
				proxy1.OH_Code = "Proxy1";
				proxy1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "12345", CountryCodes.SouthAfrica);
				var branch1 = Factory.New<GlbBranch>();
				branch1.GB_GC = company1.PK;
				branch1.GB_Code = "JNB";
				branch1.GB_BranchName = "Joburg Corporate Office";
				branch1.GB_OH_OrgProxy = proxy1.PK;
				Factory.Save();

				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_CustomsOffice = "office";
				AssertEquals(ValidationConstants.MustBeLoggedInUnderZaToSendZaMessages, header.MessageSendingNotificationHelper.GetNotifications());

				using (ZACustomsRegistry.Instance.DefaultBranchForManifestSubmission.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "JNB"))
				{
					AssertNotEquals(ValidationConstants.MustBeLoggedInUnderZaToSendZaMessages, header.MessageSendingNotificationHelper.GetNotifications());
				}
			}
		}

		public void TestHasContainers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("HasContainers by default", header.HasContainers);
			header.AMA_ManifestType = ZaManifestTypes.Codes.ALM;
			Assert("HasContainers by default", header.HasContainers);
			header.AMA_ManifestType = ZaManifestTypes.Codes.BBB;
			Assert("BBB should not have containers", !header.HasContainers);
		}

		public void TestHasBillsAndPacks()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("HasBillsAndPacks by default", header.HasBillsAndPacks);
			header.AMA_ManifestType = ZaManifestTypes.Codes.ALM;
			Assert("HasBillsAndPacks by default", header.HasBillsAndPacks);
			header.AMA_ManifestType = ZaManifestTypes.Codes.ECL;
			Assert("ECL should not have containers", !header.HasBillsAndPacks);
		}

		public void TestMasterCarrierCodeDefaulting()
		{
			var carrierOrgAddress = Factory.Load<OrgAddress>(new ZGuid("0EA85FB9-EC2E-4713-8A96-85B6290E97BB"));
			carrierOrgAddress.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "1234", "ZA");
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CarrierCode = "1234";

			AssertEquals("1234", header.MasterCarrierCode);

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_CarrierCode = "1234";
			header2.MasterCarrierCode = "456";
			header2.AMA_CarrierCode = "1234";
			AssertEquals("456", header2.MasterCarrierCode);

			header2.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			header2.AMA_AgentType = AgentType.CoLoad;
			header2.AMA_CarrierCode = "1235";
			header2.MasterCarrierCode = "455";
			header2.AMA_CarrierCode = "1235";
			AssertEquals("455", header2.MasterCarrierCode);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "Con001";
			header2.AMA_ParentTableCode = "JK";
			header2.AMA_ParentId = consol.PK;

			header2.AMA_CarrierCode = "1236";
			header2.MasterCarrierCode = "455";
			header2.AMA_CarrierCode = "1236";
			AssertEquals("455", header2.MasterCarrierCode);
		}

		public void TestSupportAssociatedPacks()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_TransportMode = TransportModes.Road;
			AssertEquals(true, header.SupportAssociatedPacks);
			header.AMA_TransportMode = TransportModes.Sea;
			AssertEquals(false, header.SupportAssociatedPacks);
		}

		public void TestSupportMultipleCustomsNumbers()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_TransportMode = TransportModes.Road;
			AssertEquals(true, header.SupportMultipleCustomsNumbers);
			header.AMA_TransportMode = TransportModes.Sea;
			AssertEquals(false, header.SupportMultipleCustomsNumbers);
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			AssertEquals(true, header.SupportMultipleCustomsNumbers);
		}

		public void TestIsAQM()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			AssertEquals(true, header.IsAQM);

			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.IsAQM);
		}

		public void TestCARN_WhenEntryNumberTypeIsNotAsy()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var entryNum = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.SouthAfrica.CustomsAssignedReferenceNumberCARN, header.AMA_RN_NKCountry, false);
			entryNum.CE_EntryNum = "123";
			AssertEquals("123", header.CARN);

			entryNum.CE_EntryType = "";
			AssertEquals("", header.CARN);
		}

		public void TestPlaceOfEntry()
		{
			var headerZA = Factory.New<AsycudaManifestHeader>();
			headerZA.PlaceOfEntry = "JSA";
			AssertEquals("JSA", headerZA.PlaceOfEntry);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			headerZA = newFactory.Load<AsycudaManifestHeader>(headerZA.PK);
			AssertEquals("JSA", headerZA.PlaceOfEntry);
		}

		public void TestPlaceOfEntryVisible()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportModes.Road;

			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			Assert(header.PlaceOfEntryVisible);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			Assert(!header.PlaceOfEntryVisible);

			header.AMA_TransportMode = TransportModes.Rail;
			Assert(!header.PlaceOfEntryVisible);
		}

		public void TestEstimatedTimeOfLoading()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.EstimatedTimeOfLoading = new ZDateTime(2017, 11, 2, 11, 2, 0);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			header = newFactory.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(new ZDateTime(2017, 11, 2, 11, 2, 0), header.EstimatedTimeOfLoading);
		}

		public void TestEstimatedTimeOfLoadingVisible()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			AssertEquals(false, header.EstimatedTimeOfLoadingVisible);

			header.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			AssertEquals(true, header.EstimatedTimeOfLoadingVisible);
		}

		public void TestCARN()
		{
			var headerZA = Factory.New<AsycudaManifestHeader>();
			headerZA.CARN = "carnie folk";
			AssertEquals("carnie folk", headerZA.CARN);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			headerZA = newFactory.Load<AsycudaManifestHeader>(headerZA.PK);
			AssertEquals("carnie folk", headerZA.CARN);
		}

		public void TestPlaceOfExit()
		{
			var headerZA = Factory.New<AsycudaManifestHeader>();
			headerZA.PlaceOfExit = "JSA";
			AssertEquals("JSA", headerZA.PlaceOfExit);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			headerZA = newFactory.Load<AsycudaManifestHeader>(headerZA.PK);
			AssertEquals("JSA", headerZA.PlaceOfExit);
		}

		public void TestIsRoutingDisabled()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Routing should be disabled for ZA", false, header.IsRoutingEnabled);
		}

		public void TestVessel()
		{
			var headerZA = Factory.New<AsycudaManifestHeader>();
			headerZA.TSS_Vessel = "Vessel";
			AssertEquals("Vessel", headerZA.TSS_Vessel);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			headerZA = newFactory.Load<AsycudaManifestHeader>(headerZA.PK);
			AssertEquals("Vessel", headerZA.TSS_Vessel);
		}

		public void TestAMA_VesselName()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_RadioCallSign = "MGY";
			vessel.RV_LloydsNumber = "7338561";
			vessel.RV_Code = "Titanic";

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_RadioCallSign = "AR1";
			vessel1.RV_LloydsNumber = "0000001";
			vessel1.RV_Code = "Calypso";
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportModes.Sea;
			header.AMA_VesselName = vessel.RV_Code;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.Transports[0].JW_Vessel = vessel.RV_Code;
			header.SetParent(consol);

			AssertEquals("Pre-requisite", true, header.Consol.IsSea);
			AssertEquals("Pre-requisite", false, header.AMA_OverrideFreightDefaults);

			AssertEquals(nameof(header.Vessel), vessel, header.Vessel);
			AssertEquals(nameof(header.AMA_VesselName), "Titanic", header.AMA_VesselName);
			AssertEquals(nameof(header.AMA_RadioCallSign), "MGY", header.AMA_RadioCallSign);
			AssertEquals(nameof(header.AMA_LloydsNumber), "7338561", header.AMA_LloydsNumber);

			header.AMA_VesselName = "TheLoveBoat";
			AssertEquals(nameof(header.Vessel), null, header.Vessel);
			AssertEquals(nameof(header.AMA_RadioCallSign), ZString.Empty, header.AMA_RadioCallSign);
			AssertEquals(nameof(header.AMA_LloydsNumber), ZString.Empty, header.AMA_LloydsNumber);
			AssertEquals("JW_Vessel", "Titanic", header.Consol.Transports[0].JW_Vessel);

			header.AMA_OverrideFreightDefaults = true;
			header.AMA_VesselName = vessel1.RV_Code;
			AssertEquals(nameof(header.Vessel), vessel1, header.Vessel);
			AssertEquals(nameof(header.AMA_RadioCallSign), "AR1", header.AMA_RadioCallSign);
			AssertEquals(nameof(header.AMA_LloydsNumber), "0000001", header.AMA_LloydsNumber);
			AssertEquals("JW_Vessel", "Calypso", header.Consol.Transports[0].JW_Vessel);
		}

		public void TestVoyageFlight()
		{
			var headerZA = Factory.New<AsycudaManifestHeader>();
			headerZA.TSS_VoyageFlight = "VoyFly";
			AssertEquals("VoyFly", headerZA.TSS_VoyageFlight);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			headerZA = newFactory.Load<AsycudaManifestHeader>(headerZA.PK);
			AssertEquals("VoyFly", headerZA.TSS_VoyageFlight);
		}

		public void TestRadioCallSign()
		{
			var headerZA = Factory.New<AsycudaManifestHeader>();
			headerZA.TSS_RadioCallSign = "CallSign";
			AssertEquals("CallSign", headerZA.TSS_RadioCallSign);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			headerZA = newFactory.Load<AsycudaManifestHeader>(headerZA.PK);
			AssertEquals("CallSign", headerZA.TSS_RadioCallSign);
		}

		public void TestCarrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CCC";
			Factory.Save();

			var headerZA = Factory.New<AsycudaManifestHeader>();

			headerZA.TSS_CargoCarrierPK = carrier.PK;
			AssertEquals(carrier.PK, headerZA.TSS_CargoCarrierPK);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			headerZA = newFactory.Load<AsycudaManifestHeader>(headerZA.PK);
			AssertEquals(carrier.PK, headerZA.TSS_CargoCarrierPK);
			AssertEquals(carrier.OH_Code, headerZA.TSS_CargoCarrier.OH_Code);
		}

		public void TestDateOfDeparture()
		{
			var headerZA = Factory.New<AsycudaManifestHeader>();
			var testDate = new ZDateTime(2019, 11, 26, 14, 15, 16);
			headerZA.TSS_DateOfDeparture = testDate;
			AssertEquals(testDate, headerZA.TSS_DateOfDeparture);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			headerZA = newFactory.Load<AsycudaManifestHeader>(headerZA.PK);
			AssertEquals(testDate, headerZA.TSS_DateOfDeparture);
		}

		public void TestCallPurposeCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.CallPurposeCode = CallPurposeCodeList.Codes.LoadingCargo;
			AssertEquals(CallPurposeCodeList.Codes.LoadingCargo, header.CallPurposeCode);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			header = newFactory.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(CallPurposeCodeList.Codes.LoadingCargo, header.CallPurposeCode);

			header.AMA_RL_NKPortOfLoading = "CNSHG";
			header.AMA_RL_NKPortOfDischarge = "ZADUR";
			AssertEquals(CallPurposeCodeList.Codes.UnloadingCargo, header.CallPurposeCode);

			header.AMA_RL_NKPortOfLoading = "ZADUR";
			header.AMA_RL_NKPortOfDischarge = "CNSHG";
			AssertEquals(CallPurposeCodeList.Codes.LoadingCargo, header.CallPurposeCode);

			header.AMA_RL_NKPortOfLoading = "ZARCB";
			header.AMA_RL_NKPortOfDischarge = "ZADUR";
			AssertEquals(ZString.Empty, header.CallPurposeCode);

			header.AMA_RL_NKPortOfLoading = ZString.Empty;
			header.AMA_RL_NKPortOfDischarge = ZString.Empty;
			AssertEquals(ZString.Empty, header.CallPurposeCode);
		}

		public void TestCallPurposeCode_Consol()
		{
			(string loadPort, string dischargePort, string expectedCallPurposeCode)[] testdata = [
				(string.Empty, string.Empty, string.Empty),
				("PRSJU", "ZAJNB", CallPurposeCodeList.Codes.UnloadingCargo),
				("ZAJNB", "PRSJU", CallPurposeCodeList.Codes.LoadingCargo),
				("ZAJNB", "ZADUR", string.Empty),
			];

			foreach (var (loadPort, dischargePort, expectedCallPurposeCode) in testdata)
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = loadPort;
				consol.JK_RL_NKDischargePort = dischargePort;

				var wrapper = new ManifestHeadersWrapper(consol);
				var header = wrapper.CreateCountry(CountryCodes.SouthAfrica, ZaManifestTypes.Codes.HAB) as AsycudaManifestHeader;

				var testCaseString = $"LoadPort '{loadPort}' DischargePort '{dischargePort}'";
				AssertNotNull(testCaseString + " Pre-requisite: expect ZA AsycudaManifestHeader", header);
				AssertEquals(testCaseString, expectedCallPurposeCode, header.CallPurposeCode);
			}
		}

		public void TestCallPurposeCode_ConsolReloaded()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "ZAJNB";
			consol.JK_RL_NKDischargePort = "GBLHR";

			var wrapper = new ManifestHeadersWrapper(consol);
			var header = wrapper.CreateCountry(CountryCodes.SouthAfrica, ZaManifestTypes.Codes.HAB) as AsycudaManifestHeader;
			AssertNotNull("Pre-requisite: expect ZA AsycudaManifestHeader", header);

			const string testNonDefaultCallPurposeCodeValue = "XX";
			header.CallPurposeCode = testNonDefaultCallPurposeCodeValue;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
			var reloadedHeaders = reloadedConsol.GetGlobalManifestHeaders();
			AssertEquals("Pre-requisite: manifest headers count on reloaded consol", 1, reloadedHeaders.Length);
			var reloadedHeader = reloadedHeaders[0] as AsycudaManifestHeader;
			AssertNotNull("Pre-requisite: reloaded (ZA) AsycudaManifestHeader", header);

			AssertEquals("CallPurposeCode on loaded manifest should not be changed", testNonDefaultCallPurposeCodeValue, reloadedHeader.CallPurposeCode);

			reloadedHeader.SynchroniseWithSourceIfNeeded();
			AssertEquals("CallPurposeCode on 'decoupled' manifest should not be changed when no changes on the consol", testNonDefaultCallPurposeCodeValue, reloadedHeader.CallPurposeCode);
		}

		[ExpectNoExceptions]
		public void TestCanSaveManifestHeaderWhenDecoupled()
		{
			var voyage = Factory.New<JobVoyage>();
			var voyageOrigin = Factory.New<VoyageOrigin>();
			voyageOrigin.JA_RL_NKPortOfLoading = "ZACPT";
			voyageOrigin.JA_E_DEP = ZDateTime.Today;
			voyageOrigin.JA_JV = voyage.PK;
			var voyageDestination = Factory.New<VoyageDestination>();
			voyageDestination.JB_RL_NKPortOfDischarge = "GBLON";
			voyageDestination.JB_E_ARV = ZDateTime.Today.AddDays(14);
			voyageDestination.JB_JV = voyage.PK;
			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = voyageOrigin.PK;
			sailing.JX_JB = voyageDestination.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "ZACPT";
			consol.JK_RL_NKDischargePort = "GBLON";
			consol.Transports[0].JW_JX = sailing.PK;
			Factory.Save();

			var headerWrapper = new ManifestHeadersWrapper(consol);
			var manifest = (AsycudaManifestHeader)headerWrapper.CreateCountry(CountryCodes.SouthAfrica, ZaManifestTypes.Codes.COH);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var manifestReloaded = newFactory.Load<AsycudaManifestHeader>(manifest.PK);
			manifestReloaded.SynchroniseWithSourceIfNeeded();
			manifestReloaded.CallPurposeCode = CallPurposeCodeList.Codes.LoadingCargo;
			newFactory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection);
	}
}
