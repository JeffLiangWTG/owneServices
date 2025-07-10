using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Recruiter;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptTreeModel))]
	sealed class GlbAccreditationAttemptTreeModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new GlbAccreditationAttemptTreeModel(Factory.New<IGlbAccreditationAttempt>(), Factory.New<GlbPerson>());
		}
	}
}
