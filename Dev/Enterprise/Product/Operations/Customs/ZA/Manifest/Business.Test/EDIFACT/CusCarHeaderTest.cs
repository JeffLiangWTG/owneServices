using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using CallPurposeCodeList = Enterprise.Customs.ZA.Business.CallPurposeCodeList;
using CargoStatusList = Enterprise.Customs.ASYCUDA.Business.CargoStatusList;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT.Testing
{
	class CusCarHeaderTest : TestCaseWithFactory
	{
		public void TestICusCarHeader_GetContainersByBillIssuer_Expect_NO_NullReferenceException_Caused_by_Relation2Object()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = manifest.Bills.AddNew();
			bill.ABL_BillIssuer = "ISS01";
			var ablEntryNum = bill.CustomsEntryNumbers.AddNew();
			_ = ablEntryNum.PackPivots.AddNew();
			var carHeader = new CusCarHeader(manifest);
			AssertNoExceptionThrown(() =>
			{
				_ = ((ICusCarHeader)carHeader).GetContainersByBillIssuer("ISS01").Any();
			});
		}

		public void TestICusCarHeader_VesselID()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_RadioCallSign = "A1";
			var carHeader = new CusCarHeader(manifest);
			AssertEquals("A1", ((ICusCarHeader)carHeader).VesselID);
		}

		public void TestICusCarHeader_CarrierCode()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_CarrierCode = "CCC";
			var carHeader = new CusCarHeader(manifest);
			AssertEquals("CCC", ((ICusCarHeader)carHeader).CarrierCode);
		}

		public void TestICusCarHeader_GetParties_DEG()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "CCC";
			carrier.ZZ4_CountryOrGrouping = "ZA";
			carrier.ZZ4_Description = "VWG";
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.MASTER, ZString.Empty);
			carrier.Attributes.AddNew("SEA", "SEA");
			Factory.Save();
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_AgentType = Core.Constants.AgentType.CoLoad;
			manifest.MasterCarrierCode = "1234";
			manifest.AMA_CarrierCode = "CCC";
			var bill = manifest.Bills.AddNew();
			bill.ABL_BillIssuer = "ISS01";
			var carHeader = new CusCarHeader(manifest);
			var rl = ((ICusCarHeader)carHeader).GetParties("ISS01").Single(x => x.PartyType == PartyType.ReportingCarrier_DEG);
			AssertEquals("CCC", rl.IdentificationCode);
			AssertEquals("VWG", rl.PartyName);
		}

		public void TestICusCarHeader_GetParties_RL()
		{
			var carrierOrgAddress = Factory.Load<OrgAddress>(new ZGuid("0EA85FB9-EC2E-4713-8A96-85B6290E97BB"));
			carrierOrgAddress.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "1234", "ZA");
			VoidParameterlessDelegate assertDelegate = () =>
			{
			};
			foreach (var testCase in new[] { new { ManifestType = nameof(ManifestDocumentType.COM), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.COH), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.BBB), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.ECL), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.FFM), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.FWB), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.HAB), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.RMA), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.RFM), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.AQM), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.ALM), IdentificationCode = "1234" },
				new { ManifestType = nameof(ManifestDocumentType.ALH), IdentificationCode = "1234" } })
			{
				assertDelegate += () =>
				{
					var manifest = Factory.New<AsycudaManifestHeader>();
					manifest.AMA_OA_ShippingAgent = carrierOrgAddress.PK;
					manifest.AMA_OA_Carrier = carrierOrgAddress.PK;
					manifest.AMA_AgentType = Core.Constants.AgentType.CoLoad;
					manifest.MasterCarrierCode = "1234";
					manifest.AMA_ManifestType = testCase.ManifestType;
					var bill = manifest.Bills.AddNew();
					var carHeader = new CusCarHeader(manifest);
					manifest.AMA_TransportMode = ZString.Empty;
					bill.ABL_BillIssuer = "ISS01";
					var rl = ((ICusCarHeader)carHeader).GetParties("ISS01").Single(x => x.PartyType == PartyType.ReportingCarrier_RL);
					AssertEquals(testCase.ManifestType, testCase.IdentificationCode, rl.IdentificationCode);
				};
			}

			CombineAssertions(assertDelegate);
		}

		public void TestICusCarHeader_ManifestTypeOrBolNature()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			ICusCarHeader carHeader = new CusCarHeader(header);
			AssertEquals("ZZZ", carHeader.ManifestTypeOrBolNature);
			bill1.ABL_ShipmentType = ZString.Empty;
			bill2.ABL_ShipmentType = ZString.Empty;
			AssertEquals(ZString.Empty, carHeader.ManifestTypeOrBolNature);
		}

		public void TestICusCarHeader_ManifestTypeOrBolNature_Road()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "ROA";
			header.AMA_Nature = ZaShipmentTypeList.Codes.MutualMultipleZzz;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			ICusCarHeader carHeader = new CusCarHeader(header);

			AssertEquals(ZaShipmentTypeList.Codes.MutualMultipleZzz, carHeader.ManifestTypeOrBolNature);

			bill2.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;

			AssertEquals("28", carHeader.ManifestTypeOrBolNature);

			header.AMA_Nature = "XYZ";
			AssertEquals("XYZ", carHeader.ManifestTypeOrBolNature);
		}

		public void TestICusCarHeader_AgentType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			ICusCarHeader carHeader = new CusCarHeader(header);
			AssertEquals("", carHeader.AgentType);
			header.AMA_AgentType = "FWB";
			AssertEquals("FWB", carHeader.AgentType);
		}

		public void TestIInterchangeSenderIdProvider()
		{
			GlbCompany.CurrentCompany.OrgProxy.SetAgentCode(RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SouthAfrica), "DJC");
			GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SouthAfrica), "DUAL");
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var carHeader = new CusCarHeader(header);
			IInterchangeSenderIdProvider provider = carHeader;
			AssertEquals("DJCDUAL", provider.SenderID);
		}

		public void TestICusCarHeader_MRNForAmendOrDelete()
		{
			ASYCUDA.Business.Testing.ZZDataTestHelper.SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			var header = Factory.New<AsycudaManifestHeader>();
			var message = Factory.New<ZA.Business.CUSRESEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message.EM_MessageType = ZA.Business.SARSEDIMessage.MessageTypes.CUSRES;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			message.EM_MessageText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+6332A821D8364BBA9A76BE1678FD52BF'LOC+22+ ::ZZZ'GIS+8:120:ZZZ'NAD+AG+00000000'RFF+BH:S700049788'RFF+AAS:RFM'DTM+137:20170419:102'RFF+AFB:CARN0111138D'UNT+10+1'";
			message.EM_LinkedObject = header;
			var message2 = Factory.New<ZA.Business.CUSRESEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message2.EM_MessageType = ZA.Business.SARSEDIMessage.MessageTypes.CUSRES;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message2.EM_MessageText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+04E362079C1245A48B4A388237F4FE5F'LOC+22+ ::ZZZ'GIS+8:120:ZZZ'NAD+AG+00000000'RFF+BH:S700049788'RFF+AAS:RFM'DTM+137:20170419:102'RFF+AFB:CARN0111138D'UNT+10+1'";
			message2.EM_LinkedObject = header;
			Factory.Save();
			var carHeader = new CusCarHeader(header);
			AssertEquals("MRNForAmendOrDelete", "04E362079C1245A48B4A388237F4FE5F", ((ICusCarHeader)carHeader).MRNForAmendOrDelete);
		}

		public void TestICusCarHeader_GetLinesByBillIssuer()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var ce1 = bill.CustomsEntryNumbers.AddNew();
			var ce2 = bill.CustomsEntryNumbers.AddNew();
			var carHeader = new CusCarHeader(manifest);
			manifest.AMA_TransportMode = ZString.Empty;
			bill.ABL_BillIssuer = "ISS01";
			AssertEquals("Precondition", false, manifest.SupportAssociatedPacks);
			var billCountryLines = ((ICusCarHeader)carHeader).GetLinesByBillIssuer("ISS01").ToList();
			AssertEquals(1, billCountryLines.Count);
			Assert(billCountryLines[0] is CusCarBill);
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
			bill.ABL_BillIssuer = "ISS01";
			AssertEquals("Precondition", true, manifest.SupportAssociatedPacks);
			var entryNumLines = ((ICusCarHeader)carHeader).GetLinesByBillIssuer("ISS01").Cast<CusCarEntryNum>().ToList();
			AssertEquals(2, entryNumLines.Count);
			Assert(entryNumLines.Any(l => l.EntryNum == ce1));
			Assert(entryNumLines.Any(l => l.EntryNum == ce2));
			Assert("All EntryNum lines use the same BillCountry instance", entryNumLines[0].Bill == entryNumLines[1].Bill);
		}

		public void TestICusCarHeader_CARNForAmendOrDelete()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.RegistrationNumber = "Poop";
			ICusCarHeader carHeader = new CusCarHeader(header);
			AssertEquals("Poop", carHeader.CARNForAmendOrDelete);
		}

		public void TestICusCarHeader_CustomsCodeForContainerMode()
		{
			var startDate = ZDateTime.FromSqlFormat("1900-01-01 00:00:00.000");
			var endDate = ZDateTime.FromSqlFormat("2079-06-06 00:00:00.000");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.CMODE, "OUT", "Container Mode Mapping", true);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.CMODE, Core.Constants.ContainerModes.BreakBulk, "BB", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.CMODE, Core.Constants.ContainerModes.Bulk, "DB", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.CMODE, Core.Constants.ContainerModes.Containerised, "CN", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.CMODE, Core.Constants.ContainerModes.Liquid, "LB", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.CMODE, Core.Constants.ContainerModes.Other, "MX", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			ICusCarHeader carHeader = new CusCarHeader(header);
			AssertEquals("BB", carHeader.CustomsCodeForContainerMode);
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("DB", carHeader.CustomsCodeForContainerMode);
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("CN", carHeader.CustomsCodeForContainerMode);
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("LB", carHeader.CustomsCodeForContainerMode);
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Other;
			AssertEquals("MX", carHeader.CustomsCodeForContainerMode);
		}

		public void TestICusCarHeader_CustomsCodeForImportExportNature()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			ICusCarHeader carHeader = new CusCarHeader(header);
			header.CallPurposeCode = ZString.Empty;
			AssertEquals(ZString.Empty, carHeader.CustomsCodeForImportExportNature);
			header.CallPurposeCode = CallPurposeCodeList.Codes.LoadingCargo;
			AssertEquals(CallPurposeCodeList.Codes.LoadingCargo, carHeader.CustomsCodeForImportExportNature);
			header.CallPurposeCode = CallPurposeCodeList.Codes.UnloadingCargo;
			AssertEquals(CallPurposeCodeList.Codes.UnloadingCargo, carHeader.CustomsCodeForImportExportNature);
		}

		public void TestICusCarHeader_MasterBol()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBOL = "123";
			ICusCarHeader carHeader = new CusCarHeader(header);
			AssertEquals("123", carHeader.MasterBol);
		}

		public void TestZAManifestEDIMessagesAddress_FreeType()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = CreateAddress(org1, "111", "Address11", "Address12", "City1", "State1", "CpName1");
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = CreateAddress(org2, "222", "Address21", "Address22", "City2", "State2", "CpName2");
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var address3 = CreateAddress(org3, "333", "Address31", "Address32", "City3", "State3", "CpName3");
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			bill.ABL_BillNumber = "BILL1";
			bill.ABL_CargoStatus = CargoStatusList.Codes.PartShipment;
			bill.ABL_BillIssuer = "ISS2";
			bill.ABL_BillNumber = "BILL2";
			bill.ABL_OA_Shipper = address1.PK;
			bill.ABL_OA_Consignee = address2.PK;
			bill.ABL_OA_NotifyParty = address3.PK;
			var popupText = CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			AssertEquals(1, manifest.Messages.Count);
			var messageText = manifest.Messages[0].EM_MessageText;
			AssertContains("Shipper detail", "NAD+CZ++CPNAME1:ADDRESS11:CITY1:STATE1:111", messageText);
			AssertContains("Consignee detail", "NAD+CN++CPNAME2:ADDRESS21:CITY2:STATE2:222", messageText);
			AssertContains("NotifyParty detail", "NAD+NI++CPNAME3:ADDRESS31:CITY3:STATE3:333", messageText);
		}

		public void TestZAManifestEDIMessagesAddress_ManualDetail()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			manifest.AMA_TransportMode = "SEA";
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			bill.ABL_BillNumber = "BILL1";
			bill.ABL_CargoStatus = CargoStatusList.Codes.PartShipment;
			bill.ABL_BillIssuer = "ISS2";
			bill.ABL_BillNumber = "BILL2";
			bill.ABL_OA_Shipper = Guid.Empty;
			bill.ABL_OA_Consignee = Guid.Empty;
			bill.ABL_OA_NotifyParty = Guid.Empty;
			bill.ABL_ShipperName = "CpName1";
			bill.ABL_ShipperStreet1 = "Address11";
			bill.ABL_ShipperStreet2 = "Address12";
			bill.ABL_ShipperCity = "City1";
			bill.ABL_ShipperState = "State1";
			bill.ABL_ShipperPostcode = "111";
			bill.ABL_ConsigneeName = "CpName2";
			bill.ABL_ConsigneeStreet1 = "Address21";
			bill.ABL_ConsigneeStreet2 = "Address22";
			bill.ABL_ConsigneeCity = "City2";
			bill.ABL_ConsigneeState = "State2";
			bill.ABL_ConsigneePostcode = "222";
			bill.ABL_NotifyPartyName = "CpName3";
			bill.ABL_NotifyPartyStreet1 = "Address31";
			bill.ABL_NotifyPartyStreet2 = "Address32";
			bill.ABL_NotifyPartyCity = "City3";
			bill.ABL_NotifyPartyState = "State3";
			bill.ABL_NotifyPartyPostcode = "333";
			var popupText = CusCarMessagingHelper.CreateCusCars(manifest, MessageSubTypeCodes.Codes.Original);
			Factory.Save();
			AssertEquals(1, manifest.Messages.Count);
			var messageText = manifest.Messages[0].EM_MessageText;
			AssertContains("Shipper detail", "NAD+CZ++CPNAME1:ADDRESS11:CITY1:STATE1:111", messageText);
			AssertContains("Consignee detail", "NAD+CN++CPNAME2:ADDRESS21:CITY2:STATE2:222", messageText);
			AssertContains("NotifyParty detail", "NAD+NI++CPNAME3:ADDRESS31:CITY3:STATE3:333", messageText);
		}

		public void TestIEDIMessageCollectionOwnerForSingleBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "ZA";
			header.AMA_ManifestType = "ECL";
			header.AMA_ManifestNumber = "MAN12345";

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "B0001";
			bill.ABL_BillIssuer = "AAA";

			var carHeader = new CusCarHeader(header);

			IEDIMessageCollectionOwner owner = new CusCarMessagingHelper.SingleBillCusCarHeader(carHeader, bill);
			CombineAssertions(() =>
			{
				AssertSame("Message Owner", bill, owner.MessageOwner);
				AssertSame("Message Collection", bill.Messages, owner.Messages);
			});
		}

		OrgAddress CreateAddress(OrgHeader org, string postCode, string address1, string address2, string city, string state, string companyName)
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.CompanyName = companyName;
			address.Address1 = address1;
			address.Address2 = address2;
			address.City = city;
			address.State = state;
			address.Postcode = postCode;
			return address;
		}
	}
}
