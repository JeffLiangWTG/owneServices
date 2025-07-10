using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.StmDataForExcelPassword;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmDataForExcelPassword))]
	public class StmDataForExcelPasswordTest : EnterpriseBusinessObjectTestCase
	{
		public void TestExcelPasswordUniqueIndexFailureHandler()
		{
			var parentPK = ZGuid.NewZGuid();
			var stmData1 = Factory.NewWithValidTestData<StmDataForExcelPassword>();
			stmData1.SD_Name = "ExcelPassword";
			stmData1.SD_Owner = parentPK;

			var handler = new ExcelPasswordUniqueIndexFailureHandler(stmData1);
			UnitTestUserNotification.Instance.ClearMessages();
			handler.NotifyUserAndAttemptToResolve(CargoWise.EntityFramework.NotificationHandler.Instance, "NR_UC__SD_Name_SD_Owner_SD_DepartmentGuid");

			AssertContains("An Excel Open Password for this staff/contact was added by another user and already exists in database. Please reload the form.", UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessages();
		}
	}
}
