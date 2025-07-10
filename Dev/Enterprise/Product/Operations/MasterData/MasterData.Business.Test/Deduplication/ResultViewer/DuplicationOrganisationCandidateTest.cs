using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Test
{
	[TestedType(typeof(DuplicationOrganisationCandidate))]
	public class DuplicationOrganisationCandidateTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DuplicationOrganisationCandidate(null, null);
		}
	}
}
