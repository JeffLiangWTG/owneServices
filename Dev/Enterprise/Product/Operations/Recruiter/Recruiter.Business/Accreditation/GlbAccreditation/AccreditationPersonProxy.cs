using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class AccreditationPersonProxy : NonPersistentBusinessObject, IAccreditationPersonProxy
	{
		public IGlbAccreditation Accreditation { get; }
		public GlbPerson Person { get; }

		public AccreditationPersonProxy(GlbAccreditation accreditation, GlbPerson person)
		{
			Accreditation = accreditation;
			Person = person;
		}

		IGlbAccreditationAttempt attempt;
		bool triedToFindAttempt;
		public IGlbAccreditationAttempt Attempt
		{
			get
			{
				if (attempt == null && !triedToFindAttempt)
				{
					triedToFindAttempt = true;
					attempt = Person.AccreditationAttemptCollection.Cast<IGlbAccreditationAttempt>()
						.OrderByDescending(at => at.HAA_CompletionDate).FirstOrDefault(a => a.HAA_HAC == Accreditation.PK);
				}

				return attempt;
			}
		}
	}
}
