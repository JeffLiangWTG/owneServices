using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(AMSDocAddressDependentCollection))]
	class AMSDocAddressDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AMSDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JobDocAddress>();
		}
	}
}
