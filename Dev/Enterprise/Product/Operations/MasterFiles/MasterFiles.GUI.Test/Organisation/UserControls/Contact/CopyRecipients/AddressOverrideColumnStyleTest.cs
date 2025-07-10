using System;
using Enterprise.Scheduler.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressOverrideColumnStyleTest : MultiEmailColumnStyleTest
	{
		MultiEmailColumnStyle columnStyle;
		protected override MultiEmailColumnStyle ColumnStyle => columnStyle ?? (columnStyle = new AddressOverrideColumnStyle<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>(new AddressOverrideColumnStyleInfo<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>()));

		protected override Type ExpectedControlType { get; } = typeof(AddressOverrideCombinationControl<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>);
	}
}
