using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Grid.Internal;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressOverrideCombinationControlTest : ZMultiCombinationControlTest
	{
		public override ZMultiCombinationControl GetNewMultiCombinationControl()
		{
			return new AddressOverrideCombinationControl<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>(new AddressOverrideColumnStyleInfo<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>());
		}

		public new void TestShowingCodeFindBox()
		{
			RunBindToListTestForCodeFindBox();
		}

		void RunBindToListTestForCodeFindBox()
		{
			using (Db.DisposableActionForDbConnection())
			{
				RunBindToListTest(FieldType.TextCodeFindBox, typeof(CopyRecipientsFindBox<StmScheduleTaskCopyRecipient, StmScheduleTaskRecipient>), false);
			}
		}

		public void RunSelectCopyRecipientsFindBox()
		{
			RunBindToListTest(FieldType.TextCodeFindBox, typeof(CopyRecipientsFindBox<StmScheduleTaskCopyRecipient, StmScheduleTaskRecipient>), false);
		}

		public void RunSelectTextBox()
		{
			RunBindToListTest(FieldType.Text, typeof(TextBox), false);
		}
	}
}
