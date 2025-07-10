using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineSpecialHandlingCodeCollection : DependentBusinessObjectCollection<RefAirlineSpecialHandlingCode, RefAirline>
	{
		public RefAirlineSpecialHandlingCodeCollection(RefAirline parent) : base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return RefAirlineSpecialHandlingCodeSchema.RHC_RM_Airline; }
		}
	}
}
