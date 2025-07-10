using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class VoyageRelatedJobCollection : DependentBusinessObjectCollection<VoyageRelatedJob, JobVoyage>
	{
		public VoyageRelatedJobCollection(JobVoyage voyage)
			: base(voyage)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return ViewVoyageRelatedJobSchema.VJV_JV; }
		}

		public override bool ReadOnly => true;

		protected override bool AllowNewCore => false;
	}
}
