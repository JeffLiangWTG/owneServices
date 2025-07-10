using System;
using System.Linq;
using System.Threading;
using System.Web.Http.Results;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class TrackingMapControllerTest : TestCaseWithFactory
	{
		public void TestTrackingMapUrl_NoEntityType()
		{
			var result = controller.GetUrl(null, Guid.NewGuid(), CancellationToken.None);

			AssertType<BadRequestResult>(result);
		}

		public void TestTrackingMapUrl_NoPk()
		{
			var result = controller.GetUrl("IJobShipment", Guid.Empty, CancellationToken.None);

			AssertType<BadRequestResult>(result);
		}

		public void TestTrackingMapUrl_EntityTypeNotSupported()
		{
			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("NotAnEntityType", Guid.NewGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("NotAnEntityType is not supported.", result.Content.Errors.SingleOrDefault());
			});
		}

		public void TestTrackingMapUrl_IJobShipment_NotInDB()
		{
			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobShipment", Guid.NewGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Shipment not found in database. If this is a new shipment, save your changes first.", result.Content.Errors.SingleOrDefault());
			});
		}

		public void TestTrackingMapUrl_IJobShipment_UrlProviderError()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment.Consols.Add(consol);
			shipment.Transports.AddNew();
			consol.Transports.AddNew();
			Factory.Save();

			var expectedTransportPks = shipment.TransportsIncludingRelated.Select(x => x.PK);

			TransportOrderHelper helper = null;
			mockService
				.Setup(x => x.GetActiveTransportMapUrl(It.IsAny<TransportOrderHelper>(), It.Is<OrgContact>(c => c.PK == contact.PK), CancellationToken.None))
				.Callback<TransportOrderHelper, OrgContact, CancellationToken>((h, c, ct) => helper = h)
				.Returns((null, "Error message from url provider"));

			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobShipment", shipment.PK.ToGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Error message from url provider", result.Content.Errors.SingleOrDefault());
				AssertContainsExactElementsInAnyOrder(expectedTransportPks, helper.Select(x => x.PK));
			});
		}

		public void TestTrackingMapUrl_IJobShipment_TransportsIncludingRelated()
		{
			AssertTrackingMapUrl_IJobShipment_TransportsIncludingRelated(true);
		}

		public void TestTrackingMapUrl_IJobShipment_TransportsIncludingRelated_Staff()
		{
			AssertTrackingMapUrl_IJobShipment_TransportsIncludingRelated(false);
		}

		void AssertTrackingMapUrl_IJobShipment_TransportsIncludingRelated(bool isContact)
		{
			OrgContact contact = null;
			if (isContact)
			{
				contact = Factory.NewWithValidTestData<OrgContact>();
				GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);
			}
			else
			{
				GlowTicketTestHelper.SetUpStaffPrincipal(controller, Factory.NewWithValidTestData<GlbStaff>());
			}

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			shipment.Consols.Add(consol);
			shipment.Transports.AddNew();
			consol.Transports.AddNew();
			Factory.Save();

			var expectedTransportPks = shipment.TransportsIncludingRelated.Select(x => x.PK);

			TransportOrderHelper helper = null;
			mockService
				.Setup(x => x.GetActiveTransportMapUrl(It.IsAny<TransportOrderHelper>(), It.Is<OrgContact>(c => isContact ? c.PK == contact.PK : c == null), CancellationToken.None))
				.Callback<TransportOrderHelper, OrgContact, CancellationToken>((h, c, ct) => helper = h)
				.Returns((new Uri("https://wisegrid.net/mapurl"), string.Empty));

			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobShipment", shipment.PK.ToGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals("https://wisegrid.net/mapurl", result.Content.Url);
				AssertNull(result.Content.Errors);
				AssertContainsExactElementsInAnyOrder(expectedTransportPks, helper.Select(x => x.PK));
			});
		}

		public void TestTrackingMapUrl_IJobContainer_NotInDB()
		{
			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobContainer", Guid.NewGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Container not found in database. If this is a new container, save your changes first.", result.Content.Errors.SingleOrDefault());
			});
		}

		public void TestTrackingMapUrl_IJobContainer_NoConsol()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();
			Factory.Save();

			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobContainer", container.PK.ToGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Consol not found for container.", result.Content.Errors.SingleOrDefault());
			});
		}

		public void TestTrackingMapUrl_IJobContainer_UrlServiceError()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			consol.Transports.AddNew();
			Factory.Save();

			var expectedTransportPks = container.Consol.Transports.Select(x => x.PK);

			TransportOrderHelper helper = null;
			mockService
				.Setup(x => x.GetActiveTransportMapUrl(It.IsAny<TransportOrderHelper>(), It.Is<OrgContact>(c => c.PK == contact.PK), CancellationToken.None))
				.Callback<TransportOrderHelper, OrgContact, CancellationToken>((h, c, ct) => helper = h)
				.Returns((null, "Error message from url service"));

			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobContainer", container.PK.ToGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Error message from url service", result.Content.Errors.SingleOrDefault());
				AssertContainsExactElementsInAnyOrder(expectedTransportPks, helper.Select(x => x.PK));
			});
		}

		public void TestTrackingMapUrl_IJobContainer_TransportsIncludingRelated()
		{
			AssertTrackingMapUrl_IJobContainer_TransportsIncludingRelated(true);
		}

		public void TestTrackingMapUrl_IJobContainer_TransportsIncludingRelated_Staff()
		{
			AssertTrackingMapUrl_IJobContainer_TransportsIncludingRelated(false);
		}

		void AssertTrackingMapUrl_IJobContainer_TransportsIncludingRelated(bool isContact)
		{
			OrgContact contact = null;
			if (isContact)
			{
				contact = Factory.NewWithValidTestData<OrgContact>();
				GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);
			}
			else
			{
				GlowTicketTestHelper.SetUpStaffPrincipal(controller, Factory.NewWithValidTestData<GlbStaff>());
			}

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			consol.Transports.AddNew();
			Factory.Save();

			var expectedTransportPks = container.Consol.Transports.Select(x => x.PK);

			TransportOrderHelper helper = null;
			mockService
				.Setup(x => x.GetActiveTransportMapUrl(It.IsAny<TransportOrderHelper>(), It.Is<OrgContact>(c => isContact ? c.PK == contact.PK : c == null), CancellationToken.None))
				.Callback<TransportOrderHelper, OrgContact, CancellationToken>((h, c, ct) => helper = h)
				.Returns((new Uri("https://wisegrid.net/mapurl"), string.Empty));

			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobContainer", container.PK.ToGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals("https://wisegrid.net/mapurl", result.Content.Url);
				AssertNull(result.Content.Errors);
				AssertContainsExactElementsInAnyOrder(expectedTransportPks, helper.Select(x => x.PK));
			});
		}

		public void TestTrackingMapUrl_IJobDeclaration_NotInDB()
		{
			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobDeclaration", Guid.NewGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Declaration not found in database. If this is a new declaration, save your changes first.", result.Content.Errors.SingleOrDefault());
			});
		}

		public void TestTrackingMapUrl_IJobDeclaration_UrlServiceError()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.Transports.AddNew();
			Factory.Save();

			var expectedTransportPks = declaration.TransportsIncludingRelated.Select(x => x.PK);

			TransportOrderHelper helper = null;
			mockService
				.Setup(x => x.GetActiveTransportMapUrl(It.IsAny<TransportOrderHelper>(), It.Is<OrgContact>(c => c.PK == contact.PK), CancellationToken.None))
				.Callback<TransportOrderHelper, OrgContact, CancellationToken>((h, c, ct) => helper = h)
				.Returns((null, "Error message from url service"));

			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobDeclaration", declaration.PK.ToGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Error message from url service", result.Content.Errors.SingleOrDefault());
				AssertContainsExactElementsInAnyOrder(expectedTransportPks, helper.Select(x => x.PK));
			});
		}

		public void TestTrackingMapUrl_IJobDeclaration_TransportsIncludingRelated()
		{
			AssertTrackingMapUrl_IJobDeclaration_TransportsIncludingRelated(true);
		}

		public void TestTrackingMapUrl_IJobDeclaration_TransportsIncludingRelated_Staff()
		{
			AssertTrackingMapUrl_IJobDeclaration_TransportsIncludingRelated(false);
		}

		void AssertTrackingMapUrl_IJobDeclaration_TransportsIncludingRelated(bool isContact)
		{
			OrgContact contact = null;
			if (isContact)
			{
				contact = Factory.NewWithValidTestData<OrgContact>();
				GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);
			}
			else
			{
				GlowTicketTestHelper.SetUpStaffPrincipal(controller, Factory.NewWithValidTestData<GlbStaff>());
			}

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.Transports.AddNew();
			Factory.Save();

			var expectedTransportPks = declaration.TransportsIncludingRelated.Select(x => x.PK);

			TransportOrderHelper helper = null;
			mockService
				.Setup(x => x.GetActiveTransportMapUrl(It.IsAny<TransportOrderHelper>(), It.Is<OrgContact>(c => isContact ? c.PK == contact.PK : c == null), CancellationToken.None))
				.Callback<TransportOrderHelper, OrgContact, CancellationToken>((h, c, ct) => helper = h)
				.Returns((new Uri("https://wisegrid.net/mapurl"), string.Empty));

			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobDeclaration", declaration.PK.ToGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals("https://wisegrid.net/mapurl", result.Content.Url);
				AssertNull(result.Content.Errors);
				AssertContainsExactElementsInAnyOrder(expectedTransportPks, helper.Select(x => x.PK));
			});
		}

		public void TestTrackingMapUrl_IJobOrderHeader_NotInDB()
		{
			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobOrderHeader", Guid.NewGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Order not found in database. If this is a new order, save your changes first.", result.Content.Errors.SingleOrDefault());
			});
		}

		public void TestTrackingMapUrl_IJobOrderHeader_UrlServiceError()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			mockService
				.Setup(x => x.GetOrderMapUrl(It.Is<Order>(o => o.PK == order.PK), It.Is<OrgContact>(c => c.PK == contact.PK), CancellationToken.None))
				.Returns((null, "Error message from url service"));

			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobOrderHeader", order.PK.ToGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertNull(result.Content.Url);
				AssertEquals("Error message from url service", result.Content.Errors.SingleOrDefault());
			});
		}

		public void TestTrackingMapUrl_IJobOrderHeader_UrlServiceSuccess()
		{
			AssertTrackingMapUrl_IJobOrderHeader_UrlServiceSuccess(true);
		}

		public void TestTrackingMapUrl_IJobOrderHeader_UrlServiceSuccess_Staff()
		{
			AssertTrackingMapUrl_IJobOrderHeader_UrlServiceSuccess(false);
		}

		void AssertTrackingMapUrl_IJobOrderHeader_UrlServiceSuccess(bool isContact)
		{
			OrgContact contact = null;
			if (isContact)
			{
				contact = Factory.NewWithValidTestData<OrgContact>();
				GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);
			}
			else
			{
				GlowTicketTestHelper.SetUpStaffPrincipal(controller, Factory.NewWithValidTestData<GlbStaff>());
			}

			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			mockService
				.Setup(x => x.GetOrderMapUrl(It.Is<Order>(o => o.PK == order.PK), It.Is<OrgContact>(c => isContact ? c.PK == contact.PK : c == null), CancellationToken.None))
				.Returns((new Uri("https://wisegrid.net/mapurl"), null));

			var result = (JsonResult<TrackingMapUrlResult>)controller.GetUrl("IJobOrderHeader", order.PK.ToGuid(), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals("https://wisegrid.net/mapurl", result.Content.Url);
				AssertNull(result.Content.Errors);
			});
		}

		protected override void SetUp()
		{
			controller = new TrackingMapController();
			mockService = new Mock<ITrackingMapUrlService>();
			ObjectFactory.Substitute(nameof(ITrackingMapUrlService), mockService.Object);
		}

		TrackingMapController controller;
		Mock<ITrackingMapUrlService> mockService;
	}
}
