using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Moq;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.Rating.Business.Test
{
	public class RateChooserRelatedChargesProviderTest : RatingTestCase
	{
		public void TestGetAllCW1Charges_PossibleCarriers()
		{
			var carrier1 = TransportProvider1;
			var carrier2 = TransportProvider2;
			carrier2.OH_IsShippingLine = true;
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var costing1 = Helper.NewCosting(carrier1);
			var costing2 = Helper.NewCosting(carrier2);
			var costing3 = Helper.NewCosting(otherOrg);
			costing1.AddRateEntryWithFlatRateLine(RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "", "ODOC", 100, "AUD");
			costing2.AddRateEntryWithFlatRateLine(RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "", "ODOC", 200, "AUD");
			costing3.AddRateEntryWithFlatRateLine(RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "", "OSEC", 300, "AUD");

			var criteria = new TestRatingCriteria("AU", "", 0, null, null);
			criteria.PossibleCarriers = new[] { carrier1, carrier2 };
			criteria.Creditors = Creditors.New(
				OrgWithSource.New(carrier1, new List<string>() { "Carrier" }),
				OrgWithSource.New(carrier2, new List<string>() { "Carrier" }),
				OrgWithSource.New(otherOrg, new List<string>() { "Supplier" }));

			var dummyLogger = new DummyLogger();
			var cw1RatesProvider = new CW1RatesProvider(Factory, dummyLogger);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var carrier1InFactory2 = factory2.Load<OrgHeader>(carrier1.PK);
			var carrier2InFactory2 = factory2.Load<OrgHeader>(carrier2.PK);

			// Selected carrier1
			CombineAssertions(() =>
			{
				using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object))
				using (_Rating.StartCost())
				{
					criteria.ValuesCanBeSet = false; //ValueCanBeSet is always true on TestRatingCriteria, but in real world examples, it can be false on Criteria.
					var provider = new RateChooserRelatedChargesProvider(Factory, dummyLogger, criteria, cw1RatesProvider, carrier1InFactory2);
					var infos = provider.GetAllCW1Charges();
					AssertEquals("selected carrier costing was matched", 1, infos.Count(x => x.ProviderPK == carrier1.PK));
					AssertEquals("other org costing was matched ", 1, infos.Count(x => x.ProviderPK == otherOrg.PK));
					AssertEquals("unused carrier costing was not matched ", 0, infos.Count(x => x.ProviderPK == carrier2.PK));
					AssertEquals("total", 2, infos.Count);
					AssertEquals("We should not change ValuesCanBeSet after calling GetAllCW1Charges()", false, criteria.ValuesCanBeSet);
				}
			});

			// Selected carrier2
			CombineAssertions(() =>
			{
				using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object))
				using (_Rating.StartCost())
				{
					criteria.ValuesCanBeSet = false;
					var provider = new RateChooserRelatedChargesProvider(Factory, dummyLogger, criteria, cw1RatesProvider, carrier2InFactory2);
					var infos = provider.GetAllCW1Charges();
					AssertEquals("selected carrier costing was matched", 1, infos.Count(x => x.ProviderPK == carrier2.PK));
					AssertEquals("other org costing was matched ", 1, infos.Count(x => x.ProviderPK == otherOrg.PK));
					AssertEquals("unsused carrier costing was not matched ", 0, infos.Count(x => x.ProviderPK == carrier1.PK));
					AssertEquals("total", 2, infos.Count);
					AssertEquals("We should not change ValuesCanBeSet after calling GetAllCW1Charges()", false, criteria.ValuesCanBeSet);
				}
			});
		}
	}
}
