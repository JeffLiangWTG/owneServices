using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.RTUS.Shared;
using Enterprise.TransportCommon.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using WTG.Foundation.Http;
using WTG.RTUS.Interface;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public abstract class RTUSBookingProvider<T> : IDisposable, ILastMileCarrierBookingService where T : BusinessObject
	{
		protected RTUSBookingProvider(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public ILastMileCarrierBookingResponseCollection BookLastMileCarrier(Guid entityPK, bool saveToeDocs = true)
		{
			var responseCollection = new LastMileCarrierBookingResponseCollection();
			var bizo = Factory.Load<T>(entityPK);

			if (bizo != null)
			{
				var responses = SendRequestToLastMileCarrier(RequestType.Booking, bizo);

				foreach (var response in responses)
				{
					var bookingResponse = new LastMileCarrierBookingResponse();
					bookingResponse.Successful = response.Response?.IsSuccessful ?? false;

					if (bookingResponse.Successful)
					{
						bookingResponse.BinaryData = new ReadOnlyCollection<byte>(response.Response.BinaryData);
						bookingResponse.FileType = response.Response.FileType.ToString();
						OnBooked(response);

						if (saveToeDocs)
						{
							var fileName = response.Reference + "." + bookingResponse.FileType;
							if (bizo is IDocManagerSupportBase docManagerSupport)
							{
								var docManagerInfo = docManagerSupport.DocManagerInfoCore();
								docManagerInfo.AddFileOrDocument(bookingResponse.BinaryData.ToArray(), fileName, Core.Constants.RefDocTypes.Label);
								docManagerInfo.Save();
							}
						}
					}
					else
					{
						bookingResponse.ErrorMessage = response.ErrorMessage;
						OnRejected(response);
					}

					responseCollection.Add(bookingResponse);
				}
			}
			else
			{
				throw new InvalidOperationException(GetBizoNotExistErrorMessage(entityPK));
			}

			return responseCollection;
		}

		public ILastMileCarrierBookingResponseCollection CancelBooking(Guid entityPK)
		{
			var responseCollection = new LastMileCarrierBookingResponseCollection();
			var bizo = Factory.Load<T>(entityPK);

			if (bizo != null)
			{
				var responses = SendRequestToLastMileCarrier(RequestType.Cancellation, bizo);

				foreach (var response in responses)
				{
					var bookingResponse = new LastMileCarrierBookingResponse();
					bookingResponse.Successful = response.Response != null && response.Response.IsSuccessful;

					if (bookingResponse.Successful)
					{
						OnCanceled(bizo);
					}
					else
					{
						bookingResponse.ErrorMessage = response.ErrorMessage;
					}

					responseCollection.Add(bookingResponse);
				}
			}
			else
			{
				throw new InvalidOperationException(GetBizoNotExistErrorMessage(entityPK));
			}

			return responseCollection;
		}

		#region Implementation

		static string GetBizoNotExistErrorMessage(Guid entityPK)
		{
			return Res.GetString("2cdad634-dac9-4b41-8d67-8bbcc39ce81a", "Unable to find {0} by PK [{1}]", typeof(T).Name, entityPK);
		}

		IEnumerable<RTUSServiceResponse<TResponse>> SendRequestToLastMileCarrier<TResponse>(RequestType<TResponse> requestType, T bizo)
			where TResponse : IRTUSResponse
		{
			AddBookingLog(bizo, AutoEvents.BookingRequested, GetBookingRequestedReferenceText());

			var serviceResponses = new List<RTUSServiceResponse<TResponse>>();
			var universalShipments = CreateUniversalShipments(bizo, requestType);

			if (universalShipments == null || universalShipments.Length == 0)
			{
				throw new InvalidOperationException(Res.GetString("a0fe6b5b-7bd1-4451-aed6-073ff6580350", "Failed to create Universal Shipment for {0}", typeof(T).Name));
			}
			else
			{
				var request = new RTUSServiceRequest(GetBookingAgentPK(bizo));

				foreach (var shipment in universalShipments)
				{
					var response = request.SendRequest(requestType, shipment)
						?? throw new InvalidOperationException(Res.GetString("D33A425A-721D-44A3-90FD-2FC7F7A4473F", "RTUS request failed for {0}", shipment.Reference));

					LogRTUSResponse(bizo, requestType, response);
					serviceResponses.Add(response);
				}
			}

			return serviceResponses;
		}

		void LogRTUSResponse<TResponse>(T bizo, RequestType<TResponse> requestType, RTUSServiceResponse<TResponse> serviceResponse) where TResponse : IRTUSResponse
		{
			if (serviceResponse.Response != null && serviceResponse.Response.IsSuccessful)
			{
				if ((RequestType)requestType == RequestType.Booking)
				{
					AddBookingLog(bizo, AutoEvents.BookingConfirmed, GetBookingConfirmedReferenceText(bizo, serviceResponse));
				}
				else if ((RequestType)requestType == RequestType.Cancellation)
				{
					AddBookingLog(bizo, AutoEvents.BookingCancelled, GetBookingCancelledReferenceText(bizo, serviceResponse));
				}
			}
			else
			{
				AddBookingLog(bizo, AutoEvents.BookingRejected, GetBookingRejectedReferenceText(bizo, serviceResponse));
			}
		}

		protected void AddBookingLog(BusinessObject bizo, Event @event, string eventReferenceText)
		{
			if (bizo is IStmALogProvider logProvider)
			{
				logProvider.Logs.AddNew(
					@event,
					eventReferenceText);
				logProvider.LogsFactory.Save();
			}
		}

		#endregion

		protected string GetBookingRequestedReferenceText()
		{
			return FormattableString.Invariant($"|TYP=Last Mile Carrier"); // For logging purpose only
		}

		protected abstract string GetBookingConfirmedReferenceText<TResponse>(T bizo,
			RTUSServiceResponse<TResponse> serviceResponse) where TResponse : IRTUSResponse;

		protected abstract string GetBookingRejectedReferenceText<TResponse>(T bizo,
			RTUSServiceResponse<TResponse> serviceResponse) where TResponse : IRTUSResponse;

		protected abstract string GetBookingCancelledReferenceText<TResponse>(T bizo,
			RTUSServiceResponse<TResponse> serviceResponse) where TResponse : IRTUSResponse;

		protected virtual void OnBooked(RTUSServiceResponse<ISingleBookingRTUSResponse> response)
		{
		}

		protected virtual void OnRejected(RTUSServiceResponse<ISingleBookingRTUSResponse> response)
		{
		}

		protected virtual void OnCanceled(T bizo)
		{
		}

		protected abstract ItemShipment[] CreateUniversalShipments(T bizo, RequestType requestType);

		protected abstract ZGuid GetBookingAgentPK(T bizo);

		protected BusinessObjectFactory Factory { get; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		#region ReferenceBusinessObject

		protected class ItemShipment
		{
			public string Reference { get; }
			public HVLVItem Item { get; }
			public UniversalDataBuss.DataObjects.Universal.Shipment Shipment { get; }

			public ItemShipment(string reference, HVLVItem item, UniversalDataBuss.DataObjects.Universal.Shipment shipment)
			{
				Reference = reference;
				Item = item;
				Shipment = shipment;
			}
		}

		#endregion

		#region RTUSRequest

		class RTUSServiceRequest : IDisposable
		{
			readonly IHttpClientFactory clientFactory = new HttpClientFactory(HttpMessageHandlerFactory.GetHandler);
			readonly ZGuid bookingAgentPK;

			public RTUSServiceRequest(ZGuid bookingAgentPK)
			{
				this.bookingAgentPK = bookingAgentPK;
			}

			public RTUSServiceResponse<TResponse> SendRequest<TResponse>(RequestType<TResponse> requestType, ItemShipment businessObjectShipment) where TResponse : IRTUSResponse
			{
				using (var xml = new CargoWise.IO.Shim.SubStreamableStream())
				{
					string serviceError = null;
					TResponse response = default;

					if (!TryGetRTUSInfo(bookingAgentPK, out var rtusCBA, out var rtusURI))
					{
						serviceError = Res.GetString("757f838c-b3ce-4a87-85f4-e7c47b7c2880", "RTUS is required for making Carrier Booking. Please raise an eRequest (CR9) for RTUS configuration");
					}
					else
					{
						ObjectFactory.New<IXmlWriter>().WriteXML(businessObjectShipment.Shipment, xml);
						response = ObjectFactory.Get<IRTUSProcessor>().PushMessage(requestType, clientFactory, xml, rtusCBA, rtusURI);

						if (response == null)
						{
							serviceError = Res.GetString("eff3be2f-9958-4022-af89-04a3fe5718ba", "Failed to get available RTUS response from {0} for {1}", rtusURI.AbsoluteUri, businessObjectShipment.Reference);
						}
						else
						{
							if (!response.IsSuccessful)
							{
								serviceError = Res.GetString("2DF3D97B-43BA-41E4-8DAB-FCD19E05F0F6", "{0} failed for {1} : {2}", requestType, businessObjectShipment.Reference, response.ErrorMessageForFailure);
							}
						}
					}

					return new RTUSServiceResponse<TResponse>(businessObjectShipment.Reference, businessObjectShipment.Item, serviceError, response);
				}
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			bool TryGetRTUSInfo(ZGuid bookingAgentPK, out RTUSCBA rtusType, out Uri rtusUrl)
			{
				var result = false;
				rtusUrl = default;
				rtusType = default;

				var rtusConfig = ObjectFactory.Get<ITransportRegistry>().GetRTUSOption(bookingAgentPK.ToGuid());

				if (rtusConfig != null)
				{
					rtusUrl = rtusConfig.WrappedUrl;
					rtusType = rtusConfig.RTUSCBA;
					result = true;
				}

				return result;
			}

			void Dispose(bool disposing)
			{
				if (disposing)
				{
					clientFactory?.Dispose();
				}
			}
		}

		#endregion

		#region RTUSResponse

		protected class RTUSServiceResponse<TResponse>
		{
			public HVLVItem Item { get; }
			public string Reference { get; }
			public string ErrorMessage { get; }
			public TResponse Response { get; }

			public RTUSServiceResponse(string reference, HVLVItem item, string error, TResponse response)
			{
				Reference = reference;
				Item = item;
				ErrorMessage = error;
				Response = response;
			}
		}

		#endregion
	}
}
