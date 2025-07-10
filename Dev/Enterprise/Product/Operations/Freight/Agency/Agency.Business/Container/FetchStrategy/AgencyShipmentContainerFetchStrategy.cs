using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class AgencyShipmentContainerFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public AgencyShipmentContainerFetchStrategy(AgencyShipmentContainer container)
			: base(container) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			bool requireShipment = false;
			bool requireContainerEntryNumbers = false;
			bool requireShipmentEntryNumbers = false;
			bool requireShipmentAddresses = false;
			bool requireContainerStock = false;
			bool requireMessages = false;
			bool requireTransports = false;
			bool requireCusEntryNums = false;

			foreach (TableColumn column in columns)
			{
				switch (column.ColumnName)
				{
					case AgencyShipmentContainer.Schema.CustomsEntryNumber:
					case AgencyShipmentContainer.Schema.CustomsEntryNumberType:
						requireContainerEntryNumbers = true;
						break;

					case AgencyShipmentContainer.Schema.BillContainersEntryNumber:
					case AgencyShipmentContainer.Schema.BillContainersEntryNumberType:
						requireShipment = true;
						requireContainerEntryNumbers = true;
						requireShipmentEntryNumbers = true;
						break;

					case "Booking+" + AgencyShipment.Schema.BookingPartyNameOrPK: // hard-coded constant
					case "Booking+" + AgencyShipment.Schema.ConsignorNameOrPK: // hard-coded constant
					case "Booking+" + AgencyShipment.Schema.ConsigneeNameOrPK: // hard-coded constant
						requireShipment = true;
						requireShipmentAddresses = true;
						break;

					case AgencyShipmentContainer.Schema.JC_ContainerYardEmptyReturnGateIn:
						requireContainerStock = true;
						requireShipment = true;
						requireTransports = true;
						break;

					case AgencyShipmentContainer.Schema.JC_ImportReleaseOrderStatus:
						requireMessages = true;
						break;

					case AgencyShipmentContainer.Schema.JC_Calc_ImportDetentionFreeDays:
					case AgencyShipmentContainer.Schema.JC_Calc_ExportDetentionFreeDays:
						requireShipment = true;
						requireShipmentAddresses = true;
						requireTransports = true;
						break;

					case nameof(AgencyShipmentContainer.AdditionalReferenceNumbersAsString):
						requireCusEntryNums = true;
						break;

					default:
						if (column.ColumnName.StartsWith("Booking+", StringComparison.OrdinalIgnoreCase)) // hard-coded constant
						{
							requireShipment = true;
						}
						break;
				}
			}

			AgencyShipmentContainer container = (AgencyShipmentContainer)BusinessObject;

			if (requireShipment)
			{
				Factory.AddFetchHint(typeof(AgencyShipment), JobShipmentSchema.PK, container.JC_JS_FCLBookingOnlyLink);
			}

			if (requireShipmentAddresses)
			{
				Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, container.JC_JS_FCLBookingOnlyLink);
			}

			if (requireContainerEntryNumbers)
			{
				ZQuery containerQuery = new ZQuery();
				containerQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, container.JC_JS_FCLBookingOnlyLink);
				containerQuery.AddToFilter(CusEntryNumSchema.CE_Category, "CUS");
				containerQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				Factory.AddFetchHint(CusEntryNumSchema.Instance, containerQuery);
			}

			if (requireCusEntryNums)
			{
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, container.PK);
			}

			if (requireShipmentEntryNumbers)
			{
				ZQuery shipmentQuery = new ZQuery();
				shipmentQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, container.PK);
				shipmentQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				Factory.AddFetchHint(CusEntryNumSchema.Instance, shipmentQuery);
			}

			if (requireContainerStock)
			{
				Factory.AddFetchHint(RefContainerStockSchema.R6_ContainerNum, container.JC_ContainerNum);
			}

			if (requireMessages)
			{
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, container.PK);
			}

			if (requireTransports)
			{
				Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, container.JC_JS_FCLBookingOnlyLink);
			}

			Factory.AddFetchHint(CusInBondHeaderSchema.BH_ParentID, container.JC_JS_FCLBookingOnlyLink);
		}
	}
}


