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
	public class RefDamageServiceFixture
	{
		static short DataSetId => Helper.GetDataSetId(DataSet.RefDamage);
		static string TblPrefix => "RFM";

		[Test]
		public void GetLatest_RefDamage_Data()
		{
			var dataSet1 = repo.Create(() => new RefDamage
			{
				RFM_PK = new Guid("48E9ADF1-400C-4E5F-9439-D57C0C83063F"),
				RFM_Code = "AA",
				RFM_Group = "CEDEX"
			});
			repo.Create(() => new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = Now,
				RVC_DataSetId = DataSetId,
				RVC_ParentPK = dataSet1.RFM_PK,
				RVC_ParentCode = TblPrefix,
				RVC_IsPublished = true
			});
			var dataSets = GetDataSets(null, 0, null, null, DataSetId);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RFM_Code, Is.EqualTo(dataSet1.RFM_Code));
		}

		[TestCase("689591D6-BF86-486D-93E9-3184E089DD11", new[] { "AA", "BB" })]
		[TestCase("689591D6-BF86-486D-93E9-3184E089DD13", new[] { "BB" })]
		[TestCase("689591D6-BF86-486D-93E9-3184E089DD15", new string[] { })]
		public void GetLatest_FilterCheckpoint_Data(string checkpointPK, string[] expected)
		{
			var dataSet1 = CreateDamage("AA", Now, new Guid("689591D6-BF86-486D-93E9-3184E089DD12"));
			var dataSet2 = CreateDamage("BB", Now, new Guid("689591D6-BF86-486D-93E9-3184E089DD14"));
			var dataSets = GetDataSets(null, 0, CheckpointHelper.Create(new Guid(checkpointPK)), null, DataSetId);
			Assert.That(dataSets.Select(x => x.RFM_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA" })]
		[TestCase(-1, 10, new[] { "AA", "BB" })]
		[TestCase(null, 3, new[] { "AA" })]
		[TestCase(null, 10, new[] { "AA", "BB" })]
		[TestCase(null, -1, new string[] { })]
		public void GetLatest_Filter_Data(int? lowerTimestampOffset, int upperTimestampOffset, string[] expected)
		{
			CreateDamage("AA", Now);
			CreateDamage("BB", Now.AddDays(10));

			var dataSets = GetDataSets(lowerTimestampOffset, upperTimestampOffset, null, null, DataSetId);
			Assert.That(dataSets.Select(x => x.RFM_Code).ToArray(), Is.EqualTo(expected));
		}

		RefDamage CreateDamage(string code, DateTime dateTime, Guid? DamagePK = null)
		{
			var result = repo.Create(() => new RefDamage
			{
				RFM_PK = DamagePK ?? Guid.NewGuid(),
				RFM_Code = code
			});
			repo.Create(() => new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = dateTime,
				RVC_DataSetId = DataSetId,
				RVC_ParentPK = result.RFM_PK,
				RVC_ParentCode = TblPrefix,
				RVC_IsPublished = true
			});
			return result;
		}

		IEnumerable<Models.RefDamage> GetDataSets(int? lowerTimestampOffset, int upperTimestampOffset, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var lowerTimestamp = lowerTimestampOffset.HasValue ? (DateTime?)Now.AddDays(lowerTimestampOffset.Value) : null;
			var upperTimestamp = Now.AddDays(upperTimestampOffset);
			var service = new RefDamageService(repo);
			return service.GetData(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, DataSetId).OrderBy(x => x.RFM_Code);
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
