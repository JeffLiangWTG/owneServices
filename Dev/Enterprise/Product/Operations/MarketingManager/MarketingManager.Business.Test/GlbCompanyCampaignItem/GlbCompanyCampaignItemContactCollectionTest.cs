using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignItemContactCollection))]
	sealed class GlbCompanyCampaignItemContactCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbCompanyCampaignItemContactCollection>
	{
		protected override GlbCompanyCampaignItemContactCollection GetCollectionToTest()
		{
			var contact = Factory.New<OrgContact>();
			return new GlbCompanyCampaignItemContactCollection(Factory, contact);
		}

		public void TestFKSchemaColumnInDependent()
		{
			var contact = Factory.New<OrgContact>();
			var campaignItem = (BusinessObject)Factory.New<GlbCompanyCampaignItem>();
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID] = contact.PK;

			var collection = new GlbCompanyCampaignItemContactCollection(Factory, contact);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(campaignItem, collection);
		}

		public void TestLoadFromNaturalKey()
		{
			var contact = Factory.New<OrgContact>();
			var applicant = (BusinessObject)Factory.New<IHRJobApplicant>();
			contact[OrgContact.Schema.OC_Email] = "fishy@dodgybros.com";
			applicant[HRJobApplicantSchema.HA_EmailAddress] = "fishy@dodgybros.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();

			campaignItem.G8_RecipientID = applicant.PK;
			campaignItem.G8_RecipientTableCode = applicant.TablePrefix;

			var collection = new GlbCompanyCampaignItemContactCollectionForTest(Factory, contact);
			var item = (BusinessObject)collection.AddNew();

			var expectedSQLLiteral = string.Format("G8_RecipientID = '{0}' \r\nOR\r\nG8_RecipientID = '{1}'\r\n", contact.PK, applicant.PK);
			var query = collection.ExposedCreateRelationshipFilter();
			AssertEquals(expectedSQLLiteral, query.LiteralTextSqlFormatted);

			var campaignItemResult = Factory.Load<GlbCompanyCampaignItem>(query);
			AssertContainsExactElementsInAnyOrder(new[] { contact.PK, applicant.PK }, campaignItemResult.Select(i => i.G8_RecipientID));
		}

		public void TestSetDefaultsForNewChild()
		{
			var contact = Factory.New<OrgContact>();
			var collection = new GlbCompanyCampaignItemContactCollection(Factory, contact);
			var item = (BusinessObject)collection.AddNew();
			AssertEquals(OrgContactSchema.Constants.Prefix, item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode]);
			AssertEquals(contact.PK, item[GlbCompanyCampaignItemSchema.G8_RecipientID]);
		}

		public void TestLoad_WithLearningCentreTest()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_Email = "sam@test.com";
			var campaignItem1 = (BusinessObject)Factory.New<GlbCompanyCampaignItem>();
			campaignItem1[GlbCompanyCampaignItemSchema.G8_RecipientID] = contact.PK;
			campaignItem1[GlbCompanyCampaignItemSchema.G8_RecipientTableCode] = contact.TablePrefix;

			var applicant = (BusinessObject)Factory.New<IHRJobApplicant>();
			applicant[HRJobApplicantSchema.HA_EmailAddress] = "sam@test.com";
			var campaignItem2 = (BusinessObject)Factory.New<GlbCompanyCampaignItem>();
			campaignItem2[GlbCompanyCampaignItemSchema.G8_RecipientID] = applicant.PK;
			campaignItem2[GlbCompanyCampaignItemSchema.G8_RecipientTableCode] = applicant.TablePrefix;

			var collection = new GlbCompanyCampaignItemContactCollection(Factory, contact);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(campaignItem1, collection);
			AssertCollectionContains(campaignItem2, collection);
			AssertEquals(contact.PK, campaignItem1[GlbCompanyCampaignItemSchema.G8_RecipientID]);
			AssertEquals(applicant.PK, campaignItem2[GlbCompanyCampaignItemSchema.G8_RecipientID]);

			var contact2 = Factory.New<OrgContact>();
			contact2.OC_Email = "sam@test.com";
			collection = new GlbCompanyCampaignItemContactCollection(Factory, contact2);
			AssertEquals("Should contain only the LearningCentreCampaignItem", 1, collection.Count);
			AssertCollectionContains(campaignItem2, collection);
			AssertCollectionNotContains(campaignItem1, collection);
			AssertEquals(applicant.PK, campaignItem2[GlbCompanyCampaignItemSchema.G8_RecipientID]);
		}

		public void TestHRJobApplicant()
		{
			var jobApplicant = (BusinessObject)Factory.New<IHRJobApplicant>();
			jobApplicant[HRJobApplicantSchema.HA_EmailAddress] = "sam@test.com";

			var campaignItem1 = Factory.New<GlbCompanyCampaignItem>();
			campaignItem1.G8_RecipientID = jobApplicant.PK;
			campaignItem1.G8_RecipientTableCode = jobApplicant.TablePrefix;

			var collection = new GlbCompanyCampaignItemContactCollection(Factory, (IHRJobApplicant)jobApplicant);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(campaignItem1, collection);
			AssertEquals(jobApplicant.PK, campaignItem1.G8_RecipientID);

			collection.DeleteAll();
			AssertEquals(true, campaignItem1.IsDeleted);
		}

		public void TestGlbStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "sam@test.com";

			var campaignItem1 = Factory.New<GlbCompanyCampaignItem>();
			campaignItem1.G8_RecipientID = staff.PK;
			campaignItem1.G8_RecipientTableCode = staff.TablePrefix;

			var collection = new GlbCompanyCampaignItemContactCollection(Factory, staff);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(campaignItem1, collection);
			AssertEquals(staff.PK, campaignItem1.G8_RecipientID);

			collection.DeleteAll();
			AssertEquals(true, campaignItem1.IsDeleted);
		}

		public void TestDeleteAll()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_Email = "sam@test.com";

			var campaignItem1 = Factory.New<GlbCompanyCampaignItem>();
			campaignItem1.G8_RecipientID = contact.PK;
			campaignItem1.G8_RecipientTableCode = contact.TablePrefix;

			var applicant = (BusinessObject)Factory.New<IHRJobApplicant>();
			applicant[HRJobApplicantSchema.HA_EmailAddress] = "sam@test.com";
			var campaignItem2 = Factory.New<GlbCompanyCampaignItem>();
			campaignItem2.G8_RecipientID = applicant.PK;
			campaignItem2.G8_RecipientTableCode = applicant.TablePrefix;

			var collection = new GlbCompanyCampaignItemContactCollection(Factory, contact);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(campaignItem1, collection);
			AssertCollectionContains(campaignItem2, collection);
			AssertEquals(contact.PK, campaignItem1.G8_RecipientID);
			AssertEquals(applicant.PK, campaignItem2.G8_RecipientID);

			collection.DeleteAll();
			AssertEquals(true, campaignItem1.IsDeleted);
			AssertEquals(false, campaignItem2.IsDeleted);
			AssertEquals(applicant.PK, campaignItem2.G8_RecipientID);
		}
	}
}
