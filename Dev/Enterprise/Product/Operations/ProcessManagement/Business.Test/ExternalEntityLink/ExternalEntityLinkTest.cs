using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ExternalEntityLink))]
	class ExternalEntityLinkTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForTests(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObjectForTests(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForTests(Factory);
		}

		static BusinessObject GetNewBusinessObjectForTests(BusinessObjectFactory factory)
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(factory);
			var linkable = new DummyExternalEntityLinkable("12345", "WKI");
			return ExternalEntityLinkHelper.CreateLink(linkable, workItem, "SYS");
		}
	}
}
