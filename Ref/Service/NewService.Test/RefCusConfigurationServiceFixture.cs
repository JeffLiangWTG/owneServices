using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusConfigurationServiceFixture
	{
		static string TblPrefix => "ZZJ";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusConfiguration);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusConfiguration { ZZJ_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZJ_ZZZ_NKDefaultDataGrouping = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZJ_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusConfiguration { ZZJ_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZJ_ZZZ_NKDefaultDataGrouping = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZJ_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZJ_ZZZ_NKDefaultDataGrouping).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Currency_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateRefCusConfiguration(Now, "AA");
			var p2 = CreateRefCusConfiguration(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZJ_ZZZ_NKDefaultDataGrouping).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_RefAirlineCommodityCode_Data()
		{
			var p = repo.Create(() => new RefCusConfiguration
			{
				ZZJ_PK = Guid.NewGuid(),
				ZZJ_AllowRiskManagement = true,
				ZZJ_ZZZ_NKDefaultDataGrouping = "Z1",
				ZZJ_EndDate = DateTime.MaxValue,
				ZZJ_StartDate = DateTime.MinValue,
				ZZJ_IsGenericCountry = true,
				ZZJ_IsTransitDeclarationCounty = true,
				ZZJ_RN_NKCustomsCountry = "ZA",
				ZZJ_TariffDataSource = "WTG",
				ZZJ_TurnOnASYCUDACustoms = false,
				ZZJ_TurnOnASYDCUDAManifest = false,
				ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping = "BZ"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZZJ_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZJ_ZZZ_NKDefaultDataGrouping, Is.EqualTo(p.ZZJ_ZZZ_NKDefaultDataGrouping));
			Assert.That(result.ZZJ_TariffDataSource, Is.EqualTo(p.ZZJ_TariffDataSource));
			Assert.That(result.ZZJ_TurnOnASYCUDACustoms, Is.EqualTo(p.ZZJ_TurnOnASYCUDACustoms));
		}

		RefCusConfiguration CreateRefCusConfiguration(DateTime dateTime, string nKDefaultDataGrouping = "XX", string tariffDataSource = "OWN")
		{
			var result = repo.Create(() => new RefCusConfiguration
			{
				ZZJ_PK = Guid.NewGuid(),
				ZZJ_AllowRiskManagement = true,
				ZZJ_ZZZ_NKDefaultDataGrouping = nKDefaultDataGrouping,
				ZZJ_EndDate = DateTime.MaxValue,
				ZZJ_StartDate = DateTime.MinValue,
				ZZJ_IsGenericCountry = true,
				ZZJ_IsTransitDeclarationCounty = true,
				ZZJ_RN_NKCustomsCountry = "ZA",
				ZZJ_TariffDataSource = tariffDataSource,
				ZZJ_TurnOnASYCUDACustoms = false,
				ZZJ_TurnOnASYDCUDAManifest = false,
				ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping = "BZ"
			});
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZJ_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusConfiguration> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusConfiguration> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusConfigurationService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZJ_ZZZ_NKDefaultDataGrouping);
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
