using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
#if NETFRAMEWORK
using System.Web.Http;
using IActionResult = System.Web.Http.IHttpActionResult;
using Route = System.Web.Http.RouteAttribute;
#endif
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
#if NET
using Enterprise.Services.ServiceHost.NetCore;
#endif
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
#if NET
	[Route("api/eTail")]
#elif NETFRAMEWORK
	[RoutePrefix("api/eTail")]
#endif
	public class RTUSBookingController : ControllerBase
	{
		const string DefaultMediaType = "text/html";

		public RTUSBookingController()
		{
		}

		//api/eTail/carrier-bookings/{entityPK:Guid}?entityTableCode={entityTableCode}&printerPK={printerPK}
		[Route("carrier-bookings/{entityPK:Guid}")]
		[HttpPost]
		public IActionResult CreateCarrierBooking(Guid entityPK, string entityTableCode, Guid? printerPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return BookLastMileCarrier(entityPK, entityTableCode, printerPK);
			}
		}

		//api/eTail/carrier-bookings/{entityPK:Guid}?entityTableCode={entityTableCode}
		[Route("carrier-bookings/{entityPK:Guid}")]
		[HttpDelete]
		public IActionResult CancelCarrierBooking(Guid entityPK, string entityTableCode)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return CancelBooking(entityPK, entityTableCode);
			}
		}

		[Route("bookLastMileCarrier/{entityTableCode}/{entityPK}/{printerPK}")]
		[HttpPost]
		public IActionResult BookLastMileCarrier(Guid entityPK, string entityTableCode, Guid? printerPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				ILastMileCarrierBookingResponseCollection responseCollection = null;
				var printErrorMessage = string.Empty;

				try
				{
					var factory = new BusinessObjectFactory() { NameForDebugging = "LMC Booking Provider Factory" };
					var bookingProvider = GetBookingProvider(entityTableCode, factory);
					responseCollection = bookingProvider.BookLastMileCarrier(entityPK);
					printErrorMessage = printerPK != null ? PrintLabels(printerPK, responseCollection) : string.Empty;
				}
				catch (InvalidOperationException exception)
				{
					return this.ReturnContentAsText(Res.GetString("6BFD63DA-9A41-4102-B8CB-31B1ABD8EA6E", "Unexpected error happened: {0}", exception.Message), HttpStatusCode.InternalServerError, Encoding.UTF8, DefaultMediaType);
				}

				if (responseCollection == null || !responseCollection.Any())
				{
					return this.ReturnContentAsText(Res.GetString("25E47916-7DFF-43AE-BB32-259F79F39D28", "Booking failed: booking service returned an empty response"), HttpStatusCode.InternalServerError, Encoding.UTF8, DefaultMediaType);
				}
				else if (responseCollection.HasError)
				{
					var formatedPrintErrorMessage = !string.IsNullOrEmpty(printErrorMessage) ? System.Environment.NewLine + printErrorMessage : string.Empty;
					return this.ReturnContentAsText(responseCollection.ErrorMessage + formatedPrintErrorMessage, HttpStatusCode.InternalServerError, Encoding.UTF8, DefaultMediaType);
				}
				else
				{
					if (!string.IsNullOrEmpty(printErrorMessage))
					{
						return this.ReturnContentAsText(printErrorMessage, HttpStatusCode.OK, Encoding.UTF8, DefaultMediaType);
					}
				}
			}

			return Ok();
		}

		[Route("cancelBooking/{entityTableCode}/{entityPK}")]
		[HttpPost]
		public IActionResult CancelBooking(Guid entityPK, string entityTableCode)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				ILastMileCarrierBookingResponseCollection responseCollection = null;

				try
				{
					var factory = new BusinessObjectFactory() { NameForDebugging = "LMC Booking Provider Factory" };
					var bookingProvider = GetBookingProvider(entityTableCode, factory);
					responseCollection = bookingProvider.CancelBooking(entityPK);
				}
				catch (InvalidOperationException exception)
				{
					return this.ReturnContentAsText(Res.GetString("E421C81D-03DD-4EFA-9485-02AC8F583C20", "Unexpected error happened: {0}", exception.Message), HttpStatusCode.InternalServerError, Encoding.UTF8, DefaultMediaType);
				}

				if (responseCollection == null || !responseCollection.Any())
				{
					return this.ReturnContentAsText(Res.GetString("0AC0CCF9-55B3-4A52-99D3-6C5EE17DB607", "Canceling failed: cancel booking service returned an empty response"), HttpStatusCode.InternalServerError, Encoding.UTF8, DefaultMediaType);
				}
				else if (responseCollection.HasError)
				{
					return this.ReturnContentAsText(responseCollection.ErrorMessage, HttpStatusCode.InternalServerError, Encoding.UTF8, DefaultMediaType);
				}
			}

			return Ok();
		}

		//api/eTail/booking-labels/{entityPK:Guid}?entityTableCode={entityTableCode}
		[Route("booking-labels/{entityPK:Guid}")]
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1801", Justification = "To be implemented")]
		public IActionResult GetBookingLabels(Guid entityPK)
		{
			throw new NotImplementedException();
		}

		//api/eTail/booking-labels/{entityPK:Guid}?entityTableCode={entityTableCode}&printerPK={printerPK}
		[Route("booking-labels/{entityPK:Guid}/print")]
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1801", Justification = "To be implemented")]
		public IActionResult PrintBookingLabels(Guid entityPK, string entityTableCode, Guid printerPK)
		{
			throw new NotImplementedException();
		}

		//api/eTail/printers/{printerPK:Guid}/print
		[Route("printers/{printerPK:Guid}/print")]
		[HttpPost]
		public IActionResult PrintData(Guid printerPK, [FromBody] byte[] data, string fileType)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (!TryPrintLabel(printerPK, data, fileType, out var printError))
				{
					return this.ReturnContentAsText(printError, HttpStatusCode.InternalServerError, Encoding.UTF8, DefaultMediaType);
				}

				return Ok();
			}
		}

		#region Implementation

		string PrintLabels(Guid? printerPK, ILastMileCarrierBookingResponseCollection responseCollection)
		{
			var errors = string.Empty;

			if (printerPK.HasValue && responseCollection != null)
			{
				var printErrors = new List<string>();

				foreach (var response in responseCollection)
				{
					if (response.Successful)
					{
						if (response.BinaryData == null)
						{
							printErrors.Add(Res.GetString("0699ccc1-40bd-4976-a684-10bc0b3153ab", "Booking was successful but failed to get a valid label file for {0}.", response.TrackingNumber));
						}
						else
						{
							if (!TryPrintLabel(printerPK.Value, response.BinaryData.ToArray(), response.FileType, out var printError))
							{
								printErrors.Add(printError);
							}
						}
					}
				}

				errors = string.Join(System.Environment.NewLine, printErrors);
			}

			return errors;
		}

		bool TryPrintLabel(Guid printerPK, byte[] data, string fileType, out string error)
		{
			error = string.Empty;
			var result = true;

			using (Db.DisposableActionForDbConnection())
			{
				var printFactory = new BusinessObjectFactory() { NameForDebugging = "LMC Booking Printer Factory" };
				var printer = ObjectFactory.New<ILabelPrintingService>(printFactory);

				if (!printer.TryPrintLabel(data, fileType, printerPK, out var printingErrorMessage))
				{
					error = Res.GetString("36cf05a6-c347-4b3e-bd3c-a68bf6a0cfff", "Booking was successful but failed to print label: {0}", printingErrorMessage);
					result = false;
				}
			}

			return result;
		}

		ILastMileCarrierBookingService GetBookingProvider(string entityTableCode, BusinessObjectFactory factory)
		{
			ILastMileCarrierBookingService bookingProvider = default;
			var registeredProviders = ObjectFactory.Get<Hashtable>("RTUSBookingProviders");
			if (registeredProviders.ContainsKey(entityTableCode))
			{
				var handle = (ObjectHandle)registeredProviders[entityTableCode];
				if (handle != null)
				{
					bookingProvider = handle.GetObject(factory) as ILastMileCarrierBookingService;
				}
			}

			return bookingProvider ?? throw new InvalidOperationException(FormattableString.Invariant($"Cannot get RTUS booking provider for table [{entityTableCode}]."));
		}

		#endregion
	}
}
