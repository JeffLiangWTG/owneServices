using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;

namespace Enterprise.Rating.Web.Test.Configuration
{
	public class ExtraValidationLoggerTest : TestCaseWithFactory
	{
		public void TestLog_RateQueryBusinessObject_InsuranceValueCurrency()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo.InsuranceValueCurrency = "AUD";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("AUD", rateQueryBO.InsuranceValueCurrency.Code);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.JobInfo.InsuranceValueCurrency = "XYZ";
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			AssertNull(rateQueryBO.InsuranceValueCurrency);
			_ = rateQueryBO.InsuranceValueCurrency;

			var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(1, validationLogs.Count());
			AssertEquals("Provided RateQuery.JobInfo.InsuranceValueCurrency (XYZ) is not a valid Currency Code in CW1.", validationLogs.Single());

			rateQuery.JobInfo.InsuranceValueCurrency = "UVW";
			AssertNull(rateQueryBO.InsuranceValueCurrency);

			validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(2, validationLogs.Count());
			AssertContainsExactElementsInAnyOrder(
				new[] {
						"Provided RateQuery.JobInfo.InsuranceValueCurrency (XYZ) is not a valid Currency Code in CW1.",
						"Provided RateQuery.JobInfo.InsuranceValueCurrency (UVW) is not a valid Currency Code in CW1."
					},
				validationLogs);
		}

		public void TestLog_RateQueryBusinessObject_GoodsValueCurrency()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo.GoodsValueCurrency = "AUD";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("AUD", rateQueryBO.GoodsValueCurrency.Code);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.JobInfo.GoodsValueCurrency = "XYZ";
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			AssertNull(rateQueryBO.GoodsValueCurrency);
			_ = rateQueryBO.GoodsValueCurrency;

			var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(1, validationLogs.Count());
			AssertEquals("Provided RateQuery.JobInfo.GoodsValueCurrency (XYZ) is not a valid Currency Code in CW1.", validationLogs.Single());

			rateQuery.JobInfo.GoodsValueCurrency = "UVW";
			AssertNull(rateQueryBO.GoodsValueCurrency);

			validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(2, validationLogs.Count());
			AssertContainsExactElementsInAnyOrder(
				new[] {
					"Provided RateQuery.JobInfo.GoodsValueCurrency (XYZ) is not a valid Currency Code in CW1.",
					"Provided RateQuery.JobInfo.GoodsValueCurrency (UVW) is not a valid Currency Code in CW1."
				},
				validationLogs);
		}

		public void TestLog_RateQueryBusinessObject_Origin()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.Origin.Value = "AUSYD";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("AUSYD", rateQueryBO.Origin.Code);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.Origin.Value = null;
			AssertNull(rateQueryBO.Origin);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.Origin.Value = string.Empty;
			AssertNull(rateQueryBO.Origin);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.Origin.Value = "ABCDE";
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			AssertNull(rateQueryBO.Origin);
			_ = rateQueryBO.Origin;

			var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(1, validationLogs.Count());
			AssertEquals("Provided RateQuery.Origin.Value (ABCDE) is not a valid Location in CW1.", validationLogs.Single());

			rateQuery.Origin.Value = "VWXYZ";
			AssertNull(rateQueryBO.Origin);

			validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(2, validationLogs.Count());
			AssertContainsExactElementsInAnyOrder(
				new[] {
					"Provided RateQuery.Origin.Value (ABCDE) is not a valid Location in CW1.",
					"Provided RateQuery.Origin.Value (VWXYZ) is not a valid Location in CW1."
				},
				validationLogs);
		}

		public void TestLog_RateQueryBusinessObject_Via()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.Via = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("AUSYD", rateQueryBO.Via.Code);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.Via.Value = null;
			AssertNull(rateQueryBO.Via);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.Via.Value = string.Empty;
			AssertNull(rateQueryBO.Via);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.Via.Value = "ABCDE";
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			AssertNull(rateQueryBO.Via);
			_ = rateQueryBO.Via;

			var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(1, validationLogs.Count());
			AssertEquals("Provided RateQuery.Via.Value (ABCDE) is not a valid Location in CW1.", validationLogs.Single());

			rateQuery.Via.Value = "VWXYZ";
			AssertNull(rateQueryBO.Via);

			validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(2, validationLogs.Count());
			AssertContainsExactElementsInAnyOrder(
				new[] {
					"Provided RateQuery.Via.Value (ABCDE) is not a valid Location in CW1.",
					"Provided RateQuery.Via.Value (VWXYZ) is not a valid Location in CW1."
				},
				validationLogs);
		}

		public void TestLog_RateQueryBusinessObject_Destination()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.Destination.Value = "AUSYD";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("AUSYD", rateQueryBO.Destination.Code);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.Destination.Value = null;
			AssertNull(rateQueryBO.Destination);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.Destination.Value = string.Empty;
			AssertNull(rateQueryBO.Destination);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.Destination.Value = "ABCDE";
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			AssertNull(rateQueryBO.Destination);
			_ = rateQueryBO.Destination;

			var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(1, validationLogs.Count());
			AssertEquals("Provided RateQuery.Destination.Value (ABCDE) is not a valid Location in CW1.", validationLogs.Single());

			rateQuery.Destination.Value = "VWXYZ";
			AssertNull(rateQueryBO.Destination);

			validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(2, validationLogs.Count());
			AssertContainsExactElementsInAnyOrder(
				new[] {
					"Provided RateQuery.Destination.Value (ABCDE) is not a valid Location in CW1.",
					"Provided RateQuery.Destination.Value (VWXYZ) is not a valid Location in CW1."
				},
				validationLogs);
		}

		public void TestLog_RateQueryBusinessObject_PlannedLoad()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.PlannedLoad = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("AUSYD", rateQueryBO.PlannedLoad.Code);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.PlannedLoad.Value = null;
			AssertNull(rateQueryBO.PlannedLoad);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.PlannedLoad.Value = string.Empty;
			AssertNull(rateQueryBO.PlannedLoad);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.PlannedLoad.Value = "ABCDE";
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			AssertNull(rateQueryBO.PlannedLoad);
			_ = rateQueryBO.PlannedLoad;

			var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(1, validationLogs.Count());
			AssertEquals("Provided RateQuery.PlannedLoad.Value (ABCDE) is not a valid Location in CW1.", validationLogs.Single());

			rateQuery.PlannedLoad.Value = "VWXYZ";
			AssertNull(rateQueryBO.PlannedLoad);

			validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(2, validationLogs.Count());
			AssertContainsExactElementsInAnyOrder(
				new[] {
					"Provided RateQuery.PlannedLoad.Value (ABCDE) is not a valid Location in CW1.",
					"Provided RateQuery.PlannedLoad.Value (VWXYZ) is not a valid Location in CW1."
				},
				validationLogs);
		}

		public void TestLog_RateQueryBusinessObject_PlannedDischarge()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.PlannedDischarge = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("AUSYD", rateQueryBO.PlannedDischarge.Code);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.PlannedDischarge.Value = null;
			AssertNull(rateQueryBO.PlannedDischarge);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.PlannedDischarge.Value = string.Empty;
			AssertNull(rateQueryBO.PlannedDischarge);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.PlannedDischarge.Value = "ABCDE";
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			AssertNull(rateQueryBO.PlannedDischarge);
			_ = rateQueryBO.PlannedDischarge;

			var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(1, validationLogs.Count());
			AssertEquals("Provided RateQuery.PlannedDischarge.Value (ABCDE) is not a valid Location in CW1.", validationLogs.Single());

			rateQuery.PlannedDischarge.Value = "VWXYZ";
			AssertNull(rateQueryBO.PlannedDischarge);

			validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(2, validationLogs.Count());
			AssertContainsExactElementsInAnyOrder(
				new[] {
					"Provided RateQuery.PlannedDischarge.Value (ABCDE) is not a valid Location in CW1.",
					"Provided RateQuery.PlannedDischarge.Value (VWXYZ) is not a valid Location in CW1."
				},
				validationLogs);
		}

		public void TestLog_RateQueryBusinessObject_RateOrigin()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.RateOrigin = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("AUSYD", rateQueryBO.RateOrigin.Code);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.RateOrigin.Value = null;
			AssertNull(rateQueryBO.RateOrigin);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.RateOrigin.Value = string.Empty;
			AssertNull(rateQueryBO.RateOrigin);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.RateOrigin.Value = "ABCDE";
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			AssertNull(rateQueryBO.RateOrigin);
			_ = rateQueryBO.RateOrigin;

			var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(1, validationLogs.Count());
			AssertEquals("Provided RateQuery.RateOrigin.Value (ABCDE) is not a valid Location in CW1.", validationLogs.Single());

			rateQuery.RateOrigin.Value = "VWXYZ";
			AssertNull(rateQueryBO.RateOrigin);

			validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(2, validationLogs.Count());
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"Provided RateQuery.RateOrigin.Value (ABCDE) is not a valid Location in CW1.",
					"Provided RateQuery.RateOrigin.Value (VWXYZ) is not a valid Location in CW1."
				},
				validationLogs);
		}

		public void TestLog_RateQueryBusinessObject_RateDestination()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);

			rateQuery.RateDestination = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals("AUSYD", rateQueryBO.RateDestination.Code);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.RateDestination.Value = null;
			AssertNull(rateQueryBO.RateDestination);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.RateDestination.Value = string.Empty;
			AssertNull(rateQueryBO.RateDestination);
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			rateQuery.RateDestination.Value = "ABCDE";
			AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

			AssertNull(rateQueryBO.RateDestination);
			_ = rateQueryBO.RateDestination;

			var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(1, validationLogs.Count());
			AssertEquals("Provided RateQuery.RateDestination.Value (ABCDE) is not a valid Location in CW1.", validationLogs.Single());

			rateQuery.RateDestination.Value = "VWXYZ";
			AssertNull(rateQueryBO.RateDestination);

			validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
			AssertEquals(2, validationLogs.Count());
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"Provided RateQuery.RateDestination.Value (ABCDE) is not a valid Location in CW1.",
					"Provided RateQuery.RateDestination.Value (VWXYZ) is not a valid Location in CW1."
				},
				validationLogs);
		}

		public void TestLog_RateQueryBusinessObject_CompanyTariffLevelOverride()
		{
			Factory.New<GlobalTariff>();
			Factory.New<GlobalTariff>();
			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
				rateQuery.JobInfo = new JobInfo();

				var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
				AssertEquals((ZByte)0, rateQueryBO.CompanyTariffLevelOverride);
				AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

				rateQuery.JobInfo.CTLevelOverride = 2;
				rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
				AssertEquals((ZByte)2, rateQueryBO.CompanyTariffLevelOverride);
				AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

				rateQuery.JobInfo.CTLevelOverride = 3;
				rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
				AssertEquals((ZByte)0, rateQueryBO.CompanyTariffLevelOverride);
				_ = rateQueryBO.CompanyTariffLevelOverride;

				var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
				AssertEquals(1, validationLogs.Count());
				AssertEquals("Provided RateQuery.JobInfo.CTLevelOverride. (3) is not a valid Company Tariff Level in CW1.", validationLogs.Single());
			}

			ExtraValidationLogger.RemoveAllLogs();

			using (DataRegistryRating.Instance.AllowOverrideCompanyTariffLevel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
				rateQuery.JobInfo = new JobInfo();

				var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
				AssertEquals((ZByte)0, rateQueryBO.CompanyTariffLevelOverride);
				AssertEquals(0, ExtraValidationLogger.GetExtraValidationMessages().Count());

				rateQuery.JobInfo.CTLevelOverride = 2;
				rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
				AssertEquals((ZByte)0, rateQueryBO.CompanyTariffLevelOverride);
				_ = rateQueryBO.CompanyTariffLevelOverride;

				var validationLogs = ExtraValidationLogger.GetExtraValidationMessages();
				AssertEquals(0, validationLogs.Count());
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			ExtraValidationLogger.RemoveAllLogs();
		}
	}
}
