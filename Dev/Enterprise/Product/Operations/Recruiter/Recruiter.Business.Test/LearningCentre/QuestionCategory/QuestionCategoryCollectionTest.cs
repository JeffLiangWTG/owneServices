using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(QuestionCategoryCollection))]
	sealed class QuestionCategoryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<QuestionCategoryCollection>
	{
		public void TestDefaultElement()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategoryCollection collection = new QuestionCategoryCollection(campaign);
			AssertEquals(0, collection.Count);

			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals(QuestionCategory.DefaultCode, collection[0].Code);
			AssertEquals(QuestionCategory.DefaultDescription, collection[0].Description);
		}

		public void TestSerialiseDeserialise()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategoryCollection collection = new QuestionCategoryCollection(campaign);
			QuestionCategory category1 = collection.AddNew();
			category1.Code = "C1";
			category1.Description = "Category 1";
			QuestionCategory category2 = collection.AddNew();
			category2.Code = "C2";
			category2.Description = "Category 2";

			StringBuilder builder = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(builder))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("root");
				((IXmlSerializable)collection).WriteXml(writer);
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

			QuestionCategoryCollection newCollection = new QuestionCategoryCollection(campaign);
			using (StringReader stringReader = new StringReader(builder.ToString()))
			using (XmlReader reader = XmlReader.Create(stringReader))
			{
				((IXmlSerializable)newCollection).ReadXml(reader);
			}
			AssertEquals(2, newCollection.Count);
			newCollection.Sort("Code");
			AssertEquals("C1", newCollection[0].Code);
			AssertEquals("Category 1", newCollection[0].Description);
			AssertEquals("C2", newCollection[1].Code);
			AssertEquals("Category 2", newCollection[1].Description);
		}

		public void TestSaveAndLoad()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategoryCollection collection = new QuestionCategoryCollection(campaign);
			QuestionCategory category1 = collection.AddNew();
			category1.Code = "C1";
			category1.Description = "Category 1";
			QuestionCategory category2 = collection.AddNew();
			category2.Code = "C2";
			category2.Description = "Category 2";
			collection.Save();

			QuestionCategoryCollection newCollection = new QuestionCategoryCollection(campaign);
			AssertEquals(0, newCollection.Count);

			newCollection.Load();
			AssertEquals(2, newCollection.Count);
			newCollection.Sort("Code");
			AssertEquals("C1", newCollection[0].Code);
			AssertEquals("Category 1", newCollection[0].Description);
			AssertEquals("C2", newCollection[1].Code);
			AssertEquals("Category 2", newCollection[1].Description);
		}

		public void TestDeleteAndSave()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategoryCollection collection = new QuestionCategoryCollection(campaign);
			QuestionCategory category1 = collection.AddNew();
			category1.Code = "C1";
			category1.Description = "Category 1";
			QuestionCategory category2 = collection.AddNew();
			category2.Code = "C2";
			category2.Description = "Category 2";
			collection.Save();

			collection.RemoveAndDelete(category1);
			Assert("Should have changes", collection.HasChanges);
			AssertEquals(1, collection.Count);
			collection.Save();

			QuestionCategoryCollection newCollection = new QuestionCategoryCollection(campaign);
			newCollection.Load();
			AssertEquals(1, newCollection.Count);
		}

		public void TestAddNewWithParams()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			campaign.QuestionCategories.RemoveAndDeleteAll();
			campaign.QuestionCategories.AddNew("MEH", "New Description");

			AssertEquals(1, campaign.QuestionCategories.Count);
			AssertEquals("MEH", campaign.QuestionCategories[0].Code);
			AssertEquals("New Description", campaign.QuestionCategories[0].Description);
		}

		#region Implementation		

		protected override QuestionCategoryCollection GetCollectionToTest()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return new QuestionCategoryCollection(campaign);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new QuestionCategory(Factory.New<LearningCentreCampaign>());
		}

		#endregion
	}
}
