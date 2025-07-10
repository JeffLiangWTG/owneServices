using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CommercialEntityTypeListTest : TestCase
	{
		public void TestGetCodeFromDocAddressType()
		{
			AssertEquals(CommercialEntityTypeList.Codes.BookingParty, CommercialEntityTypeList.GetCodeFromDocAddressType(AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress));
			AssertEquals(CommercialEntityTypeList.Codes.BuyingParty, CommercialEntityTypeList.GetCodeFromDocAddressType(AutoDocAddressTypes.Codes.BuyingParty));
			AssertEquals(CommercialEntityTypeList.Codes.Consignee, CommercialEntityTypeList.GetCodeFromDocAddressType(AutoDocAddressTypes.Codes.ConsigneeAddress));
			AssertEquals(CommercialEntityTypeList.Codes.Consolidator, CommercialEntityTypeList.GetCodeFromDocAddressType(AutoDocAddressTypes.Codes.Consolidator));
			AssertEquals(CommercialEntityTypeList.Codes.Manufacturer, CommercialEntityTypeList.GetCodeFromDocAddressType(AutoDocAddressTypes.Codes.Manufacturer));
			AssertEquals(CommercialEntityTypeList.Codes.SellingParty, CommercialEntityTypeList.GetCodeFromDocAddressType(AutoDocAddressTypes.Codes.SellingParty));
			AssertEquals(CommercialEntityTypeList.Codes.ShipToParty, CommercialEntityTypeList.GetCodeFromDocAddressType(AutoDocAddressTypes.Codes.ShipToParty));
			AssertEquals(CommercialEntityTypeList.Codes.StuffingLocation, CommercialEntityTypeList.GetCodeFromDocAddressType(AutoDocAddressTypes.Codes.ScheduledContainerStuffingLocation));
		}
	}
}
