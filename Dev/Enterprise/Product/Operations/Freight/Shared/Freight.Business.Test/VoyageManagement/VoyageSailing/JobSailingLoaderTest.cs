using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobSailing.Loader))]
	sealed class JobSailingLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "IRABD";

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			JobSailing loadedSailing = loader.Load(origin.PK, destination.PK);
			AssertNotNull(loadedSailing);
			AssertEquals(loadedSailing.PK, sailing.PK);
		}

		#region Implementation

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new JobSailing.Loader(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			loader = new JobSailing.Loader(Factory);
		}

		JobSailing.Loader loader;

		#endregion
	}
}
