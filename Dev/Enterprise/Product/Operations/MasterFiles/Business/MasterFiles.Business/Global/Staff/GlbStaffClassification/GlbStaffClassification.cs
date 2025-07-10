using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(GlbStaff), "Classifications")]
	public class GlbStaffClassification : AutoGlbStaffClassification
	{
		public GlbStaffClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GSL_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}

		[RelatedBusinessObject("Staff")]
		public override ZGuid GSL_GS_Staff { get => base.GSL_GS_Staff; set => base.GSL_GS_Staff = value; }

		public virtual GlbStaff Staff
		{
			get { return (GlbStaff)Factory.Load(typeof(GlbStaff), GSL_GS_Staff); }
		}
	}
}
