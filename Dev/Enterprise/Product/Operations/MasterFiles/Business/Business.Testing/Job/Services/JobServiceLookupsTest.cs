using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobServiceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLoadsCountrySpecificCodes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var factory = new BusinessObjectFactory();
				var service = factory.New<DummyWithServices>().Services.AddNew();
				var serviceTypes = service.Lookups.JobServiceType_List;
				AssertContains("ICI", serviceTypes.CodesAsString);
				AssertSame(serviceTypes, service.Lookups.JobServiceType_List);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var factory = new BusinessObjectFactory();
				var service = factory.New<DummyWithServices>().Services.AddNew();
				var serviceTypes = service.Lookups.JobServiceType_List;
				AssertNotContains("ICI", serviceTypes.CodesAsString);
				AssertSame(serviceTypes, service.Lookups.JobServiceType_List);
			}
		}

		#region TestServiceContractor_List

		public void TestServiceContractor_List()
		{
			Service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			AssertEquals("ServiceContractor_List should vary based on parent service type", typeof(OrgHeaderCollection), Lookups.ServiceContractor_List.GetType());

			Service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			AssertEquals("ServiceContractor_List should vary based on parent service type", typeof(FumigationContractorCollection), Lookups.ServiceContractor_List.GetType());
		}

		#endregion

		#region TestJobServiceType_List

		public void TestJobServiceType_List()
		{
			foreach (CodeDescriptionPair item in GetValidJobServiceTypes())
			{
				AssertEquals("JobServiceType_List should contain all valid types defined by parent", true, Lookups.JobServiceType_List.ContainsCode(item.Code));
			}
		}

		protected virtual CodeDescriptionPairList GetValidJobServiceTypes()
		{
			return new FreightServiceTypes();
		}

		#endregion

		#region TestOrgHeader_List

		public void TestOrgHeader_List()
		{
			AssertNotNull("OrgHeader_List should not be null.", Lookups.OrgHeader_List);
		}

		#endregion

		#region TestFumigationContractor_List

		public void TestFumigationContractor_List()
		{
			OrgHeader fumigationContractor = Factory.NewWithValidTestData<OrgHeader>();
			fumigationContractor.OH_IsMiscFreightServices = true;
			fumigationContractor.OH_IsFumigationContractor = true;

			OrgHeader regularOrg = Factory.NewWithValidTestData<OrgHeader>();
			regularOrg.OH_IsMiscFreightServices = true;
			regularOrg.OH_IsFumigationContractor = false;

			Factory.Save();

			FumigationContractorCollection collection = Lookups.FumigationContractor_List;
			collection.Load();

			AssertEquals("FumigationContractor_List should contain all saved fumigation contractors", true, collection.Contains(fumigationContractor.PK));
			AssertEquals("FumigationContractor_List should only contain fumigation contractors", false, collection.Contains(regularOrg.PK));
		}

		#endregion

		#region TestLocationAddress_List

		public void TestLocationAddress_List()
		{
			var serviceProvider = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Service.ServiceProviderPK = serviceProvider.PK;

			foreach (OrgAddress address in serviceProvider.Addresses)
			{
				AssertEquals("LocationAddress_List should contain all addresses for the location org", true, Lookups.LocationAddress_List.ContainsCode(address.OA_Code));
			}
		}

		#endregion

		#region TestServiceContext_List

		public void TestServiceContext_List()
		{
			var bizOWithContext = Factory.New<DummyWithServicesWithContext>();
			var service = bizOWithContext.Services.AddNew();
			AssertEquals("ServiceParentContext_List should contain DummyCode", true, service.Lookups.ServiceContext_List.ContainsCode("DummyCode"));

			var bizO = Factory.New<DummyWithServices>();
			service = bizO.Services.AddNew();
			AssertNull(service.Lookups.ServiceContext_List);
		}

		#endregion

		#region Measurement Basis List

		public void TestMeasurementBasisList()
		{
			var dummyRatingParent = Factory.New<DummyWithServices>();
			var dummyService = dummyRatingParent.Services.AddNew();
			var serviceMeasurements = dummyService.Lookups.MeasurementBasisList;

			AssertEquals(3, serviceMeasurements.Count);
			AssertEquals("Service Count", serviceMeasurements.GetDescriptionFromCode(JobServiceInfo.Constants.Codes.ServiceOccurrence));
			AssertEquals("Service Duration", serviceMeasurements.GetDescriptionFromCode(JobServiceInfo.Constants.Codes.Hour));
			AssertEquals("Flat Rate", serviceMeasurements.GetDescriptionFromCode(JobServiceInfo.Constants.Codes.FlatRate));

			dummyService.ES_ParentTableCode = JobDocsAndCartageSchema.Constants.Prefix;
			Factory.ClearCachedValue<CodeDescriptionPairList>("JobServiceLookups.MeasurementsBasisList");
			serviceMeasurements = dummyService.Lookups.MeasurementBasisList;

			AssertEquals(7, serviceMeasurements.Count);
			AssertEquals("Flat Rate", serviceMeasurements.GetDescriptionFromCode(JobServiceInfo.Constants.Codes.FlatRate));
			AssertEquals("Chargeable", serviceMeasurements.GetDescriptionFromCode(JobServiceInfo.Constants.Codes.Chargeable));
			AssertEquals("Container Count", serviceMeasurements.GetDescriptionFromCode(JobServiceInfo.Constants.Codes.Container));

			dummyService.ES_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			Factory.ClearCachedValue<CodeDescriptionPairList>("JobServiceLookups.MeasurementsBasisList");
			serviceMeasurements = dummyService.Lookups.MeasurementBasisList;

			AssertEquals(5, serviceMeasurements.Count);
		}

		#endregion

		#region Implementation

		JobServiceLookups Lookups
		{
			get { return Service.Lookups; }
		}

		JobService Service
		{
			get { return service ?? (service = GetNewJobService()); }
		}

		protected virtual JobService GetNewJobService()
		{
			return Factory.New<DummyWithServices>().Services.AddNew();
		}

		JobService service;

		#endregion
	}
}
