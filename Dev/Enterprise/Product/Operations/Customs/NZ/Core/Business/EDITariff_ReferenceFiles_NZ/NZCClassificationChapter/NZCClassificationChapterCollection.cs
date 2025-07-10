
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationChapterCollection : DependentBusinessObjectCollection<NZCClassificationChapter, NZCClassificationSection>
	{
		public NZCClassificationChapterCollection(NZCClassificationSection section)
			: base(section)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return NZCClassificationChapterSchema.Q2_Q1_Section; }
		}
	}
}
