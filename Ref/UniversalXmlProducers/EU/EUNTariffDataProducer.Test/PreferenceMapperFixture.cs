using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class PreferenceMapperFixture
	{
		[Test]
		public void SetsSinglePreferencesForRate()
		{
			var applicability1 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "EUN", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var rate1 = new RefCusRate
			{
				RefCusApplicabilities = new[] { applicability1 },
				ZZ2_RateFormula = "1.23 * VFD",
				ZZ2_ZZS_NKPreference = "117"
			};

			IPreferenceMapper preferenceMapper = new PreferenceMapper();
			var result = preferenceMapper.SetPreferenceOnRates(new[] { rate1 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.True(result.Any(x => x.ZZ2_ZZS_NKPreference == "140"));
			Assert.True(result.All(x => x.RefCusApplicabilities.All(y => y.ZZT_ZZA_NKTradeGroup == "EUN")));
		}

		[Test]
		public void SetsMultiplePreferencesForRate()
		{
			var applicability1 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "EUN", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var rate1 = new RefCusRate
			{
				RefCusApplicabilities = new[] { applicability1 },
				ZZ2_RateFormula = "1.23 * VFD",
				ZZ2_ZZS_NKPreference = "142"
			};

			IPreferenceMapper preferenceMapper = new PreferenceMapper();
			var result = preferenceMapper.SetPreferenceOnRates(new[] { rate1 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.True(result.Any(x => x.ZZ2_ZZS_NKPreference == "300"));
			Assert.True(result.All(x => x.RefCusApplicabilities.All(y => y.ZZT_ZZA_NKTradeGroup == "EUN")));
		}

		[Test]
		public void SetsMultiplePreferencesForGSPRate()
		{
			var applicability1 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "2005", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var rate1 = new RefCusRate
			{
				RefCusApplicabilities = new[] { applicability1 },
				ZZ2_RateFormula = "1.23 * VFD",
				ZZ2_ZZS_NKPreference = "142"
			};

			IPreferenceMapper preferenceMapper = new PreferenceMapper();
			var result = preferenceMapper.SetPreferenceOnRates(new[] { rate1 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.True(result.Any(x => x.ZZ2_ZZS_NKPreference == "200"));
			Assert.True(result.All(x => x.RefCusApplicabilities.All(y => y.ZZT_ZZA_NKTradeGroup == "2005")));
		}

		[Test]
		public void NoPreferenceForA20ForRate()
		{
			var applicability1 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "EUN", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var rate1 = new RefCusRate
			{
				RefCusApplicabilities = new[] { applicability1 },
				ZZ2_RateFormula = "1.23 * VFD",
				ZZ2_ZZS_NKPreference = "651"
			};

			IPreferenceMapper preferenceMapper = new PreferenceMapper();
			var result = preferenceMapper.SetPreferenceOnRates(new[] { rate1 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(string.Empty, result[0].ZZ2_ZZS_NKPreference);
		}

		[Test]
		public void Handles464PreferencesForRate()
		{
			var applicability1 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "1011", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var applicability2 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "1033", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var applicability3 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "1055", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };

			var rate1 = new RefCusRate
			{
				RefCusApplicabilities = new[] { applicability1, applicability2, applicability3 },
				ZZ2_RateFormula = "1.23 * VFD",
				ZZ2_ZZS_NKPreference = "103"
			};

			var rate2 = new RefCusRate
			{
				RefCusApplicabilities = new[] { applicability1 },
				ZZ2_RateFormula = "1.23 * VFD",
				ZZ2_ZZS_NKPreference = "464"
			};

			IPreferenceMapper preferenceMapper = new PreferenceMapper();
			var result = preferenceMapper.SetPreferenceOnRates(new[] { rate1, rate2 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);
			Assert.True(result.Any(x => x.ZZ2_ZZS_NKPreference == "100"));
			Assert.True(result.Any(x => string.IsNullOrEmpty(x.ZZ2_ZZS_NKPreference) && string.IsNullOrEmpty(x.ZZ2_ZZS_ZZZ_NKDataGrouping)));
		}

		[Test]
		public void SetsSinglePreferencesForCondition()
		{
			var applicability1 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "EUN", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var condition1 = new RefCusCondition
			{
				RefCusApplicabilities = new[] { applicability1 },
				ZX1_Comment = "Condtion M",
				ZX1_ZZS_NKPreference = "117"
			};

			IPreferenceMapper preferenceMapper = new PreferenceMapper();
			var result = preferenceMapper.SetPreferenceOnConditions(new[] { condition1 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.True(result.Any(x => x.ZX1_ZZS_NKPreference == "140"));
			Assert.True(result.All(x => x.RefCusApplicabilities.All(y => y.ZZT_ZZA_NKTradeGroup == "EUN")));
		}

		[Test]
		public void SetsMultiplePreferencesForCondition()
		{
			var applicability1 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "EUN", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var condition1 = new RefCusCondition
			{
				RefCusApplicabilities = new[] { applicability1 },
				ZX1_Comment = "Condtion M",
				ZX1_ZZS_NKPreference = "142"
			};

			IPreferenceMapper preferenceMapper = new PreferenceMapper();
			var result = preferenceMapper.SetPreferenceOnConditions(new[] { condition1 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.True(result.Any(x => x.ZX1_ZZS_NKPreference == "300"));
			Assert.True(result.All(x => x.RefCusApplicabilities.All(y => y.ZZT_ZZA_NKTradeGroup == "EUN")));
		}

		[Test]
		public void SetsMultiplePreferencesForGSPCondition()
		{
			var applicability1 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "2005", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var condition1 = new RefCusCondition
			{
				RefCusApplicabilities = new[] { applicability1 },
				ZX1_Comment = "Condtion M",
				ZX1_ZZS_NKPreference = "142"
			};

			IPreferenceMapper preferenceMapper = new PreferenceMapper();
			var result = preferenceMapper.SetPreferenceOnConditions(new[] { condition1 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.True(result.Any(x => x.ZX1_ZZS_NKPreference == "200"));
			Assert.True(result.All(x => x.RefCusApplicabilities.All(y => y.ZZT_ZZA_NKTradeGroup == "2005")));
		}

		[Test]
		public void NoPreferenceForCondition()
		{
			var applicability1 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "EUN", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var condition1 = new RefCusCondition
			{
				RefCusApplicabilities = new[] { applicability1 },
				ZX1_Comment = "Condtion M",
				ZX1_ZZS_NKPreference = "465"
			};

			IPreferenceMapper preferenceMapper = new PreferenceMapper();
			var result = preferenceMapper.SetPreferenceOnConditions(new[] { condition1 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(string.Empty, result[0].ZX1_ZZS_NKPreference);
			Assert.AreEqual("Condtion M", result[0].ZX1_Comment);
		}

		[Test]
		public void Handles464PreferencesForCondition()
		{
			var applicability1 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "1011", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var applicability2 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "1033", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };
			var applicability3 = new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "1055", ZZT_ZZA_ZZZ_NKDataGrouping = "EUN" };

			var condition1 = new RefCusCondition
			{
				RefCusApplicabilities = new[] { applicability1, applicability2, applicability3 },
				ZX1_Comment = "Condtion M",
				ZX1_ZZS_NKPreference = "103"
			};

			var condition2 = new RefCusCondition
			{
				RefCusApplicabilities = new[] { applicability1 },
				ZX1_Comment = "Condtion M",
				ZX1_ZZS_NKPreference = "464"
			};

			IPreferenceMapper preferenceMapper = new PreferenceMapper();
			var result = preferenceMapper.SetPreferenceOnConditions(new[] { condition1, condition2 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);
			Assert.True(result.Any(x => x.ZX1_ZZS_NKPreference == "100"));
			Assert.True(result.Any(x => string.IsNullOrEmpty(x.ZX1_ZZS_NKPreference) && string.IsNullOrEmpty(x.ZX1_ZZS_ZZZ_NKDataGrouping)));
		}
	}
}
