using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCompanyDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOB_CusPaidByList()
		{
			var cusPaidByList = Lookups.OB_CusPaidByList;
			AssertEquals("OB_CusPaidByList.Count", 2, cusPaidByList.Count);
			Assert(cusPaidByList.ContainsCode("BRK"));
			Assert(cusPaidByList.ContainsCode("CLI"));
		}

		#region Tax Configuration Template

		public void TestARTaxTemplates()
		{
			var templateAR = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAR.OCT_IsReceivable = true;

			var templateAP = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAP.OCT_IsReceivable = false;

			var templates = Lookups.ARTaxTemplates as OrgCompanyDataTaxConfigurationTemplateCollection;
			AssertNotNull(templates);
			AssertEquals(1, templates.Count);
			AssertEquals(templateAR.PK, templates[0].PK);
			AssertEquals(true, templates.IsReceivable);
		}

		public void TestAPTaxTemplates()
		{
			var templateAR = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAR.OCT_IsReceivable = true;

			var templateAP = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateAP.OCT_IsReceivable = false;

			var templates = Lookups.APTaxTemplates as OrgCompanyDataTaxConfigurationTemplateCollection;
			AssertNotNull(templates);
			AssertEquals(1, templates.Count);
			AssertEquals(templateAP.PK, templates[0].PK);
			AssertEquals(false, templates.IsReceivable);
		}

		#endregion

		public void TestOB_ARCreateVATComplianceDocumentOnPostingList()
		{
			Assert(Lookups.OB_ARCreateVATComplianceDocumentOnPostingList.ContainsOnly(new string[] { Constants.OrganisationCreateComplianceDocumentOnPostingTypes.NotApplicable, Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge }));
		}

		public void TestOB_APCreateVATComplianceDocumentOnPostingList()
		{
			Assert(Lookups.OB_APCreateVATComplianceDocumentOnPostingList.ContainsOnly(new string[]
			{ Constants.OrganisationCreateComplianceDocumentOnPostingTypes.NotApplicable, Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge, Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber }));
		}

		public void TestOB_ARCreditCardExpire_Year_List()
		{
			AssertEquals(21, Lookups.OB_ARCreditCardExpire_Year_List.Count);
			AssertEquals("__", Lookups.OB_ARCreditCardExpire_Year_List[0].Code);
			AssertEquals((ZDateTime.Now.Year - 2000).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[1].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 1).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[2].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 2).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[3].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 3).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[4].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 4).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[5].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 5).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[6].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 6).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[7].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 7).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[8].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 8).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[9].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 9).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[10].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 10).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[11].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 11).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[12].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 12).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[13].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 13).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[14].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 14).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[15].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 15).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[16].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 16).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[17].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 17).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[18].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 18).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[19].Code);
			AssertEquals((ZDateTime.Now.Year - 2000 + 19).ToString(), Lookups.OB_ARCreditCardExpire_Year_List[20].Code);
		}

		public void TestOB_ARCreditCardExpire_Month_List()
		{
			AssertEquals(13, Lookups.OB_ARCreditCardExpire_Month_List.Count);
			AssertEquals("__", Lookups.OB_ARCreditCardExpire_Month_List[0].Code);
			AssertEquals("01", Lookups.OB_ARCreditCardExpire_Month_List[1].Code);
			AssertEquals("02", Lookups.OB_ARCreditCardExpire_Month_List[2].Code);
			AssertEquals("03", Lookups.OB_ARCreditCardExpire_Month_List[3].Code);
			AssertEquals("04", Lookups.OB_ARCreditCardExpire_Month_List[4].Code);
			AssertEquals("05", Lookups.OB_ARCreditCardExpire_Month_List[5].Code);
			AssertEquals("06", Lookups.OB_ARCreditCardExpire_Month_List[6].Code);
			AssertEquals("07", Lookups.OB_ARCreditCardExpire_Month_List[7].Code);
			AssertEquals("08", Lookups.OB_ARCreditCardExpire_Month_List[8].Code);
			AssertEquals("09", Lookups.OB_ARCreditCardExpire_Month_List[9].Code);
			AssertEquals("10", Lookups.OB_ARCreditCardExpire_Month_List[10].Code);
			AssertEquals("11", Lookups.OB_ARCreditCardExpire_Month_List[11].Code);
			AssertEquals("12", Lookups.OB_ARCreditCardExpire_Month_List[12].Code);
		}

		public void TestAPDefaultChargeCodes()
		{
			AccChargeCode codeInCurrentCompany = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK));
			AccChargeCode codeInDiffCompany = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			AccChargeCodeCollection coll1 = Lookups.APDefaultChargeCodes;
			AccChargeCodeCollection coll2 = Lookups.APDefaultChargeCodes;
			coll1.Load();
			coll2.Load();

			AssertCollectionContains("codeInCurrentCompany should be in the AP Default Charge Codes collection", codeInCurrentCompany, coll1);
			AssertCollectionNotContains("codeInDiffCompany should NOT be in the AP Default Charge Codes collection", codeInDiffCompany, coll2);
			Assert("The collection should NOT be cached", !coll1.Equals(coll2));
		}

		public void TestBuyersConsolInvoicingStyles()
		{
			string originalRegValue = OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.Value;
			try
			{
				OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "MAS");
				OrgHeader org = Factory.New<OrgHeader>();
				AssertEquals("4 items - default included", 4, org.CompanyData.Lookups.BuyersConsolInvoicingStyles.Count);
				AssertEquals(true, org.CompanyData.Lookups.BuyersConsolInvoicingStyles.ContainsCode("DEF"));
				AssertEquals("Default from Registry - All Charges invoiced and rated from the Lead Shipment", org.CompanyData.Lookups.BuyersConsolInvoicingStyles.GetDescriptionFromCode("DEF"));

				AssertEquals("3 items - default not included", 3, new OrgCompanyDataLookups.BuyersConsolInvoicingStyleList(false).Count);
				AssertEquals("4 items - default not included", 4, new OrgCompanyDataLookups.BuyersConsolInvoicingStyleList(true).Count);

				OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "APP");
				org = Factory.New<OrgHeader>();
				AssertEquals("DEF", org.CompanyData.OB_ARBuyersConsolInvoicingStyle);
				AssertEquals("Default from Registry - Charges rated, apportioned and invoiced on individual shipments", org.CompanyData.Lookups.BuyersConsolInvoicingStyles.GetDescriptionFromCode("DEF"));
			}
			finally
			{
				OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalRegValue);
			}
		}

		public void TestOB_APPaymentTerms_ListWithoutDefaultValue()
		{
			AssertEquals("OB_APPaymentTerms_ListWithoutDefaultValue.Count", new APInvoiceTermsList().Count, Lookups.OB_APPaymentTerms_ListWithoutDefaultValue.Count);
		}

		public void TestOB_APPaymentTerms_List()
		{
			AssertEquals("OB_APPaymentTerms_List.Count", new APInvoiceTermsList().Count + 1, Lookups.OB_APPaymentTerms_List.Count);
			AssertCollectionContains("OB_APPaymentTerms_List contains 'DEF'", OrgCompanyDataLookups.DefaultInvoiceTerm, Lookups.OB_APPaymentTerms_List);

			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgHeader settlementOrganisation = Factory.New<OrgHeader>();

			OrgCompanyDataLookups orgTermAllLookups = organisation.CompanyData.Lookups;

			AssertEquals("orgTermAllLookups", OrgCompanyDataLookups.DefaultInvoiceTerm.Description, orgTermAllLookups.OB_APPaymentTerms_List.GetDescriptionFromCode(OrgCompanyDataLookups.DefaultInvoiceTerm.Code));

			organisation.APSettlementGroupPK = organisation.PK;
			AssertEquals("orgTermAllLookups", OrgCompanyDataLookups.DefaultInvoiceTerm.Description, orgTermAllLookups.OB_APPaymentTerms_List.GetDescriptionFromCode(OrgCompanyDataLookups.DefaultInvoiceTerm.Code));

			organisation.APSettlementGroupPK = settlementOrganisation.PK;
			AssertEquals("orgTermAllLookups", OrgCompanyDataLookups.DefaultInvoiceTerm.Description + " (COD)", orgTermAllLookups.OB_APPaymentTerms_List.GetDescriptionFromCode(OrgCompanyDataLookups.DefaultInvoiceTerm.Code));

			settlementOrganisation.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			AssertEquals("orgTermAllLookups", OrgCompanyDataLookups.DefaultInvoiceTerm.Description, orgTermAllLookups.OB_APPaymentTerms_List.GetDescriptionFromCode(OrgCompanyDataLookups.DefaultInvoiceTerm.Code));

			settlementOrganisation.CompanyData.OB_APPaymentTerms = InvoiceTermsList.FromInvoiceDate.Code;
			settlementOrganisation.CompanyData.OB_APPaymentTermDays = 1;
			AssertEquals("orgTermAllLookups", OrgCompanyDataLookups.DefaultInvoiceTerm.Description + " (INV, 1 day)", orgTermAllLookups.OB_APPaymentTerms_List.GetDescriptionFromCode(OrgCompanyDataLookups.DefaultInvoiceTerm.Code));

			settlementOrganisation.CompanyData.OB_APPaymentTerms = InvoiceTermsList.FromInvoiceDate.Code;
			settlementOrganisation.CompanyData.OB_APPaymentTermDays = 0;
			AssertEquals("orgTermAllLookups", OrgCompanyDataLookups.DefaultInvoiceTerm.Description + " (INV, 0 days)", orgTermAllLookups.OB_APPaymentTerms_List.GetDescriptionFromCode(OrgCompanyDataLookups.DefaultInvoiceTerm.Code));

			settlementOrganisation.CompanyData.OB_APPaymentTerms = InvoiceTermsList.PaymentInAdvance.Code;
			AssertEquals("orgTermAllLookups", OrgCompanyDataLookups.DefaultInvoiceTerm.Description + " (PIA)", orgTermAllLookups.OB_APPaymentTerms_List.GetDescriptionFromCode(OrgCompanyDataLookups.DefaultInvoiceTerm.Code));

			OrgCompanyDataLookups settlementOrgTermAllLookups = settlementOrganisation.CompanyData.Lookups;
			AssertEquals("settlementOrgTermAllLookups", OrgCompanyDataLookups.DefaultInvoiceTerm.Description, settlementOrgTermAllLookups.OB_APPaymentTerms_List.GetDescriptionFromCode(OrgCompanyDataLookups.DefaultInvoiceTerm.Code));
		}

		public void TestOB_APCategory_List()
		{
			Assert("List.Count > 0", Lookups.OB_APCategory_List.Count > 0);
		}

		public void TestOB_ARConsolidatedAccountingCategory_List()
		{
			Assert("OB_ARConsolidatedAccountingCategory_List.Count > 0", Lookups.OB_ARConsolidatedAccountingCategory_List.Count > 0);
		}

		public void TestOB_ARCategory_List()
		{
			Assert("OB_ARCategory_List.Count > 0", Lookups.OB_ARCategory_List.Count > 0);
		}

		public void TestOB_ARCreditRating_List()
		{
			Assert("OB_ARCreditRating_List.Count > 0", Lookups.OB_ARCreditRating_List.Count > 0);
		}

		public void TestOB_RateSecurityGroup_List()
		{
			AssertEquals("Default list is empty", 0, Lookups.OB_RateSecurityGroup_List.Count);

			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", "ABC Description");
			list.AddPair("XYZ", "XYZ Description");
			OrganisationsDataRegistry.Instance.RatesSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(list));
			Reset();

			AssertEquals("List contains 2 items", 2, Lookups.OB_RateSecurityGroup_List.Count);
			Assert("Contains ABC", Lookups.OB_RateSecurityGroup_List.ContainsCode("ABC"));
			Assert("Contains XYZ", Lookups.OB_RateSecurityGroup_List.ContainsCode("XYZ"));
		}

		public void TestOB_APTransactionCreationRestrictionList()
		{
			AssertEquals(4, Lookups.OB_APTransactionCreationRestrictionList.Count);
			Assert(Lookups.OB_APTransactionCreationRestrictionList.ContainsCode("NON"));
			Assert(Lookups.OB_APTransactionCreationRestrictionList.ContainsCode("ALL"));
			Assert(Lookups.OB_APTransactionCreationRestrictionList.ContainsCode("INV"));
			Assert(Lookups.OB_APTransactionCreationRestrictionList.ContainsCode("BAL"));
		}

		public void TestOB_ARTransactionCreationRestrictionList()
		{
			AssertEquals(4, Lookups.OB_ARTransactionCreationRestrictionList.Count);
			Assert(Lookups.OB_ARTransactionCreationRestrictionList.ContainsCode("NON"));
			Assert(Lookups.OB_ARTransactionCreationRestrictionList.ContainsCode("ALL"));
			Assert(Lookups.OB_ARTransactionCreationRestrictionList.ContainsCode("INV"));
			Assert(Lookups.OB_ARTransactionCreationRestrictionList.ContainsCode("BAL"));
		}

		public void TestOB_GoodsOwnership_List()
		{
			AssertEquals(1, Lookups.OB_GoodsOwnership_List.Count);
			Assert(Lookups.OB_GoodsOwnership_List.ContainsCode("NVR"));
		}

		public void TestWarehouseStorageCalculationMethods()
		{
			AssertEquals("WarehouseStorageCalculationMethods.Count", 4, Lookups.WarehouseStorageCalculationMethods.Count);
			Assert(Lookups.WarehouseStorageCalculationMethods.ContainsCode("MAX"));
			Assert(Lookups.WarehouseStorageCalculationMethods.ContainsCode("PEK"));
			Assert(Lookups.WarehouseStorageCalculationMethods.ContainsCode("CLO"));
			Assert(Lookups.WarehouseStorageCalculationMethods.ContainsCode("SPL"));
		}

		public void TestWarehouseRatingPeriods()
		{
			Assert("WarehouseRatingPeriods.Count > 1", Lookups.WarehouseRatingPeriods.Count > 1);
			Assert(Lookups.WarehouseRatingPeriods.ContainsCode(Core.Constants.StorageCalculationPeriods.Default));
			AssertEquals("Default from Registry - Weekly", Lookups.WarehouseRatingPeriods.GetDescriptionFromCode(Core.Constants.StorageCalculationPeriods.Default));
		}

		public void TestCompanyTariffTypesAndLevelsAndTransportModes()
		{
			InsertCompanyTariff(1, "Tariff 1");
			InsertCompanyTariff(2, "Tariff 2");

			OrgHeader org = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			CodeDescriptionPairList tariffTypes = new OrgCodeLists().CompanyTariffTypes_List(Factory, Env.CurrentCompanyPK);
			AssertEquals(17, tariffTypes.Count);
			AssertTariffType(org, tariffTypes, "DEF", "Default for All Job Types");
			AssertTariffType(org, tariffTypes, "FRT", "Freight");
			AssertTariffType(org, tariffTypes, "ORG", "Origin");
			AssertTariffType(org, tariffTypes, "DST", "Destination");
			AssertTariffType(org, tariffTypes, "SFR", "Shipping Freight");
			AssertTariffType(org, tariffTypes, "SOR", "Shipping Origin");
			AssertTariffType(org, tariffTypes, "SDE", "Shipping Destination");
			AssertTariffType(org, tariffTypes, "SCD", "Shipping Container Detention");
			AssertTariffType(org, tariffTypes, "CFS", "CFS");
			AssertTariffType(org, tariffTypes, "WHS", "Product Warehouse");
			AssertTariffType(org, tariffTypes, "TRN", "Transport");
			AssertTariffType(org, tariffTypes, "TBC", "Land Transport");
			AssertTariffType(org, tariffTypes, "CYD", "Container Yard");
			AssertTariffType(org, tariffTypes, "TRW", "Transit Warehouse");
			AssertTariffType(org, tariffTypes, "TWU", "Transit Warehouse Transportation Unit");
			AssertTariffType(org, tariffTypes, "CYU", "Container Yard Transportation Unit");
			AssertTariffType(org, tariffTypes, "CYM", "Yard Maintenance and Repair Charges");

			CodeDescriptionPairList tariffLevels = org.CompanyData.Lookups.CompanyTariffLevels;
			AssertEquals(3, tariffLevels.Count);
			AssertCodeDescription(tariffLevels, "0", "Do not use Company Tariff");
			AssertCodeDescription(tariffLevels, "1", "Tariff 1");
			AssertCodeDescription(tariffLevels, "2", "Tariff 2");
		}

		public void TestCompanyTariffTypesAndLevelsAndTransportModes_WithGlobalTariffs()
		{
			InsertCompanyTariff(1, "Tariff 1");
			InsertCompanyTariff(2, "Tariff 2");
			InsertGlobalCompanyTariff(1, "Global Tariff 1");
			InsertGlobalCompanyTariff(2, "Global Tariff 2");
			InsertGlobalCompanyTariff(3, "Global Tariff 3");

			CombineAssertions("", () =>
			{
				var org = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
				var tariffTypes = new OrgCodeLists().CompanyTariffTypes_List(Factory, Env.CurrentCompanyPK);
				AssertEquals(17, tariffTypes.Count);
				AssertTariffType(org, tariffTypes, "DEF", "Default for All Job Types");
				AssertTariffType(org, tariffTypes, "FRT", "Freight");
				AssertTariffType(org, tariffTypes, "ORG", "Origin");
				AssertTariffType(org, tariffTypes, "DST", "Destination");
				AssertTariffType(org, tariffTypes, "SFR", "Shipping Freight");
				AssertTariffType(org, tariffTypes, "SOR", "Shipping Origin");
				AssertTariffType(org, tariffTypes, "SDE", "Shipping Destination");
				AssertTariffType(org, tariffTypes, "SCD", "Shipping Container Detention");
				AssertTariffType(org, tariffTypes, "CFS", "CFS");
				AssertTariffType(org, tariffTypes, "WHS", "Product Warehouse");
				AssertTariffType(org, tariffTypes, "TRN", "Transport");
				AssertTariffType(org, tariffTypes, "TBC", "Land Transport");
				AssertTariffType(org, tariffTypes, "CYD", "Container Yard");
				AssertTariffType(org, tariffTypes, "TRW", "Transit Warehouse");
				AssertTariffType(org, tariffTypes, "TWU", "Transit Warehouse Transportation Unit");
				AssertTariffType(org, tariffTypes, "CYU", "Container Yard Transportation Unit");
				AssertTariffType(org, tariffTypes, "CYM", "Yard Maintenance and Repair Charges");

				var tariffLevels = org.CompanyData.Lookups.CompanyTariffLevels;
				AssertEquals(4, tariffLevels.Count);
				AssertCodeDescription(tariffLevels, "0", "Do not use Global or Company Tariff");
				AssertCodeDescription(tariffLevels, "1", "Global or Company Tariff Level 1");
				AssertCodeDescription(tariffLevels, "2", "Global or Company Tariff Level 2");
				AssertCodeDescription(tariffLevels, "3", "Global Tariff 3");
			});
		}

		void AssertTariffType(OrgHeader org, CodeDescriptionPairList pairList, string code, string desc)
		{
			AssertCodeDescription(pairList, code, desc);
			CodeDescriptionPairList modesList = org.CompanyData.Lookups.GetCompanyTransportModes(code);
			AssertModes(code, false, modesList);
		}

		public static void AssertDirections(string tariffType, CodeDescriptionPairList directions)
		{
			if (tariffType == "CST" || tariffType == "WHS" || tariffType == "TRW" || tariffType == "TWU" || tariffType == "TRN" || tariffType == "TBC" || tariffType == "CYD" || tariffType == "CYU" || tariffType == "CYM")
			{
				AssertEquals(1, directions.Count);
				AssertEquals("ALL", directions[0].Code);
			}
			else
			{
				AssertEquals(3, directions.Count);
				AssertEquals("ALL", directions[0].Code);
				AssertEquals("EXP", directions[1].Code);
				AssertEquals("IMP", directions[2].Code);
			}
		}

		public static void AssertModes(string code, bool includeAllPossible, CodeDescriptionPairList modesList)
		{
			switch (code)
			{
				case "DEF":
					if (includeAllPossible)
					{
						AssertEquals(1, modesList.Count);
						Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					}
					else
					{
						AssertEquals(0, modesList.Count);
					}
					break;

				case "FRT":
					if (includeAllPossible)
					{
						AssertEquals(18, modesList.Count);
						Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					}
					else
					{
						AssertEquals(17, modesList.Count);
					}

					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LSE")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ULD")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SEA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("RAI")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FTL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BLK")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BBK")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BCN")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SCN")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("OBC")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("UNA")));
					break;

				case "ORG":
				case "DST":
					AssertEquals(24, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("AIR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ULD")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LSE")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SEA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FTL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("RAI")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FWL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("MAI")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BLK")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BBK")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BCN")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SCN")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("OBC")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("UNA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("COU")));
					break;

				case "SFR":
					if (includeAllPossible)
					{
						AssertEquals(3, modesList.Count);
						Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					}
					else
					{
						AssertEquals(2, modesList.Count);
					}
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SEA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LCL")));
					break;

				case "SOR":
				case "SDE":
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FCL")));
					break;

				case "SCD":
					if (includeAllPossible)
					{
						AssertEquals(1, modesList.Count);
						Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					}
					else
					{
						AssertEquals(0, modesList.Count);
					}
					break;

				case "CFS":
					AssertEquals(16, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("AIR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ULD")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LSE")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SEA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("GRP")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FTL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("RAI")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FWL")));
					break;

				case "WHS":
				case "TRW":
					if (includeAllPossible)
					{
						AssertEquals(1, modesList.Count);
						Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					}
					else
					{
						AssertEquals(0, modesList.Count);
					}
					break;

				case "TWU":
					AssertEquals(4, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("AIR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SEA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					break;

				case "TRN":
					AssertEquals(5, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("AIR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRO")));
					break;

				case "TBC":
					AssertEquals(9, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FTL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("RAI")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FWL")));
					break;

				case "CYD":
				case "CYU":
				case "CYM":
					AssertEquals(2, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					break;

				default:
					Fail(string.Format("{0} tariff type code needs to be checked here", code));
					break;
			}
		}

		static void AssertCodeDescription(CodeDescriptionPairList pairList, string code, string desc)
		{
			Assert(pairList.ContainsCode(code));
			AssertEquals(desc, pairList.GetDescriptionFromCode(code));
		}

		#region Implementation

		protected OrgCompanyData Data;
		protected OrgCompanyDataLookups Lookups;
		protected override void SetUp()
		{
			base.SetUp();
			Reset();
		}

		void Reset()
		{
			Data = Factory.New<OrgCompanyData>();
			Lookups = Data.Lookups;
		}

		internal static void InsertCompanyTariff(int tariffLevel, string tariffLevelDescription)
		{
			string cmdText = @"INSERT INTO dbo.RatingHeader
								 (TH_PK, TH_GlobalRateLevel, TH_GlobalRateDescription, TH_RateType, TH_GC, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
							   VALUES
								 (@TH_PK, @TH_GlobalRateLevel, @TH_GlobalRateDescription, @TH_RateType, @TH_GC, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			DbCommand cmd = Db.Connection.Command(cmdText); // Need to use ODbCommand instead of BusinessObjectFactory due to circular reference with Rating solution.
			cmd.AddParameterBasedOnDbColumn("@TH_PK", Guid.NewGuid(), RatingHeaderSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@TH_GlobalRateLevel", tariffLevel, RatingHeaderSchema.TH_GlobalRateLevel);
			cmd.AddParameterBasedOnDbColumn("@TH_GlobalRateDescription", tariffLevelDescription, RatingHeaderSchema.TH_GlobalRateDescription);
			cmd.AddParameterBasedOnDbColumn("@TH_RateType", "GLB", RatingHeaderSchema.TH_RateType);
			cmd.AddParameterBasedOnDbColumn("@TH_GC", GlbCompany.CurrentCompany.PK.ToGuid(), RatingHeaderSchema.TH_GC);

			cmd.ExecuteNonQuery();
		}

		internal static void InsertGlobalCompanyTariff(int tariffLevel, string tariffLevelDescription)
		{
			string cmdText = @"INSERT INTO dbo.RatingHeader
								 (TH_PK, TH_GlobalRateLevel, TH_GlobalRateDescription, TH_RateType, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
							   VALUES
								 (@TH_PK, @TH_GlobalRateLevel, @TH_GlobalRateDescription, @TH_RateType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			DbCommand cmd = Db.Connection.Command(cmdText); // Need to use ODbCommand instead of BusinessObjectFactory due to circular reference with Rating solution.
			cmd.AddParameterBasedOnDbColumn("@TH_PK", Guid.NewGuid(), RatingHeaderSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@TH_GlobalRateLevel", tariffLevel, RatingHeaderSchema.TH_GlobalRateLevel);
			cmd.AddParameterBasedOnDbColumn("@TH_GlobalRateDescription", tariffLevelDescription, RatingHeaderSchema.TH_GlobalRateDescription);
			cmd.AddParameterBasedOnDbColumn("@TH_RateType", "GLB", RatingHeaderSchema.TH_RateType);

			cmd.ExecuteNonQuery();
		}

		#endregion
	}
}
