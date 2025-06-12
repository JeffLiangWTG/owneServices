using System.Net;

namespace CargoWise.eServices.Authentication.ServiceClient
{
	internal interface IRequest
	{
		WebResponse GetResponse();
	}
}
