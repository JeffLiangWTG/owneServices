using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	public class EntryHeaderFilterUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestNewZFilterStrip()
		{
			using (var filterControl = GetNewFilterControl())
			{
				using (var filterStrip = filterControl.NewZFilterStrip())
				{
					AssertType<EntryHeaderModuleStrip>(filterStrip);
				}
			}
		}

		[RequiresSTA]
		public virtual void TestFilteredGridColumnsCaption()
		{
			using (var filterStrip = GetNewEntryHeaderFilterUserControl())
			{
				var grid = filterStrip.FilteredGrid;
				var entryNumber = (ZArchitecture.ZTextBoxColumnStyleInfo)grid.GetColumnStyle("EntryNumber");
				var referenceNumber = (ZArchitecture.ZTextBoxColumnStyleInfo)grid.GetColumnStyle("CH_BGMReference");
				AssertEquals("Entry Number column name should be Entry Number, override ExpectedEntryNumberCaption if name has changed", ExpectedEntryNumberCaption, entryNumber.CaptionResourceString.Caption);
				AssertEquals("Reference Number", referenceNumber.CaptionResourceString.Caption);
				var whsTransStatus = grid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatus);
				var whsTransStatusDes = grid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription);
				AssertEquals("WHS Trans. Status", whsTransStatus.CaptionResourceString.Caption);
				AssertEquals("WHS Trans. Status Des.", whsTransStatusDes.CaptionResourceString.Caption);
				AssertEquals("Grouped columns", whsTransStatus.GroupName, whsTransStatusDes.GroupName);
				var hasManualWhsDone = grid.GetColumnStyle(CusEntryHeader.Schema.CH_HasManualWhsUpdate);
				AssertEquals("Manual Warehouse Update done", "Manual Warehouse Update done", hasManualWhsDone.CaptionResourceString.Caption);
			}
		}

		protected virtual string ExpectedEntryNumberCaption => "Entry Number";

		protected virtual EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			var cusEntryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(jobDeclaration, Factory);
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			return new EntryHeaderFilterUserControl(cusEntryHeaders, filterBusinessObject);
		}

		[RequiresSTA]
		public void TestFilteredGridColumns()
		{
			using (var form = new ZForm())
			using (var filterStrip = GetNewEntryHeaderFilterUserControl())
			{
				form.Controls.Add(filterStrip);
				form.Show();
				var filteredGrid = filterStrip.FilteredGrid;
				var filteredGridColumns = FilteredGridColumns;
				AssertEquals(filteredGridColumns.Count, filteredGrid.Columns.Count);
				CombineAssertions(() =>
				{
					foreach (var column in filteredGridColumns)
					{
						AssertNotNull("Column [" + column + "] should NOT be null.", filteredGrid.Columns[column]);
					}
				});
			}
		}

		protected virtual List<string> FilteredGridColumns => new List<string>()
		{
			CusEntryHeader.Schema.EntryNumber,
			CusEntryHeader.Schema.DeclarationReference,
			CusEntryHeader.Schema.CH_BGMReference,
			CusEntryHeader.Schema.CH_EntryStatus,
			CusEntryHeader.Schema.EntryHeaderStatusDescription,
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
			EntryHeaderFilterUserControl.Schema.ExportDate,
			CusEntryHeader.Schema.CH_TotalPaid,
			CusEntryHeader.Schema.CH_WarehouseTransactionStatus,
			CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription,
			CusEntryHeader.Schema.CH_HasManualWhsUpdate,
			EntryHeaderFilterUserControl.Schema.CustomsAgentCode,
			EntryHeaderFilterUserControl.Schema.CustomsAgentName,
			EntryHeaderFilterUserControl.Schema.ControllingAgentCode,
			EntryHeaderFilterUserControl.Schema.ControllingAgentName,
			EntryHeaderFilterUserControl.Schema.ControllingCustomerCode,
			EntryHeaderFilterUserControl.Schema.ControllingCustomerName,
			EntryHeaderFilterUserControl.Schema.OwnerReference,
			EntryHeaderFilterUserControl.Schema.MasterBill,
			EntryHeaderFilterUserControl.Schema.HouseBill,
			EntryHeaderFilterUserControl.Schema.Vessel,
			EntryHeaderFilterUserControl.Schema.VoyageFlightNo,
			EntryHeaderFilterUserControl.Schema.OriginETD,
			EntryHeaderFilterUserControl.Schema.FinalDestinationETA,
			EntryHeaderFilterUserControl.Schema.Origin,
			EntryHeaderFilterUserControl.Schema.Destination,
			EntryHeaderFilterUserControl.Schema.Loading,
			EntryHeaderFilterUserControl.Schema.Discharge,
			EntryHeaderFilterUserControl.Schema.ShipmentType,
			EntryHeaderFilterUserControl.Schema.DeclarantCode,
			EntryHeaderFilterUserControl.Schema.DeclarantName,
			EntryHeaderFilterUserControl.Schema.TransportMode,
			EntryHeaderFilterUserControl.Schema.DeclarationType,

			CusEntryHeader.Schema.CH_SystemCreateTimeUtc,
			CusEntryHeader.Schema.CH_SystemCreateUser,
			CusEntryHeader.Schema.CH_SystemLastEditTimeUtc,
			CusEntryHeader.Schema.CH_SystemLastEditUser,
		};

		[RequiresSTA]
		public void TestColumnNamesInSortOrder()
		{
			using (var form = new ZForm())
			using (var filterStrip = GetNewEntryHeaderFilterUserControl())
			{
				CombineAssertions(() =>
				{
					form.Controls.Add(filterStrip);
					form.Show();
					var grid = filterStrip.FilteredGrid;
					var columnNamesInSortOrder = ColumnNamesInSortOrder;
					var visibleColumns = grid.Columns.Where(x => x.IsVisible).Select(x => x.ColumnName).ToArray();

					foreach (var column in columnNamesInSortOrder)
					{
						AssertNotNull($"Column [{column}] should NOT be null.", grid.Columns[column]);
						Assert($"It's meaningless to sort invisible column, please remove Column [{column}] from ColumnNamesInSortOrder, or make it visible.", grid.Columns[column].IsVisible);
					}
					AssertContainsExactElementsInExactOrder("Column Names In Sort Order", columnNamesInSortOrder, visibleColumns);
				});
			}
		}

		protected virtual List<string> ColumnNamesInSortOrder => new List<string>()
		{
			CusEntryHeader.Schema.EntryNumber,
			CusEntryHeader.Schema.DeclarationReference,
			CusEntryHeader.Schema.CH_BGMReference,
			CusEntryHeader.Schema.CH_EntryStatus,
			CusEntryHeader.Schema.EntryHeaderStatusDescription,
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
			CusEntryHeader.Schema.CH_WarehouseTransactionStatus,
			CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription,
		};

		[RequiresSTA]
		public void TestColumnVisibility()
		{
			using (var form = new ZForm())
			using (var filterStrip = GetNewEntryHeaderFilterUserControl())
			{
				form.Controls.Add(filterStrip);
				form.Show();
				var filteredGrid = filterStrip.FilteredGrid;
				var hiddenFieldsExpected = InVisibleFilteredGridColumns;
				var hiddenFieldsActual = filteredGrid.Columns.Where(c => !c.IsVisible);
				AssertEquals("Count of the actually hidden fields should equal to count Expects.", hiddenFieldsExpected.Count, hiddenFieldsActual.Count());
				CombineAssertions(() =>
				{
					foreach (var column in hiddenFieldsExpected)
					{
						Assert("Column [" + column + "] should be hidden.", !filteredGrid.Columns[column].IsVisible);
					}
				});
			}
		}

		protected virtual List<string> InVisibleFilteredGridColumns => new List<string>()
		{
			EntryHeaderFilterUserControl.Schema.ExportDate,
			EntryHeaderFilterUserControl.Schema.CustomsAgentCode,
			EntryHeaderFilterUserControl.Schema.CustomsAgentName,
			EntryHeaderFilterUserControl.Schema.ControllingAgentCode,
			EntryHeaderFilterUserControl.Schema.ControllingAgentName,
			EntryHeaderFilterUserControl.Schema.ControllingCustomerCode,
			EntryHeaderFilterUserControl.Schema.ControllingCustomerName,
			EntryHeaderFilterUserControl.Schema.OwnerReference,
			EntryHeaderFilterUserControl.Schema.MasterBill,
			EntryHeaderFilterUserControl.Schema.HouseBill,
			EntryHeaderFilterUserControl.Schema.Vessel,
			EntryHeaderFilterUserControl.Schema.VoyageFlightNo,
			EntryHeaderFilterUserControl.Schema.OriginETD,
			EntryHeaderFilterUserControl.Schema.FinalDestinationETA,
			EntryHeaderFilterUserControl.Schema.Origin,
			EntryHeaderFilterUserControl.Schema.Destination,
			EntryHeaderFilterUserControl.Schema.Loading,
			EntryHeaderFilterUserControl.Schema.Discharge,
			EntryHeaderFilterUserControl.Schema.ShipmentType,
			EntryHeaderFilterUserControl.Schema.DeclarantCode,
			EntryHeaderFilterUserControl.Schema.DeclarantName,
			EntryHeaderFilterUserControl.Schema.TransportMode,
			EntryHeaderFilterUserControl.Schema.DeclarationType,

			CusEntryHeader.Schema.CH_HasManualWhsUpdate,
			CusEntryHeader.Schema.CH_SystemCreateTimeUtc,
			CusEntryHeader.Schema.CH_SystemCreateUser,
			CusEntryHeader.Schema.CH_SystemLastEditTimeUtc,
			CusEntryHeader.Schema.CH_SystemLastEditUser,
		};

		EntryHeaderFilterUserControlForTest GetNewFilterControl()
		{
			var collection = new CusAuthorisationHeaderCollection(Factory);
			var filterBizO = new CusAuthorisationsFilterStripBusinessObject();
			return new EntryHeaderFilterUserControlForTest(collection, filterBizO);
		}
	}

	sealed class EntryHeaderFilterUserControlForTest : EntryHeaderFilterUserControl
	{
		public EntryHeaderFilterUserControlForTest(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
		{
		}

		public new ZFilterStrip NewZFilterStrip() => base.NewZFilterStrip();
	}
}
