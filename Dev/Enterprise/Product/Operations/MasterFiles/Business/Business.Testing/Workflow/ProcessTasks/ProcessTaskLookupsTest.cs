using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTaskLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContacts()
		{
			AssertEquals("No contacts on a task without an opportunity parent", 0, ProcessTask.Lookups.Contacts.Count);
		}

		public void TestAddresses()
		{
			AssertEquals("No addresses on a task without an opportunity parent", 0, ProcessTask.Lookups.Addresses.Count);
		}

		public void TestTypes()
		{
			CategorisedWorkflowTaskTypesCollection registryValue = new CategorisedWorkflowTaskTypesCollection();

			registryValue.AddNew().Code = "STA";
			registryValue.AddNew().Code = new OpportunityWorkflowDescriptor().Code;

			registryValue[0].TaskTypes.AddNew().Code = "ABC";
			registryValue[1].TaskTypes.AddNew().Code = "XYZ";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);

			ProcessTask standAloneTask = Factory.New<ProcessTask>();
			AssertEquals("ContainsCode(\"ABC\")", true, standAloneTask.Lookups.Types.ContainsCode("ABC"));
			AssertEquals("ContainsCode(\"XYZ\")", false, standAloneTask.Lookups.Types.ContainsCode("XYZ"));

			OrgOpportunity parent = Factory.New<OrgOpportunity>();
			ProcessTask parentedTask = parent.WorkflowItems.AddNew();

			AssertEquals("ContainsCode(\"ABC\")", false, parentedTask.Lookups.Types.ContainsCode("ABC"));
			AssertEquals("ContainsCode(\"XYZ\")", true, parentedTask.Lookups.Types.ContainsCode("XYZ"));
		}

		public void TestTypes_CompanySwitching()
		{
			var company1 = GlbCompany.CurrentCompany;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var company2 = branch2.Company;

			Factory.Save();

			var company1RegistryValue = new CategorisedWorkflowTaskTypesCollection();
			company1RegistryValue.AddNew().Code = "STA";
			company1RegistryValue.AddNew().Code = new OpportunityWorkflowDescriptor().Code;
			company1RegistryValue[0].TaskTypes.AddNew().Code = "BIG";

			var company2RegistryValue = new CategorisedWorkflowTaskTypesCollection();
			company2RegistryValue.AddNew().Code = "STA";
			company2RegistryValue.AddNew().Code = new OpportunityWorkflowDescriptor().Code;
			company2RegistryValue[0].TaskTypes.AddNew().Code = "BOY";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RegistryValue);

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, company2RegistryValue);

			var company1Task = Factory.New<ProcessTask>();
			AssertEquals(true, company1Task.Lookups.Types.ContainsCode("BIG"));
			AssertEquals(false, company1Task.Lookups.Types.ContainsCode("BOY"));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var company2Task = Factory.New<ProcessTask>();
				AssertEquals(true, company2Task.Lookups.Types.ContainsCode("BOY"));
				AssertEquals(false, company2Task.Lookups.Types.ContainsCode("BIG"));

				AssertEquals(true, company1Task.Lookups.Types.ContainsCode("BIG"));
				AssertEquals(false, company1Task.Lookups.Types.ContainsCode("BOY"));
			}
		}

		#region CompletionMilestones

		public void TestCompletionMilestones()
		{
			var task = Dummy.WorkflowItems.Tasks.AddNew();

			AssertEquals(0, task.Lookups.CompletionMilestones.Count);

			var milestone1 = Dummy.WorkflowItems.Milestones.AddNew();
			var milestone2 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.EstimatedDateChangedCode;
			var milestone3 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = Events.ArrivalDocumentationReceivedCode;
			milestone3.P9_Description = "Mile";

			var actual = task.Lookups.CompletionMilestones
				.ToArray()
				.Select(cd => (new Guid(cd.PK.ToString()), cd.Code, cd.Description))
				.ToArray();

			var expected = Dummy.WorkflowItems.CompletionMilestoneCodeDescriptionPairList
				.ToArray()
				.Select(cd => (new Guid(cd.PK.ToString()), cd.Code, cd.Description))
				.ToArray();

			AssertContainsExactElementsInExactOrder(expected, actual);
		}

		#endregion

		#region Exception Types

		public void TestExceptionTypes()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var type1 = Factory.New<ProcessWorkflowExceptionType>();
			type1.WET_Code = "TP1";
			type1.WET_Description = nameof(type1);
			var type2 = Factory.New<ProcessWorkflowExceptionType>();
			type2.WET_Code = "TP2";
			type2.WET_Description = nameof(type2);
			var type1WithCategory1 = Factory.New<ProcessWorkflowExceptionType>();
			type1WithCategory1.WET_Code = "TC1";
			type1WithCategory1.WET_Description = nameof(type1WithCategory1);
			type1WithCategory1.WET_Category = "CT1";
			var type2WithCategory1 = Factory.New<ProcessWorkflowExceptionType>();
			type2WithCategory1.WET_Code = "TC2";
			type2WithCategory1.WET_Description = nameof(type2WithCategory1);
			type2WithCategory1.WET_Category = "CT1";
			var type3WithCategory2 = Factory.New<ProcessWorkflowExceptionType>();
			type3WithCategory2.WET_Code = "TC3";
			type3WithCategory2.WET_Description = nameof(type3WithCategory2);
			type3WithCategory2.WET_Category = "CT2";

			Factory.Save();

			var collection = exception.Lookups.ExceptionTypes;

			var filters = collection.FilterBusinessObjectDefaults;

			AssertEquals(2, filters.Count);

			Assert(filters.ContainsDefaultFor("Job Type:Property:1"));
			var jobType1Filter = filters["Job Type:Property:1"];
			AssertEquals(FilterOrCategory.Red, jobType1Filter.Category);
			AssertNull(jobType1Filter.Value);

			Assert(filters.ContainsDefaultFor("Job Type:Property:2"));
			var jobType2Filter = filters["Job Type:Property:2"];
			AssertEquals(FilterOrCategory.Red, jobType2Filter.Category);
			AssertEquals(jobType2Filter.Value, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			collection.Load();

			var systemExceptionTypes = Factory.Load<ProcessWorkflowExceptionType>(new ZQuery(ProcessWorkflowExceptionTypeSchema.WET_IsSystem, true));
			var expectedExceptionTypes = systemExceptionTypes.Union(new[] { type1, type2, type1WithCategory1, type2WithCategory1, type3WithCategory2 }).ToArray();

			AssertContainsExactElementsInAnyOrder(expectedExceptionTypes, collection.Cast<ProcessWorkflowExceptionType>().ToArray());

			exception.ExceptionTypeCategory = "CT1";
			collection = exception.Lookups.ExceptionTypes;
			filters = collection.FilterBusinessObjectDefaults;

			AssertEquals(3, filters.Count);

			Assert(filters.ContainsDefaultFor("Job Type:Property:1"));
			jobType1Filter = filters["Job Type:Property:1"];
			AssertEquals(FilterOrCategory.Red, jobType1Filter.Category);
			AssertNull(jobType1Filter.Value);

			Assert(filters.ContainsDefaultFor("Job Type:Property:2"));
			jobType2Filter = filters["Job Type:Property:2"];
			AssertEquals(FilterOrCategory.Red, jobType2Filter.Category);
			AssertEquals(jobType2Filter.Value, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			Assert(filters.ContainsDefaultFor("Category:Property"));
			var categoryFilter = filters["Category:Property"];
			AssertEquals(FilterOrCategory.None, categoryFilter.Category);
			AssertEquals("CT1", categoryFilter.Value);
		}

		public void TestExceptionTypeCategories()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertEquals(0, exception.Lookups.ExceptionTypeCategories.Count);

			var categories = new CodeDescriptionPairList();
			categories.AddPair("XXX", "last");
			categories.AddPair("HHH", "in the mid");
			categories.AddPair("AAA", "first");

			WorkflowDataRegistry.Instance.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(categories));

			Factory.ClearCachedValue<CodeDescriptionPairList>("ProcessTasksLookups.ProcessWorkflowExceptionTypeCategories");
			var categoriesToShow = exception.Lookups.ExceptionTypeCategories.ToList<ICodeDescription>().Select(c => c.Code).ToArray();

			AssertContainsExactElementsInExactOrder(new[] { "AAA", "HHH", "XXX" }, categoriesToShow);
		}

		public void TestExceptionCauses()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertEquals(0, exception.Lookups.ExceptionCauses.Count);

			var processWorkflowExceptionType1 = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType1.WET_Code = "TP1";
			processWorkflowExceptionType1.WET_Description = nameof(processWorkflowExceptionType1);
			var processWorkflowExceptionType2 = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType2.WET_Code = "TP2";
			processWorkflowExceptionType2.WET_Description = nameof(processWorkflowExceptionType2);

			exception.ExceptionTypeCode = processWorkflowExceptionType1.WET_Code;

			var cause1 = processWorkflowExceptionType1.Causes.AddNew();
			cause1.WEC_Code = "CA1";
			cause1.WEC_Description = nameof(cause1);

			var cause2 = processWorkflowExceptionType1.Causes.AddNew();
			cause2.WEC_Code = "CA2";
			cause2.WEC_Description = nameof(cause2);

			var cause3 = processWorkflowExceptionType1.Causes.AddNew();
			cause3.WEC_Code = "AAA";
			cause3.WEC_Description = nameof(cause3);

			var cause4 = processWorkflowExceptionType2.Causes.AddNew();
			cause4.WEC_Code = "XXX";
			cause4.WEC_Description = nameof(cause4);

			var cause5 = processWorkflowExceptionType2.Causes.AddNew();
			cause5.WEC_Code = "ZZZ";
			cause5.WEC_Description = nameof(cause5);

			Factory.Save();

			var causes = exception.Lookups.ExceptionCauses.ToList<ICodeDescription>().Select(c => c.Code).ToArray();
			AssertContainsExactElementsInExactOrder(new[] { "AAA", "CA1", "CA2" }, causes);

			exception.ExceptionTypeCode = processWorkflowExceptionType2.WET_Code;

			causes = exception.Lookups.ExceptionCauses.ToList<ICodeDescription>().Select(c => c.Code).ToArray();
			AssertContainsExactElementsInExactOrder(new[] { "XXX", "ZZZ" }, causes);
		}

		public void TestExceptionCauses_ShouldOnlyLoadActivedOrCurrent()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertEquals(0, exception.Lookups.ExceptionCauses.Count);

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TP1";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);

			exception.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;

			var cause1 = processWorkflowExceptionType.Causes.AddNew();
			cause1.WEC_Code = "CA1";
			cause1.WEC_Description = nameof(cause1);

			var cause2 = processWorkflowExceptionType.Causes.AddNew();
			cause2.WEC_Code = "CA2";
			cause2.WEC_Description = nameof(cause2);

			var cause3 = processWorkflowExceptionType.Causes.AddNew();
			cause3.WEC_Code = "CA3";
			cause3.WEC_Description = nameof(cause1);
			cause3.WEC_IsActive = false;

			Factory.Save();

			var causes = exception.Lookups.ExceptionCauses.ToList<ICodeDescription>().Select(c => c.Code).ToArray();
			AssertContainsExactElementsInExactOrder(new[] { "CA1", "CA2" }, causes);

			exception.ExceptionCausePK = cause3.PK;
			cause1.WEC_IsActive = false;

			Factory.ClearCachedValue<CodeDescriptionPairList>("ProcessTasksLookups.ExceptionCauses" + processWorkflowExceptionType.PK);
			Factory.Save();

			causes = exception.Lookups.ExceptionCauses.ToList<ICodeDescription>().Select(c => c.Code).ToArray();
			AssertContainsExactElementsInExactOrder(new[] { "CA2", "CA3" }, causes);
		}

		public void TestExceptionResolutions()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertEquals(0, exception.Lookups.ExceptionResolutions.Count);

			var processWorkflowExceptionType1 = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType1.WET_Code = "TP1";
			processWorkflowExceptionType1.WET_Description = nameof(processWorkflowExceptionType1);
			var processWorkflowExceptionType2 = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType2.WET_Code = "TP2";
			processWorkflowExceptionType2.WET_Description = nameof(processWorkflowExceptionType2);

			exception.ExceptionTypeCode = processWorkflowExceptionType1.WET_Code;

			var resolution1 = processWorkflowExceptionType1.Resolutions.AddNew();
			resolution1.WER_Code = "RE1";
			resolution1.WER_Description = nameof(resolution1);

			var resolution2 = processWorkflowExceptionType1.Resolutions.AddNew();
			resolution2.WER_Code = "RE2";
			resolution2.WER_Description = nameof(resolution2);

			var resolution3 = processWorkflowExceptionType1.Resolutions.AddNew();
			resolution3.WER_Code = "AAA";
			resolution3.WER_Description = nameof(resolution3);

			var resolution4 = processWorkflowExceptionType2.Resolutions.AddNew();
			resolution4.WER_Code = "XXX";
			resolution4.WER_Description = nameof(resolution4);

			var resolution5 = processWorkflowExceptionType2.Resolutions.AddNew();
			resolution5.WER_Code = "ZZZ";
			resolution5.WER_Description = nameof(resolution5);

			Factory.Save();

			var resolutions = exception.Lookups.ExceptionResolutions.ToList<ICodeDescription>().Select(c => c.Code).ToArray();
			AssertContainsExactElementsInExactOrder(new[] { "AAA", "RE1", "RE2" }, resolutions);

			exception.ExceptionTypeCode = processWorkflowExceptionType2.WET_Code;

			resolutions = exception.Lookups.ExceptionResolutions.ToList<ICodeDescription>().Select(c => c.Code).ToArray();
			AssertContainsExactElementsInExactOrder(new[] { "XXX", "ZZZ" }, resolutions);
		}

		public void TestExceptionResolutions_ShouldOnlyLoadActivedOrCurrent()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertEquals(0, exception.Lookups.ExceptionResolutions.Count);

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TP1";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);

			exception.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;

			var resolution1 = processWorkflowExceptionType.Resolutions.AddNew();
			resolution1.WER_Code = "RE1";
			resolution1.WER_Description = nameof(resolution1);

			var resolution2 = processWorkflowExceptionType.Resolutions.AddNew();
			resolution2.WER_Code = "RE2";
			resolution2.WER_Description = nameof(resolution2);

			var resolution3 = processWorkflowExceptionType.Resolutions.AddNew();
			resolution3.WER_Code = "RE3";
			resolution3.WER_Description = nameof(resolution1);
			resolution3.WER_IsActive = false;

			Factory.Save();

			var resolutions = exception.Lookups.ExceptionResolutions.ToList<ICodeDescription>().Select(c => c.Code).ToArray();
			AssertContainsExactElementsInExactOrder(new[] { "RE1", "RE2" }, resolutions);

			exception.ExceptionResolutionPK = resolution3.PK;
			resolution1.WER_IsActive = false;

			Factory.ClearCachedValue<CodeDescriptionPairList>("ProcessTasksLookups.ExceptionResolutions" + processWorkflowExceptionType.PK);
			Factory.Save();

			resolutions = exception.Lookups.ExceptionResolutions.ToList<ICodeDescription>().Select(c => c.Code).ToArray();
			AssertContainsExactElementsInExactOrder(new[] { "RE2", "RE3" }, resolutions);
		}

		#endregion

		public void TestEstimateDefaultedFromList_ForShipment()
		{
			ProcessTaskTemplate milestoneTemplate = Factory.New<ProcessTaskTemplate>();
			milestoneTemplate.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			ProcessTask milestone1 = milestoneTemplate.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = milestoneTemplate.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone3 = milestoneTemplate.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.CargoAvailable.Code;
			milestone1.P9_Description = "Milestone1";
			milestone2.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			milestone2.P9_Description = "Milestone2";
			milestone3.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone3.P9_Description = "Milestone3";

			AssertEquals(milestone2.P9_Sequence.ToString() + " - " + milestone2.P9_SE_NKMilestoneEvent + " (est)", milestone1.Lookups.EstimateDefaultedFromList[0].Code);
			AssertEquals(milestone2.P9_Description, milestone1.Lookups.EstimateDefaultedFromList[0].Description);
			AssertEquals(milestone2.P9_Sequence.ToString() + " - " + milestone2.P9_SE_NKMilestoneEvent + " (act)", milestone1.Lookups.EstimateDefaultedFromList[1].Code);
			AssertEquals(milestone2.P9_Description, milestone1.Lookups.EstimateDefaultedFromList[1].Description);
			AssertEquals(milestone3.P9_Sequence.ToString() + " - " + milestone3.P9_SE_NKMilestoneEvent + " (est)", milestone1.Lookups.EstimateDefaultedFromList[2].Code);
			AssertEquals(milestone3.P9_Description, milestone1.Lookups.EstimateDefaultedFromList[2].Description);
			AssertEquals(milestone3.P9_Sequence.ToString() + " - " + milestone3.P9_SE_NKMilestoneEvent + " (act)", milestone1.Lookups.EstimateDefaultedFromList[3].Code);
			AssertEquals(milestone3.P9_Description, milestone1.Lookups.EstimateDefaultedFromList[3].Description);
			AssertEquals("", milestone1.Lookups.EstimateDefaultedFromList[4].Code);
			AssertEquals("", milestone1.Lookups.EstimateDefaultedFromList[4].Description);
			AssertEquals(ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolDischargeETA, milestone1.Lookups.EstimateDefaultedFromList[5].Code);
			AssertEquals(ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolLoadingETD, milestone1.Lookups.EstimateDefaultedFromList[6].Code);
			AssertEquals(ForwardingShipmentEstimateDefaultedFromList.Codes.FCLAvailable, milestone1.Lookups.EstimateDefaultedFromList[7].Code);
			AssertEquals(ForwardingShipmentEstimateDefaultedFromList.Codes.FCLStorage, milestone1.Lookups.EstimateDefaultedFromList[8].Code);
			AssertEquals(ForwardingShipmentEstimateDefaultedFromList.Codes.LCLAvailable, milestone1.Lookups.EstimateDefaultedFromList[9].Code);
			AssertEquals(ForwardingShipmentEstimateDefaultedFromList.Codes.LCLStorage, milestone1.Lookups.EstimateDefaultedFromList[10].Code);
			AssertEquals(ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA, milestone1.Lookups.EstimateDefaultedFromList[11].Code);
			AssertEquals(ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD, milestone1.Lookups.EstimateDefaultedFromList[12].Code);
		}

		public void TestEstimateDefaultedFromList_ForConsol()
		{
			ProcessTaskTemplate milestoneTemplate = Factory.New<ProcessTaskTemplate>();
			milestoneTemplate.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;

			ProcessTask milestone1 = milestoneTemplate.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = milestoneTemplate.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone3 = milestoneTemplate.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.CargoAvailable.Code;
			milestone1.P9_Description = "Milestone1";
			milestone2.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			milestone2.P9_Description = "Milestone2";
			milestone3.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone3.P9_Description = "Milestone3";

			AssertEquals(milestone2.P9_Sequence.ToString() + " - " + milestone2.P9_SE_NKMilestoneEvent + " (est)", milestone1.Lookups.EstimateDefaultedFromList[0].Code);
			AssertEquals(milestone2.P9_Description, milestone1.Lookups.EstimateDefaultedFromList[0].Description);
			AssertEquals(milestone2.P9_Sequence.ToString() + " - " + milestone2.P9_SE_NKMilestoneEvent + " (act)", milestone1.Lookups.EstimateDefaultedFromList[1].Code);
			AssertEquals(milestone2.P9_Description, milestone1.Lookups.EstimateDefaultedFromList[1].Description);
			AssertEquals(milestone3.P9_Sequence.ToString() + " - " + milestone3.P9_SE_NKMilestoneEvent + " (est)", milestone1.Lookups.EstimateDefaultedFromList[2].Code);
			AssertEquals(milestone3.P9_Description, milestone1.Lookups.EstimateDefaultedFromList[2].Description);
			AssertEquals(milestone3.P9_Sequence.ToString() + " - " + milestone3.P9_SE_NKMilestoneEvent + " (act)", milestone1.Lookups.EstimateDefaultedFromList[3].Code);
			AssertEquals(milestone3.P9_Description, milestone1.Lookups.EstimateDefaultedFromList[3].Description);
			AssertEquals("", milestone1.Lookups.EstimateDefaultedFromList[4].Code);
			AssertEquals("", milestone1.Lookups.EstimateDefaultedFromList[4].Description);
			AssertEquals(ForwardingConsolEstimateDefaultedFromList.Codes.DischargeETA, milestone1.Lookups.EstimateDefaultedFromList[5].Code);
			AssertEquals(ForwardingConsolEstimateDefaultedFromList.Codes.FCLAvailable, milestone1.Lookups.EstimateDefaultedFromList[6].Code);
			AssertEquals(ForwardingConsolEstimateDefaultedFromList.Codes.FCLStorage, milestone1.Lookups.EstimateDefaultedFromList[7].Code);
			AssertEquals(ForwardingConsolEstimateDefaultedFromList.Codes.LCLAvailable, milestone1.Lookups.EstimateDefaultedFromList[8].Code);
			AssertEquals(ForwardingConsolEstimateDefaultedFromList.Codes.LCLStorage, milestone1.Lookups.EstimateDefaultedFromList[9].Code);
			AssertEquals(ForwardingConsolEstimateDefaultedFromList.Codes.LoadingETD, milestone1.Lookups.EstimateDefaultedFromList[10].Code);
		}

		public void TestStatuses()
		{
			AssertEquals(typeof(ProcessTaskStatusCodeList), ProcessTask.Lookups.Statuses.GetType());

			ProcessTask.IsException = true;
			AssertEquals(typeof(ExceptionStatusCodeList), ProcessTask.Lookups.Statuses.GetType());
		}

		#region Workflow Triggers

		public void TestWorkflowTriggerFieldNameList()
		{
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_NVarCharMax);
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_Money);
			AssertEquals("Z0_NVarCharMax", trigger.TriggerConditions.Lookups.WorkflowTriggerFieldNames[0].Code);
			AssertEquals("Z0_Money", trigger.TriggerConditions.Lookups.WorkflowTriggerFieldNames[1].Code);
		}

		#endregion

		#region LineTriggerTypes

		public void TestLineTriggerTypesGetter_ParentIsBusinessObject_ReturnTypesFromWorflowDescriptor()
		{
			var sampleTypes = new List<string>
			{
				TriggerLineTypes.Codes.ForwardingShipment
			};

			var worflowDescriptor = (DummyWorkflowDescriptor)ProcessTask.WorkflowDescriptor;
			worflowDescriptor.SupportedTriggerLineTypes = sampleTypes;

			var lookup = new ProcessTasksLookups(ProcessTask);
			var actualTypes = lookup.LineTriggerTypes.Cast<CodeDescriptionPair>().Select(p => p.Code).ToArray();

			sampleTypes.Add(ProcessTasksLookups.TaskLineTriggerCode);
			sampleTypes.Add(ProcessTasksLookups.ExceptionLineTriggerCode);

			AssertContainsExactElementsInAnyOrder("Types", sampleTypes, actualTypes.ToArray());
		}

		#endregion

		#region ProcessHeaders

		public void TestProcessHeaders()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");

			var jobLevelWorkflow = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(jobLevelWorkflow, "Do crows drink in bars?");
			var task1 = (ProcessTask)helper.CreateTask(workflow1);
			AssertEquals("What's a cache?", true, object.ReferenceEquals(task1.Lookups.ProcessHeaders, task1.Lookups.ProcessHeaders));
		}

		#endregion

		#region Iterations

		public void TestIterations()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");

			var jobLevelWorkflow = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(jobLevelWorkflow, "Workflow 1");
			var workflow2 = helper.CreateWorkflow(jobLevelWorkflow, "Workflow 2");
			var workflow3 = helper.CreateWorkflow(jobLevelWorkflow, "Workflow 3");

			var task1 = helper.CreateTask(workflow1);
			var task2 = helper.CreateTask(workflow1, taskType: "CB");
			var task3 = helper.CreateTask(workflow2);
			var task4 = helper.CreateTask(workflow2, taskType: "CB");

			helper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			helper.CreateQualityIteration(task3, task4, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = workflow2.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = workflow2.Tasks.Single(x => x.P9_Sequence == 4);

			helper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = workflow2.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask2 = workflow2.Tasks.Single(x => x.P9_Sequence == 4);

			helper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "Quality Iteration", shouldCreateWorkflowForIteration: true); // creating a new workflow for this one.

			var newTaskInWorkflowWithOneIteration = helper.CreateTask(workflow1);
			AssertContainsExactElementsInAnyOrder(new[] { "1 - Iteration 1 for [Workflow 1]" }, GetCodesAndDescriptionsForTaskLookups(newTaskInWorkflowWithOneIteration));

			var newTaskInWorkflowWithTwoIterations = helper.CreateTask(workflow2);
			AssertContainsExactElementsInAnyOrder(new[] { "1 - Iteration 1 for [Workflow 2]", "2 - Iteration 2 for [Workflow 2]", }, GetCodesAndDescriptionsForTaskLookups(newTaskInWorkflowWithTwoIterations)); // doesn't contain option for the child workflow.

			var newTaskInWorkflowWithNoIterations = helper.CreateTask(workflow3);
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), GetCodesAndDescriptionsForTaskLookups(newTaskInWorkflowWithNoIterations));
		}

		public void TestIterations_ShouldNotContainDuplicates()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");

			var jobLevelWorkflow = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = helper.CreateTask(workflow);
			var task2 = helper.CreateTask(workflow, taskType: "CB");

			helper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask = workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask = workflow.Tasks.Single(x => x.P9_Sequence == 4);

			helper.CreateQualityIteration(iterationTask, containmentBarrierIterationTask, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var iterations = Factory.Load<IProcessTaskIterationLink>(new ZQuery());

			AssertEquals(2, iterations.Length);
			iterations[0].P9I_Sequence = 1;
			iterations[1].P9I_Sequence = 1;

			AssertContainsExactElementsInAnyOrder("Duplicates should not appear in the lookups even if multiple iterations somehow exist with the same sequence number.", new[] { "1 - Iteration 1 for [Workflow]" }, GetCodesAndDescriptionsForTaskLookups(task1));
		}

		public void TestIterations_ShouldRefreshEachTimeTheyAreAccessed()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");

			var jobLevelWorkflow = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = helper.CreateTask(workflow);
			var task2 = helper.CreateTask(workflow, taskType: "CB");

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), GetCodesAndDescriptionsForTaskLookups(task1));

			helper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			AssertContainsExactElementsInAnyOrder("This list must be refreshed each time it is accessed because the available options may have changed during the user's session.",
				new[] { "1 - Iteration 1 for [Workflow]" }, GetCodesAndDescriptionsForTaskLookups(task1));
		}

		public void TestIterations_SortOrder()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");

			var jobLevelWorkflow = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobLevelWorkflow, "Workflow");

			var task1 = helper.CreateTask(workflow);
			var task2 = helper.CreateTask(workflow, taskType: "CB");

			helper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask = workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask = workflow.Tasks.Single(x => x.P9_Sequence == 4);

			helper.CreateQualityIteration(iterationTask, containmentBarrierIterationTask, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var iterations = Factory.Load<IProcessTaskIterationLink>(new ZQuery() { OrderBy = ProcessTaskIterationLinkSchema.Constants.P9I_Sequence });

			AssertEquals(2, iterations.Length);
			AssertEquals((ZByte)1, iterations[0].P9I_Sequence);
			AssertEquals((ZByte)2, iterations[1].P9I_Sequence);

			iterations[0].P9I_Sequence = 2;
			iterations[1].P9I_Sequence = 1;

			AssertContainsExactElementsInAnyOrder("Lookups should always be in sequence order, even if they're not in that order in the database.",
				new[] { "1 - Iteration 1 for [Workflow]", "2 - Iteration 2 for [Workflow]" }, GetCodesAndDescriptionsForTaskLookups(task1));
		}

		public void TestIterations_ShouldNotContainOtherKindsOfIterationLinks()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");

			var jobLevelWorkflow = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobLevelWorkflow, "Workflow");
			var task1 = (ProcessTask)helper.CreateTask(workflow);
			var task2 = (ProcessTask)helper.CreateTask(workflow, taskType: "CB");

			void CreateIterationLink(string linkType, string outcome, byte sequence)
			{
				var iterationLink = (IProcessTaskIterationLink)task2.IterationLinks.AddNew();
				iterationLink.P9I_FH_IterationWorkflow = workflow.PK;
				iterationLink.P9I_P9_IterationTask = task1.PK;
				iterationLink.P9I_LinkType = linkType;
				iterationLink.P9I_Outcome = outcome;
				iterationLink.P9I_Sequence = sequence;
			}

			CreateIterationLink(IterationLinkTypeList.Codes.PassedContainmentBarrier, IterationLinkOutcomeList.Codes.Passed, 1);
			CreateIterationLink(IterationLinkTypeList.Codes.QualityIterationTask, IterationLinkOutcomeList.Codes.IterationRequired, 2);
			CreateIterationLink(IterationLinkTypeList.Codes.QualityIterationTask, IterationLinkOutcomeList.Codes.Deferred, 3);
			CreateIterationLink(IterationLinkTypeList.Codes.PassedContainmentBarrier, IterationLinkOutcomeList.Codes.IterationRequired, 4);

			AssertContainsExactElementsInAnyOrder("Only links with Type = 'QIT' and Outcome = 'ITR' should be included as options in the lookups.",
				new[] { "2 - Iteration 2 for [Workflow]" }, GetCodesAndDescriptionsForTaskLookups(task1));
		}

		static IEnumerable<string> GetCodesAndDescriptionsForTaskLookups(IProcessTask task)
		{
			return ((ProcessTask)task).Lookups.Iterations.Cast<CodeDescriptionPair>().Select(x => x.CodeAndDescription);
		}

		#endregion

		#region Implementation

		CategorisedWorkflowTaskTypesCollection originalTaskTypes;

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		ProcessTask ProcessTask
		{
			get { return processTask ?? (processTask = Dummy.WorkflowItems.AddNew()); }
		}
		ProcessTask processTask;

		protected override void SetUp()
		{
			base.SetUp();
			originalTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected override void TearDown()
		{
			base.TearDown();
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalTaskTypes);
		}

		#endregion
	}
}
