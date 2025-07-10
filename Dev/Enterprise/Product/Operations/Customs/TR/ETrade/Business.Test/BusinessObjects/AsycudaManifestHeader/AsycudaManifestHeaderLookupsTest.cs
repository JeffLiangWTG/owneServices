using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageModeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(TRMessageTypes.Codes.TRE, TRMessageTypes.Descriptions.TRE),
				new CodeDescriptionPair(TRMessageTypes.Codes.TRQ, TRMessageTypes.Descriptions.TRQ),
				new CodeDescriptionPair(TRMessageTypes.Codes.TRI, TRMessageTypes.Descriptions.TRI),
				new CodeDescriptionPair(TRMessageTypes.Codes.TRL, TRMessageTypes.Descriptions.TRL),
				new CodeDescriptionPair(TRMessageTypes.Codes.TRB, TRMessageTypes.Descriptions.TRB),
				new CodeDescriptionPair(TRMessageTypes.Codes.TRD, TRMessageTypes.Descriptions.TRD),
				new CodeDescriptionPair(TRMessageTypes.Codes.TCD, TRMessageTypes.Descriptions.TCD),
			}, header.Lookups.MessageModeList);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(TRMessageTypes.Codes.TRE, TRMessageTypes.Descriptions.TRE),
				new CodeDescriptionPair(TRMessageTypes.Codes.TRS, TRMessageTypes.Descriptions.TRS),
				new CodeDescriptionPair(TRMessageTypes.Codes.TRI, TRMessageTypes.Descriptions.TRI),
				new CodeDescriptionPair(TRMessageTypes.Codes.TRL, TRMessageTypes.Descriptions.TRL),
			}, header.Lookups.MessageModeList);

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			var newHeader = Factory.New<AsycudaManifestHeader>();
			newHeader.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertSame(header.Lookups.MessageModeList, newHeader.Lookups.MessageModeList);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			newHeader.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertSame(header.Lookups.MessageModeList, newHeader.Lookups.MessageModeList);
		}

		public void TestETradeNatures()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Lookups.Natures;
			var natureList = new CodeDescriptionPairList();
			natureList.AddPair(ShipmentTypeList.Codes.Export22, ShipmentTypeList.Descriptions.Export22);
			natureList.AddPair(ShipmentTypeList.Codes.Import23, ShipmentTypeList.Descriptions.Import23);
			AssertEquals(natureList, list);
		}

		public void TestETradeTransportModeList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Lookups.TransportModeList;
			var transportModeList = new CodeDescriptionPairList();
			transportModeList.AddPair(TransportTypeList.Codes.Air, TransportTypeList.Descriptions.Air);
			transportModeList.AddPair(TransportTypeList.Codes.Sea, TransportTypeList.Descriptions.Sea);
			transportModeList.AddPair(TransportTypeList.Codes.Road, TransportTypeList.Descriptions.Road);
			AssertEquals(transportModeList, list);
		}

		public void TestCountryList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Lookups.CountryList;
			AssertNotNull(header.Lookups.CountryList);
			AssertEquals("CountryList", typeof(RefCountryCollection), list.GetType());
		}

		public void TestProcedures()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var trImport = helper.CreateRefCusProcedure("TR", "A", "10", "40", "", "Import", "IMP");
			var trExport2 = helper.CreateRefCusProcedure("TR", "H", "80", "00", "", "Export", "EXP");

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var lookup = header.Lookups;
				header.ProcedureCode = "IMP";
				AssertEquals(1, lookup.Procedures.Count);

				header.ProcedureCode = "EXP";
				AssertEquals(1, lookup.Procedures.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Andorra))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var lookup = header.Lookups;
				header.ProcedureCode = "IMP";
				AssertEquals(1, lookup.Procedures.Count);
			}
		}

		public void TestBondTypeList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			var lookups = header.Lookups;

			AssertEquals("BANAKIT, BANKA, DAC, DAC/R2, GAR, GDS, GLOBAL, GTR1, GTR2, GTRANT, NAKIT, RODER, UND", lookups.BondTypeList.CodesAsString);
		}

		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusof = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var abc = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ABC", "ABC Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var cde = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CDE", "CDE Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var xyz = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XYZ", "XYZ Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var zzz = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ZZZ", "ZZZ Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var customsOffices = header.Lookups.CustomsOfficeList;
			Assert(customsOffices.ContainsCode("ABC"));
			Assert(customsOffices.ContainsCode("CDE"));
			Assert(customsOffices.ContainsCode("XYZ"));
			Assert(customsOffices.ContainsCode("ZZZ"));
		}

		public void TestCurrencyList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var lookups = header.Lookups;

			IActiveBusinessObjectCollection fCurrencyList = (IActiveBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IRefCurrencyCollection>(), new object[] { Factory });
			AssertEquals("Count", fCurrencyList.Count, lookups.CurrenciesList.Count);
		}
		public void TestGoodsLocationCodeList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Lookups.GoodsLocationCodeList;
			list.Load();
			AssertEquals(2, header.Lookups.GoodsLocationCodeList.Count);
		}

		public void TestRegistrationStatusList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Lookups.RegistrationStatusList;
			AssertSame(Factory.GetCachedValue<CustomsStatusList>(), list);
		}
		public void TestCustomsMessageStatusList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Lookups.CustomsMessageStatusList;
			var customsMessageStatusList = new CodeDescriptionPairList();
			customsMessageStatusList.AddPair("ACP", "Accepted");
			customsMessageStatusList.AddPair("AWA", "Awaiting");
			customsMessageStatusList.AddPair("CAN", "Cancel");
			customsMessageStatusList.AddPair("CLR", "Manifest Cleared");
			customsMessageStatusList.AddPair("ERR", "Error");
			customsMessageStatusList.AddPair("NOS", "Not Sent");
			customsMessageStatusList.AddPair("SNT", "Sent");
			customsMessageStatusList.AddPair("TRG", "Temporary Registered");
			customsMessageStatusList.AddPair("UNK", "Unknown");
			customsMessageStatusList.AddPair("UPD", "Updated");
			customsMessageStatusList.AddPair("REG", "Registered");
			customsMessageStatusList.AddPair("REM", "Remaining Bill");
			AssertContainsExactElementsInAnyOrder(customsMessageStatusList, list);
		}

		public void TestContainerModes()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Lookups.ContainerModes;
			var containerModeList = new CodeDescriptionPairList();
			containerModeList.AddPair("CNT", "Containerized");
			AssertContainsExactElementsInAnyOrder("ContainerModes", containerModeList, list);
		}
		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "A0001", "Test line 1", new ZDateTime(2019, 10, 31), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "A0002", "Test line 1", new ZDateTime(2019, 10, 31), new ZDateTime(2079, 6, 6));
			Factory.Save();
		}
	}
}
