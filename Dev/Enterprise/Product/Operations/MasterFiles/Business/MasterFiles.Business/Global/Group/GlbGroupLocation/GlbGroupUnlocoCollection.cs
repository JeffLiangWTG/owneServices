using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupUnlocoCollection : ActiveBusinessObjectCollection<RefUNLOCO>
	{
		public GlbGroupUnlocoCollection(GlbGroup group, ZQuery filter = null)
			: base(group.Factory, new GlbGroupUnlocoRelationship(group, filter))
		{
		}

		class GlbGroupUnlocoRelationship : ManyToManyRelationship<SchemaGuidColumn, ZGuid, SchemaStringColumn, ZString>
		{
			public GlbGroupUnlocoRelationship(GlbGroup group, ZQuery filter)
				: base(
					group,
					typeof(RefUNLOCO),
					typeof(GlbGroupCountry),
					filter,
					GlbGroupLocationSchema.GGL_GG,
					GlbGroupSchema.PK,
					GlbGroupLocationSchema.GGL_PortOrCountry,
					RefUNLOCOSchema.RL_Code)
			{
			}
		}
	}
}
