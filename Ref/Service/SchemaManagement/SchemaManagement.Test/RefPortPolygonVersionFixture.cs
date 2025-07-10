using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	public class RefPortPolygonVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefPortPolygon()
				{
					RPP_PK = Guid.NewGuid(),
					RPP_PortId = 10,
					RPP_SerializedPolygon = new Point(-122.333056, 47.609722) { SRID = 4326 }
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefPortPolygon portPolygon)
			{
				portPolygon.RPP_SerializedPolygon = new Point(-101.333056, 23.123456) { SRID = 4326 };
			}
			return true;
		}
	}
}
