using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AdditionalServiceBusinessObjectFinder<JobService>))]
	sealed class AdditionalServiceBusinessObjectFinderTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AdditionalServiceBusinessObjectFinder<JobService>(null));

			var dataObject = new AdditionalService();
			dataObject.ServiceId = ZString.Empty;
			dataObject.ServiceCode = new CodeDescriptionPair { Code = Core.Constants.FreightServiceType.Codes.Overpack };

			var service1 = Factory.New<JobService>();
			service1.ES_ServiceId = ZString.Empty;
			service1.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			service1.ES_ExternalServiceId = ZString.Empty;
			service1.ShouldPopulateServiceId = false;

			var service2 = Factory.New<JobService>();
			service2.ES_ServiceId = "WTLKKK00000043";
			service2.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Overpack;
			service2.ES_ExternalServiceId = ZString.Empty;

			var service3 = Factory.New<JobService>();
			service3.ES_ServiceId = ZString.Empty;
			service3.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Overpack;
			service3.ES_ExternalServiceId = ZString.Empty;
			service3.ShouldPopulateServiceId = false;

			var service4 = Factory.New<JobService>();
			service4.ES_ServiceId = "WTLKKK00000044";
			service4.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Tailgate;
			service4.ES_ExternalServiceId = "WTLKKK00000045";

			var service5 = Factory.New<JobService>();
			service5.ES_ServiceId = "WTLKKK00000046";
			service5.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineUnpack;
			service5.ES_ExternalServiceId = "WTLKKK00000047";

			var finder = new AdditionalServiceBusinessObjectFinder<JobService>(dataObject);
			CombineAssertions("Should match on service code for existing services", () =>
			{
				AssertNull(finder.Find(new[] { service1, service2, service4, service5 }));
				AssertEquals(service3, finder.Find(new[] { service3 }));
			});

			dataObject.ServiceId = "WTLKKK00000047";
			CombineAssertions("Should match on service id/external service id for new services", () =>
			{
				AssertNull(finder.Find(new[] { service1, service2, service3, service4 }));
				AssertEquals(service5, finder.Find(new[] { service5 }));
			});
		}
	}
}
