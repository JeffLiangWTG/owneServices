using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	abstract class CresaBuilderForTransitWarehouseTest<T> : TestCaseWithFactory where T : EnterpriseBusinessObject, IConsignment
	{
		#region TestETAAtPortOfArrival

		public void TestETAAtPortOfArrival_Validation()
		{
			var cresa = GetCresa();

			cresa.ETA = ZDateTime.Empty;
			cresa.ValidateAllIncludingChildren();
			AssertNoMessageErrors("Port of Arrival ETA should NOT have an error message.", cresa.ETAInfo);
		}

		#endregion

		#region TestBasicSetup

		public void TestBasicSetup()
		{
			var cresa = GetCresa();
			AssertEquals(SourceType, ((IDataSourceProvider)cresa).SourceType);
			AssertEquals(ConsignmentJobID, ((IDataSourceProvider)cresa).SourceID);
			AssertEquals(ConsignmentJobID, cresa.ShipmentNumber);
			AssertEquals("STD", cresa.ShipmentType.Code);
			AssertEquals("", cresa.PackType.Code);
			AssertEquals("", cresa.PackType.Description);
			AssertEquals(0, cresa.PackingLines.Count);
		}

		#endregion

		#region TestTransportMode

		public void TestTransportMode()
		{
			var cresa = GetCresa();
			AssertEquals("Default TransportMode must be RTE.", "RTE", cresa.TransportMode);
		}

		public void TestTransportMode_Validation()
		{
			var cresa = GetCresa();
			AssertNoMessageErrors("Transport Mode should not have error message.", cresa.TransportModeInfo);

			cresa.TransportMode = "ACDE";
			cresa.ValidateAllIncludingChildren();
			AssertHasMessageError("Transport Mode must not be more than three characters.", cresa.TransportModeInfo, "Mode has maximum three characters.");

			cresa.TransportMode = ZString.Empty;
			cresa.ValidateAllIncludingChildren();
			AssertHasMessageError("Transport Mode is mandatory.", cresa.TransportModeInfo, "Transport mode is required.");
		}

		#endregion

		#region TestCargoReceiptDate

		public void TestCargoReceiptDate_NoPackages()
		{
			var cresa = GetCresa();
			cresa.ValidateAllIncludingChildren();
			var cargoReceiptDateErrorMessage = "Cargo Receipt Date is required.";
			AssertEquals("Cargo Receipt Date should not have a default value.", ZDateTime.Empty, cresa.CargoReceiptDate);
			AssertHasMessageError("Cargo Receipt Date should have error message if empty.", cresa.CargoReceiptDateInfo, cargoReceiptDateErrorMessage);
		}

		public void TestCargoReceiptDate_PackagesWithNoRTU()
		{
			var cresa = GetCresa_PackagesWithNoRTU();
			cresa.ValidateAllIncludingChildren();
			var cargoReceiptDateErrorMessage = "Cargo Receipt Date is required.";
			AssertEquals("Cargo Receipt Date should not have a default value.", ZDateTime.Empty, cresa.CargoReceiptDate);
			AssertHasMessageError("Cargo Receipt Date should have error message if empty.", cresa.CargoReceiptDateInfo, cargoReceiptDateErrorMessage);
		}

		public void TestCargoReceiptDate_PackagesWithNonGateInRTU()
		{
			var cresa = GetCresa_PackagesWithNonGateInRTU();
			cresa.ValidateAllIncludingChildren();
			var cargoReceiptDateErrorMessage = "Cargo Receipt Date is required.";
			AssertEquals("Cargo Receipt Date should not have a default value.", ZDateTime.Empty, cresa.CargoReceiptDate);
			AssertHasMessageError("Cargo Receipt Date should have error message if empty.", cresa.CargoReceiptDateInfo, cargoReceiptDateErrorMessage);
		}

		public void TestCargoReceiptDate_PackagesWithSingleGateInRTU()
		{
			var cresa = GetCresa_PackagesWithSingleGateInRTU();
			AssertEquals("CargoReceiptDate must be unload completed date.", Now.ToZDateTime(), cresa.CargoReceiptDate);
			AssertNoMessageErrors("Cargo Receipt Date should not have error message if populated.", cresa.CargoReceiptDateInfo);
		}

		public void TestCargoReceiptDate_PackagesWithMultipleGateInRTU()
		{
			var cresa = GetCresa_PackagesWithMultipleGateInRTU();
			AssertEquals("CargoReceiptDate must be the latest unload completed date.", Now.ToZDateTime(), cresa.CargoReceiptDate);
			AssertNoMessageErrors("Cargo Receipt Date should not have error message if populated.", cresa.CargoReceiptDateInfo);
		}

		#endregion

		#region TestGoodsInDate

		public void TestGoodsInDate_NoPackages()
		{
			var cresa = GetCresa();
			cresa.ValidateAllIncludingChildren();
			var goodsInDateErrorMessage = "Date/Time goods arrived at the warehouse is required.";
			AssertEquals("Goods In Date should not have a default value.", ZDateTime.Empty, cresa.GoodsInDateTime);
			AssertHasMessageError("Cargo Receipt Date should have error message if empty.", cresa.GoodsInDateTimeInfo, goodsInDateErrorMessage);
		}

		public void TestGoodsInDate_PackagesWithNoRTU()
		{
			var cresa = GetCresa_PackagesWithNoRTU();
			cresa.ValidateAllIncludingChildren();
			var goodsInDateErrorMessage = "Date/Time goods arrived at the warehouse is required.";
			AssertEquals("Goods In Date should not have a default value.", ZDateTime.Empty, cresa.GoodsInDateTime);
			AssertHasMessageError("Goods In Date should have error message if empty.", cresa.GoodsInDateTimeInfo, goodsInDateErrorMessage);
		}

		public void TestGoodsInDate_PackagesWithNonUnloadedPackage()
		{
			var cresa = GetCresa_PackagesWithNonUnloadedPackage();
			cresa.ValidateAllIncludingChildren();
			var goodsInDateErrorMessage = "Date/Time goods arrived at the warehouse is required.";
			AssertEquals("Goods In Date should not have a default value.", ZDateTime.Empty, cresa.GoodsInDateTime);
			AssertHasMessageError("Goods In Date should have error message if empty.", cresa.GoodsInDateTimeInfo, goodsInDateErrorMessage);
		}

		public void TestGoodsInDate_PackagesWithSingleUnloadedPackage()
		{
			var cresa = GetCresa_PackagesWithSingleUnloadedPackage();
			AssertEquals("Goods In Date must be unload completed date.", Now.ToZDateTime(), cresa.GoodsInDateTime);
			AssertNoMessageErrors("Goods In Date should not have error message if populated.", cresa.GoodsInDateTimeInfo);
		}

		public void TestGoodsInDate_PackagesWithMultipleUnloadedPackages()
		{
			var cresa = GetCresa_PackagesWithMultipleUnloadedPackages();
			AssertEquals("Goods In Date must be the latest unload completed date.", Now.ToZDateTime(), cresa.GoodsInDateTime);
			AssertNoMessageErrors("Goods In Date should not have error message if populated.", cresa.GoodsInDateTimeInfo);
		}

		#endregion

		#region TestOperationalPort

		public void TestOperationalPort_NoPortCode()
		{
			var cresaWithoutWarehouse = GetCresa();
			cresaWithoutWarehouse.ValidateAllIncludingChildren();
			AssertEquals("Operational Port", "", cresaWithoutWarehouse.OperationalPort.Code);
			AssertHasMessageError("Operational Port is mandatory.", cresaWithoutWarehouse.ErrorPlaceHolderInfo, "Operational port is required.");
		}

		public void TestOperationalPort_WithPortCode()
		{
			var cresaWithWarehouse = GetCresa_WithPortCode();
			AssertEquals("Operational Port", "FRMAR", cresaWithWarehouse.OperationalPort.Code);
			AssertNoMessageError("Operational Port no message error", cresaWithWarehouse.ErrorPlaceHolderInfo, "Operational port is required.");

			cresaWithWarehouse.OperationalPort.Code = "";
			cresaWithWarehouse.ValidateAllIncludingChildren();
			AssertHasMessageError("Operational Port is mandatory.", cresaWithWarehouse.ErrorPlaceHolderInfo, "Operational port is required.");
		}

		#endregion

		#region TestPortLocationAreaAndServiceReference

		public void TestPortAreaAndLocationValidation()
		{
			var cresa = GetCresa();
			AssertHasMessageErrorContaining("Port Area is required", cresa.PortAreaInfo, $"Port Area is missing from organization {ConsignmentType} > Booking Party > Organization > Config > Registration Numbers/Codes - type PSN (code before '\\')");
			AssertHasMessageErrorContaining("Port Location is required.", cresa.PortLocationInfo, $"Port Location is missing from organization {ConsignmentType} > Booking Party > Organization > Config > Registration Numbers/Codes - type PSN (code after '\\')");

			cresa.PortArea = "Area";
			cresa.PortLocation = "Location";
			cresa.ValidateAllIncludingChildren();
			AssertNoMessageErrors("Port Area should not have error message", cresa.PortAreaInfo);
			AssertNoMessageErrors("Port Location should not have error message", cresa.PortLocationInfo);
		}

		#endregion

		#region TestBookingReference

		public void TestBookingReference()
		{
			var cresa = GetCresa();
			AssertEquals("CarrierBookingReference must be Job ID.", ConsignmentJobID, cresa.CarrierBookingReference);

			cresa.CarrierBookingReference = "";
			AssertHasMessageError("Booking Reference is required.", cresa.CarrierBookingReferenceInfo, $"Booking Reference ({ConsignmentType} ID) is required.");
		}

		#endregion

		#region TestWarehouseEntryNumber

		public void TestWarehouseEntryNumber()
		{
			var cresa = GetCresa();
			AssertEquals("Warehouse Entry Number must be Job ID.", ConsignmentJobID, cresa.EntryNumber);
		}

		#endregion

		#region TestCommodityReference

		public void TestCommodityReference()
		{
			var cresa = GetCresa();

			AssertEquals("Commodity Reference must be Job ID.", ConsignmentJobID, cresa.CommodityReference);
			AssertNoMessageErrors("Commodiy Reference has no message errors", cresa.CommodityReferenceInfo);

			cresa.CommodityReference = "1717171717171717171717171717171717";
			cresa.ValidateAllIncludingChildren();

			AssertHasMessageError("Commodiy Reference max length error.", cresa.CommodityReferenceInfo, $"Commodity Reference ({ConsignmentType} ID) is too long. Maximum 17 characters allowed.");
		}

		public void TestCommodityReference_Empty()
		{
			var cresa = GetCresa();

			AssertEquals("Commodity Reference must be Job ID.", ConsignmentJobID, cresa.CommodityReference);
			AssertNoMessageErrors("Commodiy Reference has no message errors", cresa.CommodityReferenceInfo);

			cresa.CommodityReference = "";
			cresa.ValidateAllIncludingChildren();
			AssertHasMessageError("Commodity Reference is required.", cresa.CommodityReferenceInfo, $"Commodity Reference ({ConsignmentType} ID) is required");
		}

		#endregion

		#region TestShipmentNumber

		public void TestShipmentNumber()
		{
			var cresa = GetCresa();

			AssertEquals("Shipment Number must be Job ID.", ConsignmentJobID, cresa.ShipmentNumber);
			AssertNoMessageErrors("Shipment Number has no message errors.", cresa.ShipmentNumberInfo);

			var shipmentNumberErrorMessage = $"Shipment Number ({ConsignmentType} ID) is too long. Maximum 17 characters allowed.";
			cresa.ShipmentNumber = "01234567890123456789";
			cresa.ValidateAllIncludingChildren();
			AssertHasMessageError("Shipment Number should have error message", cresa.ShipmentNumberInfo, shipmentNumberErrorMessage);
		}

		#endregion

		#region TestGoodsReceiptNotes

		public void TestGoodsReceiptNotes()
		{
			var cresa = GetCresa_GoodsReceiptNotes();
			AssertEquals("GoodsReceiptNotes", "Delivery Order Receipt Notes", cresa.GoodsReceiptNotes);
		}

		#endregion

		#region TestGoodsAreSealed

		public void TestGoodsAreSealed_NoPackages()
		{
			var cresa = GetCresa();
			cresa.ValidateAllIncludingChildren();
			AssertEquals("Goods sealed must be false since there are no packages.", false, cresa.GoodsSealed);
			AssertNoMessageErrors("GoodsSealed is not mandatory.", cresa.GoodsSealedInfo);
		}

		public void TestGoodsAreSealed_PackagesWithNoRTU()
		{
			var cresa = GetCresa_PackagesWithNoRTU();
			cresa.ValidateAllIncludingChildren();
			AssertEquals("Goods sealed must be false since there are no RTUs.", false, cresa.GoodsSealed);
		}

		public void TestGoodsAreSealed_PackagesWithNonUnloadedPackage()
		{
			var cresa = GetCresa_GoodsAreSealed_PackagesWithNonUnloadedPackage();
			AssertEquals("Goods sealed must be false since there are no RTUs.", false, cresa.GoodsSealed);
		}

		public void TestGoodsAreSealed_PackagesWithSingleUnloadedPackage()
		{
			var cresa = GetCresa_GoodsAreSealed_PackagesWithSingleUnloadedPackage();
			AssertEquals("Goods sealed must be True.", true, cresa.GoodsSealed);
		}

		#endregion

		#region Implementation

		#region GetCresa

		protected abstract Cresa GetCresa();
		protected abstract Cresa GetCresa_PackagesWithNoRTU();
		protected abstract Cresa GetCresa_PackagesWithNonGateInRTU();
		protected abstract Cresa GetCresa_PackagesWithSingleGateInRTU();
		protected abstract Cresa GetCresa_PackagesWithMultipleGateInRTU();
		protected abstract Cresa GetCresa_PackagesWithNonUnloadedPackage();
		protected abstract Cresa GetCresa_PackagesWithSingleUnloadedPackage();
		protected abstract Cresa GetCresa_PackagesWithMultipleUnloadedPackages();
		protected abstract Cresa GetCresa_WithPortCode();
		protected abstract Cresa GetCresa_GoodsReceiptNotes();
		protected abstract Cresa GetCresa_GoodsAreSealed_PackagesWithNonUnloadedPackage();
		protected abstract Cresa GetCresa_GoodsAreSealed_PackagesWithSingleUnloadedPackage();

		#endregion

		protected abstract ZString ConsignmentJobID { get; }
		protected abstract ZString SourceType { get; }
		protected abstract ZString ConsignmentType { get; }

		protected ZDateTimeOffset Now => now.IsEmpty ? (now = ZDateTimeOffset.Now) : now;
		ZDateTimeOffset now = ZDateTimeOffset.Empty;

		protected OrgHeader CreateOrganisation(string fullName, string closestPort, string address1, string address2, string city, string postCode, string countryCode)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = fullName;
			org.OH_RL_NKClosestPort = closestPort;
			org.MainAddress.Address1 = address1;
			org.MainAddress.Address2 = address2;
			org.MainAddress.City = city;
			org.MainAddress.Postcode = postCode;
			org.MainAddress.OA_RN_NKCountryCode = countryCode;
			return org;
		}

		protected static Freight.Business.CommunitySystemCodesOfForwarderAndAgent CreateRegistryCode(RefUNLOCO unloco, Freight.Business.CommunitySystemCodesOfForwarderAndAgentCollection registryCodes, string agentCode, string forwarderCode)
		{
			var code = registryCodes.AddNew();
			code.AgentCode = agentCode;
			code.ForwarderCode = forwarderCode;
			code.Port = unloco.Code;
			code.PCS = FrenchPortSystemCodeList.Codes.MGI;
			return code;
		}

		#endregion

		#region Helper

		protected WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper helper;

		#endregion
	}
}
