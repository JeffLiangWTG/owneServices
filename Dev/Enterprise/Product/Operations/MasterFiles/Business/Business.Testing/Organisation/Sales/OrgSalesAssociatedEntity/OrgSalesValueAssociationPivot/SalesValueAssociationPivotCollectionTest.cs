using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesValueAssociationPivotCollection))]
	sealed class SalesValueAssociationPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<SalesValueAssociationPivotCollection>
	{
		#region Default Values

		public void TestDefaultsForNewElement()
		{
			var sales = Factory.New<OrgSales>();
			var collection = new SalesValueAssociationPivotCollection(sales, false);
			var pivot = collection.AddNew();
			AssertEquals(sales.PK, pivot.SVP_TradeId);
			AssertEquals(sales.TablePrefix, pivot.SVP_TradeTableCode);
		}

		#endregion

		#region Relationship

		public void TestRelationship()
		{
			var sales1 = Factory.NewWithValidTestData<OrgSales>();
			var sales2 = Factory.NewWithValidTestData<OrgSales>();
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var rating = ObjectFactory.Get<IRating>();
			var currentCompanyQuote = Factory.NewWithValidTestData(rating.QuoteType);
			currentCompanyQuote[RatingHeaderSchema.TH_RateType] = "QTE";
			var currentCompanyEntry = Factory.New(rating.RateEntryType);
			currentCompanyEntry[RateEntrySchema.TI_TH] = currentCompanyQuote.PK;
			currentCompanyEntry[RateEntrySchema.TI_GC_Publisher] = GlbCompany.CurrentCompany.PK;
			currentCompanyEntry[RateEntrySchema.TI_Mode] = "SEA";
			currentCompanyEntry[RateEntrySchema.TI_RateCategory] = "FCL";

			var anotherCompanyQuote = Factory.NewWithValidTestData(rating.QuoteType);
			anotherCompanyQuote[RatingHeaderSchema.TH_RateType] = "QTE";
			anotherCompanyQuote[RatingHeaderSchema.TH_GC] = anotherCompany.PK;
			var anotherCompanyEntry = Factory.New(rating.RateEntryType);
			anotherCompanyEntry[RateEntrySchema.TI_TH] = anotherCompanyQuote.PK;
			anotherCompanyEntry[RateEntrySchema.TI_GC_Publisher] = anotherCompany.PK;
			anotherCompanyEntry[RateEntrySchema.TI_Mode] = "SEA";
			anotherCompanyEntry[RateEntrySchema.TI_RateCategory] = "FCL";

			var pivotSales1ToOpp = AddTradeLanePivot(sales1, opportunity);
			var pivotSales1ToComm = AddTradeLanePivot(sales1, communication);

			var pivotSales2ToOpp = AddTradeLanePivot(sales2, opportunity);
			var pivotSales2ToCurrentCompanyEntry = AddTradeLanePivot(sales2, currentCompanyEntry);
			var pivotSales2ToAnotherCompanyEntry = AddTradeLanePivot(sales2, anotherCompanyEntry);

			Factory.Save();

			var collection1 = new SalesValueAssociationPivotCollection(sales1, true);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesValueAssociationPivot>.PKOnlyComparer, new[] { pivotSales1ToOpp, pivotSales1ToComm }, collection1);

			var collection2 = new SalesValueAssociationPivotCollection(sales2, true);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesValueAssociationPivot>.PKOnlyComparer, new[] { pivotSales2ToOpp, pivotSales2ToCurrentCompanyEntry }, collection2);

			var globalCollection1 = new SalesValueAssociationPivotCollection(sales1, false);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesValueAssociationPivot>.PKOnlyComparer, new[] { pivotSales1ToOpp, pivotSales1ToComm }, globalCollection1);

			var globalCollection2 = new SalesValueAssociationPivotCollection(sales2, false);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesValueAssociationPivot>.PKOnlyComparer, new[] { pivotSales2ToOpp, pivotSales2ToCurrentCompanyEntry, pivotSales2ToAnotherCompanyEntry }, globalCollection2);
		}

		#endregion

		#region Implementation

		OrgSalesValueAssociationPivot AddTradeLanePivot<T>(OrgSales sales, T associatedEntity)
			where T : BusinessObject
		{
			var collection = new TradeLanePivotCollection<T>(associatedEntity);
			return collection.AddPivotFor(sales);
		}

		protected override SalesValueAssociationPivotCollection GetCollectionToTest()
		{
			var sales = Factory.New<OrgSales>();
			return new SalesValueAssociationPivotCollection(sales, false);
		}

		#endregion
	}
}
