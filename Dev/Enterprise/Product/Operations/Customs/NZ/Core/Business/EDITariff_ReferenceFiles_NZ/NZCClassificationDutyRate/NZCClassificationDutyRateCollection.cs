
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationDutyRateCollection : DependentBusinessObjectCollection<NZCClassificationDutyRate, NZCClassification>
	{
		public NZCClassificationDutyRateCollection(NZCClassification classification, BusinessObjectFactory factory)
			: base(classification, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return NZCClassificationDutyRateSchema.U1_U0_Classification; }
		}
	}
}
