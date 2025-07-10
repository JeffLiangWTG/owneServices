using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	public class RefFacilityServiceFixture
	{
		static short DataSetId => Helper.GetDataSetId(DataSet.RefFacility);
		static string TblPrefix => "RFT";

		[Test]
		public void GetLatest_RefFacility_Data()
		{
			var dataSet1 = repo.Create(() => new RefFacility
			{
				RFT_PK = new Guid("689591D6-BF86-486D-93E9-3184E089DD1C"),
				RFT_Code = "AA"
			});
			repo.Create(() => new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = Now,
				RVC_DataSetId = DataSetId,
				RVC_ParentPK = dataSet1.RFT_PK,
				RVC_ParentCode = TblPrefix,
				RVC_IsPublished = true
			});
			var dataSets = GetDataSets(null, 0, null, null, DataSetId);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RFT_Code, Is.EqualTo(dataSet1.RFT_Code));
		}

		[Test]
		public void GetLatest_FacilityLocalCode_Data()
		{
			var facility = CreateFacility("AA", Now.AddDays(1));
			var facilityLocalCode = repo.Create(() => new RefFacilityLocalCode
			{
				RFL_PK = Guid.NewGuid(),
				RFL_RFT_NKFacilityCode = facility.RFT_Code,
				RFL_Code = "BB"
			});
			var dataSets = GetDataSets(null, 1, null, null, DataSetId);
			Assert.That(dataSets.ToArray().Length, Is.EqualTo(1));
			var result = dataSets.ToArray()[0];
			Assert.That(result.RFT_Code, Is.EqualTo(facility.RFT_Code));
			Assert.That(result.RefFacilityLocalCodes.Length, Is.EqualTo(1));
			var resultFacilityLocalCode = result.RefFacilityLocalCodes[0];
			Assert.That(resultFacilityLocalCode.RFL_RFT_NKFacilityCode, Is.EqualTo(facility.RFT_Code));
			Assert.That(resultFacilityLocalCode.RFL_Code, Is.EqualTo(facilityLocalCode.RFL_Code));
		}

		[TestCase("689591D6-BF86-486D-93E9-3184E089DD11", new[] { "AA", "BB" })]
		[TestCase("689591D6-BF86-486D-93E9-3184E089DD13", new[] { "BB" })]
		[TestCase("689591D6-BF86-486D-93E9-3184E089DD15", new string[] { })]
		public void GetLatest_FilterCheckpoint_Data(string checkpointPK, string[] expected)
		{
			var dataSet1 = CreateFacility("AA", Now, new Guid("689591D6-BF86-486D-93E9-3184E089DD12"));
			var dataSet2 = CreateFacility("BB", Now, new Guid("689591D6-BF86-486D-93E9-3184E089DD14"));
			var dataSets = GetDataSets(null, 0, CheckpointHelper.Create(new Guid(checkpointPK)), null, DataSetId);
			Assert.That(dataSets.Select(x => x.RFT_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA" })]
		[TestCase(-1, 10, new[] { "AA", "BB" })]
		[TestCase(null, 3, new[] { "AA" })]
		[TestCase(null, 10, new[] { "AA", "BB" })]
		[TestCase(null, -1, new string[] { })]
		public void GetLatest_Filter_Data(int? lowerTimestampOffset, int upperTimestampOffset, string[] expected)
		{
			CreateFacility("AA", Now);
			CreateFacility("BB", Now.AddDays(10));

			var dataSets = GetDataSets(lowerTimestampOffset, upperTimestampOffset, null, null, DataSetId);
			Assert.That(dataSets.Select(x => x.RFT_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, "")]
		public void GetLatest_CorrectGeoLocation_Data(int? lowerTimestampOffset, int upperTimestampOffset, string expected)
		{
			var facility = CreateFacility("AA", Now);
			var dataSets = GetDataSets(lowerTimestampOffset, upperTimestampOffset, null, null, DataSetId);
			Assert.That(dataSets.Select(x => x.RFT_GeoLocation).ToArray()[0].ToString(), Is.EqualTo("POINT (10 20)"));
		}

		RefFacility CreateFacility(string code, DateTime dateTime, Guid? facilityPK = null)
		{
			var result = repo.Create(() => new RefFacility
			{
				RFT_PK = facilityPK ?? Guid.NewGuid(),
				RFT_Code = code,
				RFT_GeoLocation = new Point(10, 20) { SRID = 4326 }
			});
			repo.Create(() => new RefDbVersionControl
			{
				RVC_LastUpdatedUTC = dateTime,
				RVC_DataSetId = DataSetId,
				RVC_ParentPK = result.RFT_PK,
				RVC_ParentCode = TblPrefix,
				RVC_IsPublished = true
			});
			return result;
		}

		IEnumerable<Models.RefFacility> GetDataSets(int? lowerTimestampOffset, int upperTimestampOffset, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			var lowerTimestamp = lowerTimestampOffset.HasValue ? (DateTime?)Now.AddDays(lowerTimestampOffset.Value) : null;
			var upperTimestamp = Now.AddDays(upperTimestampOffset);
			var service = new RefFacilityService(repo);
			return service.GetData(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, DataSetId).OrderBy(x => x.RFT_Code);
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
