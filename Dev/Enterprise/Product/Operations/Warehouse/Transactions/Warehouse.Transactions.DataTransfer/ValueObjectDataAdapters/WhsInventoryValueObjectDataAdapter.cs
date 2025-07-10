using System;
using System.Xml.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsInventoryViewValueObjectDataAdapter : ValueObjectDataAdapter<WhsInventoryView, Xsd.WhsInventoryView>
	{
		#region ValueObjectDataAdapter Setup Overrides

		public override string RootCollectionElementName => "WhsInventoryViews";

		public override string RootElementName => "WhsInventoryView";

		public override XmlSchema Schema => throw new NotSupportedException();

		public override XmlSchema CollectionSchema => throw new NotSupportedException();

		#endregion

		#region Export

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		protected override void ExportToValueObjectCore(WhsInventoryView inventory, Xsd.WhsInventoryView xsdInventory, IValueObjectExportContext context)
		{
			var warehouse = inventory.Warehouse;
			var client = inventory.Client;
			var product = inventory.SupplierPart;
			xsdInventory.WarehouseName = warehouse?.WW_WarehouseName ?? ZString.Empty;
			xsdInventory.ClientCode = client?.OH_Code ?? ZString.Empty;
			xsdInventory.ProductCode = product?.OP_PartNum ?? ZString.Empty;
			xsdInventory.Description = product?.OP_Desc ?? ZString.Empty;
			xsdInventory.Commodity = inventory.CommodityCode;
			xsdInventory.ArrivalDateOrETA = inventory.WI_ArrivalDateOrETA.ToZDateTime();
			xsdInventory.Location = inventory.Location?.WLV_LocationString ?? ZString.Empty;
			xsdInventory.AvailableQty = inventory.WI_AvailableToPickQuantity;
			xsdInventory.AllocatedQty = inventory.WI_CrossDockQuantity;
			xsdInventory.CommittedQty = ((IWhsInventoryInternals)inventory).CommittedToTransactionQuantity;
			xsdInventory.TotalQty = inventory.WI_TotalUnits;
			xsdInventory.LocationStatus = inventory.LocationStatus;
			xsdInventory.InventoryStatus = inventory.StatusDesc;
			xsdInventory.EntryKey = inventory.WI_BondedEntryKey;
			xsdInventory.PalletID = inventory.WI_PalletID;
			var docketLine = inventory.InDocketLine;
			if (docketLine != null)
			{
				xsdInventory.ReceiptReference = docketLine.ReceiptReference;
			}
			xsdInventory.PartAttribute1 = inventory.WI_PartAttrib1;
			xsdInventory.PartAttribute2 = inventory.WI_PartAttrib2;
			xsdInventory.PartAttribute3 = inventory.WI_PartAttrib3;
			xsdInventory.AreaName = inventory.LocationPickAreaName;
			xsdInventory.AreaType = inventory.LocationPickAreaType;
			xsdInventory.TariffLookup = inventory.CustomsTariffLookup;
			xsdInventory.TariffItem = inventory.CustomsTariffItem;
			xsdInventory.TariffDescription = inventory.CustomsTariffDesc;
			xsdInventory.EntryDate = inventory.CustomsData.WB_EntryDate;
			xsdInventory.EntryNo = inventory.CustomsData.WB_EntryKey;
			xsdInventory.EntryLineNo = inventory.CustomsData.WB_EntryLineNo;
			xsdInventory.EntryLineNoSpecified = true;
			xsdInventory.DeclarationReference = inventory.CustomsData.WB_DeclarationReference;
			xsdInventory.CountryOfOrigin = inventory.CustomsData.WB_RN_NKCountryOfOrigin;
			xsdInventory.CustomsQty = inventory.CustomsData.WB_CustomsQty;
			xsdInventory.CustomsQtySpecified = true;
			xsdInventory.CustomsUQ = inventory.CustomsData.WB_CustomsUnitOfQty;
			xsdInventory.ValueForDuty = inventory.CustomsData.WB_ValueForDuty;
			xsdInventory.ValueForDutySpecified = true;
			xsdInventory.TILV = inventory.CustomsData.WB_TILV;
			xsdInventory.TILVSpecified = true;
			xsdInventory.AddInfo = inventory.CustomsData.WB_AddInfo;
			xsdInventory.ExpiryDate = inventory.WI_ExpiryDate;
			xsdInventory.PackingDate = inventory.WI_PackingDate;
			AddExportEvent(xsdInventory, inventory.InDocketLine, context);
		}

		#region Temporary AddExportEvent - remove after WhsInventory is removed.

		// Temporary using this overload until inventory is removed, then should be able to use base method again.
		void AddExportEvent(IValueObject valueObject, WhsDocketLine inventoryLine, IValueObjectExportContext context)
		{
			if (valueObject != null)
			{
				var log = inventoryLine.GetLogs().AddNew(Events.DataExport, "");
				if (context != null && log.SL_Reference.IsEmpty && !string.IsNullOrEmpty(context.ExportPurpose))
				{
					string description = inventoryLine.Factory.LoadFromNaturalKey<IEDIMessagePurpose>(EDIMessagePurposeSchema.EMP_Code, context.ExportPurpose)?.EMP_Description;
					ZString suggestedReference = (NoResString)"Purpose: " + (string.IsNullOrEmpty(description) ? context.ExportPurpose : description);// this is a hard coded constant

					using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
					{
						log.SL_Reference = suggestedReference.Left(StmALog.Schema.SL_ReferenceMaxLength);
					}
				}
			}
		}

		#endregion

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(WhsInventoryView bizObj, Xsd.WhsInventoryView value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
