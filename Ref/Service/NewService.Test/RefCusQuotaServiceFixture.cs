using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusQuotaServiceFixture
	{
		static string TblPrefix => "ZXQ";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusQuota);

		[TestCase("2D06FD1A-307B-4A1A-B5BC-2946AFFA0874", new[] { "AAA", "BBB" })]
		[TestCase("2D06FD1A-307B-4A1A-B5BC-2946AFFA0878", new[] { "BBB" })]
		[TestCase("2D06FD1A-307B-4A1A-B5BC-2946AFFA087F", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusQuota { ZXQ_PK = new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0876"), ZXQ_OrderNumber = "AAA" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZXQ_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusQuota { ZXQ_PK = new Guid("2D06FD1A-307B-4A1A-B5BC-2946AFFA0879"), ZXQ_OrderNumber = "BBB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZXQ_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.ZXQ_OrderNumber).ToArray();
			Assert.That(dataSets, Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AAA", "BBB" })]
		[TestCase(1, 3, new[] { "BBB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AAA", "BBB" })]
		[TestCase(null, 1, new[] { "AAA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Data_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateData(Now, "AAA");
			var p2 = CreateData(Now.AddDays(2), "BBB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZXQ_OrderNumber).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var p = CreateData(Now.AddDays(1), "ZZZ");
			p.ZXQ_UnitOfMeasure = "Test";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZXQ_OrderNumber, Is.EqualTo("ZZZ"));
			Assert.That(result.ZXQ_UnitOfMeasure, Is.EqualTo("Test"));
		}

		RefCusQuota CreateData(DateTime dateTime, string featureCode)
		{
			var result = repo.Create(() => new RefCusQuota { ZXQ_PK = Guid.NewGuid(), ZXQ_OrderNumber = featureCode });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZXQ_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusQuota> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusQuota> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusQuotaService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZXQ_OrderNumber);
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
