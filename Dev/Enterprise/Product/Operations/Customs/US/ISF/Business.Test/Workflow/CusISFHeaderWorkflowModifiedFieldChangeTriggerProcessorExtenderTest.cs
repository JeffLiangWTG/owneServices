using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFHeaderWorkflowModifiedFieldChangeTriggerProcessorExtenderTest : TestCaseWithFactory
	{
		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange()
		{
			TestWorkflowDescriptors.Install(new TestWorkflowDescriptors());
			var header = Factory.New<CusISFHeader>();
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(CusISFHeaderSchema.BF_CustomsStatus);
			var fieldTrigger = header.WorkflowItems.Triggers.AddNew();
			fieldTrigger.TriggerConditions.TriggerFieldName = CusISFHeaderSchema.BF_CustomsStatus.Name;
			var fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
			fieldNotification.PQ_TriggerType = "XML";
			Factory.Save();
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));
			var processor = ObjectFactory.Get<IProcessorTest>("WorkflowServiceTaskTester");
			processor.Process(null);
			Factory.Save();
			fieldNotification.Reload();
			AssertEquals("Trigger fired when bill field modified", "Trigger Fired", fieldNotification.PQ_EmailText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_Description);
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
		}

		sealed class TestWorkflowDescriptors : WorkflowDescriptors
		{
			public TestWorkflowDescriptors()
			{
				AddDescriptor(new TestJobConsolWorkflowDescriptor());
			}

			public static void Install(TestWorkflowDescriptors instance)
			{
				OverridableNewDelegate.Value = () => instance;
			}
		}

		sealed class TestJobConsolWorkflowDescriptor : WorkflowDescriptor
		{
			public int WorkflowTriggerActionRunCount;

			public override string Code => JobInvoicingConsumerTypes.ImporterSecurityFiling.Code;

			public override IMultilingualString Description => (NoResString)"";

			public override ControllerID ControllerID => ControllerIDs.ImporterSecurityFiling;

			public override Type WorkflowProviderType => typeof(CusISFHeader);

			protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source) => new DummyWorkflowTriggerProcessor(this, source.Action);

			sealed class DummyWorkflowTriggerProcessor : IProcessor
			{
				public DummyWorkflowTriggerProcessor(TestJobConsolWorkflowDescriptor owner, ProcessTaskNotification action)
				{
					this.owner = owner;
					this.action = action;
				}

				void IProcessor.Process(INotifications notifications, CancellationToken token)
				{
					owner.WorkflowTriggerActionRunCount++;
					action.PQ_EmailText = "Trigger Fired";
				}

				readonly TestJobConsolWorkflowDescriptor owner;
				readonly ProcessTaskNotification action;
			}
		}
	}
}
