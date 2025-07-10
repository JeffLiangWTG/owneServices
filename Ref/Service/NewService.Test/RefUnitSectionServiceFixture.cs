using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	public class RefUnitSectionServiceFixture
	{
		static short DataSetId => Helper.GetDataSetId(DataSet.RefUnitSection);
		static string TblPrefix => "RUS";

		[Test]
		public void GetLatest_RefUnitSection_Data()
		{
			var dataSet1 = repo.Create(() => new RefUnitSection
			{
				RUS_PK = new Guid("68E9ADF1-400C-4E5F-9439-D57C0C83063F"),
				RUS_Code = "AA",
				RUS_Group = "CEDEX"
			});
			repo.Create(() => new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = Now,
				RVC_DataSetId = DataSetId,
				RVC_ParentPK = dataSet1.RUS_PK,
				RVC_ParentCode = TblPrefix,
				RVC_IsPublished = true
			});
			var dataSets = GetDataSets(null, 0, null, null, DataSetId);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RUS_Code, Is.EqualTo(dataSet1.RUS_Code));
		}

		[TestCase("789591D6-BF86-486D-93E9-3184E089DD11", new[] { "AA", "BB" })]
		[TestCase("789591D6-BF86-486D-93E9-3184E089DD13", new[] { "BB" })]
		[TestCase("789591D6-BF86-486D-93E9-3184E089DD15", new string[] { })]
		public void GetLatest_FilterCheckpoint_Data(string checkpointPK, string[] expected)
		{
			var dataSet1 = CreateUnitSection("AA", Now, new Guid("789591D6-BF86-486D-93E9-3184E089DD12"));
			var dataSet2 = CreateUnitSection("BB", Now, new Guid("789591D6-BF86-486D-93E9-3184E089DD14"));
			var dataSets = GetDataSets(null, 0, CheckpointHelper.Create(new Guid(checkpointPK)), null, DataSetId);
			Assert.That(dataSets.Select(x => x.RUS_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA" })]
		[TestCase(-1, 10, new[] { "AA", "BB" })]
		[TestCase(null, 3, new[] { "AA" })]
		[TestCase(null, 10, new[] { "AA", "BB" })]
		[TestCase(null, -1, new string[] { })]
		public void GetLatest_Filter_Data(int? lowerTimestampOffset, int upperTimestampOffset, string[] expected)
		{
			CreateUnitSection("AA", Now);
			CreateUnitSection("BB", Now.AddDays(10));

			var dataSets = GetDataSets(lowerTimestampOffset, upperTimestampOffset, null, null, DataSetId);
			Assert.That(dataSets.Select(x => x.RUS_Code).ToArray(), Is.EqualTo(expected));
		}

		RefUnitSection CreateUnitSection(string code, DateTime dateTime, Guid? setionPK = null)
		{
			var result = repo.Create(() => new RefUnitSection
			{
				RUS_PK = setionPK ?? Guid.NewGuid(),
				RUS_Code = code
			});
			repo.Create(() => new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = dateTime,
				RVC_DataSetId = DataSetId,
				RVC_ParentPK = result.RUS_PK,
				RVC_ParentCode = TblPrefix,
				RVC_IsPublished = true
			});
			return result;
		}

		IEnumerable<Models.RefUnitSection> GetDataSets(int? lowerTimestampOffset, int upperTimestampOffset, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var lowerTimestamp = lowerTimestampOffset.HasValue ? (DateTime?)Now.AddDays(lowerTimestampOffset.Value) : null;
			var upperTimestamp = Now.AddDays(upperTimestampOffset);
			var service = new RefUnitSectionService(repo);
			return service.GetData(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, DataSetId).OrderBy(x => x.RUS_Code);
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
