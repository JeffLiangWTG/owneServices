namespace eServices.Dms.Core.ServiceDefaults;

public static class DmsStatus
{
	static DmsStatus()
	{
		AllStatuses = typeof(DmsStatus).GetFields().ToDictionary(f => f.Name, f => (string)f.GetRawConstantValue()!, StringComparer.OrdinalIgnoreCase);
	}

	public static IReadOnlyDictionary<string, string> AllStatuses { get; }

	public const string Received = "Received";
	public const string Processing = "Processing";
	public const string Processed = "Processed";
	public const string Delivered = "Delivered";
	public const string Failed = "Failed";
}
