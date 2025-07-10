using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Reflection;
using AppDomainWrappers.Net;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ProcessTask_ProcessTaskNotificationLookupsTest : ProcessTaskNotificationLookupsTestCase<ProcessTask>
	{
		protected override ProcessTask CreateWorkflowItem(IWorkflowProvider job)
		{
			return job.WorkflowItems.Triggers.AddNew();
		}

		protected override ProcessTask CreateWorkflowItem(ProcessTaskTemplate template)
		{
			return template.WorkflowItems.Triggers.AddNew();
		}
	}

	class ProcessTemplateTrigger_ProcessTaskNotificationLookupsTest : ProcessTaskNotificationLookupsTestCase<ITemplateTrigger>
	{
		protected override ITemplateTrigger CreateWorkflowItem(IWorkflowProvider job)
		{
			return job.WorkflowItems.Triggers.AddNew();
		}

		protected override ITemplateTrigger CreateWorkflowItem(ProcessTaskTemplate template)
		{
			var trigger = template.TemplateTriggers.AddNew();
			trigger.FillWithValidTestData();

			return (ITemplateTrigger)trigger;
		}
	}

	[UseSnapshotProtection]
	public class ProcessTaskWiringTests : TestCase
	{
		public void TestAllWorkflowProvidersHaveCorrectlyWiredProcessTasks()
		{
			var errors = new List<string>();
			var saveFactory = new BusinessObjectFactory();

			var processTaskAmnesties = new HashSet<string>
			{
				"ContainerMovementProcessTask",
				"AgencyContainerProcessTask",
				"DailyNoticeStatementProcessTask",
				"GlbGroupProcessTask",
				"CusHAWBProcessTask",
				"OrgPartRelationProcessTask",
				"CusSCAHouseProcessTask",
			};

			foreach (var (provider, descriptor) in WorkflowProviderDescriptorGatherer.GetAllWorkflowProvidersAndDescriptors(() => { saveFactory = new BusinessObjectFactory(); return saveFactory; }, errors))
			{
				var workflowProcessTask = provider.WorkflowItems.AddNew();
				var workflowProcessTaskType = workflowProcessTask.GetType();
				var loadFactory = new BusinessObjectFactory();
				loadFactory.RefreshEnabled = false;

				if (processTaskAmnesties.Contains(workflowProcessTaskType.Name))
				{
					continue;
				}

				using (((IDbConnected)saveFactory).Connection.BeginTransactionWithManager())
				{
					try
					{
						saveFactory.Save();
					}
					catch (Exception)
					{
						// If we can't save we can't test, so we move on
						continue;
					}

					var query = new ZQuery();
					query.TableHints = TableHints.NOLOCK;
					query.AddToFilter(ProcessTasksSchema.PK, workflowProcessTask.PK);
					AssertEquals($"{workflowProcessTaskType.FullName} could not be loaded. Please check to ensure the relevant entry has been added to WorkflowDescriptorsConfiguration.xml and ProcessTaskTypesConfiguration.xml", workflowProcessTaskType, loadFactory.Load<ProcessTask>(query).First().GetType());
				}
			}

			AssertEquals(string.Join("\n", errors), 0, errors.Count);
		}

		public void TestAllWorkflowProvidersDeleteAllProcessTasks()
		{
			var errors = new List<string>();
			var saveFactory = new BusinessObjectFactory();

			var processTaskAmnesties = new HashSet<string>
			{
				"ContainerMovementProcessTask",
				"AgencyContainerProcessTask",
				"DailyNoticeStatementProcessTask",
				"GlbGroupProcessTask",
				"CusHAWBProcessTask",
				"OrgPartRelationProcessTask",
				"CusSCAHouseProcessTask"
			};

			foreach (var (provider, descriptor) in WorkflowProviderDescriptorGatherer.GetAllWorkflowProvidersAndDescriptors(() => { saveFactory = new BusinessObjectFactory(); return saveFactory; }, errors))
			{
				if (provider.WorkflowType != descriptor.Code)
				{
					continue; // this must be a line trigger type on BusinessObject that also happen to support IWorkflowProvider with different code
				}
				var aTrigger = provider.WorkflowItems.Triggers.AddNew();
				var aMilestone = provider.WorkflowItems.Milestones.AddNew();
				var aTask = provider.WorkflowItems.Tasks.AddNew();
				var workflowProcessTaskType = aTask.GetType();
				var loadFactory = new BusinessObjectFactory();
				loadFactory.RefreshEnabled = false;

				if (processTaskAmnesties.Contains(workflowProcessTaskType.Name))
				{
					continue;
				}

				using (((IDbConnected)saveFactory).Connection.BeginTransactionWithManager())
				{
					try
					{
						((BusinessObject)provider).Delete();

						if (!aTrigger.IsDeleted || !aMilestone.IsDeleted || !aTask.IsDeleted)
						{
							errors.Add($"{provider.GetType().FullName}: Trigger deleted = {aTrigger.IsDeleted}, Milestone deleted = {aMilestone.IsDeleted}, Task deleted = {aTask.IsDeleted}");
						}
					}
					catch (Exception ex)
					{
						errors.Add(ex.Message);
					}
				}
			}

			AssertEquals(string.Join("\n", errors), 0, errors.Count);
		}
	}

	abstract class ProcessTaskNotificationLookupsTestCase<TWorkflowItem> : BusinessObjectLookupsTestCase
		where TWorkflowItem : IBaseTrigger
	{
		public void TestStaffList()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "JNC";
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var notification = person.WorkflowItems.Triggers.AddNew().ProcessTaskNotifications.AddNew();
			var lookups = new ProcessTaskNotificationLookups(notification);

			AssertNotNull(lookups.StaffList);
			AssertCollectionContains(staff, lookups.StaffList);
		}

		public void TestWorkflowTriggerActionTypes()
		{
			var emptyLookups = new ProcessTaskNotificationLookups(null);
			AssertEquals("Should return empty CodeDescriptionPairList if Parent is null", 0, emptyLookups.WorkflowTriggerActionTypes.Count);

			var trigger = CreateWorkflowItem(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			var lookups = triggerAction.Lookups;

			trigger.TriggerEventCode = Events.DocumentAllocated.Code;
			lookups = new ProcessTaskNotificationLookups(triggerAction);
			Factory.Save();

			Assert(lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEDocXml));
			Assert(lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc));
			var indexOfXUE = lookups.WorkflowTriggerActionTypes.IndexOf(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalEventXML));
			var indexOfXUD = lookups.WorkflowTriggerActionTypes.IndexOf(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalEventXMLWithEDoc));
			AssertEquals("XUD should be right under XUE", indexOfXUE + 1, indexOfXUD);

			ErrorReporter.Clear();
			trigger.Delete();
			AssertEquals("Should return empty CodeDescriptionPairList if Parent is deleted", 0, lookups.WorkflowTriggerActionTypes.Count);
			AssertNull("No errors were reported during access of WorkflowTriggerActionTypes", ErrorReporter.LastExceptionReported);
		}

		public void TestXMLWithEDocTriggerActionDisabledForNonEventManagingJobs()
		{
			var errors = new List<string>();
			var tables = new List<string>();
			var problemTableCodes = new HashSet<string>();
			var problemWorkflowDecriptors = new HashSet<string>();
			var tableCodesToDescriptors = new Dictionary<ValueTuple<string, string>, HashSet<string>>();

			foreach (var (provider, workFlowDescriptor) in WorkflowProviderDescriptorGatherer.GetAllWorkflowProvidersAndDescriptors(() => Factory, errors))
			{
				TWorkflowItem trigger;
				ZString parentTableCode = "BAD";

				trigger = CreateWorkflowItem(provider);
				parentTableCode = trigger.ParentTableCode;

				ValueTuple<string, string> tableAndCode = ValueTuple.Create(((BusinessObject)provider).TableName, parentTableCode);
				if (!tableCodesToDescriptors.TryGetValue(tableAndCode, out var tableCodeToWorkflowDescriptor))
				{
					tableCodeToWorkflowDescriptor = new HashSet<string>();
					tableCodesToDescriptors.Add(tableAndCode, tableCodeToWorkflowDescriptor);
				}

				tableCodeToWorkflowDescriptor.Add(workFlowDescriptor.Code);

				// Create template based trigger to avoid validation issues
				trigger = CreateWorkflowItem(workFlowDescriptor.Code);

				var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
				trigger.TriggerEventCode = Events.DocumentAllocated.Code;
				var lookups = new ProcessTaskNotificationLookups(triggerAction);
				var managesEvents = workFlowDescriptor.WorkflowProviderType.GetUniversalDataContextManager().ManagesEvents();
				if (managesEvents)
				{
					// This test does not validate the positive case
					continue;
				}

				if (!AssertWorkflowTriggerActionEDocTypes(managesEvents, lookups, workFlowDescriptor.GetType().ToString(), provider.GetType().ToString(), workFlowDescriptor.Code, parentTableCode, ref errors))
				{
					problemTableCodes.Add(parentTableCode);
					tables.Add(((BusinessObject)provider).TableName);
					problemWorkflowDecriptors.Add(workFlowDescriptor.Code);
				}
			}

			if (errors.Count > 0)
			{
				errors.Add(string.Join(", ", problemTableCodes.ToArray()));

				foreach (var tableName in tables)
				{
					errors.Add(tableName);
				}

				string thing = "";
				foreach (var keyValue in tableCodesToDescriptors)
				{
					if (!problemTableCodes.Contains(keyValue.Key.Item2))
					{
						continue;
					}

					thing += $"\n{keyValue.Key.Item1}";
					foreach (var descriptorCode in keyValue.Value.ToArray())
					{
						thing += $"\n    {descriptorCode}";
					}
				}

				errors.Add(thing);
			}

			AssertEquals(string.Join("\n", errors.ToArray()), true, errors.Count == 0);
		}

		bool AssertWorkflowTriggerActionEDocTypes(bool expectedResult, ProcessTaskNotificationLookups lookups, string workflowDescriptorName, string workflowProviderName, string descriptorCode, string parentCode, ref List<string> errors)
		{
			var errorCount = errors.Count;

			if (expectedResult != lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML))
			{
				errors.Add($"WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML for {descriptorCode} with parent code {parentCode} is not {expectedResult}. \t \t WorkflowDescriptor is {workflowDescriptorName}, WorkflowProvider is {workflowProviderName}.");
			}

			if (expectedResult != lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc))
			{
				errors.Add($"WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc for {descriptorCode} with parent code {parentCode} is not {expectedResult}. \t \t WorkflowDescriptor is {workflowDescriptorName}, WorkflowProvider is {workflowProviderName}.");
			}

			if (expectedResult != lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEDocXml))
			{
				errors.Add($"WorkflowTriggerActionTypeConstants.Codes.SendEDocXml for {descriptorCode} with parent code {parentCode} is not {expectedResult}. \t \t WorkflowDescriptor is {workflowDescriptorName}, WorkflowProvider is {workflowProviderName}.");
			}

			return errors.Count == errorCount;
		}

		public void TestGetWorkflowTriggerActionsWhenManagesEventsDisabled()
		{
			Mock<IEventDataContextManager> contextManagerMock = new Mock<IEventDataContextManager>();
			contextManagerMock.Setup(x => x.ManagesEvents).Returns(false);
			Mock<ObjectHandle> objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(x => x.GetObject()).Returns(contextManagerMock.As<IDataContextManager>().Object);

			Hashtable lookUp = new Hashtable
			{
				{ "Organization", objectHandleMock.Object }
			};

			ObjectFactory.Substitute("UniversalDataContextManagers", lookUp);

			var trigger = CreateWorkflowItem(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			var lookups = triggerAction.Lookups;
			trigger.TriggerEventCode = Events.DocumentAllocated.Code;
			lookups = new ProcessTaskNotificationLookups(triggerAction);
			Factory.Save();

			AssertEquals("WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML when Event management is false",
				false,
				lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML));

			AssertEquals("WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc when Event management is false",
				false,
				lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc));

			AssertEquals("WorkflowTriggerActionTypeConstants.Codes.SendEDocXml when Event management is false",
				false,
				lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEDocXml));
		}

		public void TestDocumentToSendList()
		{
			var trigger = CreateWorkflowItem(WorkflowDescriptors.OpportunityWorkflowDescriptorCode);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			AssertEquals("Empty if Parent is null", 0, triggerAction.Lookups.DocumentToSendList.Count);

			var menuItemWithAutoDeliver = CreateMenuItem("test", true);
			var menuItemWithNoAutoDeliver = CreateMenuItem("test1", false);

			Factory.Save();

			var stmMenuItemCollection = triggerAction.Lookups.DocumentToSendList;
			stmMenuItemCollection.Load(new ZQuery(StmMenuItemSchema.PK, menuItemWithAutoDeliver.PK));
			AssertEquals(1, stmMenuItemCollection.Count);

			stmMenuItemCollection.Load(new ZQuery(StmMenuItemSchema.PK, menuItemWithNoAutoDeliver.PK));
			AssertEquals(0, stmMenuItemCollection.Count);

			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;

			stmMenuItemCollection = triggerAction.Lookups.DocumentToSendList;
			stmMenuItemCollection.Load(new ZQuery(StmMenuItemSchema.PK, menuItemWithAutoDeliver.PK));
			AssertEquals(1, stmMenuItemCollection.Count);

			stmMenuItemCollection.Load(new ZQuery(StmMenuItemSchema.PK, menuItemWithNoAutoDeliver.PK));
			AssertEquals(1, stmMenuItemCollection.Count);

			triggerAction.PQ_TriggerParty = "";
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;

			stmMenuItemCollection = triggerAction.Lookups.DocumentToSendList;
			stmMenuItemCollection.Load(new ZQuery(StmMenuItemSchema.PK, menuItemWithAutoDeliver.PK));
			AssertEquals(1, stmMenuItemCollection.Count);

			stmMenuItemCollection.Load(new ZQuery(StmMenuItemSchema.PK, menuItemWithNoAutoDeliver.PK));
			AssertEquals(1, stmMenuItemCollection.Count);

			Assert(stmMenuItemCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Business Context:Property"));
			AssertEquals(nameof(BusinessContext.INVALID), stmMenuItemCollection.FilterBusinessObjectDefaults["Business Context:Property"].Value);
		}

		public void TestDocumentToSendList_WorkflowProviderSpecific()
		{
			var zzzWorkflowDescriptor = new DummySpecificDocumentBusinessContextDescriptorZzz();

			var workflowDescriptors = new Hashtable
			{
				{ DummySpecificDocumentBusinessContextDescriptorZzz.WorkflowCode, new TestObjectHandle(zzzWorkflowDescriptor) }
			};

			using (ObjectFactory.Substitute("WorkflowDescriptors", workflowDescriptors))
			{
				var dummy = Factory.New<DummyWithWorkflowZZZ>();

				bool retreivedBusinessContexts = false;

				zzzWorkflowDescriptor.GetDocumentBusinessContextImpl = workflowProvider =>
				{
					AssertEquals(dummy, workflowProvider);

					retreivedBusinessContexts = true;

					return new[] { BusinessContext.Shipment, BusinessContext.Test };
				};

				var processTask = (DummyProcessTaskZZZ)dummy.WorkflowItems.Triggers.AddNew();
				var notification = processTask.ProcessTaskNotifications.AddNew();
				var stmMenuItemCollection = notification.Lookups.DocumentToSendList;

				Assert("document business contexts were retrived from ISpecificDocumentBusinessContextProvider.GetDocumentBusinessContext", retreivedBusinessContexts);
				AssertEquals("Shipment;Test", stmMenuItemCollection.FilterBusinessObjectDefaults["Business Context:Property"].Value);

				zzzWorkflowDescriptor.GetDocumentBusinessContextImpl = null;
			}
		}

		public void TestDocumentToSendList_FilteredByBusinessContext()
		{
			DummyWorkflowDescriptor.Instance.SetDocumentBusinessContext(new BusinessContext[] { BusinessContext.Shipment, BusinessContext.Consol });

			var trigger = CreateWorkflowItem(DummyWorkflowDescriptor.Instance.Code);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();

			StmMenuItem shipmentMenuItemWithAutoDeliver = CreateMenuItem("SHIPMENT1", nameof(BusinessContext.Shipment), true);
			StmMenuItem shipmentMenuItemWithNoAutoDeliver = CreateMenuItem("SHIPMENT2", nameof(BusinessContext.Shipment), false);
			StmMenuItem consolMenuItemWithAutoDeliver = CreateMenuItem("CONSOL1", nameof(BusinessContext.Consol), false);
			StmMenuItem consolMenuItemWithNoAutoDeliver = CreateMenuItem("CONSOL2", nameof(BusinessContext.Consol), false);
			StmMenuItem cBReportMenuItemWithAutoDeliver = CreateMenuItem("CBReport1", nameof(BusinessContext.CBReport), false);
			StmMenuItem cBReportMenuItemWithNoAutoDeliver = CreateMenuItem("CBReport2", nameof(BusinessContext.CBReport), false);

			ZQuery loadFromMemoryQuery = new ZQuery();
			loadFromMemoryQuery.FetchOnlyFromLocalCache = true;
			StmMenuItemCollection menuItemCollection = triggerAction.Lookups.DocumentToSendList;
			menuItemCollection.LoadWithMoreFiltering(loadFromMemoryQuery);
			AssertEquals(1, menuItemCollection.Count);
			AssertCollectionContains(shipmentMenuItemWithAutoDeliver, menuItemCollection);

			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			menuItemCollection = triggerAction.Lookups.DocumentToSendList;
			menuItemCollection.LoadWithMoreFiltering(loadFromMemoryQuery);
			AssertEquals(4, menuItemCollection.Count);
			AssertCollectionContains(shipmentMenuItemWithAutoDeliver, menuItemCollection);
			AssertCollectionContains(shipmentMenuItemWithNoAutoDeliver, menuItemCollection);
			AssertCollectionContains(consolMenuItemWithAutoDeliver, menuItemCollection);
			AssertCollectionContains(consolMenuItemWithNoAutoDeliver, menuItemCollection);

			string errorMessage = menuItemCollection.GetAllNotificationsWhenAdditionalFilterNotMet(consolMenuItemWithAutoDeliver);
			AssertEquals("This document cannot be selected. Only 'Shipment;Consol' documents can be selected.", errorMessage);
		}

		StmMenuItem CreateMenuItem(string menuName, bool autoDeliver)
		{
			return CreateMenuItem(menuName, "TEST", autoDeliver);
		}

		StmMenuItem CreateMenuItem(string menuName, string businessContext, bool autoDeliver)
		{
			StmMenuItem result = Factory.New<StmMenuItem>();

			result.SU_MenuName = menuName;
			result.SU_BusinessContext = businessContext;
			if (autoDeliver)
			{
				result.SU_PreventAutoDelivery = false;
				result.SU_ContactType = ContactType.Consignee.Code;
			}
			else
			{
				result.SU_PreventAutoDelivery = true;
				result.SU_ContactType = ContactType.NoContactType.Code;
			}

			return result;
		}

		public void TestAlterateLookupsHaveNoEmail()
		{
			var triggerAction = Factory.New<ProcessTaskNotification>();
			var lookups = triggerAction.Lookups;
			AssertEquals(false, lookups.AlternateMessagingTriggerPartiesList.ContainsCode(MessageRecipientPartyTypeList.Codes.Email));
			AssertEquals(false, lookups.AlternateMessagingTriggerPartiesList.ContainsCode(MessageRecipientPartyTypeList.Codes.Print));
			AssertEquals(false, lookups.AlternateMessagingTriggerPartiesList.ContainsCode(MessageRecipientPartyTypeList.Codes.EDICommunication));
		}

		public void TestWorkflowTemplateLookups()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_GC = company.PK;

			var triggerAction = Factory.New<ProcessTaskNotification>();
			var lookups = triggerAction.Lookups;
			AssertEquals(true, lookups.WorkflowTemplateList.Contains(template));
		}

		public void TestMessagingTriggerPartiesList()
		{
			var triggerAction = Factory.New<ProcessTaskNotification>();
			var lookups = triggerAction.Lookups;

			AssertEquals("Should return empty CodeDescriptionPairList if ParentProcessTask is null", 0, lookups.MessagingTriggerPartiesList.Count);
			triggerAction.Delete();

			var trigger = CreateWorkflowItem(WorkflowDescriptors.OpportunityWorkflowDescriptorCode);
			triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			lookups = triggerAction.Lookups;

			var workflowDecriptor = trigger.GetWorkflowDescriptor();

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			AssertEquals(true, workflowDecriptor.IsEmailSendNotificationTriggerAction(triggerAction.PQ_TriggerType));

			var expectedList = new CodeDescriptionPairList();
			var parentList = workflowDecriptor.GetMessagingTriggerPartiesList(triggerAction.PQ_TriggerType, trigger, trigger.GetJob());

			for (int i = 0; i < parentList.Count; i++)
			{
				expectedList.AddPair(MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetCodeFromDescription(parentList[i].Description), parentList[i].Description);
			}

			Factory.Save();
			CodeDescriptionPairList list = lookups.MessagingTriggerPartiesList;
			AssertEquals("Should return MessagingTriggerPartiesList if Parent is not null", expectedList.Count, list.Count);
			for (int i = 0; i < MessageRecipientPartyTypeList.AllPossiblePartyTypes.Count; i++)
			{
				AssertEquals("Should return MessagingTriggerPartiesList if Parent is not null", expectedList[0], list[0]);
			}
			AssertEquals(true, list.ContainsCode(MessageRecipientPartyTypeList.Codes.Email));
			AssertEquals(false, list.ContainsCode(MessageRecipientPartyTypeList.SpecialCodes.Other));

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertEquals(false, workflowDecriptor.IsEmailSendNotificationTriggerAction(triggerAction.PQ_TriggerType));
			list = lookups.MessagingTriggerPartiesList;
			AssertEquals(false, list.ContainsCode(MessageRecipientPartyTypeList.Codes.Email));
			AssertEquals(false, list.ContainsCode(MessageRecipientPartyTypeList.SpecialCodes.Other));

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertEquals(false, workflowDecriptor.IsEmailSendNotificationTriggerAction(triggerAction.PQ_TriggerType));
			list = lookups.MessagingTriggerPartiesList;
			AssertEquals(false, list.ContainsCode(MessageRecipientPartyTypeList.Codes.Email));
			AssertEquals(true, list.ContainsCode(MessageRecipientPartyTypeList.SpecialCodes.Other));

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML;
			AssertEquals(false, workflowDecriptor.IsEmailSendNotificationTriggerAction(triggerAction.PQ_TriggerType));
			list = lookups.MessagingTriggerPartiesList;
			AssertEquals(false, list.ContainsCode(MessageRecipientPartyTypeList.Codes.Email));
			AssertEquals(true, list.ContainsCode(MessageRecipientPartyTypeList.SpecialCodes.Other));

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			AssertEquals(false, workflowDecriptor.IsEmailSendNotificationTriggerAction(triggerAction.PQ_TriggerType));
			list = lookups.MessagingTriggerPartiesList;
			AssertEquals(false, list.ContainsCode(MessageRecipientPartyTypeList.Codes.Email));
			AssertEquals(true, list.ContainsCode(MessageRecipientPartyTypeList.SpecialCodes.Other));
			AssertEquals("Should be cached", list, lookups.MessagingTriggerPartiesList);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;
			AssertEquals(false, workflowDecriptor.IsEmailSendNotificationTriggerAction(triggerAction.PQ_TriggerType));
			list = lookups.MessagingTriggerPartiesList;
			AssertEquals(false, list.ContainsCode(MessageRecipientPartyTypeList.Codes.Email));
			AssertEquals(true, list.ContainsCode(MessageRecipientPartyTypeList.SpecialCodes.Other));

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
			AssertNotEquals("Should be cached", list, lookups.MessagingTriggerPartiesList);
			AssertEquals(false, workflowDecriptor.IsEmailSendNotificationTriggerAction(triggerAction.PQ_TriggerType));
			var list1 = lookups.MessagingTriggerPartiesList;
			AssertEquals(false, list1.ContainsCode(MessageRecipientPartyTypeList.Codes.Email));
			AssertEquals(false, list1.ContainsCode(MessageRecipientPartyTypeList.SpecialCodes.Other));

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			AssertEquals("Should be cached", list, lookups.MessagingTriggerPartiesList);
		}

		public void TestTriggerPartyServices_Job()
		{
			DummyWorkflowDescriptor.Instance.SupportedTriggerPartyServicesExposed = new ZString[] { "ABC", "DEF" };

			var triggerAction_NoParent = Factory.New<ProcessTaskNotification>();
			AssertEquals("Should return empty CodeDescriptionPairList if ParentProcessTask is null", 0, triggerAction_NoParent.Lookups.TriggerPartyServices.Count);

			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItem(dummy);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			AssertEquals("Should return empty CodeDescriptionPairList if has ParentProcessTask, but no SendUniversalShipmentXML TriggerType", 0, triggerAction.Lookups.TriggerPartyServices.Count);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertEquals("Should return empty CodeDescriptionPairList if has ParentProcessTask and SendUniversalShipmentXML, but no Recipient.", 0, triggerAction.Lookups.TriggerPartyServices.Count);

			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			var expectedDummyTriggerPartyServices = new ZString[] { "ABC", "DEF" };
			var actualTriggerPartyServices = triggerAction.Lookups.TriggerPartyServices;
			AssertContainsExactElementsInAnyOrder("Should return Parent's CodeDescriptionPairList if it has ParentProcessTask, SendUniversalShipmentXML and Recipient", expectedDummyTriggerPartyServices, actualTriggerPartyServices.GetAllCodes());
			AssertEquals("TriggerPartyServices should be cached.", actualTriggerPartyServices, triggerAction.Lookups.TriggerPartyServices);
		}

		public void TestTriggerPartyServices_Template()
		{
			DummyWorkflowDescriptor.Instance.SupportedTriggerPartyServicesExposed = new ZString[] { "ABC", "DEF" };

			var emptyTemplate = Factory.New<ProcessTaskTemplate>();
			var emptyTrigger = CreateWorkflowItem(emptyTemplate);
			var emptyTriggerAction = emptyTrigger.CompletionTriggerActionsCollection().AddNew();
			AssertEquals("Should return empty CodeDescriptionPairList if no Valid Template.", 0, emptyTriggerAction.Lookups.TriggerPartyServices.Count);

			var dummyTemplate = Factory.New<ProcessTaskTemplate>();
			dummyTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			dummyTemplate.P0_GC = GlbCompany.CurrentCompany.PK;
			var dummyTrigger = CreateWorkflowItem(dummyTemplate);
			var dummyTriggerAction = dummyTrigger.CompletionTriggerActionsCollection().AddNew();
			AssertEquals("Should return empty CodeDescriptionPairList if has Valid Template Descriptor, but no SendUniversalShipmentXML TriggerType", 0, dummyTriggerAction.Lookups.TriggerPartyServices.Count);

			dummyTriggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertEquals("Should return empty CodeDescriptionPairList if has Valid Template Descriptor and SendUniversalShipmentXML, but no Recipient.", 0, dummyTriggerAction.Lookups.TriggerPartyServices.Count);

			dummyTriggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			var expectedDummyTriggerPartyServices = new ZString[] { "ABC", "DEF" };
			var actualTriggerPartyServices = dummyTriggerAction.Lookups.TriggerPartyServices;
			AssertContainsExactElementsInAnyOrder("Should return Template Descriptor's CodeDescriptionPairList if it has Valid Template Descriptor, SendUniversalShipmentXML and Recipient", expectedDummyTriggerPartyServices, actualTriggerPartyServices.GetAllCodes());
			AssertEquals("TriggerPartyServices should be cached.", actualTriggerPartyServices, dummyTriggerAction.Lookups.TriggerPartyServices);
		}

		public void TestProcessTaskTriggerPurposeList()
		{
			ProcessTaskNotification pTNotification = Factory.NewWithValidTestData<ProcessTaskNotification>();

			var purposes = Factory.Load<IEDIMessagePurpose>(new ZQuery()).OrderBy(d => d.EMP_Code).ToArray();
			var len = purposes.Length;
			AssertEquals("That table be the same", len, pTNotification.Lookups.ProcessTaskTriggerPurposeList.Count - 1);

			AssertEquals("Should add empty string as first element", ZString.Empty, pTNotification.Lookups.ProcessTaskTriggerPurposeList[0].Code);
			for (int i = 0; i < len; i++)
			{
				AssertEquals("Should return ProcessTaskTriggerPurposeList from registry and add empty string as first element", purposes[i].EMP_Code, pTNotification.Lookups.ProcessTaskTriggerPurposeList[i + 1].Code);
				AssertEquals("Should return ProcessTaskTriggerPurposeList from registry and add empty string as first element", purposes[i].EMP_Description, pTNotification.Lookups.ProcessTaskTriggerPurposeList[i + 1].Description);
			}
		}

		public void TestProcessTaskNotificationMacroTypeList()
		{
			var processTaskNotification = Factory.NewWithValidTestData<ProcessTaskNotification>();

			var list = processTaskNotification.Lookups.MacroTypeCodeList;

			AssertEquals(2, list.Count);
			Assert(list.ContainsCode(EventReferenceConditionList.Codes.ConditionWithMacros));
			Assert(list.ContainsCode(EventReferenceConditionList.Codes.UserDefined));
		}

		public void TestWorkflowTypes_TemplateCompletionTriggerActions()
		{
			var dummyTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			dummyTemplate.P0_ProcessType = "DUM";
			dummyTemplate.P0_GC = GlbCompany.CurrentCompany.PK;

			var orgTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			orgTemplate.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			orgTemplate.P0_GC = GlbCompany.CurrentCompany.PK;

			var trigger = CreateWorkflowItem(dummyTemplate);
			var notification = trigger.CompletionTriggerActionsCollection().AddNew();

			Factory.Save();

			var templates = notification.Lookups.WorkflowTemplateList;
			AssertContainsExactElementsInAnyOrder(new[] { dummyTemplate }, templates);
		}

		public void TestWorkflowTypes_JobCompletionTriggerActions()
		{
			var dummyTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			dummyTemplate.P0_ProcessType = "DUM";
			dummyTemplate.P0_GC = GlbCompany.CurrentCompany.PK;

			var orgTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			orgTemplate.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			orgTemplate.P0_GC = GlbCompany.CurrentCompany.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var trigger = CreateWorkflowItem(org);
			var notification = trigger.CompletionTriggerActionsCollection().AddNew();

			Factory.Save();

			var templates = notification.Lookups.WorkflowTemplateList;
			AssertContainsExactElementsInAnyOrder(new[] { orgTemplate }, templates);
		}

		public void TestRelatedEntities()
		{
			var triggerAction = Factory.New<ProcessTaskNotification>();
			triggerAction.PQ_P9 = Factory.New<ProcessTask>().PK;
			var lookups = triggerAction.Lookups;

			var relatedEntities = lookups.RelatedEntities;
			var existingTags = Factory.Load<ITagMagnitude>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(existingTags, relatedEntities);

			var tagDef1 = BMSTestHelper.CreateTagDefinition(Factory, "A1", usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.All);
			var tagDef2 = BMSTestHelper.CreateTagDefinition(Factory, "A2", usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.Task);
			var tagDef3 = BMSTestHelper.CreateTagDefinition(Factory, "A3", usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.Workflow);
			var tagDef_inactive1 = BMSTestHelper.CreateTagDefinition(Factory, "AI1", usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.All, isActive: false);
			var tagDef_inactive2 = BMSTestHelper.CreateTagDefinition(Factory, "AI2", usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.Task, isActive: false);
			var tagDef_inactive3 = BMSTestHelper.CreateTagDefinition(Factory, "AI3", usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.Workflow, isActive: false);

			var tagDef4 = BMSTestHelper.CreateTagDefinition(Factory, "R1", usageScope: TagUsageScopeList.Codes.Rule, scope: TagScopeList.Codes.All);
			var tagDef5 = BMSTestHelper.CreateTagDefinition(Factory, "R2", usageScope: TagUsageScopeList.Codes.Rule, scope: TagScopeList.Codes.Task);
			var tagDef6 = BMSTestHelper.CreateTagDefinition(Factory, "R3", usageScope: TagUsageScopeList.Codes.Rule, scope: TagScopeList.Codes.Workflow);

			var tagDef_inactive4 = BMSTestHelper.CreateTagDefinition(Factory, "RI1", usageScope: TagUsageScopeList.Codes.Rule, scope: TagScopeList.Codes.All, isActive: false);
			var tagDef_inactive5 = BMSTestHelper.CreateTagDefinition(Factory, "RI2", usageScope: TagUsageScopeList.Codes.Rule, scope: TagScopeList.Codes.Task, isActive: false);
			var tagDef_inactive6 = BMSTestHelper.CreateTagDefinition(Factory, "RI3", usageScope: TagUsageScopeList.Codes.Rule, scope: TagScopeList.Codes.Workflow, isActive: false);

			var tagDef7 = BMSTestHelper.CreateTagDefinition(Factory, "U1", usageScope: TagUsageScopeList.Codes.User, scope: TagScopeList.Codes.All);
			var tagDef8 = BMSTestHelper.CreateTagDefinition(Factory, "U2", usageScope: TagUsageScopeList.Codes.User, scope: TagScopeList.Codes.Task);
			var tagDef9 = BMSTestHelper.CreateTagDefinition(Factory, "U3", usageScope: TagUsageScopeList.Codes.User, scope: TagScopeList.Codes.Workflow);

			var tagDef_inactive7 = BMSTestHelper.CreateTagDefinition(Factory, "UI1", usageScope: TagUsageScopeList.Codes.User, scope: TagScopeList.Codes.All, isActive: false);
			var tagDef_inactive8 = BMSTestHelper.CreateTagDefinition(Factory, "UI2", usageScope: TagUsageScopeList.Codes.User, scope: TagScopeList.Codes.Task, isActive: false);
			var tagDef_inactive9 = BMSTestHelper.CreateTagDefinition(Factory, "UI3", usageScope: TagUsageScopeList.Codes.User, scope: TagScopeList.Codes.Workflow, isActive: false);

			var tag1 = BMSTestHelper.CreateTagMagnitude(tagDef1, "TG1", "TAG 1");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagDef2, "TG2", "TAG 2");
			var tag3 = BMSTestHelper.CreateTagMagnitude(tagDef3, "TG3", "TAG 3");
			var tag4 = BMSTestHelper.CreateTagMagnitude(tagDef4, "TG4", "TAG 4");
			var tag5 = BMSTestHelper.CreateTagMagnitude(tagDef5, "TG5", "TAG 5");
			var tag6 = BMSTestHelper.CreateTagMagnitude(tagDef6, "TG6", "TAG 6");
			var tag7 = BMSTestHelper.CreateTagMagnitude(tagDef7, "TG7", "TAG 7");
			var tag8 = BMSTestHelper.CreateTagMagnitude(tagDef8, "TG8", "TAG 8");
			var tag9 = BMSTestHelper.CreateTagMagnitude(tagDef9, "TG9", "TAG 9");
			var tag11 = BMSTestHelper.CreateTagMagnitude(tagDef1, "T11", "TAG 11", isActive: false);
			var tag12 = BMSTestHelper.CreateTagMagnitude(tagDef2, "T12", "TAG 12", isActive: false);
			var tag13 = BMSTestHelper.CreateTagMagnitude(tagDef3, "T13", "TAG 13", isActive: false);
			var tag14 = BMSTestHelper.CreateTagMagnitude(tagDef4, "T14", "TAG 14", isActive: false);
			var tag15 = BMSTestHelper.CreateTagMagnitude(tagDef5, "T15", "TAG 15", isActive: false);
			var tag16 = BMSTestHelper.CreateTagMagnitude(tagDef6, "T16", "TAG 16", isActive: false);
			var tag17 = BMSTestHelper.CreateTagMagnitude(tagDef7, "T17", "TAG 17", isActive: false);
			var tag18 = BMSTestHelper.CreateTagMagnitude(tagDef8, "T18", "TAG 18", isActive: false);
			var tag19 = BMSTestHelper.CreateTagMagnitude(tagDef9, "T19", "TAG 19", isActive: false);

			var tag21 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive1, "T21", "TAG 21");
			var tag22 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive2, "T22", "TAG 22");
			var tag23 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive3, "T23", "TAG 23");
			var tag24 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive1, "T24", "TAG 24", isActive: false);
			var tag25 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive2, "T25", "TAG 25", isActive: false);
			var tag26 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive3, "T26", "TAG 26", isActive: false);

			var tag31 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive4, "T31", "TAG 31");
			var tag32 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive5, "T32", "TAG 32");
			var tag33 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive6, "T33", "TAG 33");
			var tag34 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive4, "T34", "TAG 34", isActive: false);
			var tag35 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive5, "T35", "TAG 35", isActive: false);
			var tag36 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive6, "T36", "TAG 36", isActive: false);

			var tag41 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive7, "T41", "TAG 41");
			var tag42 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive8, "T42", "TAG 42");
			var tag43 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive9, "T43", "TAG 43");
			var tag44 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive7, "T44", "TAG 44", isActive: false);
			var tag45 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive8, "T45", "TAG 45", isActive: false);
			var tag46 = BMSTestHelper.CreateTagMagnitude(tagDef_inactive9, "T46", "TAG 46", isActive: false);

			Factory.Save();

			triggerAction = Factory.New<ProcessTaskNotification>();
			lookups = triggerAction.Lookups;

			relatedEntities = lookups.RelatedEntities;
			var expectedTags = new List<ITagMagnitude>();
			expectedTags.AddRange(existingTags);
			expectedTags.AddRange(new[] { tag1, tag3, tag4, tag6 });
			AssertContainsExactElementsInAnyOrder(expectedTags, relatedEntities);
		}

		#region Implementation

		#region DummySpecificDocumentBusinessContextDescriptorZzz

		class DummySpecificDocumentBusinessContextDescriptorZzz : WorkflowDescriptor, ISpecificDocumentBusinessContextProvider
		{
			public override string Code
			{
				get { return WorkflowCode; }
			}

			public const string WorkflowCode = "ZZZ";

			public Func<IWorkflowProvider, BusinessContext[]> GetDocumentBusinessContextImpl { get; set; }

			BusinessContext[] ISpecificDocumentBusinessContextProvider.GetDocumentBusinessContext(IWorkflowProvider workflowProvider)
			{
				return GetDocumentBusinessContextImpl != null ? GetDocumentBusinessContextImpl(workflowProvider) : Array.Empty<BusinessContext>();
			}

			public override IMultilingualString Description
			{
				get { return (NoResString)"ZZZ Workflow"; }
			}

			public override ControllerID ControllerID
			{
				get { return DummyControllerIDs.Dummy; }
			}

			public override Type WorkflowProviderType
			{
				get { return typeof(DummyWithWorkflowZZZ); }
			}

			protected override ZString[] SupportedTriggerPartyServicesCore(ZString recipient) => new ZString[] { "ABC", "DEF" };
		}

		class DummyWithWorkflowZZZ : DummyWithWorkflow
		{
			public DummyWithWorkflowZZZ(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetWorkflowType()
			{
				return DummySpecificDocumentBusinessContextDescriptorZzz.WorkflowCode;
			}

			protected override DummyProcessTaskCollection GetNewWorkflowItems()
			{
				return this.GetOrCreateProcessTaskCollection(() => new DummyProcessTaskCollectionZZZ(this));
			}
		}

		class DummyProcessTaskCollectionZZZ : DummyProcessTaskCollection
		{
			public DummyProcessTaskCollectionZZZ(BusinessObject master)
				: base(master)
			{
			}

			public new DummyProcessTaskZZZ this[int index]
			{
				get { return (DummyProcessTaskZZZ)Elements[index]; }
			}
		}

		class DummyProcessTaskZZZ : DummyProcessTask
		{
			public DummyProcessTaskZZZ(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected internal override Type ParentType
			{
				get { return typeof(DummyWithWorkflowZZZ); }
			}
		}

		#endregion

		TWorkflowItem CreateWorkflowItem(string workflowType)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = workflowType;

			return CreateWorkflowItem(template);
		}

		protected abstract TWorkflowItem CreateWorkflowItem(ProcessTaskTemplate template);
		protected abstract TWorkflowItem CreateWorkflowItem(IWorkflowProvider job);

		internal IBMTestHelper BMSTestHelper
		{
			get { return ObjectFactory.Get<IBMTestHelper>(); }
		}

		#endregion
	}

	internal static class WorkflowProviderDescriptorGatherer
	{
		public static IEnumerable<(IWorkflowProvider provider, IWorkflowDescriptor descriptor)> GetAllWorkflowProvidersAndDescriptors(Func<BusinessObjectFactory> factoryFactory, List<string> errors)
		{
			var processTaskDescriptorsRequiringCountryContext = workflowDescriptorsRequiringCountryContext.Value;
			var descriptorToDefaultCompanyMap = workflowDescriptorToDefaultCompanyMap.Value;

			foreach (var workFlowDescriptor in WorkflowDescriptors.Instance.Values)
			{
				var factory = factoryFactory.Invoke();
				var objs = new List<BusinessObject>();

				try
				{
					IDisposable countryContextDisposable = null;
					var workflowDescriptorCode = workFlowDescriptor.Code;
					if (processTaskDescriptorsRequiringCountryContext.Contains(workflowDescriptorCode))
					{
						var countryCodeForTest = workFlowDescriptor.ControllerID is not null
							? new ControllerList().All.First(x => x.ID == workFlowDescriptor.ControllerID).CountryCodeForTest
							: descriptorToDefaultCompanyMap.TryGetValue(workflowDescriptorCode, out var countryCode) ? countryCode : null;

						if (!string.IsNullOrWhiteSpace(countryCodeForTest))
						{
							countryContextDisposable = GlbCompany.TemporaryLoginInNewCompanyForCountry(countryCodeForTest);
						}
					}

					using (countryContextDisposable)
					{
						objs.Add(factory.NewWithValidTestData(workFlowDescriptor.WorkflowProviderType));
					}
				}
				catch (Exception)
				{
					var appDomainWrapper = new AppDomainWrapper();
					var types = appDomainWrapper.GetAssemblies().SelectMany(x => GetLoadableTypes(x)).Where(p =>
						p != workFlowDescriptor.WorkflowProviderType &&
						!p.ToString().Contains("test", StringComparison.OrdinalIgnoreCase) &&
						!p.ToString().Contains("castle", StringComparison.OrdinalIgnoreCase) &&
						workFlowDescriptor.WorkflowProviderType.IsAssignableFrom(p));

					foreach (var atype in types)
					{
						try
						{
							objs.Add(factory.NewWithValidTestData(atype));
						}
						catch (Exception)
						{
							// Keep trying to find a type to instantiate
						}
					}
				}

				if (objs.Count == 0)
				{
					if (workFlowDescriptor.WorkflowProviderType.ToString().Contains("QuotedBooking", StringComparison.OrdinalIgnoreCase))
					{
						// Special case for quoted booking as it cannot be instantiated from type
						objs.Add((BusinessObject)Activator.CreateInstance(workFlowDescriptor.WorkflowProviderType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { factory }, null));
					}
					else
					{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
						errors.Add($"Failed to instantiate {workFlowDescriptor.WorkflowProviderType.ToString()} from {workFlowDescriptor.GetType().ToString()}");
						var appDomainWrapper = new AppDomainWrapper();
						var types = appDomainWrapper.GetAssemblies().SelectMany(x => GetLoadableTypes(x)).Where(p =>
							p != workFlowDescriptor.WorkflowProviderType &&
							!p.ToString().Contains("test", StringComparison.OrdinalIgnoreCase) &&
							!p.ToString().Contains("castle", StringComparison.OrdinalIgnoreCase) &&
							workFlowDescriptor.WorkflowProviderType.IsAssignableFrom(p));
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

						errors.Add(string.Join("\n", types.Select(x => x.ToString())));
						continue;
					}
				}

				foreach (var obj in objs)
				{
					var provider = obj as IWorkflowProvider;
					ZString parentTableCode = "BAD";

					if (provider == null)
					{
						// This issue is ignored for now, but indicates an invalid configuration
					}
					else
					{
						yield return (provider, workFlowDescriptor);
					}
				}
			}
		}

		public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(t => t != null);
			}
		}

		static readonly Lazy<IReadOnlyDictionary<string, string>> workflowDescriptorToDefaultCompanyMap = new(() => new Dictionary<string, string>
		{
			{ WorkflowDescriptors.CusNOEmmaMessageGeneratorWorkflowDescriptorCode, Core.Constants.CountryCodes.Norway },
		});

		static readonly Lazy<ImmutableHashSet<string>> workflowDescriptorsRequiringCountryContext = new(() => new HashSet<string>(
		[
			WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode,
			WorkflowDescriptors.DrawBackWorkflowDescriptorCode,
			WorkflowDescriptors.ProtestWorkflowDescriptorCode,
			WorkflowDescriptors.ReconWorkflowDescriptorCode,
			WorkflowDescriptors.CusNOEmmaMessageGeneratorWorkflowDescriptorCode
		]).ToImmutableHashSet());
	}
}
