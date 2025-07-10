using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbReleaseNoteCollection))]
	sealed class GlbReleaseNoteCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbReleaseNoteCollection>
	{
		public void TestProductivityWiseShowsOnlyVisibleCategories()
		{
			var originalValue = DataRegistry.Instance.ProductivityWiseModeEnabled;
			try
			{
				DataRegistry.Instance.ProductivityWiseModeEnabled = true;
				var note1 = Factory.NewWithValidTestData<GlbReleaseNote>();
				note1.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
				note1.GF_Category = "PRD";
				var note2 = Factory.NewWithValidTestData<GlbReleaseNote>();
				note2.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
				note2.GF_Category = "ACI";
				var collection = new GlbReleaseNoteCollection(Factory);
				AssertCollectionContains(note1, collection);
				AssertCollectionNotContains(note2, collection);
			}
			finally
			{
				DataRegistry.Instance.ProductivityWiseModeEnabled = originalValue;
			}
		}

		public void TestOnlyProductUpdatesIncluded_ByDefault()
		{
			var collection = new GlbReleaseNoteCollection(Factory);

			TestCaseHelper.ClearTable(GlbReleaseNoteReadSchema.Constants.TableName);
			TestCaseHelper.ClearTable(GlbReleaseNoteSchema.Constants.TableName);

			AssertNoteExists(collection, NewsSectionTypeList.Codes.ProductUpdates, true);
			AssertNoteExists(collection, NewsSectionTypeList.Codes.WiseNews, false);
			AssertNoteExists(collection, NewsSectionTypeList.Codes.WiseLearningUpdates, false);
			AssertNoteExists(collection, NewsSectionTypeList.Codes.ClientAnnouncements, false);
			AssertNoteExists(collection, NewsSectionTypeList.Codes.ClientNews, false);
			AssertNoteExists(collection, NewsSectionTypeList.Codes.ClientStaffNews, false);
			AssertNoteExists(collection, "XYZ", false);
		}

		public void TestOtherSectionsIncludedIfSpecified()
		{
			var collection = new GlbReleaseNoteCollection(Factory, NewsSectionTypeList.Codes.WiseNews, NewsSectionTypeList.Codes.WiseLearningUpdates);

			TestCaseHelper.ClearTable(GlbReleaseNoteReadSchema.Constants.TableName);
			TestCaseHelper.ClearTable(GlbReleaseNoteSchema.Constants.TableName);

			AssertNoteExists(collection, NewsSectionTypeList.Codes.ProductUpdates, true);
			AssertNoteExists(collection, NewsSectionTypeList.Codes.WiseNews, true);
			AssertNoteExists(collection, NewsSectionTypeList.Codes.WiseLearningUpdates, true);
			AssertNoteExists(collection, NewsSectionTypeList.Codes.ClientAnnouncements, false);
			AssertNoteExists(collection, NewsSectionTypeList.Codes.ClientNews, false);
			AssertNoteExists(collection, NewsSectionTypeList.Codes.ClientStaffNews, false);
			AssertNoteExists(collection, "XYZ", false);
		}

		void AssertNoteExists(GlbReleaseNoteCollection collection, string section, bool expectExists)
		{
			var note = Factory.New<GlbReleaseNote>();
			note.GF_Section = section;
			if (expectExists)
			{
				AssertCollectionContains(note, collection);
			}
			else
			{
				AssertCollectionNotContains(note, collection);
			}
		}

		protected override GlbReleaseNoteCollection GetCollectionToTest()
		{
			return new GlbReleaseNoteCollection(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(GlbReleaseNoteReadSchema.Constants.TableName);
			TestCaseHelper.ClearTable(GlbReleaseNoteSchema.Constants.TableName);
		}
	}
}
