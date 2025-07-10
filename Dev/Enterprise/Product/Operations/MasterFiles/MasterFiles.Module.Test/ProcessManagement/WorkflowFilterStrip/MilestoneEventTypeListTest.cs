using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class MilestoneEventTypeListTest : TestCaseWithFactory
	{
		public void TestEventTypeListIsFull()
		{
			MilestoneEventTypeList eventTypes = new MilestoneEventTypeList("");
			foreach (Event type in Events.All)
			{
				if (!Events.ChangeLogs.Contains(type))
				{
					Assert(eventTypes.Contains(new CodeDescriptionPair(type.Code, type.Description)));
				}
			}
		}

		public void TestInactiveTemplatesAreExcluded()
		{
			var template1 = CreateTemplate("SHP", true, true, "", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, Events.CustomisableEvent01.Code, "bat1");
			var template2 = CreateTemplate("SHP", false, true, "", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, Events.CustomisableEvent02.Code, "bat2");
			Factory.Save();

			var eventTypes = new MilestoneEventTypeList("SHP");
			AssertEquals(true, eventTypes.ContainsCode(Events.CustomisableEvent01.Code));

			template1.P0_IsActive = false;
			template2.P0_IsActive = true;
			Factory.Save();

			eventTypes = new MilestoneEventTypeList("SHP");
			AssertEquals(true, eventTypes.ContainsCode(Events.CustomisableEvent02.Code));
		}

		public void TestEventTypeListUsesCorrectTemplate()
		{
			CreateTemplates();

			var eventTypes = new MilestoneEventTypeList("DUM");
			AssertEquals(false, eventTypes.ContainsCode("EV1"));
			AssertEquals(true, eventTypes.ContainsCode("EV2"));
			AssertEquals(true, eventTypes.ContainsCode("EV3"));
			AssertEquals(true, eventTypes.ContainsCode("EV4"));
			AssertEquals(true, eventTypes.ContainsCode("EV5"));
			AssertEquals(true, eventTypes.ContainsCode("EV6"));
			AssertEquals(true, eventTypes.ContainsCode("EV7"));
			AssertEquals(true, eventTypes.ContainsCode("EV8"));
			AssertEquals(true, eventTypes.ContainsCode("EV9"));
			AssertEquals(true, eventTypes.ContainsCode("E10"));
			AssertEquals(true, eventTypes.ContainsCode("E11"));
			AssertEquals(false, eventTypes.ContainsCode("E12"));

			var newTemplate = CreateTemplate("DUM", true, false, "", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV1", "dog");
			AddMilestone(newTemplate, "E12", "cat");
			Factory.Save();

			eventTypes = new MilestoneEventTypeList("DUM");
			AssertEquals(true, eventTypes.ContainsCode("EV1"));
			AssertEquals(true, eventTypes.ContainsCode("EV2"));
			AssertEquals(true, eventTypes.ContainsCode("EV3"));
			AssertEquals(true, eventTypes.ContainsCode("EV4"));
			AssertEquals(true, eventTypes.ContainsCode("EV5"));
			AssertEquals(true, eventTypes.ContainsCode("EV6"));
			AssertEquals(true, eventTypes.ContainsCode("EV7"));
			AssertEquals(true, eventTypes.ContainsCode("EV8"));
			AssertEquals(true, eventTypes.ContainsCode("EV9"));
			AssertEquals(true, eventTypes.ContainsCode("E10"));
			AssertEquals(true, eventTypes.ContainsCode("E11"));
			AssertEquals(true, eventTypes.ContainsCode("E12"));
		}

		public void TestEventTypeListIncludesPartialTemplates()
		{
			CreateTemplates();

			var eventTypes = new MilestoneEventTypeList("DUM");
			AssertEquals(false, eventTypes.ContainsCode("EV1"));
			AssertEquals(false, eventTypes.ContainsCode("E12"));

			var newTemplate = CreateTemplate("DUM", true, false, "", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV1", "dog");
			newTemplate.P0_IsPartialTemplate = true;
			AddMilestone(newTemplate, "E12", "cat");
			Factory.Save();

			eventTypes = new MilestoneEventTypeList("DUM");
			AssertEquals(true, eventTypes.ContainsCode("EV1"));
			AssertEquals(true, eventTypes.ContainsCode("E12"));
		}

		public void TestTemplatesNotCached()
		{
			CreateTemplates();

			var eventTypes = new MilestoneEventTypeList("DUM");
			AssertEquals(true, eventTypes.ContainsCode("EV2"));

			var newTemplate = CreateTemplate("DUM", true, false, "", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "E99", "dog");
			Factory.Save();

			eventTypes = new MilestoneEventTypeList("DUM");
			AssertEquals(true, eventTypes.ContainsCode("E99"));
		}

		public void TestAddMilestoneTemplateEvents1DbHit()
		{
			CreateTemplate("DUM", true, false, "aaa", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV1", "Event 1");
			CreateTemplate("DUM", true, false, "bbb", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV2", "Event 2");
			var template = CreateTemplate("DUM", true, false, "ccc", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV3", "Event 3");
			template.Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			AssertEquals(0, newFactory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));

			var eventTypes = new MilestoneEventTypeList("DUM");

			AssertEquals("No detected selects; There is an sqlReader probably.", 0, newFactory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));
		}

		public void TestMilestoneTemplateEventListShowsCustomizedDescription()
		{
			var processTaskTemplate = Factory.New<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "SHP";
			processTaskTemplate.P0_IsSystem = true;
			processTaskTemplate.P0_Name = "TestFYO";
			var processTask = processTaskTemplate.WorkflowItems.Triggers.AddNew();
			processTask.TriggerConditions.TriggerEventCode = "Z02";
			processTask.P9_Description = "Customizable Event 02";

			var customizableEventZ00 = Factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_Code, "Z00"))[0];
			customizableEventZ00.SE_Desc = "This is a customized Z00 description from freddy";
			var customizableEventZ01 = Factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_Code, "Z01"))[0];
			customizableEventZ01.SE_Desc = "This is a customized Z01 description from freddy";
			customizableEventZ01.SE_IsActive = false;
			var customizableEventZ02 = Factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_Code, "Z02"))[0];
			customizableEventZ02.SE_Desc = "This is a customized Z02 description from freddy";
			Factory.Save();

			var eventTypes = new MilestoneEventTypeList("");
			AssertEquals("This is a customized Z00 description from freddy", eventTypes["Z00"].Description);
			AssertEquals("Customizable Event 01", eventTypes["Z01"].Description);
			AssertEquals("This is a customized Z02 description from freddy", eventTypes["Z02"].Description);
		}

		#region Implementation

		void AddMilestone(IWorkflowProvider businessObject, string code, string description)
		{
			var task = businessObject.WorkflowItems.Milestones.AddNew();
			task.TriggerConditions.TriggerEventCode = code;
			task.P9_Description = description;
		}

		ProcessTaskTemplate[] CreateTemplates(string processType = "DUM")
		{
			var result = new[]
			{
				CreateTemplate(processType, true, true, "", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV2", "bat"),
				CreateTemplate(processType, true, false, "str", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV3", "zebra"),
				CreateTemplate(processType, true, false, "", "str", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV4", "dinasour"),
				CreateTemplate(processType, true, false, "", "", "str", "", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV5", "rex"),
				CreateTemplate(processType, true, false, "", "", "", "str", "", "", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV6", "mary"),
				CreateTemplate(processType, true, false, "", "", "", "", "", "str", "", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV7", "smith"),
				CreateTemplate(processType, true, false, "", "", "", "", "", "", "str", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "EV8", "had"),
				CreateTemplate(processType, true, false, "", "", "", "", "", "", "", GlbBranch.CurrentBranch.PK, ZGuid.Empty, ZGuid.Empty, "EV9", "a"),
				CreateTemplate(processType, true, false, "", "", "", "", "", "", "", ZGuid.Empty, GlbDepartment.CurrentDepartment.PK, ZGuid.Empty, "E10", "dead"),
				CreateTemplate(processType, true, false, "", "", "", "", "", "", "", ZGuid.Empty, ZGuid.Empty, Factory.NewWithValidTestData<OrgHeader>().PK, "E11", "bird"),
			};
			Factory.Save();
			return result;
		}

		ProcessTaskTemplate CreateTemplate(string jobType, bool isActive, bool isSystem, string subType1, string subType2, string subType3, string subType4, string subType5, string loadPort, string dischargePort, ZGuid branchPK, ZGuid deptPK, ZGuid clientPK, string eventCode, string description)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_ProcessType = jobType;
			template.P0_IsActive = isActive;
			template.P0_IsSystem = isSystem;
			template.P0_SubType1 = subType1;
			template.P0_SubType2 = subType2;
			template.P0_SubType3 = subType3;
			template.P0_SubType4 = subType4;
			template.P0_SubType5 = subType5;
			template.P0_LoadPortCountry = loadPort;
			template.P0_DischargePortCountry = dischargePort;
			template.P0_GB = branchPK;
			template.P0_GE = deptPK;
			template.P0_OH_Client = clientPK;

			AddMilestone(template, eventCode, description);

			return template;
		}

		#endregion
	}
}
