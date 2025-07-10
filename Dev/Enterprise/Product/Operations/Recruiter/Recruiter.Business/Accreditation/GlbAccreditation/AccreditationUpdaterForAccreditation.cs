using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class AccreditationUpdaterForAccreditation : AccreditationUpdater
	{
		public AccreditationUpdaterForAccreditation(GlbAccreditation accreditation) : base(accreditation.Factory)
		{
			ParentAccreditation = accreditation;
		}

		public override ZBool IsForAccreditation => true;

		public override ZBool IsForPerson => false;

		public override void Run()
		{
			// accred > job skills > job skill tests > campaigns > Campaign items > exam attempts
			isCancelled = false;

			var inititalFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var personReader = new FilteredBusinessObjectReader<GlbPerson>(GetPersonsQuery(ParentAccreditation.PK), inititalFactory);
			personReader.BatchSize = 100;
			personReader.SaveBeforeLoadNextEnabled = true;

			OnRunBegin();

			try
			{
				long examCount = 0;
				long applicantsCount = 0;
				long totalCount =
					personReader.Factory.GetDatabaseCount(typeof(ExamAttempt),
						GetExamAttemptQuery(this, ParentAccreditation.PK, null));
				long totalApplicantCount = personReader.ApproximateCount;

				TryDeleteAttemptsAndCertificates();

				if (totalCount == 0)
				{
					return;
				}

				foreach (GlbPerson person in personReader)
				{
					if (isCancelled)
					{
						break;
					}

					var factoryInstance = person.Factory._Instance;
					GlbAccreditation currentAccreditation = null;

					foreach (var applicant in person.ApplicantCollection)
					{
						AddApplicantFetchHints(personReader.Factory, applicant);
						if (personReader.Factory._Instance != factoryInstance || currentAccreditation?.Factory._Instance != factoryInstance)
						{
							factoryInstance = personReader.Factory._Instance;
							currentAccreditation?.ClearCache();
							personReader.Factory.AddFetchHint(typeof(GlbAccreditationJobSkillGroup), GlbAccreditationJobSkillGroupSchema.HJG_ParentID, ParentAccreditation.PK);
							currentAccreditation = personReader.Factory.Load<GlbAccreditation>(ParentAccreditation.PK);
						}

						var query = GetExamAttemptQuery(this, currentAccreditation.PK, new[] { applicant.PK });

						var examAttempts = personReader.Factory.Load<ExamAttempt>(query);
						foreach (ExamAttempt examAttempt in examAttempts)
						{
							if (isCancelled)
							{
								break;
							}

							if (currentAccreditation.ShouldCreateNewAttempt(person, examAttempt.EXA_TestCommencedUtc.Date))
							{
								currentAccreditation.CreateNewAttempt(person, examAttempt.EXA_TestCommencedUtc.Date, this);
							}

							currentAccreditation.CompleteAttemptIfRequired(person, examAttempt.EXA_TestCompletedUtc.Date, true, new AccreditationUpdaterParameters(IsFirstExamType, AdditionalCompletionToleranceDays, CompletionToleranceDays, ToDate));

							examCount++;
						}

						OnExamAttemptProcessed(++applicantsCount, totalApplicantCount, examCount, totalCount);
					}
				}
			}
			finally
			{
				OnRunEnd();
			}
		}

		void AddApplicantFetchHints(BusinessObjectFactory factory, BusinessObject applicant)
		{
			factory.AddFetchHint(typeof(LearningCentreCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID, applicant.PK);
		}

		public override void DeleteAttemptsAndCertificates(IGlbAccreditation targetAccreditation = null)
		{
			var deleteFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var accred = deleteFactory.Load<GlbAccreditation>(ParentAccreditation.PK);
			accred.Attempts.Reload(true);
			accred.Attempts.RemoveAndDeleteAll();

			if (DeleteExistingCertificates)
			{
				accred.Factory.Load<GenRegCertAccredMaintList>(GetCertificatesQuery(accred))?.DeleteAll();
			}

			accred.Factory.Save();
		}
	}
}
