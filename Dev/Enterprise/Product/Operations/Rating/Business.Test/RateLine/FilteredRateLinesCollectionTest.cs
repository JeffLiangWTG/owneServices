using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.DataMapping;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test
{
	[TestedType(typeof(FilteredRateLinesCollection))]
	public class FilteredRateLinesCollectionTest : BusinessObjectCollectionViewTestCase<FilteredRateLinesCollection>
	{
		/// <summary>
		/// This test is to simulate a very tricky case with RateEntry grid and RateLine grid in Forwarding tab and Summary tab.
		/// </summary>
		public void TestAddNewLineForQuotation()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			// New rate entry without any further entered data
			var airRateEntryCollection = quote.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var entry = ((IBindingList)airRateEntryCollection).AddNew();

			// New rate line in Forwarding tab but it also initializes FilteredRateLinesForBinding which is used in Summary tab.
			// Shortcut: add rate line to the collection.
			var collection = ((RateEntry)entry).FilteredRateLinesForBinding;
			AssertNoExceptionThrown(() => ((IBindingList)collection).AddNew());
		}

		public void TestIImportCollectionElementMatchingSupporter_Defaults()
		{
			var org = Helper.NewOrgHeader("test1");
			var costing = Helper.NewCosting(org);
			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.FCL, "AUSYD", "NZAKL", "FRT", 222.22);

			var matcher = rateEntry.FilteredRateLinesForBinding as IImportCollectionElementMatchingSupporter;
			AssertEquals(string.Empty, matcher.MatchingColumnName);
			AssertEquals(false, matcher.IsGenericColumnMatchingAllowed);
			AssertNull(matcher.GetMatchingBizObject("whatever"));
			AssertNull(matcher.GetMatchingBizObject(string.Empty));
		}

		#region Implementations

		protected override FilteredRateLinesCollection GetCollectionToTest()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry("AIR");
			return new FilteredRateLinesCollection(entry.RateLines, new ZQuery());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> Collection.AddNew();

		protected TestHelper Helper
			=> helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		protected override void SetUp()
		{
			base.SetUp();
			// Prevent default rate lines being added
			RatingDataRegistry.Instance.AIRFreightDefaultCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
		}

		#endregion
	}
}
