using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionHeaderFiscalization))]
	public class AccTransactionHeaderFiscalizationTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return (AccTransactionHeaderFiscalization)base.GetNewBusinessObjectForDeleteTest(factory);
		}
	}
}
