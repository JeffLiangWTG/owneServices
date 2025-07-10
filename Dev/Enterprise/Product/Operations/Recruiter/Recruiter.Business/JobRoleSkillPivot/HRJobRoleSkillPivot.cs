using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business
{
	[DependentBusinessObject(typeof(HRJobRole), "JobRoleSkills")]
	public class HRJobRoleSkillPivot : AutoHRJobRoleSkillPivot
	{
		public HRJobRoleSkillPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		#region H1_HS

		public override ZGuid H1_HS
		{
			get { return base.H1_HS; }
			set { base.H1_HS = value; }
		}

		#endregion

		#region H1_HJ

		[RelatedBusinessObject("JobRole")]
		public override ZGuid H1_HJ
		{
			get { return base.H1_HJ; }
			set { base.H1_HJ = value; }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public HRJobRole JobRole
		{
			get { return Factory.Load<HRJobRole>(H1_HJ); }
		}

		#endregion
	}
}

