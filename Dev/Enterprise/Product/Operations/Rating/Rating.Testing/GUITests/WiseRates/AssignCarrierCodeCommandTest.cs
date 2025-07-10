using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.Test
{
	public class AssignCarrierCodeCommandTest : BaseRatingIntegrationTest
	{
		public void TestIsEnabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CARRIER1";
			org.OH_IsShippingProvider = true;
			Factory.Save();

			var wiseEntryViewWithSCACMapped = CreateWiseEntryView("ZZZZ");
			((WiseHeader)wiseEntryViewWithSCACMapped.ParentRatingHeader).TH_OH = org.PK;
			var wiseEntryViewWithC1CCMapped = CreateWiseEntryView(null, "C1CC");
			((WiseHeader)wiseEntryViewWithC1CCMapped.ParentRatingHeader).TH_OH = org.PK;

			var wiseEntryViewC1COnlyNoMap = CreateWiseEntryView(null, "C1CC");
			var wiseEntryViewSCACOnlyNoMap = CreateWiseEntryView("ZZZZ");

			var command = new AssignCarrierCodeCommand() as IWiseRatesCommand;

			AssertEquals("SCAC-mapped entry should not be enabled for assigning carrier code.", false, command.IsEnabled(wiseEntryViewWithSCACMapped));
			AssertEquals("C1CC-mapped entry should not be enabled for assigning carrier code.", false, command.IsEnabled(wiseEntryViewWithC1CCMapped));

			AssertEquals("C1C-only, no map entry should be enabled for assigning carrier code.", true, command.IsEnabled(wiseEntryViewC1COnlyNoMap));
			AssertEquals("SCAC-only, no map entry should be enabled for assigning carrier code.", true, command.IsEnabled(wiseEntryViewSCACOnlyNoMap));
		}

		public void TestAssign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CARRIER1";
			org.OH_IsShippingProvider = true;

			// existing shipping line with same codes but not linked with org
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "ZZZZ";
			shippingLine.RSL_CargoWiseOneCode = "C1AA";

			Factory.Save();

			var wiseEntryView = CreateWiseEntryView("ZZZZ", "C1AA");
			var cmd = new AssignCarrierCodeCommandForTest(org);

			UnitTestUserNotification.Instance.AddYesAnswer();
			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals(
				"Expected confirmation message during assignment operation.",
				"During the operation SCAC/C1C ZZZZ will be assigned to the carrier CARRIER1. Do you want to proceed?",
				UnitTestUserNotification.Instance.PreviousMessages[1].Text
			);

			AssertEquals(
				"Expected final message after assignment operation.",
				"SCAC/C1C ZZZZ is now assigned to Carrier CARRIER1.",
				UnitTestUserNotification.Instance.LastMessage.Text
			);

			AssertEquals("Assignment result should be true.", true, assignmentResult);
			AssertEquals("The organization should now have the shipping line assigned.", shippingLine.PK, org.OH_RSL_ShippingLine);
		}

		public void TestAssign_OnlySCACFromRate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CARRIER1";
			org.OH_IsShippingProvider = true;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "ZZZZ";
			shippingLine.RSL_CargoWiseOneCode = "XXXX";

			Factory.Save();

			var wiseEntryView = CreateWiseEntryView("ZZZZ");
			var cmd = new AssignCarrierCodeCommandForTest(org);

			UnitTestUserNotification.Instance.AddYesAnswer();
			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals(
				"Expected message during assignment operation not shown.",
				"During the operation SCAC/C1C ZZZZ will be assigned to the carrier CARRIER1. Do you want to proceed?",
				UnitTestUserNotification.Instance.PreviousMessages[1].Text
			);

			AssertEquals(
				"Expected final confirmation message not shown.",
				"SCAC/C1C ZZZZ is now assigned to Carrier CARRIER1.",
				UnitTestUserNotification.Instance.LastMessage.Text
			);

			AssertEquals("Assignment result should be true.", true, assignmentResult);
			AssertEquals("Shipping line PK should be assigned to OrgHeader.", shippingLine.PK, org.OH_RSL_ShippingLine);
			AssertEquals("Shipping line StandardCarrierAlphaCode mismatched.", "ZZZZ", org.ShippingLine.RSL_StandardCarrierAlphaCode);
			AssertEquals("Shipping line CargoWiseOneCode mismatched.", "XXXX", org.ShippingLine.RSL_CargoWiseOneCode);
		}

		public void TestAssign_OnlyC1CFromRate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CARRIER1";
			org.OH_IsShippingProvider = true;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "ZZZZ";
			shippingLine.RSL_CargoWiseOneCode = "C1CC";

			Factory.Save();

			var wiseEntryView = CreateWiseEntryView(null, "C1CC");
			var cmd = new AssignCarrierCodeCommandForTest(org);

			UnitTestUserNotification.Instance.AddYesAnswer();
			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals(
				"Unexpected notification message during SCAC/C1C assignment",
				"During the operation SCAC/C1C C1CC will be assigned to the carrier CARRIER1. Do you want to proceed?",
				UnitTestUserNotification.Instance.PreviousMessages[1].Text
			);

			AssertEquals(
				"Unexpected final notification message after SCAC/C1C assignment",
				"SCAC/C1C C1CC is now assigned to Carrier CARRIER1.",
				UnitTestUserNotification.Instance.LastMessage.Text
			);

			AssertEquals("Assignment result is expected to be true", true, assignmentResult);
			AssertEquals("Unexpected shipping line assignment in OrgHeader", shippingLine.PK, org.OH_RSL_ShippingLine);
			AssertEquals(
				"Unexpected StandardCarrierAlphaCode in OrgHeader's shipping line",
				"ZZZZ",
				org.ShippingLine.RSL_StandardCarrierAlphaCode
			);
			AssertEquals(
				"Unexpected CargoWiseOneCode in OrgHeader's shipping line",
				"C1CC",
				org.ShippingLine.RSL_CargoWiseOneCode
			);
		}

		public void TestAssign_ShippingLineMatchingSCACPriorToC1C()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CARRIER1";
			org.OH_IsShippingProvider = true;

			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "XXXX";
			shippingLine1.RSL_CargoWiseOneCode = "C1C1";

			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "YYYY";
			shippingLine2.RSL_CargoWiseOneCode = "C1C2";

			var shippingLine3 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine3.RSL_StandardCarrierAlphaCode = "ZZZZ";
			shippingLine3.RSL_CargoWiseOneCode = "YYYY";

			Factory.Save();

			var wiseEntryView = CreateWiseEntryView("YYYY");
			var cmd = new AssignCarrierCodeCommandForTest(org);

			UnitTestUserNotification.Instance.AddYesAnswer();
			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals(
				"Expected notification message for SCAC/C1C assignment to the carrier.",
				"During the operation SCAC/C1C YYYY will be assigned to the carrier CARRIER1. Do you want to proceed?",
				UnitTestUserNotification.Instance.PreviousMessages[1].Text
			);

			AssertEquals(
				"Expected final notification message indicating successful assignment.",
				"SCAC/C1C YYYY is now assigned to Carrier CARRIER1.",
				UnitTestUserNotification.Instance.LastMessage.Text
			);

			AssertEquals("Assignment result should be true.", true, assignmentResult);
			AssertEquals("Expected assigned shipping line primary key.", shippingLine2.PK, org.OH_RSL_ShippingLine);
			AssertEquals("Expected standard carrier alpha code of assigned shipping line.", "YYYY", org.ShippingLine.RSL_StandardCarrierAlphaCode);
			AssertEquals("Expected CargoWiseOne code of assigned shipping line.", "C1C2", org.ShippingLine.RSL_CargoWiseOneCode);
		}

		public void TestAssign_ShippingLineMatchingC1CWhenSCACDoesNotMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CARRIER1";
			org.OH_IsShippingProvider = true;

			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "XXXX";
			shippingLine1.RSL_CargoWiseOneCode = "C1C1";

			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "YYYY";
			shippingLine2.RSL_CargoWiseOneCode = "C1C2";

			Factory.Save();

			var wiseEntryView = CreateWiseEntryView("ZZZZ", "C1C2");
			var cmd = new AssignCarrierCodeCommandForTest(org);

			UnitTestUserNotification.Instance.AddYesAnswer();
			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals(
				"Expected confirmation message when assigning SCAC/C1C C1C2 to carrier CARRIER1.",
				"During the operation SCAC/C1C C1C2 will be assigned to the carrier CARRIER1. Do you want to proceed?",
				UnitTestUserNotification.Instance.PreviousMessages[1].Text
			);

			AssertEquals(
				"Expected success message after assigning SCAC/C1C C1C2 to carrier CARRIER1.",
				"SCAC/C1C C1C2 is now assigned to Carrier CARRIER1.",
				UnitTestUserNotification.Instance.LastMessage.Text
			);

			AssertEquals(true, assignmentResult);
			AssertEquals(
				"Expected shipping line ID to match shippingLine2.",
				shippingLine2.PK,
				org.OH_RSL_ShippingLine
			);

			AssertEquals(
				"Expected StandardCarrierAlphaCode to match shippingLine2.",
				"YYYY",
				org.ShippingLine.RSL_StandardCarrierAlphaCode
			);

			AssertEquals(
				"Expected CargoWiseOneCode to match shippingLine2.",
				"C1C2",
				org.ShippingLine.RSL_CargoWiseOneCode
			);
		}

		public void TestAssign_NoOrgSelected_ThenNoAssignment()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "ZZZZ";
			shippingLine.RSL_CargoWiseOneCode = "C1C1";

			Factory.Save();

			var wiseEntryView = CreateWiseEntryView("ZZZZ");
			var cmd = new AssignCarrierCodeCommandForTest();

			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertNull("Cancel early. No confirmation.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("The assignment result should be false.", false, assignmentResult);
		}

		public void TestAssign_NotConfirm_ThenNoAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CARRIER1";
			org.OH_IsShippingProvider = true;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "ZZZZ";
			shippingLine.RSL_CargoWiseOneCode = "C1C1";

			Factory.Save();

			var wiseEntryView = CreateWiseEntryView("ZZZZ");
			var cmd = new AssignCarrierCodeCommandForTest(org);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals(
				"Expected notification message to match the specified text.",
				"During the operation SCAC/C1C ZZZZ will be assigned to the carrier CARRIER1. Do you want to proceed?", 
				UnitTestUserNotification.Instance.LastMessage.Text
			);

			AssertEquals("Expected assignment result to be False.", false, assignmentResult);
			AssertEquals("Expected shipping line reference to be empty.", ZGuid.Empty, org.OH_RSL_ShippingLine);
		}

		public void TestAssign_NoOrgEditSecurity_ThenNoAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CARRIER1";
			org.OH_IsShippingProvider = true;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "ZZZZ";
			shippingLine.RSL_CargoWiseOneCode = "C1C1";

			Factory.Save();

			var wiseEntryView = CreateWiseEntryView("ZZZZ");

			Env.Security.OrganisationModify.IsAllowed = false;

			var cmd = new AssignCarrierCodeCommandForTest(org);
			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals("Assignment should be false when OrganisationModify security is not allowed.", false, assignmentResult);
			AssertEquals("Expected LoginForm to be shown.", "Enterprise.Rating.GUI.LoginForm", ZFormModaliser.LastFormShownDialogForTest.GetType().FullName);
			AssertEquals("Shipping line should not be assigned when OrganisationModify security is not allowed.", ZGuid.Empty, org.OH_RSL_ShippingLine);
		}

		public void TestAssign_NoShippingLineMatching_ThenNoAssignment()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "CARRIER1";
			org.OH_IsShippingProvider = true;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "XXXX";
			shippingLine.RSL_CargoWiseOneCode = "C1C1";

			Factory.Save();

			var wiseEntryView = CreateWiseEntryView("ZZZZ", "C1ZZ");
			var cmd = new AssignCarrierCodeCommandForTest(org);

			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals(
				"A Shipping Line with SCAC/C1C 'ZZZZ/C1ZZ' could not be found. Please raise eRequest for support.",
				UnitTestUserNotification.Instance.LastMessage.Text
			);

			AssertEquals(false, assignmentResult);

			AssertEquals(ZGuid.Empty, org.OH_RSL_ShippingLine);
		}

		public void TestAssign_OrgWithNoShippingLineFound_ThenAssignment()
		{
			var usedShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			usedShippingLine.RSL_StandardCarrierAlphaCode = "SCA1";
			usedShippingLine.RSL_CargoWiseOneCode = "C111";

			var unusedShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			unusedShippingLine.RSL_StandardCarrierAlphaCode = "SCA2";
			unusedShippingLine.RSL_CargoWiseOneCode = "C122";

			var orgWithShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			orgWithShippingLine.OH_FullName = "Has Shipping Line";
			orgWithShippingLine.OH_Code = "HasLine";
			orgWithShippingLine.OH_IsShippingProvider = true;
			orgWithShippingLine.OH_IsShippingLine = true;
			orgWithShippingLine.OH_RSL_ShippingLine = usedShippingLine.PK;

			var orgWithoutShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			orgWithoutShippingLine.OH_FullName = "Has no Shipping Line";
			orgWithoutShippingLine.OH_Code = "NoLine";
			orgWithoutShippingLine.OH_IsShippingProvider = true;
			orgWithoutShippingLine.OH_IsShippingLine = true;

			Factory.Save();

			var orgWithoutShippingLineFound = false;
			var orgWithShippingLineFound = false;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(delegate (object dialog)
			{
				var embeddedPopup = ((EmbeddedModulePopup)dialog);
				var module = embeddedPopup.Module_ForTest;
				var filter = module.FilterBusinessObject.Filter;
				var decisionProvider = embeddedPopup.Module_ForTest.ModuleDecisionProvider;

				var organisationsFound = Factory.Load<OrgHeader>(filter);

				// Need to move the assert out of this delegate or the test will never end
				// if the assert fails (because exception gets thrown and now decisionProvider
				// makes a selection)
				orgWithShippingLineFound = organisationsFound.Contains(orgWithShippingLine);
				orgWithoutShippingLineFound = organisationsFound.Contains(orgWithoutShippingLine);

				decisionProvider.HandleDefaultAction(new[] { orgWithoutShippingLine });
			});

			var wiseEntryView = CreateWiseEntryView("SCA2", "C122");
			var cmd = new AssignCarrierCodeCommand();
			var assignmentResult = ((IWiseRatesCommand)cmd).Assign(wiseEntryView, null);

			AssertEquals("Assignment result should be true", true, assignmentResult);
			AssertEquals("Org without shipping line should be assigned the unused shipping line", unusedShippingLine.PK, orgWithoutShippingLine.OH_RSL_ShippingLine);

			Assert("Should not show org with shipping line", !orgWithShippingLineFound);
			Assert("Should show org without shipping line", orgWithoutShippingLineFound);
		}

		WiseEntryView CreateWiseEntryView(string scacCode, string c1cCode = null)
		{
			var costing = CreateTestRate("AIR", "FCL", "AUSYD", "USLAX", "", "", "", "REFCARRIER", "", "");

			var entry = new WiseEntry(costing, Factory);

			var header = new WiseHeader(Factory);
			header.ChildRateEntries = new[] { entry };

			var response = new RatesSearchResponse
			{
				Carriers = new[]
				{
					new RefCarrier { Code = "REFCARRIER", SCACCode = scacCode, C1Code = c1cCode, Name = "FULL NAME" }
				}
			};

			return new WiseEntryView(entry, response);
		}

		class AssignCarrierCodeCommandForTest : AssignCarrierCodeCommand
		{
			public AssignCarrierCodeCommandForTest()
			{
			}

			public AssignCarrierCodeCommandForTest(OrgHeader orgToSelect)
			{
				OrgToSelect = orgToSelect;
			}

			OrgHeader OrgToSelect { get; }

			protected override OrgHeader SelectCarrierOrg(BusinessObjectFactory factory)
			{
				return OrgToSelect ?? base.SelectCarrierOrg(factory);
			}
		}
	}
}
