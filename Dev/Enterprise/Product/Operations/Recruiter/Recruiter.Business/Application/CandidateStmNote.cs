using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Business
{
	public class CandidateStmNote : StmNote
	{
		public CandidateStmNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString ST_NoteSource
		{
			get
			{
				switch (ST_Table)
				{
					case AutoHRJobApplication.Schema.TableName when Master is HRJobApplication application:
						return !IsRelatedInParentView ?
							Res.GetString("c820f5cd-35cc-439f-bdd5-b95ee9c85589", "This Job Application") :
							$"{application.JobOpening?.HV_AdTitle ?? Res.GetString("98e5375e-0e9b-4767-bd98-8cda877fad26", "No Valid Campaign")} {application.HP_ApplicationNumber}";

					case AutoHRJobApplicant.Schema.TableName when Master is HRJobApplicant applicant:
						return !IsRelatedInParentView ?
							Res.GetString("7ab2051e-5704-495f-af0d-0095ad5cbbd6", "This Job Applicant") :
							$"Job Applicant {applicant.HA_FullName}";
					default:
						return base.ST_NoteSource;
				}
			}
		}
	}
}
