using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRefFacilityCollection : DependentBusinessObjectCollection<OrgRefFacility, OrgHeader>
	{
		public OrgRefFacilityCollection(OrgHeader parentHeader, BusinessObjectFactory factory) : base(parentHeader, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return OrgRefFacilitySchema.OFC_OH_Organization; }
		}
	}
}
