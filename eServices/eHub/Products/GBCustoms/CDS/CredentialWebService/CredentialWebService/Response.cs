using System.Runtime.Serialization;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService
{
	[DataContract]
	public class Response
	{
		public Response()
			: this(true, "")
		{ }

		public Response(bool isSuccess, string errorMessage)
		{
			IsSuccess = isSuccess;
			ErrorMessage = errorMessage;
		}

		[DataMember]
		public bool IsSuccess { get; set; }

		[DataMember]
		public string ErrorMessage { get; set; }

		public static Response Success
		{
			get
			{
				return new Response();
			}
		}
	
	}
}