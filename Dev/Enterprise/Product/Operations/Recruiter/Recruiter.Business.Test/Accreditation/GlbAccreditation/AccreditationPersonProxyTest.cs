using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(AccreditationPersonProxy))]
	sealed class AccreditationPersonProxyTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccreditationPersonProxy(Factory.New<GlbAccreditation>(), Factory.New<GlbPerson>());
		}
	}
}
