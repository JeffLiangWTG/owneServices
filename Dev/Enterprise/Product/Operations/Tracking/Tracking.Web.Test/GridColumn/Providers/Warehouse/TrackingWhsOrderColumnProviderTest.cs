using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
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
	[TestedType(typeof(TrackingWhsOrderColumnProvider))]
	[HttpContextEnabledTest]
	sealed class TrackingWhsOrderColumnProviderTest : GridColumnProviderTest
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
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.CustomerReference],
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.OrderNumber],
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.Status],
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.TransportReference]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.CustomerReference],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.OrderNumber],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.FinalisedDate],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.RequiredDate],
				TestProvider[WebTracker.Grids.Milestones.NextMilestoneDescription]
			};
		}

		protected override DataGridColumn[] GetColumnsForLayoutFixFewDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.CustomerReference],
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.RequiredDate],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDate],
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.OrderNumber],
				TestProvider[WebTracker.Grids.Milestones.LastMilestoneDescription],
				TestProvider[WebTracker.Grids.TrackingWarehouseOrders.Units]
			};
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();

			ZHyperLinkColumn orderNumberColumn = new ZHyperLinkColumn("Order#", TrackingWhsOrder.WrapperSchema.WD_ExternalReference)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.OrderNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.WarehouseOrderDetailsPage + "?Ref={0}", // Partial URL
				DataNavigateUrlFields = new string[1] { TrackingWhsOrder.WrapperSchema.WD_PK }
			};
			AddRequiredColumn(orderNumberColumn);

			ZTextEditColumn warehouseColumn = new ZTextEditColumn("Warehouse", TrackingWhsOrder.GetSchemaPath("Warehouse+WW_WarehouseName"))
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Warehouse
			};
			AddDefaultsColumn(warehouseColumn);

			ZTextEditColumn consigneeColumn = new ZTextEditColumn("Consignee", TrackingWhsOrder.GetSchemaPath("ConsigneeDocAddress+E2_CompanyName"))
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Consignee
			};
			AddDefaultsColumn(consigneeColumn);

			ZTextEditColumn transportCoColumn = new ZTextEditColumn("Transport Co.", TrackingWhsOrder.GetSchemaPath("TransportCoDocAddress+E2_CompanyName"))
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.TransportCompany
			};
			AddDefaultsColumn(transportCoColumn);

			ZHyperLinkColumn transportRefColumn = new ZHyperLinkColumn("Transport Ref.", TrackingWhsOrder.WrapperSchema.WD_TransportReference)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.TransportReference,
				DataNavigateUrlFormatString = "{0}",
				DataNavigateUrlFields = new string[1] { TrackingWhsOrder.WrapperSchema.WD_TransportCoUrl },
				IsExternalHyperlink = true,
				Target = "_blank" // programmatic constant
			};
			AddDefaultsColumn(transportRefColumn);

			AddDefaultsColumn(new ZTextEditColumn("Docket#", TrackingWhsOrder.WrapperSchema.WD_DocketID)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.DocketNumber
			});
			AddDefaultsColumn(new ZDateTimeColumn("Req. Date", TrackingWhsOrder.WrapperSchema.TrackingRequiredDate, ZDateTimePickerFormat.Short)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.RequiredDate
			});
			AddDefaultsColumn(new ZTextEditColumn("Status", TrackingWhsOrder.WrapperSchema.WD_DocketStatusDescription)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Status
			});
			AddColumn(new ZTextEditColumn("Units", TrackingWhsOrder.WrapperSchema.WD_TotalUnits)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Units
			});
			AddColumn(new ZTextEditColumn("Weight", TrackingWhsOrder.WrapperSchema.WD_TotalWeight)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Weight
			});
			AddDefaultsColumn(new ZTextEditColumn("Cubic", TrackingWhsOrder.WrapperSchema.WD_TotalCubic)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Cubic
			});
			AddDefaultsColumn(new ZDateTimeColumn("Finalized Date", TrackingWhsOrder.WrapperSchema.WD_FinalisedDate, ZDateTimePickerFormat.Short)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.FinalisedDate
			});

			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns("Milestones")) // data column name
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

			if (WebEnv.AppInstance != null)
			{
				var siteUser = WebEnv.AppInstance.SiteUser as OrgContactWebUser;
				if (siteUser != null)
				{
					int customColumnKey = 1500;
#pragma warning disable IDE0001 // Simplify Names - Reason = All logic in this file has its scope on TrackingOrder
					foreach (CustomLabelInfo field in new WhsOrder.CustomLabelsProvider(null).GetCustomFields(siteUser.LoggedInOrganisation, Factory))
#pragma warning restore IDE0001 // Simplify Names
					{
						if (field.LabelName.StartsWith("WhsDocket.") && field.IsEnabled)
						{
							ZTemplateColumn column = ZTemplateColumn.GetNew(field.Caption, field.PropertyType, field.PropertyName);
							column.ColumnKey = customColumnKey;
							AddColumn(column);
							customColumnKey++;
						}
					}
				}
			}

#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrder)null).WhsOrder.WD_CustomerReference);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
			AddColumn(new ZTextEditColumn("Customer Ref.", TrackingWhsOrder.WrapperSchema.WD_CustomerReference)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.CustomerReference
			});
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingWhsOrderColumnProvider();
		}
	}
}
