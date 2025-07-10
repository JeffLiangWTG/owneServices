using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration.Rating;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.OrgTradeDetailLookups;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class GenerateQuoteForSalesValueAssociatedEntityControllerTest : TestCaseWithFactory
	{
		public void TestGetNewGenerateQuoteSettings()
		{
			var product = Factory.NewWithValidTestData<OrgSalesProduct>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			ZControllerInternals quoteController = ZControllerFactory.Create(ControllerIDs.Quotations);
			var quote1 = (IQuote)quoteController.GetNewBusinessEntityInFactory(Factory);
			var quote2 = (IQuote)quoteController.GetNewBusinessEntityInFactory(Factory);
			var quoteForAnotherCompany = (IQuote)quoteController.GetNewBusinessEntityInFactory(Factory);
			((BusinessObject)quoteForAnotherCompany)[RatingHeaderSchema.TH_GC] = Factory.New<GlbCompany>().PK;
			Factory.Save();

			var quote2b = GetQuoteAmendment(quote2);
			opportunity.RelatedChildActivityPivotCollection.AddNewPivot((IRelatableActivity)quote1);
			opportunity.RelatedChildActivityPivotCollection.AddNewPivot((IRelatableActivity)quote2);

			var salesHeaderCollection = (SalesHeaderCollection)opportunity.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var tradeLane1 = salesHeader.EntitySalesCollectionProductView.AddNew();
			var tradeDetail1a = tradeLane1.EntityTradeDetailsCollection.AddNew();
			var tradeDetail1b = tradeLane1.EntityTradeDetailsCollection.AddNew();
			var tradeLane2 = salesHeader.EntitySalesCollectionProductView.AddNew();
			var tradeDetail2 = tradeLane2.EntityTradeDetailsCollection.AddNew();

			var settings = GenerateQuoteForSalesValueAssociatedEntityController.GetNewGenerateQuoteSettings(opportunity, new[] { tradeDetail1a, tradeDetail1b, tradeDetail2 });
			AssertContainsExactElementsInAnyOrder(
					new[]
					{
						quote1,
						quote2,
						quote2b,
					},
					settings.QuoteSelectionItems.Cast<QuoteSelectionItem>().Select(x => x.Quote));

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
						tradeDetail1a,
						tradeDetail1b,
						tradeDetail2,
				},
				settings.TradeDetailSelectionItems.Cast<TradeDetailSelectionItem>().Select(x => x.TradeDetail));
		}

		public void TestGetNewQuote()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var quoteController = ZControllerFactory.Create(ControllerIDs.Quotations);
			var quote = (BusinessObject)((ZControllerInternals)quoteController).GetNewBusinessEntityInFactory(Factory);
			Factory.Save();

			var quoteSelectionItems = new QuoteSelectionItemCollection();
			var quoteSelectionItem = quoteSelectionItems.AddNew((IRelatableActivity)quote);
			var settings = new GenerateQuoteSettings(quoteSelectionItems, new TradeDetailSelectionItemCollection());
			settings.ShouldCreateAmendment = false;

			var newQuote = GenerateQuoteForSalesValueAssociatedEntityController.GetNewQuote(quoteController, opportunity, settings);
			AssertNotContains("", (ZString)quote[RatingHeaderSchema.Constants.TH_QuoteNumber], (ZString)((BusinessObject)newQuote)[RatingHeaderSchema.Constants.TH_QuoteNumber]);

			settings.ShouldCreateAmendment = true;
			quoteSelectionItem.Selected = true;
			AssertEquals(quote, settings.GetQuoteToAmend());
			newQuote = GenerateQuoteForSalesValueAssociatedEntityController.GetNewQuote(quoteController, opportunity, settings);
			AssertStartsWith("", (ZString)quote[RatingHeaderSchema.Constants.TH_QuoteNumber], (ZString)((BusinessObject)newQuote)[RatingHeaderSchema.Constants.TH_QuoteNumber]);
		}

		public void TestGetNewQuoteForOrganizations()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var quoteController = ZControllerFactory.Create(ControllerIDs.Quotations);
			var quote = (BusinessObject)((ZControllerInternals)quoteController).GetNewBusinessEntityInFactory(Factory);
			Factory.Save();

			var quoteSelectionItems = new QuoteSelectionItemCollection();
			var quoteSelectionItem = quoteSelectionItems.AddNew((IRelatableActivity)quote);
			var settings = new GenerateQuoteSettings(quoteSelectionItems, new TradeDetailSelectionItemCollection());
			settings.ShouldCreateAmendment = false;

			var newQuote = GenerateQuoteForSalesValueAssociatedEntityController.GetNewQuote(quoteController, org, settings);
			AssertEquals(org.PK, ((BusinessObject)newQuote)[RatingHeaderSchema.Constants.TH_OH]);
		}

		public void TestSyncNewSalesValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_Status = "WON";

			var collectionHelper = new OrgSalesCollectionTestHelper(org.SalesCollection, Factory);

			var newQuote = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			newQuote[RatingHeaderSchema.TH_OH.Name] = org.PK;
			collectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", Guid.Empty, Guid.Empty);
			collectionHelper.AddRateEntry(newQuote, "FCL", "AUMEL", "NZAKL", Guid.Empty, Guid.Empty);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orgInNewFactory = newFactory.Load<OrgHeader>(org.PK);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			GenerateQuoteForSalesValueAssociatedEntityController.SyncNewSalesValues(opportunity, newQuote, orgInNewFactory, () => { return true; });

			AssertEquals("New sales are created", 2, orgInNewFactory.SalesCollection.Count);
			var newSales1 = orgInNewFactory.SalesCollection.Cast<OrgSales>().Single(x => x.OriginCode == "AUSYD");
			var newDetail1 = newSales1.TradeDetails[0];
			var newSales2 = orgInNewFactory.SalesCollection.Cast<OrgSales>().Single(x => x.OriginCode == "AUMEL");
			var newDetail2 = newSales2.TradeDetails[0];
			AssertEquals("Associations are created for trade lane and detail", 4, opportunity.AssociatedTradeLanesPivots.Count);
			Assert("Association is created for trade lane", opportunity.AssociatedTradeLanesPivots.Any(x => x.SalesValue.Identifier == newSales1.PK));
			Assert("Association is created for trade lane", opportunity.AssociatedTradeLanesPivots.Any(x => x.SalesValue.Identifier == newSales2.PK));
			Assert("Association is created for trade detail", opportunity.AssociatedTradeLanesPivots.Any(x => x.SalesValue.Identifier == newDetail1.PK));
			Assert("Association is created for trade detail", opportunity.AssociatedTradeLanesPivots.Any(x => x.SalesValue.Identifier == newDetail2.PK));
			AssertEquals("Trade Detail Status should match opportunity", OpportunityTradeStatus.Codes.Successful, newDetail1.PA_Status);
			AssertEquals("Trade Detail Status should match opportunity", OpportunityTradeStatus.Codes.Successful, newDetail2.PA_Status);
		}

		public void TestSyncNewSalesValuesWhenJCDServiceTaskNotRun()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_Status = "WON";

			var collectionHelper = new OrgSalesCollectionTestHelper(org.SalesCollection, Factory);

			var newQuote = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			newQuote[RatingHeaderSchema.TH_OH.Name] = org.PK;
			collectionHelper.AddRateEntry(newQuote, "AIR", "AUSYD", "USLAX", Guid.Empty, Guid.Empty);
			collectionHelper.AddRateEntry(newQuote, "FCL", "AUMEL", "NZAKL", Guid.Empty, Guid.Empty);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orgInNewFactory = newFactory.Load<OrgHeader>(org.PK);
			GenerateQuoteForSalesValueAssociatedEntityController.SyncNewSalesValues(opportunity, newQuote, orgInNewFactory, () => { return true; });
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Job Costing Data Queue Service Task Incomplete", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Text", "Before you can synchronize sales values, you must finish processing all Transaction Lines through the Job Costing Data Queue Service Task (https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20180319d.pdf).", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestSyncNewSalesValues_ForExistingOrgSales_LinerAndAgency()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity1 = org.SalesOpportunities.AddNew();
			opportunity1.P8_Status = "WON";

			var opportunity2 = org.SalesOpportunities.AddNew();
			opportunity2.P8_Status = "WON";

			var collectionHelper = new OrgSalesCollectionTestHelper(org.SalesCollection, Factory);

			var newQuote = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			newQuote[RatingHeaderSchema.TH_OH.Name] = org.PK;
			collectionHelper.AddRateEntry(newQuote, "SCO", "AUSYD", "NZAKL", Guid.Empty, Guid.Empty);

			Factory.Save();

			var linerAgencyProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.LinerAgency);
			var sales = org.SalesCollection.AddNew();

			sales.OW_MP_Product = linerAgencyProduct.PK;
			sales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales.OW_OriginTableCode = "RL";
			sales.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "NZAKL", RefUNLOCOSchema.Constants.Prefix).PK;
			sales.OW_DestinationTableCode = "RL";

			var tradeDetail1 = sales.TradeDetails.AddNew();
			tradeDetail1.PA_TradeType = Constants.ContainerModes.FCL;
			tradeDetail1.PA_TradeMode = LinerAgencyTradeModes.BillOfLading;

			var tradeDetail2 = sales.TradeDetails.AddNew();
			tradeDetail2.PA_TradeType = Constants.ContainerModes.FCL;
			tradeDetail2.PA_TradeMode = LinerAgencyTradeModes.BillOfLading;

			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity1, sales);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity2, sales);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity1, tradeDetail1);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity2, tradeDetail2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orgInNewFactory = newFactory.Load<OrgHeader>(org.PK);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			GenerateQuoteForSalesValueAssociatedEntityController.SyncNewSalesValues(opportunity2, newQuote, orgInNewFactory, () => { Assert("Confirm Create Associations should not be shown", false); return true; });

			AssertEquals("Should only be one sales value", 1, orgInNewFactory.SalesCollection.Count);
		}

		public void TestSyncNewSalesValues_ForDuplicateOrgSales_LinerAndAgency()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity1 = org.SalesOpportunities.AddNew();
			opportunity1.P8_Status = "WON";

			var opportunity2 = org.SalesOpportunities.AddNew();
			opportunity2.P8_Status = "WON";

			var collectionHelper = new OrgSalesCollectionTestHelper(org.SalesCollection, Factory);

			var newQuote = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			newQuote[RatingHeaderSchema.TH_OH.Name] = org.PK;
			collectionHelper.AddRateEntry(newQuote, "SCO", "AUSYD", "NZAKL", Guid.Empty, Guid.Empty);

			Factory.Save();

			var linerAgencyProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.LinerAgency);
			var sales1 = org.SalesCollection.AddNew();

			sales1.OW_MP_Product = linerAgencyProduct.PK;
			sales1.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales1.OW_OriginTableCode = "RL";
			sales1.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "NZAKL", RefUNLOCOSchema.Constants.Prefix).PK;
			sales1.OW_DestinationTableCode = "RL";

			var tradeDetail1 = sales1.TradeDetails.AddNew();
			tradeDetail1.PA_TradeType = Constants.ContainerModes.FCL;
			tradeDetail1.PA_TradeMode = LinerAgencyTradeModes.BillOfLading;

			var sales2 = org.SalesCollection.AddNew();

			sales2.OW_MP_Product = linerAgencyProduct.PK;
			sales2.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			sales2.OW_OriginTableCode = "RL";
			sales2.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "NZAKL", RefUNLOCOSchema.Constants.Prefix).PK;
			sales2.OW_DestinationTableCode = "RL";

			var tradeDetail2 = sales2.TradeDetails.AddNew();
			tradeDetail2.PA_TradeType = Constants.ContainerModes.FCL;
			tradeDetail2.PA_TradeMode = LinerAgencyTradeModes.BillOfLading;

			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity1, sales1);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity2, sales2);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity1, tradeDetail1);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity2, tradeDetail2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orgInNewFactory = newFactory.Load<OrgHeader>(org.PK);
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.InitialiseAndRunJCDServiceTask(Db.Connection);

			GenerateQuoteForSalesValueAssociatedEntityController.SyncNewSalesValues(opportunity2, newQuote, orgInNewFactory, () => { Assert("Confirm Create Associations should not be shown", false); return true; });

			AssertEquals("Should only be two sales values", 2, orgInNewFactory.SalesCollection.Count);
		}

		static IQuote GetQuoteAmendment(IQuote quote)
		{
			quote.SameClientCopy = true;
			quote.AmendmentCopy = true;
			return (IQuote)((ITemplateCopyable)quote).TemplateCopy();
		}
	}
}
