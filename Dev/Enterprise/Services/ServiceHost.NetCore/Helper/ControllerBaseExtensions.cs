using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.NetCore
{
	public static class ControllerBaseExtensions
	{
		/// <summary>
		/// Returns a 400 response code, with the provided string as a response body.
		/// </summary>
		/// <param name="message">The message to display to the user</param>
		/// <returns></returns>
		/// <remarks>
		///	This is trying to retain the old framework behaviour where if a string is passed in, it returns a 400 with a JSON object with the field Message set to the value of the string provided
		/// </remarks>
		public static IActionResult BadRequest(this ControllerBase controller, string message)
		{
			return controller.BadRequest(new { Message = message });
		}

		/// <summary>
		/// Returns a 400 response code explaing what was wrong with the object provided to the controller
		/// </summary>
		/// <param name="modelstate">The model state provide the issues with the object provided to the controller</param>
		/// <param name="bodyObjectName">the name of object that is binded to the model state</param>
		/// <returns></returns>
		/// <remarks>
		/// THe model state is response is a bit different between NET8 and NET Framework, this restores what the object response should look like.
		/// </remarks>
		public static IActionResult BadRequest(this ControllerBase controller, ModelStateDictionary modelstate, string bodyObjectName)
		{
			return controller.ConvertModelState(modelstate, bodyObjectName);
		}

		static IActionResult ConvertModelState(this ControllerBase controller, ModelStateDictionary modelState, string bodyObjectName)
		{
			Dictionary<string, List<string>> errors = [];

			foreach (var state in modelState)
			{
				var stateKey = string.IsNullOrEmpty(state.Key) ? bodyObjectName : $"{bodyObjectName}.{state.Key}";
				errors.Add(stateKey, state.Value.Errors.Select(x => x.ErrorMessage).ToList());
			}

			var coreShim = new
			{
				Message = (NoResString)"The request is invalid.",
				ModelState = errors
			};

			return controller.BadRequest(coreShim);
		}

		/// <summary>
		/// Returns a 500 response code, with the provided object as the response body
		/// </summary>
		/// <typeparam name="T">The type of the response object being returned</typeparam>
		/// <param name="response">The response to display to the user</param>
		/// <returns></returns>
		/// <remarks>
		///	Mircosoft did not provide a simple way of returning a 500 error. Tries to retains the odd behaviour of the old .NET Framework where if an exception is passed in, it returns a 500 with a message object with "An error has occurred."
		/// </remarks>
		public static IActionResult InternalServerError<T>(this ControllerBase controller, T response)
		{
			return response switch
			{
				Exception exception => controller.StatusCode((int)HttpStatusCode.InternalServerError, new { Message = (NoResString)"An error has occurred." }),
				_ => controller.StatusCode((int)HttpStatusCode.InternalServerError, response)
			};
		}

		/// <summary>
		/// Returns a 403 response
		/// </summary>
		/// <typeparam name="T">The type for the response object</typeparam>
		/// <param name="response">The orbidden response</param>
		/// <returns>Returns an ObjectResult repersenting a 403</returns>
		/// <remarks>
		/// This skips the Auth pipeline to return the 403 https://stackoverflow.com/a/45096777.
		/// </remarks>
		public static IActionResult Forbidden<T>(this ControllerBase controller, T response)
			=> controller.StatusCode((int)HttpStatusCode.Forbidden, response);

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
		public static IActionResult ReturnContentAsText<T>(this ControllerBase controller, T response, HttpStatusCode responseCode, Encoding responseEncoding, string contentType = MediaTypeNames.Text.Html)
		{
			var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);
			mediaTypeHeaderValue.Encoding = responseEncoding ?? mediaTypeHeaderValue.Encoding;

			string responseContent = response switch
			{
				string responseString => responseString,
				IMultilingualString responseString => responseString.ToString(),
				_ => JsonConvert.SerializeObject(response)
			};

			return new ContentResult
			{
				StatusCode = (int)responseCode,
				Content = responseContent,
				ContentType = mediaTypeHeaderValue?.ToString()
			};
		}
	}
}
