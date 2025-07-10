using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionHeaderReference))]
	sealed class AccTransactionHeaderReferenceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return (AccTransactionHeaderReference)base.GetNewBusinessObjectForDeleteTest(factory);
		}
	}
}
