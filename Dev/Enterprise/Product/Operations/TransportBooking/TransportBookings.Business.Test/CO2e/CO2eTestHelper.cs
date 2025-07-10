using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EventReference;
using CargoWise.IO;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Integration.Freight;
using Enterprise.TransportBookings.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	public static class CO2eTestHelper
	{
		public const string CO2Warning = "Greenhouse gas emissions recalculation is required because the input data has changed.";

		public static DtbBooking CreateBasicBooking(BusinessObjectFactory factory, IDtbBookingParent parent = null)
		{
			var helper = new TransportBookingTestHelper(factory);
			var bookingConsolidation = parent == null ? helper.CreateConsolidation() : helper.CreateConsolidation(parent);
			var booking = helper.CreateBooking(bookingConsolidation);
			booking.KM_JobID = "TB00009231";
			var bookingInstructionPic = helper.CreateInstruction(booking, "PIC");
			bookingInstructionPic.KN_Sequence = 1;
			var orgABC = helper.CreateOrganisation("ABC");
			var orgAddressABC = helper.CreateOrgAddress(factory, orgABC, "ABC");
			bookingInstructionPic.Address.E2_OA_Address = orgAddressABC.PK;
			var bookingInstructionDlv = helper.CreateInstruction(booking, "DLV");
			bookingInstructionDlv.KN_Sequence = 2;
			var orgABC2 = helper.CreateOrganisation("ABC2");
			var orgAddressABC2 = helper.CreateOrgAddress(factory, orgABC2, "ABC2");
			bookingInstructionDlv.Address.E2_OA_Address = orgAddressABC2.PK;

			var bookingConfirmationPic = helper.CreateConfirmation(instruction: bookingInstructionPic, confirmationTypeCode: "PIC");
			var bookingConfirmationDlv = helper.CreateConfirmation(instruction: bookingInstructionDlv, confirmationTypeCode: "DLV");

			var packageJob = helper.CreatePackageJob(bookingConsolidation);
			var package = helper.CreatePackage("PKG001", 1, "PKG");
			package.KP_Sequence = 1;
			package.KP_KJ_ParentPackageJob = packageJob.PK;
			package.KP_Weight = 1000m;

			var packageDivotPic = helper.CreatePackageDivot(bookingInstructionPic, package, 1);
			var packageDivotDlv = helper.CreatePackageDivot(bookingInstructionDlv, package, 1);

			return booking;
		}

		public static Shipment CreateSampleCO2eResponse()
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.DataProviderForCodeMapping = "WTG Greenhouse Gas Emission";
			dataObject.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = 10000m,
				CO2eUnit = new UnitOfWeight { Code = "KG" },
				CO2eDistanceInKm = 1500m,
			};
			return dataObject;
		}

		public static IApiResponse<EmissionResult> GenerateEmissionResponse(string path, string mediaType = "application/xml", string assemblyName = "Enterprise.TransportBookings.DataTransfer.Test")
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(Assembly.Load(assemblyName)))
			{
				var content = new StringContent(resourceRetriever.GetString(path));
				content.Headers.ContentType.MediaType = mediaType;
				var serializer = new EmissionSerializer();
				var result = serializer.FromHttpContentAsync<EmissionResult>(content).GetAwaiter().GetResult();
				var responseMock = new Mock<IApiResponse<EmissionResult>>();
				responseMock.SetupGet(x => x.Content).Returns(result);
				return responseMock.Object;
			}
		}

		public static void AssertGHGEvent(Logs logs, string reference, int logCount = 1)
		{
			Assertion.AssertEquals("New GHG event created", logCount, logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			var transportGHGEvent = logs.MostRecentLogByEventTime(AutoEvents.GreenhouseGasEmissionsCalculation);
			Assertion.AssertEquals("GHG event Reference", reference, transportGHGEvent.SL_Reference);
		}

		public static void AssertHasCO2Warning(DtbBooking booking, Action change, string stuReason)
		{
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			change.Invoke();
			Assertion.AssertEquals(CO2eStatusList.Codes.NotCurrent, booking.GetCO2eStatus());
			TestCaseWithFactory.AssertHasWarning(booking.TotalCO2eForBindingInfo, CO2Warning);
			TestCaseWithFactory.AssertHasWarning(booking.TotalCO2eForSortingInfo, CO2Warning);
			AssertSTUEvent(booking, stuReason);
		}

		public static void AssertNoCO2Warning(DtbBooking booking, Action change)
		{
			var stuCountBefore = booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).Count();
			booking.SetCO2eStatus(CO2eStatusList.Codes.Current);
			change.Invoke();
			Assertion.AssertEquals(CO2eStatusList.Codes.Current, booking.GetCO2eStatus());
			TestCaseWithFactory.AssertNoWarning(booking.TotalCO2eForBindingInfo, CO2Warning);
			TestCaseWithFactory.AssertNoWarning(booking.TotalCO2eForSortingInfo, CO2Warning);
			var stuCountAfter = booking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).Count();
			Assertion.AssertEquals("No new STU event", stuCountBefore, stuCountAfter);
		}

		static void AssertSTUEvent(DtbBooking dtbBooking, string reason)
		{
			var stu = dtbBooking.Logs.MostRecentLogByEventTime(AutoEvents.StatusUpdated);
			Assertion.AssertEquals("TYP parameter", "CO2e Status", stu.Parameters[Constants.EventReferenceParameters.Codes.Type]);
			Assertion.AssertEquals("RES parameter", $"Input value(s) have changed: {reason}", stu.Parameters[Constants.EventReferenceParameters.Codes.Reason]);
			Assertion.AssertEquals("NEW parameter", CO2eStatusList.Codes.NotCurrent, stu.Parameters[Constants.EventReferenceParameters.Codes.New]);
			Assertion.AssertEquals("OLD parameter", CO2eStatusList.Codes.Current, stu.Parameters[Constants.EventReferenceParameters.Codes.Old]);
		}

		public static IDisposable MockCO2eFeatureControl(bool enabled = true)
		{
			var mock = new Mock<ICO2eFeatureControlHelper>();
			mock.Setup(m => m.Enabled).Returns(enabled);
			return ObjectFactory.Substitute(mock.Object);
		}
	}
}
