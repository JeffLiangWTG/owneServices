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
	public class TrackingWhsOrderColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZHyperLinkColumn orderNumberColumn = new ZHyperLinkColumn(Res.GetString("78caef54-927a-4401-9e76-44c59eaff7d1", "Order#"), TrackingWhsOrder.WrapperSchema.WD_ExternalReference)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.OrderNumber,
				DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.WarehouseOrderDetailsPage) + (NoResString)"?Ref={0}", // Partial URL
				DataNavigateUrlFields = new string[1] { TrackingWhsOrder.WrapperSchema.WD_PK }
			};
			AddToDictionaryAsRequired(orderNumberColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrder)null).WhsOrder.Warehouse.WW_WarehouseName);
			ZTextEditColumn warehouseColumn = new ZTextEditColumn(Res.GetString("d50e9e1c-b149-492f-a53a-c37f018e2988", "Warehouse"), TrackingWhsOrder.GetSchemaPath("Warehouse+WW_WarehouseName")) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Warehouse };
			AddToDictionaryAsDefault(warehouseColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrder)null).WhsOrder.ConsigneeDocAddress.E2_CompanyName);
			ZTextEditColumn consigneeColumn = new ZTextEditColumn(Res.GetString("d589c3f8-8ab7-431b-8154-b8c97e7a83e2", "Consignee"), TrackingWhsOrder.GetSchemaPath("ConsigneeDocAddress+E2_CompanyName")) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Consignee };
			AddToDictionaryAsDefault(consigneeColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrder)null).WhsOrder.TransportCoDocAddress.E2_CompanyName);
			ZTextEditColumn transportCoColumn = new ZTextEditColumn(Res.GetString("e6e40be3-177e-4291-b800-38fb8ba8b542", "Transport Co."), TrackingWhsOrder.GetSchemaPath("TransportCoDocAddress+E2_CompanyName")) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.TransportCompany };
			AddToDictionaryAsDefault(transportCoColumn);

			ZHyperLinkColumn transportRefColumn = new ZHyperLinkColumn(Res.GetString("8391657e-7407-4eba-93ec-a6cb93e8f80d", "Transport Ref."), TrackingWhsOrder.WrapperSchema.WD_TransportReference)
			{
				ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.TransportReference,
				DataNavigateUrlFormatString = "{0}",
				DataNavigateUrlFields = new string[1] { TrackingWhsOrder.WrapperSchema.WD_TransportCoUrl },
				IsExternalHyperlink = true,
				Target = (NoResString)"_blank" // programmatic constant
			};
			AddToDictionaryAsDefault(transportRefColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrder)null).WhsOrder.WD_DocketID);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("618ac345-599a-45cc-bed3-437d12a12e2c", "Docket#"), TrackingWhsOrder.WrapperSchema.WD_DocketID) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.DocketNumber });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingWhsOrder)null).WhsOrder.RequiredDate);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("6092f053-1478-4e79-8983-72e9e94372e7", "Req. Date"), TrackingWhsOrder.WrapperSchema.TrackingRequiredDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.RequiredDate });

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrder)null).WhsOrder.WD_DocketStatusDescription);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("55356ebd-fa15-4fd0-868a-bb221d71895b", "Status"), TrackingWhsOrder.WrapperSchema.WD_DocketStatusDescription) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Status });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrder)null).WhsOrder.WD_TotalUnits);
			AddToDictionary(new ZTextEditColumn(Res.GetString("744546c2-b67f-41ab-89ee-b58dca7a7b3f", "Units"), TrackingWhsOrder.WrapperSchema.WD_TotalUnits) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Units });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrder)null).WhsOrder.WD_TotalWeight);
			AddToDictionary(new ZTextEditColumn(Res.GetString("488a7a8e-129a-4404-a3cc-26a9d7000b2f", "Weight"), TrackingWhsOrder.WrapperSchema.WD_TotalWeight) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Weight });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingWhsOrder)null).WhsOrder.WD_TotalCubic);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("411cb23f-c30c-448b-aecb-a0749d9b9af4", "Cubic"), TrackingWhsOrder.WrapperSchema.WD_TotalCubic) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.Cubic });

			ZBindToChecker.CheckBindTo((ZDateTimeOffset)((TrackingWhsOrder)null).WhsOrder.WD_FinalisedDate);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("d5e00410-907e-4ff4-8b3f-5c82edce82a6", "Finalized Date"), TrackingWhsOrder.WrapperSchema.WD_FinalisedDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.FinalisedDate });

			ZBindToChecker.CheckBindTo((TrackingMilestoneCollection)((TrackingWhsOrder)null).Milestones);
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // data column name
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
#pragma warning disable IDE0001 // Simplify Names - Reason = All logic in this file has its scope on WhsOrder
						foreach (CustomLabelInfo field in new WhsOrder.CustomLabelsProvider(null).GetCustomFields(loggedInOrg, loggedInOrg.Factory))
#pragma warning restore IDE0001 // Simplify Names
						{
							if (field.LabelName.StartsWith("WhsDocket."))
							{
								var specifiedBindTo = TrackingWhsOrder.GetSchemaPath(field.PropertyName);

								if (AddToDictionary(field, customColumnKey, specifiedBindTo))
								{
									customColumnKey++;
								}
							}
						}
					}
				}
			}

			ZBindToChecker.CheckBindTo((ZString)((TrackingWhsOrder)null).WhsOrder.WD_CustomerReference);
			AddToDictionary(new ZTextEditColumn(Res.GetString("93972139-45b8-468e-be56-223af49925b1", "Customer Ref."), TrackingWhsOrder.WrapperSchema.WD_CustomerReference) { ColumnKey = WebTracker.Grids.TrackingWarehouseOrders.CustomerReference });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.OrderNumber);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.Warehouse);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.Consignee);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.TransportCompany);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.TransportReference);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.DocketNumber);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.RequiredDate);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.Status);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.Units);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.Weight);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.Cubic);
			result.Add((int)WebTracker.Grids.TrackingWarehouseOrders.FinalisedDate);
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
#pragma warning disable IDE0001 // Simplify Names - Reason = All logic in this file has its scope on WhsOrder
						foreach (CustomLabelInfo field in new WhsOrder.CustomLabelsProvider(null).GetCustomFields(loggedInOrg, loggedInOrg.Factory))
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
