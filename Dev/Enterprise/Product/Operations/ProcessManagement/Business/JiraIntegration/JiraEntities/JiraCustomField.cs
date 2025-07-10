using System.Diagnostics;

namespace Enterprise.ProcessManagement.Business
{
	[DebuggerDisplay("Custom Field: {Id}: {Key} -> {Value}")]
	public class JiraCustomField
	{
		public string Id { get; set; }

		public string Key { get; set; }
		public string Value { get; set; }
	}
}
