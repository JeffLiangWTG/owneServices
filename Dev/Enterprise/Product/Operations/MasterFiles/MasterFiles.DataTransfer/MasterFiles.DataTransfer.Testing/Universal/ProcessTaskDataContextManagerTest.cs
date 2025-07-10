using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Testing.Universal
{
	[TestedType(typeof(ProcessTaskDataContextManager))]
	internal class ProcessTaskDataContextManagerTest : DataContextManagerTestCase<ProcessTaskDataContextManager, ProcessTask>
	{
		public void TestEventCanBeImportedAgainstATaskAndUpdateFields()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.AddNew();
			task.P9_Type = "UDF";

			Factory.SaveForTesting();

			AssertEquals("Precondition: task.P9_Status", "OPN", task.P9_Status);

			string eventText = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>WorkflowTask</Type>
		  <Key>{task.P9_TaskID}</Key>
		</DataTarget>
	  </DataTargetCollection>

			<CodesMappedToTarget>true</CodesMappedToTarget>
	</DataContext>

	<EventTime>2022-10-14T12:30:21.007</EventTime>
	<EventType>Z00</EventType>
	<CreatedTime>2022-10-13T23:30:23.007</CreatedTime>
	<EventReference>Powered by Internet Explorer</EventReference>
	<IsEstimate>false</IsEstimate>

	<ContextCollection>
	  <Context>
		<Type>TaskType</Type>
		<Value>UDF</Value>
	  </Context>
	</ContextCollection>

		<AdditionalFieldsToUpdateCollection>
			<AdditionalFieldsToUpdate>
		  <Type>ProcessTasks.P9_Status</Type>
		<Value>CLS</Value>
	  </AdditionalFieldsToUpdate>
			<AdditionalFieldsToUpdate>
		  <Type>ProcessTasks.P9_NotesAsString</Type>
		<Value>https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/109672?_a=overview</Value>
	  </AdditionalFieldsToUpdate>
		</AdditionalFieldsToUpdateCollection>
  </Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(eventText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			var taskReloaded = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
			CombineAssertions(() =>
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertContains("Message Log Note", $"Linked Event to {taskReloaded.HumanReadableName}.", message.GetLogNoteText());
				AssertEquals("task.P9_Status", "CLS", taskReloaded.P9_Status);
				AssertEquals("task.P9_NotesAsString", "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/109672?_a=overview", taskReloaded.P9_NotesAsString);
			});

			var logEvent = taskReloaded.GetLogs().Find(e => e.SL_SE_NKEvent == "Z00").FirstOrDefault();
			AssertNotNull(logEvent);
			AssertEquals("Powered by Internet Explorer", logEvent.SL_Reference);
		}
	}
}
