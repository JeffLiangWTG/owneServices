using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.Management.Matching;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Matching.Testing
{
	/// <summary>
	/// The testing for the OrganizationAddressMatcher and associated classes is done down in the core UniversalDataBuss 
	/// code and out in the modules implementing the matching in their real business case tests.
	/// </summary>
	class OrganizationAddressMatcherTest : TestCaseWithFactory
	{
		public void TestCanBeConstructedWithSpring()
		{
			var organizationAddressMatcher = ObjectFactory.New<IOrganizationAddressMatcher>();
			AssertNotNull("organizationAddressMatcher", organizationAddressMatcher);
			AssertEquals("organizationAddressMatcher.GetType()", typeof(OrganizationAddressMatcher), organizationAddressMatcher.GetType());
		}
	}
}
