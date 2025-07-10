using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	abstract class FreightWorkflowModifiedFieldChangeTriggerProcessorExtenderTest : TestCaseWithFactory
	{
		protected void TestFireTriggerOnFieldChange(bool expectedFired, bool isLinked, ZPropertyInfo propertyToChange, SchemaColumn transportProperty, bool hasOriginalValue = true, string triggerCondition = null, string triggerConditionValue = null)
		{
			if (hasOriginalValue)
			{
				propertyToChange.Value = (propertyToChange.PropertyType == typeof(ZDateTime)) ? ZDateTime.Now.AddDays(-1) : (ZString)"v1";
			}
			else
			{
				propertyToChange.Value = (propertyToChange.PropertyType == typeof(ZDateTime)) ? ZDateTime.Empty : ZString.Empty;
			}

			TestWorkflowDescriptors.Install(new TestWorkflowDescriptors());
			Consol.Factory.Save();

			if (!isLinked)
			{
				ConsolTransport.JW_IsLinked = isLinked;
			}

			ProcessTask fieldTrigger = ((IWorkflowProvider)Consol).WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = (TestWorkflowDescriptor)fieldTrigger.WorkflowDescriptor;
			workflowDescriptor.WorkflowTriggerFieldColumnsExposed.Add(transportProperty);
			fieldTrigger.TriggerConditions.TriggerFieldName = transportProperty.Name;

			if (triggerCondition != null && triggerConditionValue != null)
			{
				fieldTrigger.TriggerConditions.TriggerCondition = triggerCondition;
				fieldTrigger.TriggerConditions.TriggerConditionValue = triggerConditionValue;
			}

			ProcessTaskNotification fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
			fieldNotification.PQ_TriggerType = "XML";

			Consol.Factory.Save();

			propertyToChange.Value = (propertyToChange.PropertyType == typeof(ZDateTime)) ? ZDateTime.Now : (ZString)"v2";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			Processor.Process(null);
			Factory.Save();

			fieldNotification.Reload();

			AssertEquals("Trigger fired when sailing field modified", expectedFired ? (ZString)"Trigger Fired" : ZString.Empty, fieldNotification.PQ_EmailText);
		}

		protected void TestFireTriggerOnFieldChange(bool expectedFired, IWorkflowProvider workflowProvider, ZPropertyInfo propertyToChange, SchemaColumn propertyTriggeringChange, bool hasOriginalValue = true)
		{
			if (hasOriginalValue)
			{
				propertyToChange.Value = (propertyToChange.PropertyType == typeof(ZDateTime)) ? ZDateTime.Now.AddDays(-1) : (ZString)"v1";
			}
			else
			{
				propertyToChange.Value = (propertyToChange.PropertyType == typeof(ZDateTime)) ? ZDateTime.Empty : ZString.Empty;
			}

			Consol.Factory.Save();

			TestWorkflowDescriptors.Install(new TestWorkflowDescriptors());
			ProcessTask fieldTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			var workflowDescriptor = (TestWorkflowDescriptor)fieldTrigger.WorkflowDescriptor;
			workflowDescriptor.WorkflowTriggerFieldColumnsExposed.Add(propertyTriggeringChange);
			fieldTrigger.TriggerConditions.TriggerFieldName = propertyTriggeringChange.Name;
			ProcessTaskNotification fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
			fieldNotification.PQ_TriggerType = "XML";

			((BusinessObject)workflowProvider).Factory.Save();

			propertyToChange.Value = (propertyToChange.PropertyType == typeof(ZDateTime)) ? ZDateTime.Now : (ZString)"v2";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			Processor.Process(null);
			Factory.Save();

			fieldNotification.Reload();

			if (expectedFired)
			{
				AssertEquals(string.Format("Trigger fired when {0} field modified", propertyToChange.Name),
					(ZString)"Trigger Fired", fieldNotification.PQ_EmailText);
			}
			else
			{
				AssertEquals(string.Format("Trigger not fired when {0} field modified", propertyToChange.Name),
					ZString.Empty, fieldNotification.PQ_EmailText);
			}
		}

		public void TestExcludeEmptyProperties()
		{
			var changeLog = Factory.New<StmChangeLog>();
			var change = changeLog.FieldChanges.AddNew();
			change.PropertyName = "JW_VoyageFlight";
			change.NewValue = new ZString("NewValue");

			var extender = new FreightWorkflowModifiedFieldChangedTriggerProcessorExtenderForTest();
			AssertNoExceptionThrown("No null ref exception should be occuring", () => { extender.ExcludeEmptyProperties(changeLog, new string[] { "JW_VoyageFlight" }); });
		}

		#region Test Classes

		protected class TestWorkflowDescriptors : WorkflowDescriptors
		{
			public TestWorkflowDescriptors()
			{
				AddDescriptor(new TestWorkflowDescriptor(JobInvoicingConsumerTypes.Consol.Code, typeof(CommonConsol)));
				AddDescriptor(new TestWorkflowDescriptor(SailingScheduleWorkflowDescriptorCode, typeof(JobVoyage)));
			}

			public static void Install(TestWorkflowDescriptors mock)
			{
				OverridableNewDelegate.Value = () => mock;
			}
		}

		protected class TestWorkflowDescriptor : WorkflowDescriptor
		{
			public TestWorkflowDescriptor(string code, Type workflowProviderType)
			{
				this.code = code;
				this.workflowProviderType = workflowProviderType;
			}

			public int WorkflowTriggerActionRunCount;
			protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
			{
				return new TestWorkflowTriggerProcessor(this, source.Action);
			}

			protected override BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
			{
				AssertEquals(true, workflowProviderType.IsAssignableFrom(businessObject.GetType()));
				return base.GetEventDataModelCore(businessObject);
			}

			public override string Code
			{
				get { return code; }
			}

			readonly string code;

			public override IMultilingualString Description
			{
				get { return (NoResString)""; }
			}

			public override ControllerID ControllerID
			{
				get { return DummyControllerIDs.Dummy; }
			}

			public override Type WorkflowProviderType
			{
				get { return workflowProviderType; }
			}

			readonly Type workflowProviderType;

			public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
			{
				return WorkflowTriggerFieldColumnsExposed.ToArray();
			}

			public readonly List<SchemaColumn> WorkflowTriggerFieldColumnsExposed = new List<SchemaColumn>();
		}

		class TestWorkflowTriggerProcessor : IProcessor
		{
			public TestWorkflowTriggerProcessor(TestWorkflowDescriptor owner, ProcessTaskNotification action)
			{
				this.owner = owner;
				this.action = action;
			}

			void IProcessor.Process(INotifications notifications, CancellationToken unused)
			{
				owner.WorkflowTriggerActionRunCount++;
				action.PQ_EmailText = "Trigger Fired";
			}

			readonly TestWorkflowDescriptor owner;
			readonly ProcessTaskNotification action;
		}

		class FreightWorkflowModifiedFieldChangedTriggerProcessorExtenderForTest : FreightWorkflowModifiedFieldChangeTriggerProcessorExtender
		{
			protected override List<string> GetSqls() => new List<string>();
		}

		#endregion

		#region Implementation

		protected IProcessor Processor
		{
			get { return processor ?? (processor = ObjectFactory.Get<IProcessorTest>("WorkflowServiceTaskTester")); }
		}

		IProcessor processor;

		protected CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = (CommonConsol)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingConsol)));
					consol.JK_TransportMode = Constants.TransportModes.Sea;
					consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "MYPKG";
					consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "AUSYD";
					consol.Transports.MostInterestingTransport.JW_Vessel = "Vessel";
					consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Voyage";
				}
				return consol;
			}
		}
		CommonConsol consol;

		protected Transport ConsolTransport
		{
			get { return Consol.Transports.MostInterestingTransport; }
		}

		protected CommonShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Consol.Shipments.AddNew();
				}
				return shipment;
			}
		}
		CommonShipment shipment;

		protected JobSailing Sailing
		{
			get { return Consol.Transports.MostInterestingTransport.Sailing; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_Description);
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
		}

		protected JobVoyage CreateVoyage(string fromUnloco, string toUnloco)
		{
			var voyage = Factory.New<JobVoyage>();

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = fromUnloco;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = toUnloco;

			voyage.GenerateSailings();

			return voyage;
		}

		#endregion
	}
}
