using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingWhsReceiveColumnProvider))]
	[HttpContextEnabledTest]
	sealed class TrackingWhsReceiveColumnProviderTest : GridColumnProviderTest
	{
		public override void TestFixOldLayout()
		{
			string cachedRegistryValue = WebDataRegistry.Instance.MilestoneVisibility.Value;
			base.TestFixOldLayout();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryValue);
		}

		protected override void BeforeLayoutsWithFewDynamicColumns()
		{
			base.BeforeLayoutsWithFewDynamicColumns();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly);
		}

		protected override void BeforeLayoutsWithAllDynamicColumns()
		{
			base.BeforeLayoutsWithAllDynamicColumns();
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.All);
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.FinalisedDate],
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.ReceiveNumber],
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.Status],
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.TotalPallets]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.FinalisedDate],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.TotalUnits],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.Status],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.ReceiveNumber],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDescription]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixFewDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.FinalisedDate],
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.Warehouse],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.BookingDate],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingWarehouseReceives.ReceiveNumber]
			};
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			ZHyperLinkColumn refNumberColumn = new ZHyperLinkColumn("Receive Ref. #", TrackingWhsReceive.WrapperSchema.WD_ExternalReference)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.ReceiveNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.WarehouseReceiveDetailsPage + "?Ref={0}",
				DataNavigateUrlFields = new string[1] { TrackingWhsReceive.WrapperSchema.WD_PK }
			};
			AddRequiredColumn(refNumberColumn);

			ZFindBoxColumn warehouseColumn = new ZFindBoxColumn("Warehouse", TrackingWhsReceive.WrapperSchema.WD_WW_Whs, "Lookups.Warehouses", typeof(TrackingWhsOrder)) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.Warehouse };
			warehouseColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddDefaultsColumn(warehouseColumn);

			AddDefaultsColumn(new ZTextEditColumn("Docket #", TrackingWhsReceive.WrapperSchema.WD_DocketID) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.DocketNumber });
			AddDefaultsColumn(new ZDateTimeColumn("Booking Date", TrackingWhsReceive.WrapperSchema.WD_BookingDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.BookingDate });
			AddDefaultsColumn(new ZDateTimeColumn("ETA", TrackingWhsReceive.WrapperSchema.WD_ETA, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.ETA });
			AddDefaultsColumn(new ZDateTimeColumn("Arrival Date", TrackingWhsReceive.WrapperSchema.WD_ArrivalDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.ArrivalDate });
			AddDefaultsColumn(new ZTextEditColumn("Status", TrackingWhsReceive.WrapperSchema.WD_DocketStatusDescription) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.Status });

			AddDefaultsColumn(new ZTextEditColumn("Total Units", TrackingWhsReceive.WrapperSchema.WD_TotalUnits) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.TotalUnits });
			AddDefaultsColumn(new ZTextEditColumn("Total Pallets", TrackingWhsReceive.WrapperSchema.WD_TotalPallets) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.TotalPallets });
			AddDefaultsColumn(new ZDateTimeColumn("Finalized Date", TrackingWhsReceive.WrapperSchema.WD_FinalisedDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.FinalisedDate });
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns("Milestones")) // Data column name
			{
				if (column.HeaderText == "Last Milestone Desc.")
				{
					AddDefaultsColumn(column);
				}
				else
				{
					AddColumn(column);
				}
			}

			if (WebEnv.AppInstance?.SiteUser is OrgContactWebUser siteUser)
			{
				var customColumnKey = 1500;

#pragma warning disable IDE0001 // Simplify Names - Reason = All logic in this file has its scope on WhsReceive
				foreach (CustomLabelInfo field in new WhsReceive.CustomLabelsProvider(null).GetCustomFields(siteUser.LoggedInOrganisation, Factory))
#pragma warning restore IDE0001 // Simplify Names
				{
					if (field.LabelName.StartsWith("WhsDocket.") && field.IsEnabled)
					{
						var specifiedBindTo = TrackingWhsOrder.GetSchemaPath(field.PropertyName);
						var column = ZTemplateColumn.GetNew(field.Caption, field.PropertyType, specifiedBindTo);
						column.ColumnKey = customColumnKey;
						AddColumn(column);
						customColumnKey++;
					}
				}
			}

			AddColumn(new ZTextEditColumn("Customer Ref.", TrackingWhsReceive.WrapperSchema.WD_CustomerReference) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.CustomerReference });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.TrackingWarehouseReceives.Warehouse
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingWhsReceiveColumnProvider();
		}
	}
}
