using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static CargoWise.EventReference.Constants;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class HVLVConsignmentACASUserControlTest : TestCaseWithFactory
	{
		public void TestSendACASAmendmentButton_WhenAmendmentRequired_ThenIsVisible()
		{
			AssertSendACASAmendmentButtonVisibility(ACASActions.Code.SelecteeDataIssueHold, HVLVACASMessageStatusList.Codes.AmendmentRequired, true);
		}

		public void TestSendACASAmendmentButton_IncorrectACASMessageStatus_ThenIsNotVisible()
		{
			AssertSendACASAmendmentButtonVisibility(ACASActions.Code.SelecteeDataIssueHold, HVLVACASMessageStatusList.Codes.OriginalSent, false);
		}

		public void TestSendACASAmendmentButton_IncorrectACASStatus_ThenIsNotVisible()
		{
			AssertSendACASAmendmentButtonVisibility(ACASActions.Code.DoNotLoadHold, HVLVACASMessageStatusList.Codes.AmendmentRequired, false);
		}

		public void TestSendACASAmendmentButton_IncorrectACASStatusAndACASMessageStatus_ThenIsNotVisible()
		{
			AssertSendACASAmendmentButtonVisibility(ACASActions.Code.DoNotLoadHold, HVLVACASMessageStatusList.Codes.OriginalSent, false);
		}

		public void AssertSendACASAmendmentButtonVisibility(string acasStatus, string acasMessageStatus, bool expectedVisibility)
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ACASStatus = acasStatus;
			consignment.HVC_ACASMessageStatus = acasMessageStatus;
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm((HVLVConsignmentHeader)shipment.HVLVConsignmentHeader))
			{
				form.Show();
				form.ConsignmentsGrid.SelectSingleElement(consignment);

				var tabPage = form.Controls.Find("tabPageACAS", true)[0] as ZTabPage;
				tabPage.Show();

				var zSendACASAmendmentButton = form.Controls.Find("zSendACASAmendmentButton", true)[0] as ZButton;

				if (expectedVisibility)
				{
					Assert("Send ACAS Amendment button should be visible", zSendACASAmendmentButton.Visible);
				}
				else
				{
					Assert("Send ACAS Amendment button should not be visible", !zSendACASAmendmentButton.Visible);
				}
			}
		}

		public void TestSendACASAmendmentButton_TriggersValidation_WhenConsignmentHasChanges()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "US");
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);
			GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

			CreateNewValidShipmentWithConsolForACASAmendment();

			TestConsignment.HVC_GoodsDescription = "AMD Graphics Cards";

			using (var form = new ConsignmentUserControlTestForm(TestConsignmentHeader))
			{
				form.Show();
				form.ConsignmentsGrid.SelectSingleElement(TestConsignment);

				var tabPage = form.Controls.Find("tabPageACAS", true)[0] as ZTabPage;
				tabPage.Show();

				var zSendACASAmendmentButton = form.Controls.Find("zSendACASAmendmentButton", true)[0] as ZButton;
				Assert("Pre-condition: Send ACAS Amendment button should be visible", zSendACASAmendmentButton.Visible);
				zSendACASAmendmentButton.PerformClick();
				AssertEquals("Consignment should be saved before sending ACAS Amendment", @"Please save the form before sending ACAS Amendment.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendACASAmendmentButton_TriggersValidation_NoACASCodeSet()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);

			CreateNewValidShipmentWithConsolForACASAmendment();

			using (var form = new ConsignmentUserControlTestForm(TestConsignmentHeader))
			{
				form.Show();
				form.ConsignmentsGrid.SelectSingleElement(TestConsignment);

				var tabPage = form.Controls.Find("tabPageACAS", true)[0] as ZTabPage;
				tabPage.Show();

				var zSendACASAmendmentButton = form.Controls.Find("zSendACASAmendmentButton", true)[0] as ZButton;
				Assert("Pre-condition: Send ACAS Amendment button should be visible", zSendACASAmendmentButton.Visible);

				zSendACASAmendmentButton.PerformClick();
				AssertEquals("ACAS code needs to be set before sending ACAS messages", @"Sender's ACAS code is required for ACAS messaging. Raise an eRequest to register your interest. Once provided by WTG, enter the code against the Branch or Company Organization Proxy > Config > Registration Numbers/Codes tab using Type = US ACA.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendACASAmendmentButton_TriggersValidation_ChecksForMessageErrors()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "US");
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);
			GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

			CreateNewValidShipmentWithConsolForACASAmendment(false);

			using (var form = new ConsignmentUserControlTestForm(TestConsignmentHeader))
			{
				form.Show();
				form.ConsignmentsGrid.SelectSingleElement(TestConsignment);

				var tabPage = form.Controls.Find("tabPageACAS", true)[0] as ZTabPage;
				tabPage.Show();

				var zSendACASAmendmentButton = form.Controls.Find("zSendACASAmendmentButton", true)[0] as ZButton;
				Assert("Pre-condition: Send ACAS Amendment button should be visible", zSendACASAmendmentButton.Visible);

				var wrapper = new HVLVConsignmentForACASWrapper(TestConsignment);
				wrapper.RunPreSaveValidation();
				Assert("Pre-condition: TestConsignment has message errors", wrapper.HasMessageErrors);

				zSendACASAmendmentButton.PerformClick();
				AssertEquals("Message errors need to be fixed before sending ACAS messages", @"There are message errors that need to be corrected before sending ACAS Amendment.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendACASAmendmentButton_SendsAmendment()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "US");
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.UnitedStates);
			GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

			CreateNewValidShipmentWithConsolForACASAmendment();

			using (var form = new ConsignmentUserControlTestForm(TestConsignmentHeader))
			{
				form.Show();
				form.ConsignmentsGrid.SelectSingleElement(TestConsignment);

				var tabPage = form.Controls.Find("tabPageACAS", true)[0] as ZTabPage;
				tabPage.Show();

				var zSendACASAmendmentButton = form.Controls.Find("zSendACASAmendmentButton", true)[0] as ZButton;
				Assert("Pre-condition: Send ACAS Amendment button should be visible", zSendACASAmendmentButton.Visible);

				zSendACASAmendmentButton.PerformClick();
				AssertEquals("ACAS amendment message successfully sent", "ACAS amendment message successfully sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Consignment's ACAS message status is now AST", HVLVACASMessageStatusList.Codes.AmendmentSent, TestConsignment.HVC_ACASMessageStatus);
			}
		}

		void CreateNewValidShipmentWithConsolForACASAmendment(bool setConsignmentShipper = true)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "CTO";
			orgHeader.OH_RL_NKClosestPort = "USLAX";
			orgHeader.MainAddress.Address1 = "House 16777214";
			orgHeader.MainAddress.Address2 = "Coelosis inermis";
			orgHeader.MainAddress.City = "The Big City";
			orgHeader.MainAddress.Postcode = "1234";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "US";
			orgHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "123", "US");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ArrivalCTOAddress = orgHeader.MainAddress.PK;
			shipment.Consols.Add(consol);

			TestConsignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			TestConsignment = TestConsignmentHeader.Consignments.AddNew();
			TestConsignment.HVC_ConsigneeName = "John Wick";
			TestConsignment.HVC_ConsigneeAddress1 = "House 16777214";
			TestConsignment.HVC_ConsigneeAddress2 = "Coelosis inermis";
			TestConsignment.HVC_ConsigneeCity = "The Big City";
			TestConsignment.HVC_ConsigneePostcode = "1234";
			TestConsignment.HVC_RN_NKConsigneeCountryCode = "US";
			TestConsignment.HVC_ConsigneeState = "LAS";
			TestConsignment.HVC_ConsigneeMobile = "+0123456789";
			TestConsignment.HVC_ConsigneeEmail = "JohnWick@email.com";
			TestConsignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			if (setConsignmentShipper)
			{
				TestConsignment.HVC_ShipperName = "Test Company";
				TestConsignment.HVC_ShipperAddress1 = "Test Address";
				TestConsignment.HVC_RN_NKShipperCountryCode = "AU";
				TestConsignment.HVC_ShipperState = "NSW";
				TestConsignment.HVC_ShipperPostcode = "123456";
				TestConsignment.HVC_ShipperCity = "Sydney";
				TestConsignment.HVC_ShipperMobile = "+9876543210";
				TestConsignment.HVC_ShipperEmail = "TestCompany@email.com";
			}

			TestConsignment.HVC_GoodsDescription = "NVIDIA Graphics Cards";
			TestConsignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			TestConsignment.HVC_ACASStatus = ACASActions.Code.SelecteeDataIssueHold;
			TestConsignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;

			var item = TestConsignment.Items.AddNew();
			item.HVI_GoodsDescription = "RTX 3090";
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();
		}

		HVLVConsignment TestConsignment { get; set; }

		HVLVConsignmentHeader TestConsignmentHeader { get; set; }
	}
}
