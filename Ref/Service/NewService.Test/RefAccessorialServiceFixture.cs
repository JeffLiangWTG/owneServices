using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefAccessorialServiceFixture
	{
		static string TblPrefix => "ASI";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefAccessorial);

		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDD4", new[] { "AAA", "BBB" })]
		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDD8", new[] { "BBB" })]
		[TestCase("7FEBD40B-8FC0-42CD-9096-A95823B9BDDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefAccessorial { ASI_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD6"), ASI_Code = "AAA" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ASI_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefAccessorial { ASI_PK = new Guid("7FEBD40B-8FC0-42CD-9096-A95823B9BDD9"), ASI_Code = "BBB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ASI_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.ASI_Code).ToArray();
			Assert.AreEqual(expected, dataSets);
		}

		[TestCase(-1, 3, new[] { "AAA", "BBB" })]
		[TestCase(1, 3, new[] { "BBB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AAA", "BBB" })]
		[TestCase(null, 1, new[] { "AAA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Data_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var data1 = CreateData(Now, "AAA");
			var data2 = CreateData(Now.AddDays(2), "BBB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.AreEqual(expected, dataSets.Select(x => x.ASI_Code).ToArray());
		}

		[Test]
		public void GetLatest_RefAccessorial_Data()
		{
			var data = CreateData(Now.AddDays(1), "AAA");
			data.ASI_Description = "STRING";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.AreEqual("AAA", result.ASI_Code);
			Assert.AreEqual("STRING", result.ASI_Description);
		}

		RefAccessorial CreateData(DateTime dateTime, string code)
		{
			var result = repo.Create(() => new RefAccessorial { ASI_PK = Guid.NewGuid(), ASI_Code = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ASI_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefAccessorial> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefAccessorial> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefAccessorialService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(10), checkpoint, null, DataSetId).OrderBy(x => x.ASI_Code);
		}

		DateTime Now = DateTime.UtcNow;
		ObjectReferenceDataRepository repo;

		[SetUp]
		public void SetUp()
		{
			repo = new ObjectReferenceDataRepository();
		}
	}
}
