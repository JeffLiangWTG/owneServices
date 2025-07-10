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
	[TestedType(typeof(CancelPickActionMethod))]
	public class CancelPickActionMethodTest : OperationalActionMethodTest<CancelPickActionMethod>
	{
		public void TestNameValue()
		{
			AssertEquals("Cancel Picks", new CancelPickActionMethod().Name);
		}

		public void TestDescriptionValue()
		{
			AssertEquals("Cancel Picks", new CancelPickActionMethod().Description);
		}

		public void TestNewApplicator_RegistersFactoryInIFactoryService()
		{
			var mock = new Mock<IFactoryService>();
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertType<CancelPickApplicator>(new CancelPickActionMethod().NewApplicator(Factory, new DummyOperationalActionMethodSettings()));
				mock.Verify(f => f.RegisterFactory(It.Is<Func<BusinessObjectFactory>>(func => func() != Factory && func() != null && func() != func())));
			}
		}

		protected override CancelPickActionMethod NewMethod() => new CancelPickActionMethod();
	}
}
