using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SailingsByCountryView))]
	sealed class SailingsByCountryViewTest : BusinessObjectCollectionViewTestCase<SailingsByCountryView>
	{
		public void TestTestingTheCorrectCollection()
		{
			AssertEquals("are we even testing the correct collection.", typeof(SailingsByCountryView), GetCollectionToTest().GetType());
		}

		#region Implementation

		public SailingsByCountryViewTest()
		{
			LocationRotation = new string[] { "AUGOV", "AUBNE", "AUSYD", "AUMEL", "NZAKL", "SGSIN", "MYBAG", "USLAX" };
		}

		protected override SailingsByCountryView GetCollectionToTest()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			return voyage.Countries.GetCountry("AU", true).Sailings;
		}

		readonly string[] LocationRotation;
		int LocationIndex;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = LocationRotation[(LocationIndex++) & 3];

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = LocationRotation[((LocationIndex++) & 3) + 4];

			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			return sailing;
		}

		#endregion
	}
}
