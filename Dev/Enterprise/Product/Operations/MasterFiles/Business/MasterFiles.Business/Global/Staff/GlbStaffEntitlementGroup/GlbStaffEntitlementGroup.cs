using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffEntitlementGroup : AutoGlbStaffEntitlementGroup
	{
		public GlbStaffEntitlementGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public GlbStaffEntitlementTypeCollection EntitlementTypes => fEntitlementTypes = fEntitlementTypes ?? new GlbStaffEntitlementTypeCollection(Factory, new ZQuery(GlbStaffEntitlementTypeSchema.GEW_GEG_EntitlementGroup, PK));
		GlbStaffEntitlementTypeCollection fEntitlementTypes;

		#endregion
	}
}
