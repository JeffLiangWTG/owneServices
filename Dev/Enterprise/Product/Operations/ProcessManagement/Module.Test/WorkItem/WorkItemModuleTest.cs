using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(WorkItemModule))]
	class WorkItemModuleTest : ZModuleBasherTest
	{
		#region ID

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WorkItem;
		}

		#endregion

		#region Licence

		public void TestLicenseCheckpoint()
		{
			using (var module = GetModule())
			{
				AssertEquals(Env.Licence.ProductivityTools, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region ImportDataWizard

		public void TestImportDataWizardMenuItem()
		{
			AssertImportByDataWizardRequiresImportToSystemPrivilege(Env.Security.WorkItem, Env.Security.WorkItemNew);
		}

		public void TestDisplayImportDataWizardResult()
		{
			var flattenedCollection = new WorkItemFlattenedCollection(Factory);
			var flattenedCollectionInfo = new WorkItemFlattenedCollectionInfo(flattenedCollection);
			var processor = new WorkItemFlattenedDataTransferProcessorForTest(flattenedCollectionInfo);

			for (var i = 0; i < 10; i++)
			{
				flattenedCollection.AddNew();
			}

			processor.SetNewCountForTesting(8);
			processor.SetErrorCountForTesting(2);
			processor.AddLogForTesting("Line 2: Invalid work item type 'XXX'");
			processor.AddLogForTesting("Line 5: Invalid work item area 'XXX'");

			WorkItemModuleForTest.DisplayImportDataWizardResult_Exposed(processor, isCancelled: true);
			AssertEquals("LastMessage.Caption", "Import Canceled", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("LastMessage.Text", "No work item was created.", UnitTestUserNotification.Instance.LastMessage.Text);

			var previousShowDialogsInTest = ZFormModaliser.ShowDialogsInTest;
			ZFormModaliser.ShowDialogsInTest = true;
			try
			{
				WorkItemModuleForTest.DisplayImportDataWizardResult_Exposed(processor, isCancelled: false);

				using (var dialog = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(ZMessageBox), dialog);
					var messageBox = (ZMessageBox)dialog;

					AssertEquals("messageBox.Text", "Import Completed", messageBox.Text);
					AssertMultilineASCIIEquals("messageBox.Message",
@"Work items to import = 10
Line 2: Invalid work item type 'XXX'
Line 5: Invalid work item area 'XXX'
TOTAL: Work items created = 8, work items excluded = 2",
					messageBox.Message);
				}
			}
			finally
			{
				ZFormModaliser.ShowDialogsInTest = previousShowDialogsInTest;
			}
		}

		#endregion
	}

	#region Classes For Testing

	class WorkItemModuleForTest : WorkItemModule
	{
		public static void DisplayImportDataWizardResult_Exposed(WorkItemFlattenedDataTransferProcessor processor, bool isCancelled)
		{
			DisplayImportDataWizardResult(processor, isCancelled);
		}
	}

	class WorkItemFlattenedDataTransferProcessorForTest : WorkItemFlattenedDataTransferProcessor
	{
		public WorkItemFlattenedDataTransferProcessorForTest(WorkItemFlattenedCollectionInfo flattenedCollectionInfo)
			: base(flattenedCollectionInfo)
		{
		}

		public void SetNewCountForTesting(int value)
		{
			NewCount = value;
		}

		public void SetErrorCountForTesting(int value)
		{
			ErrorCount = value;
		}

		public void AddLogForTesting(string value)
		{
			LogList.Add(value);
		}
	}

	#endregion
}
