using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupCountryCollection : ActiveBusinessObjectCollection<RefCountry>
	{
		public GlbGroupCountryCollection(GlbGroup group, ZQuery filter = null)
			: base(group.Factory, new GlbGroupCountryRelationship(group, filter))
		{
		}

		class GlbGroupCountryRelationship : ManyToManyRelationship<SchemaGuidColumn, ZGuid, SchemaStringColumn, ZString>
		{
			public GlbGroupCountryRelationship(GlbGroup group, ZQuery filter)
				: base(
					group,
					typeof(RefCountry),
					typeof(GlbGroupCountry),
					filter,
					GlbGroupLocationSchema.GGL_GG,
					GlbGroupSchema.PK,
					GlbGroupLocationSchema.GGL_PortOrCountry,
					RefCountrySchema.RN_Code)
			{
			}
		}
	}
}
