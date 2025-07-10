using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class PersonAssociationsJobApplicantWrapper : PersonAssociationsTreeBizObjWrapper
	{
		public PersonAssociationsJobApplicantWrapper(PersonAssociationsTreeModel treeModel, IHRJobApplicant jobApplicant)
			: base(treeModel)
		{
			this.JobApplicant = jobApplicant;
		}

		#region Properties

		public IHRJobApplicant JobApplicant { get; }

		public bool ViewAllowed
		{
			get { return Env.Security.HRJobApplicantView.IsAllowed; }
		}

		#endregion

		#region Overrides

		#region Active

		public override ZBool Active => true;

		#endregion

		#region City

		public override ZString City => ViewAllowed ? JobApplicant.HA_City : ViewDeniedMessage;

		#endregion

		#region Description

		public override ZString Description => JobApplicant.HA_FullName;

		#endregion

		#region Grouping

		public override ZString Grouping => (JobApplicant == null)
			? Unknown
			: (ZString)Enterprise.MasterFiles.Business.Res.GetString("PersonAssociationsJobApplicantWrapper|Applicant",
				"Applicant");

		#endregion

		#region State

		public override ZString State => ViewAllowed ? JobApplicant.HA_State : ViewDeniedMessage;

		#endregion

		#region CreatedTime

		public override ZString CreatedTime => JobApplicant.HA_SystemCreateTimeUtc.ToBestReadableDateString();

		#endregion

		#region Country

		RefCountry country;
		public override ZString Country
		{
			get
			{
				if (country == null)
				{
					country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, JobApplicant.HA_RN_NKCountry);
				}

				return country != null ? country.RN_Desc : ZString.Empty;
			}
		}

		#endregion

		#region Email

		public override ZString Email => ViewAllowed ? JobApplicant.HA_EmailAddress : ViewDeniedMessage;

		#endregion

		#endregion
	}
}
