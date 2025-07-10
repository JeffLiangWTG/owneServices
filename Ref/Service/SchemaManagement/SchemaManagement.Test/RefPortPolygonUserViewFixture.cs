using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[TransactionedTestCase]
	class RefPortPolygonUserViewFixture
	{
		[Test]
		public void View()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var wktReader = new WKTReader();
			var emptyPoint = (Point)wktReader.Read("POINT EMPTY");
			emptyPoint.SRID = 4326;
			var polygon = (Polygon)wktReader.Read("POLYGON ((0.5 0.5, 5 0, 5 5, 0 5, 0.5 0.5), (1.5 1, 4 3, 4 1, 1.5 1))");
			polygon.SRID = 4326;

			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var portPolygonUserView = new RefPortPolygonUserView()
				{
					RPP_PK = Guid.NewGuid(),
					RPP_PortId = 10,
					RPP_SerializedPolygon = emptyPoint,
					RPP_IsPublished = true
				};
				context.RefPortPolygonUserViews.Add(portPolygonUserView);
				context.SaveChanges();
				Assert.That(context.DataSetChangeHistories.Where(x => x.DCH_ParentPK == portPolygonUserView.RPP_PK).ToArray(), Has.Length.EqualTo(1));
			}
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var portPolygonUserView = context.RefPortPolygonUserViews.FirstOrDefault();
				Assert.AreEqual(10, portPolygonUserView.RPP_PortId);
				Assert.AreEqual(emptyPoint.AsText(), portPolygonUserView.RPP_SerializedPolygon.AsText());

				var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == portPolygonUserView.RPP_PK);
				Assert.AreEqual(false, versionControl.RVC_Deleted);

				portPolygonUserView.RPP_PortId = 11;
				portPolygonUserView.RPP_SerializedPolygon = polygon;
				portPolygonUserView.RPP_IsPublished = false;
				context.SaveChanges();
			}

			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var portPolygonUserView = context.RefPortPolygonUserViews.FirstOrDefault();
				Assert.AreEqual(11, portPolygonUserView.RPP_PortId);
				Assert.AreEqual(polygon.AsText(), portPolygonUserView.RPP_SerializedPolygon.AsText());

				var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == portPolygonUserView.RPP_PK);
				Assert.AreEqual(true, versionControl.RVC_Deleted);
			}

			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var portPolygonUserView = context.RefPortPolygonUserViews.FirstOrDefault();
				context.RefPortPolygonUserViews.Remove(portPolygonUserView);
				Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Should not enable Delete in top-level tables");
			}
		}
	}
}
