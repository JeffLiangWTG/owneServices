using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NewsAnnouncementCollection))]
	sealed class NewsAnnouncementCollectionTest : ActiveBusinessObjectCollectionTestCase<NewsAnnouncementCollection>
	{
		public void TestOnlyCustomerSectionsIncluded()
		{
			AssertNoteExists(NewsSectionTypeList.Codes.ProductUpdates, false);
			AssertNoteExists(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes, false);
			AssertNoteExists(NewsSectionTypeList.Codes.WiseNews, false);
			AssertNoteExists(NewsSectionTypeList.Codes.WiseLearningUpdates, false);
			AssertNoteExists(NewsSectionTypeList.Codes.BorderWise, false);
			AssertNoteExists(NewsSectionTypeList.Codes.WiseTechAcademy, false);
			AssertNoteExists(NewsSectionTypeList.Codes.ClientAnnouncements, true);
			AssertNoteExists(NewsSectionTypeList.Codes.ClientNews, true);
			AssertNoteExists(NewsSectionTypeList.Codes.ClientStaffNews, true);
			AssertNoteExists("XYZ", true);
		}

		public void TestWiseTechOnlyNewsSectionTypesAreExcluded()
		{
			var excludedNewsSections = NewsAnnouncementCollection.ExcludedNewsSectionTypes;
			var wtgOnlySections = NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.WiseTechOnly);

			var missingSections = wtgOnlySections.GetAllCodes().Except(excludedNewsSections);
			Assert($"The following section(s) are missing from NewsAnnouncementCollection.ExcludedNewsSectionTypes: {string.Join(", ", missingSections)}", !missingSections.Any());
		}

		void AssertNoteExists(string section, bool expectExists)
		{
			var note = Factory.New<NewsAnnouncement>();
			note.GF_Section = section;
			if (expectExists)
			{
				AssertCollectionContains(note, new NewsAnnouncementCollection(Factory));
			}
			else
			{
				AssertCollectionNotContains(note, new NewsAnnouncementCollection(Factory));
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var item = Factory.New<NewsAnnouncement>();
			item.GF_Section = NewsSectionTypeList.Codes.ClientAnnouncements;
			return item;
		}
	}
}
