using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class RefPortPolygonServiceFixture
	{
		static string TblPrefix => "RPP";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefPortPolygon);

		[Test]
		public void GetLatest_RefPortPolygon()
		{
			CreateRefPortPolygon(Now.AddDays(1), CreatePolygon("POLYGON((0.5 0.5,5 0,5 5,0 5,0.5 0.5), (1.5 1,4 3,4 1,1.5 1))"));
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RPP_SerializedPolygon, Is.EqualTo("POLYGON ((0.5 0.5, 5 0, 5 5, 0 5, 0.5 0.5), (1.5 1, 4 3, 4 1, 1.5 1))"));
		}
		RefPortPolygon CreateRefPortPolygon(DateTime dateTime, Geometry geography)
		{
			var result = repo.Create(() => new RefPortPolygon { RPP_PK = Guid.NewGuid(), RPP_SerializedPolygon = geography, RPP_PortId = 10 });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RPP_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		Geometry CreatePolygon(string wkt)
		{
			var reader = new WKTReader();
			return reader.Read(wkt);
		}

		IEnumerable<Models.RefPortPolygon> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefPortPolygonService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : Now.AddDays(100), checkpoint, null, DataSetId);
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
