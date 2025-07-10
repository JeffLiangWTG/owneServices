using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class BasePricingPageRateLineListComparerTest : TestCaseWithFactory
	{
		public void TestFromPage_Direction()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "CLIENT";

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;

			var expEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "SG");
			var expEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");
			var impEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "SG", "AU");
			var impEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "NL", "AU");

			SetSortOrder(client, OrgConstants.ServiceDirection.Code.All, OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical);
			SetSortOrder(client, OrgConstants.ServiceDirection.Code.Import, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);
			SetSortOrder(client, OrgConstants.ServiceDirection.Code.Export, OrgConstants.GroupOrSubTotalCharges.Code.User);

			Factory.Save();

			var impPage = new PricingPage(impEntry1, Factory, PricingPageStyle.Landscape);
			impPage.AddRateEntry(impEntry2);

			var expPage = new PricingPage(expEntry1, Factory, PricingPageStyle.Landscape);
			expPage.AddRateEntry(expEntry2);

			var mixPage = new PricingPage(impEntry1, Factory, PricingPageStyle.Landscape);
			mixPage.AddRateEntry(expEntry1);

			AssertEquals("Import", BasePricingPageRateLineListComparer.PrintSequence(client, "SHP"), BasePricingPageRateLineListComparer.FromPage(impPage));
			AssertEquals("Export", BasePricingPageRateLineListComparer.LineSequence, BasePricingPageRateLineListComparer.FromPage(expPage));
			AssertEquals("Mix", BasePricingPageRateLineListComparer.Alphabetical, BasePricingPageRateLineListComparer.FromPage(mixPage));
		}

		public void TestFromPage_NoClient()
		{
			var tariff = Factory.New<CompanyTariff>();

			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");

			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)line.Calculator).PerUnit = 50;

			var page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			AssertEquals(BasePricingPageRateLineListComparer.LineSequence, BasePricingPageRateLineListComparer.FromPage(page));
		}

		public void TestFromPage_Alphabetical()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "CLIENT";

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;

			var entry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");

			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)line.Calculator).PerUnit = 50;

			Factory.Save();

			var page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical);
			AssertEquals(BasePricingPageRateLineListComparer.Alphabetical, BasePricingPageRateLineListComparer.FromPage(page));
		}

		public void TestFromPage_PrintSequence()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "CLIENT";

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;

			var entry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");

			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)line.Calculator).PerUnit = 50;

			Factory.Save();

			var page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);
			AssertEquals("Sequence", BasePricingPageRateLineListComparer.PrintSequence(client, JobInvoicingConsumerTypes.Shipment.Code), BasePricingPageRateLineListComparer.FromPage(page));

			SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.RollupAndSequence);
			AssertEquals("RollupAndSequence", BasePricingPageRateLineListComparer.PrintSequence(client, JobInvoicingConsumerTypes.Shipment.Code), BasePricingPageRateLineListComparer.FromPage(page));

			SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence);
			AssertEquals("SubTotalAndSequence", BasePricingPageRateLineListComparer.PrintSequence(client, JobInvoicingConsumerTypes.Shipment.Code), BasePricingPageRateLineListComparer.FromPage(page));
		}

		public void TestFromPage_LineSequence()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "CLIENT";

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;

			var entry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");

			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)line.Calculator).PerUnit = 50;

			Factory.Save();

			var page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.User);
			AssertEquals("User", BasePricingPageRateLineListComparer.LineSequence, BasePricingPageRateLineListComparer.FromPage(page));

			SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.SubTotal);
			AssertEquals("SubTotal", BasePricingPageRateLineListComparer.LineSequence, BasePricingPageRateLineListComparer.FromPage(page));

			SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.RollUp);
			AssertEquals("RollUp", BasePricingPageRateLineListComparer.LineSequence, BasePricingPageRateLineListComparer.FromPage(page));

			SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.RollUpEntireConsol);
			AssertEquals("RollUpEntireConsol", BasePricingPageRateLineListComparer.LineSequence, BasePricingPageRateLineListComparer.FromPage(page));
		}

		public void TestCompareAlphabetical()
		{
			var cxb = NewChargeCode("XXB", "Bob", 3);
			var cxc = NewChargeCode("XXC", "Cat", 1);
			var cxd = NewChargeCode("XXD", "Dog", 2);
			var cxf = NewChargeCode("XXF", "Freed", 2);

			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");

			PricingPageRateLineList[] orderedSets =
			{
				NewSet(AddRateLine(entry, cxb)),
				NewSet(AddRateLine(entry, cxc)),
				NewSet(AddRateLine(entry, cxd)),
				NewSet(AddRateLine(entry, cxf)),
				NewSet(),
			};

			AssertSequence(BasePricingPageRateLineListComparer.Alphabetical, orderedSets);
		}

		public void TestComparePrintSequence()
		{
			var cxc = NewChargeCode("XXC", "Cat", 1);
			var cxd = NewChargeCode("XXD", "Dog", 2);
			var cxf = NewChargeCode("XXF", "Freed", 2);
			var cxb = NewChargeCode("XXB", "Bob", 3);

			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");

			PricingPageRateLineList[] orderedSets =
			{
				NewSet(AddRateLine(entry, cxc)),
				NewSet(AddRateLine(entry, cxd)),
				NewSet(AddRateLine(entry, cxf)),
				NewSet(AddRateLine(entry, cxb)),
				NewSet(),
			};

			AssertSequence(BasePricingPageRateLineListComparer.PrintSequence(null, ""), orderedSets);
		}

		public void TestComparePrintSequenceOverridden()
		{
			var cxd = NewChargeCode("XXD", "Dog", 2);
			var cxb = NewChargeCode("XXB", "Bob", 3);
			var cxc = NewChargeCode("XXC", "Cat", 1);
			var cxf = NewChargeCode("XXF", "Freed", 2);

			Factory.Save(); // charge codes must be saved before setting the sequence overrides, otherwise it gets confused.

			var client = Factory.New<OrgHeader>();
			client.OH_Code = "CLIENT";
			SetSequenceOverride(client, cxd, 1);
			SetSequenceOverride(client, cxb, 2);
			SetSequenceOverride(client, cxc, 2);
			SetSequenceOverride(client, cxf, 3);

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;

			var entry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");

			PricingPageRateLineList[] orderedSets =
			{
				NewSet(AddRateLine(entry, cxd)),
				NewSet(AddRateLine(entry, cxb)),
				NewSet(AddRateLine(entry, cxc)),
				NewSet(AddRateLine(entry, cxf)),
				NewSet(),
			};

			AssertSequence(BasePricingPageRateLineListComparer.PrintSequence(client, ""), orderedSets);
		}

		public void TestCompareLineSequence()
		{
			var quote = Factory.New<Quote>();

			byte[] orders = { 1, 2, 9 };

			var entries = new RateLine[orders.Length, orders.Length];

			for (var i = 0; i < orders.Length; i++)
			{
				var entry = quote.AddRateEntry("FCL", "SEA", "AU", "NL");
				entry.TI_LineOrder = orders[i];

				for (var j = 0; j < orders.Length; j++)
				{
					var line = entry.AddRateLine("FRT", UnitCalculator.Code, "CN", "USD");
					line.TL_LineOrder = orders[j];

					entries[j, i] = line;
				}
			}

			PricingPageRateLineList[] orderedSets =
			{
				/* 1  , 1 */ NewSet(entries[0, 0]),
				/* 1  , 2 */ NewSet(entries[0, 1]),
				/* 1  , 5 */ NewSet(entries[0, 1], entries[0, 2]),
				/* 1  , 9 */ NewSet(entries[0, 2]),
				/* 1.5, 9 */ NewSet(entries[0, 2], entries[1, 2]),
				/* 2  , 1 */ NewSet(entries[1, 0]),
				/* 2  , 2 */ NewSet(entries[1, 1]),
				/* 2  , 5 */ NewSet(entries[1, 1], entries[1, 2]),
				/* 2  , 9 */ NewSet(entries[1, 2]),
				/* -  , - */ NewSet(),
			};

			var comparer = BasePricingPageRateLineListComparer.LineSequence;

			AssertSequence(comparer, orderedSets);
		}

		#region Local Description

		#region Alphabetical

		#region ClientRate

		public void TestCompareAlphabetical_ClientRate_RegistryEnabled_LocalUser() => AssertCompareAlphabetical(isRegistryEnabled: true, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewClientRate(orgHeader));

		public void TestCompareAlphabetical_ClientRate_RegistryDisabled_LocalUser() => AssertCompareAlphabetical(isRegistryEnabled: false, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewClientRate(orgHeader));

		public void TestCompareAlphabetical_ClientRate_RegistryEnabled_NonLocalUser() => AssertCompareAlphabetical(isRegistryEnabled: true, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewClientRate(orgHeader));

		public void TestCompareAlphabetical_ClientRate_RegistryDisabled_NonLocalUser() => AssertCompareAlphabetical(isRegistryEnabled: false, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewClientRate(orgHeader));

		#endregion

		#region Company Tariff

		public void TestCompareAlphabetical_CompanyTariff_RegistryEnabled_LocalUser() => AssertCompareAlphabetical(isRegistryEnabled: true, isLocalClient: true, getRate: (helper, orgHeader) => Factory.New<CompanyTariff>());

		public void TestCompareAlphabetical_CompanyTariff_RegistryDisabled_LocalUser() => AssertCompareAlphabetical(isRegistryEnabled: false, isLocalClient: true, getRate: (helper, orgHeader) => Factory.New<CompanyTariff>());

		public void TestCompareAlphabetical_CompanyTariff_RegistryEnabled_NonLocalUser() => AssertCompareAlphabetical(isRegistryEnabled: true, isLocalClient: false, getRate: (helper, orgHeader) => Factory.New<CompanyTariff>());

		public void TestCompareAlphabetical_CompanyTariff_RegistryDisabled_NonLocalUser() => AssertCompareAlphabetical(isRegistryEnabled: false, isLocalClient: false, getRate: (helper, orgHeader) => Factory.New<CompanyTariff>());

		#endregion

		#region Costing

		public void TestCompareAlphabetical_Costing_RegistryEnabled_LocalUser() => AssertCompareAlphabetical(isRegistryEnabled: true, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewCosting(orgHeader));

		public void TestCompareAlphabetical_Costing_RegistryDisabled_LocalUser() => AssertCompareAlphabetical(isRegistryEnabled: false, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewCosting(orgHeader));

		public void TestCompareAlphabetical_Costing_RegistryEnabled_NonLocalUser() => AssertCompareAlphabetical(isRegistryEnabled: true, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewCosting(orgHeader));

		public void TestCompareAlphabetical_Costing_RegistryDisabled_NonLocalUser() => AssertCompareAlphabetical(isRegistryEnabled: false, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewCosting(orgHeader));

		#endregion

		#region Quote

		public void TestCompareAlphabetical_Quote_RegistryEnabled_LocalUser() => AssertCompareAlphabetical(isRegistryEnabled: true, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewQuote(orgHeader));

		public void TestCompareAlphabetical_Quote_RegistryDisabled_LocalUser() => AssertCompareAlphabetical(isRegistryEnabled: false, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewQuote(orgHeader));

		public void TestCompareAlphabetical_Quote_RegistryEnabled_NonLocalUser() => AssertCompareAlphabetical(isRegistryEnabled: true, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewQuote(orgHeader));

		public void TestCompareAlphabetical_Quote_RegistryDisabled_NonLocalUser() => AssertCompareAlphabetical(isRegistryEnabled: false, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewQuote(orgHeader));

		#endregion

		void AssertCompareAlphabetical(bool isRegistryEnabled, bool isLocalClient, Func<TestHelper, OrgHeader, RatingHeader> getRate)
		{
			var chargeCode1 = NewChargeCode("AAA", "AAA Chg Desc", 1);
			var chargeCode2 = NewChargeCode("BBB", "BBB Chg Desc", 1);
			var chargeCode3 = NewChargeCode("CCC", "CCC Chg Desc", 1);
			var chargeCode4 = NewChargeCode("DDD", "DDD Chg Desc", 1, localDescription: "DDD Chg Local Desc");

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			if (isLocalClient)
			{
				orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			}
			AssertEquals("Local/nonLocal client", isLocalClient, orgHeader.IsLocalCountry);

			var helper = new TestHelper(Factory);
			var rate = getRate(helper, orgHeader);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");

			var orderedSets = isRegistryEnabled && (isLocalClient || rate.IsTariff())
				? new[] {
					NewSet(AddRateLine(entry, chargeCode3, localDescription: "")), // compare "AAA Chg Desc" because RateLine.LocalDescription is overriden with empty string hence default to RateLine.Description that was set from ChargeCode.Description on overriden
					NewSet(AddRateLine(entry, chargeCode4)), // compare "DDD Chg Local Desc" because RateLine.LocalDescription is not overriden hence default to ChargeCode.LocalDescription
					NewSet(AddRateLine(entry, chargeCode2, localDescription: "Y Rate Local Desc")), // compare "Y Rate Local Desc"
					NewSet(AddRateLine(entry, chargeCode1, localDescription: "Z Rate Local Desc")), // compare "Z Rate Local Desc"
					NewSet(),
				}
				: new[] {
					NewSet(AddRateLine(entry, chargeCode1, localDescription: "Z Rate Local Desc")),
					NewSet(AddRateLine(entry, chargeCode2, localDescription: "Y Rate Local Desc")),
					NewSet(AddRateLine(entry, chargeCode3, localDescription: "X Rate Local Desc")),
					NewSet(AddRateLine(entry, chargeCode4, localDescription: "W Rate Local Desc")),
					NewSet(),
				};

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled))
			{
				AssertSequence(BasePricingPageRateLineListComparer.Alphabetical, orderedSets);
			}
		}

		#endregion

		#region Print Sequence

		#region Client Rate

		public void TestComparePrintSequence_ClientRate_RegistryEnabled_LocalUser() => AssertComparePrintSequence(isRegistryEnabled: true, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewClientRate(orgHeader));

		public void TestComparePrintSequence_ClientRate_RegistryDisabled_LocalUser() => AssertComparePrintSequence(isRegistryEnabled: false, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewClientRate(orgHeader));

		public void TestComparePrintSequence_ClientRate_RegistryEnabled_NonLocalUser() => AssertComparePrintSequence(isRegistryEnabled: true, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewClientRate(orgHeader));

		public void TestComparePrintSequence_ClientRate_RegistryDisabled_NonLocalUser() => AssertComparePrintSequence(isRegistryEnabled: false, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewClientRate(orgHeader));

		#endregion

		#region Company Tariff

		public void TestComparePrintSequence_CompanyTariff_RegistryEnabled_LocalUser() => AssertComparePrintSequence(isRegistryEnabled: true, isLocalClient: true, getRate: (helper, orgHeader) => Factory.New<CompanyTariff>());

		public void TestComparePrintSequence_CompanyTariff_RegistryDisabled_LocalUser() => AssertComparePrintSequence(isRegistryEnabled: false, isLocalClient: true, getRate: (helper, orgHeader) => Factory.New<CompanyTariff>());

		public void TestComparePrintSequence_CompanyTariff_RegistryEnabled_NonLocalUser() => AssertComparePrintSequence(isRegistryEnabled: true, isLocalClient: false, getRate: (helper, orgHeader) => Factory.New<CompanyTariff>());

		public void TestComparePrintSequence_CompanyTariff_RegistryDisabled_NonLocalUser() => AssertComparePrintSequence(isRegistryEnabled: false, isLocalClient: false, getRate: (helper, orgHeader) => Factory.New<CompanyTariff>());

		#endregion

		#region Costing

		public void TestComparePrintSequence_Costing_RegistryEnabled_LocalUser() => AssertComparePrintSequence(isRegistryEnabled: true, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewCosting(orgHeader));

		public void TestComparePrintSequence_Costing_RegistryDisabled_LocalUser() => AssertComparePrintSequence(isRegistryEnabled: false, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewCosting(orgHeader));

		public void TestComparePrintSequence_Costing_RegistryEnabled_NonLocalUser() => AssertComparePrintSequence(isRegistryEnabled: true, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewCosting(orgHeader));

		public void TestComparePrintSequence_Costing_RegistryDisabled_NonLocalUser() => AssertComparePrintSequence(isRegistryEnabled: false, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewCosting(orgHeader));

		#endregion

		#region Quote

		public void TestComparePrintSequence_Quote_RegistryEnabled_LocalUser() => AssertComparePrintSequence(isRegistryEnabled: true, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewQuote(orgHeader));

		public void TestComparePrintSequence_Quote_RegistryDisabled_LocalUser() => AssertComparePrintSequence(isRegistryEnabled: false, isLocalClient: true, getRate: (helper, orgHeader) => helper.NewQuote(orgHeader));

		public void TestComparePrintSequence_Quote_RegistryEnabled_NonLocalUser() => AssertComparePrintSequence(isRegistryEnabled: true, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewQuote(orgHeader));

		public void TestComparePrintSequence_Quote_RegistryDisabled_NonLocalUser() => AssertComparePrintSequence(isRegistryEnabled: false, isLocalClient: false, getRate: (helper, orgHeader) => helper.NewQuote(orgHeader));

		#endregion

		void AssertComparePrintSequence(bool isRegistryEnabled, bool isLocalClient, Func<TestHelper, OrgHeader, RatingHeader> getRate)
		{
			var chargeCode = NewChargeCode("AAA", "AAA Chg Desc", 1, localDescription: "A Chg Local Desc");

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			if (isLocalClient)
			{
				orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			}
			AssertEquals("Local/nonLocal client", isLocalClient, orgHeader.IsLocalCountry);

			var helper = new TestHelper(Factory);
			var rate = getRate(helper, orgHeader);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");

			var orderedSets = isRegistryEnabled && (isLocalClient || rate.IsTariff())
				? new[] {
					NewSet(AddRateLine(entry, chargeCode)),
					NewSet(AddRateLine(entry, chargeCode, description: "D Rate Desc")), // compare "A Chg Local Desc" because on overriden RateLine.LocalDescription = ChargeCode.LocalDescription
					NewSet(AddRateLine(entry, chargeCode, description: "A Rate Desc", localDescription: "")), // compare "A Rate Desc" because RateLine.LocalDescription is overriden with empty string hence default to RateLine.Description
					NewSet(AddRateLine(entry, chargeCode, description: "C Rate Desc", localDescription: "Y Rate Local Desc")), // compare "Y Local Description"
					NewSet(AddRateLine(entry, chargeCode, description: "B Rate Desc", localDescription: "Z Rate Local Desc")), // compare "Z local Description"
					NewSet(),
				}
				: new[] {
					NewSet(AddRateLine(entry, chargeCode, description: "A Rate Desc", localDescription: "Z Rate Local Desc")),
					NewSet(AddRateLine(entry, chargeCode, description: "B Rate Desc", localDescription: "Y Rate Local Desc")),
					NewSet(AddRateLine(entry, chargeCode, description: "C Rate Desc", localDescription: "X Rate Local Desc")),
					NewSet(),
				};

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled))
			{
				AssertSequence(BasePricingPageRateLineListComparer.PrintSequence(null, ""), orderedSets);
			}
		}

		#endregion

		#endregion

		#region Implementation

		static void AssertSequence(BasePricingPageRateLineListComparer comparer, PricingPageRateLineList[] orderedSets)
		{
			for (var i = 1; i < orderedSets.Length; i++)
			{
				Assert(string.Format("Comparer(orderedSets[{0}], orderedSets[{1}]) < 0", i - 1, i), comparer.Compare(orderedSets[i - 1], orderedSets[i]) < 0);
				Assert(string.Format("Comparer(orderedSets[{0}], orderedSets[{1}]) > 0", i, i - 1), comparer.Compare(orderedSets[i], orderedSets[i - 1]) > 0);
			}
		}

		static void SetSortOrder(OrgHeader header, ZString invoiceRollUpOrGroup)
		{
			SetSortOrder(header, OrgConstants.ServiceDirection.Code.All, invoiceRollUpOrGroup);
		}

		static void SetSortOrder(OrgHeader header, ZString direction, ZString invoiceRollUpOrGroup)
		{
			OrgInvoiceRollupOrGroup irog = null;

			foreach (OrgInvoiceRollupOrGroup current in header.CompanyData.InvoiceRollupOrGroups)
			{
				if (current.PG_JobType == "ALL" && current.PG_ServiceDirection == direction)
				{
					irog = current;
					break;
				}
			}

			if (irog == null)
			{
				irog = header.CompanyData.InvoiceRollupOrGroups.AddNew();
				irog.PG_JobType = "ALL";
				irog.PG_ServiceDirection = direction;
				irog.PG_TransportMode = "ALL";
			}

			irog.PG_GroupOrSubTotal = invoiceRollUpOrGroup;
		}

		static void SetSequenceOverride(OrgHeader client, AccChargeCode chargeCode, short sequence)
		{
			foreach (var order in client.InvoiceOrders)
			{
				if (order.AI_AC == chargeCode.PK)
				{
					order.AI_PrintOrder = sequence;
					return;
				}
			}

			var newOrder = client.InvoiceOrders.AddNew();
			newOrder.AI_AC = chargeCode.PK;
			newOrder.AI_PrintOrder = sequence;
		}

		PricingPageRateLineList NewSet(params RateLine[] lines)
		{
			var set = new PricingPageRateLineList();
			set.AddRange(lines);
			return set;
		}

		AccChargeCode NewChargeCode(string code, string description, short sequence, string localDescription = null)
		{
			var result = Factory.NewWithValidTestData<AccChargeCode>();
			result.AC_Code = code;
			result.AC_Desc = description;
			result.AC_PrintSequence = sequence;

			if (localDescription != null)
			{
				result.AC_LocalLanguageDescription = localDescription;
			}

			return result;
		}

		static RateLine AddRateLine(RateEntry parent, AccChargeCode chargeCode, string description = null, string localDescription = null)
		{
			var rateLine = parent.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateDesc = chargeCode.AC_Desc;
			rateLine.TL_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;

			if (description != null || localDescription != null)
			{
				rateLine.OverrideChargeDescription = true;
			}

			if (description != null)
			{
				rateLine.TL_RateDesc = description;
			}

			if (localDescription != null)
			{
				rateLine.TL_RateDescLocal = localDescription;
			}

			return rateLine;
		}

		#endregion
	}
}
