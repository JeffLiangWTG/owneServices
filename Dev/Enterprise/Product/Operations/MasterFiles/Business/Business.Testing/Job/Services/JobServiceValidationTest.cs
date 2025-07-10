using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobServiceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckES_ServiceCode()
		{
			var dummy = Factory.New<DummyWithServices>();

			var service1 = dummy.Services.AddNew();
			service1.ShouldPopulateServiceId = false;
			var service2 = dummy.Services.AddNew();
			service2.ShouldPopulateServiceId = false;

			var service3 = dummy.Services.AddNew();
			service3.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			var service4 = dummy.Services.AddNew();
			service4.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;

			//Test mandatory validation
			service1.ES_ServiceCode = "";
			service2.ES_ServiceCode = "";
			Assert("Expect error on ES_ServiceCode", service1.ES_ServiceCodeInfo.HasErrors());
			Assert("Expect error on ES_ServiceCode", service2.ES_ServiceCodeInfo.HasErrors());

			//Test list validation
			service1.ES_ServiceCode = "XYZ";
			service2.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Assert("Expect error on ES_ServiceCode", service1.ES_ServiceCodeInfo.HasErrors());
			Assert("Expect no error on ES_ServiceCode", !service2.ES_ServiceCodeInfo.HasErrors());

			//Test unique service type validation
			service1.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			service2.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Assert("Expect error on ES_ServiceCode", service1.ES_ServiceCodeInfo.HasErrors());
			Assert("Expect error on ES_ServiceCode", service2.ES_ServiceCodeInfo.HasErrors());

			//ensure validaiton uses the Lookups list instead of the Freight List
			service1.ES_ServiceCode = "XXX";
			AssertEquals("Expect error on ES_ServiceCode", true, service1.ES_ServiceCodeInfo.HasErrors());

			CombineAssertions("Allow multiple services of the same type", () =>
			{
				Assert("Expect no error on service3.ES_ServiceCode", !service3.ES_ServiceCodeInfo.HasErrors());
				Assert("Expect no error on service4.ES_ServiceCode", !service4.ES_ServiceCodeInfo.HasErrors());
			});
		}

		public void TestServiceTypeNeedsToBeUnique()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var factory = new BusinessObjectFactory();
				var lookupCodes = new CodeDescriptionPairList();
				lookupCodes.AddPair("XXX", "Dummy Service Type");
				lookupCodes.AddPair("ICI", "Commodity Inspection");
				var countrySpecificServiceCodes = factory.GetCachedValue<ICodeDescriptionPairList>("Enterprise.MasterFiles.Business.JobServiceLookups.JobServiceTypeList_" + Core.Constants.CountryCodes.Taiwan, () => lookupCodes);

				var parent = factory.New<DummyWithServices>();
				var service1 = parent.Services.AddNew();
				service1.ES_ServiceCode = "ICI";
				service1.ShouldPopulateServiceId = false;
				var service2 = parent.Services.AddNew();
				service2.ES_ServiceCode = "ICI";
				service2.ShouldPopulateServiceId = false;
				Assert(service2.Lookups.JobServiceType_List.ContainsCode("ICI"));
				Assert(service2.Lookups.CountrySpecificJobServiceTypeList.ContainsCode("ICI"));
				AssertNoErrorContaining(service2.ES_ServiceCodeInfo, "Can't have more than one ");

				service1.ES_ServiceCode = "XXX";
				service2.ES_ServiceCode = "XXX";
				Assert(service2.Lookups.JobServiceType_List.ContainsCode("XXX"));
				Assert(service2.Lookups.CountrySpecificJobServiceTypeList.ContainsCode("XXX"));
				AssertHasErrorContaining(service2.ES_ServiceCodeInfo, "Can't have more than one ");
			}
		}

		public void TestCheckES_RX_NKServiceRateCurrency()
		{
			var dummy = Factory.New<DummyWithServices>();
			var service = dummy.Services.AddNew();
			service.Validation.ValidateES_RX_NKServiceRateCurrency();
			AssertNoErrors(service.ES_RX_NKServiceRateCurrencyInfo);

			service.ES_ServiceRate = 15m;
			AssertHasError(service.ES_RX_NKServiceRateCurrencyInfo, "Please enter a Service Rate Currency.");

			service.ES_RX_NKServiceRateCurrency = Core.Constants.CurrencyCodes.Australia;
			service.Validation.ValidateES_RX_NKServiceRateCurrency();
			AssertNoErrors(service.ES_RX_NKServiceRateCurrencyInfo);

			service.ES_RX_NKServiceRateCurrency = "XXX";
			service.Validation.ValidateES_RX_NKServiceRateCurrency();
			AssertHasError(service.ES_RX_NKServiceRateCurrencyInfo, "Enter a valid Service Rate Currency.");
		}

		public void TestCheckES_MeasurementBasis()
		{
			var dummy = Factory.New<DummyWithServices>();
			var service = dummy.Services.AddNew();
			service.Validation.ValidateES_MeasurementBasis();
			AssertNoErrors(service.ES_MeasurementBasisInfo);

			service.ES_ServiceRate = 15m;
			AssertHasError(service.ES_MeasurementBasisInfo, "Please enter a Measurement Basis.");

			var validMeasurementType = service.Lookups.MeasurementBasisList.Cast<CodeDescriptionPair>().FirstOrDefault();
			AssertNotNull("List should have at least service count and duration defaults", validMeasurementType);

			service.ES_MeasurementBasis = validMeasurementType.Code;
			service.Validation.ValidateES_MeasurementBasis();
			AssertNoErrors(service.ES_MeasurementBasisInfo);

			service.ES_MeasurementBasis = "XXX";
			service.Validation.ValidateES_MeasurementBasis();
			AssertHasError(service.ES_MeasurementBasisInfo, "Enter a valid Measurement Basis.");
		}
	}
}
