using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business.Quotations;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingQuotationsModule))]
	sealed class TrackingQuotationsModuleTest : ZFilterStripGridModuleTestCase
	{
		#region Overrides

		protected override BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var result = base.GetNewElement(elementType, isCancelled);
			var quote = result as Quote;
			if (quote != null)
			{
				quote.TH_QuoteNumber = DateTime.Now.Ticks.ToString();
				quote.TH_OneTimeQuote = true;
				quote.QuotationClientAddress.E2_OA_Address = SiteUser.LoggedInOrganisation.MainAddress.PK;
			}

			return result;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<Quote>();
				testObject.TH_QuoteNumber = "Include" + i;
				testObject.TH_OneTimeQuote = true;
				testObject.QuotationClientAddress.E2_OA_Address = SiteUser.LoggedInOrganisation.MainAddress.PK;
				result.Add(testObject);
			}

			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<Quote>();
				testObject.TH_QuoteNumber = "Other" + i;
				result.Add(testObject);
			}

			return result;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(RatingHeaderSchema.TH_QuoteNumber, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			var result = base.GetExpectedAuditFilters();
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");

			return result;
		}

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutForwardingQuotes;

		protected override WebModuleID TestID => WebModuleIDs.TrackingQuotations;

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(RatingHeaderSchema.TH_QuoteDate.Name, ListSortDirection.Ascending) };

		protected override ZWebModule GetNewZWebModule() => new TrackingQuotationsModuleForTest(Factory, TestPage);

		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var i = 0;

				return new[]
				{
					new ColumnDetailsForTest("Quote #", i++, typeof(ZLinkButtonColumn)),
					new ColumnDetailsForTest("Company", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Status", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Quote Date", i++, typeof(ZDateTimeColumn)),
					new ColumnDetailsForTest("Expiry Date", i++, typeof(ZDateTimeColumn)),
					new ColumnDetailsForTest("Transport Mode", i++, typeof(ZDropDownListColumn)),
					new ColumnDetailsForTest("Origin", i++, typeof(ZCodeFindBoxColumn)),
					new ColumnDetailsForTest("Destination", i++, typeof(ZCodeFindBoxColumn)),
					new ColumnDetailsForTest("Volume", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Vol. Units", i++, typeof(ZTextEditColumn)),
					new ColumnDetailsForTest("Weight", i++, typeof(ZCalcEditColumn)),
					new ColumnDetailsForTest("Weight Units", i++, typeof(ZTextEditColumn)),
				};
			}
		}

		protected override DataGridColumn[] ExpectedRequiredGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingQuotationsModule;

				return new[] { module.AllColumns["Quote #"] };
			}
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var module = FilterGridModule as TrackingQuotationsModule;

				return new[]
				{
					module.AllColumns["Company"],
					module.AllColumns["Status"],
					module.AllColumns["Quote Date"],
					module.AllColumns["Expiry Date"],
					module.AllColumns["Origin"],
					module.AllColumns["Destination"]
				};
			}
		}

		#endregion

		public void TestLoadCollectionWithExpiryDateFilter()
		{
			var filter = FilterGridModule.CreateNewFilterBusinessObject();

			var futureQuote = Factory.NewWithValidTestData<Quote>();
			futureQuote.TH_OneTimeQuote = ZBool.True;

			var expiredQuote = Factory.NewWithValidTestData<Quote>();
			expiredQuote.TH_OneTimeQuote = ZBool.True;
			expiredQuote.TH_QuoteEndDate = ZDate.Today.AddDays(-1);
			expiredQuote.TH_QuoteDate = expiredQuote.TH_QuoteEndDate.AddDays(-1);

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = ((OrgContactWebUser)TestPage.SiteUser).LoggedInUser.OC_OH;
			orgAddress.OA_Code = "Quote Address";

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = orgAddress.PK;
			jobDocAddress.E2_ParentID = futureQuote.PK;
			jobDocAddress.E2_ParentTableCode = futureQuote.TablePrefix;
			jobDocAddress.E2_AddressType = MasterFiles.Integration.AutoDocAddressTypes.Codes.QuotationClientAddress;
			jobDocAddress.E2_AddressSequence = 1;

			var newJobDocAddres = (JobDocAddress)jobDocAddress.Clone();
			newJobDocAddres.E2_ParentID = expiredQuote.PK;
			newJobDocAddres.E2_ParentTableCode = expiredQuote.TablePrefix;

			Factory.Save();

			FilterGridModule.LoadCollection(filter);
			AssertEquals("Expected future quote to be in the collection", 1, FilterGridModule.GridCollection.Count);
			AssertEquals("Expected future quote", ((TrackingQuoteCollection)FilterGridModule.GridCollection)[0].PK, futureQuote.PK);

			var expiryDateFilter = (ModuleDateFilter)((OneOffQuoteFilterStripBusinessObject)filter)["Expiry Date"];
			expiryDateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Yesterday;
			expiryDateFilter.IsActive = true;

			FilterGridModule.LoadCollection(filter);
			AssertEquals("Expected expired quote to be in the collection", 1, FilterGridModule.GridCollection.Count);
			AssertEquals("Expected expired quote", ((TrackingQuoteCollection)FilterGridModule.GridCollection)[0].PK, expiredQuote.PK);
		}

		protected override ZWebTestHelper GetNewHelper() => new TestHelper(Factory);
	}
}
