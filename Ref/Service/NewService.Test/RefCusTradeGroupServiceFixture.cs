using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusTradeGroupServiceFixture
	{
		static string TblPrefix => "ZZA";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusTradeGroup);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusTradeGroup { ZZA_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZA_TradeGroup = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZA_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusTradeGroup { ZZA_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZA_TradeGroup = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZA_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZA_TradeGroup).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Agreement_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateAgreement(Now, "AA");
			var p2 = CreateAgreement(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZA_TradeGroup).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_TradeAgreementCountry_Data()
		{
			var p = CreateAgreement(Now.AddDays(1));
			var c = repo.Create(() => new RefCusTradeGroupCountry
			{
				ZZB_StartDate = Now,
				ZZB_EndDate = Now.AddDays(1),
				ZZB_RN_NKTradeGroupCountryCode = "AU",
				ZZB_ZZA_TradeGroup = p.ZZA_PK
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusTradeGroupCountries.ElementAt(0);
			Assert.That(result.ZZB_StartDate, Is.EqualTo(c.ZZB_StartDate));
			Assert.That(result.ZZB_EndDate, Is.EqualTo(c.ZZB_EndDate));
			Assert.That(result.ZZB_RN_NKTradeGroupCountryCode, Is.EqualTo(c.ZZB_RN_NKTradeGroupCountryCode));
		}

		[Test]
		public void GetLatest_TradeAgreementLanguage_Data()
		{
			var p = CreateAgreement(Now.AddDays(1));
			var c = repo.Create(() => new RefCusTradeGroupLanguage
			{
				ZXD_Description = "CC",
				ZXD_ZX6_NKLanguage = "EN",
				ZXD_ZZA_TradeGroup = p.ZZA_PK
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusTradeGroupLanguages.ElementAt(0);
			Assert.That(result.ZXD_ZX6_NKLanguage, Is.EqualTo(c.ZXD_ZX6_NKLanguage));
			Assert.That(result.ZXD_Description, Is.EqualTo(c.ZXD_Description));
		}

		[Test]
		public void GetLatest_TradeAgreementCountriesAndLanguages_Count()
		{
			var p = CreateAgreement(Now.AddDays(1));
			var country1 = repo.Create(() => new RefCusTradeGroupCountry
			{
				ZZB_PK = Guid.NewGuid(),
				ZZB_StartDate = Now,
				ZZB_EndDate = Now.AddDays(1),
				ZZB_RN_NKTradeGroupCountryCode = "AU",
				ZZB_ZZA_TradeGroup = p.ZZA_PK
			});
			var country2 = repo.Create(() => new RefCusTradeGroupCountry
			{
				ZZB_PK = Guid.NewGuid(),
				ZZB_StartDate = Now.AddDays(1),
				ZZB_EndDate = Now.AddDays(2),
				ZZB_RN_NKTradeGroupCountryCode = "AU",
				ZZB_ZZA_TradeGroup = p.ZZA_PK
			});
			var language = repo.Create(() => new RefCusTradeGroupLanguage
			{
				ZXD_PK = Guid.NewGuid(),
				ZXD_Description = "CC1",
				ZXD_ZX6_NKLanguage = "EN",
				ZXD_ZZA_TradeGroup = p.ZZA_PK
			});
			var language2 = repo.Create(() => new RefCusTradeGroupLanguage
			{
				ZXD_PK = Guid.NewGuid(),
				ZXD_Description = "CC2",
				ZXD_ZX6_NKLanguage = "FR",
				ZXD_ZZA_TradeGroup = p.ZZA_PK
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.AreEqual(2, result.RefCusTradeGroupCountries.Length);
			Assert.AreEqual(2, result.RefCusTradeGroupLanguages.Length);
		}

		[Test]
		public void GetLatest_TradeAgreement_Data()
		{
			var p = repo.Create(() => new RefCusTradeGroup
			{
				ZZA_PK = Guid.NewGuid(),
				ZZA_Description = "AA",
				ZZA_StartDate = Now,
				ZZA_EndDate = Now.AddDays(1),
				ZZA_ZZZ_NKDataGrouping = "AU",
				ZZA_TradeGroup = "BB",
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZZA_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZA_Description, Is.EqualTo(p.ZZA_Description));
			Assert.That(result.ZZA_StartDate, Is.EqualTo(p.ZZA_StartDate));
			Assert.That(result.ZZA_EndDate, Is.EqualTo(p.ZZA_EndDate));
			Assert.That(result.ZZA_ZZZ_NKDataGrouping, Is.EqualTo(p.ZZA_ZZZ_NKDataGrouping));
			Assert.That(result.ZZA_TradeGroup, Is.EqualTo(p.ZZA_TradeGroup));
		}

		RefCusTradeGroup CreateAgreement(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusTradeGroup { ZZA_PK = Guid.NewGuid(), ZZA_TradeGroup = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZA_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusTradeGroup> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet));
		}

		IEnumerable<Models.RefCusTradeGroup> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusTradeGroupService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZA_TradeGroup);
		}

		DateTime Now;
		ObjectReferenceDataRepository repo;
		[SetUp]
		public void SetUp()
		{
			Now = DateTime.UtcNow;
			repo = new ObjectReferenceDataRepository();
		}
	}
}
