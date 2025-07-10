using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TaxIdAndTaxMessageCombinationRulesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLineTypes()
		{
			var lineTypes = new string[]
			{
				"REV",
				"CST",
				"DRC",
				"DPY",
			};

			AssertContainsExactElementsInAnyOrder(lineTypes, Parent.Lookups.LineTypes.GetAllCodes());
		}

		public void TestTaxRates()
		{
			var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var newValidTaxRate = taxRateCollection.AddNew();
			newValidTaxRate.AT_Code = "TGST";
			newValidTaxRate.AT_Type = "RAT";
			newValidTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			newValidTaxRate.SetRateNumerator_ForTestOnly(10);

			Factory.Save();

			var cccTaxRateCollectionInDB = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cccTaxRateCollectionInDB.Load();
			var taxRateCodes = cccTaxRateCollectionInDB.OfType<AccTaxRate>()
				.Select(taxRate => taxRate.AT_Code)
				.ToArray();

			var taxRatesInLookups = Parent.Lookups.TaxRates;
			taxRatesInLookups.Load();
			AssertContainsExactElementsInAnyOrder(taxRateCodes, taxRatesInLookups.OfType<AccTaxRate>().Select(x => x.AT_Code).ToArray());

			AssertEquals("TGST", ((AccTaxRate)taxRatesInLookups.FindByPK(newValidTaxRate.PK)).AT_Code);
		}

		public void TestTaxMessages()
		{
			var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var newValidTaxMessage = taxMessageCollection.AddNew();
			newValidTaxMessage.A9_Code = "AAAAAA";
			newValidTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();

			var taxMessages = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).Cast<AccInvMsg>()
				.Select(taxMessage => taxMessage.A9_Code)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(taxMessages, Parent.Lookups.TaxMessages.Find(new ZQuery()).OfType<AccInvMsg>().Select(x => x.A9_Code).ToArray());

			AssertEquals("AAAAAA", ((AccInvMsg)Parent.Lookups.TaxMessages.FindByPK(newValidTaxMessage.PK)).A9_Code);
		}

		public void TestCountryCode()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Parent.Lookups.CountryCode);
		}

		TaxIdAndTaxMessageCombinationRules Parent
		{
			get
			{
				if (parent == null)
				{
					parent = new TaxIdAndTaxMessageCombinationRules();
					parent.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				}
				return parent;
			}
		}

		TaxIdAndTaxMessageCombinationRules parent;
	}
}
