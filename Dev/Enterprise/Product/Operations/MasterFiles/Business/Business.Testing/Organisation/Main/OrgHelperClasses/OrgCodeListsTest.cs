using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCodeListsTest : TestCaseWithFactory
	{
		public void TestGetCompanyTransportModes()
		{
			OrgCompanyDataLookupsTest.InsertCompanyTariff(1, "Tariff 1");

			foreach (string tariffType in tariffTypes)
			{
				AssertModes(tariffType);
			}
		}

		public static string[] tariffTypes = new[] { "DEF", "FRT", "ORG", "DST", "SFR", "SOR", "SDE", "SCD", "CFS", "WHS", "TRN", "CYD", "TBC" };

		void AssertModes(string code)
		{
			OrgCompanyDataLookupsTest.AssertModes(code, false, new OrgCodeLists().GetCompanyTransportModes(Factory, GlbCompany.CurrentCompany.PK, code));
		}

		public void TestAddressType_List()
		{
			AssertEquals("AddressType_List.Count", 13, OrgCodeLists.AddressType_List(Factory).Count);
			Assert("Address types should contain OFC.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.Office));
			Assert("Address types should contain PST.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.Postal));
			Assert("Address types should contain ARM.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.Receivables));
			Assert("Address types should contain APM.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.Payables));
			Assert("Address types should contain SQM.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.Sales));
			Assert("Address types should contain PIC.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.Pickup));
			Assert("Address types should contain DLV.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.Delivery));
			Assert("Address types should contain PAD.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.PickupAndDelivery));
			Assert("Address types should contain MSC.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.Miscellaneous));
			Assert("Address types should contain RSD.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.Residential));
			Assert("Address types should contain CST.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.CustomsAddressOfRecord));
			Assert("Address types should contain AWB.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.AWB));
			Assert("Address types should contain ECA.", OrgCodeLists.AddressType_List(Factory).ContainsCode(OrgConstants.AddressType.EUCustomsAddress));
		}

		public void TestAttachmentType_List()
		{
			AssertEquals("AttachmentType_List.Count", 8, OrgCodeLists.AttachmentType_List.Count);
			Assert("Attachment types should contain XLS.", OrgCodeLists.AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Xls));
			Assert("Attachment types should contain XLSX.", OrgCodeLists.AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			Assert("Attachment types should contain PDF.", OrgCodeLists.AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Pdf));
			Assert("Attachment types should contain PDF/A.", OrgCodeLists.AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Pdfa));
			Assert("Attachment types should contain PDFC.", OrgCodeLists.AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Pdfc));
			Assert("Attachment types should contain TIF.", OrgCodeLists.AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Tif));
			Assert("Attachment types should contain HTML.", OrgCodeLists.AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Html));
			Assert("Attachment types should contain HTMF.", OrgCodeLists.AttachmentType_List.ContainsCode(AttachmentTypeList.Codes.Htmf));
		}

		public void TestContactType_List()
		{
			AssertEquals("ContactType_List.Count", 43, OrgCodeLists.ContactType_List.Count);
			AssertEquals("Contact types should contain TWH.", true, OrgCodeLists.ContactType_List.ContainsCode(ContactType.TransitWarehouse));
			AssertEquals("Contact types should contain VGM.", true, OrgCodeLists.ContactType_List.ContainsCode(ContactType.VerifiedGrossWeightContact));
			AssertEquals("Contact types should contain NPS.", true, OrgCodeLists.ContactType_List.ContainsCode(ContactType.NettingParticipantStatement));
			AssertEquals("Contact types should contain NCJ.", true, OrgCodeLists.ContactType_List.ContainsCode(ContactType.NettingClearingJournal));
			AssertEquals("Contact types should contain CCU.", true, OrgCodeLists.ContactType_List.ContainsCode(ContactType.ControllingCustomer));
			AssertEquals("Contact types should contain CAG.", true, OrgCodeLists.ContactType_List.ContainsCode(ContactType.ControllingAgent));
			AssertEquals("Contact types should contain DEC.", true, OrgCodeLists.ContactType_List.ContainsCode(ContactType.Declarant));
			AssertEquals("Contact types should contain PRI.", true, OrgCodeLists.ContactType_List.ContainsCode(ContactType.Principal));
		}

		public void TestContactType_List_OverridableContactTypeListDelegate()
		{
			AssertEquals("ContactType_List.Count", 43, OrgCodeLists.ContactType_List.Count);

			var list = new CodeDescriptionPairList();
			list.AddPair("#01", "#01");
			list.AddPair("#02", "#02");
			OrgCodeListsForTest.AppendContactTypes(list);
			AssertEquals("ContactType_List.Count", 45, OrgCodeLists.ContactType_List.Count);
			Assert(OrgCodeLists.ContactType_List.ContainsCode("#01"));
			Assert(OrgCodeLists.ContactType_List.ContainsCode("#02"));
		}

		class OrgCodeListsForTest : OrgCodeLists
		{
			public static void AppendContactTypes(CodeDescriptionPairList types)
			{
				OverridableContactTypeListDelegate.Value = (list) =>
				{
					list.AddRange(types);
					return list;
				};
			}
		}

		public void TestEnsureAllCountriesIncludeLSCCode()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			RefCountry[] countryList = (RefCountry[])factory.Load(typeof(RefCountry), new ZQuery());
			StringBuilder sb = new StringBuilder();

			foreach (RefCountry country in countryList)
			{
				CodeDescriptionPairList orgCodes = new OrgCodeLists().CustomsCodes_List(country);
				bool legacyCodeFound = false;
				foreach (CodeDescriptionPair codePair in orgCodes)
				{
					if (codePair.Code == "LSC")
					{
						legacyCodeFound = true;
						break;
					}
				}

				if (!legacyCodeFound)
				{
					sb.AppendLine(country.RN_Code);
				}
			}
			Assert("Countries which don't have Legacy System Code (LSC) established:\n\r" + sb.ToString(), sb.ToString().Length == 0);
		}

		public void TestInvoiceLineGroupings_List()
		{
			AssertEquals("InvoiceLineGroupings_List.Count", 14, OrgCodeLists.InvoiceLineGroupings_List.Count);
			Assert("Base list shoudl contain CCG", OrgCodeLists.InvoiceLineGroupings_List.ContainsCode(OrgConstants.InvoiceLineGroupings.Code.CCG));
		}

		public void TestPaymentMethod_List()
		{
			AssertEquals("PaymentMethod_List.Count", 4, OrgCodeLists.PaymentMethod_List.Count);

			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("PaymentMethod_List.Count", 2, OrgCodeLists.PaymentMethod_List.Count);
			Assert(OrgCodeLists.PaymentMethod_List.ContainsCode(Customs.PaidByCodeList.Codes.BRK));
			Assert(OrgCodeLists.PaymentMethod_List.ContainsCode(Customs.PaidByCodeList.Codes.CLI));

			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore);
			AssertEquals("PaymentMethod_List.Count", 2, OrgCodeLists.PaymentMethod_List.Count);
			Assert(OrgCodeLists.PaymentMethod_List.ContainsCode(Customs.Asycuda.SGPayeeIndicatorList.Codes.Q));
			Assert(OrgCodeLists.PaymentMethod_List.ContainsCode(Customs.Asycuda.SGPayeeIndicatorList.Codes.T));
		}

		public void TestSendImportDocsTo_List()
		{
			AssertEquals("SendImportDocsTo_List.Count", 3, OrgCodeLists.SendImportDocsTo_List.Count);
		}

		public void TestState_List()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			RefUNLOCO uNLOCO = null;
			AssertEquals("Code list count", 0, new OrgCodeLists().State_List(uNLOCO).Count);

			uNLOCO = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			RefCountry country = uNLOCO.Country;
			AssertNotNull(country);
			CodeDescriptionPairList countryList = new OrgCodeLists().State_List(country);
			CodeDescriptionPairList unlocoList = new OrgCodeLists().State_List(uNLOCO);
			CodeDescriptionPairList countryPKList = new OrgCodeLists().State_List(country.Factory, country.RN_Code);
			AssertEquals("Code list for Country count", 8, countryList.Count);
			AssertEquals("Code list for UNLOCO count", 8, unlocoList.Count);
			AssertEquals("Code list for Country PK count", 8, countryPKList.Count);

			for (int i = 0; i < 8; i++)
			{
				AssertEquals("Item " + i.ToString(), countryList[i], unlocoList[i]);
			}

			uNLOCO = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
			AssertEquals("Code list count", 0, new OrgCodeLists().State_List(uNLOCO).Count);
		}

		public void TestStateList_WithCountryCode_ExcludeInactiveState()
		{
			// Arrange.

			var country = Factory.New<RefCountry>();
			country.RN_Code = "W8";

			var activeState = Factory.New<RefCountryStates>();
			activeState.RW_RN_NKCountryCode = country.RN_Code;
			activeState.RW_Code = "MAS";
			activeState.RW_Description = "[_MOCK_ACTIVE_STATE_]";
			activeState.RW_IsActive = true;

			var inactiveState = Factory.New<RefCountryStates>();
			inactiveState.RW_RN_NKCountryCode = country.RN_Code;
			inactiveState.RW_Code = "MIS";
			inactiveState.RW_Description = "[_MOCK_INACTIVE_STATE_]";
			inactiveState.RW_IsActive = false;

			// Act.

			var states = new OrgCodeLists().State_List(Factory, "W8");

			// Assert.

			AssertEquals(1, states.Count);

			var state = states[0];
			AssertEquals("MAS", state.Code);
			AssertEquals("[_MOCK_ACTIVE_STATE_]", state.Description);
		}

		public void TestStateList_WithCountry_ExcludeInactiveState()
		{
			// Arrange.

			var country = Factory.New<RefCountry>();
			country.RN_Code = "W8";

			var activeState = Factory.New<RefCountryStates>();
			activeState.RW_RN_NKCountryCode = country.RN_Code;
			activeState.RW_Code = "MAS";
			activeState.RW_Description = "[_MOCK_ACTIVE_STATE_]";
			activeState.RW_IsActive = true;

			var inactiveState = Factory.New<RefCountryStates>();
			inactiveState.RW_RN_NKCountryCode = country.RN_Code;
			inactiveState.RW_Code = "MIS";
			inactiveState.RW_Description = "[_MOCK_INACTIVE_STATE_]";
			inactiveState.RW_IsActive = false;

			// Act.

			var states = new OrgCodeLists().State_List(country);

			// Assert.

			AssertEquals(1, states.Count);

			var state = states[0];
			AssertEquals("MAS", state.Code);
			AssertEquals("[_MOCK_ACTIVE_STATE_]", state.Description);
		}

		public void TestStateList_WithUnloco_ExcludeInactiveState()
		{
			// Arrange.

			var country = Factory.New<RefCountry>();
			country.RN_Code = "W8";

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "W8FOO";
			unloco.RL_RN_NKCountryCode = country.RN_Code;

			var activeState = Factory.New<RefCountryStates>();
			activeState.RW_RN_NKCountryCode = country.RN_Code;
			activeState.RW_Code = "MAS";
			activeState.RW_Description = "[_MOCK_ACTIVE_STATE_]";
			activeState.RW_IsActive = true;

			var inactiveState = Factory.New<RefCountryStates>();
			inactiveState.RW_RN_NKCountryCode = country.RN_Code;
			inactiveState.RW_Code = "MIS";
			inactiveState.RW_Description = "[_MOCK_INACTIVE_STATE_]";
			inactiveState.RW_IsActive = false;

			// Act.

			var states = new OrgCodeLists().State_List(unloco);

			// Assert.

			AssertEquals(1, states.Count);

			var state = states[0];
			AssertEquals("MAS", state.Code);
			AssertEquals("[_MOCK_ACTIVE_STATE_]", state.Description);
		}

		public void TestEnsureThereIsNoDuplicateCodesForOneCountry()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.UnitedKingdom).RN_EconomicGrouping = ZString.Empty;
			RefCountry[] countries = factory.Load<RefCountry>(new ZQuery());
			StringBuilder sb = new StringBuilder();

			foreach (RefCountry country in countries)
			{
				CodeDescriptionPairList orgCodes = new OrgCodeLists().CustomsCodes_List(country);

				for (int i = orgCodes.Count; i > 0; i--)
				{
					ICodeDescription codePair = orgCodes[i - 1];

					orgCodes.RemoveAt(i - 1);

					if (orgCodes.ContainsCode(codePair.Code))
					{
						sb.AppendLine(country.Code + " has more than 1 code: " + codePair.Code);
					}
				}
			}
			Assert(sb.ToString(), sb.ToString().Length == 0);
		}

		public void TestCustomsCodes_ListContainsTCU()
		{
			var testObject = new OrgCodeListsForTest();

			CombineAssertions(() =>
			{
				AssertEquals("No TCU in DE", false, testObject.CustomsCodes_List("DE").CodesAsString.Contains("TCU"));
				AssertEquals("TCU enabled in PL", true, testObject.CustomsCodes_List("PL").CodesAsString.Contains("TCU"));
			});
		}

		public void TestGetMergeInvoiceLinesByListByCountryCode()
		{
			AssertEquals("NON, TRD, PNO, TRF, DEF", OrgCodeLists.GetMergeInvoiceLinesByListByCountryCode("TW").CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, PNO, PNP, DEF", OrgCodeLists.GetMergeInvoiceLinesByListByCountryCode("CN").CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP, TRM, DEF", OrgCodeLists.GetMergeInvoiceLinesByListByCountryCode("CA").CodesAsString);
		}

		public void TestGetMergeByListByCountryCode()
		{
			AssertEquals("NON, TRD, PNO, TRF", OrgCodeLists.GetMergeByListByCountryCode("TW").CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, PNO, PNP", OrgCodeLists.GetMergeByListByCountryCode("CN").CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP, TRM", OrgCodeLists.GetMergeByListByCountryCode("CA").CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();

			NewCompanyAndBranch("TW", "TWX");
			NewCompanyAndBranch("CN", "CNX");
			NewCompanyAndBranch("CA", "CAX");
			NewCompanyAndBranch("FR", "FRX");
			NewCompanyAndBranch("TR", "TRX");
			Factory.Save();
		}

		void NewCompanyAndBranch(string countryCode, string code)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			company.GC_Code = code;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = code;
		}
	}
}
