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
	public class RefGlbReleaseNoteServiceFixture
	{
		static short DataSetId => Helper.GetDataSetId(DataSet.RefGlbReleaseNote);
		static string TblPrefix => "ZGF";

		[Test]
		public void GetLatest_RefGlbReleaseNote_Data()
		{
			var dataSet1 = repo.Create(() => new RefGlbReleaseNote
			{
				ZGF_PK = new Guid("D0E14592-C3E8-4312-AADD-DD830732BD2D"),
				ZGF_URL = "http://www.example.com",
				ZGF_ReleaseNoteDate = DateTime.Now,
				ZGF_Section = "BOR",
				ZGF_QuickStartPK = Guid.NewGuid()
			});
			repo.Create(() => new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = Now,
				RVC_DataSetId = DataSetId,
				RVC_ParentPK = dataSet1.ZGF_PK,
				RVC_ParentCode = TblPrefix,
				RVC_IsPublished = true
			});
			var dataSets = GetDataSets(null, 0, null, null, DataSetId);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZGF_Section, Is.EqualTo(dataSet1.ZGF_Section));
		}

		[TestCase("589591D6-BF86-486D-93E9-3184E089DD11", new[] { "AA", "BB" })]
		[TestCase("589591D6-BF86-486D-93E9-3184E089DD13", new[] { "BB" })]
		[TestCase("589591D6-BF86-486D-93E9-3184E089DD15", new string[] { })]
		public void GetLatest_FilterCheckpoint_Data(string checkpointPK, string[] expected)
		{
			var dataSet1 = CreateRefGlbReleaseNote("AA", "http://www.example.com", "Sum1", "BOR", "", Now, new Guid("589591D6-BF86-486D-93E9-3184E089DD12"));
			var dataSet2 = CreateRefGlbReleaseNote("BB", "http://www.example.com", "Sum2", "C1U", "1", Now, new Guid("589591D6-BF86-486D-93E9-3184E089DD14"));
			var dataSets = GetDataSets(null, 0, CheckpointHelper.Create(new Guid(checkpointPK)), null, DataSetId);
			Assert.That(dataSets.Select(x => x.ZGF_Category).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA" })]
		[TestCase(-1, 10, new[] { "AA", "BB" })]
		[TestCase(null, 3, new[] { "AA" })]
		[TestCase(null, 10, new[] { "AA", "BB" })]
		[TestCase(null, -1, new string[] { })]
		public void GetLatest_Filter_Data(int? lowerTimestampOffset, int upperTimestampOffset, string[] expected)
		{
			CreateRefGlbReleaseNote("AA", "http://www.example.com", "Sum1", "BOR", "", Now);
			CreateRefGlbReleaseNote("BB", "http://www.example.com", "Sum2", "BOR", "", Now.AddDays(10));

			var dataSets = GetDataSets(lowerTimestampOffset, upperTimestampOffset, null, null, DataSetId);
			Assert.That(dataSets.Select(x => x.ZGF_Category).ToArray(), Is.EqualTo(expected));
		}

		RefGlbReleaseNote CreateRefGlbReleaseNote(string category, string url, string summary, string section, string minVersion, DateTime dateTime, Guid? notePK = null)
		{
			var result = repo.Create(() => new RefGlbReleaseNote
			{
				ZGF_PK = notePK ?? Guid.NewGuid(),
				ZGF_Category = category,
				ZGF_URL = url,
				ZGF_Summary = summary,
				ZGF_Section = section,
				ZGF_MinVersion = minVersion
			});
			repo.Create(() => new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = dateTime,
				RVC_DataSetId = DataSetId,
				RVC_ParentPK = result.ZGF_PK,
				RVC_ParentCode = TblPrefix,
				RVC_IsPublished = true
			});
			return result;
		}

		IEnumerable<Models.RefGlbReleaseNote> GetDataSets(int? lowerTimestampOffset, int upperTimestampOffset, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var lowerTimestamp = lowerTimestampOffset.HasValue ? (DateTime?)Now.AddDays(lowerTimestampOffset.Value) : null;
			var upperTimestamp = Now.AddDays(upperTimestampOffset);
			var service = new RefGlbReleaseNoteService(repo);
			return service.GetData(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, DataSetId).OrderBy(x => x.ZGF_Category);
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
