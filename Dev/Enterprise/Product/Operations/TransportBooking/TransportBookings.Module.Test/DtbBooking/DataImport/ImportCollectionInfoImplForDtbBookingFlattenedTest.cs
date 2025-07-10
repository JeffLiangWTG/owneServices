using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.TransportBookings.Module.Testing
{
	public class ImportCollectionInfoImplForDtbBookingFlattenedTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var info = (IImportCollectionInfo)new ImportCollectionInfoImplForDtbBookingFlattened(new DtbBookingFlattenedCollection(Factory));
			var result = string.Join(System.Environment.NewLine, info.Properties.OrderBy(x => x.MappingName).Select(x => x.MappingName));
			AssertEquals(@"Consolidation_KB_GoodsDescription
Delivery_ConNoteNo
Delivery_KN_DropMode
Delivery_KN_ServiceInstruction
Delivery_Sequence
DeliveryAddress_E2_Address1
DeliveryAddress_E2_Address2
DeliveryAddress_E2_City
DeliveryAddress_E2_CompanyName
DeliveryAddress_E2_Contact
DeliveryAddress_E2_Email
DeliveryAddress_E2_Fax
DeliveryAddress_E2_Mobile
DeliveryAddress_E2_Phone
DeliveryAddress_E2_Postcode
DeliveryAddress_E2_RN_NKCountryCode
DeliveryAddress_E2_State
DeliveryAddress_OH_Code
DeliveryConfirmation_KK_RequiredFrom
DeliveryConfirmation_KK_RequiredTo
KM_IsHazardous
KM_PL_NKCarrierServiceLevel
KM_RequiresRefrigeration
KM_RS_NKServiceLevel
KM_Status
KM_TransportReference
LocalClientAddress_E2_Address1
LocalClientAddress_E2_Address2
LocalClientAddress_E2_City
LocalClientAddress_E2_CompanyName
LocalClientAddress_E2_Postcode
LocalClientAddress_E2_RN_NKCountryCode
LocalClientAddress_E2_State
LocalClientAddress_OH_Code
Pack_KP_DimensionUQ
Pack_KP_F3_NKPackType
Pack_KP_GoodsDescription
Pack_KP_Height
Pack_KP_Length
Pack_KP_PackageID
Pack_KP_PackageQty
Pack_KP_RequiredTemperatureMaximum
Pack_KP_RequiredTemperatureMinimum
Pack_KP_RequiredTemperatureUnit
Pack_KP_RequiresTemperatureControl
Pack_KP_RH_NKCommodityCode
Pack_KP_TransportRef
Pack_KP_Volume
Pack_KP_VolumeUQ
Pack_KP_Weight
Pack_KP_WeightUQ
Pack_KP_Width
Pickup_KN_DropMode
Pickup_KN_ServiceInstruction
Pickup_Sequence
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
PickupConfirmation_KK_RequiredFrom
PickupConfirmation_KK_RequiredTo", result);
		}

		public void TestHeaderText()
		{
			var info = (IImportCollectionInfo)new ImportCollectionInfoImplForDtbBookingFlattened(new DtbBookingFlattenedCollection(Factory));
			AssertPropertyHeaderText(info, "KM_TransportReference", "Transport Reference");
			AssertPropertyHeaderText(info, "Pack_KP_PackageQty", "Package - Package Quantity");
			AssertPropertyHeaderText(info, "Pickup_KN_DropMode", "Pickup - Drop Mode");
			AssertPropertyHeaderText(info, "PickupAddress_E2_Address1", "Pickup - Address Line 1");
			AssertPropertyHeaderText(info, "PickupConfirmation_KK_RequiredFrom", "Pickup - Required From Date");
			AssertPropertyHeaderText(info, "Delivery_KN_DropMode", "Delivery - Drop Mode");
			AssertPropertyHeaderText(info, "DeliveryAddress_E2_Address1", "Delivery - Address Line 1");
			AssertPropertyHeaderText(info, "DeliveryConfirmation_KK_RequiredFrom", "Delivery - Required From Date");
			AssertPropertyHeaderText(info, "LocalClientAddress_E2_Address1", "Local Client - Address Line 1");
			AssertPropertyHeaderText(info, "Consolidation_KB_GoodsDescription", "Consolidation - Goods Desc.");
		}

		void AssertPropertyHeaderText(IImportCollectionInfo info, string propertyName, string expectedHeaderText)
		{
			var headerText = info.Properties.Single(x => x.MappingName == propertyName).HeaderText;
			AssertEquals(expectedHeaderText, headerText);
		}
	}
}
