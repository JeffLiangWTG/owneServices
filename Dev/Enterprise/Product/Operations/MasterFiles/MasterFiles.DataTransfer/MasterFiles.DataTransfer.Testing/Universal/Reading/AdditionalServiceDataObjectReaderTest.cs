using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class AdditionalServiceDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestNullOrgAddressDoesNotThrowException()
		{
			var serviceDataObject = SetupAdditionalService();
			serviceDataObject.Contractor = null;
			serviceDataObject.Location = null;
			var reader = new AdditionalServiceDataObjectReader(serviceDataObject, Logger, Factory, null);
			AssertNoExceptionThrown("AdditionalService reads in fine.", () => reader.ReadIntoBusinessObject());
		}

		public void TestBasicServiceLevelFieldMappings()
		{
			var locationAddress = new OrganisationDataObjectReader(GetLocalClientAddress(), Logger, Factory).GetMatchedOrNewForTesting();
			var contractorAddress = new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.Contractor)), Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			Logger.ClearLogs();
			var serviceDataObject = SetupAdditionalService();
			var reader = new AdditionalServiceDataObjectReader(serviceDataObject, Logger, Factory, null);
			var serviceBO = reader.ReadIntoBusinessObject();

			AssertNotNull("serviceBO", serviceBO);

			CombineAssertions(delegate
			{
				AssertContents(serviceBO);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'Contractor':- Matched to 'INTHEMSYD' by code, address '' (only address).
".Trim(), Logger.Logs);
			});
		}

		public void TestServicesAreLoaded()
		{
			var locationAddress = new OrganisationDataObjectReader(GetLocalClientAddress(), Logger, Factory).GetMatchedOrNewForTesting();
			var contractorAddress = new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.Contractor)), Logger, Factory).GetMatchedOrNewForTesting();
			var dummyBO = Factory.NewWithValidTestData<DummyWithServices>();
			var service = dummyBO.Services.AddNew();
			service.ShouldPopulateServiceId = false;
			service.ES_ServiceCode = "MAN";

			Factory.SaveForTesting();

			Logger.ClearLogs();
			var serviceDataObject = SetupAdditionalService();

			var reader = new AdditionalServiceDataObjectReader(serviceDataObject, Logger, Factory, dummyBO);
			var serviceBO = reader.ReadIntoBusinessObject();

			AssertNotNull("serviceBO", serviceBO);

			CombineAssertions(delegate
			{
				AssertContents(serviceBO);
				AssertEquals("serviceBO.PK", service.PK, serviceBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching JobService.
Information - Populating JobService...
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'Contractor':- Matched to 'INTHEMSYD' by code, address '' (only address).
".Trim(), Logger.Logs);
			});
		}

		public void TestReadIntoBusinessObject_ShouldCreateNewBusinessObject_WhenServiceTypeIsNonUnique()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var locationAddress = new OrganisationDataObjectReader(GetLocalClientAddress(), Logger, Factory).GetMatchedOrNewForTesting();
				var contractorAddress = new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.Contractor)), Logger, Factory).GetMatchedOrNewForTesting();

				var dummyBO = Factory.NewWithValidTestData<DummyWithServices>();
				var service = dummyBO.Services.AddNew();
				service.ShouldPopulateServiceId = false;
				service.ES_ServiceCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection;

				Factory.SaveForTesting();

				Logger.ClearLogs();
				var serviceDataObject = SetupAdditionalService();
				serviceDataObject.ServiceCode = new CodeDescriptionPair { Code = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, Description = "Commodity Inspection" };

				var reader = new AdditionalServiceDataObjectReader(serviceDataObject, Logger, Factory, dummyBO);
				var serviceBO = reader.ReadIntoBusinessObject();

				AssertNotNull("serviceBO", serviceBO);

				CombineAssertions(delegate
				{
					AssertNotEquals("It should create a new business object for non-unique service", service.PK, serviceBO.PK);
					AssertContents(serviceBO, new ZDateTime(2011, 1, 1), new ZDateTime(2011, 1, 2), TimeSpan.FromDays(2), "Refer to the reference", "ICI", 32.1m, "Note to self", null, null, ZString.Empty, ZString.Empty, shouldPopulateServiceId: false, "Sub Location");
					AssertAddressContentMatches_INTHEMSYD(serviceBO.Contractor.MainAddress);
					AssertLocalClientAddress(serviceBO.Location);
					AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'Contractor':- Matched to 'INTHEMSYD' by code, address '' (only address).
".Trim(), Logger.Logs);
				});
			}
		}

		public void TestReadIntoBusinessObject_ShouldCreateNewBusinessObject_WhenServiceIdIsNotEmpty_NoMatchingServiceId()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var locationAddress = new OrganisationDataObjectReader(GetLocalClientAddress(), Logger, Factory).GetMatchedOrNewForTesting();
				var contractorAddress = new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.Contractor)), Logger, Factory).GetMatchedOrNewForTesting();

				var dummyBO = Factory.NewWithValidTestData<DummyWithServices>();
				var service = dummyBO.Services.AddNew();
				service.ES_ServiceId = "WTLKKK00000043";
				service.ES_ServiceCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection;
				service.ES_ExternalServiceId = ZString.Empty;

				Factory.SaveForTesting();

				Logger.ClearLogs();
				var serviceDataObject = SetupAdditionalService();
				serviceDataObject.ServiceId = "WTLKKK00000043";
				serviceDataObject.ServiceCode = new CodeDescriptionPair { Code = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, Description = "Commodity Inspection" };
				serviceDataObject.ExternalServiceId = ZString.Empty;

				var reader = new AdditionalServiceDataObjectReader(serviceDataObject, Logger, Factory, Factory.NewWithValidTestData<DummyWithServices>());
				var serviceBO = reader.ReadIntoBusinessObject();

				AssertNotNull("serviceBO", serviceBO);

				CombineAssertions(delegate
				{
					AssertEquals("service.ES_ExternalServiceId", serviceBO.ES_ServiceId, service.ES_ExternalServiceId);
					AssertContents(serviceBO, new ZDateTime(2011, 1, 1), new ZDateTime(2011, 1, 2), TimeSpan.FromDays(2), "Refer to the reference", "ICI", 32.1m, "Note to self", null, null, serviceBO.ES_ServiceId, "WTLKKK00000043", shouldPopulateServiceId: true, "Sub Location");
					AssertAddressContentMatches_INTHEMSYD(serviceBO.Contractor.MainAddress);
					AssertLocalClientAddress(serviceBO.Location);
					AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'Contractor':- Matched to 'INTHEMSYD' by code, address '' (only address).
".Trim(), Logger.Logs);
				});
			}
		}

		public void TestReadIntoBusinessObject_ShouldCreateNewBusinessObject_WhenServiceIdIsNotEmpty_MatchingServiceId()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var locationAddress = new OrganisationDataObjectReader(GetLocalClientAddress(), Logger, Factory).GetMatchedOrNewForTesting();
				var contractorAddress = new OrganisationDataObjectReader(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.Contractor)), Logger, Factory).GetMatchedOrNewForTesting();

				var dummyBO = Factory.NewWithValidTestData<DummyWithServices>();
				var service = dummyBO.Services.AddNew();
				service.ES_ServiceId = "WTLKKK00000043";
				service.ES_ServiceCode = "MAN";
				service.ES_ExternalServiceId = ZString.Empty;

				Factory.SaveForTesting();

				Logger.ClearLogs();
				var serviceDataObject = SetupAdditionalService();
				serviceDataObject.ServiceId = "WTLKKK00000043";
				serviceDataObject.ServiceCode = new CodeDescriptionPair { Code = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, Description = "Commodity Inspection" };
				serviceDataObject.ExternalServiceId = "WTLKKK00000044";

				var reader = new AdditionalServiceDataObjectReader(serviceDataObject, Logger, Factory, dummyBO);
				var serviceBO = reader.ReadIntoBusinessObject();

				AssertNotNull("serviceBO", serviceBO);

				CombineAssertions(delegate
				{
					AssertContents(serviceBO, new ZDateTime(2011, 1, 1), new ZDateTime(2011, 1, 2), TimeSpan.FromDays(2), "Refer to the reference", "ICI", 32.1m, "Note to self", null, null, "WTLKKK00000043", "WTLKKK00000044", shouldPopulateServiceId: true, "Sub Location");
					AssertAddressContentMatches_INTHEMSYD(serviceBO.Contractor.MainAddress);
					AssertLocalClientAddress(serviceBO.Location);
					AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching JobService.
Information - Populating JobService...
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'Contractor':- Matched to 'INTHEMSYD' by code, address '' (only address).
".Trim(), Logger.Logs);
				});
			}
		}

		#region Implementation

		public static AdditionalService SetupAdditionalService()
		{
			return SetupAdditionalService(GetNewAddressData_INTHEMSYD(nameof(DocAddressType.Contractor)), GetLocalClientAddress());
		}

		public static AdditionalService SetupAdditionalService(OrganizationAddress contractor, OrganizationAddress location)
		{
			var serviceDataObject = new AdditionalService();
			serviceDataObject.Booked = new ZDateTime(2011, 1, 1);
			serviceDataObject.Completed = new ZDateTime(2011, 1, 2);
			serviceDataObject.Contractor = contractor;
			serviceDataObject.Duration = new ZDateTime(2011, 1, 3);
			serviceDataObject.Location = location;
			serviceDataObject.References = "Refer to the reference";
			serviceDataObject.ServiceCode = new CodeDescriptionPair { Code = "MAN", Description = "Manage" };
			serviceDataObject.ServiceCount = 32.1m;
			serviceDataObject.ServiceNote = "Note to self";
			serviceDataObject.SubLocation = "Sub Location";

			return serviceDataObject;
		}

		public static void AssertContents(JobService serviceBO)
		{
			AssertContents(serviceBO, AssertAddressContentMatches_INTHEMSYD, AssertLocalClientAddress);
		}

		public static void AssertContents(JobService serviceBO, Action<OrgAddress> assertContractor, Action<OrgAddress> assertLocation)
		{
			AssertContents(serviceBO, new ZDateTime(2011, 1, 1), new ZDateTime(2011, 1, 2), TimeSpan.FromDays(2), "Refer to the reference", "MAN", 32.1m, "Note to self", null, null, ZString.Empty, ZString.Empty, shouldPopulateServiceId: false, "Sub Location");
			assertContractor(serviceBO.Contractor.MainAddress);
			assertLocation(serviceBO.Location);
		}

		public static void AssertContents(
			JobService serviceBO, ZDateTime booked, ZDateTime completed, ZDateTime duration, ZString references, ZString serviceCode, ZDecimal serviceCount,
			ZString serviceNote, ZGuid? locationAddressPK, ZGuid? contractorPK, ZString serviceId, ZString externalServiceId, bool shouldPopulateServiceId, ZString subLocation)
		{
			AssertEquals("serviceBO.ES_Booked", booked, serviceBO.ES_Booked);
			AssertEquals("serviceBO.ES_Completed", completed, serviceBO.ES_Completed);
			AssertEquals("serviceBO.ES_Duration", duration, serviceBO.ES_Duration);
			AssertEquals("serviceBO.ES_References", references, serviceBO.ES_References);
			AssertEquals("serviceBO.ES_ServiceCode", serviceCode, serviceBO.ES_ServiceCode);
			AssertEquals("serviceBO.ES_ServiceCount", serviceCount, serviceBO.ES_ServiceCount);
			AssertEquals("serviceBO.ES_ServiceNote", serviceNote, serviceBO.ES_ServiceNote);
			AssertEquals("serviceBO.ES_ServiceId", serviceId, serviceBO.ES_ServiceId);
			AssertEquals("serviceBO.ES_ExternalServiceId", externalServiceId, serviceBO.ES_ExternalServiceId);
			AssertEquals("serviceBO.ShouldPopulateServiceId", shouldPopulateServiceId, serviceBO.ShouldPopulateServiceId);
			AssertEquals("serviceBO.ES_SubLocation", subLocation, serviceBO.ES_SubLocation);
			if (locationAddressPK.HasValue)
			{
				AssertEquals("serviceBO.ES_OA_Location", locationAddressPK.Value, serviceBO.ES_OA_Location);
			}
			if (contractorPK.HasValue)
			{
				AssertEquals("serviceBO.ES_OH_Contractor", contractorPK.Value, serviceBO.ES_OH_Contractor);
			}
		}
		#endregion
	}
}
