using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(StatementProcessTask))]
	sealed class StatementProcessTaskTest : ProcessTaskTest
	{
		public void TestParent()
		{
			void AssertParentType(string countryCode, Type expectedType)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var task = Factory.New<StatementProcessTask>();

					var statementHeader = Factory.New<BaseCusStatementHeader>();
					task.P9_ParentID = statementHeader.PK;
					task.P9_ParentTableCode = CusStatementHeaderSchema.Constants.Prefix;

					Factory.Save();

					var newFactory = NewFactory();
					var newTask = newFactory.Load<StatementProcessTask>(task.PK);

					AssertEquals(expectedType, newTask.Parent.GetType());
				}
			}

			AssertParentType(Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusStatementHeader>());
			AssertParentType(Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.US.ICusStatementHeader>());
		}

		public void TestStatementWorkflow()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsActive = true;
			template.P0_IsSystem = false;
			template.P0_ProcessType = StatementProcessTask.StatementWorkflow.Code;

			var processTask = template.WorkflowItems.Triggers.AddNew();
			processTask.P9_Description = "TEST";
			processTask.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;

			var notification = processTask.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "dong.nguyen@cargowise.com";
			notification.PQ_EmailText = "Dong is fatter than me";

			Factory.Save();

			var statement = Factory.New<BaseCusStatementHeader>();
			statement.B2_StatementNumber = "456464656";
			Factory.Save();

			AssertEquals(1, statement.WorkflowItems.Count);
		}

		public void TestWorkFlowTypeDeciderLoadsStatementProcessTask()
		{
			var statement = Factory.New<BaseCusStatementHeader>();
			statement.B2_StatementNumber = "456464656";

			var processTask = Factory.New<ProcessTask>();
			processTask.P9_Description = "TEST";
			processTask.TriggerConditions.TriggerEventCode = AutoEvents.AddedARecordToTheSystem.Code;
			processTask.P9_ParentID = statement.PK;
			processTask.P9_ParentTableCode = CusStatementHeaderSchema.Constants.Prefix;

			Factory.Save();

			var type = ProcessTask.TypeDecider.GetTypeForLoad(((INeedRow)processTask).Row, new BusinessObjectFactory());
			AssertEquals(typeof(StatementProcessTask), type);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return WorkflowItems.AddNew();
		}

		ProcessTaskCollection WorkflowItems => Factory.New<BaseCusStatementHeader>().WorkflowItems;
	}
}
