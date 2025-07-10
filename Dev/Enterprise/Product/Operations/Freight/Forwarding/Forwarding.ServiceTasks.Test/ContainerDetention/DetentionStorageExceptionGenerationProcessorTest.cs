using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Test
{
	internal class DetentionStorageExceptionGenerationProcessorTest : TestCaseWithFactory
	{
		[TestDate(2000, 1, 2, 0, 0, 0)]
		public void TestCreateConsolForMilestoneException()
		{
			ConsolContainer[JobContainerSchema.JC_JK] = ForwardingConsol.PK;
			BusinessObject consol = (BusinessObject)Factory.Load<Enterprise.Integration.Forwarding.IForwardingConsol>(ForwardingConsol.PK);
			Factory.Save();
			Processor.Process(new NotificationBuffer());
			AssertEquals("1 exception generated for Container Consolidation Detention Passed  ", 1, ((IWorkflowProvider)consol).WorkflowItems.Exceptions.Count);
			AssertEquals("1 exception generated for Container Consolidation Detention Passed  ", ProcessWorkflowExceptionType.ExceptionContainerDetention, ((IWorkflowProvider)consol).WorkflowItems.Exceptions[0].P9_SE_NKExceptionEvent);
			Processor.Process(new NotificationBuffer());
			AssertEquals("No additional exception generated", 1, ((IWorkflowProvider)consol).WorkflowItems.Exceptions.Count);
		}

		[TestDate(2000, 1, 2, 0, 0, 0)]
		public void TestCreateBrokerageForMilestoneException()
		{
			CusContainer[CusContainerSchema.CO_JE] = JobDeclaration.PK;
			CusContainer[CusContainerSchema.CO_JC] = BrokerageContainer.PK;
			BusinessObject declaration = (BusinessObject)Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(JobDeclaration.PK);
			Factory.Save();
			Processor.Process(new NotificationBuffer());
			AssertEquals("1 exception generated for Container Brokerage Detention Passed  ", 1, ((IWorkflowProvider)declaration).WorkflowItems.Exceptions.Count);
			AssertEquals("1 exception generated for Container Brokerage Detention Passed  ", ProcessWorkflowExceptionType.ExceptionContainerDetention, ((IWorkflowProvider)declaration).WorkflowItems.Exceptions[0].P9_SE_NKExceptionEvent);
			Processor.Process(new NotificationBuffer());
			AssertEquals("No additional exception generated", 1, ((IWorkflowProvider)declaration).WorkflowItems.Exceptions.Count);
		}

		[TestDate(2000, 1, 2, 0, 0, 0)]
		public void TestWeDoNotCreateBrokerageWithShipmentForMilestoneException()
		{
			CusContainer[CusContainerSchema.CO_JE] = JobDeclaration.PK;
			CusContainer[CusContainerSchema.CO_JC] = BrokerageContainer.PK;
			JobDeclaration[JobDeclarationSchema.JE_JS] = Shipment.PK;
			BusinessObject declaration = (BusinessObject)Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(JobDeclaration.PK);
			Factory.Save();
			Processor.Process(new NotificationBuffer());
			AssertEquals("No exception generated for Container Brokerage Detention Passed  ", 0, ((IWorkflowProvider)declaration).WorkflowItems.Exceptions.Count);
			AssertEquals("No exception generated for Related Shipment Detention Passed  ", 0, ((IWorkflowProvider)Shipment).WorkflowItems.Exceptions.Count);
			Processor.Process(new NotificationBuffer());
			AssertEquals("No additional exception generated", 0, ((IWorkflowProvider)declaration).WorkflowItems.Exceptions.Count);
			AssertEquals("No additional exception generated", 0, ((IWorkflowProvider)Shipment).WorkflowItems.Exceptions.Count);
		}

		#region Generator Overrides

		public void TestContainerExceptionEvent()
		{
			AssertEquals("Event", ProcessWorkflowExceptionType.ExceptionContainerDetention, new ContainerDetentionExceptionGeneratorForTesting().ContainerExceptionEvent);
		}

		public void TestHighWaterMarkRegistryItem()
		{
			ForwardingConfigurationRegistry.Instance.ContainerDetentionExceptionGeneratorHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2011, 04, 04));
			AssertEquals("HighWaterMarkRegistryItem", new DateTime(2011, 04, 04), new ContainerDetentionExceptionGeneratorForTesting().HighWaterMarkRegistryItem.Value);
		}

		#endregion Generator Overrides

		#region Objects

		BusinessObject ForwardingConsol
		{
			get
			{
				if (consol == null)
				{
					consol = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
					consol[JobConsolSchema.JK_IsForwarding] = ZBool.True;
				}

				return consol;
			}
		}

		BusinessObject consol;

		CommonContainer ConsolContainer
		{
			get
			{
				if (consolcontainer == null)
				{
					consolcontainer = Factory.New<CommonContainer>();
					consolcontainer.JC_JK = ForwardingConsol.PK;
					consolcontainer.JC_EmptyReturnedBy = ZDateTime.Now.AddDays(-1);
					consolcontainer.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Empty;
				}

				return consolcontainer;
			}
		}

		CommonContainer consolcontainer;

		BusinessObject JobDeclaration
		{
			get
			{
				if (jobdeclaration == null)
				{
					jobdeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					jobdeclaration[JobDeclarationSchema.JE_IsCancelled] = ZBool.False;
				}

				return jobdeclaration;
			}
		}

		BusinessObject jobdeclaration;

		CommonShipment Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<ForwardingShipment>());
			}
		}

		CommonShipment shipment;

		CommonContainer BrokerageContainer
		{
			get
			{
				if (brokeragecontainer == null)
				{
					brokeragecontainer = Factory.New<CommonContainer>();
					brokeragecontainer.JC_EmptyReturnedBy = ZDateTime.Now.AddDays(-1);
					brokeragecontainer.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Empty;
				}

				return brokeragecontainer;
			}
		}

		CommonContainer brokeragecontainer;

		BusinessObject CusContainer
		{
			get
			{
				return cuscontainer ?? (cuscontainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>());
			}
		}

		BusinessObject cuscontainer;

		#endregion Objects

		#region Implementation

		ContainerDetentionExceptionGenerator Processor
		{
			get
			{
				return processor ?? (processor = new ContainerDetentionExceptionGenerator());
			}
		}

		ContainerDetentionExceptionGenerator processor;

		class ContainerDetentionExceptionGeneratorForTesting : ContainerDetentionExceptionGenerator
		{
			public new string ContainerExceptionEvent
			{
				get
				{
					return base.ContainerExceptionEvent;
				}
			}

			public new DateTimeRegistryItem HighWaterMarkRegistryItem
			{
				get
				{
					return base.HighWaterMarkRegistryItem;
				}
			}
		}

		#endregion Implementation
	}
}
