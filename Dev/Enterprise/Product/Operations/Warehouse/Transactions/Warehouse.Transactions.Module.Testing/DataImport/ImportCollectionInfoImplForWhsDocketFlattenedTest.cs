using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class ImportCollectionInfoImplForWhsDocketFlattenedTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var info = (IImportCollectionInfo)new ImportCollectionInfoImplForWhsDocketFlattened(new WhsDocketFlattenedCollection());
			var result = string.Join(System.Environment.NewLine, info.Properties.OrderBy(x => x.MappingName).Select(x => x.MappingName));
			AssertEquals(@"Client_OH_Code
ConsigneeAddress_E2_Address1
ConsigneeAddress_E2_Address2
ConsigneeAddress_E2_City
ConsigneeAddress_E2_CompanyName
ConsigneeAddress_E2_Contact
ConsigneeAddress_E2_Email
ConsigneeAddress_E2_Fax
ConsigneeAddress_E2_Mobile
ConsigneeAddress_E2_Phone
ConsigneeAddress_E2_Postcode
ConsigneeAddress_E2_RN_NKCountryCode
ConsigneeAddress_E2_State
ConsigneeAddress_OH_Code
DropOffAddress_E2_Address1
DropOffAddress_E2_Address2
DropOffAddress_E2_City
DropOffAddress_E2_CompanyName
DropOffAddress_E2_Contact
DropOffAddress_E2_Email
DropOffAddress_E2_Fax
DropOffAddress_E2_Mobile
DropOffAddress_E2_Phone
DropOffAddress_E2_Postcode
DropOffAddress_E2_RN_NKCountryCode
DropOffAddress_E2_State
DropOffAddress_OH_Code
Forwarder_OH_Code
Line_WE_ClientOrderedUnits
Line_WE_ExpiryDate
Line_WE_ExtendedLinePrice
Line_WE_F3_NKPackType
Line_WE_LineComment
Line_WE_LineNo
Line_WE_PackageGroupId
Line_WE_PackingDate
Line_WE_PalletID
Line_WE_PartAttrib1
Line_WE_PartAttrib2
Line_WE_PartAttrib3
Line_WE_PerPackageQty
Line_WE_RecommendedUnitPrice
Line_WE_RequiredByDate
Line_WE_RX_NKUnitPriceCurrency
Line_WE_SerialNumber
Line_WE_StockOnHand
Line_WE_SubLineNo
Line_WE_TransactionQuantity
Line_WE_UnitDiscountAmount
Line_WE_UnitDiscountPercent
LinePart_OP_PartNum
PickupAddress_E2_Address1
PickupAddress_E2_Address2
PickupAddress_E2_City
PickupAddress_E2_CompanyName
PickupAddress_E2_Contact
PickupAddress_E2_Email
PickupAddress_E2_Fax
PickupAddress_E2_Mobile
PickupAddress_E2_Phone
PickupAddress_E2_Postcode
PickupAddress_E2_RN_NKCountryCode
PickupAddress_E2_State
PickupAddress_OH_Code
SupplierAddress_E2_Address1
SupplierAddress_E2_Address2
SupplierAddress_E2_City
SupplierAddress_E2_CompanyName
SupplierAddress_E2_Contact
SupplierAddress_E2_Email
SupplierAddress_E2_Fax
SupplierAddress_E2_Mobile
SupplierAddress_E2_Phone
SupplierAddress_E2_Postcode
SupplierAddress_E2_RN_NKCountryCode
SupplierAddress_E2_State
SupplierAddress_OH_Code
Warehouse_WW_WarehouseCode
WD_ArrivalDate
WD_BookingDate
WD_CODPayMethod
WD_ContainerMode
WD_CustomAttrib1
WD_CustomAttrib2
WD_CustomAttrib3
WD_CustomAttrib4
WD_CustomAttrib5
WD_CustomDate1
WD_CustomDate2
WD_CustomDecimal1
WD_CustomDecimal2
WD_CustomDecimal3
WD_CustomDecimal4
WD_CustomDecimal5
WD_CustomerReference
WD_CustomFlag1
WD_CustomFlag2
WD_CustomFlag3
WD_CustomFlag4
WD_CustomFlag5
WD_DropMode
WD_ETA
WD_ETD
WD_ExternalReference
WD_F3_NKTotalPackType
WD_FinalisedDate
WD_GoodsDescription
WD_INCO
WD_PL_NKCarrierServiceLevel
WD_RequiredDate
WD_RS_NKServiceLevel
WD_RX_NKTotalOrderCurrency
WD_TotalCubic
WD_TotalCubicUnit
WD_TotalOrderValue
WD_TotalPallets
WD_TotalUnits
WD_TotalWeight
WD_TotalWeightUnit
WD_TransportMode
WD_TransportReference", result);
		}

		public void TestHeaderText()
		{
			var info = (IImportCollectionInfo)new ImportCollectionInfoImplForWhsDocketFlattened(new WhsDocketFlattenedCollection());
			AssertPropertyHeaderText(info, "WD_TransportReference", "Transport Reference");
			AssertPropertyHeaderText(info, "Warehouse_WW_WarehouseCode", "Warehouse - Code");
			AssertPropertyHeaderText(info, "PickupAddress_E2_Address1", "Pickup - Address Line 1");
			AssertPropertyHeaderText(info, "SupplierAddress_E2_Address1", "Supplier - Address Line 1");
			AssertPropertyHeaderText(info, "Forwarder_OH_Code", "Forwarder - Organization Code");
			AssertPropertyHeaderText(info, "Line_WE_ClientOrderedUnits", "Line - Client Ordered Units");
		}

		void AssertPropertyHeaderText(IImportCollectionInfo info, string propertyName, string expectedHeaderText)
		{
			var headerText = info.Properties.Single(x => x.MappingName == propertyName).HeaderText;
			AssertEquals(expectedHeaderText, headerText);
		}
	}
}
