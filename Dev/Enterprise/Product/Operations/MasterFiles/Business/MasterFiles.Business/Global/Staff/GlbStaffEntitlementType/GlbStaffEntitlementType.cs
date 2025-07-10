using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(GlbStaffEntitlementGroup), "EntitlementTypes")]
	public class GlbStaffEntitlementType : AutoGlbStaffEntitlementType
	{
		public GlbStaffEntitlementType(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[RelatedBusinessObject("EntitlementGroup")]
		public override ZGuid GEW_GEG_EntitlementGroup { get => base.GEW_GEG_EntitlementGroup; set => base.GEW_GEG_EntitlementGroup = value; }

		public virtual GlbStaffEntitlementGroup EntitlementGroup
		{
			get { return Factory.Load<GlbStaffEntitlementGroup>(GEW_GEG_EntitlementGroup); }
		}
	}
}
