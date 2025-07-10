using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class BillProviderTest : TestCaseWithFactory
	{
		public void TestBillMembers()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);
				var billofLading = sumDec.BillofLadings.ToArray();
				CombineAssertions("Bill Provider Test", () =>
				{
					AssertEquals("ContainerAgentName", ZString.Empty, billofLading[0].ContainerAgentName);
					AssertEquals("1234567892", billofLading[0].ContainerAgentRegNo);
					AssertEquals("xConsignee Name", billofLading[0].ConsigneeName);
					AssertEquals("ConsigneeRegNo is selected to Order", "emre", billofLading[0].ConsigneeRegNo);
					AssertEquals(TurkishConstants.AnswerNo, billofLading[0].IsWarehouseExternal);
					AssertEquals("xNotify Name", billofLading[0].NotifyPartyName);
					AssertEquals("1234567892", billofLading[0].NotifyPartyRegNo);
					AssertEquals("052", billofLading[0].Origin);
					AssertEquals(ZString.Empty, billofLading[0].SafetySecurityT);
					AssertEquals("GEMI", billofLading[0].GoodsLocation);
					AssertEquals("USD", billofLading[0].TransportValueCurrency);
					AssertEquals(1500, Convert.ToInt32(billofLading[0].TransportTotalValue));
					AssertEquals("xShipper Name", billofLading[0].ShipperName);
					AssertEquals("1234567893", billofLading[0].ShipperRegNo);
					AssertEquals(TurkishConstants.AnswerNo, billofLading[0].IsGroup);
					AssertEquals(TurkishConstants.AnswerYes, billofLading[0].Iscontainer);
					AssertEquals(ZString.Empty, billofLading[0].NKFreightValueCurrency);
					AssertEquals(ZDecimal.Zero, billofLading[0].FreightValue);
					AssertEquals("B", billofLading[0].PaymentType);
					AssertEquals(ZString.Empty, billofLading[0].PreviousVoyageNo);
					AssertEquals(ZDateTime.Empty, billofLading[0].PreviousVoyageArrivalDate);
					AssertEquals("55555", billofLading[0].EntryNumber);
					AssertEquals(TurkishConstants.AnswerNo, billofLading[0].IsRoro);
					AssertEquals("1", billofLading[0].SequenceNo);
					AssertEquals(TurkishConstants.AnswerYes, billofLading[0].IsTransshipmentType);
					AssertEquals("Yurtiçi Aktarma", billofLading[0].TransshipmentType);
					AssertEquals("ABC111222333", billofLading[0].BillNumber);
					AssertEquals("ConsigneeRegNo is not selected to Order", "sahip değildir", billofLading[1].ConsigneeRegNo);
					AssertEquals("ConsigneeRegNo is Tax Number", "1234567890", billofLading[2].ConsigneeRegNo);
					AssertEquals("ContainerAgentName with TaxID", "xContainerAgentName2", billofLading[3].ContainerAgentName);
					AssertEquals("ContainerAgentRegNo", ZString.Empty, billofLading[3].ContainerAgentRegNo);
				});
			}
		}

		public void TestSendWithIsWarehouseExternalAndIsGroup()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				header.AMA_TransportMode = TransportTypeList.Codes.Air;
				CombineAssertions("Bill Provider IsWarehouseExternalAndIsGroup Test", () =>
				{
					var sumDec = new ManifestMessageProvider(header);
					var billofLading = sumDec.BillofLadings.ToArray();
					AssertEquals("IsGroup | No", TurkishConstants.AnswerNo, billofLading[0].IsGroup);
					AssertEquals("IsWarehouseExternal | No", TurkishConstants.AnswerNo, billofLading[0].IsWarehouseExternal);
					header.Bills[0].ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
					header.Bills[0].ABL_SpecialCargoCode = Universal.CodeDescriptionPairLists.YesNoList.Codes.Yes;
					sumDec = new ManifestMessageProvider(header);
					billofLading = sumDec.BillofLadings.ToArray();
					AssertEquals("IsGroup | Yes", TurkishConstants.AnswerYes, billofLading[0].IsGroup);
					AssertEquals("IsWarehouseExternal | Yes", TurkishConstants.AnswerYes, billofLading[0].IsWarehouseExternal);
				});
			}
		}

		public void TestXMLSendingVisibilityForFreightValueAndNKFreightValueCurrency()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);
				var billofLading = sumDec.BillofLadings.ToArray();
				header.AMA_TransportMode = TransportTypeList.Codes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				CombineAssertions("FreightAmount and NKFretightValueCurrency Xml Nodes Should Be Filled", () =>
				{
					AssertEquals(500m, billofLading[0].FreightValue);
					AssertEquals("EUR", billofLading[0].NKFreightValueCurrency);
				});
				header.AMA_TransportMode = TransportTypeList.Codes.Sea;
				CombineAssertions("FreightAmount and NKFretightValueCurrency Xml Nodes Should Be 0", () =>
				{
					AssertEquals(0m, billofLading[0].FreightValue);
					AssertEquals("", billofLading[0].NKFreightValueCurrency);
				});
			}
		}
	}
}
