using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.TransportConsignment.ProductionRulesEngine.Testing
{
	public abstract class ValueDefaultingManagerTest : TestCaseWithFactory
	{
		protected abstract ValueDefaultingManager GetDefaultingManager(ILandTransportFactLoaderProvider factLoaderProvider);
		protected abstract string Context { get; }
		protected abstract Guid PK1 { get; }
		protected abstract Guid PK2 { get; }

		public void TestNullEntity_NoDefaultValueSet()
		{
			var factLoaderProviderMock = new Mock<ILandTransportFactLoaderProvider>();
			var defaultingManager = GetDefaultingManager(factLoaderProviderMock.Object);

			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranch>();
			var departmentMock = new Mock<IDepartment>();
			defaultingManager.FetchDefaultValue(null, companyMock.Object, branchMock.Object, departmentMock.Object);

			AssertNull(defaultingManager.DefaultValue);
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
