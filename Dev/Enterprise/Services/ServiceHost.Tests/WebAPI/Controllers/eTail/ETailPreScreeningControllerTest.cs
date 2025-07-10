using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using CargoWise.Application;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class ETailPreScreeningControllerTest : TestCase
	{
		public void TestGetPreScreenResultList_UnsupportedTableCode()
		{
			using (ObjectFactory.Substitute<IETailPreScreeningService>(new ETailPreScreeningServiceForTest()))
			{
				var result = controller.GetPreScreenResultList("XX", GuidsForTesting.ParentWithPassingConsignments);
				result.AssertResultContains(HttpStatusCode.BadRequest, "Request failed: Unsupported consignment parent type XX");
			}
		}

		public void TestGetPreScreenResultList_NonExistantParent()
		{
			using (ObjectFactory.Substitute<IETailPreScreeningService>(new ETailPreScreeningServiceForTest()))
			{
				var result = controller.GetPreScreenResultList("HVH", GuidsForTesting.NonExistantParent);
				var expectedResponse = string.Format("Request failed: Consignment parent {0} not found with table code HVH", GuidsForTesting.NonExistantParent);
				result.AssertResultContains(HttpStatusCode.BadRequest, expectedResponse);
			}
		}

		public void TestPreScreenResultList_EmptyConsignmentCollection()
		{
			using (ObjectFactory.Substitute<IETailPreScreeningService>(new ETailPreScreeningServiceForTest()))
			{
				var result = controller.GetPreScreenResultList("HVH", GuidsForTesting.ParentWithNoConsignment);
				var expectedResponse = string.Format("Request failed: Consignment parent {0} with table code HVH has no consignment", GuidsForTesting.ParentWithNoConsignment);
				result.AssertResultContains(HttpStatusCode.BadRequest, expectedResponse);
			}
		}

		public void TestGetPreScreenResultList_PreScreenDisabled()
		{
			using (ObjectFactory.Substitute<IETailPreScreeningService>(new ETailPreScreeningServiceForTest()))
			{
				var result = controller.GetPreScreenResultList("HVH", GuidsForTesting.DisablePreScreening);
				result.AssertResultContains(HttpStatusCode.BadRequest, "Request failed: HVLV Pre-Screening is disabled");
			}
		}

		public void TestGetPreScreenResultList_WillAutoPass_WhenNoPreScreeningRule()
		{
			using (ObjectFactory.Substitute<IETailPreScreeningService>(new ETailPreScreeningServiceForTest()))
			{
				var result = controller.GetPreScreenResultList("HVH", GuidsForTesting.NoPreScreeningRule);
				AssertNotNull(result);

				var resultString = result
					.GetResult()
					.GetMessage(HttpStatusCode.OK, isJson: true);
				var resultObj = JsonConvert.DeserializeObject<ETailPreScreeningResponseForTest>(resultString);
				Assert("all consignment pass pre-screening", resultObj.Passed);
			}
		}

		public void TestGetPreScreenResultList_CollectionWithFailingConsignment()
		{
			using (ObjectFactory.Substitute<IETailPreScreeningService>(new ETailPreScreeningServiceForTest()))
			{
				var result = controller.GetPreScreenResultList("HVH", GuidsForTesting.ParentWithFailingConsignments);
				AssertNotNull(result);

				CombineAssertions("notification details", () =>
				{
					var resultString = result
						.GetResult()
						.GetMessage(HttpStatusCode.OK, isJson: true);
					var resultObj = JsonConvert.DeserializeObject<ETailPreScreeningResponseForTest>(resultString);
					Assert("not all consignment pass pre-screening", !resultObj.Passed);
					Assert("consignment1 warning message", resultObj.Results.Any(n => n.FormattedWarningMessage == "consignment1 has some warning message"));
					Assert("consignment1 error message", resultObj.Results.Any(n => n.FormattedErrorMessage == "consignment1 has some error message"));
					Assert("consignment2 error message", resultObj.Results.Any(n => n.FormattedErrorMessage == "consignment2 has some error message"));
				});
			}
		}

		public void TestGetPreScreenResultList_CollectionWithPassingConsignment()
		{
			using (ObjectFactory.Substitute<IETailPreScreeningService>(new ETailPreScreeningServiceForTest()))
			{
				var result = controller.GetPreScreenResultList("HVH", GuidsForTesting.ParentWithPassingConsignments);
				AssertNotNull(result);

				var resultString = result
					.GetResult()
					.GetMessage(HttpStatusCode.OK, isJson: true);
				var resultObj = JsonConvert.DeserializeObject<ETailPreScreeningResponseForTest>(resultString);
				Assert("all consignment pass pre-screening", resultObj.Passed);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			controller = ControllerHelper.GetController<ETailPreScreeningController>();
		}

		ETailPreScreeningController controller;
	}

	public static partial class GuidsForTesting
	{
		public static readonly Guid DisablePreScreening = new Guid("87b5f90a-350d-4194-8267-f4117e77df00");
		public static readonly Guid NoPreScreeningRule = new Guid("eb14f229-ddd0-46ea-a4ec-c929cf7922f1");

		public static readonly Guid NonExistantConsignment = new Guid("d4827754-8050-4db0-ad00-14aeba573f12");
		public static readonly Guid PassingConsignment = new Guid("3c796b7c-a714-447b-9eec-fd56abb81ffb");
		public static readonly Guid FailingConsignment = new Guid("7187b713-2373-448e-af59-8cadbb4f71e2");

		public static readonly Guid NonExistantParent = new Guid("18cc005f-25b0-4195-a0f8-56474e324e4c");
		public static readonly Guid ParentWithNoConsignment = new Guid("0163d702-329f-4948-8489-aee7dd3d019b");
		public static readonly Guid ParentWithPassingConsignments = new Guid("8ef833b2-4118-4e0a-b93c-8f439b594053");
		public static readonly Guid ParentWithFailingConsignments = new Guid("367f59f1-94cc-43b5-b2df-4ca0098e221e");
	}

	class ETailPreScreeningServiceForTest : IETailPreScreeningService
	{
		public IETailPreScreeningResponse PreScreenHVLVConsignment(Guid consignmentPK)
		{
			if (GuidsForTesting.NonExistantConsignment == consignmentPK)
			{
				return new ETailPreScreeningConsignmentNotFoundResponse(consignmentPK);
			}
			else if (GuidsForTesting.DisablePreScreening == consignmentPK)
			{
				return new ETailPreScreeningRegistryDisabledResponse();
			}
			else if (GuidsForTesting.FailingConsignment == consignmentPK)
			{
				var result = new HVLVConsignmentPreScreeningResultForTest(null);
				result.FormattedWarningMessage = "some warning message";
				result.FormattedErrorMessage = "some error message";
				var response = new ETailPreScreeningResponse();
				response.AddPreScreeningResult(result);
				return response;
			}
			else
			{
				var result = new HVLVConsignmentPreScreeningResultForTest(null);
				result.FormattedWarningMessage = string.Empty;
				result.FormattedErrorMessage = string.Empty;
				var response = new ETailPreScreeningResponse();
				response.AddPreScreeningResult(result);
				return response;
			}
		}

		public IETailPreScreeningResponse PreScreenHVLVConsignmentCollection(string consignmentParentTableCode, Guid consignmentParentPK)
		{
			if (consignmentParentTableCode != "JS" && consignmentParentTableCode != "HVH")
			{
				return new ETailPreScreeningUnsupportedConsignmentParentTypeResponse(consignmentParentTableCode);
			}
			else if (GuidsForTesting.DisablePreScreening == consignmentParentPK)
			{
				return new ETailPreScreeningRegistryDisabledResponse();
			}
			else if (GuidsForTesting.NonExistantParent == consignmentParentPK)
			{
				return new ETailPreScreeningConsignmentParentNotFoundResponse(consignmentParentTableCode, consignmentParentPK);
			}
			else if (GuidsForTesting.ParentWithNoConsignment == consignmentParentPK)
			{
				return new ETailPreScreeningEmptyConsignmentCollectionResponse(consignmentParentTableCode, consignmentParentPK);
			}
			else if (GuidsForTesting.ParentWithFailingConsignments == consignmentParentPK)
			{
				var result1 = new HVLVConsignmentPreScreeningResultForTest(null);
				result1.FormattedWarningMessage = "consignment1 has some warning message";
				result1.FormattedErrorMessage = "consignment1 has some error message";
				result1.PreScreeningStatus = "FAL";

				var result2 = new HVLVConsignmentPreScreeningResultForTest(null);
				result2.FormattedWarningMessage = string.Empty;
				result2.FormattedErrorMessage = "consignment2 has some error message";
				result2.PreScreeningStatus = "FAL";

				var response = new ETailPreScreeningResponse();
				response.AddPreScreeningResult(result1);
				response.AddPreScreeningResult(result2);
				return response;
			}
			else
			{
				var result1 = new HVLVConsignmentPreScreeningResultForTest(null);
				result1.FormattedWarningMessage = string.Empty;
				result1.FormattedErrorMessage = string.Empty;

				var result2 = new HVLVConsignmentPreScreeningResultForTest(null);
				result2.FormattedWarningMessage = string.Empty;
				result2.FormattedErrorMessage = string.Empty;

				var response = new ETailPreScreeningResponse();
				response.AddPreScreeningResult(result1);
				response.AddPreScreeningResult(result2);
				return response;
			}
		}

		public IETailPreScreeningResponse PreScreenHVLVConsignmentCollection(IHVLVBookingHeader bookingHeader)
		{
			throw new NotImplementedException();
		}

		public IETailPreScreeningResponse PreScreenHVLVConsignmentCollection(Forwarding.IForwardingShipment forwardingShipment)
		{
			throw new NotImplementedException();
		}
	}

	class HVLVConsignmentPreScreeningResultForTest : IHVLVConsignmentPreScreeningResult
	{
		public HVLVConsignmentPreScreeningResultForTest(IHVLVConsignment consignment)
		{
			Consignment = consignment;
		}

		public string PreScreeningStatus { get; set; }

		public IHVLVConsignment Consignment { get; set; }

		public string FormattedWarningMessage { get; set; }

		public string FormattedErrorMessage { get; set; }

		public ReadOnlyCollection<IPreScreenNotificationDetail> PreScreeningWarningDetails { get; set; }

		public ReadOnlyCollection<IPreScreenNotificationDetail> PreScreeningErrorDetails { get; set; }

		public ReadOnlyCollection<IPreScreenNotificationDetail> PreScreeningNotifyOnlyWarningDetails { get; set; }

		public Guid ConsignmentPK => new Guid();
	}

	class ETailPreScreeningResponseForTest
	{
		public bool Finished { get; set; }

		public bool Passed { get; set; }

		public string ErrorMessage { get; set; }

		public ReadOnlyCollection<HVLVConsignmentPreScreeningResultForTest> Results { get; set; }
	}
}
