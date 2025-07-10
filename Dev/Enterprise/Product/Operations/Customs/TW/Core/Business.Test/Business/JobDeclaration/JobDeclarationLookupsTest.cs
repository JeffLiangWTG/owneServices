using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryStatusListForDefaultFallBack()
		{
			var expectedList = Factory.GetCachedValue<EntryStatusCodeList>();
			var actualList = Factory.New<JobDeclaration>().Lookups.EntryStatusList;
			AssertEquals(expectedList, actualList);
		}

		public void TestMessageStatusList()
		{
			var expectedList = Factory.GetCachedValue<JobDeclarationMessageStatusList>();
			var actualList = Factory.New<JobDeclaration>().Lookups.MessageStatusList;
			AssertEquals(expectedList, actualList);
		}

		public void TestTransportTypeList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("The count of TransportTypeList is 2", 2, declaration.Lookups.TransportTypeList.Count);
				AssertEquals("The TransportTypeList value is 'AIR' and 'SEA'", "AIR, SEA", declaration.Lookups.TransportTypeList.CodesAsString);
				AssertEquals("The TransportTypeList does not contain 'OTH'  ", false, declaration.Lookups.TransportTypeList.ContainsCode("OTH"));
			});
		}

		public void TestDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration.Lookups.Declaration, declaration);
		}

		public void TestCargoIdTypeList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Customs.TW.Business.TransportTypeList.Codes.Sea;
			AssertEquals("CNT, BLK, BBK, EXP, OWN, MAI, HND, FTI, PIL, POL, OTH", declaration.Lookups.CargoIdTypeList.CodesAsString);
			declaration.JE_TransportMode = Customs.TW.Business.TransportTypeList.Codes.Air;
			AssertEquals("LSE, EXP, OWN, MAI, HND, FTI, PIL, POL, OTH", declaration.Lookups.CargoIdTypeList.CodesAsString);
			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("LSE, CNT, BLK, BBK, EXP, OWN, MAI, HND, FTI, PIL, POL, OTH", declaration.Lookups.CargoIdTypeList.CodesAsString);
		}

		public void TestCustomsOfficeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			new JobDeclarationLookups(declaration);
			var officeCode = declaration.Lookups.CustomsOfficeList;
			AssertNotNull(officeCode);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "RR", "BABA THE BUILDER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList.PK, Customs.TW.Business.TransportTypeList.Codes.Air);
			Factory.Save();
			declaration.JE_TransportMode = Customs.TW.Business.TransportTypeList.Codes.Air;
			AssertEquals(1, declaration.Lookups.CustomsOfficeList.Count);
			declaration.JE_TransportMode = Customs.TW.Business.TransportTypeList.Codes.Sea;
			AssertEquals(0, declaration.Lookups.CustomsOfficeList.Count);
		}

		public void TestLocationOfGoodsCollection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BA", "Taipei office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "ANP0060D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Taiwan);
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BA");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "BA";
			declaration.JE_LocationOfGoods = "ANP0060D";
			AssertEquals("BA", declaration.JE_CustomsOffice);
			var collection = (BusinessObjectCollection)declaration.Lookups.LocationOfGoodsCollection;
			var filters = collection.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["List Type:Property"];
			var attributeNameFilter = filters["Attribute Name:Property"];
			var attributeValueFilter = filters["Attribute Value:Property"];
			CombineAssertions(() =>
			{
				AssertEquals("FAC", listTypeFilter.Value);
				AssertEquals("CUSTOMSOFFICE", attributeNameFilter.Value);
				AssertEquals("BA", attributeValueFilter.Value);
			});
		}

		public void TestCusAgentsTWList()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_IsResource = true;
			var cantLoginStaff = Factory.NewWithValidTestData<GlbStaff>();
			cantLoginStaff.GS_CanLogin = false;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var cusAgentList = declaration.Lookups.CusAgentsTWList;
			CombineAssertions(() =>
			{
				AssertCollectionContains(cantLoginStaff, cusAgentList);
				AssertCollectionNotContains(resource, cusAgentList);
			});
		}

		public void TestProfileList()
		{
			new TestTWCreator(Factory).CreateBrokerStaff();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = "CYO";
			var customsProfileList = declaration.Lookups.CustomsProfileList;
			CombineAssertions(() =>
			{
				AssertEquals(1, customsProfileList.Count);
				AssertEquals(PasswordTypesList.Codes.UVC, customsProfileList.GetDescriptionFromCode("TBK0461-0"));
			});
		}

		public void TestOrgCusCodeList()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "DEVELOPER COMPANY";
			org1.OH_Code = "DVLPRCMPNY";
			var mainAddress = org1.MainAddress;
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_PostCode = "123";
			mainAddress.OA_City = "APPLE CITY";
			OrgCusCode orgCusCode1 = Factory.New<OrgCusCode>();
			orgCusCode1.OK_RN_NKCodeCountry = "AU";
			orgCusCode1.OK_CodeType = "AAA";
			orgCusCode1.OK_OH = org1.PK;
			orgCusCode1.OK_CustomsRegNo = "12345";
			OrgCusCode orgCusCode2 = Factory.New<OrgCusCode>();
			orgCusCode2.OK_RN_NKCodeCountry = "TW";
			orgCusCode2.OK_CodeType = "AAA";
			orgCusCode2.OK_OH = org1.PK;
			orgCusCode2.OK_CustomsRegNo = "AAAA";
			OrgCusCode orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = "TW";
			orgCusCode.OK_CodeType = "PBR";
			orgCusCode.OK_OH = org1.PK;
			orgCusCode.OK_CustomsRegNo = "987654";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = mainAddress.PK;
			var caseNoList = declaration.Lookups.CaseNoList;
			AssertEquals(1, caseNoList.Count);
			var caseNoOne = caseNoList[0];
			CombineAssertions(() =>
			{
				AssertEquals(orgCusCode.OK_CustomsRegNo, caseNoOne.Code);
				AssertEquals(orgCusCode.CompanyCodeAndPremisesAddresses, caseNoOne.Description);
			});
		}

		public void TestCaseNoList()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "DEVELOPER COMPANY";
			org1.OH_Code = "DVLPRCMPNY";
			var mainAddress = org1.MainAddress;
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_PostCode = "123";
			mainAddress.OA_City = "APPLE CITY";
			OrgCusCode orgCusCode3 = Factory.New<OrgCusCode>();
			orgCusCode3.OK_RN_NKCodeCountry = "AU";
			orgCusCode3.OK_CodeType = "AAA";
			orgCusCode3.OK_OH = org1.PK;
			orgCusCode3.OK_CustomsRegNo = "12345";
			OrgCusCode orgCusCode4 = Factory.New<OrgCusCode>();
			orgCusCode4.OK_RN_NKCodeCountry = "TW";
			orgCusCode4.OK_CodeType = "AAA";
			orgCusCode4.OK_OH = org1.PK;
			orgCusCode4.OK_CustomsRegNo = "AAAA";
			OrgCusCode orgCusCode1 = Factory.New<OrgCusCode>();
			orgCusCode1.OK_RN_NKCodeCountry = "TW";
			orgCusCode1.OK_CodeType = "PBR";
			orgCusCode1.OK_OH = org1.PK;
			orgCusCode1.OK_CustomsRegNo = "987654";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			var caseNoList = declaration.Lookups.CaseNoList;
			AssertEquals(0, caseNoList.Count);
			declaration.JE_OA_DeclarantAddress = mainAddress.PK;
			caseNoList = declaration.Lookups.CaseNoList;
			AssertEquals(1, caseNoList.Count);
			var caseNo1 = caseNoList[0];
			CombineAssertions(() =>
			{
				AssertEquals(orgCusCode1.OK_CustomsRegNo, caseNo1.Code);
				AssertEquals(orgCusCode1.CompanyCodeAndPremisesAddresses, caseNo1.Description);
			});
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "DEVELOPER COMPANY2";
			org2.OH_Code = "DVLPRCMPNY2";
			OrgCusCode orgCusCode5 = Factory.New<OrgCusCode>();
			orgCusCode5.OK_RN_NKCodeCountry = "AU";
			orgCusCode5.OK_CodeType = "AAA";
			orgCusCode5.OK_OH = org2.PK;
			orgCusCode5.OK_CustomsRegNo = "12345";
			OrgCusCode orgCusCode6 = Factory.New<OrgCusCode>();
			orgCusCode6.OK_RN_NKCodeCountry = "TW";
			orgCusCode6.OK_CodeType = "AAA";
			orgCusCode6.OK_OH = org2.PK;
			orgCusCode6.OK_CustomsRegNo = "AAAA";
			OrgCusCode orgCusCode2 = Factory.New<OrgCusCode>();
			orgCusCode2.OK_RN_NKCodeCountry = "TW";
			orgCusCode2.OK_CodeType = "PBR";
			orgCusCode2.OK_OH = org2.PK;
			orgCusCode2.OK_CustomsRegNo = "45678";
			Factory.Save();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = mainAddress.PK;
			declaration.JE_OH_Importer = org2.PK;
			caseNoList = declaration.Lookups.CaseNoList;
			AssertEquals(2, caseNoList.Count);
			List<ICodeDescription> list = new List<ICodeDescription>(declaration.Lookups.CaseNoList.ToArray());
			caseNo1 = list.Find(iCodeDescription =>
			{
				return ((OrgCusCode)iCodeDescription).OK_OH == org1.PK;
			});
			CombineAssertions(() =>
			{
				AssertEquals(orgCusCode1.OK_CustomsRegNo, caseNo1.Code);
				AssertEquals(orgCusCode1.CompanyCodeAndPremisesAddresses, caseNo1.Description);
			});
			var caseNo2 = list.Find(iCodeDescription =>
			{
				return ((OrgCusCode)iCodeDescription).OK_OH == org2.PK;
			});
			CombineAssertions(() =>
			{
				AssertEquals(orgCusCode2.OK_CustomsRegNo, caseNo2.Code);
				AssertEquals(orgCusCode2.CompanyCodeAndPremisesAddresses, caseNo2.Description);
			});
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			caseNoList = declaration.Lookups.CaseNoList;
			AssertEquals(1, caseNoList.Count);
			caseNo1 = caseNoList[0];
			CombineAssertions(() =>
			{
				AssertEquals(orgCusCode2.OK_CustomsRegNo, caseNo1.Code);
				AssertEquals(orgCusCode2.CompanyCodeAndPremisesAddresses, caseNo1.Description);
			});
		}

		public void TestPaymentsMethodList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var paymentListLookup = declaration.Lookups.PaymentMethodsList;
			CombineAssertions(() =>
			{
				AssertEquals(2, paymentListLookup.Count);
				AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<EXPPaymentMethod>(), paymentListLookup);
			});
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			paymentListLookup = declaration.Lookups.PaymentMethodsList;
			CombineAssertions(() =>
			{
				AssertEquals(8, paymentListLookup.Count);
				AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<IMPPaymentMethod>(), paymentListLookup);
			});
		}

		public void TestPackingUnitTypesListCore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Commercial Pack Units");
			helper.CreateCusCodeList("TW", "TWCIU", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			var packingUnitTypesList = declaration.Lookups.PackingUnitTypesList;
			CombineAssertions(() =>
			{
				AssertSame(declaration.Lookups.JE_TotalNoOfPacksPackType_List, packingUnitTypesList);
				AssertEquals(1, packingUnitTypesList.Count);
				AssertEquals("AMP", packingUnitTypesList[0].Code);
				AssertEquals("Ampere", packingUnitTypesList[0].Description);
			});
		}

		public void TestApplicationCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var list = declaration.Lookups.ApplicationCodeList;
			CombineAssertions(() =>
			{
				AssertNotNull("JobMessageTypeList can't be null", list);
				AssertEquals("ApplicationCodeList should have 2 values.", 2, list.Count);
			});
			var expectedList = new DeclarationApplicationCodeList();
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Description, expectedList.GetDescriptionFromCode(pair.Code));
			}
		}

		public void TestMergeByList()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var list = testDeclaration.Lookups.MergeByList;
			var mergeByCodeList = Factory.GetCachedValue<MergeByCodeList>();
			CombineAssertions(() =>
			{
				AssertEquals(mergeByCodeList.Count, list.Count);
				AssertEquals(mergeByCodeList, list);
				AssertEquals(4, list.Count);
			});
		}

		public void TestFinalDestinations()
		{
			var localPort = Factory.New<RefUNLOCO>();
			localPort.Code = "TW001";
			localPort.Description = "TW001";
			localPort.RL_HasAirport = true;
			localPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var foreignPort = Factory.New<RefUNLOCO>();
			foreignPort.Code = "US001";
			foreignPort.Description = "US001";
			foreignPort.RL_HasSeaport = true;
			foreignPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_RL_NKOrigin = foreignPort.RL_Code;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(false, foreignPort.MatchesFilter(filter));
			});

			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(false, localPort.MatchesFilter(filter));
				AssertEquals(false, foreignPort.MatchesFilter(filter));
			});

			dec.JE_RL_NKOrigin = localPort.RL_Code;
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(false, localPort.MatchesFilter(filter));
				AssertEquals(false, foreignPort.MatchesFilter(filter));
			});

			dec.JE_TransportMode = ZString.Empty;
			dec.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.NewZGuid();
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			dec.JE_RL_NKOrigin = ZString.Empty;
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.NewZGuid();
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(false, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.NewZGuid();
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(false, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.JE_MessageType = "Z!Z";
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.JE_MessageType = "EXP";
			dec.CusEntryInstruction.CEI_Style = "G3";
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(false, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_Style = "D1";
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(false, foreignPort.MatchesFilter(filter));
			});
		}

		public void TestPortOfArrivals()
		{
			var localPort = Factory.New<RefUNLOCO>();
			localPort.Code = "TW001";
			localPort.Description = "TW001";
			localPort.RL_HasAirport = false;
			localPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var foreignPort = Factory.New<RefUNLOCO>();
			foreignPort.Code = "US001";
			foreignPort.Description = "US001";
			foreignPort.RL_HasAirport = false;
			foreignPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var filter = dec.Lookups.PortOfArrivals.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.NewZGuid();
			filter = dec.Lookups.PortOfArrivals.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			filter = dec.Lookups.PortOfArrivals.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.NewZGuid();
			filter = dec.Lookups.PortOfArrivals.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.JE_MessageType = "Z!Z";
			dec.CusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.NewZGuid();
			filter = dec.Lookups.PortOfArrivals.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});
		}

		public void TestPortOfLoading()
		{
			var localPort = Factory.New<RefUNLOCO>();
			localPort.Code = "TW001";
			localPort.Description = "TW001";
			localPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var foreignPort = Factory.New<RefUNLOCO>();
			foreignPort.Code = "US001";
			foreignPort.Description = "US001";
			foreignPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var filter = dec.Lookups.PortOfLoadings.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.NewZGuid();
			filter = dec.Lookups.PortOfLoadings.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			filter = dec.Lookups.PortOfLoadings.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.NewZGuid();
			filter = dec.Lookups.PortOfLoadings.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.JE_MessageType = "Z!Z";
			dec.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.NewZGuid();
			filter = dec.Lookups.PortOfLoadings.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});
		}

		public void TestOrigins()
		{
			var localPort = Factory.New<RefUNLOCO>();
			localPort.Code = "TW001";
			localPort.Description = "TW001";
			localPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var foreignPort = Factory.New<RefUNLOCO>();
			foreignPort.Code = "US001";
			foreignPort.Description = "US001";
			foreignPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var filter = dec.Lookups.Origins.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(false, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.NewZGuid();
			filter = dec.Lookups.Origins.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(false, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			filter = dec.Lookups.Origins.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(false, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.NewZGuid();
			filter = dec.Lookups.Origins.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.JE_MessageType = "Z!Z";
			dec.CusEntryInstruction.CEI_OA_Warehouse = ZGuid.NewZGuid();
			filter = dec.Lookups.Origins.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.JE_MessageType = "IMP";
			dec.CusEntryInstruction.CEI_Style = "G1";
			filter = dec.Lookups.Origins.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(false, localPort.MatchesFilter(filter));
				AssertEquals(true, foreignPort.MatchesFilter(filter));
			});

			dec.CusEntryInstruction.CEI_Style = "G2";
			filter = dec.Lookups.Origins.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals(true, localPort.MatchesFilter(filter));
				AssertEquals(false, foreignPort.MatchesFilter(filter));
			});
		}

		public void TestEntryStatusList()
		{
			var expectedList = Factory.GetCachedValue<EntryStatusCodeList>();
			var actualList = Factory.New<JobDeclaration>().Lookups.EntryStatusList;
			AssertEquals(expectedList, actualList);
		}

		public void TestClearanceStatusList()
		{
			var expectedList = Factory.GetCachedValue<ClearanceStatusCodeList>();
			var actualList = Factory.New<JobDeclaration>().Lookups.ClearanceStatusList;
			AssertEquals(expectedList, actualList);
		}

		public void TestMessageTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var list = declaration.Lookups.MessageTypeList;
			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertEquals("EXP, IMP", list.CodesAsString);
			});
		}

		public void TestDeclarantOrganisationsFindBoxCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<OrganisationsFindBoxCollection>(declaration.Lookups.DeclarantOrganisationsFindBoxCollection);
		}

		public void TestVessels()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(declaration);
			AssertType<TWRefVesselCollection>(lookups.Vessels);
		}

		public void TestDeclDocTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = declaration.Lookups;
			var expectedExportList = Factory.GetCachedValue<ExportDeclDocTypeList>();
			var expectedImportList = Factory.GetCachedValue<ImportDeclDocTypeList>();
			declaration.JE_MessageType = ZString.Empty;
			var actualList = lookups.DeclDocTypeList;
			AssertEquals(0, actualList.Count);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			actualList = lookups.DeclDocTypeList;
			CombineAssertions(() =>
			{
				AssertEquals(6, actualList.Count);
				AssertEquals(expectedExportList, actualList);
				AssertEquals("1, 3, 4, 5, 6, 7", actualList.CodesAsString);
			});

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			actualList = lookups.DeclDocTypeList;
			CombineAssertions(() =>
			{
				AssertEquals(5, actualList.Count);
				AssertEquals(expectedImportList, actualList);
				AssertEquals("1, 2, 3, 4, 5", actualList.CodesAsString);
			});
		}

		public void TestTWTransportCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = declaration.Lookups;
			var list = lookups.TWTransportCodeList;
			CombineAssertions(() =>
			{
				AssertSame(Factory.GetCachedValue<TransportCodeList>(), list);
				AssertEquals(16, list.Count);
			});
		}
	}
}
