using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingAccountsModule))]
	sealed class TrackingAccountsModule_Test : ZFilterGridModuleTest
	{
		protected override bool AllowActiveStatusFilterTest() => false;

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection) => false;

		#endregion

		#region Setup

		protected override WebModuleID TestID => WebModuleIDs.TrackingAccounts;

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault() => Array.Empty<FilterBusinessObjectDefault>();

		protected override ZWebTestHelper GetNewHelper() => new TestHelper(Factory);

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var header = (AccTransactionHeader)base.CreateNewElementForExcelExport();
			header.AH_OH = TestHelper.TestOrg.PK;
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_FullyPaidDate = ZDateTime.Empty;
			header.AH_TransactionType = TransactionTypes.CreditNote;
			header.AH_IsCancelled = false;

			return header;
		}

		protected override FilterBusinessObject GetFilterObjectForExcelExport()
		{
			var filterBO = (TrackingTransactionFilterBusinessObject)base.GetFilterObjectForExcelExport();
			filterBO.LoggedInUser = TestHelper.TestContact;

			return filterBO;
		}

		protected override Type GetElementTypeForExcelExport() => typeof(AccTransactionHeader);

		protected override void FillCollectionWithAtLeastOneElement() => FilterGridModule.GridCollection.Add(Factory.New<APInvoice>());

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var baseDefaultColumns = base.ExpectedDefaultGridColumns;
				var defaultColumns = new DataGridColumn[baseDefaultColumns.Length - 3];
				for (var i = 0; i < defaultColumns.Length; i++)
				{
					defaultColumns[i] = baseDefaultColumns[i];
				}
				return defaultColumns;
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(AccTransactionHeaderSchema.AH_InvoiceDate.Name, ListSortDirection.Ascending) };

		#endregion
	}
}
