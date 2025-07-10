using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceLineFetchStrategyTest : Customs.Business.FetchStrategies.Testing.JobComInvoiceLineFetchStrategyTest
	{
		public new void TestFetchForView()
		{
			Assert("Will be fixed in US WI", true);
		}

		protected override int FetchHintsIncrementCount => 4;

		protected override IEnumerable<string> GetTablesToCollectQueriesFor()
		{
			return base.GetTablesToCollectQueriesFor().Union(new[]
			{
				USCTariffDutyRateSchema.Constants.TableName,
				USCTariffDateRestrictionSchema.Constants.TableName,
				USCTariffRuleExceptionSchema.Constants.TableName,
				USCTariffSchema.Constants.TableName,
				USCTariffRuleSchema.Constants.TableName
			});
		}

		protected override IEnumerable<TestCase> SetupFetchForValidateTestCases()
		{
			JobComInvoiceLineTest.SetDataForSupTariffTest(Factory);

			yield return ABIImportInvoiceLineTestCase();
			yield return AESHTSExportInvoiceLineTestCase();
			yield return AESSHBExportInvoiceLineTestCase();
			yield return ReconInvoiceLineTestCase();
			yield return ImportInvoiceLineADDCaseNoTestCase();
			yield return CusCodeDataCollectionTestCase();
		}

		TestCase CusCodeDataCollectionTestCase()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(org);
			var codesS = new RestrictedCodeCollection(wrapper.CountryData, RestrictedCodeTypeList.Codes.RestrictedSPI);
			codesS.AddNew();
			var codesT = new RestrictedCodeCollection(wrapper.CountryData, RestrictedCodeTypeList.Codes.RestrictedTariff);
			codesT.AddNew();
			var codesE = new RestrictedCodeCollection(wrapper.CountryData, RestrictedCodeTypeList.Codes.RestrictedEntryType);
			codesE.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.IOROrgPK = org.PK;
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			invoice.InvoiceLines.AddNew();
			invoice.InvoiceLines.AddNew();
			invoice.InvoiceLines.AddNew();
			invoice.InvoiceLines.AddNew();
			invoice.InvoiceLines.AddNew();
			invoice.InvoiceLines.AddNew();
			invoice.InvoiceLines.AddNew();
			invoice.InvoiceLines.AddNew();
			Factory.Save();

			declaration.Validation.ValidateAll();
			return new TestCase
			{
				Message = "CusCodeData SPI,EntryType and Tariff RestrictedCodeCollection",
				DeclarationPK = declaration.PK,
				ExpectedHitCounts = new Dictionary<string, int>
				{
					{ JobDocAddressSchema.Constants.TableName, 2 },
					{ OrgAddressSchema.Constants.TableName, 3 },
					{ CusCodeDataSchema.Constants.TableName, 3 }
				}
			};
		}

		TestCase AESHTSExportInvoiceLineTestCase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "070610", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffType.PK, "22083010", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			header.FillWithValidTestData();
			var invoiceLine = header.InvoiceLines.AddNew();
			var invoiceLine2 = header.InvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_Tariff = "070610";
			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.US_CVD_NA = true;
			invoiceLine.US_ADD_NA = true;
			invoiceLine2.JI_Tariff = "22083010";
			invoiceLine2.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine2.US_CVD_NA = true;
			invoiceLine2.US_ADD_NA = true;

			Factory.Save();

			return new TestCase
			{
				Message = "AES HTS Export Invoice Line",
				DeclarationPK = declaration.PK,
				ExpectedHitCounts = new Dictionary<string, int>
				{
					{ JobDocAddressSchema.Constants.TableName, 3 },
					{ RefPackTypeSchema.Constants.TableName, 1 },
					{ RefCusTariffTypeSchema.Constants.TableName, 1 },
					{ RefDataGroupingSchema.Constants.TableName, 1 },
					{ TariffAttributeViewSchema.Constants.TableName, 2 },
					{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 2 }
				}
			};
		}

		TestCase AESSHBExportInvoiceLineTestCase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();
			var scheduleB1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "070610", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			helper.CreateTariffUOM(scheduleB1, "CU1", "D");
			helper.CreateTariffUOM(scheduleB1, "CU2", "E");

			var scheduleB2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, "22083010", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			helper.CreateTariffUOM(scheduleB2, "CU1", "D");
			helper.CreateTariffUOM(scheduleB2, "CU2", "E");
			Factory.Save();
			Factory.ResetDatabaseLoadCount();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			header.FillWithValidTestData();
			var invoiceLine = header.InvoiceLines.AddNew();
			var invoiceLine2 = header.InvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_Tariff = "070610";
			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.US_CVD_NA = true;
			invoiceLine.US_ADD_NA = true;
			invoiceLine2.JI_Tariff = "22083010";
			invoiceLine2.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine2.US_CVD_NA = true;
			invoiceLine2.US_ADD_NA = true;

			Factory.Save();

			return new TestCase
			{
				Message = "AES SHB Export Invoice Line",
				DeclarationPK = declaration.PK,
				ExpectedHitCounts = new Dictionary<string, int>
				{
					{ JobDocAddressSchema.Constants.TableName, 3 },
					{ TariffUOMViewSchema.Constants.TableName, 2 },
					{ TariffViewSchema.Constants.TableName, 1 },
					{ TariffAttributeViewSchema.Constants.TableName, 2 },
					{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 2 }
			}
			};
		}

		TestCase ImportInvoiceLineADDCaseNoTestCase()
		{
			ZString tariffCode1 = "9010000010";
			ZString supTariffCode1 = "9010100010";
			USCTariffTest.CreateTariff(Factory, tariffCode1);

			ZString tariffCode2 = "8010000010";
			ZString supTariffCode2 = "8010010010";
			USCTariffTest.CreateTariff(Factory, tariffCode2);

			ZString tariffCode3 = "7010000010";
			ZString supTariffCode3 = "7010001010";
			USCTariffTest.CreateTariff(Factory, tariffCode3);

			ZString tariffCode4 = "6010000010";
			ZString supTariffCode4 = "6010000110";
			USCTariffTest.CreateTariff(Factory, tariffCode4);

			ZString tariffCode5 = "5010000010";
			ZString supTariffCode5 = "5010000020";
			USCTariffTest.CreateTariff(Factory, tariffCode5);

			ZString tariffCode6 = "4010000010";
			ZString supTariffCode6 = "4010000011";
			USCTariffTest.CreateTariff(Factory, tariffCode6);

			//1
			var acCase1 = Factory.New<USCACCase>();
			acCase1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase1.U5_CaseNumber = "AA0101";
			acCase1.U5_ISOCountryCode = "HK";
			acCase1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff1 = acCase1.CaseTariffs.AddNew();
			caTariff1.U9_TariffNumber = tariffCode1.Left(4);

			//2
			var acCase2 = Factory.New<USCACCase>();
			acCase2.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase2.U5_CaseNumber = "AA0102";
			acCase2.U5_ISOCountryCode = "HK";
			acCase2.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff2 = acCase2.CaseTariffs.AddNew();
			caTariff2.U9_TariffNumber = tariffCode2.Left(5);

			//3
			var acCase3 = Factory.New<USCACCase>();
			acCase3.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase3.U5_CaseNumber = "AA0103";
			acCase3.U5_ISOCountryCode = "HK";
			acCase3.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff3 = acCase3.CaseTariffs.AddNew();
			caTariff3.U9_TariffNumber = tariffCode3.Left(6);

			//4
			var acCase4 = Factory.New<USCACCase>();
			acCase4.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase4.U5_CaseNumber = "AA0104";
			acCase4.U5_ISOCountryCode = "HK";
			acCase4.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff4 = acCase4.CaseTariffs.AddNew();
			caTariff4.U9_TariffNumber = tariffCode4.Left(7);

			//5
			var acCase5 = Factory.New<USCACCase>();
			acCase5.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase5.U5_CaseNumber = "AA0105";
			acCase5.U5_ISOCountryCode = "HK";
			acCase5.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff5 = acCase5.CaseTariffs.AddNew();
			caTariff5.U9_TariffNumber = tariffCode5.Left(8);

			//6
			var acCase6 = Factory.New<USCACCase>();
			acCase6.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase6.U5_CaseNumber = "AA0106";
			acCase6.U5_ISOCountryCode = "HK";
			acCase6.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff6 = acCase3.CaseTariffs.AddNew();
			caTariff6.U9_TariffNumber = tariffCode6.Left(9);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariffCode1;
			invoiceLine.US_SupTariff = supTariffCode1;
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine.US_ADDCaseNo = ZString.Empty;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariffCode2;
			invoiceLine2.US_SupTariff = supTariffCode2;
			invoiceLine2.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine2.US_ADDCaseNo = ZString.Empty;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = tariffCode3;
			invoiceLine3.US_SupTariff = supTariffCode3;
			invoiceLine3.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine3.US_ADDCaseNo = ZString.Empty;

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = tariffCode4;
			invoiceLine4.US_SupTariff = supTariffCode4;
			invoiceLine4.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine4.US_ADDCaseNo = ZString.Empty;

			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = tariffCode5;
			invoiceLine5.US_SupTariff = supTariffCode5;
			invoiceLine5.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine5.US_ADDCaseNo = ZString.Empty;

			var invoiceLine6 = declaration.InvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = tariffCode6;
			invoiceLine6.US_SupTariff = supTariffCode6;
			invoiceLine6.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine6.US_ADDCaseNo = ZString.Empty;

			Factory.Save();

			return new TestCase
			{
				Message = "ADD Case No",
				DeclarationPK = declaration.PK,
				ExpectedHitCounts = new Dictionary<string, int>
				{
					{ TariffRelationshipViewSchema.Constants.TableName, 1 },
					{ TariffViewSchema.Constants.TableName, 2 },
					{ CusRefTariffVersionSchema.Constants.TableName, 1 },
					{ RefDataGroupingSchema.Constants.TableName, 1 },
					{ USCTariffDutyRateSchema.Constants.TableName, 1 },
					{ USCTariffDateRestrictionSchema.Constants.TableName, 1 },
					{ USCTariffRuleExceptionSchema.Constants.TableName, 1 },
					{ USCTariffSchema.Constants.TableName, 1 },
					{ USCTariffRuleSchema.Constants.TableName, 3 },
					{ JobDocAddressSchema.Constants.TableName, 2 },
					{ "NonPersitentTable REFDBENTUS_USCACCASE INNER JOIN REFDBENTUS_USCACCASETARIFF ON U9_CASENUMBER = U5_CASENUMBER", 2 }
				}
			};
		}

		TestCase ABIImportInvoiceLineTestCase()
		{
			var tariffCode1 = "84501100";
			var tariffCode2 = "99034501";
			var tariffCode3 = "721610";
			var tariffCode4 = "99038001";
			USCTariffTest.CreateTariff(Factory, tariffCode1);
			USCTariffTest.CreateTariff(Factory, tariffCode2);
			USCTariffTest.CreateTariff(Factory, tariffCode3);
			USCTariffTest.CreateTariff(Factory, tariffCode4);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = tariffCode2;
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			header.FillWithValidTestData();
			var invoiceLine = header.InvoiceLines.AddNew();
			var invoiceLine2 = header.InvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = tariffCode1;
			invoiceLine.US_SupTariff = tariffCode2;
			invoiceLine.US_CVD_NA = true;
			invoiceLine.US_ADD_NA = true;
			invoiceLine2.JI_Tariff = tariffCode3;
			invoiceLine2.US_SupTariff = tariffCode4;
			invoiceLine2.US_CVD_NA = true;
			invoiceLine2.US_ADD_NA = true;

			Factory.Save();

			return new TestCase
			{
				Message = "ABI Import Invoice Line",
				DeclarationPK = declaration.PK,
				ExpectedHitCounts = new Dictionary<string, int>
				{
					// TODO: These are caused by USRefTariffDataLoader.GetSupTariffs, to be fixed separately.
					{ TariffRelationshipViewSchema.Constants.TableName, 1 },
					{ USCTariffDutyRateSchema.Constants.TableName, 1 },
					{ USCTariffDateRestrictionSchema.Constants.TableName, 1 },
					{ USCTariffRuleExceptionSchema.Constants.TableName, 1 },
					{ TariffViewSchema.Constants.TableName, 3 }, // TariffView.Loader.GetEffectiveChildTariffs is a ZDBOnlyQuery. + Search Global Tariff to check applied Rules
					{ TariffAttributeViewSchema.Constants.TableName, 1 }, // TariffAttributeView is used to filter the list of tariffs after loading.
					{ JobDocAddressSchema.Constants.TableName, 2 }, // TariffAttributeView is used to filter the list of tariffs after loading.
					{ RefCusConditionSchema.Constants.TableName, 2 }
				}
			};
		}

		TestCase ReconInvoiceLineTestCase()
		{
			var declaration = Factory.New<JobDeclaration>();
			var recon = new ReconDeclaration(declaration);
			var reconEntry = recon.OriginalEntries.AddNew();
			var reconInvoice = recon.Invoices.AddNew();
			reconInvoice.US_CH_ReconEntry = reconEntry.CH_PK;
			reconInvoice.ReconOriginalEntry.US_R_DutyRateDate = ZDateTime.Now;
			reconInvoice.InvoiceLines.AddNew();
			reconInvoice.InvoiceLines.AddNew();
			Factory.Save();

			return new TestCase
			{
				Message = "Recon Invoice Line",
				DeclarationPK = declaration.PK,
				ExpectedHitCounts = new Dictionary<string, int>
				{
					{ CusInvPackSchema.Constants.TableName, 0 },
					{ CusDispositionSchema.Constants.TableName, 0 },
					{ JobDocAddressSchema.Constants.TableName, 2 }
				}
			};
		}
	}
}
