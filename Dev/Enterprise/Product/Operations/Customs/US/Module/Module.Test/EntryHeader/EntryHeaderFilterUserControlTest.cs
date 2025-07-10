using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class EntryHeaderFilterUserControlTest : Customs.Module.Testing.EntryHeaderFilterUserControlTest
	{
		public void TestFilteredGridColumnsVisibility()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.EntryHeader))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entries = new CusEntryHeaderCollection(declaration, Factory);
				var filterBusinessObject = new EntryHeaderFilterBusinessObject();

				using (var form = new ZForm())
				{
					using (var filter = new EntryHeaderFilterUserControl(entries, filterBusinessObject))
					{
						form.Controls.Add(filter);
						form.Show();
						var filteredGrid = filter.FilteredGrid;

						AssertEquals(true, filteredGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BGMReference).IsUnavailable);
						AssertEquals("Entry Number", filteredGrid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber).CaptionResourceString.Caption);
						AssertEquals("JE_EntryAuthorisationDate", filteredGrid.GetColumnStyle(CusEntryHeader.Schema.JE_EntryAuthorisationDate).ColumnName);

						var columnStyles = filteredGrid.ColumnStyles;
						var length = ColumnNamesInSortOrder.ToArray().Length;
						CombineAssertions(() =>
						{
							for (var i = 0; i < length; i++)
							{
								var columnInfo = columnStyles[i] as ZGridColumnInfo;
								AssertNotNull(columnInfo);
								var expectedColumnName = ColumnNamesInSortOrder[i];
								AssertEquals(i.ToString() + " Expected", expectedColumnName, columnInfo.ColumnName);
								AssertEquals(true, columnInfo.IsVisible);
							}
						});
					}
				}
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
						"US_SuretyCode",
						"Declaration+US_OtherReconIndicator",
						"NAFTAReconciliation",
						"JE_DateOfArrival",
						"US_SchDEntry",
						"IOROrgPK",
						"EntryFilerCode",
						"EntryType",
						"US_PaymentType",
						"US_PreliminaryStatementPrintDate",
						"JE_EntryAuthorisationDate",
					};
				list.AddRange(base.FilteredGridColumns);
				list.Remove(CusEntryHeader.Schema.CH_EntryReleaseDate);
				foreach (var column in UnAvailableColumnNames)
				{
					list.Remove(column);
				}
				return list;
			}
		}

		protected override List<string> ColumnNamesInSortOrder => new List<string>()
		{
			CusEntryHeader.Schema.EntryFilerCode,
			CusEntryHeader.Schema.EntryNumber,
			CusEntryHeader.Schema.DeclarationReference,
			CusEntryHeader.Schema.CH_EntryStatus,
			CusEntryHeader.Schema.JE_EntryAuthorisationDate,
			CusEntryHeader.Schema.US_SchDEntry,
			EntryHeaderFilterUserControl.Schema.DateOfArrival,
			CusEntryHeader.Schema.EntryType,
			EntryHeaderFilterUserControl.Schema.BranchName,
			EntryHeaderFilterUserControl.Schema.ImporterName,
			EntryHeaderFilterUserControl.Schema.SupplierName,
			CusEntryHeader.Schema.CH_Status,
			CusEntryHeader.Schema.MessageStatusDescription,
			CusEntryHeader.Schema.CH_MessageType,
		};

		protected override List<string> InVisibleFilteredGridColumns
		{
			get
			{
				var list = FilteredGridColumns;
				foreach (var column in ColumnNamesInSortOrder)
				{
					list.Remove(column);
				}
				return list;
			}
		}

		List<string> UnAvailableColumnNames => new List<string>()
		{
			CusEntryHeader.Schema.CH_BGMReference,
			CusEntryHeader.Schema.CH_WarehouseTransactionStatus,
			CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription,
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
		};
	}
}
