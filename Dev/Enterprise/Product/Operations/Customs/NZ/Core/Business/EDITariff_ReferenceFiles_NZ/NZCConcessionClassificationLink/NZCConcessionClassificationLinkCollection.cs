
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCConcessionClassificationLinkCollection : DependentBusinessObjectCollection<NZCConcessionClassificationLink, NZCConcession>
	{
		public NZCConcessionClassificationLinkCollection(NZCConcession parentConcession)
			: base(parentConcession)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(NZCConcessionClassificationLinkSchema.U3_U2_Concession, Master.PK);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return NZCConcessionClassificationLinkSchema.U3_U2_Concession; }
		}
	}
}
