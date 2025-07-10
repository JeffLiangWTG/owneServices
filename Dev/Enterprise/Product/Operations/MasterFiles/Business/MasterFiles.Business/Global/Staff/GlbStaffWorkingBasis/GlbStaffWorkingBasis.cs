using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(GlbStaff), "WorkingBases")]
	public class GlbStaffWorkingBasis : AutoGlbStaffWorkingBasis
	{
		public GlbStaffWorkingBasis(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GSW_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}

		[RelatedBusinessObject("Staff")]
		public override ZGuid GSW_GS_Staff { get => base.GSW_GS_Staff; set => base.GSW_GS_Staff = value; }

		public virtual GlbStaff Staff
		{
			get { return (GlbStaff)Factory.Load(typeof(GlbStaff), GSW_GS_Staff); }
		}
	}
}
