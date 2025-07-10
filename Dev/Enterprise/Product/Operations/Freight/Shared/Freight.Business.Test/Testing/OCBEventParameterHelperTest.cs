using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class OCBEventParameterHelperTest : TestCaseWithFactory
	{
		public void TestGetSCACShippingLine()
		{
			var consol = CreateConsol(true);

			var scac = OCBEventParameterHelper.GetSCAC(consol);

			AssertEquals("SCAC should be retrieved from shipping line.", "2222", scac);
		}

		public void TestGetSCACNoShippingLine()
		{
			var consol = CreateConsol(false);

			var scac = OCBEventParameterHelper.GetSCAC(consol);

			AssertEquals("SCAC should be retrieved from customs code.", "9876", scac);
		}

		public void TestGetRefShippingLineFromCarrierWithFallback()
		{
			var consol = CreateConsol(false);

			var refShippingLine = OCBEventParameterHelper.GetRefShippingLineFromCarrierWithFallback(consol);

			AssertNotNull(refShippingLine);
			AssertEquals("CarrierName of ShippingLine not linked to Carrier", "Not linked Carrier", refShippingLine.RSL_CarrierName);
		}

		public void TestGetRefShippingLineFromCarrierWithFallback_WithDuplicateRSL_CargoWiseOneCode()
		{
			ErrorReporter.Clear();

			var consol = CreateConsol(false, OrgCusCode.CodeTypes.CargoWiseOneCarrierCode);

			var shippingLineInactive = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLineInactive.RSL_IsActive = false;
			shippingLineInactive.RSL_CarrierName = "Inactive Shipping";
			shippingLineInactive.RSL_StandardCarrierAlphaCode = "2222";
			shippingLineInactive.RSL_CargoWiseOneCode = "9876";

			var shippingLineActive = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLineActive.RSL_IsActive = true;
			shippingLineActive.RSL_CarrierName = "Active Shipping";
			shippingLineActive.RSL_StandardCarrierAlphaCode = "2222";
			shippingLineActive.RSL_CargoWiseOneCode = "9876";

			var refShippingLine = OCBEventParameterHelper.GetRefShippingLineFromCarrierWithFallback(consol);

			AssertNotNull(refShippingLine);
			CombineAssertions(() =>
			{
				AssertEquals("Should receive active RefShippingLine", shippingLineActive.RSL_CarrierName, refShippingLine.RSL_CarrierName);
				AssertEquals("No errors should be reported", 0, ErrorReporter.ExceptionsThrown.Count);
			});
		}

		#region Implementation

		CommonConsol CreateConsol(bool createShippingLine, string orgCustomsCodeType = OrgCusCode.CodeTypes.CarrierCode)
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.AgentConsol;
			consol.JK_UniqueConsignRef = "CONSOL0001";
			consol.JK_AgentsReference = "AgentRef002";
			consol.JK_MasterBillNum = "1112222222";
			consol.JK_CoLoadMasterBill = "COLOAD004";
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "USARD";
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.JK_ReleaseType = ZString.Empty;
			consol.JK_NoCopyBills = 3;
			consol.JK_NoOriginalBills = 4;
			consol.JK_BookingReference = "BOOKINGREF01";
			consol.JK_CoLoadBookingReference = "COLOADREF02";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 2, 19);

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAERSK";
			org.OH_RL_NKClosestPort = "DKAAL";
			org.MainAddress.Address1 = "Unit 13";
			org.MainAddress.Address2 = "4 Lost Lane";
			org.MainAddress.City = "Aalborg";
			org.MainAddress.Postcode = "2000";
			org.MainAddress.OA_RN_NKCountryCode = "DK";
			org.OH_IsShippingProvider = true;
			org.OH_IsShippingLine = true;
			org.OH_IsSeaWholesaler = false;

			if (createShippingLine)
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_StandardCarrierAlphaCode = "2222";
				shippingLine.RSL_ShippingOrderAvailable = true;
				shippingLine.RSL_IsNVO = false;

				org.OH_RSL_ShippingLine = shippingLine.PK;
			}

			var customscode = org.CustomsCodes.AddNew();
			customscode.OK_RN_NKCodeCountry = "US";
			customscode.OK_CodeType = orgCustomsCodeType;
			customscode.OK_CustomsRegNo = "9876";

			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;

			var notLinkedShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			notLinkedShippingLine.RSL_CarrierName = "Not linked Carrier";
			notLinkedShippingLine.RSL_StandardCarrierAlphaCode = "9876";
			notLinkedShippingLine.RSL_ShippingOrderAvailable = true;
			notLinkedShippingLine.RSL_IsNVO = false;

			return consol;
		}

		#endregion
	}
}
