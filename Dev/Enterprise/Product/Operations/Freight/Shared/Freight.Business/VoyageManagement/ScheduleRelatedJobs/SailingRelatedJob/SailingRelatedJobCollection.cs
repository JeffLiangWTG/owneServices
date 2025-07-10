using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class SailingRelatedJobCollection : DependentBusinessObjectCollection<SailingRelatedJob, JobSailing>
	{
		public SailingRelatedJobCollection(JobSailing sailing)
			: base(sailing)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return ViewSailingRelatedJobSchema.VJX_JX; }
		}

		public override bool ReadOnly => true;

		protected override bool AllowNewCore => false;
	}
}
