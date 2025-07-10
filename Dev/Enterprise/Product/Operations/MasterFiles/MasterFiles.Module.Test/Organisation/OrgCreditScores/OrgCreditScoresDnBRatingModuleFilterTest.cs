using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgCreditScoresDnBRatingModuleFilter))]
	sealed class OrgCreditScoresDnBRatingModuleFilterTest : ModuleTextFilterTest
	{
		#region Empty

		public void TestIsEmpty()
		{
			var filterStripBizO = new OrganisationFilterBusinessObject();
			var dnbRatingFilter = (OrgCreditScoresDnBRatingModuleFilter)filterStripBizO[ExpectedDescription];

			AssertEquals("IsEmpty 1", true, dnbRatingFilter.IsEmpty);

			dnbRatingFilter.FinancialStrength = FinancialStrengthList.Codes.A;
			AssertEquals("IsEmpty 2", false, dnbRatingFilter.IsEmpty);

			dnbRatingFilter.FinancialStrength = ZString.Empty;
			dnbRatingFilter.CreditAppraisal = CreditAppraisalList.Codes.Good;
			AssertEquals("IsEmpty 3", false, dnbRatingFilter.IsEmpty);

			dnbRatingFilter.CreditAppraisal = ZString.Empty;
			AssertEquals("IsEmpty 4", true, dnbRatingFilter.IsEmpty);
		}

		#endregion

		#region Clear

		public void TestClear()
		{
			var filterStripBizO = new OrganisationFilterBusinessObject();
			var dnbRatingFilter = (OrgCreditScoresDnBRatingModuleFilter)filterStripBizO[ExpectedDescription];

			dnbRatingFilter.FinancialStrength = FinancialStrengthList.Codes.A;
			dnbRatingFilter.CreditAppraisal = CreditAppraisalList.Codes.Good;

			CombineAssertions("Preconditions:", () =>
			{
				AssertEquals("Financial Strength", FinancialStrengthList.Codes.A, dnbRatingFilter.FinancialStrength);
				AssertEquals("Credit Appraisal", CreditAppraisalList.Codes.Good, dnbRatingFilter.CreditAppraisal);
			});

			dnbRatingFilter.Clear();

			CombineAssertions("All fields should be empty", () =>
			{
				AssertEquals("Financial Strength", ZString.Empty, dnbRatingFilter.FinancialStrength);
				AssertEquals("Credit Appraisal", ZString.Empty, dnbRatingFilter.CreditAppraisal);
			});
		}

		#endregion

		#region Serialization

		public void TestSerialization()
		{
			#region ExpectedXML

			const string ExpectedXML = @"<Filter>
  <Comparer>starts with</Comparer>
  <Property />
  <FinancialStrength>5A</FinancialStrength>
  <CreditAppraisal>1</CreditAppraisal>
</Filter>";

			#endregion

			var filterStripBizO = new OrganisationFilterBusinessObject();
			var dnbRatingFilter = (OrgCreditScoresDnBRatingModuleFilter)filterStripBizO[ExpectedDescription];

			using (var writer = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(writer))
			{
				dnbRatingFilter.FinancialStrength = FinancialStrengthList.Codes.FiveA;
				dnbRatingFilter.CreditAppraisal = CreditAppraisalList.Codes.Strong;

				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)dnbRatingFilter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();

				AssertMultilineASCIIEquals(ExpectedXML, writer.ToString());
			}
		}

		public void TestDeserialization()
		{
			#region TestXML

			const string TestXML = @"<Filter>
 <Comparer>starts with</Comparer>
  <Property />
  <FinancialStrength>5A</FinancialStrength>
  <CreditAppraisal>1</CreditAppraisal>
</Filter>";

			#endregion

			var filterStripBizO = new OrganisationFilterBusinessObject();
			var dnbRatingFilter = (OrgCreditScoresDnBRatingModuleFilter)filterStripBizO[ExpectedDescription];

			using (var reader = new StringReader(TestXML))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)dnbRatingFilter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();
			}

			CombineAssertions("Filter properties should all be deserialized", () =>
			{
				AssertEquals("Comparer", SQLComparisonOperator.StartsWith, dnbRatingFilter.SqlComparisonOperator);
				AssertEquals("Financial Strength", "5A", dnbRatingFilter.FinancialStrength);
				AssertEquals("Credit Appraisal", "1", dnbRatingFilter.CreditAppraisal);
			});
		}

		public override void TestQueryIsEmptyByDefault()
		{
			Assert("Filter uses 'any credit scores' by default, which isn't empty", true);
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleTextFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { nameof(filter.ComparisonOperator) }).ToArray();
		}

		#endregion

		#region List

		public void TestFinancialStrengthList()
		{
			var list = new FinancialStrengthList();
			AssertEquals(14, list.Count);
			CombineAssertions(() =>
			{
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.FiveA}", list.ContainsCode(FinancialStrengthList.Codes.FiveA));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.FourA}", list.ContainsCode(FinancialStrengthList.Codes.FourA));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.ThreeA}", list.ContainsCode(FinancialStrengthList.Codes.ThreeA));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.TwoA}", list.ContainsCode(FinancialStrengthList.Codes.TwoA));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.A}", list.ContainsCode(FinancialStrengthList.Codes.A));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.B}", list.ContainsCode(FinancialStrengthList.Codes.B));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.C}", list.ContainsCode(FinancialStrengthList.Codes.C));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.D}", list.ContainsCode(FinancialStrengthList.Codes.D));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.E}", list.ContainsCode(FinancialStrengthList.Codes.E));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.F}", list.ContainsCode(FinancialStrengthList.Codes.F));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.G}", list.ContainsCode(FinancialStrengthList.Codes.G));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.O}", list.ContainsCode(FinancialStrengthList.Codes.O));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.N}", list.ContainsCode(FinancialStrengthList.Codes.N));
				Assert($"Financial Strength list contains {FinancialStrengthList.Codes.NQ}", list.ContainsCode(FinancialStrengthList.Codes.NQ));
			});
		}

		public void TestCreditAppraisalList()
		{
			var list = new CreditAppraisalList();
			AssertEquals(4, list.Count);
			CombineAssertions(() =>
			{
				Assert($"Credit Appraisal list contains {CreditAppraisalList.Codes.Strong}", list.ContainsCode(CreditAppraisalList.Codes.Strong));
				Assert($"Credit Appraisal list contains {CreditAppraisalList.Codes.Good}", list.ContainsCode(CreditAppraisalList.Codes.Good));
				Assert($"Credit Appraisal list contains {CreditAppraisalList.Codes.Fair}", list.ContainsCode(CreditAppraisalList.Codes.Fair));
				Assert($"Credit Appraisal list contains {CreditAppraisalList.Codes.Limited}", list.ContainsCode(CreditAppraisalList.Codes.Limited));
			});
		}

		#endregion

		#region Implementation

		protected override ZString ExpectedDescription => "D&B Rating";

		protected override ModuleTextFilter GetNewModuleFilter() => new OrgCreditScoresDnBRatingModuleFilter(ExpectedDescription);

		#endregion
	}
}
