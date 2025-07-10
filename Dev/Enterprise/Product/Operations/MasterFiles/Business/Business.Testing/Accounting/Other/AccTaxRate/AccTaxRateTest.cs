using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccTaxRate;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxRate))]
	sealed class AccTaxRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsNonReportable()
		{
			var taxRate = Factory.New<AccTaxRate>();

			Assert("Empty tax type is treated as non reportable.", taxRate.IsNonReportable);

			var nonReportableTypes = new[] { AccTaxRate.Types.NotReportable, AccTaxRate.Types.ExcludedFromTheTaxBase };

			foreach (var taxRateType in typeof(Types).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => (string)field.GetValue(null)))
			{
				taxRate.AT_Type = taxRateType;
				AssertEquals(nonReportableTypes.Contains(taxRateType), taxRate.IsNonReportable);
			}
		}

		public void TestTaxSystemForDisplay()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("GST", taxRate.AT_TaxSystemCode_ForDisplay);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Argentina))
			{
				AssertEquals("IVA", taxRate.AT_TaxSystemCode_ForDisplay);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Bangladesh))
			{
				AssertEquals("VAT", taxRate.AT_TaxSystemCode_ForDisplay);
			}

			taxRate.AT_TaxSystemCode = "TEST";
			AssertEquals("TEST", taxRate.AT_TaxSystemCode_ForDisplay);
		}

		public void TestGetExtraRate()
		{
			AssertEquals(4m, AccTaxRate.GetExtraRate(10, "X", 20, 5));
			AssertEquals(40m, AccTaxRate.GetExtraRate(10, ExtraTypes.VATRetentionFraction, 20, 5));
		}

		public void TestGetRate()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRate_ForTestOnly(1, 2, ZDate.Today.AddDays(1), null);
			taxRate.SetRate_ForTestOnly(3, 4, ZDate.Today, ZDate.Today);
			taxRate.SetRate_ForTestOnly(5, 6, null, ZDate.Today.AddDays(-1));

			AssertEquals(0.75m, taxRate.GetRate(ZDate.Today));
			AssertEquals(0.75m, taxRate.GetRateRaw(ZDate.Today));
			AssertEquals(0.5m, taxRate.GetRate(ZDate.Today.AddDays(1)));
			AssertEquals(0.5m, taxRate.GetRateRaw(ZDate.Today.AddDays(1)));
			AssertEquals(0.5m, taxRate.GetRate((ZDate)DateTime.MaxValue));
			AssertEquals(0.5m, taxRate.GetRateRaw((ZDate)DateTime.MaxValue));
			AssertEquals(0.8333333333333333333333333333m, taxRate.GetRate(ZDateTime.MinSmallDateTimeValue.Date));
			AssertEquals(0.8333333333333333333333333333m, taxRate.GetRateRaw(ZDateTime.MinSmallDateTimeValue.Date));
			AssertEquals(0.8333333333333333333333333333m, taxRate.GetRate(ZDate.Today.AddDays(-1)));
			AssertEquals(0.8333333333333333333333333333m, taxRate.GetRateRaw(ZDate.Today.AddDays(-1)));

			taxRate.AT_Type = Types.ReverseRated;
			AssertEquals(0m, taxRate.GetRate(ZDate.Today.AddDays(-1)));
			AssertEquals(0.8333333333333333333333333333m, taxRate.GetRateRaw(ZDate.Today.AddDays(-1)));

			taxRate.AT_Type = Types.Suspended;
			AssertEquals(0m, taxRate.GetRate(ZDate.Today.AddDays(-1)));
			AssertEquals(0.8333333333333333333333333333m, taxRate.GetRateRaw(ZDate.Today.AddDays(-1)));
		}

		public void TestGetEffectiveExtraRate()
		{
			AssertEquals(0.4m, AccTaxRate.GetEffectiveExtraRate("x", 10, ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 4));
			AssertEquals(4.1666666666666666666666666700m, AccTaxRate.GetEffectiveExtraRate("x", 10, ExtraTypes.ChinaInputVATClaimed, 4));
			AssertEquals(-4m, AccTaxRate.GetEffectiveExtraRate("x", 10, ExtraTypes.VATRetention, 4));
			AssertEquals(-4m, AccTaxRate.GetEffectiveExtraRate("x", 10, ExtraTypes.VATRetentionFraction, 4));
			AssertEquals(-10m, AccTaxRate.GetEffectiveExtraRate("x", 10, ExtraTypes.VATRemittedByCustomer, 4));
			AssertEquals(4m, AccTaxRate.GetEffectiveExtraRate("x", 10, ExtraTypes.StateGST, 4));
			AssertEquals(0m, AccTaxRate.GetEffectiveExtraRate(Types.ReverseRated, 10, ExtraTypes.StateGST, 4));
			AssertEquals(4m, AccTaxRate.GetEffectiveExtraRate(Types.ReverseRated, 10, ExtraTypes.QuebecQSTExcludingGSTInQSTBase, 4));
			AssertEquals(4m, AccTaxRate.GetEffectiveExtraRate(Types.ReverseRated, 10, ExtraTypes.ChinaInputVATOffsetAgainstOutputTax, 4));
			AssertEquals(4.4m, AccTaxRate.GetEffectiveExtraRate("x", 10, "x", 4));
		}

		public void TestGetEffectiveExtraRateByDate()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "GSTANDQST";
			taxRate.AT_Description = "GST and QST";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_IsActive = true;
			taxRate.SetRateNumerator_ForTestOnly(5);
			taxRate.SetExtraRate_ForTestOnly(75, 10, ZDate.Today, ZDate.Today);
			taxRate.SetExtraRate_ForTestOnly(85, 10, null, ZDate.Today.AddDays(-1));
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;

			AssertEquals(7.875m, taxRate.GetEffectiveExtraRate(ZDate.Today));
			AssertEquals(0m, taxRate.GetEffectiveExtraRate(ZDate.Today.AddDays(1)));
			AssertEquals(8.925m, taxRate.GetEffectiveExtraRate(ZDate.Today.AddDays(-1)));
		}

		public void TestGetRateComponents()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRate_ForTestOnly(1, 2, ZDate.Today.AddDays(1), null);
			taxRate.SetRate_ForTestOnly(3, 4, ZDate.Today, ZDate.Today);
			taxRate.SetRate_ForTestOnly(5, 6, null, ZDate.Today.AddDays(-1));

			AssertEquals((numerator: (ZInt)1, denominator: (ZInt)2), taxRate.GetRateComponents(ZDate.Today.AddYears(10)));
			AssertEquals((numerator: (ZInt)1, denominator: (ZInt)2), taxRate.GetRateComponents(ZDate.Today.AddDays(1)));
			AssertEquals((numerator: (ZInt)3, denominator: (ZInt)4), taxRate.GetRateComponents(ZDate.Today));
			AssertEquals((numerator: (ZInt)3, denominator: (ZInt)4), taxRate.GetRateComponents(ZDate.Empty));
			AssertEquals((numerator: (ZInt)5, denominator: (ZInt)6), taxRate.GetRateComponents(ZDate.Today.AddDays(-1)));
			AssertEquals((numerator: (ZInt)5, denominator: (ZInt)6), taxRate.GetRateComponents(ZDate.Today.AddYears(-10)));
			AssertEquals((numerator: (ZInt)0, denominator: (ZInt)1), taxRate.GetRateComponents(new ZDate(DateTime.MinValue)));
		}

		public void TestGetExtraRateComponents()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetExtraRate_ForTestOnly(1, 2, ZDate.Today.AddDays(1), null);
			taxRate.SetExtraRate_ForTestOnly(3, 4, ZDate.Today, ZDate.Today);
			taxRate.SetExtraRate_ForTestOnly(5, 6, null, ZDate.Today.AddDays(-1));

			AssertEquals((numerator: (ZInt)1, denominator: (ZInt)2), taxRate.GetExtraRateComponents(ZDate.Today.AddYears(10)));
			AssertEquals((numerator: (ZInt)1, denominator: (ZInt)2), taxRate.GetExtraRateComponents(ZDate.Today.AddDays(1)));
			AssertEquals((numerator: (ZInt)3, denominator: (ZInt)4), taxRate.GetExtraRateComponents(ZDate.Today));
			AssertEquals((numerator: (ZInt)3, denominator: (ZInt)4), taxRate.GetExtraRateComponents(ZDate.Empty));
			AssertEquals((numerator: (ZInt)5, denominator: (ZInt)6), taxRate.GetExtraRateComponents(ZDate.Today.AddDays(-1)));
			AssertEquals((numerator: (ZInt)5, denominator: (ZInt)6), taxRate.GetExtraRateComponents(ZDate.Today.AddYears(-10)));
			AssertEquals((numerator: (ZInt)0, denominator: (ZInt)1), taxRate.GetExtraRateComponents(new ZDate(DateTime.MinValue)));
		}

		public void TestGetRateComponents_Cache()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			var rate = taxRate.SetRate_ForTestOnly(20, 8);
			Factory.Save();
			rate = new BusinessObjectFactory { RefreshEnabled = false }.Load<RefAccTaxRate>(rate.PK);

			AssertEquals((numerator: (ZInt)20, denominator: (ZInt)8), taxRate.GetRateComponents(ZDate.Today.AddDays(1)));

			rate.ZAT_RateNumerator *= 2;
			rate.ZAT_RateDenominator *= 2;
			rate.Factory.Save();

			AssertEquals((numerator: (ZInt)20, denominator: (ZInt)8), taxRate.GetRateComponents(ZDate.Today.AddMonths(-3)));
			AssertEquals((numerator: (ZInt)20, denominator: (ZInt)8), taxRate.GetRateComponents(ZDate.Today.AddMonths(-6)));
		}

		public void TestGetExtraRateComponents_Cache()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			var rate = taxRate.SetExtraRate_ForTestOnly(20, 8);
			Factory.Save();
			rate = new BusinessObjectFactory { RefreshEnabled = false }.Load<RefAccTaxRate>(rate.PK);

			AssertEquals((numerator: (ZInt)20, denominator: (ZInt)8), taxRate.GetExtraRateComponents(ZDate.Today.AddDays(1)));

			rate.ZAT_RateNumerator *= 2;
			rate.ZAT_RateDenominator *= 2;
			rate.Factory.Save();

			AssertEquals((numerator: (ZInt)20, denominator: (ZInt)8), taxRate.GetExtraRateComponents(ZDate.Today.AddMonths(-3)));
			AssertEquals((numerator: (ZInt)20, denominator: (ZInt)8), taxRate.GetExtraRateComponents(ZDate.Today.AddMonths(-6)));
		}

		public void TestSynchronizationTaxRateTypesWithDBConstraints()
		{
			//if you add a new AT_Type value in AccTaxRate.Types, you also need to update the DB Constraint on AT_Type (ask Brett Shearer)
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();

			foreach (var taxRateType in new AccTaxRateLookups(Factory.NewWithValidTestData<AccTaxRate>()).Types.GetAllCodes())
			{
				taxRate.AT_Type = taxRateType;
				AssertNoExceptionThrown("All AccTaxRate.Types need to be synchronize with the DB Constraint on AT_Type", () => Factory.Save());
			}
		}

		public void TestSynchronizationTaxRateExtraTypesWithDBConstraints()
		{
			//if you add a new AT_ExtraTaxRateType value in AccTaxRate.ExtraTypes, you also need to update the DB Constraint on AT_ExtraTaxRateType (ask Brett Shearer)
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();

			foreach (var taxRateExtraType in new AccTaxRateLookups(Factory.NewWithValidTestData<AccTaxRate>()).ExtraTypes.GetAllCodes())
			{
				taxRate.AT_ExtraTaxRateType = taxRateExtraType;
				AssertNoExceptionThrown("All AccTaxRate.ExtraTypes need to be synchronize with the DB Constraint on AT_ExtraTaxRateType", () => Factory.Save());
			}
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccTaxRate()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();

			var rateList = new List<string>
			{
				nameof(taxRate.ExtraRateForToday)
			};

			var tester = new DecimalPlacesAttributeTester(taxRate);
			tester.CheckSetter(rateList, nameof(taxRate.TaxRateDecimals));
		}

		public void TestCreateApplicableTaxIdsForNonMalaysiaLoginCompany()
		{
			var taxRates = Factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.KoreaSouth));
			AssertEquals("Blank DB", 0, taxRates.Length);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.KoreaSouth);
			GlbCompany.CurrentCompany.Factory.Save();

			AssertEquals("No changes required. The current Login Country/Region's Tax ID Set is already up to date.", AccTaxRate.CreateApplicableTaxIds(Factory));

			taxRates = Factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.KoreaSouth));
			AssertEquals("Tax Ids are imported from XML", 6, taxRates.Length);
		}

		public void TestRestrictedFilteredItemAttribute()
		{
			AssertEquals("Attribute is required for objects which have GC as a foreign key to get business obejcts from code with filters on ZPopupFindBox", true, GetExpectedBusinessObjectType().GetCustomAttributes(typeof(RestrictedFilteredItemAttribute), true).Any());
		}

		public void TestTemplateCopy()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "ABCDEF";
			taxRate.AT_Type = "TYP";
			taxRate.AT_ExtraTaxRateType = "XYZ";
			AccInvMsg msg = Factory.NewWithValidTestData<AccInvMsg>();
			taxRate.AT_A9_DefaultVatClass = msg.PK;

			AccTaxRate copy = (AccTaxRate)((ITemplateCopyable)taxRate).TemplateCopy();
			AssertEquals("AT_Code", "ABCDEF", copy.AT_Code);
			AssertEquals("AT_Type", "TYP", copy.AT_Type);
			AssertEquals("AT_ExtraTaxRateType", "XYZ", copy.AT_ExtraTaxRateType);
			AssertEquals("AT_A9_DefaultVatClass", msg.PK, copy.AT_A9_DefaultVatClass);
		}

		public void TestGetNOTREPORTTaxID()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;

			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = newCompany.GC_RN_NKCountryCode;
			rate.AT_Type = AccTaxRate.Types.NotReportable;

			var rate2 = Factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_RN_NKCountry = newCompany.GC_RN_NKCountryCode;
			rate2.AT_Code = "BBCXYZ";
			rate.AT_Type = AccTaxRate.Types.NotReportable;

			Factory.Save();

			ErrorReporter.Instance.Clear();

			var notREPORTTaxID = AccTaxRate.GetNOTREPORTTaxID(Factory, newCompany);
			AssertNotNull("There is no NOTREPORT tax rate in the registry", notREPORTTaxID);
			AssertEquals("Factory._Instance", Factory._Instance, notREPORTTaxID.Factory._Instance);
			AssertNotEquals("NOTREPORT tax rate should be default value set to the registry", rate.PK, notREPORTTaxID.PK);
			AssertNotEquals("NOTREPORT tax rate should be default value set to the registry", rate2.PK, notREPORTTaxID.PK);
			AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);

			var accounting = ObjectFactory.Get<IAccounting>();
			accounting.SetMainNotReportableTaxIDConfiguration(newCompany.PK.ToGuid(), Guid.Empty);
			var newFactory = new BusinessObjectFactory();
			notREPORTTaxID = AccTaxRate.GetNOTREPORTTaxID(newFactory, newCompany);
			AssertNull("There is no NOTREPORT tax rate in the registry, so we return null.", notREPORTTaxID);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Instance.Clear();

			accounting.SetMainNotReportableTaxIDConfiguration(newCompany.PK.ToGuid(), rate.PK.ToGuid());
			AssertEquals("NOTREPORT tax rate should be default value from registry", rate.PK, AccTaxRate.GetNOTREPORTTaxID(new BusinessObjectFactory(), newCompany).PK);

			accounting.SetMainNotReportableTaxIDConfiguration(newCompany.PK.ToGuid(), rate2.PK.ToGuid());
			AssertEquals("NOTREPORT tax rate should be default value from registry", rate2.PK, AccTaxRate.GetNOTREPORTTaxID(new BusinessObjectFactory(), newCompany).PK);
		}

		public void TestIsReverseOrSuspendedChargeAndGetTaxRate()
		{
			AssertNotNull("Precondition: Tax Code Exists [GST]", GST);
			AssertNotNull("Precondition: Tax Code Exists [CAPGST]", CAPGST);
			AssertNotNull("Precondition: Tax Code Exists [EXEMPTZero]", EXEMPT);
			AssertNotNull("Precondition: Tax Code Exists [FREEGSTZero]", FREEGST);
			AssertNotNull("Precondition: Tax Code Exists [NOTREPORTZero]", NOTREPORT);
			AssertNotNull("Precondition: Tax Code Exists [GSTREVZero]", GSTREV);
			AssertNotNull("Precondition: Tax Code Exists [GSTSUS]", GSTSUS);
			AssertNotNull("Precondition: Tax Code Exists [FREEGSTREVZero]", FREEGSTREV);
			AssertNotNull("Precondition: Tax Code Exists [LOWGSTREVZero]", LOWGSTREV);
			AssertNotNull("Precondition: Tax Code Exists [MIDGSTREVZero]", MIDGSTREV);

			Factory.Save();

			AccTaxRateCollection taxRates = new AccTaxRateCollection(Factory);
			taxRates.Load();

			foreach (AccTaxRate taxRate in taxRates)
			{
				bool expectedReverse = taxRate.AT_Type == AccTaxRate.Types.ReverseRated;
				bool expectedSuspended = taxRate.AT_Type == AccTaxRate.Types.Suspended;
				bool expectedAny = expectedReverse || expectedSuspended;
				AssertEquals("IsReverseCharge", expectedReverse, taxRate.IsReverseCharge);
				AssertEquals("IsSuspendedCharge", expectedSuspended, taxRate.IsSuspendedCharge);
				AssertEquals("IsReverseOrSuspendedCharge", expectedAny, taxRate.IsReverseOrSuspendedCharge);

				ZDecimal expectedRate = expectedAny ? ZDecimal.Zero : taxRate.GetRate_ForTestOnly();
				AssertEquals("GetRate()", expectedRate, taxRate.GetRate_ForTestOnly());
			}
		}

		public void TestIsVATRemittedByCustomer()
		{
			SetupAndAssertIsVATRemittedByCustomer(Core.Constants.CountryCodes.Italy);
			SetupAndAssertIsVATRemittedByCustomer(Core.Constants.CountryCodes.CostaRica);

			void SetupAndAssertIsVATRemittedByCustomer(ZString countryCodeWithSPV)
			{
				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.AT_RN_NKCountry = countryCodeWithSPV;
				taxRate.AT_ExtraTaxRateType = ZString.Empty;
				AssertEquals("Tax Rate is not VAT Remitted By CUstomer", false, taxRate.IsVATRemittedByCustomer);
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
				AssertEquals("Tax Rate is VAT Remitted By CUstomer", true, taxRate.IsVATRemittedByCustomer);
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
				AssertEquals("Tax Rate is not VAT Remitted By CUstomer", false, taxRate.IsVATRemittedByCustomer);
				taxRate.AT_RN_NKCountry = countryCodeWithSPV;
				taxRate.AT_Type = AccTaxRate.Types.CapitalRated;
				AssertEquals("Tax Rate is not VAT Remitted By CUstomer", false, taxRate.IsVATRemittedByCustomer);
			}
		}

		public void TestIsVATWithholding()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			AssertEquals("Tax Rate is not VAT Withholding", false, taxRate.IsVATWithholding);
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			AssertEquals("Tax Rate is VAT Withholding", true, taxRate.IsVATWithholding);
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;
			AssertEquals("Tax Rate is VAT Withholding", true, taxRate.IsVATWithholding);
		}

		public void TestIsLocalExtraTaxAmountValuePersistent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				IsLocalExtraTaxAmountValuePersistentTestCore(new string[] { AccTaxRate.ExtraTypes.StateGST });
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				IsLocalExtraTaxAmountValuePersistentTestCore(new string[] { AccTaxRate.ExtraTypes.VATRemittedByCustomer });
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
			{
				IsLocalExtraTaxAmountValuePersistentTestCore(new string[] { AccTaxRate.ExtraTypes.VATRetention, AccTaxRate.ExtraTypes.VATRetentionFraction });
			}
		}

		public void IsLocalExtraTaxAmountValuePersistentTestCore(string[] extraTypes)
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			foreach (var taxRateExtraType in typeof(ExtraTypes).GetFields().Where(f => f.IsLiteral).Select(field => (string)field.GetValue(null)))
			{
				taxRate.AT_ExtraTaxRateType = taxRateExtraType;
				if (extraTypes.Contains(taxRateExtraType))
				{
					Assert($"If extra type is {taxRateExtraType} only then extra tax amount is stored in database for now, if any new extra tax type comes in please check with Imraan/Baaber/Hasib to see if it the extra tax amount should be stored in DB"
						, taxRate.IsLocalExtraTaxAmountValuePersistent);
				}
				else
				{
					Assert(!taxRate.IsLocalExtraTaxAmountValuePersistent);
				}
			}
		}

		public void TestIsMexicoNeedExtraType()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			AssertNotEquals("Preconditional: Country must be distinct than Mexico", CountryCodes.Mexico, taxRate.AT_RN_NKCountry);
			AssertEquals(false, taxRate.IsMexicoNeedExtraType);

			AssertExtraTaxRateType(ExtraTypes.VATRetention, false);
			AssertExtraTaxRateType(ExtraTypes.VATRetentionFraction, false);

			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			AssertExtraTaxRateType(ExtraTypes.VATRemittedByCustomer, false);
			AssertExtraTaxRateType(ExtraTypes.VATRetention, true);

			AssertExtraTaxRateType(ExtraTypes.VATRemittedByCustomer, false);
			AssertExtraTaxRateType(ExtraTypes.VATRetentionFraction, true);

			void AssertExtraTaxRateType(string extraTypeValue, bool expectedValue)
			{
				taxRate.AT_ExtraTaxRateType = extraTypeValue;
				AssertEquals(expectedValue, taxRate.IsMexicoNeedExtraType);
			}
		}

		public void TestIsIndiaStateTax()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var stateTax = Factory.NewWithValidTestData<AccTaxRate>();
				stateTax.AT_Type = AccTaxRate.Types.Rated;
				stateTax.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;
				stateTax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				Assert("Is India state tax", stateTax.IsIndiaStateTax);

				var otherIndiaTax = Factory.NewWithValidTestData<AccTaxRate>();
				otherIndiaTax.AT_Type = AccTaxRate.Types.Rated;
				otherIndiaTax.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				otherIndiaTax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				Assert("EDU tax is not state tax", !otherIndiaTax.IsIndiaStateTax);

				var indiaServiceTax = Factory.NewWithValidTestData<AccTaxRate>();
				indiaServiceTax.AT_Type = AccTaxRate.Types.ServiceTax;
				indiaServiceTax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				Assert("Service tax is not state tax", !indiaServiceTax.IsIndiaStateTax);
			}

			var otherCountryTax = Factory.NewWithValidTestData<AccTaxRate>();
			otherCountryTax.AT_Type = AccTaxRate.Types.Rated;
			otherCountryTax.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST; //not a real case, just a hypothetical case
			otherCountryTax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Assert("AU does not have State Tax", !otherCountryTax.IsIndiaStateTax);
		}

		public void TestIsIndiaExtraTax()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var serviceTax = Factory.New<AccTaxRate>();
				serviceTax.AT_Type = AccTaxRate.Types.ServiceTax;
				serviceTax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				Assert(serviceTax.IsIndiaServiceTax);

				var extraServiceTax = Factory.New<AccTaxRate>();
				extraServiceTax.AT_Type = AccTaxRate.Types.NotReportable;
				extraServiceTax.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ServiceTax;
				extraServiceTax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				Assert(extraServiceTax.IsIndiaServiceTax);

				var stateTax = Factory.New<AccTaxRate>();
				stateTax.AT_Type = AccTaxRate.Types.Rated;
				stateTax.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;
				stateTax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				Assert(!stateTax.IsIndiaServiceTax);
			}
		}

		public void TestHumanReadableNameCore()
		{
			var serviceTax = Factory.NewWithValidTestData<AccTaxRate>();
			serviceTax.AT_Code = "TAX";
			serviceTax.AT_Description = "Tax Free";

			AssertEquals("Tax ID - TAX - Tax Free", serviceTax.HumanReadableName);
		}

		public void TestDoesRateExists()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRate_ForTestOnly(18, 6, ZDate.Today.AddMonths(-1), ZDate.Today.AddDays(1));
			taxRate.SetRate_ForTestOnly(6, 3, ZDate.Today.AddDays(5), ZDate.Today.AddMonths(1));

			AssertEquals(true, taxRate.DoesRateExists(ZDate.Today));
			AssertEquals(false, taxRate.DoesRateExists(ZDate.Today.AddDays(2)));
			AssertEquals(true, taxRate.DoesRateExists(ZDate.Today.AddMonths(1)));
		}

		AccTaxRate GST
		{
			get { return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10); }
		}

		AccTaxRate CAPGST
		{
			get { return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "CAPGST", AccTaxRate.Types.CapitalRated, 10); }
		}

		AccTaxRate EXEMPT
		{
			get { return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "EXEMPT", AccTaxRate.Types.Exempt, 0); }
		}

		AccTaxRate FREEGST
		{
			get { return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEGST", AccTaxRate.Types.Rated, 0); }
		}

		AccTaxRate NOTREPORT
		{
			get { return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "NOTREPORT", AccTaxRate.Types.NotReportable, 0); }
		}

		AccTaxRate GSTREV
		{
			get { return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GSTREV", AccTaxRate.Types.ReverseRated, 175, 10); }
		}

		AccTaxRate GSTSUS
		{
			get { return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GSTSUS", AccTaxRate.Types.Suspended, 125, 10); }
		}

		AccTaxRate LOWGSTREV
		{
			get { return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "LOWGSTREV", AccTaxRate.Types.ReverseRated, 6); }
		}

		AccTaxRate MIDGSTREV
		{
			get { return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "MIDGSTREV", AccTaxRate.Types.ReverseRated, 12); }
		}

		AccTaxRate FREEGSTREV
		{
			get { return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEGSTREV", AccTaxRate.Types.ReverseRated, 0); }
		}

		public void TestGetEffectiveExtraRateQST()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "GSTANDQST";
			taxRate.AT_Description = "GST and QST";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_IsActive = true;
			taxRate.SetRateNumerator_ForTestOnly(5);
			taxRate.SetExtraRate_ForTestOnly(75, 10);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;

			AssertEquals(7.875m, taxRate.GetEffectiveExtraRate(ZDate.Today));
		}

		public void TestGetEffectiveExtraRateQCT()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "GSTANDQST";
			taxRate.AT_Description = "GST and QST";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_IsActive = true;
			taxRate.SetRateNumerator_ForTestOnly(5);
			taxRate.SetExtraRate_ForTestOnly(9975, 1000);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;

			AssertEquals(9.975m, taxRate.GetEffectiveExtraRate(ZDate.Today));
		}

		public void TestGetEffectiveExtraRateSTA()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "STA";
			taxRate.AT_Description = "State tax";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_IsActive = true;
			taxRate.SetRateNumerator_ForTestOnly(9);
			taxRate.SetExtraRate_ForTestOnly(9, 1);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;

			AssertEquals(9M, taxRate.GetEffectiveExtraRate(ZDate.Today));
		}

		public void TestGetEffectiveExtraRateGSTANDEDU()
		{
			AccTaxRate eduAndGst = Factory.NewWithValidTestData<AccTaxRate>();
			eduAndGst.AT_Code = "GSTANDEDU";
			eduAndGst.AT_Description = "EDU And EDU";
			eduAndGst.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			eduAndGst.AT_IsActive = true;
			eduAndGst.SetRateNumerator_ForTestOnly(10);
			eduAndGst.SetExtraRate_ForTestOnly(3, 1);
			eduAndGst.AT_Type = AccTaxRate.Types.Rated;
			eduAndGst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;

			AssertEquals(0.3m, eduAndGst.GetEffectiveExtraRate(ZDate.Today));
		}

		public void TestGetEffectiveExtraRateINP7()
		{
			AccTaxRate eduAndGst = Factory.NewWithValidTestData<AccTaxRate>();
			eduAndGst.AT_Code = "INP7";
			eduAndGst.AT_Description = "7% Input VAT Claimed";
			eduAndGst.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			eduAndGst.AT_IsActive = true;
			eduAndGst.SetExtraRate_ForTestOnly(7, 1);
			eduAndGst.AT_Type = AccTaxRate.Types.Rated;
			eduAndGst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATClaimed;

			AssertEquals(7.527m, Math.Round(eduAndGst.GetEffectiveExtraRate(ZDate.Today), 3));
		}

		public void TestGetEffectiveExtraRateOTO()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "OTO";
			taxRate.AT_Description = "Input VAT Offset Against Output";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_IsActive = true;
			taxRate.SetExtraRate_ForTestOnly(6, 1);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax;

			AssertEquals(6m, taxRate.GetEffectiveExtraRate(ZDate.Today));
		}

		public void TestGetEffectiveExtraRateSPV()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "IVASPV";
			taxRate.AT_Description = "Split Payment VAT";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_IsActive = true;
			taxRate.SetRateNumerator_ForTestOnly(22);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;

			AssertEquals(-22m, taxRate.GetEffectiveExtraRate(ZDate.Today));
		}

		public void TestGetEffectiveExtraRateRET()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "IVARET";
			taxRate.AT_Description = "Standard VAT and Withheld VAT";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_IsActive = true;
			taxRate.SetRateNumerator_ForTestOnly(16);
			taxRate.SetExtraRate_ForTestOnly(4, 1);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;

			AssertEquals(-4m, taxRate.GetEffectiveExtraRate(ZDate.Today));
		}

		public void TestGetEffectiveExtraRateREF()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "IVAREF";
			taxRate.AT_Description = "Standard VAT and 1/2 Withheld VAT";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_IsActive = true;
			taxRate.SetRateNumerator_ForTestOnly(16);
			taxRate.SetExtraRate_ForTestOnly(1, 2);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;

			AssertEquals(-8m, taxRate.GetEffectiveExtraRate(ZDate.Today));
		}

		public void TestGetEffectiveExtraRateRVSWithSTA()
		{
			AccTaxRate taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_Code = "GREV20";
			taxRate.AT_Description = "Reverse Charge with State GST";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_IsActive = true;
			taxRate.SetRateNumerator_ForTestOnly(10);
			taxRate.SetExtraRate_ForTestOnly(10, 1);
			taxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;

			AssertEquals(0m, taxRate.GetEffectiveExtraRate(ZDate.Today));
		}

		public void TestGetExtraTaxBaseAmountGSTANDQST()
		{
			AssertEquals(315m, AccTaxRate.GetExtraTaxBaseAmount(AccTaxRate.ExtraTypes.QuebecQST, 0m, 0m, 15m, 300m));
		}

		public void TestGetExtraTaxBaseAmountQCTType()
		{
			AssertEquals(300m, AccTaxRate.GetExtraTaxBaseAmount(AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, 0m, 0m, 0m, 300m));
		}

		public void TestGetExtraTaxBaseAmountSERANDEDU()
		{
			AssertEquals(15m, AccTaxRate.GetExtraTaxBaseAmount(AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 0m, 0m, 15m, 0m));
		}

		public void TestGetExtraTaxBaseAmountINP7()
		{
			AssertEquals(215.14m, Utilities.Round(AccTaxRate.GetExtraTaxBaseAmount(AccTaxRate.ExtraTypes.ChinaInputVATClaimed, 7m, 15.06m, 0m, 0m), 2));
		}

		public void TestGetExtraTaxBaseAmountRET()
		{
			AssertEquals(-300m, AccTaxRate.GetExtraTaxBaseAmount(AccTaxRate.ExtraTypes.VATRetention, 0m, 0m, 0m, 300m));
		}

		public void TestGetExtraTaxBaseAmountREF()
		{
			AssertEquals(-300m, AccTaxRate.GetExtraTaxBaseAmount(AccTaxRate.ExtraTypes.VATRetentionFraction, 0m, 0m, 0m, 300m));
		}

		public void TestIsPostingGroupsEnabled()
		{
			var taxRatesForcountry = new TaxRateXmlParser().BuildTaxRatesDictionaryBasedOnCountry();
			var allCountries = new RefCountryCollection(Factory);

			var countriesEnabledForTaxRatesDirectlyInDatabase = new string[] {
				Constants.CountryCodes.India,
				Constants.CountryCodes.Malaysia,
				Constants.CountryCodes.Turkey,
				Constants.CountryCodes.Colombia
			};

			CombineAssertions(() =>
			{
				foreach (var country in allCountries)
				{
					var isPostingGroupsEnabledFromTaxRatesXML = taxRatesForcountry.ContainsKey(country.Code) && taxRatesForcountry[country.Code].Select(x => x.PostingGroup).Distinct().Count() > 1;
					AssertEquals("Country: " + country.Description.ToString(), isPostingGroupsEnabledFromTaxRatesXML || countriesEnabledForTaxRatesDirectlyInDatabase.Contains(country.Code.ToString()), AccTaxRate.IsPostingGroupsEnabled(country.Code));
				}
			});
		}

		public void TestTaxRateDecimal()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			var decimalPlacesMember = typeof(AccTaxRate).GetProperty(nameof(taxRate.ExtraRateForToday)).GetCustomAttributes(false).OfType<DecimalPlacesAttribute>().FirstOrDefault().DecimalPlacesMember;
			Assert(!string.IsNullOrWhiteSpace(decimalPlacesMember));

			var decimalPlacesMemberProperty = typeof(AccTaxRate).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(x => x.Name == decimalPlacesMember);
			AssertNotNull(decimalPlacesMemberProperty);

			var allCountries = new RefCountryCollection(Factory);
			foreach (RefCountry country in allCountries)
			{
				taxRate.AT_RN_NKCountry = country.Code;

				var taxRateDecimal = (int)decimalPlacesMemberProperty.GetValue(taxRate);

				switch (country.Code)
				{
					case "VN":
					case "BO":
						{
							AssertEquals(3, taxRateDecimal);
							break;
						}
					default:
						{
							AssertEquals(2, taxRateDecimal);
							break;
						}
				}
			}
		}

		public void TestFindExistingTaxRate()
		{
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "CNC";
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.CocosKeelingIslands;

			GlbBranch newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "CNB";
			newBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, newCompany.GC_RN_NKCountryCode)).Code;

			Factory.Save();

			var fESDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FES"));
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, newBranch.PK.ToGuid(), fESDepartment.PK.ToGuid()))
			{
				var localTaxRate1 = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "LOCTAX1", AccTaxRate.Types.Rated, 205, 10);
				var localTaxRate2 = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "LOCTAX2", AccTaxRate.Types.Exempt, 105, 10);
				var localTaxRate3 = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "LOCTAX3", AccTaxRate.Types.ReverseRated, 105, 10);
				var localTaxRate4 = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "LOCTAX4", AccTaxRate.Types.ReverseRated, 95, 10);

				var loadedTaxRates = AccTaxRate.FindExistingTaxRate(Factory, null, null, null);
				AssertCollectionContains(localTaxRate1, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate2, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate3, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate4, loadedTaxRates, false);

				loadedTaxRates = AccTaxRate.FindExistingTaxRate(Factory, "LOCTAX1", null, null);
				AssertCollectionContains(localTaxRate1, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate2, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate3, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate4, loadedTaxRates, false);

				loadedTaxRates = AccTaxRate.FindExistingTaxRate(Factory, null, AccTaxRate.Types.ReverseRated, null);
				AssertCollectionContains(localTaxRate1, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate2, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate3, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate4, loadedTaxRates, false);

				loadedTaxRates = AccTaxRate.FindExistingTaxRate(Factory, "LOCTAX3", AccTaxRate.Types.ReverseRated, null);
				AssertCollectionContains(localTaxRate1, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate2, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate3, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate4, loadedTaxRates, false);

				loadedTaxRates = AccTaxRate.FindExistingTaxRate(Factory, null, null, Core.Constants.CountryCodes.China);
				AssertCollectionContains(localTaxRate1, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate2, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate3, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate4, loadedTaxRates, false);

				loadedTaxRates = AccTaxRate.FindExistingTaxRate(Factory, null, null, Core.Constants.CountryCodes.CocosKeelingIslands);
				AssertCollectionContains(localTaxRate1, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate2, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate3, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate4, loadedTaxRates, false);

				loadedTaxRates = AccTaxRate.FindExistingTaxRate(Factory, null, null, Core.Constants.CountryCodes.CocosKeelingIslands);
				AssertCollectionContains(localTaxRate1, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate2, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate3, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate4, loadedTaxRates, false);

				loadedTaxRates = AccTaxRate.FindExistingTaxRate(Factory, "LOCTAX1", AccTaxRate.Types.Rated, Core.Constants.CountryCodes.CocosKeelingIslands);
				AssertCollectionContains(localTaxRate1, loadedTaxRates, true);
				AssertCollectionContains(localTaxRate2, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate3, loadedTaxRates, false);
				AssertCollectionContains(localTaxRate4, loadedTaxRates, false);
			}
		}

		public void TestRestrictTaxRateChange()
		{
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "Test Rate", "RAT", 10);
			rate.AT_ReferenceRateType = string.Empty;
			rate.AT_ReferenceExtraRateType = string.Empty;
			AssertEquals(false, rate.SystemTaxRate);

			rate.AT_ReferenceRateType = "STD";
			rate.AT_ReferenceExtraRateType = "QCT";
			AssertEquals(true, rate.SystemTaxRate);
		}

		public void TestValidateReadOnlyProperties()
		{
			var excludedPropertiesList = new[] {
				AccTaxRate.Schema.AT_A9_DefaultVatClass,
				AccTaxRate.Schema.AT_Description,
				AccTaxRate.Schema.AT_PostingGroupId,
				AccTaxRate.Schema.AT_IsActive,
				AccTaxRate.Schema.AT_RateSource,
				AccTaxRate.Schema.AT_TaxSystemCode,
				AccTaxRate.Schema.AT_SystemCreateTimeUtc,
				AccTaxRate.Schema.AT_SystemCreateUser,
				AccTaxRate.Schema.AT_SystemLastEditTimeUtc,
				AccTaxRate.Schema.AT_SystemLastEditUser,
			};

			var alwaysReadOnlyProperties = new[] {
				AccTaxRate.Schema.AT_Code,
				AccTaxRate.Schema.AT_ExtraTaxRateType,
				AccTaxRate.Schema.AT_Type,
				AccTaxRate.Schema.AT_ReferenceRateType,
				AccTaxRate.Schema.AT_ReferenceExtraRateType,
				AccTaxRate.Schema.AT_RN_NKCountry };

			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "Test Rate", "RAT", 10);
			rate.AT_ReferenceRateType = string.Empty;
			AssertPropertyForReadOnly(false);

			rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "Test Rate", "RAT", 10);
			rate.AT_ReferenceRateType = "STD";
			AssertPropertyForReadOnly(true);

			void AssertPropertyForReadOnly(bool isReadOnly)
			{
				CombineAssertions(() =>
				{
					foreach (var name in rate.ZPropertyInfoHash.Cast<ZPropertyInfo>().Select(x => x.Name).Except(excludedPropertiesList).Except(alwaysReadOnlyProperties))
					{
						var property = rate.ZPropertyInfoHash[name];
						AssertEquals($"Checking property - '{property.Name}' for readonlyness", isReadOnly, property.ReadOnly);
					}
					foreach (var name in alwaysReadOnlyProperties)
					{
						var property = rate.ZPropertyInfoHash[name];
						AssertEquals("Property should be always readonly", true, property.ReadOnly);
					}
					foreach (var name in excludedPropertiesList)
					{
						var property = rate.ZPropertyInfoHash[name];
						AssertEquals("Property should not be readonly", false, property.ReadOnly);
					}
				});
			}
		}

		public void TestNoStmALogs()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, taxRate.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				taxRate.AT_Description = "TEST LOG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				taxRate.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		#region Unit tests for RefDB properties

		public void TestTaxRateUsesOldRate()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = "AU";
			CreateObsoleteRateWeGetFromDataTransformation(taxRate, "RateObsolete", "9");
			AssertEquals(9M, taxRate.RateForTodayForUIBinding);

			MasterFilesTestHelper.CreateRefTaxRate(Factory, "AU", "STD", 8, 4, ZDateTime.Now.AddYears(-1).Date, ZDateTime.Now.AddMonths(2).Date);
			taxRate.AT_ReferenceRateType = "STD";
			Factory.Save();
			var factory = new BusinessObjectFactory();
			taxRate = factory.Load<AccTaxRate>(taxRate.PK);

			AssertEquals(2M, taxRate.RateForTodayForUIBinding);
		}

		public void TestExtraRateForTodayForUIBinding()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = "AU";
			CreateObsoleteRateWeGetFromDataTransformation(taxRate, "ExtraTaxRateNumeratorObsolete", "16");
			CreateObsoleteRateWeGetFromDataTransformation(taxRate, "ExtraTaxRateDenominatorObsolete", "4");
			AssertEquals(4M, taxRate.ExtraRateForTodayForUIBinding);

			MasterFilesTestHelper.CreateRefTaxRate(Factory, "AU", "MID", 21, 7, ZDateTime.Now.AddMonths(-1).Date, ZDateTime.Now.AddMonths(2).Date);
			taxRate.AT_ReferenceRateType = "XXX";
			taxRate.AT_ReferenceExtraRateType = "MID";
			Factory.Save();
			var factory = new BusinessObjectFactory();
			taxRate = factory.Load<AccTaxRate>(taxRate.PK);

			AssertEquals(3M, taxRate.ExtraRateForTodayForUIBinding);
		}

		public void TestRefTaxRateDataForUIBinding()
		{
			CreateTaxRate();
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_ReferenceRateType = "STD";
			taxRate.AT_RN_NKCountry = "AU";
			AssertNotNull(taxRate.RefTaxRateDataForUIBinding);
			AssertEquals(3, taxRate.RefTaxRateDataForUIBinding.Count);
		}

		public void TestRefExtraTaxRateDataForUIBinding()
		{
			CreateTaxRate();
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_ReferenceExtraRateType = "MID";
			taxRate.AT_RN_NKCountry = "AU";
			AssertNotNull(taxRate.RefExtraTaxRateDataForUIBinding);
			AssertEquals(2, taxRate.RefExtraTaxRateDataForUIBinding.Count);
		}

		void CreateTaxRate()
		{
			MasterFilesTestHelper.CreateRefTaxRate(Factory, "AU", "STD", 125, 10, ZDateTime.Now.AddMonths(-1).Date, ZDateTime.Now.AddMonths(2).Date);
			MasterFilesTestHelper.CreateRefTaxRate(Factory, "AU", "STD", 234, 343, ZDateTime.Now.AddMonths(-5).Date, ZDateTime.Now.AddMonths(-2).Date);
			MasterFilesTestHelper.CreateRefTaxRate(Factory, "AU", "STD", 5, 2, ZDateTime.Now.AddMonths(3).Date, ZDateTime.Now.AddMonths(9).Date);
			MasterFilesTestHelper.CreateRefTaxRate(Factory, "AU", "MID", 15, 3, ZDateTime.Now.AddMonths(-1).Date, ZDateTime.Now.AddMonths(2).Date);
			MasterFilesTestHelper.CreateRefTaxRate(Factory, "AU", "MID", 15, 3, ZDateTime.Now.AddMonths(3).Date, ZDateTime.Now.AddMonths(5).Date);
		}

		public void TestTaxRateIsZeroTaxRate()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_ReferenceRateType = Constants.ZeroTaxReferenceRateType.Zero;
			taxRate.AT_ReferenceExtraRateType = "";
			Assert(taxRate.IsZeroTaxRate);

			taxRate.AT_ReferenceExtraRateType = "STD";
			Assert(!taxRate.IsZeroTaxRate);

			taxRate.AT_ReferenceRateType = Constants.ZeroTaxReferenceRateType.Empty;
			taxRate.AT_ReferenceExtraRateType = "";
			Assert(taxRate.IsZeroTaxRate);

			taxRate.AT_ReferenceExtraRateType = "STD";
			Assert(!taxRate.IsZeroTaxRate);

			taxRate.AT_ReferenceRateType = "STD";
			taxRate.AT_ReferenceExtraRateType = "";
			Assert(!taxRate.IsZeroTaxRate);

			taxRate.AT_ReferenceExtraRateType = "STD";
			Assert(!taxRate.IsZeroTaxRate);
		}

		#endregion

		GenAddOnColumn CreateObsoleteRateWeGetFromDataTransformation(AccTaxRate rate, ZString propertyName, ZString propertyValue)
		{
			GenAddOnColumns = GenAddOnColumns ?? new GenAddOnColumnCollection(rate);
			var genAddOnColumn = GenAddOnColumns.AddNew();
			genAddOnColumn.XA_Name = propertyName;
			genAddOnColumn.XA_Data = propertyValue;

			return genAddOnColumn;
		}

		GenAddOnColumnCollection GenAddOnColumns;
	}
}
