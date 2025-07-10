using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(GlbStaff), "Reviews")]
	public class GlbStaffReview : AutoGlbStaffReview
	{
		public GlbStaffReview(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GSV_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}

		[RelatedBusinessObject("Staff")]
		public override ZGuid GSV_GS_Staff { get => base.GSV_GS_Staff; set => base.GSV_GS_Staff = value; }

		public virtual GlbStaff Staff
		{
			get { return (GlbStaff)Factory.Load(typeof(GlbStaff), GSV_GS_Staff); }
		}
	}
}
