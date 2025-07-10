using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;

namespace Enterprise.Recruiter.Business
{
	[DependentBusinessObject(typeof(GlbAccreditation), "RequirementPivotCollection")]
	public class GlbAccreditationRequirementPivot : AutoGlbAccreditationRequirementPivot, IGlbAccreditationRequirementPivot
	{
		public GlbAccreditationRequirementPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbAccreditation Accreditation
		{
			get { return Factory.Load<GlbAccreditation>(HAR_HAC); }
		}

		public GlbAccreditation AccreditationParent
		{
			get { return Factory.Load<GlbAccreditation>(HAR_HAC_Parent); }
		}

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var accred1 = Factory.NewWithValidTestData<GlbAccreditation>();
			var accred2 = Factory.NewWithValidTestData<GlbAccreditation>();
			HAR_HAC_Parent = accred1.PK;
			HAR_HAC = accred2.PK;
		}

#endif
		#endregion
	}
}
