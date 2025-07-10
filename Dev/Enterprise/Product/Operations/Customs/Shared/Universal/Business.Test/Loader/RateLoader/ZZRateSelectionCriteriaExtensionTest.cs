using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class ZZRateSelectionCriteriaExtensionTest : TestCase
	{
		[TestDate(2020, 08, 12)]
		public void TestValidEffectiveDate()
		{
			CombineAssertions(() =>
			{
				var validDate = ZDateTime.Today.AddMonths(-1);
				var criteriaSetWithValidDate = new SpecificRateSelectionCriteria("", "", "", "", new HashSet<ZString>(), validDate, "", "");
				AssertEquals("ValidEffectiveDate with a valid date", validDate, criteriaSetWithValidDate.ValidEffectiveDate());
				AssertEquals("EffectiveDate with a valid date", validDate, criteriaSetWithValidDate.EffectiveDate);
				var invalidDate = ZDateTime.Invalid;
				var criteriaSetWithInvalidDate = new SpecificRateSelectionCriteria("", "", "", "", new HashSet<ZString>(), invalidDate, "", "");
				AssertEquals("ValidEffectiveDate with an invalid date", ZDateTime.Today, criteriaSetWithInvalidDate.ValidEffectiveDate());
				AssertEquals("EffectiveDate with an invalid date", invalidDate, criteriaSetWithInvalidDate.EffectiveDate);
			});
		}

		public void TestFlatSecondTradeGroups()
		{
			CombineAssertions(() =>
			{
				var criteriaSetWithNullSecondTradeGroups = new SpecificRateSelectionCriteria("", "", "", "", additionalCodes: null, ZDateTime.Invalid, "", "");
				AssertEquals("FlatSecondTradeGroups with a null SecondTradeGroups set", "", criteriaSetWithNullSecondTradeGroups.FlatSecondTradeGroups());

				var criteriaSetWithEmptySecondTradeGroups = new SpecificRateSelectionCriteria("", "", "", "", null, ZDateTime.Invalid, "", "");
				AssertEquals("FlatSecondTradeGroups with an empty SecondTradeGroups set", "", criteriaSetWithEmptySecondTradeGroups.FlatSecondTradeGroups());

				var criteriaSetWithDuplicateSecondTradeGroups = new SpecificRateSelectionCriteria("", "", "", "", null, ZDateTime.Invalid, "", "", new HashSet<ZString> { "TG2", "TG1", "TG2", "", "TG1" });
				AssertEquals("FlatSecondTradeGroups with duplicate SecondTradeGroups and sorted", "TG1.TG2", criteriaSetWithDuplicateSecondTradeGroups.FlatSecondTradeGroups());
			});
		}

		public void TestFlatAdditionalCodes()
		{
			CombineAssertions(() =>
			{
				var criteriaSetWithNullAdditionalCodes = new SpecificRateSelectionCriteria("", "", "", "", additionalCodes: null, ZDateTime.Invalid, "", "");
				AssertEquals("FlatAdditionalCodes with a null additional code set", "", criteriaSetWithNullAdditionalCodes.FlatAdditionalCodes());

				var criteriaSetWithEmptyAdditionalCodes = new SpecificRateSelectionCriteria("", "", "", "", new HashSet<ZString>(), ZDateTime.Invalid, "", "");
				AssertEquals("FlatAdditionalCodes with an empty additional code set", "", criteriaSetWithEmptyAdditionalCodes.FlatAdditionalCodes());

				var criteriaSetWithDuplicateAdditionalCodes = new SpecificRateSelectionCriteria("", "", "", "", new HashSet<ZString> { "AC3", "", "AC2", "AC1", "AC2" }, ZDateTime.Invalid, "", "");
				AssertEquals("FlatAdditionalCodes with duplicate additional codes and sorted", "AC1.AC2.AC3", criteriaSetWithDuplicateAdditionalCodes.FlatAdditionalCodes());
			});
		}

		public void TestXmlAdditionalCodes()
		{
			CombineAssertions(() =>
			{
				var criteriaSetWithNullAdditionalCodes = new SpecificRateSelectionCriteria("", "", "", "", additionalCodes: null, ZDateTime.Invalid, "", "");
				AssertEquals("XmlAdditionalCodes with a null additional code set", "<v></v>", criteriaSetWithNullAdditionalCodes.XmlAdditionalCodes());
				var criteriaSetWithEmptyAdditionalCodes = new SpecificRateSelectionCriteria("", "", "", "", new HashSet<ZString>(), ZDateTime.Invalid, "", "");
				AssertEquals("XmlAdditionalCodes with an empty additional code set", "<v></v>", criteriaSetWithEmptyAdditionalCodes.XmlAdditionalCodes());
				var criteriaSetWithAdditionalCodes = new SpecificRateSelectionCriteria("", "", "", "", new HashSet<ZString> { "AC1", "AC2", "AC3" }, ZDateTime.Invalid, "", "");
				AssertEquals("XmlAdditionalCodes with additional codes", "<v>AC1</v><v>AC2</v><v>AC3</v>", criteriaSetWithAdditionalCodes.XmlAdditionalCodes());
			});
		}
	}
}
