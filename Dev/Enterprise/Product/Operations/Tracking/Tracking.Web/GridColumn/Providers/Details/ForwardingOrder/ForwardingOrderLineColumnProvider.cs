using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ForwardingOrderLineColumnProvider : LineColumnProvider
	{
		public ForwardingOrderLineColumnProvider(TrackingOrder order)
		{
			Order = order;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("ba285b57-7906-4551-acde-7bf7d45df1f1", "Line #"), JobOrderLineSchema.JO_LineNo.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.LineNumber });

			ZBindToChecker.CheckBindTo((ZGuid)((OrderLine)null).Product.PK);
			ZBindToChecker.CheckBindTo((ZGuid)((OrderLine)null).Order.PK);

			var siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			if (siteUser == null || siteUser.IsShipmentQuickViewUser)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("e989a192-6d95-48b9-9c9c-4a9fda0eda24", "Part #"), JobOrderLineSchema.JO_Partno.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.PartNumber });
			}
			else
			{
				AddToDictionaryAsDefault(new ZHyperLinkColumn(Res.GetString("e989a192-6d95-48b9-9c9c-4a9fda0eda24", "Part #"), JobOrderLineSchema.JO_Partno.Name)
				{
					ColumnKey = WebTracker.Grids.TrackingOrderLines.PartNumber,
					DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.ProductProfileDetailsPage) + (NoResString)"?Ref={0}&OrderRef={1}", // Request parameter names
					DataNavigateUrlFields = new[] { "Product.PK", "Order.PK" }
				});
			}

			if (siteUser != null && !siteUser.IsShipmentQuickViewUser)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b565170c-f303-41dc-a7b9-6e9887bdb110", "Description"), JobOrderLineSchema.JO_Description.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.Description });
			}

			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				var partManager = siteUser.LoggedInOrganisation.PartAttributeManager;
				if (partManager != null)
				{
					if (partManager.IsPartAttributeUsedByOrganisation(1))
					{
						AddToDictionaryAsDefault(new ZTextEditColumn(partManager.PartAttributeName1, JobOrderLineSchema.JO_PartAttrib1.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.PartAttribute1 });
					}
					if (partManager.IsPartAttributeUsedByOrganisation(2))
					{
						AddToDictionaryAsDefault(new ZTextEditColumn(partManager.PartAttributeName2, JobOrderLineSchema.JO_PartAttrib2.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.PartAttribute2 });
					}
					if (partManager.IsPartAttributeUsedByOrganisation(3))
					{
						AddToDictionaryAsDefault(new ZTextEditColumn(partManager.PartAttributeName3, JobOrderLineSchema.JO_PartAttrib3.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.PartAttribute3 });
					}
					if (partManager.IsSerialNumberUsedByOrganisation)
					{
						AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("cea220f2-d80c-4b99-bbf0-a83c124dc9a0", "Serial Number"), JobOrderLineSchema.JO_SerialNumber.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.SerialNumber });
					}
				}
			}

			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("b758dd3d-74b2-40e1-835b-013e747dcf6d", "Inner Packs"), JobOrderLineSchema.JO_InnerPacks.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.InnerPacks });
			AddToDictionaryAsDefault(new ZDropEditColumn(Res.GetString("7d6319e2-cf91-4e9c-a05c-3bd7a677c6b0", "Inner Package Type"), JobOrderLineSchema.JO_InnerPacksUQ.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.InnerPacksUQ });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("515e0266-f916-44a9-b196-073bea9cdff2", "Outer Packs"), JobOrderLineSchema.JO_OuterPacks.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.OuterPacks });
			AddToDictionaryAsDefault(new ZDropEditColumn(Res.GetString("5dd2a192-57b8-4542-8a22-69891467c3f4", "Outer Package Type"), JobOrderLineSchema.JO_OuterPacksUQ.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.OuterPacksUQ });
			if (siteUser != null && !siteUser.IsShipmentQuickViewUser)
			{
				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("75c44615-d6a4-4d4b-8769-900c9dcc4475", "Qty Ordered"), JobOrderLineSchema.JO_Quantity.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.QuantityOrdered });
				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("a7a29e7a-33c3-46c8-a245-0d10df18a3ea", "Qty Invoiced"), JobOrderLineSchema.JO_QtyInvoiced.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.QuantityInvoiced });
				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("02e38f0f-79e6-4eac-b95b-57c29dd63d7e", "Qty Received"), JobOrderLineSchema.JO_QtyReceived.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.QuantityReceived });
				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("1edef9b3-e928-4cc6-a7c9-b72223afc3b6", "Qty Remaining"), OrderLine.Schema.JO_QuantityRemaining) { ColumnKey = WebTracker.Grids.TrackingOrderLines.QuantityRemaining });
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b4c7a9c9-e604-4837-97dc-25b56fab4c80", "Unit of Qty"), JobOrderLineSchema.JO_F3_NKPackType.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.UnitOfQuantity });
				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("062e8d41-36fc-480d-a46b-1d286b668a31", "Item Price"), JobOrderLineSchema.JO_ItemPrice.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ItemPrice });
				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("1a0dd683-71fa-4d25-a9f9-9d05453e8884", "Total Price"), JobOrderLineSchema.JO_LinePrice.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.TotalPrice });
				AddToDictionary(new ZTextEditColumn(Res.GetString("d49339d1-b9d2-458e-98b3-5fcf883b8e4c", "Invoice #"), JobOrderLineSchema.JO_CommercialInvoiceNo.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.InvoiceNumber });
			}

			ZBindToChecker.CheckBindTo((string)((OrderLine)null).JO_LineStatus);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((OrderLine)null).JO_LineStatus_List);

			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("800d85bf-e056-4821-9c6d-dab053233539", "Line Status"), JobOrderLineSchema.JO_LineStatus.Name, "JO_LineStatus_List") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingOrderLines.LineStatus });

			if (Order != null)
			{
				AddLineAttributeColumns(AttributeManager.AttributeModules.Order, Order.Buyer, ZString.Empty);
			}

			ZBindToChecker.CheckBindTo((ZDateTime)((OrderLine)null).JO_LineDropDate);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("2c24c4b9-41cb-4563-a8c6-6ee2be14f4e5", "Required In Store Date"), JobOrderLineSchema.JO_LineDropDate.Name, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrderLines.RequiredDate });

			AddToDictionary(new ZDropEditColumn(Res.GetString("2569b52b-3c69-44b8-497d-97c7e4cf51c4", "Incoterm"), JobOrderLineSchema.JO_INCO.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.INCOTerm });
			AddToDictionary(new ZTextEditColumn(Res.GetString("86c56318-357d-4b1d-9682-a7ddeb683d4f", "Additional Terms"), JobOrderLineSchema.JO_AdditionalTerms.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.AdditionalTerms });

			AddToDictionary(new ZTextEditColumn(Res.GetString("e2b0dfff-46d2-4d76-9982-44f31c4c30ea", "Confirm Number"), JobOrderLineSchema.JO_ConfirmationNum.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ConfirmNumber });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("6e14c5ff-9960-423c-84de-cc07408eeb09", "Confirm Date"), JobOrderLineSchema.JO_ConfirmationDate.Name, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ConfirmDate });
			AddToDictionary(new ZDateTimeColumn(Res.GetString("e238af19-97d1-4e04-b6b5-5c3c7efe1ec8", "Required Ex Works Date"), JobOrderLineSchema.JO_ExWorksDate.Name, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrderLines.RequiredExWorksDate });

			AddToDictionary(new ZTextEditColumn(Res.GetString("be284702-b35a-44ea-bdc4-216fc4941b29", "Container #"), JobOrderLineSchema.JO_ContainerNumber.Name) { ColumnKey = WebTracker.Grids.TrackingOrderLines.ContainerNumber });
		}

		protected TrackingOrder Order { get; }
	}
}
