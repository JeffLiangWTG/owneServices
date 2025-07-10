using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GenerateMultiOrderPickActionMethod))]
	public class GenerateMultiOrderPickActionMethodTest : OperationalActionMethodTest<GenerateMultiOrderPickActionMethod>
	{
		public void TestNameValue()
		{
			AssertEquals("Generate Multi-Order Pick", new GenerateMultiOrderPickActionMethod().Name);
		}

		public void TestDescriptionValue()
		{
			AssertEquals("Generate Pick For All Selected Orders", new GenerateMultiOrderPickActionMethod().Description);
		}

		public void TestNewApplicator_RegistersFactoryInIFactoryService()
		{
			var mock = new Mock<IFactoryService>();
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertType<GenerateMultiOrderPickActionMethodApplicator>(new GenerateMultiOrderPickActionMethod().NewApplicator(Factory, new DummyOperationalActionMethodSettings()));
				mock.Verify(f => f.RegisterFactory(It.Is<Func<BusinessObjectFactory>>(func => func() != Factory && func() != null && func() != func())));
			}
		}

		protected override GenerateMultiOrderPickActionMethod NewMethod()
		{
			return new GenerateMultiOrderPickActionMethod();
		}
	}
}
