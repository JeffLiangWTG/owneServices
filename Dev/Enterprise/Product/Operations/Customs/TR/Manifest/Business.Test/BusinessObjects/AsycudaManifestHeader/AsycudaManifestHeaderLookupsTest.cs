using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportType()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var lookUp = manifestHeader.Lookups;
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(5, lookUp.TransportTypeList.Count);
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT10));
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT12));
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT16));
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT17));
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT18));
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals(2, lookUp.TransportTypeList.Count);
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT20));
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT23));
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(1, lookUp.TransportTypeList.Count);
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT30));
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(1, lookUp.TransportTypeList.Count);
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT40));
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals(1, lookUp.TransportTypeList.Count);
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT50));
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
			AssertEquals(1, lookUp.TransportTypeList.Count);
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT70));
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			AssertEquals(1, lookUp.TransportTypeList.Count);
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT80));
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
			AssertEquals(1, lookUp.TransportTypeList.Count);
			Assert(lookUp.TransportTypeList.ContainsCode(TRTransportTypes.Codes.TT90));
		}

		public void TestTR_GM_PresentationCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Presentation Customs Office");
			var tr070400 = helper.CreateNewOrGetExistingCusCodeList("TR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "tr070400", "Presentation Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tr070500 = helper.CreateNewOrGetExistingCusCodeList("TR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "tr070500", "Presentation Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "TR";
			var customsOffices = header.Lookups.CustomsOffices as CodeDescriptionPairList;
			AssertEquals(true, customsOffices.ContainsCode("tr070400"));
			AssertEquals(true, customsOffices.ContainsCode("tr070500"));
			AssertEquals(false, customsOffices.ContainsCode("XX"));
		}

		public void TestCustomsDischargePortList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeType("PORT", "Port");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TRMER-001", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "DEHAM", yesterday, tomorrow);
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "TR";
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var list = header.Lookups.CustomsDischargePortList as BusinessObjectCollection;
			list.Load();
			AssertEquals(2, list.Count);
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "TRMER-001"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "DEHAM"));
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var listUNLOCO = header.Lookups.CustomsDischargePortList;
			AssertType<RefUNLOCOCollection>(listUNLOCO);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			list = header.Lookups.CustomsDischargePortList as BusinessObjectCollection;
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
		}

		public void TestCustomsLoadPortList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeType("PORT", "Port");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "TRMER-001", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "DEHAM", yesterday, tomorrow);
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "TR";
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var list = header.Lookups.CustomsLoadingPortList as BusinessObjectCollection;
			list.Load();
			AssertEquals(2, list.Count);
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "TRMER-001"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "DEHAM"));
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var listUNLOCO = header.Lookups.CustomsLoadingPortList;
			AssertType<RefUNLOCOCollection>(listUNLOCO);
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			list = header.Lookups.CustomsLoadingPortList as BusinessObjectCollection;
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
		}
	}
}
