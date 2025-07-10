using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Rating
{
	public class AccChargeCodeUniversalCodeMappingCollection : DependentBusinessObjectCollection<AccChargeCodeUniversalCodeMapping, AccChargeCode>
	{
		public AccChargeCodeUniversalCodeMappingCollection(AccChargeCode chargeCode)
			: base(chargeCode)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(AccChargeCodeUniversalCodeMappingSchema.AUP_AC, Master.PK);
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			AccChargeCodeUniversalCodeMapping mapping = (AccChargeCodeUniversalCodeMapping)dependent;
			mapping.AUP_AC = Master.PK;
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject dependent, bool forDelete)
		{
			AccChargeCodeUniversalCodeMapping mapping = (AccChargeCodeUniversalCodeMapping)dependent;
			mapping.AUP_AC = ZGuid.Empty;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return AccChargeCodeUniversalCodeMappingSchema.AUP_AC; }
		}
	}
}
