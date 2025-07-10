using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Test
{
	[TestedType(typeof(DuplicationCandidate))]
	public class DuplicationCandidateTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DuplicationOrganisationCandidate(null, null);
		}
	}
}
