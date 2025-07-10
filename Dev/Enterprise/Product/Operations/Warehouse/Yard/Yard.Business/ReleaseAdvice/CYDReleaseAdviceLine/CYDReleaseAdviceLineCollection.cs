using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Busines
{
	public class CYDReleaseAdviceLineCollection : DependentBusinessObjectCollection<CYDReleaseAdviceLine, CYDReleaseAdvice>
	{
		public CYDReleaseAdviceLineCollection(CYDReleaseAdvice master) : base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CYDReleaseAdviceLineSchema.YEL_YRE_ReleaseAdvice; }
		}
	}
}
