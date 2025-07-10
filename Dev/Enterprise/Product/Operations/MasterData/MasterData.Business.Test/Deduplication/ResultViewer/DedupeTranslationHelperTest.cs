using System.Collections.Generic;
using System.Reflection;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Test
{
	[TestedType(typeof(DedupeTranslationHelper))]
	public class DedupeTranslationHelperTest : TestCase
	{
		public void TestConfidenceDescriptionDictionaryKeysAndCount_AreCorrect()
		{
			var fieldInfo = typeof(DedupeTranslationHelper).GetField("confidenceDescriptionDictionary", BindingFlags.NonPublic | BindingFlags.Static);
			var dictionary = (IReadOnlyDictionary<string, ResourceString>)fieldInfo.GetValue(null);
			var expectedKeys = new List<string>
			{
				"None",
				"Low",
				"Medium",
				"High",
				"Undefined",
				"Exact"
			};

			AssertContainsExactElementsInAnyOrder(expectedKeys, dictionary.Keys);
		}

		public void TestModelSourceDictionaryKeysAndCount_AreCorrect()
		{
			var fieldInfo = typeof(DedupeTranslationHelper).GetField("modelSourceDictionary", BindingFlags.NonPublic | BindingFlags.Static);
			var dictionary = (IReadOnlyDictionary<string, ResourceString>)fieldInfo.GetValue(null);
			var expectedKeys = new List<string>
			{
				"Name",
				"Number",
				"Website",
				"Domain",
				"Email",
				"Birthday"
			};

			AssertContainsExactElementsInAnyOrder(expectedKeys, dictionary.Keys);
		}

		public void TestModelHeaderCaptionDictionaryKeysAndCount_AreCorrect()
		{
			var fieldInfo = typeof(DedupeTranslationHelper).GetField("modelHeaderCaptionDictionary", BindingFlags.NonPublic | BindingFlags.Static);
			var dictionary = (IReadOnlyDictionary<string, ResourceString>)fieldInfo.GetValue(null);
			var expectedKeys = new List<string>
			{
				"Emails",
				"Birthdays",
				"Organisations",
				"Addresses",
				"Contacts",
				"RegistrationCodes",
				"Websites",
				"Domains",
				"PhoneNumbers",
				"OrganisationNames",
				"PersonNames",
				"Person",
				"Staff",
				"Applicant",
				"ActiveAssociations"
			};

			AssertContainsExactElementsInAnyOrder(expectedKeys, dictionary.Keys);
		}

		[ExpectNoExceptions]
		public void TestDedupeTranslationHelper_NoException()
		{
			AssertEquals(string.Empty, DedupeTranslationHelper.GetConfidenceDescription(null));
			AssertEquals(string.Empty, DedupeTranslationHelper.GetModelSource(null));
			AssertEquals(string.Empty, DedupeTranslationHelper.GetModelHeaderCaption(null));
		}

		[ExpectNoExceptions]
		public void TestDedupeTranslationHelper_UnHandledKey()
		{
			AssertEquals("XXX", DedupeTranslationHelper.GetConfidenceDescription("XXX"));
			AssertEquals("YYY", DedupeTranslationHelper.GetModelSource("YYY"));
			AssertEquals("XYZ", DedupeTranslationHelper.GetModelHeaderCaption("XYZ"));
		}
	}
}
