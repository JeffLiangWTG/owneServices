
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationCollection : DependentBusinessObjectCollection<NZCClassification, NZCClassificationChapter>
	{
		public NZCClassificationCollection(NZCClassificationChapter chapter)
			: base(chapter)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return NZCClassificationSchema.U0_Q2_Chapter; }
		}
	}
}
