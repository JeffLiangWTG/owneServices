using System;
using System.Collections.Generic;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace CargoWise.eHub.DataModel.IntegrationTests
{
	[TestFixture]
	public abstract class DataModelIntegrationTestBase
	{
		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			TestFixtureSetUpCore();
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
			TestFixtureTearDownCore();
		}

		[SetUp]
		public void SetUp()
		{
			SetUpCore();
		}

		[TearDown]
		public void TearDown()
		{
			TearDownCore();
		}

		protected virtual void TestFixtureSetUpCore()
		{
		}

		protected virtual void SetUpCore()
		{
		}

		protected virtual void TestFixtureTearDownCore()
		{
		}

		protected virtual void TearDownCore()
		{
		}

		protected void CompareEntities(Object real, Object expected, List<string> memberToIgnore = null)
		{
			memberToIgnore = memberToIgnore ?? new List<string>();
			var compareLogic = new CompareLogic()
			{
				Config = new ComparisonConfig()
				{
					MembersToIgnore = memberToIgnore,
					IgnoreObjectTypes = true
				}
			};
			var compareResult = compareLogic.Compare(real, expected);
			var diffs = compareResult.Differences;
			var sb = new StringBuilder();
			if (diffs.Count > 0)
			{
				sb.AppendLine("Comparison Failed:");
				foreach (Difference diff in diffs)
				{
					sb.AppendLine("Property name:" + diff.PropertyName);
					sb.AppendLine("Real value:" + diff.Object1Value);
					sb.AppendLine("Expected value:" + diff.Object2Value + "\n");
				}
				throw new Exception(sb.ToString());
			}
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
		}
	}
}
