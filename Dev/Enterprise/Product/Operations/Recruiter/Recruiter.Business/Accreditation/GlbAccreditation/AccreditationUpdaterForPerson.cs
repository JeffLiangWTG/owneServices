using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class AccreditationUpdaterForPerson : AccreditationUpdater, IAccreditationUpdaterForPerson
	{
		public AccreditationUpdaterForPerson() : this(new BusinessObjectFactory())
		{ }

		public AccreditationUpdaterForPerson(BusinessObjectFactory factory) : base(factory)
		{ }

		[BusinessObjectTestExclude]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public IGlbPerson[] Persons { get; private set; }

		public void SetPersons(IGlbPerson[] persons)
		{
			Persons = persons;
		}

		public override ZBool IsForAccreditation => false;

		public override ZBool IsForPerson => true;

		public override void Run()
		{
			if (Persons == null || Persons.Length == 0)
			{
				return;
			}

			isCancelled = false;

			try
			{
				OnRunBegin();
				TryDeleteAttemptsAndCertificates(ParentAccreditation);

				int personsProcessed = 0;
				foreach (GlbPerson person in Persons)
				{
					long examCount = 0;
					var pks = person.ApplicantCollection.Select(a => a.PK).ToArray();
					var query = GetExamAttemptQuery(this, ParentAccreditation?.PK ?? ZGuid.Empty, pks);

					var inititalFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var examReader = new FilteredBusinessObjectReader<ExamAttempt>(query, inititalFactory);
					examReader.BatchSize = 100;
					examReader.SaveBeforeLoadNextEnabled = true;

					long totalCount = examReader.ApproximateCount;

					if (totalCount == 0)
					{
						return;
					}

					GlbPerson personInExamAttemptFactory = null;
					foreach (ExamAttempt examAttempt in examReader)
					{
						if (isCancelled)
						{
							break;
						}

						if (personInExamAttemptFactory == null || personInExamAttemptFactory.Factory != examAttempt.Factory)
						{
							personInExamAttemptFactory = examAttempt.Factory.Load<GlbPerson>(person.PK);
						}

						examAttempt.CampaignItem.CompanyCampaign.SetCurrentSettings(new GlbCompanyCampaign.CampaignSettings(examAttempt.EXA_Version));
						foreach (var accreditation in examAttempt.CampaignItem.CompanyCampaign.RelatedAccreditations.Where(x => ParentAccreditation == null || ParentAccreditation.PK == x.PK))
						{
							if (accreditation.ShouldCreateNewAttempt(personInExamAttemptFactory, examAttempt.EXA_TestCommencedUtc.Date))
							{
								accreditation.CreateNewAttempt(personInExamAttemptFactory, examAttempt.EXA_TestCommencedUtc.Date, this);
							}

							if (accreditation.CompleteAttemptIfRequired(personInExamAttemptFactory, examAttempt.EXA_TestCompletedUtc.Date, true,
								ParentAccreditation != null
								? new AccreditationUpdaterParameters(IsFirstExamType, AdditionalCompletionToleranceDays, CompletionToleranceDays, ToDate)
								: null))
							{
								personInExamAttemptFactory.Factory.Save();
							}
						}

						if (Persons.Length == 1)
						{
							OnExamAttemptProcessed(0, 0, examCount++, totalCount);
						}
					}

					person.AccreditationAttemptCollection.Reload();
					if (Persons.Length > 1)
					{
						OnPersonProcessed(++personsProcessed, Persons.Length);
					}
				}
			}
			finally
			{
				OnRunEnd();
			}
		}

		public override void DeleteAttemptsAndCertificates(IGlbAccreditation targetAccreditation = null)
		{
			foreach (GlbPerson person in Persons)
			{
				if (person == null)
				{
					return;
				}

				if (DeleteExistingCertificates)
				{
					DeleteCertificatesForPerson(person, targetAccreditation);
				}

				person.AccreditationAttemptCollection.Reload();
				var accreditationAttempts = person.AccreditationAttemptCollection.OfType<GlbAccreditationAttempt>()
					.Where(x => targetAccreditation == null || targetAccreditation.PK == x.HAA_HAC).ToArray();
				foreach (var attempt in accreditationAttempts)
				{
					attempt.Delete();
					if (attempt.Factory != person.Factory)
					{
						attempt.Factory.Save();
					}
				}

				person.Factory.Save();
			}
		}
	}
}
