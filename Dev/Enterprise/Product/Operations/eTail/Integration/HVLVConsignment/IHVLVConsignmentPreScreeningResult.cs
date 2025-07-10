using System;
using System.Collections.ObjectModel;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVConsignmentPreScreeningResult
	{
		string PreScreeningStatus { get; }
		Guid ConsignmentPK { get; }
		string FormattedWarningMessage { get; }
		string FormattedErrorMessage { get; }

		ReadOnlyCollection<IPreScreenNotificationDetail> PreScreeningWarningDetails { get; }
		ReadOnlyCollection<IPreScreenNotificationDetail> PreScreeningErrorDetails { get; }
		ReadOnlyCollection<IPreScreenNotificationDetail> PreScreeningNotifyOnlyWarningDetails { get; }
	}
}
