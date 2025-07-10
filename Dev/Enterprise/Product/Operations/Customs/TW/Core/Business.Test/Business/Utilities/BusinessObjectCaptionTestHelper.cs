using System;
using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	public static class BusinessObjectCaptionTestHelper
	{
		[ExpectNoExceptions]
		public static void CombineAssertCaptions(ZPropertyInfo propertyInfo, string expectedCaption, string expectedShortCaption, [CallerLineNumber] int lineNumber = 0)
		{
			AssertWithNullCheck(propertyInfo, (resourceStringData) =>
			{
				AssertionWithHtml.CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo(expectedCaption), $"{resourceStringData.Key} Caption (Line: {lineNumber})");
					NUnit.Framework.Assert.That(resourceStringData.ShortCaption, NUnit.Framework.Is.EqualTo(expectedShortCaption), $"{resourceStringData.Key} Short Caption (Line: {lineNumber})");
				});
			}, lineNumber);
		}

		[ExpectNoExceptions]
		public static void AssertCaptionsWithFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedFullDescription, [CallerLineNumber] int lineNumber = 0)
		{
			AssertWithNullCheck(propertyInfo, (resourceStringData) =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo(expectedCaption), $"{resourceStringData.Key} Caption (Line: {lineNumber})");
				NUnit.Framework.Assert.That(resourceStringData.FullDescription, NUnit.Framework.Is.EqualTo(expectedFullDescription), $"{resourceStringData.Key} Full Description (Line: {lineNumber})");
			}, lineNumber);
		}

		[ExpectNoExceptions]
		public static void AssertCaptionsWithFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedShortCaption, string expectedFullDescription, [CallerLineNumber] int lineNumber = 0)
		{
			AssertWithNullCheck(propertyInfo, (resourceStringData) =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo(expectedCaption), $"{resourceStringData.Key} Caption (Line: {lineNumber})");
				NUnit.Framework.Assert.That(resourceStringData.ShortCaption, NUnit.Framework.Is.EqualTo(expectedShortCaption), $"{resourceStringData.Key} Short Caption (Line: {lineNumber})");
				NUnit.Framework.Assert.That(resourceStringData.FullDescription, NUnit.Framework.Is.EqualTo(expectedFullDescription), $"{resourceStringData.Key} Full Description (Line: {lineNumber})");
			}, lineNumber);
		}

		[ExpectNoExceptions]
		public static void AssertCaptionsWithFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedMediumCaption, string expectedShortCaption, string expectedFullDescription, [CallerLineNumber] int lineNumber = 0)
		{
			AssertWithNullCheck(propertyInfo, (resourceStringData) =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo(expectedCaption), $"{resourceStringData.Key} Caption (Line: {lineNumber})");
				NUnit.Framework.Assert.That(resourceStringData.MediumCaption, NUnit.Framework.Is.EqualTo(expectedMediumCaption), $"{resourceStringData.Key} Medium (Line: {lineNumber})");
				NUnit.Framework.Assert.That(resourceStringData.ShortCaption, NUnit.Framework.Is.EqualTo(expectedShortCaption), $"{resourceStringData.Key} Short Caption (Line: {lineNumber})");
				NUnit.Framework.Assert.That(resourceStringData.FullDescription, NUnit.Framework.Is.EqualTo(expectedFullDescription), $"{resourceStringData.Key} Full Description (Line: {lineNumber})");
			}, lineNumber);
		}

		[ExpectNoExceptions]
		public static void CombineAssertCaptionsWithFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedMediumCaption, string expectedShortCaption, string expectedFullDescription, [CallerLineNumber] int lineNumber = 0)
		{
			AssertWithNullCheck(propertyInfo, (resourceStringData) =>
			{
				AssertionWithHtml.CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo(expectedCaption), $"{resourceStringData.Key} Caption (Line: {lineNumber})");
					NUnit.Framework.Assert.That(resourceStringData.MediumCaption, NUnit.Framework.Is.EqualTo(expectedMediumCaption), $"{resourceStringData.Key} Medium (Line: {lineNumber})");
					NUnit.Framework.Assert.That(resourceStringData.ShortCaption, NUnit.Framework.Is.EqualTo(expectedShortCaption), $"{resourceStringData.Key} Short Caption (Line: {lineNumber})");
					NUnit.Framework.Assert.That(resourceStringData.FullDescription, NUnit.Framework.Is.EqualTo(expectedFullDescription), $"{resourceStringData.Key} Full Description (Line: {lineNumber})");
				});
			}, lineNumber);
		}

		[ExpectNoExceptions]
		public static void AssertCaptions(ZPropertyInfo propertyInfo, string expectedCaption, [CallerLineNumber] int lineNumber = 0)
		{
			AssertWithNullCheck(propertyInfo, (resourceStringData) =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo(expectedCaption), $"{resourceStringData.Key} Caption (Line: {lineNumber})");
			}, lineNumber);
		}

		[ExpectNoExceptions]
		public static void AssertCaptions(ZPropertyInfo propertyInfo, string expectedCaption, string expectedShortCaption, [CallerLineNumber] int lineNumber = 0)
		{
			AssertWithNullCheck(propertyInfo, (resourceStringData) =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo(expectedCaption), $"{resourceStringData.Key} Caption (Line: {lineNumber})");
				NUnit.Framework.Assert.That(resourceStringData.ShortCaption, NUnit.Framework.Is.EqualTo(expectedShortCaption), $"{resourceStringData.Key} Short Caption (Line: {lineNumber})");
			}, lineNumber);
		}

		static void AssertWithNullCheck(ZPropertyInfo propertyInfo, Action<ResourceStringData> assertions, int lineNumber = 0)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			if (resourceStringData != null)
			{
				assertions(resourceStringData);
			}
			else
			{
				Assertion.Fail($"NULL ResourceStringData for {propertyInfo.Name} Line: {lineNumber}");
			}
		}

		[ExpectNoExceptions]
		public static void AssertCaptions(ZPropertyInfo propertyInfo, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo(expectedCaption), $"{resourceStringData.Key} Caption");
			NUnit.Framework.Assert.That(resourceStringData.MediumCaption, NUnit.Framework.Is.EqualTo(expectedMediumCaption), $"{resourceStringData.Key} Medium");
			NUnit.Framework.Assert.That(resourceStringData.ShortCaption, NUnit.Framework.Is.EqualTo(expectedShortCaption), $"{resourceStringData.Key} Short Caption");
		}
	}
}
