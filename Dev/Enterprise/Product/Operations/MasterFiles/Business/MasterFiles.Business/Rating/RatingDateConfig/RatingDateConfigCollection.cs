using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RatingDateConfigCollection : DependentBusinessObjectCollection<RatingDateConfig, OrgHeader>
	{
		public RatingDateConfigCollection(OrgHeader parent)
			: base(parent)
		{
		}

		#region Implementation

		protected override SchemaGuidColumn FKSchemaColumnInDependent => RatingDateConfigSchema.RDT_ParentID;

		#endregion
	}
}
