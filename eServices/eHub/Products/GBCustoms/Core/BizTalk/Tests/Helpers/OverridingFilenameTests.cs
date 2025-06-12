using System;
using System.Collections.Generic;
using CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.Helpers
{
	[TestFixture]
	public class OverridingFilenameTests
	{
		[TestFixture]
		public class ParseMethod
		{
			[Test]
			public void WhenGettingValidFilename_ShouldReturnEnterpriseDatabaseCodeAndBadge()
			{
				// Arrange.

				var text = "ENTDBC-CSP-BADGE";

				// Act.

				var overridingFilename = OverridingFilename.Parse(text);

				// Assert.

				Assert.That(overridingFilename, Is.Not.Null);
				Assert.That(overridingFilename.EnterpriseDatabaseCode, Is.EqualTo("ENTDBC"));
				Assert.That(overridingFilename.Badge, Is.EqualTo("BADGE"));
			}

			[Test]
			[TestCaseSource(typeof(TestSource), "InvalidCases")]
			public void WhenGettingInvalidFilename_ShouldThrowArgumentException(string text)
			{
				// Arrange.

				// Act.

				Assert.Throws<ArgumentException>(() =>
				{
					OverridingFilename.Parse(text);
				});

				// Assert.
			}

			private static class TestSource
			{
				public static IEnumerable<TestCaseData> InvalidCases
				{
					get
					{
						yield return new TestCaseData("-CSP-BADGE")
							.SetName("CASE 01: Missing Enterprise and Database Code");

						yield return new TestCaseData("ENTDBC--BADGE")
							.SetName("CASE 02: Missing CSP");

						yield return new TestCaseData("ENTDBC-CSP-")
							.SetName("CASE 03: Missing Badge");

						yield return new TestCaseData("ENTDBC-CSP-BADGE-XYZ")
							.SetName("CASE 04: More than 3 values");

					}
				}
			}
		}
	}
}
