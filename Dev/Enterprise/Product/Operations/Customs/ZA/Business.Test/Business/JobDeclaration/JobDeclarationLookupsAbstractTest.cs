using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	abstract class JobDeclarationLookupsAbstractTest : BusinessObjectLookupsTestCase
	{
		public void TestRadioCallSignVessels()
		{
			AssertType<RadioCallSignCodeFindBoxCollection>(declaration.Lookups.RadioCallSignVessels);
		}

		public void TestImporterList()
		{
			var importersList = declaration.Lookups.ImportersList;
			AssertNotNull(importersList);
			AssertEquals("ImportersListType", typeof(ConsigneeCollection), importersList.GetType());
		}

		public void TestCarrierCodes()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertNotNull("List can be accessed without error", dec.Lookups.CarrierCodeCollection);
		}

		public void TestMessageStatusList()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertSame(Factory.GetCachedValue<ZAMessageStatusList>(), dec.Lookups.MessageStatusList);
		}

		public void TestCargoCarrierCodeList()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertNotNull("List can be accessed without error", dec.Lookups.CargoCarrierCodeList);
		}

		public void TestLocationOfGoodsCollection()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			var bbrCode = testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			Factory.Save();

			using (ZACustomsRegistry.Instance.CustomsOfficeCode.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, bbrCode.PK.ToGuid()))
			{
				var declaration1 = Factory.New<JobDeclaration>();
				AssertEquals("BBR", declaration1.JE_CustomsOffice);

				var declaration2 = Factory.New<JobDeclaration>();
				var collection1 = (BusinessObjectCollection)declaration1.Lookups.LocationOfGoodsCollection;
				var collection2 = (BusinessObjectCollection)declaration2.Lookups.LocationOfGoodsCollection;

				AssertEquals("IsCached", collection1, collection2);

				var filters = collection1.FilterBusinessObjectDefaults;
				var listTypeFilter = filters["List Type:Property"];
				var attributeNameFilter = filters["Attribute Name:Property"];
				var attributeValueFilter = filters["Attribute Value:Property"];
				AssertEquals("FAC", listTypeFilter.Value);
				AssertEquals("DistrictOffices", attributeNameFilter.Value);
				AssertEquals("BBR", attributeValueFilter.Value);
			}
		}

		public void TestEntryStatusLookups()
		{
			AssertNotNull(declaration.Lookups.EntryStatusList);
			AssertEquals("EntryStatusListType", declaration.Lookups.EntryStatusList.GetType(), typeof(ZAMessageStatusList).BaseType);
		}

		public void TestJobMessageTypeList()
		{
			var list = declaration.Lookups.MessageTypeList;
			AssertNotNull("ZAJobMessageTypeList", list);
			var expectedList = new ZAJobMessageTypeList();
			AssertEquals("ZAJobMessageTypeList", true, expectedList.Count >= list.Count);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Description, expectedList.GetDescriptionFromCode(pair.Code));
			}

			AssertEquals("Should not have IMX", false, declaration.Lookups.MessageTypeList.ContainsCode("IMX"));
			declaration.JE_ApplicationCode = "BLT";
			AssertEquals("Should not have IMX", true, declaration.Lookups.MessageTypeList.ContainsCode("IMX"));
		}

		public void TestApplicationCodeList()
		{
			var list = declaration.Lookups.ApplicationCodeList;
			AssertNotNull("ZAJobMessageTypeList", list);
			var expectedList = new Customs.Business.DeclarationApplicationCodeList();
			AssertEquals("ZAJobMessageTypeList", true, expectedList.Count >= list.Count);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Description, expectedList.GetDescriptionFromCode(pair.Code));
			}
		}

		public void TestTransportTypeList()
		{
			var tester = declaration.Lookups.TransportTypeList;
			AssertContainsExactElementsInAnyOrder(new string[] { "", "AIR", "SEA", "RAI", "MAI", "FIX", "ROA", "OTH" }, tester.GetAllCodes());
		}

		public void TestRemovalTransportCodeList()
		{
			var tester = declaration.Lookups.RemovalTransportCodeList;
			var actual = from ICodeDescription item in tester select item.Code;
			AssertContainsExactElementsInAnyOrder(new string[] { "", "AIR", "SEA", "RAI", "MAI", "FIX", "ROA", "OTH" }, actual);
		}

		public void TestROOTypesList()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROOType, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.CountryCodes.SouthAfrica);
			var codeList1 = testHelper.CreateAdditionalInformationCusCodeEntry("XX");
			codeList1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty);
			var codeList2 = testHelper.CreateAdditionalInformationCusCodeEntry("YY");
			codeList2.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty);
			Factory.Save();
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			var testLookups = declaration1.Lookups;
			CombineAssertions("Test ROOTypesList", () =>
			{
				Assert("Test 1", testLookups.ROOTypesList.ContainsCode("XX"));
				Assert("Test 2", testLookups.ROOTypesList.ContainsCode("YY"));
				var list1 = declaration1.Lookups.ROOTypesList;
				var list2 = declaration2.Lookups.ROOTypesList;
				AssertSame("IsCached", list1, list2);
			});
		}

		public void TestOrganisations()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertNotNull(declaration.Lookups.Organisations);
			AssertEquals("Type of AddInfo.Lookups.Organisations should be OrgHeaderCollection", typeof(OrgHeaderCollection), declaration.Lookups.Organisations.GetType());
		}

		public void TestVesselAgentList()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.VesselAgent, "Vessel Agent Code");
			testHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.VesselAgent, "AMAL", "AMAL DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var vesselAgentList = declaration.Lookups.VesselAgentList;
			vesselAgentList.Load();
			AssertNotNull(vesselAgentList);
			AssertEquals(1, vesselAgentList.Count);
			AssertEquals("AMAL", vesselAgentList[0].ZZD_Code);
			AssertEquals("AMAL DESC", vesselAgentList[0].ZZD_Description);
		}

		public void TestVATClaimBackIndicatorList()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var lookup = testDeclaration.Lookups;
			var testList = lookup.VATClaimBackIndicator;
			AssertNotNull(testList);
			AssertEquals(2, testList.Count);
			Assert("Code Y", testList.ContainsCode("Y"));
			Assert("Code N", testList.ContainsCode("N"));
		}

		public void TestRefTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertContainsExactElementsInAnyOrder(new[] { "I", "C", "P", "T" }, declaration.Lookups.EntityTypeList.GetAllCodes());
		}

		public void TestEntityTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertContainsExactElementsInAnyOrder(new[] { "CON", "CUS", "DCL", "DEL", "INF", "INV", "OTH", "PON" }, declaration.Lookups.RefTypeList.GetAllCodes());
		}

		public void TestScopeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertContainsExactElementsInAnyOrder(new[] { "M", "S" }, declaration.Lookups.ScopeList.GetAllCodes());
		}

		delegate ICodeDescriptionPairList GetListDelegate(JobDeclarationLookups lookups);
		public void TestLookupsListAreCached()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertListIsCached(
				new KeyValuePair<ICodeDescriptionPairList, GetListDelegate>(declaration.Lookups.PortsOfExit, x => x.PortsOfExit),
				new KeyValuePair<ICodeDescriptionPairList, GetListDelegate>(declaration.Lookups.BankCodes, x => x.BankCodes),
				new KeyValuePair<ICodeDescriptionPairList, GetListDelegate>(declaration.Lookups.ProvisionalPaymentTypes, x => x.ProvisionalPaymentTypes),
				new KeyValuePair<ICodeDescriptionPairList, GetListDelegate>(declaration.Lookups.EntityTypeList, x => x.EntityTypeList),
				new KeyValuePair<ICodeDescriptionPairList, GetListDelegate>(declaration.Lookups.RefTypeList, x => x.RefTypeList),
				new KeyValuePair<ICodeDescriptionPairList, GetListDelegate>(declaration.Lookups.ScopeList, x => x.ScopeList));
		}

		void AssertListIsCached(params KeyValuePair<ICodeDescriptionPairList, GetListDelegate>[] listMatchTypes)
		{
			var lookup = Factory.New<JobDeclaration>().Lookups;
			foreach (var pair in listMatchTypes)
			{
				var cachedList = pair.Key;
				var getList = pair.Value;
				AssertSame("lookup", cachedList, getList(lookup));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
		}

		protected JobDeclaration declaration;
	}
}
