using CargoWise.EntityFramework;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(YardUnit))]
	sealed class YardUnitTest : EnterpriseBusinessObjectTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		#endregion
	}
}
