#if NETFRAMEWORK
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using CargoWiseOne.ResourceStrings;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;
using IActionResult = System.Web.Http.IHttpActionResult;

namespace Microsoft.AspNetCore.Mvc;

[CodeAlive("ServiceHost upgrade to .NET 8. Delete the class once fully upgraded.")]
public abstract class ControllerBase : ApiController
{
	// Implement missing in .NET Framework methods.
	// We should only use return types and methods available in .NET Core and "fake" them in .NET Framework.

	protected StatusCodeResult StatusCode(int statusCode)
	{
		return base.StatusCode((HttpStatusCode)statusCode);
	}

	protected IActionResult BadRequest<T>(T response)
	{
		return response switch
		{
			string responseString => base.BadRequest(responseString), //Don't hide the original method
			ModelStateDictionary modelStateDictionary => base.BadRequest(modelStateDictionary), //Don't hide the original method
			_ => base.Content(HttpStatusCode.BadRequest, response)
		};
	}

	// Matches the extension method for NET so both can be called.
	protected IActionResult BadRequest(ModelStateDictionary modelStateDictionary, string bodyObjectName)
	{
		return base.BadRequest(modelStateDictionary);
	}

	protected IActionResult Forbidden<T>(T response)
		=> base.Content(HttpStatusCode.Forbidden, response);

	protected IActionResult Conflict<T>(T response)
		=> base.Content(HttpStatusCode.Conflict, response);

	protected IActionResult UnprocessableEntity<T>(T response)
		=> base.Content((HttpStatusCode)422, response);

	protected IActionResult InternalServerError<T>(T response)
	{
		return response switch
		{
			Exception exception => base.InternalServerError(exception),
			_ => base.Content(HttpStatusCode.InternalServerError, response)
		};
	}

	/// <summary>
	/// An extension method to help with returning responses that should be as a string, instead of the standard application/json
	/// </summary>
	/// <typeparam name="T">The type for the response object</typeparam>
	/// <param name="response">The response to return as a string object</param>
	/// <param name="responseCode">the http status code of the response</param>
	/// <param name="responseEncoding">the encoding of the response when read as a string</param>
	/// <param name="contentType">What the client should interpret the response as</param>
	/// <returns>A string wrapped as an IActionResult</returns>
	/// <remarks>
	///	This was written originally for the eTail controllers. They also default the contentType to text/html but every single usage they override it to text/plain.
	/// </remarks>
	protected IActionResult ReturnContentAsText<T>(T response, HttpStatusCode responseCode, Encoding responseEncoding, string contentType = MediaTypeNames.Text.Html)
	{
		HttpResponseMessage responseMessage = new(responseCode);
		switch (response)
		{
			case string responseString:
				responseMessage.Content = new StringContent(responseString, responseEncoding, contentType);
				break;
			case IMultilingualString responseString:
				responseMessage.Content = new StringContent(responseString.ToString(), responseEncoding, contentType);
				break;
			default:
				responseMessage.Content = new StringContent(JsonConvert.SerializeObject(response), responseEncoding, contentType);
				break;
		}

		return ResponseMessage(responseMessage);
	}
}
#endif
