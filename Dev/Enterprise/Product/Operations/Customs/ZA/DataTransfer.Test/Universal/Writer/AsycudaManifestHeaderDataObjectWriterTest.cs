using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class AsycudaManifestHeaderDataObjectWriterTest : TestCaseWithFactory
	{
		[TestDate(1998, 10, 31)]
		public void TestAsycudaManifestHeaderDataObjectWriter()
		{
			var zaCompany = CreateZABranch(Factory);
			Factory.Save();

			using (Enterprise.Environment.DisposableEnvironment.ForCompany(zaCompany.GC_Code))
			{
				var bo = CreateAsycudaManifestHeader(Factory, "OUTTURN", "1");
				Factory.Save();

				var xmlShipmentData = GenerateShipmentData(Factory, bo);
				AssertPopulatedUniversalShipmentXml(xmlShipmentData);
			}
		}

		public static Shipment GenerateShipmentData(BusinessObjectFactory factory, AsycudaManifestHeader bo)
		{
			var writer = new AsycudaManifestHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, bo)));
			return writer.GetDataObject(bo);
		}

		public static void AssertPopulatedUniversalShipmentXml(Shipment xmlShipmentData)
		{
			AssertNotNull(xmlShipmentData);

			AssertEquals("SEA", xmlShipmentData.TransportMode?.Code);
			AssertEquals("Sea Freight", xmlShipmentData.TransportMode?.Description);
			AssertEquals("DRT", xmlShipmentData.DeclarantType?.Code);
			AssertEquals("Direct", xmlShipmentData.DeclarantType?.Description);
			AssertEquals("VOYAGE1", xmlShipmentData.VoyageFlightNo);
			AssertEquals("04", xmlShipmentData.LocationAtClearance.Code);
			AssertEquals("BFN", xmlShipmentData.CustomsOffice.Code);
			AssertEquals("", xmlShipmentData.CustomsOffice.Description);
			AssertEquals("EXP", xmlShipmentData.ShipmentType.Code);
			AssertEquals("Export (22)", xmlShipmentData.ShipmentType.Description);
			AssertEquals("ZAPL1", xmlShipmentData.PortOfLoading.Code);
			AssertEquals("ZAPU1", xmlShipmentData.PortOfDischarge.Code);
			AssertEquals("VOR", xmlShipmentData.ExportGoodsType.Code);
			AssertEquals("Vessel Outturn Report", xmlShipmentData.ExportGoodsType.Description);
			AssertEquals("", xmlShipmentData.MessageStatus.Code);
			AssertEquals("Not Sent", xmlShipmentData.MessageStatus.Description);
			AssertEquals("", xmlShipmentData.EntryStatus.Code);
			AssertEquals("", xmlShipmentData.EntryStatus.Description);
			AssertEquals("BOOKING1", xmlShipmentData.QuoteNumber);
			AssertEquals("MASTERBILL1", xmlShipmentData.WayBillNumber);

			AssertAddressData(xmlShipmentData.OrganizationAddressCollection, nameof(DocAddressType.Carrier), "CARRIER", "1");
			AssertAddressData(xmlShipmentData.OrganizationAddressCollection, nameof(DocAddressType.CustomsContainerYardAddress), "DECONSOLIDATOR", "2");
			AssertAddressData(xmlShipmentData.OrganizationAddressCollection, nameof(DocAddressType.CustomsContainerTerminalOperatorAddress), "DISCHARGE", "3");
			AssertAddressData(xmlShipmentData.OrganizationAddressCollection, nameof(DocAddressType.ControllingAgent), "AGENT", "4");

			AssertDateData(xmlShipmentData.DateCollection, DateType.Departure, ZDateTime.BrettsBirthday.AddDays(+1));
			AssertDateData(xmlShipmentData.DateCollection, DateType.Arrival, ZDateTime.BrettsBirthday.AddDays(+2));
			AssertDateData(xmlShipmentData.DateCollection, DateType.Unpack, ZDateTime.BrettsBirthday.AddDays(+4));
			AssertDateData(xmlShipmentData.DateCollection, DateType.BillIssued, ZDate.BrettsBirthday);

			AssertShipmentAddInfosData(xmlShipmentData.AddInfoCollection, AddInfoConstants.AsycudaManifestHeader.ExcessIndicator, "1");
			AssertShipmentAddInfosData(xmlShipmentData.AddInfoCollection, AddInfoConstants.AsycudaManifestHeader.GateInOutMessageType, "DGI");
			AssertShipmentAddInfosData(xmlShipmentData.AddInfoCollection, AddInfoConstants.AsycudaManifestHeader.ParentBill, "PARENTBILL1");

			AssertContainerData(xmlShipmentData.ContainerCollection, "20NOR", "FCL", "CAR", "CONTAINER1", "SEAL1");
			AssertContainerData(xmlShipmentData.ContainerCollection, "20NOR", "LCL", "AGT", "CONTAINER2", "SEAL2");
			AssertContainerData(xmlShipmentData.ContainerCollection, "40GP", "FCL", "CUS", "CONTAINER3", "SEAL3");
			AssertContainerData(xmlShipmentData.ContainerCollection, "40GP", "LCL", "TOR", "CONTAINER4", "SEAL4");

			AssertEquals("CNT", xmlShipmentData.ContainerMode?.Code);
			AssertEquals("Containerised", xmlShipmentData.ContainerMode?.Description);
			AssertEquals("VESSEL1", xmlShipmentData.VesselName);

			var billCollection = xmlShipmentData.SubShipmentCollection;
			AssertEquals(2, billCollection.Count);
			AssertBillData(xmlShipmentData.SubShipmentCollection, "HOUSEBILL1", "HWB", "ISSUER1", "MRN1", "CPC1", "LRN1", "ST1", "1", "2");
			AssertBillData(xmlShipmentData.SubShipmentCollection, "HOUSEBILL2", "HWB", "ISSUER2", "MRN2", "CPC2", "LRN2", "ST2", "3", "4");
		}

		public static void AssertBillData(DataObjectList<Shipment> billCollection, string billNo, string billType, string billIssuer, string mrn, string cpc, string lrn, string billStatus, string suffix1, string suffix2)
		{
			var billData = billCollection.FirstOrDefault(x => x.WayBillNumber.ToString() == billNo);
			AssertEquals(billType, billData.WayBillType?.Code);
			AssertEquals(billIssuer, billData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudaBill.BillIssuer)?.Value);
			AssertEquals(mrn, billData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudaBill.MRN)?.Value);
			AssertEquals(cpc, billData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudaBill.CustomsCPC)?.Value);
			AssertEquals(lrn, billData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudaBill.LRN)?.Value);
			AssertEquals(billStatus, billData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudaBill.BillStatus)?.Value);

			AssertPackData(billData.PackingLineCollection, suffix1);
			AssertPackData(billData.PackingLineCollection, suffix2);
		}

		public static void AssertPackData(DataObjectList<PackingLine> packingLineCollection, string suffix)
		{
			var packData = packingLineCollection.FirstOrDefault(x => x.MarksAndNos.ToString() == $"MARKSANDNUMBERS{suffix}");
			var longValue = ZLong.Parse(suffix);
			var decimalValue = ZDecimal.Parse(suffix + ".99");
			AssertEquals($"CONTAINER{suffix}", packData.ContainerNumber);
			AssertEquals(decimalValue, packData.ManifestedWeight);
			AssertEquals(decimalValue, packData.ManifestedVolume);
			AssertEquals(longValue, packData.PackQty);
			AssertEquals($"{suffix}A", packData.PackType?.Code);
			AssertEquals(longValue.ToZInt(), packData.OutturnQty);
			AssertEquals($"GOODSDESC{suffix}", packData.GoodsDescription);
			AssertEquals(decimalValue, packData.OutturnedWeight);
			AssertEquals(decimalValue, packData.OutturnedVolume);

			AssertEquals($"B{suffix}", packData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudePack.CargoType)?.Value);
			AssertEquals($"{suffix}", packData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudePack.PackageConditionInd)?.Value);
			AssertEquals($"{suffix}", packData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudePack.ExcessShortInd)?.Value);
			AssertEquals($"KG", packData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudePack.ManifestedWeightUnit)?.Value);
			AssertEquals($"L", packData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudePack.ManifestedVolumeUnit)?.Value);
			AssertEquals($"PACKCONDITIONDESC{suffix}", packData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudePack.PackCondDesc)?.Value);
			AssertEquals($"CONTENTSSHOULDBEDESC{suffix}", packData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudePack.ContShouldBe)?.Value);
			AssertEquals($"KG", packData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudePack.OutturnedWeightUnit)?.Value);
			AssertEquals($"L", packData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudePack.OutturnedVolumeUnit)?.Value);
			AssertEquals($"Yes", packData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudePack.SealIntactIndicator)?.Value);
		}

		public static void AssertContainerData(DataObjectList<Container> containerCollection, string containerType, string emptyFullIndicator, string sealingPartyType, string containerId, string sealId)
		{
			var containerData = containerCollection.FirstOrDefault(x => x.ContainerNumber.ToString() == containerId);
			AssertEquals(containerType, containerData.ContainerType?.Code);
			AssertEquals(emptyFullIndicator, containerData.ContainerStatus?.Code);
			AssertEquals(sealId, containerData.Seal);
			AssertEquals(sealingPartyType, containerData.SealPartyType?.Code);
			AssertEquals(new ZDateTime(1998, 8, 9, 1, 5, 0).ToString(), containerData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudaContainer.ContUnpackTime)?.Value);
			AssertEquals(new ZDateTime(1998, 8, 9, 2, 5, 0).ToString(), containerData.AddInfoCollection.FirstOrDefault(x => x.Key.ToString() == AddInfoConstants.AsycudaContainer.GateInOutDate)?.Value);
		}

		public static void AssertShipmentAddInfosData(List<AddInfo> addInfoCollection, string key, string expectedValue)
		{
			var addinfoData = addInfoCollection.FirstOrDefault(x => x.Key.ToString() == key);
			AssertEquals(expectedValue, addinfoData.Value);
		}

		public static void AssertDateData(List<Date> dateCollection, DateType dateType, ZDateTime? expectedValue)
		{
			var dateData = dateCollection.FirstOrDefault(x => x.Type == dateType);
			AssertEquals(expectedValue, dateData.Value);
		}

		public static void AssertAddressData(List<OrganizationAddress> organizationAddressCollection, ZString docAddressType, string traderName, string suffix,
			string address1 = "ADDRESS1", string address2 = "ADDRESS2", string city = "CITY", string state = "STATE", string postCode = "POST XX", string countryCode = "GB")
		{
			var addressData = organizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == docAddressType);
			AssertEquals(traderName + suffix, addressData.CompanyName);
			AssertEquals(address1 + suffix, addressData.Address1);
			AssertEquals(address2 + suffix, addressData.Address2);
			AssertEquals(city + suffix, addressData.City);
			AssertEquals(state + suffix, addressData.State);
			AssertEquals(countryCode, addressData.Country?.Code);
		}

		public static AsycudaManifestHeader CreateAsycudaManifestHeader(BusinessObjectFactory factory, ZString jobReference, ZString suffix)
		{
			var carrierAddressPK = CreateOrganisation(factory, "1", "CARRIER").PK;
			var desconsolidatorAddressPK = CreateOrganisation(factory, "2", "DECONSOLIDATOR").PK;
			var dischargeTerminalAddressPK = CreateOrganisation(factory, "3", "DISCHARGE").PK;
			var agentPK = CreateOrganisation(factory, "4", "AGENT").PK;

			var manifestHeader = factory.New<AsycudaManifestHeader>();
			manifestHeader.FillWithValidTestData();
			manifestHeader.AMA_JobReference = $"{jobReference}";
			manifestHeader.AMA_VesselName = $"VESSEL{suffix}";
			manifestHeader.AMA_Voyage = $"VOYAGE{suffix}";
			manifestHeader.AMA_OA_Carrier = carrierAddressPK;
			manifestHeader.AMA_OA_ShippingAgent = agentPK;
			manifestHeader.AMA_TransportMode = "SEA";
			manifestHeader.AMA_ApplicationCode = "OUT";
			manifestHeader.AMA_ContainerMode = "CNT";
			manifestHeader.AMA_VehicleRegistration = $"REG{suffix}";
			manifestHeader.AMA_AgentType = "DRT";
			manifestHeader.AMA_OA_DeconsolidateAddress = desconsolidatorAddressPK;
			manifestHeader.AMA_OA_DischargeTerminalAddress = dischargeTerminalAddressPK;
			manifestHeader.AMA_Nature = "EXP";
			manifestHeader.AMA_ManifestType = "VOR";
			manifestHeader.AMA_CustomsOffice = "BFN";
			manifestHeader.AMA_RN_NKCountry = "ZA";
			manifestHeader.ExcessIndicator = "1";
			manifestHeader.FullyLoadedUnloadedDate = ZDateTime.BrettsBirthday.AddDays(+3);
			manifestHeader.GateInOutMessageType = "DGI";
			manifestHeader.ParentBill = $"PARENTBILL{suffix}";
			manifestHeader.UnpackedDate = ZDateTime.BrettsBirthday.AddDays(+4);
			manifestHeader.GateInOutDate = ZDateTime.BrettsBirthday.AddDays(+5);

			manifestHeader.MasterBill.ABL_BillNumber = $"MASTERBILL{suffix}";
			manifestHeader.MasterBill.ABL_CarrierReference = $"BOOKING{suffix}";
			manifestHeader.MasterBill.ABL_BolType = "BOL";
			manifestHeader.MasterBill.ABL_BillIssueDate = ZDate.BrettsBirthday;
			manifestHeader.MasterBill.ABL_E_DEP = ZDateTime.BrettsBirthday.AddDays(+1);
			manifestHeader.MasterBill.ABL_E_ARV = ZDateTime.BrettsBirthday.AddDays(+2);
			manifestHeader.MasterBill.ABL_RL_NKPortOfLoading = $"ZAPL{suffix}";
			manifestHeader.MasterBill.ABL_RL_NKPortOfDischarge = $"ZAPU{suffix}";
			manifestHeader.MasterBill.ABL_GoodsLocation = "04";

			var houseBill1 = CreateHouseBill(factory, manifestHeader, "1");
			var houseBill2 = CreateHouseBill(factory, manifestHeader, "2");

			var containerType1PK = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20NOR").PK;
			var containerType2PK = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			var container1 = CreateContainer(manifestHeader, containerType1PK, "FCL", "CAR", "1");
			var container2 = CreateContainer(manifestHeader, containerType1PK, "LCL", "AGT", "2");
			var container3 = CreateContainer(manifestHeader, containerType2PK, "FCL", "CUS", "3");
			var container4 = CreateContainer(manifestHeader, containerType2PK, "LCL", "TOR", "4");

			var pack1 = CreatePack(houseBill1, container1, "1");
			var pack2 = CreatePack(houseBill1, container2, "2");
			var pack3 = CreatePack(houseBill2, container3, "3");
			var pack4 = CreatePack(houseBill2, container4, "4");

			return manifestHeader;
		}

		public static AsycudaBill CreateHouseBill(BusinessObjectFactory factory, AsycudaManifestHeader manifestHeader, ZString suffix)
		{
			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BillNumber = $"HOUSEBILL{suffix}";
			houseBill.ABL_BolType = "HWB";
			houseBill.ABL_BillIssuer = $"ISSUER{suffix}";
			houseBill.MRN = $"MRN{suffix}";
			houseBill.LRN = $"LRN{suffix}";
			houseBill.CustomsCPC = $"CPC{suffix}";
			houseBill.ABL_BillStatus = $"ST{suffix}";
			houseBill.ABL_AMA = manifestHeader.PK;
			return houseBill;
		}

		public static AsycudaContainer CreateContainer(AsycudaManifestHeader manifestHeader, ZGuid containerTypePK, ZString emptyFullIndicator, ZString sealingPartyType, ZString suffix)
		{
			var container = manifestHeader.Containers.AddNew();
			container.ACN_ContainerNumber = $"CONTAINER{suffix}";
			container.ACN_RC_ContainerType = containerTypePK;
			container.ACN_EmptyFullIndicator = emptyFullIndicator;
			container.ACN_Seal1 = $"SEAL{suffix}";
			container.ACN_SealingPartyType = sealingPartyType;
			container.ContUnpackTime = new ZDateTime(1998, 8, 9, 1, 5, 0);
			container.GateInOutDate = new ZDateTime(1998, 8, 9, 2, 5, 0);
			return container;
		}

		public static AsycudaPack CreatePack(AsycudaBill houseBill, AsycudaContainer container, ZString suffix)
		{
			var intValue = ZInt.Parse(suffix);
			var decimalValue = ZDecimal.Parse(suffix + ".99");
			var pack = houseBill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			pack.Outturn.C5_CargoType = $"B{suffix}";
			pack.Outturn.C5_PackageCondition = $"{suffix}";
			pack.Outturn.ExcessShortInd = $"{suffix}";
			pack.Outturn.C5_GoodsDescription = suffix == "3" ? "CONTENTSSHOULDBEDESC3" : $"CONTENTSFOUNDTOBEDESC{suffix}";
			pack.APA_Weight = decimalValue;
			pack.APA_WeightUQ = "KG";
			pack.APA_Volume = decimalValue;
			pack.APA_VolumeUQ = "L";
			pack.APA_MarksAndNumbers = $"MARKSANDNUMBERS{suffix}";
			pack.APA_PackQty = intValue;
			pack.APA_PackUQ = $"{suffix}A";
			pack.Outturn.C5_PackagesOutturned = intValue;
			pack.Outturn.PackCondDesc = $"PACKCONDITIONDESC{suffix}";
			pack.APA_GoodsDescription = $"GOODSDESC{suffix}";
			pack.Outturn.ContShouldBe = $"CONTENTSSHOULDBEDESC{suffix}";
			pack.Outturn.C5_WeightOutturned = decimalValue;
			pack.Outturn.C5_WeightOutturnedUQ = "KG";
			pack.Outturn.C5_VolumeOutturned = decimalValue;
			pack.Outturn.C5_VolumeOutturnedUQ = "L";
			pack.Outturn.C5_SealIntactIndicator = true;

			return pack;
		}

		public static OrgAddress CreateOrganisation(BusinessObjectFactory factory, string suffix, string traderName = "TRADER",
			string traderCode = "TR", string address1 = "ADDRESS1", string address2 = "ADDRESS2", string city = "CITY", string state = "STATE",
			string postCode = "POST XX", string relatedPortCode = "GBXX")
		{
			var traderOrgHeader = CreateOrgHeader(factory, suffix, traderName, traderCode, relatedPortCode);

			var address = traderOrgHeader.Addresses.AddNew();
			address.OA_Address1 = address1 + suffix;
			address.OA_Address2 = address2 + suffix;
			address.OA_City = city + suffix;
			address.OA_State = state + suffix;
			address.OA_PostCode = postCode + suffix;
			address.OA_RL_NKRelatedPortCode = relatedPortCode + suffix;
			return address;
		}

		public static OrgHeader CreateOrgHeader(BusinessObjectFactory factory, string suffix, string traderName = "TRADER", string traderCode = "TR", string relatedPortCode = "GBXX")
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = traderCode + suffix;
			orgHeader.OH_FullName = traderName + suffix;
			orgHeader.OH_RL_NKClosestPort = relatedPortCode + suffix;
			return orgHeader;
		}

		public static GlbCompany CreateZABranch(BusinessObjectFactory factory)
		{
			var company = factory.New<GlbCompany>();
			company.GC_Code = "CP1";
			company.GC_RN_NKCountryCode = "ZA";
			var branch = factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "BR1";
			return company;
		}
	}
}
