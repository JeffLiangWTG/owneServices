using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class UNDGVersionServiceFixture
	{
		static short DataSetId => Helper.GetDataSetId(DataSet.UNDGVersion);
		static string TblPrefix => "DV";

		[Test]
		public void GetLatest_UNDGVersion_Data()
		{
			var dataSet1 = repo.Create(() => new UNDGVersion
			{
				DV_PK = new Guid("88948BBF-A740-46E8-94D6-4061630B09B8"),
				DV_Name = "version 1",
				DV_Standard = "standard 1",
				DV_IsActive = true
			});
			repo.Create(() => new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = Now,
				RVC_DataSetId = DataSetId,
				RVC_ParentPK = dataSet1.DV_PK,
				RVC_ParentCode = TblPrefix,
				RVC_IsPublished = true
			});
			var dataSets = GetDataSets(null, 0, null, null, DataSetId);
			var result = dataSets.ElementAt(0);
			Assert.That(result.DV_Name, Is.EqualTo(dataSet1.DV_Name));
		}

		[TestCase("842BB1E5-2B82-409D-AB8F-A25806315B11", new[] { "version 1", "version 2" })]
		[TestCase("842BB1E5-2B82-409D-AB8F-A25806315B13", new[] { "version 2" })]
		[TestCase("842BB1E5-2B82-409D-AB8F-A25806315B15", new string[] { })]
		public void GetLatest_FilterCheckpoint_Data(string checkpointPK, string[] expected)
		{
			var dataSet1 = CreateUNDGVersion("version 1", "standard 1", true, Now, new Guid("842BB1E5-2B82-409D-AB8F-A25806315B12"));
			var dataSet2 = CreateUNDGVersion("version 2", "standard 2", false, Now, new Guid("842BB1E5-2B82-409D-AB8F-A25806315B14"));
			var dataSets = GetDataSets(null, 0, CheckpointHelper.Create(new Guid(checkpointPK)), null, DataSetId);
			Assert.That(dataSets.Select(x => x.DV_Name).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "version 1" })]
		[TestCase(-1, 10, new[] { "version 1", "version 2" })]
		[TestCase(null, 3, new[] { "version 1" })]
		[TestCase(null, 10, new[] { "version 1", "version 2" })]
		[TestCase(null, -1, new string[] { })]
		public void GetLatest_Filter_Data(int? lowerTimestampOffset, int upperTimestampOffset, string[] expected)
		{
			CreateUNDGVersion("version 1", "standard 1", true, Now);
			CreateUNDGVersion("version 2", "standard 2", false, Now.AddDays(10));

			var dataSets = GetDataSets(lowerTimestampOffset, upperTimestampOffset, null, null, DataSetId);
			Assert.That(dataSets.Select(x => x.DV_Name).ToArray(), Is.EqualTo(expected));
		}

		UNDGVersion CreateUNDGVersion(string name, string standard, bool isActive,DateTime dateTime, Guid? dvPk = null)
		{
			var result = repo.Create(() => new UNDGVersion
			{
				DV_PK = dvPk ?? Guid.NewGuid(),
				DV_Name = name,
				DV_Standard = standard,
				DV_IsActive = isActive
			});
			repo.Create(() => new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = dateTime,
				RVC_DataSetId = DataSetId,
				RVC_ParentPK = result.DV_PK,
				RVC_ParentCode = TblPrefix,
				RVC_IsPublished = true
			});
			return result;
		}

		IEnumerable<Models.UNDGVersion> GetDataSets(int? lowerTimestampOffset, int upperTimestampOffset, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var lowerTimestamp = lowerTimestampOffset.HasValue ? (DateTime?)Now.AddDays(lowerTimestampOffset.Value) : null;
			var upperTimestamp = Now.AddDays(upperTimestampOffset);
			var service = new UNDGVersionService(repo);
			return service.GetData(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, DataSetId).OrderBy(x => x.DV_Name);
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
