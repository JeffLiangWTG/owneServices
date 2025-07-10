using System.Runtime.Serialization;

namespace Enterprise.Services.ServiceHost
{
	[DataContract]
	public class ChangeContainerNumberArgs
	{
		[DataMember(Name = "containerLoadListID")]
		public string ContainerLoadListID { get; set; }

		[DataMember(Name = "containerJobID")]
		public string ContainerJobID { get; set; }

		[DataMember(Name = "containerID")]
		public string ContainerID { get; set; }
	}
}
