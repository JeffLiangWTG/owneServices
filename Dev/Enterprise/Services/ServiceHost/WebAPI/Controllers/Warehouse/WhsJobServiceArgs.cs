using System;

namespace Enterprise.Services.ServiceHost
{
	public class WhsJobServiceArgs
	{
		public Guid ServicePK;
		public string ServiceType;
		public decimal ServiceCount;
		public Guid JobPK;
		public string JobType;
		public DateTimeOffset BookedDateTimeOffset;
		public Guid Contractor;
		public int Duration;
		public Guid LocationPK;
		public string SubLocation;
		public string Reference;
		public string Note;
		public bool FinaliseServiceJob;
	}
}
