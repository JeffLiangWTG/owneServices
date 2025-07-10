using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace CargoWise.RefDbRepo.Common.TypeProvider
{
	static class NtsGeometryServicesProvider
	{
		public static NtsGeometryServices GetGeometryServices(int srid) =>
			new NtsGeometryServices(precisionModel: PrecisionModel.Floating.Value, srid: srid);
	}
}
