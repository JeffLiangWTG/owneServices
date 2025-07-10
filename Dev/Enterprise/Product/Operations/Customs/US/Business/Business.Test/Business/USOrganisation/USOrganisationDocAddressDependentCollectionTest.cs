using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USOrganisationDocAddressDependentCollection))]
	sealed class USOrganisationDocAddressDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new USOrganisationDocAddressDependentCollection(Factory.New<JobComInvoiceHeader>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<USOrganisationDocAddress>();
		}
	}
}
