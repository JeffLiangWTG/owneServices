using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Rating.Business.Testing
{
	public class JobServicePresenceCheckerTest : RatingTestCase
	{
		public void TestCombineRatingResults_WhenTotalCostNotZeroAndRateIsZero()
		{
			var collection = new JobServicesCollection();
			collection.Add(new JobServiceInfo(true, "ORG", "FUM", "Fumigation", totalCost: 10m));

			var checker = new JobServicePresenceChecker();
			Combine(checker, collection, Enumerable.Empty<AutoRateInfo>());

			Assert("service with TotalCost not zero is not checked", !checker.ServicesToCheck.Any());
		}

		public void TestGetMissingJobServicesNotification()
		{
			var checker = new JobServicePresenceChecker();
			var jobName = "Shipment SH10011991";

			Assert(string.IsNullOrEmpty(checker.GetMissingJobServicesNotification(jobName)));

			var testCode1 = Helper.ChargeCodes.New("TSTFUMI", "Test Fumigation", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation);
			var testCode2 = Helper.ChargeCodes.New("TSTBFUMI", "Test Broker Fumigation", FlatCalculator.Code, ChargeCodeGroupList.Codes.Brokerage, Core.Constants.FreightServiceType.Codes.Fumigation);
			var testCode3 = Helper.ChargeCodes.New("TSTOQIN", "Test Fumigation", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.QuarantineInspection);
			var testCode4 = Helper.ChargeCodes.New("TSTOLBR", "Test Labor", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Labor);

			var emptyCollection = new JobServicesCollection();
			Combine(checker, emptyCollection, Enumerable.Empty<AutoRateInfo>());
			Assert(string.IsNullOrEmpty(checker.GetMissingJobServicesNotification(jobName)));

			var collection = new JobServicesCollection();
			collection.Add(new JobServiceInfo(true, "ORG", "FUM", "Insect Squeezing"));
			collection.Add(new JobServiceInfo(false, "ORG", "QIN", "Insect Detection"));
			Combine(checker, collection, Enumerable.Empty<AutoRateInfo>());

			var expectedNotification = @"Rates for the below job services were not found. Please either create these charge codes, or ensure that your rate contains these charges and have the correct commodity code, service level and validity dates.
Until you do this, these charges will not be rated.

  • Insect Squeezing: ORG / FUM for Shipment SH10011991";

			AssertEquals(expectedNotification, checker.GetMissingJobServicesNotification(jobName));

			collection = new JobServicesCollection();
			collection.Add(new JobServiceInfo(true, "", "FUM", "Insect Squeezing by Broker"));
			Combine(checker, collection, new[] { GetRateInfoWithChargeCode(testCode2) });

			expectedNotification = @"Rates for the below job services were not matched for exact charge group/service type pairs. These job services were still matched and rated during this Autorating process as shown below:

• FUM (Insect Squeezing) service for ORG charge group has not been matched exactly but still has been rated for another charge group match and TSTBFUMI (Test Broker Fumigation) charge code was applied.";

			AssertEquals(expectedNotification, checker.GetMissingJobServicesNotification(jobName));

			Combine(checker, emptyCollection, new[] { GetRateInfoWithChargeCode(testCode1) });
			Assert(string.IsNullOrEmpty(checker.GetMissingJobServicesNotification(jobName)));

			collection = new JobServicesCollection();
			collection.Add(new JobServiceInfo(true, "ORG", "FUM", "Insect Squeezing"));

			checker = new JobServicePresenceChecker();
			Combine(checker, collection, Enumerable.Empty<AutoRateInfo>());

			collection = new JobServicesCollection();
			collection.Add(new JobServiceInfo(true, "", "FUM", "Insect Squeezing by Broker"));
			Combine(checker, collection, new[] { GetRateInfoWithChargeCode(testCode2) });

			AssertEquals(expectedNotification, checker.GetMissingJobServicesNotification(jobName));
		}

		AutoRateInfo GetRateInfoWithChargeCode(AccChargeCode code)
		{
			var info = new AutoRateInfo(Factory);
			info.ChargeCode = code;
			return info;
		}

		void Combine(JobServicePresenceChecker checker, JobServicesCollection servicesCollection, IEnumerable<AutoRateInfo> infos)
		{
			var mockRepository = new MockRepository(MockBehavior.Strict);
			var autoRatingMock = mockRepository.Create<IAutoRating>();
			autoRatingMock.Setup(x => x.JobServices).Returns(servicesCollection);
			checker.CombineRatingResults(autoRatingMock.Object, infos);
		}
	}
}
