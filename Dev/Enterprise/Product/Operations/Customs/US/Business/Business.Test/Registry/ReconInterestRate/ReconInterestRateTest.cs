using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(ReconInterestRate))]
	sealed class ReconInterestRateTest : RegistryBusinessObjectTemplateTestCase<ReconInterestRate>
	{
		public void TestValidateStartDate()
		{
			ReconInterestRateCollection collection = new ReconInterestRateCollection();
			ReconInterestRate otherRate = collection.AddNew(new ZDateTime(2008, 6, 1), new ZDateTime(2008, 12, 31), 5m);
			ReconInterestRate rate = collection.AddNew();
			rate.StartDate = ZDateTime.Empty;
			rate.EndDate = new ZDateTime(2009, 6, 1);
			AssertHasErrorContaining(rate.StartDateInfo, MandatoryValidation.MustBeEntered);
			rate.StartDate = new ZDateTime(2008, 1, 1);
			AssertNoErrorContaining(rate.StartDateInfo, MandatoryValidation.MustBeEntered);
			string errorMessage = string.Format("Other rate's dates: '{0}' and '{1}'.", otherRate.StartDate.ToShortDateString(), otherRate.EndDate.ToShortDateString());
			AssertHasErrorContaining(rate.StartDateInfo, "This rate's dates overlap another rate's dates.");
			AssertHasErrorContaining(rate.StartDateInfo, errorMessage);
			rate.StartDate = new ZDateTime(2008, 10, 1);
			AssertHasErrorContaining(rate.StartDateInfo, "This rate's dates overlap another rate's dates.");
			AssertHasErrorContaining(rate.StartDateInfo, errorMessage);
			rate.StartDate = new ZDateTime(2009, 1, 1);
			AssertNoErrorContaining(rate.StartDateInfo, "This rate's dates overlap another rate's dates.");
			AssertNoErrorContaining(rate.StartDateInfo, errorMessage);
		}

		public void TestValidateEndDate()
		{
			ReconInterestRateCollection collection = new ReconInterestRateCollection();
			ReconInterestRate otherRate = collection.AddNew(new ZDateTime(2008, 6, 1), new ZDateTime(2008, 12, 31), 5m);
			ReconInterestRate rate = collection.AddNew();
			rate.StartDate = new ZDateTime(2008, 1, 1);
			rate.EndDate = ZDateTime.Empty;
			AssertHasErrorContaining(rate.EndDateInfo, MandatoryValidation.MustBeEntered);
			rate.EndDate = new ZDateTime(2007, 1, 1);
			AssertNoErrorContaining(rate.EndDateInfo, MandatoryValidation.MustBeEntered);
			AssertHasError(rate.EndDateInfo, "End Date cannot be less than Start Date.");
			rate.EndDate = new ZDateTime(2009, 1, 1);
			AssertNoError(rate.EndDateInfo, "End Date cannot be less than Start Date.");
			string errorMessage = string.Format("Other rate's dates: '{0}' and '{1}'.", otherRate.StartDate.ToShortDateString(), otherRate.EndDate.ToShortDateString());
			AssertHasErrorContaining(rate.EndDateInfo, "This rate's dates overlap another rate's dates.");
			AssertHasErrorContaining(rate.EndDateInfo, errorMessage);
			rate.EndDate = new ZDateTime(2008, 10, 1);
			AssertHasErrorContaining(rate.EndDateInfo, "This rate's dates overlap another rate's dates.");
			AssertHasErrorContaining(rate.EndDateInfo, errorMessage);
			rate.EndDate = new ZDateTime(2008, 5, 30);
			AssertNoErrorContaining(rate.EndDateInfo, "This rate's dates overlap another rate's dates.");
			AssertNoErrorContaining(rate.EndDateInfo, errorMessage);
		}

		public void TestValidateRate()
		{
			ReconInterestRate rate = new ReconInterestRate();
			rate.Rate = 51m;
			AssertHasError(rate.RateInfo, "Rate must be greater than 0 and less than 50.");
			rate.Rate = 0m;
			AssertHasError(rate.RateInfo, "Rate must be greater than 0 and less than 50.");
			rate.Rate = 5m;
			AssertNoError(rate.RateInfo, "Rate must be greater than 0 and less than 50.");
		}

		public void TestIsWithinDateRate()
		{
			ReconInterestRate rate = new ReconInterestRate();
			rate.StartDate = new ZDateTime(2008, 4, 1);
			rate.EndDate = new ZDateTime(2008, 6, 30);
			AssertEquals(false, rate.IsWithinDateRate(new ZDateTime(2007, 3, 31)));
			AssertEquals(true, rate.IsWithinDateRate(new ZDateTime(2008, 4, 1)));
			AssertEquals(true, rate.IsWithinDateRate(new ZDateTime(2008, 5, 1)));
			AssertEquals(true, rate.IsWithinDateRate(new ZDateTime(2008, 6, 30)));
			AssertEquals(false, rate.IsWithinDateRate(new ZDateTime(2008, 7, 1)));
		}

		protected override ReconInterestRate GetBusinessObjectToClone()
		{
			ReconInterestRate rate = new ReconInterestRate();
			rate.StartDate = new ZDateTime(2008, 4, 1);
			rate.EndDate = new ZDateTime(2008, 6, 30);
			rate.Rate = 6m;
			return rate;
		}

		protected override ReconInterestRate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
