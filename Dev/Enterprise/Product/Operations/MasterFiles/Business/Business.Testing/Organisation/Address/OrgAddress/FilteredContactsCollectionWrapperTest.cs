using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(FilteredContactsCollectionWrapper))]
	sealed class FilteredContactsCollectionWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new OrgContactCollection(Factory);
			return new FilteredContactsCollectionWrapper(collection);
		}
	}
}
