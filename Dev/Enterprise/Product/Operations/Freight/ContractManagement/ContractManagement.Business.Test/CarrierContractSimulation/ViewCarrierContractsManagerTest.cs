using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(ViewCarrierContractsManager))]
	sealed class ViewCarrierContractsManagerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ViewCarrierContractsManager(Factory, new Mock<IContractSimulationFormConfiguration>().Object);
		}
	}
}
