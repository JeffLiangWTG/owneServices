using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefAirlineCommodityCodeServiceFixture
	{
		static string TblPrefix => "RAC";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefAirlineCommodityCode);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefAirlineCommodityCode { RAC_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), RAC_Code = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.RAC_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefAirlineCommodityCode { RAC_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), RAC_Code = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.RAC_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.RAC_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Currency_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateAirlineCommodityCode(Now, "AA");
			var p2 = CreateAirlineCommodityCode(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.RAC_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_RefAirlineCommodityCode_Data()
		{
			var p = repo.Create(() => new RefAirlineCommodityCode
			{
				RAC_PK = Guid.NewGuid(),
				RAC_AirlineID = "XXX",
				RAC_Code = "XXX",
				RAC_Description = "Test Description"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.RAC_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RAC_AirlineID, Is.EqualTo(p.RAC_AirlineID));
			Assert.That(result.RAC_Code, Is.EqualTo(p.RAC_Code));
			Assert.That(result.RAC_Description, Is.EqualTo(p.RAC_Description));
		}

		RefAirlineCommodityCode CreateAirlineCommodityCode(DateTime dateTime, string code = "XX", string airlineID = "XX")
		{
			var result = repo.Create(() => new RefAirlineCommodityCode { RAC_PK = Guid.NewGuid(), RAC_Code = code, RAC_Description = "Description", RAC_AirlineID = airlineID });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RAC_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefAirlineCommodityCode> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefAirlineCommodityCode> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefAirlineCommodityCodeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.RAC_Code);
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
