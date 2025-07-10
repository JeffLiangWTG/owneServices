using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(EntitySalesWrapperCollection))]
	sealed class EntitySalesWrapperCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationship()
		{
			var anotherFactory = new BusinessObjectFactory();
			var product = anotherFactory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var orgInOtherFactory = anotherFactory.NewWithValidTestData<OrgHeader>();
			var salesHeaderCollectionInOtherFactory = (SalesHeaderCollection)orgInOtherFactory.ActualAndProspectiveSalesHeaderCollection;
			var salesHeaderInOtherFactory = salesHeaderCollectionInOtherFactory.AddNew(product);
			var tradeLane1InOtherFactory = salesHeaderInOtherFactory.EntitySalesCollectionProductView.AddNew();
			var tradeLane2InOtherFactory = salesHeaderInOtherFactory.EntitySalesCollectionProductView.AddNew();
			var tradeLane3InOtherFactory = salesHeaderInOtherFactory.EntitySalesCollectionProductView.AddNew();

			var opportunity1InOtherFactory = orgInOtherFactory.SalesOpportunities.AddNew();
			var pivot11InOtherFactory = tradeLane1InOtherFactory.SalesAssociationPivotCollectionGlobal.AddNew(opportunity1InOtherFactory);
			var pivot12InOtherFactory = tradeLane2InOtherFactory.SalesAssociationPivotCollectionGlobal.AddNew(opportunity1InOtherFactory);

			var opportunity2InOtherFactory = orgInOtherFactory.SalesOpportunities.AddNew();
			var pivot21InOtherFactory = tradeLane1InOtherFactory.SalesAssociationPivotCollectionGlobal.AddNew(opportunity2InOtherFactory);

			anotherFactory.Save();

			var org = Factory.Load<OrgHeader>(orgInOtherFactory.PK);
			var opportunity1 = Factory.Load<OrgOpportunity>(opportunity1InOtherFactory.PK);
			var salesHeaderCollection1 = (SalesHeaderCollection)opportunity1.ActualAndProspectiveSalesHeaderCollection;
			var collection1 = salesHeaderCollection1.EntitySalesCollection;
			var expectedDbHits = new Dictionary<string, int>
			{
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgOpportunitySchema.Constants.TableName, 1 },
				{ OrgSalesProductSchema.Constants.TableName, 1 },
				{ OrgSalesSchema.Constants.TableName, 2 },
				{ OrgTradeDetailSchema.Constants.TableName, 2 },
				{ RefCurrencySchema.Constants.TableName, 1 },
			};
			AssertMaxDbHits(expectedDbHits, Factory);

			var opportunity2 = Factory.Load<OrgOpportunity>(opportunity2InOtherFactory.PK);
			var tradeLane1 = Factory.Load<EntitySalesWrapper>(tradeLane1InOtherFactory.PK);
			var tradeLane2 = Factory.Load<EntitySalesWrapper>(tradeLane2InOtherFactory.PK);
			var tradeLane3 = Factory.Load<EntitySalesWrapper>(tradeLane3InOtherFactory.PK);
			var pivot11 = Factory.Load<OrgSalesValueAssociationPivot>(pivot11InOtherFactory.PK);
			var pivot12 = Factory.Load<OrgSalesValueAssociationPivot>(pivot12InOtherFactory.PK);
			var pivot21 = Factory.Load<OrgSalesValueAssociationPivot>(pivot21InOtherFactory.PK);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<EntitySalesWrapper>.PKOnlyComparer,
				new[] { tradeLane1, tradeLane2 },
				collection1.Cast<EntitySalesWrapper>());

			collection1.Remove(tradeLane1);
			Assert(!opportunity1.AssociatedTradeLanesPivots.Any(x => x.PK == pivot11.PK));
			AssertEquals(true, pivot11.IsDeleted);

			Assert(opportunity2.AssociatedTradeLanesPivots.Any(x => x.PK == pivot21.PK));
			AssertEquals(false, pivot21.IsDeleted);

			collection1.Remove(tradeLane2);

			collection1.Add(EntitySalesWrapper.Get(tradeLane3, opportunity1));
			Assert(opportunity1.AssociatedTradeLanesPivots.Any(x => x.SVP_TradeId == tradeLane3.PK));
		}

		public void TestLoad_OpportunityOrgChanged()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org1InSetupFactory = Factory.NewWithValidTestData<OrgHeader>();
			var org2InSetupFactory = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeadersInSetupFactory = (SalesHeaderCollection)org1InSetupFactory.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader1InSetupFactory = salesHeadersInSetupFactory.AddNew(product);
			var tradeLane1InSetupFactory = salesHeader1InSetupFactory.EntitySalesCollectionProductView.AddNew();

			var oppInSetupFactory = org1InSetupFactory.SalesOpportunities.AddNew();
			var pivot11InSetupFactory = tradeLane1InSetupFactory.SalesAssociationPivotCollectionGlobal.AddNew(oppInSetupFactory);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			{
				var opp = factory2.Load<OrgOpportunity>(oppInSetupFactory.PK);
				AssertEquals(1, ((SalesHeaderCollection)opp.ProspectiveSalesHeaderCollection).Count);
				opp.P8_OH = org2InSetupFactory.PK;
				AssertEquals("changing org for initialized collection does not lose headers", 1, ((SalesHeaderCollection)opp.ProspectiveSalesHeaderCollection).Count);
				AssertEquals("changing org for initialized collection does not lose sales", 1, ((SalesHeaderCollection)opp.ProspectiveSalesHeaderCollection)[0].EntitySalesCollectionProductView.Count);
			}

			var factory3 = new BusinessObjectFactory();
			{
				var opp = factory3.Load<OrgOpportunity>(oppInSetupFactory.PK);
				opp.P8_OH = org2InSetupFactory.PK;
				AssertEquals("changing org for non-initialized collection does not lose headers", 1, ((SalesHeaderCollection)opp.ProspectiveSalesHeaderCollection).Count);
				AssertEquals("changing org for non-initialized collection does not lose sales", 1, ((SalesHeaderCollection)opp.ProspectiveSalesHeaderCollection)[0].EntitySalesCollectionProductView.Count);
			}
		}

		#region Overrides

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			return salesHeaderCollection.EntitySalesCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<EntitySalesWrapper>();
		}

		#endregion
	}
}
