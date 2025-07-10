using System.Runtime.Serialization;

namespace Enterprise.Services.ServiceHost
{
	[DataContract]
	public class SplitContainerArgs
	{
		[DataMember(Name = "containerJobID")]
		public string ContainerJobID { get; set; }

		[DataMember(Name = "containerCount")]
		public int ContainerCount { get; set; }

		[DataMember(Name = "ContainerLoadListID")]
		public string ContainerLoadListID { get; set; }
	}
}
