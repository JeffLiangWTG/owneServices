using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	sealed class EntryHeaderFilterUserControlTest : Customs.Module.Testing.EntryHeaderFilterUserControlTest
	{
		[RequiresSTA]
		public override void TestFilteredGridColumnsCaption()
		{
			using (var filterStrip = GetNewEntryHeaderFilterUserControl())
			{
				var grid = filterStrip.FilteredGrid;
				var entryNumber = (ZArchitecture.ZTextBoxColumnStyleInfo)grid.GetColumnStyle("EntryNumber");
				var referenceNumber = (ZArchitecture.ZTextBoxColumnStyleInfo)grid.GetColumnStyle("CH_BGMReference");
				AssertEquals("Entry Number (MRN)", entryNumber.CaptionResourceString.Caption);
				AssertEquals("Reference Number (LRN)", referenceNumber.CaptionResourceString.Caption);
			}
		}

		public void TestContextMenu()
		{
			using (var control = new EntryHeaderFilterUserControl())
			{
				Assert(ZString.Format("Contains the new Menu Item '{0}'", EntryHeaderFilterUserControl.Constants.RequestCustomsResendOfResponses), control.FilteredGrid.ContextMenu.MenuItems.FindByText(EntryHeaderFilterUserControl.Constants.RequestCustomsResendOfResponses) != null);
			}
		}

		public void TestAcquittedAndAcquitByDateProperties()
		{
			using (var form = new ZForm())
			using (var filterStrip = GetNewEntryHeaderFilterUserControl())
			{
				CombineAssertions(() =>
				{
					form.Controls.Add(filterStrip);
					form.Show();
					var grid = filterStrip.FilteredGrid;
					var acquitBy = (ZArchitecture.ZDateEditColumnStyleInfo)grid.GetColumnStyle("CH_BondValidToDate");
					var acquitted = (ZArchitecture.ZDateEditColumnStyleInfo)grid.GetColumnStyle("CH_BondAcquittedDate");
					AssertEquals(ZArchitecture.Core.ZDateTimePickerFormat.Short, acquitBy.DateTimeFormat);
					AssertEquals(ZArchitecture.Core.ZDateTimePickerFormat.Short, acquitted.DateTimeFormat);
					AssertEquals(true, acquitBy.IsVisible);
					AssertEquals(true, acquitted.IsVisible);
				});
			}
		}

		protected override Customs.Module.EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeaders = new CusEntryHeaderCollection(jobDeclaration, Factory);
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			return new EntryHeaderFilterUserControl(cusEntryHeaders, filterBusinessObject);
		}

		protected override List<string> FilteredGridColumns
		{
			get
			{
				var list = new List<string>()
					{
						CusEntryHeader.Schema.ValueAddedTax,
						CusEntryHeader.Schema.CustomsDuty,
						CusEntryHeader.Schema.CH_PaymentMethod,
						CusEntryHeader.Schema.CH_Packages,
						CusEntryHeader.Schema.CustomsProcedureCode,
						CusEntryHeader.Schema.CustomsProcedureInstructionDescription,
						CusEntryHeader.Schema.EntryInstructionAssessmentDate,
						CusEntryHeader.Schema.UniqueConsignmentReference,
						CusEntryHeader.Schema.CombinedUCREntryNumbers,
						CusEntryHeader.Schema.CH_RelPrintInd,
						CusEntryHeader.Schema.CH_BondValidToDate,
						CusEntryHeader.Schema.CH_BondAcquittedDate,
					};
				list.AddRange(base.FilteredGridColumns);
				return list;
			}
		}

		protected override List<string> ColumnNamesInSortOrder => new List<string>()
					{
						CusEntryHeader.Schema.EntryNumber,
						CusEntryHeader.Schema.DeclarationReference,
						CusEntryHeader.Schema.CH_BGMReference,
						CusEntryHeader.Schema.CH_EntryStatus,
						CusEntryHeader.Schema.EntryHeaderStatusDescription,
						CusEntryHeader.Schema.CustomsProcedureCode,
						CusEntryHeader.Schema.CH_EntrySubmittedDate,
						CusEntryHeader.Schema.CH_EntryReleaseDate,
						CusEntryHeader.Schema.CH_Status,
						CusEntryHeader.Schema.MessageStatusDescription,
						CusEntryHeader.Schema.CH_MessageType,
						CusEntryHeader.Schema.CH_MessageTypeDescription,
						EntryHeaderFilterUserControl.Schema.BranchName,
						EntryHeaderFilterUserControl.Schema.ImporterName,
						EntryHeaderFilterUserControl.Schema.SupplierName,
						EntryHeaderFilterUserControl.Schema.AgentsReference,
						EntryHeaderFilterUserControl.Schema.DateOfArrival,
						CusEntryHeader.Schema.CH_TotalPaid,
						CusEntryHeader.Schema.ValueAddedTax,
						CusEntryHeader.Schema.CustomsDuty,
						CusEntryHeader.Schema.CH_PaymentMethod,
						CusEntryHeader.Schema.CH_Packages,
						CusEntryHeader.Schema.CustomsProcedureInstructionDescription,
						CusEntryHeader.Schema.EntryInstructionAssessmentDate,
						CusEntryHeader.Schema.UniqueConsignmentReference,
						CusEntryHeader.Schema.CH_BondValidToDate,
						CusEntryHeader.Schema.CH_BondAcquittedDate,
						CusEntryHeader.Schema.CH_WarehouseTransactionStatus,
						CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription,
						CusEntryHeader.Schema.CH_RelPrintInd
					};

		protected override List<string> InVisibleFilteredGridColumns
		{
			get
			{
				var list = base.InVisibleFilteredGridColumns;
				list.Add(CusEntryHeader.Schema.CombinedUCREntryNumbers);
				return list;
			}
		}
	}
}
