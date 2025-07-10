using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.Shared
{
	[TestedType(typeof(Contact))]
	class ContactTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new Contact()
			{
				FullName = "who",
				Phone = "123456789"
			};
		}
	}
}
