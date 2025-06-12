namespace eServices.Dms.Core.ServiceDefaults;

public static class DmsHeaders
{
	static DmsHeaders()
	{
		AllHeaders = typeof(DmsHeaders).GetFields().ToDictionary(f => f.Name, f => (string)f.GetRawConstantValue()!, StringComparer.OrdinalIgnoreCase);
	}

	public static IReadOnlyDictionary<string, string> AllHeaders { get; }

	public const string MessageId = "x-dms-message-id";
	public const string MessageUri = "x-dms-message-uri";
	public const string MessageSenderId = "x-dms-message-sender-id";
	public const string MessageRecipientId = "x-dms-message-recipient-id";
	public const string MessageType = "x-dms-message-type";
	public const string MessageAppCode = "x-dms-message-app-code";
	public const string MessageDescription = "x-dms-message-description";
	public const string MessageFileName = "x-dms-message-file-name";
	public const string MessageParentId = "x-dms-message-parent-id";
	public const string MessageBatchId = "x-dms-message-batch-id";
	public const string MessageTrackingId = "x-dms-message-tracking-id";
	public const string MessageStatus = "x-dms-message-status";
	public const string MessageError = "x-dms-message-error";
	public const string MessageIssueManagerReportId = "x-dms-message-issue-manager-report-id";
	public const string MessageTimeReceived = "x-dms-message-time-received";
	public const string MessageTimeLastUpdate = "x-dms-message-time-last-update";
	public const string MessageAckReqd = "x-dms-message-ack-reqd";
}
