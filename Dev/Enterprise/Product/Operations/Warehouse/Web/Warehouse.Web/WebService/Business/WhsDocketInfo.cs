using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsDocketInfo : DataObjectInfo
	{
		#region Constructors

		public WhsDocketInfo(WhsDocket docket, bool shouldCreateDocketLines = true, GlbStaff staff = null)
			: this()
		{
			PK = docket.PK.ToGuid();
			DocketID = docket.WD_DocketID;
			DocketSubType = docket.WD_DocketSubType;
			ExternalReference = docket.WD_ExternalReference;
			ExternalReferenceSplit = docket.WD_ExternalReferenceSplit;

			var client = docket.Client;
			if (client != null)
			{
				ClientCode = client.OH_Code;
				PartAttributes = new WhsOrgPartAttributesInfo(client);
				var clientParams = WhsClientParams.GetClientParams(client);
				var whsClientParameterByWarehouseCollection = clientParams.ClientParametersByWarehouse;
				var whsClientParameterByWarehouse = whsClientParameterByWarehouseCollection.FindWithEmptyFallback(client.PK, docket.WD_WW_Whs, docket.WD_ReceiveCategory);
				if (whsClientParameterByWarehouse != null)
				{
					EnforcePalletIDs = whsClientParameterByWarehouse.WY_EnforcePalletIDEntry;
					ValidatePalletID = whsClientParameterByWarehouse.WY_ValidatePalletIDAsSSCCOnUnload;
				}
			}

			var supplier = docket.Supplier;
			if (supplier != null)
			{
				SupplierCode = supplier.OH_Code;
			}

			var transportCo = docket.TransportCo;
			if (transportCo != null)
			{
				TransportCompanyCode = transportCo.OH_Code;
			}

			var docketLines = docket.Lines;
			if (shouldCreateDocketLines)
			{
				lines = new WhsDocketLineInfoCollection(docketLines);
			}
			docketHoldCodes = new WhsDocketProductHoldCodeInfoCollection(docketLines);

			if (docket is WhsOrder order)
			{
				PickPriority = order.WD_PickPriority;
				AssignedPacker = order.WD_GS_NKAssignedPacker;
				RequiredDate = order.WD_RequiredDate.IsValid ? order.WD_RequiredDate.ToDateTime() : DateTime.MinValue;
				CreateTime = order.WD_SystemCreateTimeUtc.IsValid ? order.WD_SystemCreateTimeUtc.ToDateTime() : DateTime.MinValue;
			}
			else if (docket is WhsReceive)
			{
				var docketWarehouse = docket.Warehouse;
				if (client?.MiscServ?.OM_WhsGenerateSSCCOnInbound ?? false)
				{
					GS1Prefix = SSCCPrefixFinder.GetSSCCPrefix(SSCCGenerationContext.CheckIfBarcodeIsSSCC, () => client, () => docketWarehouse, docket.NotificationSubscriber, shouldPrompt: false);
				}

				if (docketWarehouse.WW_GG_ReleaseGroup.IsValid)
				{
					var docketTask = docket.Factory.LoadTop1<ProcessTask>(TaskManagementHelper.GetJobTasksQuery(docket, WarehouseTaskFormFlowTypes.UnloadJob, staff));
					TaskPK = docketTask?.PK.ToGuid() ?? Guid.Empty;
				}
			}
		}

		public WhsDocketInfo()
		{
			PK = Guid.Empty;
			DocketID = "";
			DocketSubType = "";
			ExternalReference = "";
			ClientCode = "";
			SupplierCode = "";
			TransportCompanyCode = "";
			ASNUnloadState = ASNState.NoPallets;
			EnforcePalletIDs = false;
			IsEmptyAsnPalletMatchingEnabledForUnload = false;
			ValidatePalletID = false;
		}

		#endregion

		#region Properties

		#region PK

		public Guid PK
		{
			get;
			set;
		}

		#endregion

		#region ClientCode

		public string ClientCode
		{
			get;
			set;
		}

		#endregion

		#region PalletsToTransferCompletely

		public List<string> PalletsToTransferCompletely
		{
			get { return palletsToTransferCompletely ?? (palletsToTransferCompletely = new List<string>()); }
			set { palletsToTransferCompletely = value; }
		}
		List<string> palletsToTransferCompletely;

		#endregion

		#region DocketID

		public string DocketID
		{
			get;
			set;
		}

		#endregion

		#region DocketSubType

		public string DocketSubType
		{
			get;
			set;
		}

		#endregion

		#region ExternalReference

		public string ExternalReference
		{
			get;
			set;
		}

		#endregion

		#region ExternalReferenceSplit

		public byte ExternalReferenceSplit
		{
			get;
			set;
		}

		#endregion

		#region SupplierCode

		public string SupplierCode
		{
			get;
			set;
		}

		#endregion

		#region TransportCompanyCode

		public string TransportCompanyCode
		{
			get;
			set;
		}

		#endregion

		#region CanASNUnload

		public ASNState ASNUnloadState
		{
			get;
			set;
		}

		#endregion

		#region EnforcePalletIDs

		public bool EnforcePalletIDs
		{
			get;
			set;
		}

		#endregion

		#region ValidatePalletID

		public bool ValidatePalletID
		{
			get;
			set;
		}

		#endregion

		#region GS1Prefix

		public string GS1Prefix
		{
			get;
			set;
		}

		#endregion

		#region IsEmptyAsnPalletMatchingEnabled

		public bool IsEmptyAsnPalletMatchingEnabledForUnload
		{
			get;
			set;
		}

		#endregion

		#region ProductsWhichMayFulfillAsnLinesWithStockUnit

		public Guid[] ProductsWhichMayFulfillAsnLinesWithStockUnit
	{
			get { return productsWhichMayFulfillAsnLinesWithStockUnit ?? (productsWhichMayFulfillAsnLinesWithStockUnit = Array.Empty<Guid>()); }
			set { productsWhichMayFulfillAsnLinesWithStockUnit = value; }
		}

		Guid[] productsWhichMayFulfillAsnLinesWithStockUnit;

		#endregion

		#region PartAttributes

		public WhsOrgPartAttributesInfo PartAttributes
		{
			get { return partAttributes ?? (partAttributes = new WhsOrgPartAttributesInfo()); }
			set { partAttributes = value; }
		}

		WhsOrgPartAttributesInfo partAttributes;

		#endregion

		#region Lines

		public WhsDocketLineInfoCollection Lines
		{
			get { return lines ?? (lines = new WhsDocketLineInfoCollection()); }
			set { lines = value; }
		}

		WhsDocketLineInfoCollection lines;

		#endregion

		#region DocketHoldCodes

		public WhsDocketProductHoldCodeInfoCollection DocketHoldCodes
		{
			get { return docketHoldCodes ?? (docketHoldCodes = new WhsDocketProductHoldCodeInfoCollection()); }
			set { docketHoldCodes = value; }
		}

		WhsDocketProductHoldCodeInfoCollection docketHoldCodes;

		#endregion

		#region UsedSerialNumbers

		public string[] UsedSerialNumbers
		{
			get { return usedSerialNumbers ?? (usedSerialNumbers = Array.Empty<string>()); }
			set { usedSerialNumbers = value; }
		}

		string[] usedSerialNumbers;

		#endregion

		#region RecentlyUsedDockDoorLocation

		public string RecentlyUsedDockDoorLocation { get; set; }
		public string RecentlyUsedDockDoorLocation_UserFriendly { get; set; }
		public Guid RecentlyUsedDockDoorLocationPK { get; set; }

		#endregion

		public byte PickPriority { get; set; }
		public string AssignedPacker { get; set; }
		public DateTime RequiredDate { get; set; }
		public DateTime CreateTime { get; set; }
		public Guid TaskPK { get; set; }

		ISSCCPrefixFinder SSCCPrefixFinder
		{
			get => ssccPrefixFinder ?? (ssccPrefixFinder = ObjectFactory.Get<ISSCCPrefixFinder>());
		}

		ISSCCPrefixFinder ssccPrefixFinder;

		#endregion
	}

	#region ASNState

	public enum ASNState
	{
		NoPallets,
		HasASNPalletsToUnload,
		AllASNPalletsUnloaded
	}

	#endregion
}
