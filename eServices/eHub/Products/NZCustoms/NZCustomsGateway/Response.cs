using System.Runtime.Serialization;

namespace CargoWise.eHub.Products.NZCustoms.Gateway
{
	[DataContract]
	public class Response
	{
		public Response()
		{
			IsSuccess = true;
		}

		//TODO: add [DataMember(IsRequired = true)] when clean up SendLodgementOld()
		[DataMember]
		public string MessageTrackingID { get; set; }
		[DataMember]
		public bool IsSuccess { get; set; }
		[DataMember]
		public string ErrorMessage { get; set; }
	}
}
