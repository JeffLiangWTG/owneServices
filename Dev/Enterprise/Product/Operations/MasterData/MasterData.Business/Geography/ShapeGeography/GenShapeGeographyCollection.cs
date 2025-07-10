using CargoWise.EntityFramework;

namespace Enterprise.MasterData.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class will be used by the shape geography module later.")]
	public class GenShapeGeographyCollection : ActiveBusinessObjectCollection<GenShapeGeography>
	{
		public GenShapeGeographyCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GenShapeGeographyCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
