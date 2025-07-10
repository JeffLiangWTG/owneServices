using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using IInvoicesProvider = Enterprise.Customs.Business.IInvoicesProvider;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USInvoiceLineFilterBusinessObject))]
	sealed class USInvoiceLineFilterBusinessObjectTest : InvoiceLineFilterBusinessObjectTest
	{
		public new void TestTextFilter_Origin()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			var invoice2 = Declaration.Invoices.AddNew();
			var invoiceLine11 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine12 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine21 = invoice2.JobComInvoiceLines.AddNew();
			var invoiceLine22 = invoice2.JobComInvoiceLines.AddNew();
			AssertEquals(4, Declaration.FilteredInvoiceLines.Count);
			invoiceLine11.US_UC_NKCountryOfOrigin = "11";
			invoiceLine12.US_UC_NKCountryOfOrigin = "11";
			invoiceLine21.US_UC_NKCountryOfOrigin = "22";
			invoiceLine22.US_UC_NKCountryOfOrigin = "22";
			var filter = (ModuleTextFilter)FilterBO[InvoiceLineFilterConstants.Origin];
			filter.IsActive = true;
			var collectionFilter = Declaration.FilteredInvoiceLines;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = string.Empty;
			FilterBO.Search();
			AssertEquals(4, collectionFilter.Count);
			filter.Property = "11";
			FilterBO.Search();
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = string.Empty;
			FilterBO.Search();
			AssertEquals(4, collectionFilter.Count);
			filter.Property = "11";
			FilterBO.Search();
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = string.Empty;
			FilterBO.Search();
			AssertEquals(4, collectionFilter.Count);
			filter.Property = "11";
			FilterBO.Search();
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = string.Empty;
			FilterBO.Search();
			AssertEquals(4, collectionFilter.Count);
			filter.Property = "11";
			FilterBO.Search();
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = string.Empty;
			FilterBO.Search();
			AssertEquals(4, collectionFilter.Count);
			filter.Property = "11";
			FilterBO.Search();
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = string.Empty;
			FilterBO.Search();
			AssertEquals(4, collectionFilter.Count);
			filter.Property = "11";
			FilterBO.Search();
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			FilterBO.Search();
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			FilterBO.Search();
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
			// filter AND filter2
			var filterStrip = FilterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = filter.Description;
			var filter2 = (ModuleTextFilter)FilterBO[InvoiceLineFilterConstants.Origin + " (1)"];
			filter2.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			FilterBO.Search();
			AssertEquals(2, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
			// filter OR filter2
			filter.OrCategory = FilterOrCategory.Red;
			filter2.OrCategory = FilterOrCategory.Red;
			FilterBO.Search();
			AssertEquals(2, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
			// (filter OR filter2) AND filter3
			var filterStrip3 = FilterBO.FilterStrips.AddNew();
			filterStrip3.FilterDescription = filter.Description;
			var filter3 = (ModuleTextFilter)FilterBO[InvoiceLineFilterConstants.Origin + " (2)"];
			filter3.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter3.OrCategory = FilterOrCategory.None;
			FilterBO.Search();
			AssertEquals(3, FilterBO.ActiveModuleFilters.Count);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine11, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine12, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine21, collectionFilter);
			AssertCollectionNotContains(InvoiceLineFilterConstants.Origin, invoiceLine22, collectionFilter);
		}

		protected override InvoiceLineFilterBusinessObject CreateNewInvoiceLineFilterBusinessObject(Func<IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable) => new USInvoiceLineFilterBusinessObject(getInvoicesProvider, isColumnAvailable);

		protected override BaseJobDeclaration GetDeclaration() => Factory.New<JobDeclaration>();

		JobDeclaration Declaration => (JobDeclaration)declaration;

		USInvoiceLineFilterBusinessObject FilterBO => (USInvoiceLineFilterBusinessObject)filterBO;
	}
}
