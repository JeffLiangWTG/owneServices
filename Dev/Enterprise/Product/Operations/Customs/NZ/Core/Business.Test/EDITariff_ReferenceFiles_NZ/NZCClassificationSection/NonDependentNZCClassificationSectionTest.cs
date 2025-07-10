using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NonDependentNZCClassificationSectionCollection))]
	public class NonDependentNZCClassificationSectionCollectionTest : BusinessObjectCollectionTestCase
	{
		#region TestLoad

		public void TestLoadsSortsBySectionPadded()
		{
			TestCaseHelper.ClearTable(NZCClassificationSection.Schema.TableName);
			NZCClassificationSection section1 = GetNewSection();
			section1.Q1_Section = "1";

			NZCClassificationSection section2 = GetNewSection();
			section2.Q1_Section = "2";

			NZCClassificationSection section3 = GetNewSection();
			section3.Q1_Section = "3";

			NZCClassificationSection section10 = GetNewSection();
			section10.Q1_Section = "10";

			NZCClassificationSection section11 = GetNewSection();
			section11.Q1_Section = "11";

			Sections.Load();
			AssertEquals("Expected 5 sections to be loaded", 5, Sections.Count);
			AssertEquals("Expected Section 1 to be first", section1, Sections[0]);
			AssertEquals("Expected Section 2 to be first", section2, Sections[1]);
			AssertEquals("Expected Section 3 to be first", section3, Sections[2]);
			AssertEquals("Expected Section 10 to be first", section10, Sections[3]);
			AssertEquals("Expected Section 11 to be first", section11, Sections[4]);
		}

		#endregion TestLoad

		#region TestFamilyMembers

		public void TestFamilyMembers()
		{
			Sections.Load();
			Assert("PreCondition: Collection should contain at least 1 element", Sections.Count > 0);
			AssertEquals("FamilyMember count differs to expected", Sections.Count, Sections.FamilyMembers.Length);
		}

		#endregion TestFamilyMembers

		#region TestGetNearestNodeForCode

		public void TestGetNearestNodeForCodeReturnsClassification()
		{
			ITraversibleNode node = null;
			node = Sections.GetNearestNodeForCode("1");
			Assert(typeof(NZCClassificationChapter).IsInstanceOfType(node));
			AssertEquals("1", ((NZCClassificationChapter)node).Q2_Chapter);
			AssertEquals("Live animals", ((NZCClassificationChapter)node).Q2_Description);

			node = Sections.GetNearestNodeForCode("0101");
			Assert(typeof(NZCClassification).IsInstanceOfType(node));
			AssertEquals("0101.10.00", ((NZCClassification)node).U0_Tariff);
			Assert("Checking if we got the correct Description. Obviously Not.", ((NZCClassification)node).U0_Description.Contains("Pure-bred breeding animal"));

			node = Sections.GetNearestNodeForCode("0101.10.00");
			Assert(typeof(NZCClassification).IsInstanceOfType(node));
			AssertEquals("0101.10.00", ((NZCClassification)node).U0_Tariff);
			Assert("Checking if we got the correct Description. Obviously Not.", ((NZCClassification)node).U0_Description.Contains("Pure-bred breeding animal"));

			node = Sections.GetNearestNodeForCode("0101.10.00.1");
			Assert(typeof(NZCClassification).IsInstanceOfType(node));
			AssertEquals("0101.10.00.11E", ((NZCClassification)node).U0_Tariff);
			if (!((NZCClassification)node).U0_DateActiveTo.IsEmpty)
			{
				Assert(ZDateTime.Today.CompareTo(((NZCClassification)node).U0_DateActiveTo) < 0);
			}
			Assert("Checking if we got the correct Description. Obviously Not.", ((NZCClassification)node).U0_Description.Contains("Stallion"));

			node = Sections.GetNearestNodeForCode("0101.10.00.13E");
			Assert(typeof(NZCClassification).IsInstanceOfType(node));
			AssertEquals("0101.10.00.13A", ((NZCClassification)node).U0_Tariff);
			if (!((NZCClassification)node).U0_DateActiveTo.IsEmpty)
			{
				Assert(ZDateTime.Today.CompareTo(((NZCClassification)node).U0_DateActiveTo) < 0);
			}
			Assert("Checking if we got the correct Description. Obviously Not.", ((NZCClassification)node).U0_Description.Contains("Mare"));

			node = Sections.GetNearestNodeForCode("0101.10.00.70A");
			Assert(typeof(NZCClassification).IsInstanceOfType(node));
			AssertEquals("0101.10.00", ((NZCClassification)node).U0_Tariff);
			if (!((NZCClassification)node).U0_DateActiveTo.IsEmpty)
			{
				Assert(ZDateTime.Today.CompareTo(((NZCClassification)node).U0_DateActiveTo) < 0);
			}
			Assert("Checking if we got the correct Description. Obviously Not.", ((NZCClassification)node).U0_Description.Contains("Pure-bred breeding animal"));

			node = Sections.GetNearestNodeForCode("XX");
			AssertNull(node);

			node = Sections.GetNearestNodeForCode("XXXXX");
			AssertNull(node);

			node = Sections.GetNearestNodeForCode("XXXX.XX.XX.XX");
			AssertNull(node);
		}

		public void TestGetNearestNodeForCodeReturnsChapter()
		{
			ITraversibleNode node = null;

			node = Sections.GetNearestNodeForCode("1");
			AssertNotNull(node);
			Assert(typeof(NZCClassificationChapter).IsInstanceOfType(node));
			AssertEquals("1", ((NZCClassificationChapter)node).Q2_Chapter);

			node = Sections.GetNearestNodeForCode("10");
			AssertNotNull(node);
			Assert(typeof(NZCClassificationChapter).IsInstanceOfType(node));
			AssertEquals("10", ((NZCClassificationChapter)node).Q2_Chapter);
		}

		#endregion TestGetNearestNodeForCode

		#region Helpers

		protected NZCClassificationSection GetNewSection()
		{
			return (NZCClassificationSection)GetNewElementToAddToTheCollection();
		}

		protected NonDependentNZCClassificationSectionCollection Sections
		{
			get
			{
				if (fSections == null)
				{
					fSections = (NonDependentNZCClassificationSectionCollection)GetCollectionToTest();
				}
				return fSections;
			}
		}
		NonDependentNZCClassificationSectionCollection fSections;

		#endregion Helpers

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<NZCClassificationSection>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NonDependentNZCClassificationSectionCollection(Factory);
		}

		#endregion Implementation
	}
}
