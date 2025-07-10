using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class ProcessTaskBaseCollectionViewTest<T> : BusinessObjectCollectionViewTestCase<T> where T : WorkflowItemCollectionView
	{
		public virtual void TestIsThisPartOfTheCollection()
		{
			ProcessTask matchedTask = Collection.AddNew();
			matchedTask.P9_Description = "Match";

			ProcessTask unmatchedTask = Collection.AddNew();
			unmatchedTask.P9_Description = "Not matched";

			AssertEquals("The item that returns true on IsTypeMatch should be included", 1, Collection.Count);
			AssertEquals("The item that returns true on IsTypeMatch should be included", matchedTask, Collection[0]);
		}

		public void TestReadOnly()
		{
			Assert(!Dummy.WorkflowItems.Tasks.ReadOnly);
			Assert(!Dummy.WorkflowItems.Triggers.ReadOnly);
			Assert(!Dummy.WorkflowItems.Milestones.ReadOnly);

			DummyWithWorkflow readOnlyDummy = Factory.New<DummyWithWorkflow>();
			readOnlyDummy.WorkflowItems.SetReadOnlyIncludingChildren(true);
			Assert(readOnlyDummy.WorkflowItems.Tasks.ReadOnly);
			Assert(readOnlyDummy.WorkflowItems.Triggers.ReadOnly);
			Assert(readOnlyDummy.WorkflowItems.Milestones.ReadOnly);
		}

		[TestDate(2009, 10, 19, 15, 46, 27)]
		public void TestFindOrCreateItemFromTemplateDoesNotCycles()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			WorkflowItemCollectionView collection = new WorkflowTriggerCollectionView(dummy.WorkflowItems);

			var eventTime = new ZDateTimeOffset(2009, 10, 19, 15, 46, 27); // 27 Seconds

			ProcessTask existingTrigger = collection.AddNew();
			existingTrigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			existingTrigger.P9_GC = ZGuid.Empty;
			existingTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			dummy.Logs.AddNew(Events.Arrival);

			existingTrigger.ProcessTaskNotifications.AddNew();

			Factory.Save();

			AssertEquals("Time is 'same' yet.", eventTime.ToZDateTime(), existingTrigger.P9_ActualDate);
			existingTrigger.Reload();
			AssertEquals("Seconds are not eliminated by db.", eventTime.ToZDateTime(), existingTrigger.P9_ActualDate);

			dummy.Logs.AddNew(Events.Arrival, eventTime);

			Factory.Save();

			ZQuery query = new ZQuery(StmALogSchema.SL_Parent, existingTrigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			StmALog[] wteLogs = Factory.Load<StmALog>(query);
			int existingCount = wteLogs.Length;

			existingTrigger.Reload();

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			// Emulate reloading/recreating workflow template on bizo save
			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			templateTrigger.P9_GC = ZGuid.Empty;
			templateTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			collection.CreateItemsFromTemplate(template, null, true);
			Factory.Save();

			wteLogs = Factory.Load<StmALog>(query);
			AssertEquals("No new 'WTE' logs should be added.", existingCount, wteLogs.Length);
		}

		public void TestCreateJobWithComplexTemplate()
		{
			int messageNumber = 1;

			List<BusinessObject> shipments = new List<BusinessObject>();
			for (int i = 0; i < messageNumber; i++)
			{
				var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipment[JobShipmentSchema.JS_TransportMode] = "AIR";
				shipments.Add(shipment);
			}
			Factory.Save();

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";

			AddTriggersWithUDFConditionsToTemplate(template, "(\"<TransportMode>\"==\"AIR\"  && \"<Origin.Code>\" == \"CNSHA\")");//this one will match
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<Consignee.OH_Code>\" == \"KYOCOMPAF\" || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<CompanyCode>\"==\"YJ1\"");
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<TransportMode>\"==\"SEA\"  || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<PortDisplayMode>\" == \"LoadDischargeCollectDeliver\"");
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<TransportMode>\"==\"SEA\"  || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<CompanyCode>\"==\"YJ1\"");
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<ConsigneeAddress>\"==\"SEA\"  || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<PortDisplayMode>\" == \"LoadDischargeCollectDeliver\"");
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<NotifyParty.PostalAddress>\" == \"KYOCOMPAF\" || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<CommodityCode>\"==\"\"");
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<ReleaseType>\"==\"SEA\"  || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<BrokerName>\" == \"LoadDischargeCollectDeliver\"");
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<JobHeader.JobChargesForLocalClient.Description>\" == \"KYOCOMPAF\" || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<JobHeader.ExchangeRates.Currency.Code>\"==\"YJ1\"");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<DestinationLoco.Code> \" == \"LoadDischargeCollectDeliver\"");
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<PortOfLoading.Code> \"==\"SEA\"  || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<DestinationLoco.Code>\"==\"YJ1\"");
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<OriginLoco.Code>\"==\"SEA\"  || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<OriginLoco.PortName>\" == \"LoadDischargeCollectDeliver\"");
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<OuterPackLineCollection.Length>\" == \"KYOCOMPAF\" || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<OuterPackLineCollection.PackType>\"==\"\"");
			AddTriggersWithUDFConditionsToTemplate(template, "(\"<Volume> \"==\"SEA\"  || \"<Consignee.OH_Code>\" == \"ITOPRONYC\")");
			AddTriggersWithUDFConditionsToTemplate(template, "\"<DocsAndCartage.EstimatedDelivery>\" == \"LoadDischargeCollectDeliver\"");

			Factory.Save();

			foreach (BusinessObject shipment in shipments)
			{
				((Forwarding.IForwardingShipment)shipment).JS_SystemLastEditTimeUtc = DateTime.Now;
				shipment[JobShipmentSchema.JS_RL_NKOrigin] = "CNSHA";
				Factory.Save();
			}

			ZQuery processtaskQuery = new ZQuery();
			foreach (BusinessObject shipment in shipments)
			{
				processtaskQuery.Clear();
				((Forwarding.IForwardingShipment)shipment).JS_SystemLastEditTimeUtc = DateTime.Now;
				processtaskQuery.AddToFilter(ProcessTasksSchema.P9_ParentID, ((Forwarding.IForwardingShipment)shipment).PK);
				processtaskQuery.AddToFilter(ProcessTasksSchema.P9_Condition2, ProcessTasksLookups.UserDefinedCondition);
				processtaskQuery.AddToFilter(ProcessTasksSchema.P9_Description, "UDF Test");
				Assert("Workflow task has been created.", Factory.Load<ProcessTask>(processtaskQuery).Length > 0);
			}
		}

		void AddTriggersWithUDFConditionsToTemplate(ProcessTaskTemplate template, string condition)
		{
			var triggerTask = template.WorkflowItems.Triggers.AddNew();
			triggerTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			triggerTask.TemplateConditions.TemplateCondition2Value = condition;
			triggerTask.TriggerConditions.TriggerEventCode = "CLR";
			triggerTask.TemplateConditions.TemplateCondition1 = "";
			triggerTask.P9_Description = "UDF Test";
		}

		#region Sorting

		public void TestAllowSort()
		{
			bool actualAllowSort = (bool)typeof(WorkflowItemCollectionView).InvokeMember("AllowSort", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, Collection, null);
			AssertEquals("Sorting isn't required and sort information goes away when the collection is loaded", false, actualAllowSort);
		}

		#endregion

		#region Default Values

		public void TestSetDefaultsForNewChild()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XXX";
			staff.GS_IsActive = true;

			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_IsActive = true;

			ProcessTask task1 = Collection.AddNew();
			AssertEquals("P9_Sequence 1", 1, task1.P9_Sequence);
			ProcessTask task2 = Collection.AddNew();
			AssertEquals("P9_Sequence 2", 2, task2.P9_Sequence);

			task2.P9_GS_NKAssignedStaffMember = "XXX";
			task2.P9_GG_AssignedGroup = group.PK;
			ProcessTask task3 = Collection.AddNew();
			AssertEquals("P9_Sequence 3", 3, task3.P9_Sequence);

			if (!task1.IsException)
			{
				AssertEquals("P9_GS_NKAssignedStaffMember defaulted from previous item", task2.P9_GS_NKAssignedStaffMember, task3.P9_GS_NKAssignedStaffMember);
				AssertEquals("P9_GG_AssignedGroup defaulted from previous item", task2.P9_GG_AssignedGroup, task3.P9_GG_AssignedGroup);
			}
		}

		#endregion

		#region CreateItemsFromTemplate

		public void TestCreateItemsFromTemplate_FallBack()
		{
			ProcessTaskTemplate templateWithCountries = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			templateWithCountries.P0_ProcessType = "DUM";
			templateWithCountries.P0_LoadPortCountry = "AU";
			templateWithCountries.P0_DischargePortCountry = "US";
			templateWithCountries.WorkflowItems.AddNew();

			ProcessTaskTemplate templateWithPorts = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			templateWithPorts.P0_ProcessType = "DUM";
			templateWithPorts.P0_LoadPortCountry = "AUSYD";
			templateWithPorts.P0_DischargePortCountry = "USLAX";
			templateWithPorts.WorkflowItems.AddNew();
			templateWithPorts.WorkflowItems.AddNew();

			ZDateTime datetime = ZDateTime.Now;
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			ProcessTaskTemplate templateWithPortsAndSubTypesAndClient = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			templateWithPortsAndSubTypesAndClient.P0_ProcessType = "DUM";
			templateWithPortsAndSubTypesAndClient.P0_LoadPortCountry = "AUSYD";
			templateWithPortsAndSubTypesAndClient.P0_DischargePortCountry = "USLAX";
			templateWithPortsAndSubTypesAndClient.P0_SubType1 = "ABC";
			templateWithPortsAndSubTypesAndClient.P0_OH_Client = client.PK;
			ProcessTask task1 = templateWithPortsAndSubTypesAndClient.WorkflowItems.AddNew();
			task1.P9_Sequence = 30;

			ProcessTask task2 = templateWithPortsAndSubTypesAndClient.WorkflowItems.AddNew();
			task2.P9_Sequence = 20;

			ProcessTask task3 = templateWithPortsAndSubTypesAndClient.WorkflowItems.AddNew();
			task3.P9_Sequence = 40;

			ProcessTask task4 = templateWithPortsAndSubTypesAndClient.WorkflowItems.AddNew();
			task4.P9_Sequence = 10;
			Factory.Save();

			Dummy.LoadPort = "AUBNE";
			Dummy.DischargePort = "INBOM";
			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Job has different ports and countries so no template is used", 0, Dummy.WorkflowItems.Count);

			Dummy.LoadPort = "AUBNE";
			Dummy.DischargePort = "USMEM";
			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Job has different ports but SAME countries so first template is used", 1, Dummy.WorkflowItems.Count);

			Dummy.LoadPort = "AUSYD";
			Dummy.DischargePort = "USLAX";
			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Job has same ports but different subtype 1 so 2nd template used", 2, Dummy.WorkflowItems.Count);

			Dummy.LoadPort = "AUSYD";
			Dummy.DischargePort = "USLAX";
			Dummy.SubType1 = "ABC";
			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Job has same ports and subtype 1 so 2nd template used", 2, Dummy.WorkflowItems.Count);

			ProcessTaskTemplate templateWithPortsAndSubTypes = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			templateWithPortsAndSubTypes.P0_ProcessType = "DUM";
			templateWithPortsAndSubTypes.P0_LoadPortCountry = "AUSYD";
			templateWithPortsAndSubTypes.P0_DischargePortCountry = "USLAX";
			templateWithPortsAndSubTypes.P0_SubType1 = "ABC";
			templateWithPortsAndSubTypes.WorkflowItems.AddNew();
			templateWithPortsAndSubTypes.WorkflowItems.AddNew();
			templateWithPortsAndSubTypes.WorkflowItems.AddNew();

			Factory.Save();

			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			templateWithPortsAndSubTypes.Delete();
			AssertEquals("Job has same ports and same subtype 1 and 3rd template now exists so it is used", 3, Dummy.WorkflowItems.Count);

			Factory.Save();

			Dummy.Client = client.PK;
			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			templateWithPortsAndSubTypes.Delete();
			AssertEquals("Job has same ports and same subtype 1 and same client, so 4th template used", 4, Dummy.WorkflowItems.Count);
			AssertEquals("Sorted by sequence", 10, Dummy.WorkflowItems.Tasks[0].P9_Sequence);
			AssertEquals("Sorted by sequence", 20, Dummy.WorkflowItems.Tasks[1].P9_Sequence);
			AssertEquals("Sorted by sequence", 30, Dummy.WorkflowItems.Tasks[2].P9_Sequence);
			AssertEquals("Sorted by sequence", 40, Dummy.WorkflowItems.Tasks[3].P9_Sequence);
		}

		public void TestCreateItemsFromTemplate_FallBackDifferentCompany()
		{
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			ProcessTaskTemplate template = factory2.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			template.P0_LoadPortCountry = "AU";
			template.P0_DischargePortCountry = "US";
			template.P0_GC = otherCompany.PK;
			template.WorkflowItems.AddNew();

			template.Factory.Save();

			Dummy.LoadPort = "AUBNE";
			Dummy.DischargePort = "USLAX";
			Dummy.WorkflowItems.RemoveAndDeleteAll();

			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Template exists, but for a different company - so no tasks created", 0, Dummy.WorkflowItems.Count);

			template.P0_GC = GlbCompany.CurrentCompany.PK;
			template.Factory.Save();

			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Template exists for current company - so tasks created", 1, Dummy.WorkflowItems.Count);
		}

		public void TestCreateItemsFromTemplate_FallBackToLowerScoreMatchesIfNoElementsFound()
		{
			ProcessTaskTemplate activeTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate1.P0_ProcessType = "DUM";
			activeTemplate1.P0_SubType1 = "AIR";
			ProcessTask milestoneA = activeTemplate1.WorkflowItems.AddNew();
			milestoneA.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestoneA.P9_Description = "TEMPLATE 1";
			ProcessTask triggerA = activeTemplate1.WorkflowItems.AddNew();
			triggerA.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			triggerA.P9_Description = "TEMPLATE 1";

			ProcessTaskTemplate activeTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate2.P0_ProcessType = "DUM";
			activeTemplate2.P0_SubType1 = "AIR";
			activeTemplate2.P0_LoadPortCountry = "AU";
			ProcessTask milestoneB = activeTemplate2.WorkflowItems.AddNew();
			milestoneB.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestoneB.P9_Description = "TEMPLATE 2";

			ProcessTaskTemplate inactiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			inactiveTemplate.P0_ProcessType = "DUM";
			inactiveTemplate.P0_SubType1 = "AIR";
			inactiveTemplate.P0_LoadPortCountry = "AU";
			inactiveTemplate.P0_IsActive = false;

			Factory.Save();

			Dummy.SubType1 = "AIR";
			Dummy.LoadPort = "AUSYD";
			AssertItemsCreated("Should be successful", Dummy.WorkflowItems.Milestones.CreateItemsFromTemplate());
			AssertEquals(1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("TEMPLATE 2", Dummy.WorkflowItems.Milestones[0].P9_Description);
			var milestones = Dummy.WorkflowItems.Milestones.GetItemsToCreateFromTemplate();
			AssertEquals(1, milestones.Count);
			AssertEquals(milestoneB.PK, milestones.Single().PK);

			AssertItemsCreated("Should be successful", Dummy.WorkflowItems.Triggers.CreateItemsFromTemplate());
			AssertEquals(1, Dummy.WorkflowItems.Triggers.Count);
			AssertEquals("TEMPLATE 1", Dummy.WorkflowItems.Triggers[0].P9_Description);
			var triggers = Dummy.WorkflowItems.Triggers.GetItemsToCreateFromTemplate();
			AssertEquals(1, triggers.Count);
			AssertEquals(triggerA.PK, triggers.Single().PK);

			milestoneB.Delete();
			Factory.Save();
			Dummy.WorkflowItems.Milestones.DeleteAll();
			AssertItemsCreated("Should be successful", Dummy.WorkflowItems.Milestones.CreateItemsFromTemplate());
			AssertEquals(1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("TEMPLATE 1", Dummy.WorkflowItems.Milestones[0].P9_Description);
			milestones = Dummy.WorkflowItems.Milestones.GetItemsToCreateFromTemplate();
			AssertEquals(1, milestones.Count);
			AssertEquals(milestoneA.PK, milestones.Single().PK);

			milestoneA.Delete();
			Factory.Save();
			Dummy.WorkflowItems.Milestones.DeleteAll();
			AssertItemsNotCreated("Should return false", Dummy.WorkflowItems.Milestones.CreateItemsFromTemplate());
			AssertEquals(0, Dummy.WorkflowItems.Milestones.Count);
			milestones = Dummy.WorkflowItems.Milestones.GetItemsToCreateFromTemplate();
			AssertEquals(0, milestones.Count);
		}

		void AssertItemsCreated(string message, ApplyWorkflowTemplateResult items) => Assert(message, items.HasTemplateApplied);
		void AssertItemsNotCreated(string message, ApplyWorkflowTemplateResult items) => Assert(message, !items.HasTemplateApplied);

		[TestDate(2009, 1, 1)]
		public void TestDefaultDatesFromMilestone()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			template.P0_LoadPortCountry = "AU";
			template.P0_DischargePortCountry = "US";
			ProcessTask trigger = template.WorkflowItems.AddNew();
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			ProcessTask milestone = template.WorkflowItems.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone.P9_Type = Core.Constants.Workflow.MilestoneType;
			ProcessTaskNotification milestoneNotification = milestone.ProcessTaskNotifications.AddNew();
			Factory.Save();

			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals("Precondition", 0, Dummy.WorkflowItems.Count);

			Dummy.LoadPort = "AUSYD";
			Dummy.DischargePort = "USLAX";
			Dummy.WorkflowItems.Milestones.CreateItemsFromTemplate();
			AssertEquals("Template exists for current company - so milestone created", 1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("Actual Date is set", ZDateTime.Now, Dummy.WorkflowItems.Milestones[0].P9_ActualDate.ToZDateTime());

			Dummy.WorkflowItems.Triggers.CreateItemsFromTemplate();
			AssertEquals("Template exists for current company - so trigger created", 1, Dummy.WorkflowItems.Triggers.Count);
			AssertEquals("Actual Date is set", ZDateTime.Now, Dummy.WorkflowItems.Triggers[0].P9_ActualDate.ToZDateTime());

			StmALog[] logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code));
			AssertEquals("Both the milestone and trigger fire.", 2, logs.Length);
		}

		public void TestCreateItemsFromTemplate_FallBackBlankCompany()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			template.P0_LoadPortCountry = "AU";
			template.P0_DischargePortCountry = "US";
			template.WorkflowItems.AddNew();

			Factory.Save();

			Dummy.LoadPort = "AUBNE";
			Dummy.DischargePort = "USLAX";
			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Template exists for BLANK company, so 1 task created", 1, Dummy.WorkflowItems.Count);
		}

		public void TestCreateItemsFromTemplate_FallBackOriginCountrySpecific()
		{
			TestCreateItemsFromTemplate_FallBackCountrySpecific(ProcessTasksSchema.P9_RN_NKOriginCountry);
		}

		public void TestCreateItemsFromTemplate_FallBackDestinationCountrySpecific()
		{
			TestCreateItemsFromTemplate_FallBackCountrySpecific(ProcessTasksSchema.P9_RN_NKDestinationCountry);
		}

		void TestCreateItemsFromTemplate_FallBackCountrySpecific(SchemaStringColumn countryCodeProperty)
		{
			string countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			try
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				ProcessTaskTemplate template = factory2.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = "DUM";
				template.P0_LoadPortCountry = "AU";
				template.P0_DischargePortCountry = "US";
				template.P0_GC = GlbCompany.CurrentCompany.PK;
				ProcessTask task1 = template.WorkflowItems.AddNew();
				ProcessTask task2 = template.WorkflowItems.AddNew();
				task2[countryCodeProperty] = Core.Constants.CountryCodes.India;
				ProcessTask task3 = template.WorkflowItems.AddNew();
				task3[countryCodeProperty] = Core.Constants.CountryCodes.Australia;

				template.Factory.Save();

				Dummy.LoadPort = "AUBNE";
				Dummy.DischargePort = "USLAX";
				Dummy.WorkflowItems.RemoveAndDeleteAll();

				Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
				AssertEquals("1 task created - all country task, and aussie task", 1, Dummy.WorkflowItems.Count);

				task3[countryCodeProperty] = Core.Constants.CountryCodes.Albania;
				template.Factory.Save();

				Dummy.WorkflowItems.RemoveAndDeleteAll();
				Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
				AssertEquals("1 tasks created - all country task only", 1, Dummy.WorkflowItems.Count);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);

				Dummy.WorkflowItems.RemoveAndDeleteAll();
				Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
				AssertEquals("1 task created - all country task and India task", 1, Dummy.WorkflowItems.Count);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestCreateItemsFromTemplate_DontRecreateItemsThatAlreadyExist()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			ProcessTask departureMilestoneTemplate = template.WorkflowItems.AddNew();
			departureMilestoneTemplate.IsMilestone = true;
			departureMilestoneTemplate.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			ProcessTask arrivalMilestoneTemplate = template.WorkflowItems.AddNew();
			arrivalMilestoneTemplate.IsMilestone = true;
			arrivalMilestoneTemplate.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			Factory.Save();

			AssertEquals("No milestones created initially", 0, Dummy.WorkflowItems.Milestones.Count);
			Dummy.WorkflowItems.Milestones.CreateItemsFromTemplate();
			AssertEquals("2 milestones created from template", 2, Dummy.WorkflowItems.Milestones.Count);

			Dummy.WorkflowItems.Milestones.CreateItemsFromTemplate();
			AssertEquals("No new milestones created from template as both already exist", 2, Dummy.WorkflowItems.Milestones.Count);
		}

		public void TestCreateItemsFromTemplate_StaffAndGroupComesFromTemplate()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			ProcessTask taskTemplate1 = template.WorkflowItems.AddNew();
			taskTemplate1.P9_Description = "Task1";
			taskTemplate1.P9_GS_NKAssignedStaffMember = "CRV";
			taskTemplate1.P9_GG_AssignedGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			ProcessTask taskTemplate2 = template.WorkflowItems.AddNew();
			taskTemplate2.P9_Description = "Task1";
			taskTemplate2.P9_GS_NKAssignedStaffMember = "";
			taskTemplate2.P9_GG_AssignedGroup = ZGuid.Empty;
			Factory.Save();

			AssertEquals("No tasks created initially", 0, Dummy.WorkflowItems.Tasks.Count);
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("2 tasks created from template", 2, Dummy.WorkflowItems.Tasks.Count);

			AssertEquals(taskTemplate1.P9_GS_NKAssignedStaffMember, Dummy.WorkflowItems.Tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(taskTemplate1.P9_GG_AssignedGroup, Dummy.WorkflowItems.Tasks[0].P9_GG_AssignedGroup);
			AssertEquals(ZString.Empty, Dummy.WorkflowItems.Tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals(ZGuid.Empty, Dummy.WorkflowItems.Tasks[1].P9_GG_AssignedGroup);
		}

		public void TestCreateItemsFromTemplate_SetP9_ParentTemplateID()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			ProcessTask templateMilestone = template.WorkflowItems.Milestones.AddNew();

			Dummy.Factory.Save();
			Dummy.WorkflowItems.Milestones.CreateItemsFromTemplate_ForTest(template);
			ProcessTask milestone = Dummy.WorkflowItems.Milestones[0];
			AssertEquals("P9_ParentTemplateID set", templateMilestone.PK, milestone.P9_ParentTemplateID);
		}

		public void TestOriginCountrySpecificTasks()
		{
			TestCountrySpecificTasks(ProcessTasksSchema.P9_RN_NKOriginCountry);
		}

		public void TestDestinationCountrySpecificTasks()
		{
			TestCountrySpecificTasks(ProcessTasksSchema.P9_RN_NKDestinationCountry);
		}

		void TestCountrySpecificTasks(SchemaStringColumn countryCodeProperty)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "NZAKL";
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ProcessTaskTemplate template = factory2.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTask task1 = template.WorkflowItems.AddNew();
			template.P0_ProcessType = "OPP";
			task1.P9_Description = "Client country Test";
			task1[countryCodeProperty] = Core.Constants.CountryCodes.Australia;
			task1.P9_Description = "Match";
			factory2.Save();

			opportunity.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("No tasks as task was for AU", 0, opportunity.WorkflowItems.Count);

			org.OH_RL_NKClosestPort = "AUSYD";
			opportunity.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("1 task as task was for AU", 1, opportunity.WorkflowItems.Count);
		}

		public void TestCreateTasksWithDuplicatedDescriptions()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var testHelper = ObjectFactory.Get<IBMTestHelper>();

			var system = testHelper.CreateSystem(Factory, "WKI");

			var jobHeader = testHelper.CreateJobHeader<IWorkItem>(Factory);
			jobHeader.ProcessHeaders.AddNew().FH_CompletionStatement = "Job Workflow 1";
			jobHeader.ProcessHeaders.AddNew().FH_CompletionStatement = "Job Workflow 2";
			jobHeader.ProcessHeaders.AddNew().FH_CompletionStatement = "Job Workflow 3";
			jobHeader.FH_CompletionStatement = "Job Workflow";

			AssertEquals("Number of process job headers", 4, jobHeader.ProcessHeaders.Count);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "WKI";

			var taskTemplate1 = template.WorkflowItems.AddNew();
			taskTemplate1.P9_FH_ProcessHeader = jobHeader.PK;
			taskTemplate1.P9_Description = "Task1";
			taskTemplate1.P9_GS_NKAssignedStaffMember = "CRV";
			taskTemplate1.P9_GG_AssignedGroup = Factory.NewWithValidTestData<GlbGroup>().PK;

			Factory.Save();

			AssertNoExceptionThrown("No exception is expected", () => ((IWorkflowProvider)jobHeader.Parent).WorkflowItems.Tasks.CreateItemsFromTemplate());
		}

		public void TestCreateItemsFromTemplate_ExcludeItemsWithUnmetConditions()
		{
			TemplateWithConditions.Factory.Save();

			Dummy.Z0_Code = "CD1";
			Dummy.Z0_Description = "";
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Only item with matching condition created", 1, Dummy.WorkflowItems.Tasks.Count);
			AssertEquals("Only item with matching condition created", "CD1", Dummy.WorkflowItems.Tasks[0].P9_Description);
			Dummy.WorkflowItems.RemoveAndDeleteAll();

			Dummy.Z0_Code = "";
			Dummy.Z0_Description = "CD2";
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Only item with matching condition created", 1, Dummy.WorkflowItems.Tasks.Count);
			AssertEquals("Only item with matching condition created", "CD2", Dummy.WorkflowItems.Tasks[0].P9_Description);
			Dummy.WorkflowItems.RemoveAndDeleteAll();

			AnotherTemplateWithConditions.Factory.Save();

			Dummy.Z0_Code = "CD3";
			Dummy.Z0_Description = "";
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Only item with matching condition created, should fall back to next applicable template if first one does not contain items matching condition", 1, Dummy.WorkflowItems.Tasks.Count);
			AssertEquals("Only item with matching condition created, should fall back to next applicable template if first one does not contain items matching condition", "CD3", Dummy.WorkflowItems.Tasks[0].P9_Description);
			Dummy.WorkflowItems.RemoveAndDeleteAll();

			Dummy.Z0_Code = "";
			Dummy.Z0_Description = "CD2";
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Only item with matching condition created, should neither fall back nor merge if several templates contain items matching condition, should use only first such template", 1, Dummy.WorkflowItems.Tasks.Count);
			AssertStartsWith("Only item with matching condition created", "CD2", Dummy.WorkflowItems.Tasks[0].P9_Description);
			Dummy.WorkflowItems.RemoveAndDeleteAll();
		}

		public void TestCreateItemsFromTemplate_AlwaysIncludeItemsWithActionConditions()
		{
			ProcessTask triggerTemplate = TemplateWithConditions.WorkflowItems.Triggers.AddNew();
			triggerTemplate.P9_Description = "Trigger";
			triggerTemplate.TriggerConditions.TriggerEventCode = Events.CFSContainerPacked.Code;
			triggerTemplate.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			triggerTemplate.TriggerConditions.TriggerConditionValue = "MEH MEH";
			TemplateWithConditions.Factory.Save();

			Dummy.Z0_Code = "CD1";
			AssertEquals(0, Dummy.WorkflowItems.Count);

			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			Dummy.WorkflowItems.Triggers.CreateItemsFromTemplate();
			AssertEquals(2, Dummy.WorkflowItems.Count);
			AssertEquals(1, Dummy.WorkflowItems.Tasks.Count);
			AssertEquals("CD1", Dummy.WorkflowItems.Tasks[0].P9_Description);
			AssertEquals(1, Dummy.WorkflowItems.Triggers.Count);
			AssertEquals("Trigger", Dummy.WorkflowItems.Triggers[0].P9_Description);
			AssertEquals("MEH MEH", Dummy.WorkflowItems.Triggers[0].P9_TriggerConditionValue);
		}

		public void TestCreateItemsFromTemplate_DoesNotCreateDuplicates()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			ProcessTask templateTrigger = TemplateWithConditions.WorkflowItems.Triggers.AddNew();
			templateTrigger.P9_Description = "T1";
			templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.CFSContainerPackedCode;
			templateTrigger.TemplateConditions.TemplateCondition1 = "CD1";

			template.Factory.Save();

			ProcessTaskNotification templateNotification = templateTrigger.ProcessTaskNotifications.AddNew();
			templateNotification.PQ_EmailText = "text";

			Dummy.Z0_Code = "CD1";
			AssertEquals("Precondition: no triggers", 0, Dummy.WorkflowItems.Count);

			Dummy.ApplyWorkflowTemplates();
			AssertEquals("1 new trigger should be created", 1, Dummy.WorkflowItems.Triggers.Count);

			Dummy.ApplyWorkflowTemplates();
			AssertEquals("1 trigger should remain with no duplicates", 1, Dummy.WorkflowItems.Triggers.Count);

			ProcessTask otherTemplateTrigger = TemplateWithConditions.WorkflowItems.Triggers.AddNew();
			otherTemplateTrigger.P9_Description = "T2";
			otherTemplateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.ExportCustomsCommencedCode;
			otherTemplateTrigger.TemplateConditions.TemplateCondition1 = "CD1";
			otherTemplateTrigger.TemplateConditions.TemplateCondition2 = "UDF";
			otherTemplateTrigger.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			template.Factory.Save();

			Dummy.ApplyWorkflowTemplates();
			AssertEquals("1 new trigger should be created with total = 2", 2, Dummy.WorkflowItems.Triggers.Count);
		}

		public void TestCreateItemsFromTemplate_DuplicatesOnDifferentEvents_ExceptDontWhenItComesFromTheSameTemplate()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			ProcessTask templateTrigger = TemplateWithConditions.WorkflowItems.Triggers.AddNew();
			templateTrigger.P9_Description = "T1";
			templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.CFSContainerPackedCode;
			templateTrigger.TemplateConditions.TemplateCondition1 = "CD1";
			templateTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTrigger.TemplateConditions.TemplateCondition2Value = "1>0";

			template.Factory.Save();

			ProcessTaskNotification templateNotification = templateTrigger.ProcessTaskNotifications.AddNew();
			templateNotification.PQ_EmailText = "text";
			Dummy.Z0_Code = "CD1";

			Dummy.WorkflowItems.Triggers.CreateItemsFromTemplate();
			AssertEquals("First trigger should have been created", 1, Dummy.WorkflowItems.Count);

			templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.ChangeOfIdentifierCode;

			template.Factory.Save();
			Dummy.WorkflowItems.Triggers.CreateItemsFromTemplate();
			AssertEquals("Changing the event code should not change template application", 1, Dummy.WorkflowItems.Count);
		}

		ProcessTaskTemplate TemplateWithConditions
		{
			get
			{
				if (templateWithConditions == null)
				{
					templateWithConditions = Factory.NewWithValidTestData<ProcessTaskTemplate>();
					templateWithConditions.P0_ProcessType = "DUM";

					ProcessTask item1 = templateWithConditions.WorkflowItems.AddNew();
					item1.P9_Description = "CD1";
					item1.TemplateConditions.TemplateCondition1 = "CD1";
					ProcessTask item2 = templateWithConditions.WorkflowItems.AddNew();
					item2.P9_Description = "CD2";
					item2.TemplateConditions.TemplateCondition2 = "CD2";
				}
				return templateWithConditions;
			}
		}
		ProcessTaskTemplate templateWithConditions;

		ProcessTaskTemplate AnotherTemplateWithConditions
		{
			get
			{
				if (anotherTemplateWithConditions == null)
				{
					anotherTemplateWithConditions = Factory.NewWithValidTestData<ProcessTaskTemplate>();
					anotherTemplateWithConditions.P0_ProcessType = "DUM";

					ProcessTask item1 = anotherTemplateWithConditions.WorkflowItems.AddNew();
					item1.P9_Description = "CD2a";
					item1.TemplateConditions.TemplateCondition2 = "CD2";
					ProcessTask item2 = anotherTemplateWithConditions.WorkflowItems.AddNew();
					item2.P9_Description = "CD3";
					item2.TemplateConditions.TemplateCondition1 = "CD3";
				}
				return anotherTemplateWithConditions;
			}
		}
		ProcessTaskTemplate anotherTemplateWithConditions;

		#endregion

		#region IWorkflowProvider

		public void TestIWorkflowProvider()
		{
			AssertEquals(Dummy.WorkflowItems, ((IWorkflowProvider)Collection).WorkflowItems);
		}

		#endregion

		#region Test Classes

		public class TestProcessTaskBaseCollectionView : WorkflowItemCollectionView
		{
			public TestProcessTaskBaseCollectionView(DummyProcessTaskCollection collection)
				: base(collection)
			{
			}

			protected override bool IsTypeMatch(ProcessTask task)
			{
				return task.P9_Description == "Match";
			}

			protected override void AddNewTaskCore(ProcessTask task)
			{
				task.P9_Description = "Match";
				base.AddNewTaskCore(task);
			}

			protected override IEqualityComparer<ProcessTask> GetTemplateApplicationComparer()
			{
				throw new NotImplementedException();
			}

			protected override TemplateEntityType TemplateEntityType => TemplateEntityType.Tasks;

			protected override string AddSecurityCode => SecurityCore.WorkflowAddTasksAutoGeneratedCode;
		}

		#endregion

		#region Implementation

		protected DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}

		protected new WorkflowItemCollectionView Collection
		{
			get { return base.Collection; }
		}

		protected sealed override T GetCollectionToTest()
		{
			return (T)GetNewCollectionView(Dummy.WorkflowItems);
		}

		protected virtual WorkflowItemCollectionView GetNewCollectionView(ProcessTaskCollection collection)
		{
			return new TestProcessTaskBaseCollectionView((DummyProcessTaskCollection)collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ProcessTask result = Factory.New<ProcessTask>();
			result.P9_Description = "Match";
			return result;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString> { ProcessTasksSchema.P9_MilestoneCompletionPivotKey.Name };
		}

		#endregion
	}
}
