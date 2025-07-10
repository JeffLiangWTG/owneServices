using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Recruiter;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbAccreditationTreeModel))]
	sealed class GlbAccreditationTreeModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new GlbAccreditationTreeModel(Factory.New<IGlbAccreditation>());
		}
	}
}
