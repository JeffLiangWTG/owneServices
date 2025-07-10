using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbReleaseNoteReadCollectionByStaff : DependentBusinessObjectCollection<GlbReleaseNoteRead, GlbStaff>
	{
		public GlbReleaseNoteReadCollectionByStaff(GlbStaff parent) : base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => GlbReleaseNoteReadSchema.GR_GS_Staff;
	}
}
