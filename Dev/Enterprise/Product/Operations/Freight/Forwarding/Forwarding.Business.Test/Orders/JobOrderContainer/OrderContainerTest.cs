using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[TestedType(typeof(OrderContainer))]
	sealed class OrderContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCountReadOnlyOnContainerNumEntered()
		{
			AssertEquals(
				"No container number initially, count should be writable",
				false, BO.J1_ContainerCountInfo.ReadOnly);

			BO.J1_ContainerNumber = "a";
			AssertEquals(
				"You shouldn't be able to change the count when container number is entered",
				true, BO.J1_ContainerCountInfo.ReadOnly);

			BO.J1_ContainerNumber = "";
			AssertEquals(
				"No container number, count should be writable",
				false, BO.J1_ContainerCountInfo.ReadOnly);

			BO.J1_ContainerNumber = "a"; // make count read-only again
			Factory.Save();
			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory();

			OrderContainer retrievedBO = retrievingFactory.Load<OrderContainer>(BO.PK);
			AssertEquals("Should still be read only when re-loaded", true, retrievedBO.J1_ContainerCountInfo.ReadOnly);
		}

		public void TestCountGoes1WhenEnterContainerNumber()
		{
			BO.J1_ContainerCount = 2;
			BO.J1_ContainerNumber = "x";
			AssertEquals("Only 1 container permitted when entering a container number", new ZShort((short)1), BO.J1_ContainerCount);
		}

		public void TestClone()
		{
			OrderContainer container = Factory.New<OrderContainer>();
			container.J1_ContainerNumber = "123";
			OrderContainer clonedContainer = (OrderContainer)container.Clone();
			AssertEquals("OrderContainer cloned correctly", "123", clonedContainer.J1_ContainerNumber);
		}

		#region Implementation

		OrderContainer BO;

		protected override void SetUp()
		{
			base.SetUp();
			BO = Factory.NewWithValidTestData<OrderContainer>();
		}

		#endregion
	}
}
