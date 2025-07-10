
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignSenderPool))]
	sealed class GlbCompanyCampaignSenderPoolTest : ActiveBusinessObjectCollectionTestCase<GlbCompanyCampaignSenderPool>
	{
		protected override GlbCompanyCampaignSenderPool GetCollectionToTest()
		{
			return new GlbCompanyCampaignSenderPool(Factory.NewWithValidTestData<GlbCompanyCampaign>());
		}

		public void TestAddNewSenders()
		{
			var header = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var subscription1 = header.SenderPool.AddNew();
			AssertEquals(header.PK, subscription1.GCP_G0_Campaign);
			var subscription2 = header.SenderPool.AddNew();
			AssertEquals(header.PK, subscription2.GCP_G0_Campaign);
			AssertEquals(2, header.SenderPool.Count);
		}

		public void TestDeleteAll()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var poolItem1 = campaign.SenderPool.AddNew();
			var glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff1.GS_EmailAddress = $"{nameof(glbStaff1)}@ema.il";
			poolItem1.GCP_GS_NKSender = glbStaff1.GS_Code;
			AssertEquals(campaign.PK, poolItem1.GCP_G0_Campaign);

			var poolItem2 = campaign.SenderPool.AddNew();
			var glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff2.GS_EmailAddress = $"{nameof(glbStaff2)}@ema.il";
			poolItem2.GCP_GS_NKSender = glbStaff2.GS_Code;
			AssertEquals(campaign.PK, poolItem2.GCP_G0_Campaign);

			var poolItem3 = Factory.New<GlbCompanyCampaignSenderPoolItem>();

			AssertEquals(2, campaign.SenderPool.Count);
			AssertCollectionContains(poolItem1, campaign.SenderPool);
			AssertCollectionContains(poolItem2, campaign.SenderPool);

			campaign.SenderPool.DeleteAll();
			AssertEquals(true, poolItem1.IsDeleted);
			AssertEquals(true, poolItem2.IsDeleted);
			AssertEquals(false, poolItem3.IsDeleted);
		}

		public void TestLoadExistingRecords()
		{
			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var poolItem1 = campaign1.SenderPool.AddNew();
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "a1@ema.il";
			poolItem1.GCP_GS_NKSender = glbStaff.GS_Code;
			AssertEquals(campaign1.PK, poolItem1.GCP_G0_Campaign);

			var poolItem2 = campaign1.SenderPool.AddNew();
			glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "a2@ema.il";
			poolItem2.GCP_GS_NKSender = glbStaff.GS_Code;
			AssertEquals(campaign1.PK, poolItem2.GCP_G0_Campaign);

			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var poolItem3 = campaign2.SenderPool.AddNew();

			glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "a3@ema.il";
			poolItem3.GCP_GS_NKSender = glbStaff.GS_Code;
			AssertEquals(campaign2.PK, poolItem3.GCP_G0_Campaign);

			Factory.Save();

			var campaign = Factory.Load<GlbCompanyCampaign>(campaign1.PK);
			AssertEquals(2, campaign.SenderPool.Count);
			AssertCollectionContains(poolItem1, campaign.SenderPool);
			AssertCollectionContains(poolItem2, campaign.SenderPool);

			campaign.Delete();
			AssertEquals(true, poolItem1.IsDeleted);
			AssertEquals(true, poolItem2.IsDeleted);
			AssertEquals(false, poolItem3.IsDeleted);

			campaign = Factory.Load<GlbCompanyCampaign>(campaign2.PK);
			AssertEquals(1, campaign.SenderPool.Count);
			AssertCollectionContains(poolItem3, campaign.SenderPool);

			campaign.SenderPool.DeleteAll();
			AssertEquals(true, poolItem1.IsDeleted);
			AssertEquals(true, poolItem2.IsDeleted);
			AssertEquals(true, poolItem3.IsDeleted);
		}

		public void TestValidateAllUniqueSenders()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			var poolItem1 = campaign.SenderPool.AddNew();
			poolItem1.GCP_GS_NKSender = glbStaff1.GS_Code;
			poolItem1.GCP_SendRatio = 1;

			var poolItem2 = campaign.SenderPool.AddNew();
			poolItem2.GCP_GS_NKSender = glbStaff2.GS_Code;
			poolItem2.GCP_SendRatio = 1;

			var poolItem3 = campaign.SenderPool.AddNew();
			poolItem3.GCP_GS_NKSender = glbStaff1.GS_Code;
			poolItem3.GCP_SendRatio = 1;

			AssertNoRowErrors("Not validated, should not have errors", poolItem1);
			AssertNoRowErrors("Not validated, should not have errors", poolItem2);
			AssertNoRowErrors("Not validated, should not have errors", poolItem3);

			AssertEquals(3, campaign.SenderPool.Count);
			campaign.SenderPool.ValidateAll();

			AssertHasRowError("Sender non unique, should have errors", poolItem1, "Each staff member should be unique in Sender Pool.");
			AssertNoRowErrors("Campaign Category is valid, should not have errors", poolItem2);
			AssertHasRowError("Sender non unique, should have errors", poolItem3, "Each staff member should be unique in Sender Pool.");

			poolItem3.Delete();

			AssertEquals(2, campaign.SenderPool.Count);
			campaign.SenderPool.ValidateAll();

			AssertNoRowErrors("Campaign Category is valid, should not have errors", poolItem1);
			AssertNoRowErrors("Campaign Category is valid, should not have errors", poolItem2);
			AssertEquals(true, poolItem3.IsDeleted);
		}

		public void TestValidateAllTotalSendRatio()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			var poolItem1 = campaign.SenderPool.AddNew();
			poolItem1.GCP_GS_NKSender = glbStaff1.GS_Code;

			var poolItem2 = campaign.SenderPool.AddNew();
			poolItem2.GCP_GS_NKSender = glbStaff2.GS_Code;

			AssertNoRowErrors("Not validated, should not have errors", poolItem1);
			AssertNoRowErrors("Not validated, should not have errors", poolItem2);

			AssertEquals(2, campaign.SenderPool.Count);
			campaign.SenderPool.ValidateAll();

			AssertNoRowErrors("Campaign Category is valid, should not have errors", poolItem1);
			AssertNoRowErrors("Campaign Category is valid, should not have errors", poolItem2);
		}

		public void TestValidateAllForNotValidatedElements()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var poolItem = campaign.SenderPool.AddNew();
			AssertNoRowErrors("Not validated, should not have errors", poolItem);
			AssertEquals("Not validated, should not have errors", false, campaign.SenderPool.HasErrors());

			campaign.SenderPool.ValidateAll();
			AssertEquals("Broken pool, should have errors", true, campaign.SenderPool.HasErrors());
		}

		public void TestPercentageCalculation()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			var poolItem1 = campaign.SenderPool.AddNew();
			AssertEquals("The only one row", "100", poolItem1.SendRatioPercentage);
			poolItem1.GCP_GS_NKSender = glbStaff1.GS_Code;
			AssertEquals("The only one row", "100", poolItem1.SendRatioPercentage);

			var poolItem2 = campaign.SenderPool.AddNew();
			AssertEquals("Two equals rows", "50", poolItem1.SendRatioPercentage);
			AssertEquals("Two equals rows", "50", poolItem2.SendRatioPercentage);
			poolItem2.GCP_GS_NKSender = glbStaff2.GS_Code;
			AssertEquals("Two equals rows", "50", poolItem1.SendRatioPercentage);
			AssertEquals("Two equals rows", "50", poolItem2.SendRatioPercentage);

			poolItem1.GCP_SendRatio = 3;
			AssertEquals("75%", "75", poolItem1.SendRatioPercentage);
			AssertEquals("25%", "25", poolItem2.SendRatioPercentage);

			poolItem1.GCP_SendRatio = 32000;
			AssertEquals("99%", "99", poolItem1.SendRatioPercentage);
			AssertEquals("Less than 1%", "< 1", poolItem2.SendRatioPercentage);
		}

		public void TestPercentageCalculationOnLoad()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			var poolItem = campaign.SenderPool.AddNew();
			poolItem.GCP_GS_NKSender = glbStaff1.GS_Code;
			poolItem.GCP_SendRatio = 3;

			poolItem = campaign.SenderPool.AddNew();
			poolItem.GCP_GS_NKSender = glbStaff2.GS_Code;

			Factory.Save();

			campaign = new BusinessObjectFactory().Load<GlbCompanyCampaign>(campaign.PK);
			AssertEquals("Pool size", 2, campaign.SenderPool.Count);
			var items = campaign.SenderPool.OrderByDescending(item => item.GCP_SendRatio).ToArray();
			AssertEquals("75%", "75", items[0].SendRatioPercentage);
			AssertEquals("25%", "25", items[1].SendRatioPercentage);

			items[0].GCP_SendRatio = 1;
			items[1].GCP_SendRatio = 3;
			AssertEquals("25%", "25", items[0].SendRatioPercentage);
			AssertEquals("75%", "75", items[1].SendRatioPercentage);
		}

		public void TestHasChangesWhenSendRatioPercentageChanges()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			var glbStaff3 = Factory.NewWithValidTestData<GlbStaff>();

			var poolItem1 = campaign.SenderPool.AddNew();
			poolItem1.GCP_GS_NKSender = glbStaff1.GS_Code;

			var poolItem2 = campaign.SenderPool.AddNew();
			poolItem2.GCP_GS_NKSender = glbStaff2.GS_Code;

			Factory.Save();

			AssertEquals("No changes", false, campaign.HasChanges);
			AssertEquals("No changes", false, poolItem1.HasChanges);
			AssertEquals("No changes", false, poolItem2.HasChanges);

			var poolItem3 = campaign.SenderPool.AddNew();
			poolItem3.GCP_GS_NKSender = glbStaff3.GS_Code;

			AssertEquals("Has changes", true, campaign.HasChanges);
			AssertEquals("No changes", false, poolItem1.HasChanges);
			AssertEquals("No changes", false, poolItem2.HasChanges);
			AssertEquals("Has changes", true, poolItem3.HasChanges);

			Factory.Save();

			AssertEquals("No changes", false, campaign.HasChanges);
			AssertEquals("No changes", false, poolItem1.HasChanges);
			AssertEquals("No changes", false, poolItem2.HasChanges);
			AssertEquals("No changes", false, poolItem3.HasChanges);

			poolItem1.GCP_SendRatio = 8;
			AssertEquals("Has changes", true, campaign.HasChanges);
			AssertEquals("Has changes", true, poolItem1.HasChanges);
			AssertEquals("No changes", false, poolItem2.HasChanges);
			AssertEquals("No changes", false, poolItem3.HasChanges);
		}
	}
}
