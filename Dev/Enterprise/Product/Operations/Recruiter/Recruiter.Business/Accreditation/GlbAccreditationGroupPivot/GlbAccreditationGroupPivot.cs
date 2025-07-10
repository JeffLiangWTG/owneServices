using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationGroupPivot : AutoGlbAccreditationGroupPivot
	{
		public GlbAccreditationGroupPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("Accreditation")]
		public override ZGuid HAP_HAC
		{
			get => base.HAP_HAC;
			set => base.HAP_HAC = value;
		}

		public GlbAccreditation Accreditation => Factory.Load<GlbAccreditation>(HAP_HAC);

		[RelatedBusinessObject("AccreditationGroup")]
		public override ZGuid HAP_HAG
		{
			get => base.HAP_HAG;
			set => base.HAP_HAG = value;
		}

		public GlbAccreditationGroup AccreditationGroup => Factory.Load<GlbAccreditationGroup>(HAP_HAG);
	}
}
