using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	public class JobShipmentPreplanningFilterControlBashFetchTest : FilterControlBashFetchHintTest<JobShipmentPreplanning>
	{
		#region BashFetchTest

		public void TestBashFetchForView_EF_PreshipID()
		{
			BashFetchForView("EF_PreshipID", 0);
		}

		public void TestBashFetchForView_BuyerPK()
		{
			BashFetchForView("BuyerPK", 1);
		}

		public void TestBashFetchForView_EF_RL_NKPortLoad()
		{
			BashFetchForView("EF_RL_NKPortLoad", 0);
		}

		public void TestBashFetchForView_EF_RL_NKPortDisch()
		{
			BashFetchForView("EF_RL_NKPortDisch", 0);
		}

		public void TestBashFetchForView_EF_OH_SendingAgent()
		{
			BashFetchForView("EF_OH_SendingAgent", 0);
		}

		public void TestBashFetchForView_EF_OH_ReceivingAgent()
		{
			BashFetchForView("EF_OH_ReceivingAgent", 0);
		}

		public void TestBashFetchForView_EF_OH_Carrier()
		{
			BashFetchForView("EF_OH_Carrier", 0);
		}

		public void TestBashFetchForView_EF_MasterBill()
		{
			BashFetchForView("EF_MasterBill", 0);
		}

		public void TestBashFetchForView_EF_HouseBill()
		{
			BashFetchForView("EF_HouseBill", 0);
		}

		public void TestBashFetchForView_EF_JS()
		{
			BashFetchForView("EF_JS", 0);
		}

		public void TestBashFetchForView_EF_JE()
		{
			BashFetchForView("EF_JE", 0);
		}

		public void TestBashFetchForView_EF_SystemCreateUser()
		{
			BashFetchForView("EF_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_EF_SystemCreateBranch()
		{
			BashFetchForView("EF_SystemCreateBranch", 0);
		}

		public void TestBashFetchForView_EF_SystemCreateDepartment()
		{
			BashFetchForView("EF_SystemCreateDepartment", 0);
		}

		public void TestBashFetchForView_EF_SystemCreateTimeUtc()
		{
			BashFetchForView("EF_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_EF_SystemLastEditUser()
		{
			BashFetchForView("EF_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_EF_SystemLastEditTimeUtc()
		{
			BashFetchForView("EF_SystemLastEditTimeUtc", 0);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_Description()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_Description", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_ActualDateForBinding()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_Description()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_Description", 12);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_ScheduledDateForBinding()
		{
			// ProcessTasks: 12

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding", 12);
		}

		#endregion

		protected override SchemaPKColumn PkColumn => JobShipmentPreplanningSchema.PK;

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			for (var i = 0; i < 12; i++)
			{
				var buyer = factory.NewWithValidTestData<OrgHeader>();

				var preAdvice = factory.New<JobShipmentPreplanning>();
				preAdvice.BuyerPK = buyer.PK;

				var order = preAdvice.Orders.AddNew();
				order.FillWithValidTestData();
				order.BuyerPK = buyer.PK;

				var milestone = preAdvice.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				milestone = preAdvice.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Invalid);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				milestone = preAdvice.WorkflowItems.MilestonesIncludingRelated.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				milestone = preAdvice.WorkflowItems.MilestonesIncludingRelated.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Invalid);
				milestone.P9_GC = GlbCompany.CurrentCompany.PK;

				result.Add(preAdvice.PK);
			}

			factory.Save();

			return result.ToArray();
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var collection = new JobShipmentPreplanningCollection(Factory);
			var filterBusinessObject = new JobShipmentPreplanningFilterBusinessObject();
			return new JobShipmentPreplanningFilterControl(collection, filterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new JobShipmentPreplanningCollection(Factory);
		}
	}
}
