namespace OcmPoc.Utils.Config
{
	public class QueueConfig
	{
		public string Input { get; set; }
		public string InputDeadLetter { get; set; }
		public string Output { get; set; }
		public string OutputDeadLetter { get; set; }

		public string InternalPrefix { get; set; }
		public string InternalInput => $"{InternalPrefix}.ip";
		public string InternalDeadLetter => $"{InternalPrefix}.dl";
		public string InternalOutput => $"{InternalPrefix}.op";
	}
}
