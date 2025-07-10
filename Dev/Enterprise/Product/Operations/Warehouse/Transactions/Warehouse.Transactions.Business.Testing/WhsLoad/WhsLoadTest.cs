using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLoad))]
	class WhsLoadTest : WhsBusinessObjectTestCase
	{
		#region TestReadOnlyProperties

		public void TestReadOnlyProperties()
		{
			var load = Factory.New<WhsLoad>();
			var fieldsTester = ObjectFactory.Get<IOperationalActionFieldTester>();

			foreach (var property in load
										.ZPropertyInfoHash
										.Cast<ZPropertyInfo>()
										.Where(pi => !pi.Name.StartsWith("WLO_System")))
			{
				CombineAssertions(
					$"{property.Name} should be readonly.",
					() =>
					{
						AssertEquals("Should be readonly", true, property.ReadOnly);

						var actionFieldAttribute = ActionFieldAttribute.Get(typeof(WhsLoad).GetProperty(property.Name));
						if (actionFieldAttribute != null)
						{
							AssertEquals($"Action field attribute for {property.Name} must be read-only.", true,
								actionFieldAttribute.ReadOnly);
						}
						else
						{
							AssertEquals(true, fieldsTester.IsFieldUnsupported(typeof(WhsLoad), property.Name));
						}
					});
			}
		}

		#endregion

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			var load = Factory.New<WhsLoad>();
			AssertEquals(true, load.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var load = Factory.New<WhsLoad>();
			AssertEquals("Load Planning", load.HumanReadableName);

			load.WLO_JobID = "WL00000001";
			AssertEquals("Load Planning WL00000001", load.HumanReadableName);
		}

		#endregion

		#region TestCarrierServiceLevel

		public void TestCarrierServiceLevel()
		{
			var load = Factory.New<WhsLoad>();

			AssertNull("Precondition", load.CarrierServiceLevel);

			var transportCo = Factory.New<OrgHeader>();
			load.WLO_OH_TransportCompany = transportCo.PK;
			load.WLO_PL_NKCarrierServiceLevel = "XXX";
			AssertNull(load.CarrierServiceLevel);

			var service = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			service.PL_Code = "XXX";
			AssertEquals(service.PK, load.CarrierServiceLevel.PK);

			service.PL_Code = "xxx";
			AssertEquals(service.PK, load.CarrierServiceLevel.PK);

			load.WLO_PL_NKCarrierServiceLevel = "ZZZ";
			AssertNull(load.CarrierServiceLevel);
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var load = Factory.New<WhsLoad>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] { PredefinedNoteTypes.Instance.InternalWorkNotes }, load.NoteTypes);
		}

		#endregion

		#region IWorkflowProvider

		public void TestOnFactorySavingBeforeTransaction_IWorkflowProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var activeTemplateWithWhs = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplateWithWhs.P0_Name = "ActiveTemplateForLoad";
			activeTemplateWithWhs.P0_WW = data.Whs1.PK;
			activeTemplateWithWhs.P0_ProcessType = "WLO";
			activeTemplateWithWhs.P0_IsActive = true;

			var trigger = activeTemplateWithWhs.WorkflowItems.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ServiceRequestedCode;
			trigger.P9_Type = "TRG";
			trigger.P9_Description = "Test";
			Factory.Save();

			var load = Helper.CreateWhsLoad(Helper.CreateClient(), data.Whs1.DefaultOutboundDockDoorLocation);
			AssertEquals("Precondition: Count of Workflow Items is zero.", 0, load.WorkflowItems.Count);
			load.WLO_JobID = "WL00000002";

			Factory.Save();
			AssertEquals("Have new Workflow Items", 1, load.WorkflowItems.Count);
		}

		public void TestGetTemplateFilterCriteria_ColumnRanksCorrect_IWorkflowProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var load = Helper.CreateWhsLoad(Helper.CreateClient(), data.Whs1.DefaultOutboundDockDoorLocation);

			var ranker = ((IWorkflowProvider)load).GetTemplateSelectionCriteria();
			var columnValues = ((IColumnValueRankerInternals)ranker).ColumnValues.ToArray();

			AssertArrayEqualsByElements(
				new[] {
					$"P0_WW - {load.PlannedDockDoor.WLV_WW_Whs}, 00000000-0000-0000-0000-000000000000"
				},
				columnValues.Select(cv => $"{cv.ColumnName} - {string.Join(", ", cv.Values)}").ToArray());
		}

		public void TestOnDelete_IWorkflowProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var load = Helper.CreateWhsLoad(Helper.CreateClient(), data.Whs1.DefaultOutboundDockDoorLocation);

			load.WorkflowItems.AddNew();

			AssertEquals("Have one Workflow Items", 1, load.WorkflowItems.Count);

			load.Delete();

			AssertEquals("Workflow Items all removed", 0, load.WorkflowItems.Count);
		}

		#endregion

		// interfaces

		#region eDocs

		public void TestIEDocsProvider_GetEDocsProviderSupporter()
		{
			var whsLoad = Factory.New<WhsLoad>();
			var whsLoadEDocProvider = ((IEDocsProvider)whsLoad).GetEDocsProviderSupporter();
			AssertNotNull(whsLoadEDocProvider);
			AssertType<JobInvoicingEDocsProviderSupporter>(whsLoadEDocProvider);
		}

		public void TestIDocumentSupportable_DocumentSupporter()
		{
			var whsLoad = Factory.New<WhsLoad>();
			var whsLoadEDocProvider = (IDocumentSupportable)whsLoad;
			AssertNotNull(whsLoadEDocProvider.DocumentSupporter);
			AssertType<WhsLoadDocumentSupporter>(whsLoadEDocProvider.DocumentSupporter);
			AssertEquals(whsLoad, whsLoadEDocProvider.DocumentSupporter.BusinessObject);
		}

		public void TestIDocManagerSupport_DocManagerInfo()
		{
			var whsLoad = Factory.New<WhsLoad>();
			var info = ((IDocManagerSupport)whsLoad).DocManagerInfo;
			AssertEquals(whsLoad, info.BusinessEntity);
			AssertEquals(Constants.DocManagerCodes.WarehouseLoad, info.DocManagerCode);
		}

		#endregion

		#region INumberFountainConsumer

		public void TestINumberFountainConsumer()
		{
			var load = Factory.New<WhsLoad>();
			AssertEquals("Load uses correct Fountain.", Env.NumberFountains.WarehouseLoadID, ((INumberFountainConsumer)load).Fountain);

			load.WLO_JobID = "WL01";
			AssertEquals("ID refers to correct Field.", "WL01", ((INumberFountainConsumer)load).ID);

			((INumberFountainConsumer)load).ID = "WL02";
			AssertEquals("ID refers to correct Field.", "WL02", load.WLO_JobID);
		}

		public void TestINumberFountainConsumer_SavingSetsJobID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var load = Helper.CreateWhsLoad(Helper.CreateClient(), data.Whs1.DefaultOutboundDockDoorLocation);
			AssertEquals("Precondition: Job ID is empty.", "", load.WLO_JobID);

			Factory.Save();
			AssertNotEquals("Job ID should not be empty.", "", load.WLO_JobID);
			AssertStartsWith("Job ID should start with the correct prefix.", "WL", load.WLO_JobID);
		}

		#endregion

		#region ITaskPlanningJob

		public void TestITaskPlanningJob_BasicProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL001");

			var job = (ITaskPlanningJob)load;
			CombineAssertions(() =>
			{
				AssertEquals(data.Whs1.PK, job.WarehousePK);
				AssertEquals(Factory, job.Factory);
				AssertEquals("WL001", job.JobID);
				AssertEquals("Load Planning", job.HumanReadableNameWithoutID);
			});
		}

		public void TestITaskPlanningJob_TaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);

			var job = (ITaskPlanningJob)load;
			AssertEquals(string.Empty, job.TaskPlanningStatus);

			job.TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals("RFP", job.TaskPlanningStatus);

			job.TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertEquals("NRP", job.TaskPlanningStatus);
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);
			Factory.Save();

			var job = (ITaskPlanningJob)load;
			AssertEquals(true, job.IsInDatabase);
			AssertEquals(false, job.HasChanges);
			AssertEquals(false, job.IsFinalisedOrCancelled);
			AssertEquals(string.Empty, job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals(string.Empty, job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason_NotInDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);

			var job = (ITaskPlanningJob)load;
			AssertEquals(false, job.IsInDatabase);
			AssertEquals("Cannot change Task Planning Status as the Load Planning is not saved.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals("Cannot change Task Planning Status as the Load Planning is not saved.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason_ValueChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);
			Factory.Save();

			var job = (ITaskPlanningJob)load;
			AssertEquals(string.Empty, job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals(string.Empty, job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));

			job.TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals(true, job.HasChanges);
			AssertEquals("Cannot change Task Planning Status as the Load Planning is not saved.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals("Cannot change Task Planning Status as the Load Planning is not saved.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason_GateOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);
			load.WLO_TransportationUnitNumber = "1234";
			load.WLO_StartTime = ZDateTimeOffset.Now;
			load.WLO_CompleteTime = ZDateTimeOffset.Now;
			load.WLO_GateOutTime = ZDateTimeOffset.Now;
			Factory.Save();

			var job = (ITaskPlanningJob)load;
			AssertEquals(true, job.IsFinalisedOrCancelled);
			AssertEquals("Cannot change Task Planning Status as the Load Planning is gate out.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals("Cannot change Task Planning Status as the Load Planning is gate out.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		#endregion
	}
}
