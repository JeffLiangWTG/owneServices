using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusContainerTestCase : TestCaseWithFactory
	{
		public void TestJobContainer()
		{
			var cusContainer = Factory.New<BaseCusContainer>();
			var jobContainer = Factory.New<ForwardingContainer>();
			AssertEquals("There should be no JobContainer against the container yet.", true, cusContainer.CO_JC.IsEmpty);

			cusContainer.CO_JC = jobContainer.PK;
			AssertEquals("There should be a JobContainer against the container now.", jobContainer, cusContainer.JobContainer);

			cusContainer.CO_JC = ZGuid.Empty;
			AssertEquals("The previously assigned record should be deleted if there are no other connection.", true, jobContainer.IsDeleted);
		}

		public void TestJobContainerTriggerHasChanges()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_JC = ZGuid.Empty;
			cusContainer.HasChanges = false;
			_ = cusContainer.JobContainer;
			AssertEquals("get_JobContainer should trigger HasChanges", true, cusContainer.HasChanges);

			var uncommittedCusContainer = (BaseCusContainer)((IBindingList)declaration.CusContainers).AddNew();
			uncommittedCusContainer.CO_JC = ZGuid.Empty;
			uncommittedCusContainer.HasChanges = false;
			_ = uncommittedCusContainer.JobContainer;
			AssertEquals("get_JobContainer should not trigger HasChanges for uncommitted CusContainer", false, uncommittedCusContainer.HasChanges);
		}

		public void TestJobContainerChildrenChangeTriggerHasChanges()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Factory.Save();

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_JC = ZGuid.Empty;
			cusContainer.HasChanges = false;
			cusContainer.CO_ContainerNumber = "123";
			var cusContainerJobContainer = cusContainer.JobContainer;
			AssertEquals("get_JobContainer should trigger HasChanges", true, cusContainer.HasChanges);
			AssertEquals("get_JobContainer should trigger HasChanges on JobContainer", true, cusContainerJobContainer.HasChanges);

			var uncommittedCusContainer = (BaseCusContainer)((IBindingList)declaration.CusContainers).AddNew();
			uncommittedCusContainer.CO_JC = ZGuid.Empty;
			uncommittedCusContainer.HasChanges = false;
			var uncommittedCusContainerJobContainer = uncommittedCusContainer.JobContainer;
			AssertEquals("add container to declaration.RelevantConsol should not trigger HasChanges for uncommitted JobContainer", false, uncommittedCusContainerJobContainer.HasChanges);
			AssertEquals("add container to declaration.RelevantConsol should not trigger HasChanges for uncommitted CusContainer", false, uncommittedCusContainer.HasChanges);
		}

		public void TestJobContainerDeletedWhenNotYetInDatabase()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var container = dec.CusContainers.AddNew();
			var jobContainer = container.JobContainer;
			container.Delete();
			AssertEquals(true, jobContainer.IsDeleted);
		}

		public void TestCloneOfCustomsContainerCreatesNewJobContainer()
		{
			var container = Factory.New<BaseCusContainer>();
			var jobContainer = Factory.New<ForwardingContainer>();
			container.CO_JC = jobContainer.PK;

			container.CO_ContainerNumber = "OOCL0000011";
			jobContainer.JC_RefrigGeneratorID = "LALALA";

			var clonedContainer = (BaseCusContainer)container.Clone();
			AssertEquals("clonedContainer.CO_ContainerNumber", "OOCL0000011", clonedContainer.CO_ContainerNumber);

			var clonedJobContainer = clonedContainer.JobContainer;
			AssertEquals("clonedJobContainer.JC_RefrigGeneratorID", "LALALA", clonedJobContainer.JC_RefrigGeneratorID);

			AssertNotEquals("clonedContainer.PK", container.PK, clonedContainer.PK);
			AssertNotEquals("clonedJobContainer.PK", jobContainer.PK, clonedJobContainer.PK);
		}
	}
}
