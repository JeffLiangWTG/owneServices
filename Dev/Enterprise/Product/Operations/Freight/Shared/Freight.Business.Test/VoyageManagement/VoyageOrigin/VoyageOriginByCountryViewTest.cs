using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(VoyageOriginByCountryView))]
	sealed class VoyageOriginByCountryViewTest : BusinessObjectCollectionViewTestCase<VoyageOriginByCountryView>
	{
		public void TestTestingTheCorrectCollection()
		{
			AssertEquals("are we even testing the correct collection.", typeof(VoyageOriginByCountryView), GetCollectionToTest().GetType());
		}

		#region Implementation

		public VoyageOriginByCountryViewTest()
		{
			LocationRotation = new string[] { "AUGOV", "AUBNE", "AUSYD", "AUMEL" };
		}

		protected override VoyageOriginByCountryView GetCollectionToTest()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			return voyage.Countries.GetCountry("AU", true).Origins;
		}

		readonly string[] LocationRotation;
		int LocationIndex;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = LocationRotation[(LocationIndex++) & 3];
			return origin;
		}

		#endregion
	}
}
