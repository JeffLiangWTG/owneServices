using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class StmALogEntryLocatorTest : TestCaseWithFactory
	{
		public void TestGetLastPostedEvent()
		{
			// Arrange
			var businessObject = Factory.New<OrgHeader>();
			var locator = StmALogEntryLocator.Instance;
			var eventLog = locator.GetLastPostEventOfType(businessObject, AutoEvents.AddedARecordToTheSystem);
			AssertNull("Precondition - No Added events yet", eventLog);
			eventLog = locator.GetLastPostEventOfType(businessObject, AutoEvents.EditedARecord);
			AssertNull("Precondition - No Edited events yet", eventLog);
			// Act 1
			businessObject.OH_RL_NKClosestPort = "AUSYD";
			businessObject.MainAddress.OA_Address1 = "Address";
			businessObject.OH_Code = "xxx";
			Factory.Save();
			// Assert 1
			eventLog = locator.GetLastPostEventOfType(businessObject, AutoEvents.AddedARecordToTheSystem);
			AssertEquals("Record added", AutoEvents.AddedARecordToTheSystem.Code, eventLog.SL_SE_NKEvent);
			eventLog = locator.GetLastPostEventOfType(businessObject.PK, businessObject.Factory, AutoEvents.Arrival);
			AssertNull("No Arrival event", eventLog);
			// Act 2
			businessObject.OH_RL_NKClosestPort = "AUBNE";
			Factory.Save();
			// Assert 2
			eventLog = locator.GetLastPostEventOfType(businessObject, AutoEvents.AddedARecordToTheSystem);
			AssertEquals("Record added", AutoEvents.AddedARecordToTheSystem.Code, eventLog.SL_SE_NKEvent);
			eventLog = locator.GetLastPostEvent(businessObject.PK.ToString(), new[] { AutoEvents.EditedARecord.Code }, "*", businessObject.Factory);
			AssertEquals("Record edited", AutoEvents.EditedARecord.Code, eventLog.SL_SE_NKEvent);
		}
	}
}
