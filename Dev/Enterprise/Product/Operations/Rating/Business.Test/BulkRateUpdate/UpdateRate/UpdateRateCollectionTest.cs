using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(UpdateRateCollection))]
	internal sealed class UpdateRateCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UpdateRateCollection>
	{
		public void TestLoad()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader(1, 1, 1, 1));
			var rateEntry1a = rate1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var rateEntry1b = rate1.AddRateEntry("LCL", "LCL", "AUSYD", "GBLON");

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader(1, 1, 0, 0));
			var rateEntry2a = rate2.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			var rateEntry2b = rate2.AddRateEntry("ORG", "LCL", "AUSYD", "");

			var rate3 = Helper.NewClientRate(Helper.NewOrgHeader(1, 0, 1, 1));
			var rateEntry3a = rate3.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			var rateEntry3b = rate3.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			var rateEntry3c = rate3.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "20GP");

			var rate4 = Helper.NewClientRate(Helper.NewOrgHeader(1, 0, 1, 0));
			var rateEntry4a = rate4.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP");

			var rate5 = Helper.NewClientRate(Helper.NewOrgHeader(0));
			var rateEntry5a = rate5.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var companyTariff1 = Factory.New<CompanyTariff>();
			companyTariff1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Factory.Save();

			UpdateRateEntryLastEditTime(rateEntry1a);
			UpdateRateEntryLastEditTime(rateEntry1b);
			UpdateRateEntryLastEditTime(rateEntry2a);
			UpdateRateEntryLastEditTime(rateEntry2b);
			UpdateRateEntryLastEditTime(rateEntry3a);
			UpdateRateEntryLastEditTime(rateEntry3b);
			UpdateRateEntryLastEditTime(rateEntry3c);
			UpdateRateEntryLastEditTime(rateEntry4a);
			UpdateRateEntryLastEditTime(rateEntry5a);
			UpdateRatingHeaderLastEditTime(rate1);
			UpdateRatingHeaderLastEditTime(rate2);
			UpdateRatingHeaderLastEditTime(rate3);
			UpdateRatingHeaderLastEditTime(rate4);
			UpdateRatingHeaderLastEditTime(rate5);

			var collection = new UpdateRateCollection(Factory);
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);
			AssertEquals("TESTORG1", collection[0].Client.OH_Code);
			companyTariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).RemoveAndDeleteAll();

			companyTariff1.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);
			AssertEquals("TESTORG3", collection[0].Client.OH_Code);
			companyTariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).RemoveAndDeleteAll();

			companyTariff1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);
			AssertEquals("TESTORG4", collection[0].Client.OH_Code);
			companyTariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.FCL).RemoveAndDeleteAll();

			companyTariff1.AddRateEntry("ORG", "LCL", "AUSYD", "");
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(2, collection.Count);
			Assert(collection.Contains("TESTORG1"));
			Assert(collection.Contains("TESTORG2"));
			companyTariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).RemoveAndDeleteAll();

			companyTariff1.AddRateEntry("DST", "SEA", "", "GBLON");
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(2, collection.Count);
			Assert(collection.Contains("TESTORG1"));
			Assert(collection.Contains("TESTORG3"));
			companyTariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST).RemoveAndDeleteAll();

			companyTariff1.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX");
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(2, collection.Count);
			Assert(collection.Contains("TESTORG1"));
			Assert(collection.Contains("TESTORG2"));
			companyTariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).RemoveAndDeleteAll();

			companyTariff1.AddRateEntry("LCL", "LCL", "AU", "USLAX");
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);
			AssertEquals("TESTORG3", collection[0].Client.OH_Code);
			companyTariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).RemoveAndDeleteAll();

			companyTariff1.AddRateEntry("LCL", "LCL", "AU", "GB");
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);
			AssertEquals("TESTORG1", collection[0].Client.OH_Code);
			companyTariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).RemoveAndDeleteAll();

			companyTariff1.AddRateEntry("LCL", "LCL", "AUSYD", "US");
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);
			AssertEquals("TESTORG3", collection[0].Client.OH_Code);
			companyTariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).RemoveAndDeleteAll();
		}

		public void TestLoadCostBased()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader(1, 1, 1, 1));
			var rateEntry1a = rate1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var rateEntry1b = rate1.AddRateEntry("LCL", "LCL", "AUSYD", "GBLON");

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader(1, 0, 0, 0));
			var rateEntry2a = rate2.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry2a.RateLines[0].TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			var rateEntry2b = rate2.AddRateEntry("LCL", "LCL", "AUSYD", "GBLON");

			var cost1 = Helper.NewCosting(Helper.NewOrgHeader());
			cost1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			Factory.Save();
			UpdateRateEntryLastEditTime(rateEntry1a);
			UpdateRateEntryLastEditTime(rateEntry1b);
			UpdateRatingHeaderLastEditTime(rate1);

			var collection = new UpdateRateCollection(Factory);
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);
			AssertEquals("TESTORG2", collection[0].Client.OH_Code);

			rateEntry2a.TI_OH_Supplier = Helper.NewOrgHeader().PK;
			Factory.Save();
			UpdateRateEntryLastEditTime(rateEntry2a);
			UpdateRateEntryLastEditTime(rateEntry2b);
			UpdateRatingHeaderLastEditTime(rate2);
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(0, collection.Count);

			rateEntry2a.TI_OH_TransportProvider = cost1.TH_OH;
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);
			AssertEquals("TESTORG2", collection[0].Client.OH_Code);

			rateEntry2a.TI_OH_TransportProvider = Helper.NewOrgHeader().PK;
			Factory.Save();
			UpdateRateEntryLastEditTime(rateEntry2a);
			UpdateRatingHeaderLastEditTime(rate2);
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(0, collection.Count);

			rateEntry2a.TI_OH_Supplier = cost1.TH_OH;
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);
			AssertEquals("TESTORG2", collection[0].Client.OH_Code);
			cost1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).RemoveAndDeleteAll();

			var companyTariff1 = Factory.New<CompanyTariff>();
			var companyTariffEntry1a = companyTariff1.AddRateEntry("LCL", "LCL", "AUSYD", "GBLON");
			companyTariffEntry1a.RateLines[0].TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			Factory.Save();
			UpdateRateEntryLastEditTime(companyTariffEntry1a);
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);

			cost1.AddRateEntry("LCL", "LCL", "AUSYD", "GBLON");
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(2, collection.Count);
		}

		public void TestLoadChangedRateEntries()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader(1, 1, 1, 1));
			var rateEntry1a = rate1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var rateEntry1b = rate1.AddRateEntry("LCL", "LCL", "AUSYD", "GBLON");

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader(1, 1, 0, 0));
			var rateEntry2a = rate2.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			var rate3 = Helper.NewClientRate(Helper.NewOrgHeader(1, 0, 1, 1));
			var rateEntry3a = rate3.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			Factory.Save();

			UpdateRateEntryLastEditTime(rateEntry1a);
			UpdateRateEntryLastEditTime(rateEntry1b);
			UpdateRateEntryLastEditTime(rateEntry2a);
			UpdateRatingHeaderLastEditTime(rate1);
			UpdateRatingHeaderLastEditTime(rate2);

			var collection = new UpdateRateCollection(Factory);
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(1, collection.Count);

			var rate4 = Helper.NewClientRate(Helper.NewOrgHeader(1, 1, 0, 0));
			var rateEntry4a = rate4.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			Factory.Save();
			UpdateRatingHeaderLastEditTime(rate4);
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(2, collection.Count);

			rateEntry1a.TI_DestinationLRC = "NZAKL";
			Factory.Save();
			collection.Load(ZDateTime.UtcNow.AddHours(-1));
			AssertEquals(3, collection.Count);
		}

		#region Implementation

		void UpdateRateEntryLastEditTime(RateEntry rateEntry)
		{
			var update = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'",
				RateEntrySchema.Constants.TableName,
				RateEntrySchema.TI_SystemLastEditTimeUtc.Name,
				ZDateTime.UtcNow.AddHours(-2).ToISO8601String(),
				RateEntrySchema.PK.Name,
				rateEntry.PK);
			Db.Connection.ExecuteNonQuery(update); // Need to update data in database for testing
		}

		void UpdateRatingHeaderLastEditTime(RatingHeader rate)
		{
			var update = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'",
				RatingHeaderSchema.Constants.TableName,
				RatingHeaderSchema.TH_SystemLastEditTimeUtc.Name,
				ZDateTime.UtcNow.AddHours(-2).ToISO8601String(),
				RatingHeaderSchema.PK.Name,
				rate.PK);
			Db.Connection.ExecuteNonQuery(update); // Need to update data in database for testing
		}

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		protected override UpdateRateCollection GetCollectionToTest()
		{
			return new UpdateRateCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UpdateRate(Factory) { ClientPK = Factory.New<OrgHeader>().PK };
		}

		#endregion
	}
}
