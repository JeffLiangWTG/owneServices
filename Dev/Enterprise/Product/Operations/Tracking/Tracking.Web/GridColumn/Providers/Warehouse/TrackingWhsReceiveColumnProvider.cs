using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingWhsReceiveColumnProvider : GridColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZHyperLinkColumn refNumberColumn = new ZHyperLinkColumn(Res.GetString("2c03544e-4fa9-451e-bb12-3614cf967fbc", "Receive Ref. #"), TrackingWhsReceive.WrapperSchema.WD_ExternalReference)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.ReceiveNumber,
				DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.WarehouseReceiveDetailsPage) + (NoResString)"?Ref={0}", // Its string formater
				DataNavigateUrlFields = new string[1] { TrackingWhsReceive.WrapperSchema.WD_PK }
			};
			AddToDictionaryAsRequired(refNumberColumn);

			ZFindBoxColumn warehouseColumn = new ZFindBoxColumn(Res.GetString("5c253057-e9f3-4a2c-be84-d95417008445", "Warehouse"), TrackingWhsReceive.WrapperSchema.WD_WW_Whs, TrackingWhsReceive.GetSchemaPath("Lookups.Warehouses"), typeof(TrackingWhsReceive)) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.Warehouse };
			warehouseColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(warehouseColumn);

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("af769536-4354-4c16-88e0-f1639f5d8f40", "Docket #"), TrackingWhsReceive.WrapperSchema.WD_DocketID) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.DocketNumber });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("bf360a4f-10af-4c7f-933e-5293745dfa93", "Booking Date"), TrackingWhsReceive.WrapperSchema.WD_BookingDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.BookingDate });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("e75f9094-9b91-4f64-9ab2-e6586c4eb1d3", "ETA"), TrackingWhsReceive.WrapperSchema.WD_ETA, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.ETA });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("1546295d-ec9f-4525-9e41-750c07559af4", "Arrival Date"), TrackingWhsReceive.WrapperSchema.WD_ArrivalDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.ArrivalDate });

#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsReceive)null).WhsReceive.WD_DocketStatusDescription);
#pragma warning restore IDE0004 // Remove Unnecessary Cast

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("e0b1a279-1a28-4c87-9eed-a9fd885d0224", "Status"), TrackingWhsReceive.WrapperSchema.WD_DocketStatusDescription) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.Status });

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("8913a6aa-5d60-43c4-bb66-fe841fb79a0e", "Total Units"), TrackingWhsReceive.WrapperSchema.WD_TotalUnits) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.TotalUnits });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("25c02acc-a810-4ad0-96d0-87fe841bae96", "Total Pallets"), TrackingWhsReceive.WrapperSchema.WD_TotalPallets) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.TotalPallets });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("da8ffdbd-ec95-4f17-a6d6-31eb0ce1bf07", "Finalized Date"), TrackingWhsReceive.WrapperSchema.WD_FinalisedDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.FinalisedDate });

#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo((TrackingMilestoneCollection)((TrackingWhsReceive)null).Milestones);
#pragma warning restore IDE0004 // Remove Unnecessary Cast

			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // May be an identifier
			{
				if (column.HeaderText == Res.GetString("f3ff4281-e665-4549-8321-e80e987ec09e", "Last Milestone Desc."))
				{
					AddToDictionaryAsDefault(column);
				}
				else
				{
					AddToDictionary(column);
				}
			}

			if (WebEnv.AppInstance != null)
			{
				var siteUser = WebEnv.AppInstance.SiteUser as OrgContactWebUser;
				if (siteUser != null)
				{
					var loggedInOrg = siteUser.LoggedInOrganisation;
					if (loggedInOrg != null)
					{
						int customColumnKey = 1500;
#pragma warning disable IDE0001 // Simplify Names - Reason = All logic in this file has its scope on WhsReceive
						foreach (CustomLabelInfo field in new WhsReceive.CustomLabelsProvider(null).GetCustomFields(loggedInOrg, loggedInOrg.Factory))
#pragma warning restore IDE0001 // Simplify Names
						{
							if (field.LabelName.StartsWith("WhsDocket.")) // Inside string
							{
								var specifiedBindTo = TrackingWhsReceive.GetSchemaPath(field.PropertyName);
								if (AddToDictionary(field, customColumnKey, specifiedBindTo))
								{
									customColumnKey++;
								}
							}
						}
					}
				}
			}

			AddToDictionary(new ZTextEditColumn(Res.GetString("03aceb6e-726c-4be0-a752-dd90b8faad24", "Customer Ref."), TrackingWhsReceive.WrapperSchema.WD_CustomerReference) { ColumnKey = WebTracker.Grids.TrackingWarehouseReceives.CustomerReference });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingWarehouseReceives.ReceiveNumber);
			result.Add((int)WebTracker.Grids.TrackingWarehouseReceives.Warehouse);
			result.Add((int)WebTracker.Grids.TrackingWarehouseReceives.DocketNumber);
			result.Add((int)WebTracker.Grids.TrackingWarehouseReceives.BookingDate);
			result.Add((int)WebTracker.Grids.TrackingWarehouseReceives.ETA);
			result.Add((int)WebTracker.Grids.TrackingWarehouseReceives.ArrivalDate);
			result.Add((int)WebTracker.Grids.TrackingWarehouseReceives.Status);
			result.Add((int)WebTracker.Grids.TrackingWarehouseReceives.TotalUnits);
			result.Add((int)WebTracker.Grids.TrackingWarehouseReceives.TotalPallets);
			result.Add((int)WebTracker.Grids.TrackingWarehouseReceives.FinalisedDate);
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // data column name
			{
				result.Add(column.UniqueKey);
			}
			if (WebEnv.AppInstance != null)
			{
				var siteUser = WebEnv.AppInstance.SiteUser as OrgContactWebUser;
				if (siteUser != null)
				{
					var loggedInOrg = siteUser.LoggedInOrganisation;
					if (loggedInOrg != null)
					{
						int customColumnKey = 1500;
#pragma warning disable IDE0001 // Simplify Names - Reason = All logic in this file has its scope on WhsReceive
						foreach (CustomLabelInfo field in new WhsReceive.CustomLabelsProvider(null).GetCustomFields(loggedInOrg, loggedInOrg.Factory))
#pragma warning restore IDE0001 // Simplify Names
						{
							if (field.LabelName.StartsWith("WhsDocket.") && field.IsEnabled)
							{
								result.Add(customColumnKey);
								customColumnKey++;
							}
						}
					}
				}
			}
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.CustomerReference);
			return result;
		}
	}
}
