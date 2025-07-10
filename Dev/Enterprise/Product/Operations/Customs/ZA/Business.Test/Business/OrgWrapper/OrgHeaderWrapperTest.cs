using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(OrgHeaderWrapper))]
	sealed class OrgHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			return OrgHeaderWrapper.New(org);
		}
	}
}
