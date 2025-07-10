using System;
using Enterprise.Scheduler.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressOverrideColumnStyleInfoTest : MultiEmailColumnStyleInfoTest
	{
		protected override MultiEmailColumnStyleInfo ColumnStyleInfo { get; } = new AddressOverrideColumnStyleInfo<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>();

		protected override Type ExpectedColumnStyleType { get; } = typeof(AddressOverrideColumnStyle<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>);
	}
}
