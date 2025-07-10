using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public static class PricingPageTestHelper
	{
		public static PricingPage SetupRates(BusinessObjectFactory factory)
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "Client";
			SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical);

			AccChargeCode[] freightChargeCodes =
			{
				secondHelper.ChargeCodes.New("FCC1", "Freight Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true),
				secondHelper.ChargeCodes.New("FCC2", "Freight Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true),
				secondHelper.ChargeCodes.New("FCC3", "Freight Charge Code 3", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true),
			};

			AccChargeCode[] originChargeCodes =
			{
				secondHelper.ChargeCodes.New("OCC1", "Origin Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true),
				secondHelper.ChargeCodes.New("OCC2", "Origin Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true),
				secondHelper.ChargeCodes.New("OCC3", "Origin Charge Code 3", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true),
			};

			AccChargeCode[] destinationChargeCodes =
			{
				secondHelper.ChargeCodes.New("DCC1", "Destination Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true),
				secondHelper.ChargeCodes.New("DCC2", "Destination Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true),
				secondHelper.ChargeCodes.New("DCC3", "Destination Charge Code 3", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true),
			};

			var quote = createFactory.New<Quote>();
			quote.TH_OH = client.PK;

			var rate = createFactory.New<ClientRate>();
			rate.TH_OH = client.PK;

			var tariff = createFactory.New<CompanyTariff>();

			client.CompanyData.RateTariffLevels.SetLevel("DEF", tariff.TH_GlobalRateLevel);

			var headers = new RatingHeader[] { tariff, rate, quote };
			var containers = createFactory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, new ZString[] { "20GP", "40GP", "20RE", "40RE" }));

			for (var i = 0; i < headers.Length; i++)
			{
				var header = headers[i];

				foreach (var container in containers)
				{
					var freightEntry = header.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUBNE", "NLAMS", "STD", container.RC_Code);
					freightEntry.RateLines.RemoveAndDeleteAll();

					if (header.IsQuote())
					{
						AddChargeLinesWithOverriddenDescriptions(freightEntry, freightChargeCodes[2]);
					}
					else
					{
						AddChargeLines(freightEntry, i, freightChargeCodes);
					}
				}

				var originEntry = header.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "");
				AddChargeLines(originEntry, i, originChargeCodes);

				var destinationEntry = header.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "", "NLAMS");
				AddChargeLines(destinationEntry, i, destinationChargeCodes);
			}

			createFactory.Save();

			var reloadedQuote = factory.Load<Quote>(quote.PK);
			var pages = new PricingPageCollection(reloadedQuote);
			pages.LoadStandard();

			return pages[0];
		}

		public static PricingPage SetupSampleRatesForEmptyMatching(BusinessObjectFactory factory, ZString category, bool empty)
		{
			var consignor = factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";

			var consignee = factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";

			var provider = factory.NewWithValidTestData<OrgHeader>();
			provider.OH_Code = "PROVIDER";

			var supplier = factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";

			var pickup = factory.NewWithValidTestData<OrgHeader>().MainAddress;
			pickup.Header.OH_Code = "PICKUP";
			pickup.OA_PostCode = "1111";

			var delivery = factory.NewWithValidTestData<OrgHeader>().MainAddress;
			delivery.Header.OH_Code = "DELIVERY";
			delivery.OA_PostCode = "2222";

			var fields = new[]
			{
				new { Field = (SchemaColumn)RateEntrySchema.TI_OH_Consignor,                      Code = "R", Value = (IZType)consignor.PK,        },
				new { Field = (SchemaColumn)RateEntrySchema.TI_OH_Consignee,                      Code = "E", Value = (IZType)consignee.PK,        },
				new { Field = (SchemaColumn)RateEntrySchema.TI_OH_TransportProvider,              Code = "T", Value = (IZType)provider.PK,         },
				new { Field = (SchemaColumn)RateEntrySchema.TI_OH_Supplier,                       Code = "S", Value = (IZType)supplier.PK,         },
				new { Field = (SchemaColumn)RateEntrySchema.TI_OA_CartagePickupAddressOverride,   Code = "P", Value = (IZType)pickup.PK,           },
				new { Field = (SchemaColumn)RateEntrySchema.TI_OA_CartageDeliveryAddressOverride, Code = "D", Value = (IZType)delivery.PK,         },
				new { Field = (SchemaColumn)RateEntrySchema.TI_CartagePickupAddressPostCode,      Code = "1", Value = (IZType)new ZString("1111"), },
				new { Field = (SchemaColumn)RateEntrySchema.TI_CartageDeliveryAddressPostCode,    Code = "2", Value = (IZType)new ZString("2222"), },
				new { Field = (SchemaColumn)RateEntrySchema.TI_PL_NKCarrierServiceLevel,          Code = "L", Value = (IZType)new ZString("STD"),  },
			};

			var rateEntryCategoryAndModes = new[]
			{
				new { Category = RatingConstants.RateCategory.LCL, Mode ="LCL", ChargeCode = "FRT" },
				new { Category = RatingConstants.RateCategory.ORG, Mode ="ALL", ChargeCode = "OPCH" },
				new { Category = RatingConstants.RateCategory.DST, Mode ="ALL", ChargeCode = "DPCH" },
			};

			var tariff = factory.LoadTop1<CompanyTariff>(new ZQuery()) ?? factory.New<CompanyTariff>();

			RateEntry result = null;

			foreach (var rateEntryCategoryAndMode in rateEntryCategoryAndModes)
			{
				var rateEntry = tariff.AddRateEntry(rateEntryCategoryAndMode.Category, rateEntryCategoryAndMode.Mode, "AU", "NL", removeLines: true);

				if (empty)
				{
					rateEntry.AddUnitRateLine(rateEntryCategoryAndMode.ChargeCode, 100m, QuantityUnit.KG, description: $"{rateEntryCategoryAndMode.ChargeCode} Charge");
				}
				else
				{
					rateEntry.AddUnitRateLine(rateEntryCategoryAndMode.ChargeCode, 200m, QuantityUnit.KG, description: $"{rateEntryCategoryAndMode.ChargeCode} Charge");

					var lineUnit = 201;
					foreach (var field in fields)
					{
						rateEntry[field.Field] = field.Value;

						var otherEntry = tariff.AddRateEntry(rateEntryCategoryAndMode.Category, rateEntryCategoryAndMode.Mode, "AU", "NL", removeLines: true);
						otherEntry[field.Field] = field.Value;

						otherEntry.AddUnitRateLine(rateEntryCategoryAndMode.ChargeCode, lineUnit++, QuantityUnit.KG, description: $"{rateEntryCategoryAndMode.ChargeCode} Charge");
					}
				}

				if (rateEntryCategoryAndMode.Category == category)
				{
					result = rateEntry;
				}
			}

			return new PricingPage(result, factory, PricingPageStyle.Standard);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public static PricingPage SetupSampleRatesForVisibilityChecking(BusinessObjectFactory factory, ZString category)
		{
			var helper = new TestHelper(factory);

			AccChargeCode[] freightChargeCodes =
			{
				helper.ChargeCodes.New("CC1", "Suppress", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true),
				helper.ChargeCodes.New("CC2", "Show", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, false),
				helper.ChargeCodes.New("CC3", "Hide", "FLT", ChargeCodeGroupList.Codes.Freight, "", false, false),
			};

			ZString mode = category == RatingConstants.RateCategory.FCL ? "SEA" : "FCL";
			var tariff = factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry(category, mode, "AU", "NL", "", "20GP");

			foreach (var chargeCode in freightChargeCodes)
			{
				var line1 = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
				line1.TL_RateDesc = chargeCode.AC_Desc + " Charge";
				((UnitCalculator)line1.Calculator).PerUnit = 50;

				var line2 = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
				line2.TL_RateDesc = chargeCode.AC_Desc + " Charge (Z)";
				((UnitCalculator)line2.Calculator).PerUnit = 0;
			}

			factory.Save();

			return new PricingPage(entry, factory, PricingPageStyle.Standard);
		}

		public static PricingPage SetupSampleRatesForVisibilityCheckingWhenBaseIsZero(BusinessObjectFactory factory)
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);
			var client = secondHelper.NewOrgHeader(1);

			var code1 = secondHelper.ChargeCodes.New("FCC1", "Freight Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			var code2 = secondHelper.ChargeCodes.New("FCC2", "Freight Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);

			var tariff = createFactory.New<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;

			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUBNE", "NLAMS", "STD", "20GP");
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffline1 = tariffEntry.AddRateLine(code1, FlatCalculator.Code);
			tariffline1.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var tariffline2 = tariffEntry.AddRateLine(code2, FlatCalculator.Code);
			tariffline2.GetCalculator<FlatCalculator>().BaseRate = 0m;

			var quote = secondHelper.NewQuote(client);
			var quoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUBNE", "NLAMS", "STD", "20GP");
			quoteEntry.RateLines.RemoveAndDeleteAll();

			var quoteline1 = quoteEntry.AddRateLine(code2, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, QuantityUnit.CN);
			quoteline1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5m;

			var quoteline2 = quoteEntry.AddRateLine(code2, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, QuantityUnit.CN);
			quoteline2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5m;

			createFactory.Save();

			var reloadedQuote = factory.Load<Quote>(quote.PK);
			var pages = new PricingPageCollection(reloadedQuote);
			pages.LoadStandard();

			return pages[0];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public static PricingPage SetupSampleRatesForVisibilityCheckingWhenBaseIsZeroAgencyCalculator(BusinessObjectFactory factory, decimal percent)
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);
			var client = secondHelper.NewOrgHeader(1);

			var code1 = secondHelper.ChargeCodes.New("FCC", "Freight Charge Code", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			var code2 = secondHelper.ChargeCodes.New("CCC", "Customs Charge Code", "FLT", ChargeCodeGroupList.Codes.Brokerage, "", true, true);

			var tariff = createFactory.New<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;

			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUBNE", "NLAMS", "STD", "20GP");
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffline1 = tariffEntry.AddRateLine(code1, FlatCalculator.Code, "", Constants.CurrencyCodes.UnitedStates);
			tariffline1.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var tariffline2 = tariffEntry.AddRateLine(code2, AgencyCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			var calculator = tariffline2.GetCalculator<AgencyCalculator>();
			calculator.AgencyRate = 200m;
			calculator.Maximum = 100000m;
			calculator.AgencyFeeType = RateFeeTypeList.Codes.PerEntryPage;
			calculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			calculator.IncludedHeaders = 1;
			calculator.IncludedLines = 2;
			calculator.AdditionalRate = 150m;
			calculator.MaximumLines = 55;
			calculator.PerAdditionalLine = 5;
			calculator.MessageType = SharedJobMessageTypeList.Codes.Export;
			calculator.MessageSubType = "SWL";

			var quote = secondHelper.NewQuote(client);
			var quoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUBNE", "NLAMS", "STD", "20GP");
			quoteEntry.RateLines.RemoveAndDeleteAll();

			var quoteline = quoteEntry.AddRateLine(code2, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			quoteline.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = percent;

			createFactory.Save();

			var reloadedQuote = factory.Load<Quote>(quote.PK);
			var pages = new PricingPageCollection(reloadedQuote);
			pages.LoadStandard();

			return pages[0];
		}

		public static PricingPage SetupSampleRatesForVisibilityCheckingWithOverride(BusinessObjectFactory factory)
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);
			var client = secondHelper.NewOrgHeader(1);

			var code1 = secondHelper.ChargeCodes.New("FCC1", "Freight Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			var code2 = secondHelper.ChargeCodes.New("FCC2", "Freight Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);

			var rate = secondHelper.NewClientRate(client);

			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUBNE", "NLAMS", "STD", "20GP");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.AddRateLine(code1, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 100m;
			rateEntry.AddRateLine(code2, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 90m;

			var quote = secondHelper.NewQuote(client);
			var quoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUBNE", "NLAMS", "STD", "20GP");
			quoteEntry.RateLines.RemoveAndDeleteAll();
			quoteEntry.AddRateLine(code2, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 0m;

			createFactory.Save();

			var reloadedQuote = factory.Load<Quote>(quote.PK);
			var pages = new PricingPageCollection(reloadedQuote);
			pages.LoadStandard();

			return pages[0];
		}

		public static PricingPage SetupSampleRatesForCombining(BusinessObjectFactory factory)
		{
			var consignor1 = factory.NewWithValidTestData<OrgHeader>();
			consignor1.OH_Code = "CONSIGNOR1";
			consignor1.OH_FullName = "Consignor 1";

			var consignor2 = factory.NewWithValidTestData<OrgHeader>();
			consignor2.OH_Code = "CONSIGNOR2";
			consignor2.OH_FullName = "Consignor 2";

			var consignee1 = factory.NewWithValidTestData<OrgHeader>();
			consignee1.OH_Code = "CONSIGNEE1";
			consignee1.OH_FullName = "Consignee 1";

			var consignee2 = factory.NewWithValidTestData<OrgHeader>();
			consignee2.OH_Code = "CONSIGNEE2";
			consignee2.OH_FullName = "Consignee 2";

			var tariff = factory.New<CompanyTariff>();

			var entry = NewEntryWithLine(tariff, RatingConstants.RateCategory.LCL, "LCL", "FRT", null, null, 100);

			NewEntryWithLine(tariff, RatingConstants.RateCategory.ORG, "ALL", "OPCH", consignor1, null, 200);
			NewEntryWithLine(tariff, RatingConstants.RateCategory.ORG, "ALL", "OPCH", consignor2, null, 201);

			NewEntryWithLine(tariff, RatingConstants.RateCategory.DST, "ALL", "DPCH", null, consignee1, 300);
			NewEntryWithLine(tariff, RatingConstants.RateCategory.DST, "ALL", "DPCH", null, consignee2, 301);

			return new PricingPage(entry, factory, PricingPageStyle.Standard);
		}

		public static Dictionary<string, PricingPage> SetupSampleRatesForSupplementaryPortFiltering(BusinessObjectFactory factory, bool withBothPorts)
		{
			var helper = new TestHelper(factory);
			var zone1 = helper.NewInternationalZone("AUNZ", null, "AU", "NZ");
			var zone2 = helper.NewInternationalZone("NLGB", null, "NL", "GB");

			var tariff = factory.New<CompanyTariff>();

			var frt = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "20GP");
			var frtLine = frt.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 500;

			var ori1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "", "", "20GP");
			var ori1Line = ori1.AddRateLine("ODOC", FlatCalculator.Code);
			ori1Line.GetCalculator<FlatCalculator>().BaseRate = 100;

			var ori2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			var ori2Line = ori2.AddRateLine("OPCH", FlatCalculator.Code);
			ori2Line.GetCalculator<FlatCalculator>().BaseRate = 100;

			var ori3 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "NL", "", "20GP");
			var ori3Line = ori3.AddRateLine("OPCH", FlatCalculator.Code);
			ori3Line.GetCalculator<FlatCalculator>().BaseRate = 100;

			var ori4 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUNZ", "", "", "40GP");
			var ori4Line = ori4.AddRateLine("OPCH", FlatCalculator.Code);
			ori4Line.GetCalculator<FlatCalculator>().BaseRate = 100;

			var des1 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NL", "", "20GP");
			var des1Line = des1.AddRateLine("DDOC", FlatCalculator.Code);
			des1Line.GetCalculator<FlatCalculator>().BaseRate = 100;

			var des2 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NLAMS", "", "20GP");
			var des2Line = des2.AddRateLine("DPCH", FlatCalculator.Code);
			des2Line.GetCalculator<FlatCalculator>().BaseRate = 100;

			var des3 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "AU", "NL", "", "20GP");
			var des3Line = des3.AddRateLine("DPCH", FlatCalculator.Code);
			des3Line.GetCalculator<FlatCalculator>().BaseRate = 100;

			var des4 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NLGB", "", "40GP");
			var des4Line = des4.AddRateLine("DPCH", FlatCalculator.Code);
			des4Line.GetCalculator<FlatCalculator>().BaseRate = 100;

			return new Dictionary<string, PricingPage>
				{
					{ RatingConstants.RateCategory.FCL, new PricingPage(frt, factory, PricingPageStyle.Standard) },
					{ RatingConstants.RateCategory.ORG, new PricingPage(withBothPorts ? ori3 : ori1, factory, PricingPageStyle.Standard) },
					{ RatingConstants.RateCategory.DST, new PricingPage(withBothPorts ? des3 : des1, factory, PricingPageStyle.Standard) },
				};
		}

		public static Dictionary<string, PricingPage> SetupSampleRatesForSupplementaryModeFiltering(BusinessObjectFactory factory, bool? fcl)
		{
			var tariff = factory.New<CompanyTariff>();

			var frt1 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "SEA", "AU", "NL");
			var frt1Line = frt1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			frt1Line.GetCalculator<UnitCalculator>().PerUnit = 500;

			var frt2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL", "", "20GP");
			var frt2Line = frt2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			frt2Line.GetCalculator<UnitCalculator>().PerUnit = 500;

			var ori1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AU", "");
			var ori1Line = ori1.AddRateLine("ODOC", FlatCalculator.Code);
			ori1Line.GetCalculator<FlatCalculator>().BaseRate = 101;

			var ori2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "LCL", "AU", "");
			var ori2Line = ori2.AddRateLine("OPCH", FlatCalculator.Code);
			ori2Line.GetCalculator<FlatCalculator>().BaseRate = 102;

			var ori3 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "");
			var ori3Line = ori3.AddRateLine("OPCH", FlatCalculator.Code);
			ori3Line.GetCalculator<FlatCalculator>().BaseRate = 103;

			var ori4 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "", "", "20GP");
			var ori4Line = ori4.AddRateLine("OLAB", FlatCalculator.Code);
			ori4Line.GetCalculator<FlatCalculator>().BaseRate = 104;

			var des1 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "", "NL");
			var des1Line = des1.AddRateLine("DDOC", FlatCalculator.Code);
			des1Line.GetCalculator<FlatCalculator>().BaseRate = 101;

			var des2 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "LCL", "", "NL");
			var des2Line = des2.AddRateLine("DPCH", FlatCalculator.Code);
			des2Line.GetCalculator<FlatCalculator>().BaseRate = 102;

			var des3 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NL");
			var des3Line = des3.AddRateLine("DPCH", FlatCalculator.Code);
			des3Line.GetCalculator<FlatCalculator>().BaseRate = 103;

			var des4 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NL", "", "20GP");
			var des4Line = des4.AddRateLine("DLAB", FlatCalculator.Code);
			des4Line.GetCalculator<FlatCalculator>().BaseRate = 104;

			return new Dictionary<string, PricingPage>
				{
					{ RatingConstants.RateCategory.FCL, new PricingPage(frt2, factory, PricingPageStyle.Standard) },
					{ RatingConstants.RateCategory.LCL, new PricingPage(frt1, factory, PricingPageStyle.Standard) },
					{ RatingConstants.RateCategory.ORG, new PricingPage(!fcl.HasValue ? ori1 : fcl.Value ? ori3 : ori2, factory, PricingPageStyle.Standard) },
					{ RatingConstants.RateCategory.DST, new PricingPage(!fcl.HasValue ? des1 : fcl.Value ? des3 : des2, factory, PricingPageStyle.Standard) },
				};
		}

		static RateEntry NewEntryWithLine(RatingHeader header, ZString category, ZString mode, ZString chargeCode, OrgHeader consignor, OrgHeader consignee, decimal perUnit)
		{
			var entry = header.AddRateEntry(category, mode, "AU", "NL");
			entry.TI_OH_Consignor = consignor == null ? ZGuid.Empty : consignor.PK;
			entry.TI_OH_Consignee = consignee == null ? ZGuid.Empty : consignee.PK;
			entry.RateLines.RemoveAndDeleteAll();

			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			line.TL_RateDesc = chargeCode + " Charge";
			line.GetCalculator<UnitCalculator>().PerUnit = perUnit;

			return entry;
		}

		public static void AddChargeLines(RateEntry entry, int index, AccChargeCode[] codes)
		{
			for (var i = index; i < codes.Length; i++)
			{
				var line = entry.AddRateLine(codes[i], FlatCalculator.Code, "", Constants.CurrencyCodes.UnitedStates);
				line.Calculator[FlatCalculator.Items.Operator.BAS] = new ZDecimal(100m - index * 10);
			}
		}

		public static void SetSortOrder(OrgHeader header, ZString invoiceRollUpOrGroup)
		{
			OrgInvoiceRollupOrGroup irog = null;

			foreach (OrgInvoiceRollupOrGroup current in header.CompanyData.InvoiceRollupOrGroups)
			{
				if (current.PG_JobType == "ALL")
				{
					irog = current;
					break;
				}
			}

			if (irog == null)
			{
				irog = header.CompanyData.InvoiceRollupOrGroups.AddNew();
				irog.PG_JobType = "ALL";
			}

			irog.PG_GroupOrSubTotal = invoiceRollUpOrGroup;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		static void AddChargeLinesWithOverriddenDescriptions(RateEntry entry, AccChargeCode code)
		{
			var line = entry.AddRateLine(code, FlatCalculator.Code, "", Constants.CurrencyCodes.UnitedStates);
			line.TL_RateDesc = code.AC_Desc + " desc overriden";
			line.Calculator[FlatCalculator.Items.Operator.BAS] = new ZDecimal(80);

			var line2 = entry.AddRateLine(code, FlatCalculator.Code, "", Constants.CurrencyCodes.UnitedStates);
			line2.TL_RateDesc = code.AC_Desc + " desc overriden";
			line2.Calculator[FlatCalculator.Items.Operator.BAS] = new ZDecimal(980);
		}
	}
}
