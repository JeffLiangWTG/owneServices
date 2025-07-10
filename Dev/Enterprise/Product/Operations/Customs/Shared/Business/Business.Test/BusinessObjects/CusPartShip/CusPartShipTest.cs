using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPartShip))]
	sealed class CusPartShipTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CusPartShip>();
		}
	}
}
