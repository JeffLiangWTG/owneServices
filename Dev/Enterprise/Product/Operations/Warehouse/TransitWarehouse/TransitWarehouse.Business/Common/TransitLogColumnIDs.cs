namespace Enterprise.Warehouse.Transit.Business
{
	public static class TransitLogColumnIDs
	{
		public enum PackageStateColumn
		{
			Package,
			RTU,
			DTU,
			RCN,
			ASN,
			DCN,
			LoadList,
			Status
		}

		public enum PackageStateIndexerColumn
		{
			Package,
			RCN,
			DCN,
			LoadList,
			Status,
			UxmlPackage
		}

		public enum RTUColumn
		{
			RTU
		}

		public enum DTUColumn
		{
			DTU
		}

		public enum ASNColumn
		{
			ASN
		}

		public enum RCNColumn
		{
			RCN
		}

		public enum DCNColumn
		{
			DCN
		}

		public enum DCNIndexerColumn
		{
			DCN
		}

		public enum LoadListColumn
		{
			LoadList
		}

		public enum ShipmentColumn
		{
			Source,
			Target
		}

		public enum CusEntryNumberColumn
		{
			Type,
			Number,
			CountryCode
		}

		public enum CIN750NotificationColumn
		{
			Result,
			MessageType,
			RefType,
			RefCode,
			EnterpriseCode,
			CustomsStatus,
			FromCTO,
			CTOCINCode,
			ToCTO,
			ToCINCode,
			CFSWarehouse,
			CFSCINCode
		}

		public enum DocPackingLineColumn
		{
			RefType,
			PNTS,
			RefCode,
			Description,
			Weight,
			Quantity,
			AccompanyingDocumentType,
			AccompanyingDocumentRef,
			TemporaryStorageDeclaration
		}

		public enum CRESAColumn
		{
			OperationalPort,
			PCS,
			TransportMode,
			TranshipmentPort,
			PortOfArrival,
			PortArea,
			PortServiceReference,
			PortLocation,
			CargoReceiptDate,
			ETAatPortOfArrival,
			Buyer,
			BuyerProviderID,
			Supplier,
			SupplierProviderID,
			SendingParty,
			SendingPartyProviderID,
			Forwarder,
			ForwarderProviderID,
			Agent,
			AgentProviderID,
			BookingReference,
			WarehouseEntryNumber,
			ECVReference,
			CRESAReference,
			ShipmentNumber,
			CommodityReference,
			GoodsIn,
			GoodsAreSealed
		}

		public enum CRESABookingPackingLine
		{
			Packs,
			Weight,
			Volume,
			GoodsDescription
		}
	}
}
