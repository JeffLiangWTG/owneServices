
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCConcessionDutyRateCollection : DependentBusinessObjectCollection<NZCConcessionDutyRate, NZCConcession>
	{
		public NZCConcessionDutyRateCollection(NZCConcession concession)
			: base(concession)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(NZCConcessionDutyRateSchema.U5_U2_Concession, Master.PK);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return NZCConcessionDutyRateSchema.U5_U2_Concession; }
		}
	}
}
