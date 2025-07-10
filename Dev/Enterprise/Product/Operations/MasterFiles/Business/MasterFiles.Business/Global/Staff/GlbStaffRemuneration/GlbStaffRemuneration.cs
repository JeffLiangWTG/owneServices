using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(GlbStaff), "Remuneration")]
	public class GlbStaffRemuneration : AutoGlbStaffRemuneration
	{
		public GlbStaffRemuneration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GSR_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}

		[RelatedBusinessObject("Staff")]
		public override ZGuid GSR_GS_Staff { get => base.GSR_GS_Staff; set => base.GSR_GS_Staff = value; }

		public virtual GlbStaff Staff
		{
			get { return (GlbStaff)Factory.Load(typeof(GlbStaff), GSR_GS_Staff); }
		}

		#region Properties

		#region Entitlements

		public GlbStaffEntitlementCollection Entitlements => fEntitlements = fEntitlements ?? new GlbStaffEntitlementCollection(Factory, new ZQuery(GlbStaffEntitlementSchema.GSI_GSR_Remuneration, PK));
		GlbStaffEntitlementCollection fEntitlements;

		#endregion

		#endregion
	}
}
