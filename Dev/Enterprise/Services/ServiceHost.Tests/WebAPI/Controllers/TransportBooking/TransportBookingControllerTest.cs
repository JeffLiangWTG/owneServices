using System;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class TransportBookingControllerTest : TestCaseWithFactory
	{
		public void TestTransportBooking_ErrorCreate()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var actionResult = controller.TransportBooking(shipment.PK.ToGuid(), shipment.TablePrefix, "DLV", false, true);
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			Assert(response.IsSuccessStatusCode);
			AssertEquals("{\"messageType\":\"Error\",\"message\":\"Cannot Create Transport Booking. Check DEX logs for details.\"}", content);
		}

		public void TestTransportBooking_ErrorUpdate()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var actionResult = controller.TransportBooking(shipment.PK.ToGuid(), shipment.TablePrefix, "DLV", false, false);
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			Assert(response.IsSuccessStatusCode);
			AssertEquals("{\"messageType\":\"Error\",\"message\":\"Cannot Update Transport Booking. Check DEX logs for details.\"}", content);
		}

		public void TestTransportBooking_ErrorParent()
		{
			var dummy = DummyBusinessObject.New(Factory);
			Factory.Save();

			var actionResult = controller.TransportBooking(dummy.PK.ToGuid(), dummy.TablePrefix, "DLV", false);
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			Assert(response.IsSuccessStatusCode);
			AssertEquals("{\"messageType\":\"Error\",\"message\":\"Z0 does not support Transport Booking.\"}", content);
		}

		[ExpectNoExceptions]
		public void TestTransportBooking_RunSafelyFromAnotherThread()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			ExceptionDispatchInfo exceptionInfo = null;
			var thread = new Thread(() =>
			{
				try
				{
					_ = controller.TransportBooking(shipment.PK.ToGuid(), shipment.TablePrefix, "DLV", false);
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		protected override void SetUp()
		{
			base.SetUp();

			controller = new TransportBookingController();
			controller.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage())
			{
				Controller = controller
			};
		}

		TransportBookingController controller;
	}
}
