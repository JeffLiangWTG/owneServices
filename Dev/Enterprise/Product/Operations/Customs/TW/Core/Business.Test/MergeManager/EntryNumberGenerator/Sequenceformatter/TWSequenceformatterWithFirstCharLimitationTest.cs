using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWSequenceformatterWithFirstCharLimitationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetFormattedInt()
		{
			var formatter = new TWSequenceformatterWithFirstCharLimitation(1377, 3, new List<char> { 'A', 'B', 'C' }, new List<char> { 'D', 'E', 'F' });
			NUnit.Framework.Assert.That(formatter.FormatIntToString(1), NUnit.Framework.Is.EqualTo("001"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(999), NUnit.Framework.Is.EqualTo("999"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(1000), NUnit.Framework.Is.EqualTo("D01"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(1296), NUnit.Framework.Is.EqualTo("F99"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(1297), NUnit.Framework.Is.EqualTo("DA1"));
			NUnit.Framework.Assert.That(formatter.FormatIntToString(1377), NUnit.Framework.Is.EqualTo("FC9"));
		}

		[ExpectNoExceptions]
		public void TestDoesEntryNumberFallIntoThisCategory()
		{
			var formatter = new TWSequenceformatterWithFirstCharLimitation(1377, 3, new List<char> { 'A', 'B', 'C' }, new List<char> { 'D', 'E', 'F' });
			NUnit.Framework.Assert.That(!formatter.DoesEntryNumberFallIntoThisCategory("AAF50812X00010"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!formatter.DoesEntryNumberFallIntoThisCategory("AAF50812XA0010"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!formatter.DoesEntryNumberFallIntoThisCategory("AAF50812XD0010"), NUnit.Framework.Is.True);
		}
	}
}
