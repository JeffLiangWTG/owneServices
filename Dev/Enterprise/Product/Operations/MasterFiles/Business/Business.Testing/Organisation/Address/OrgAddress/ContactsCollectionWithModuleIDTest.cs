using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(FilteredContactsCollectionWrapper.ContactsCollectionWithModuleID))]
	sealed class ContactsCollectionWithModuleIDTest : BusinessObjectCollectionViewTestCase<FilteredContactsCollectionWrapper.ContactsCollectionWithModuleID>
	{
		protected override FilteredContactsCollectionWrapper.ContactsCollectionWithModuleID GetCollectionToTest()
		{
			return new FilteredContactsCollectionWrapper.ContactsCollectionWithModuleID(new OrgContactCollection(Factory), typeof(OrgContact));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<OrgContact>();
		}
	}
}
