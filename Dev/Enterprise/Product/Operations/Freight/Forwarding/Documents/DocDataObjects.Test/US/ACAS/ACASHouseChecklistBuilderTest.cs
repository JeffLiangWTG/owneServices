using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	class ACASHouseChecklistBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081474747474");

			var shipment1 = consol.Shipments.AddNew();
			PopulateShipment(shipment1, "081001", 12, 25, "goods1");

			var shipment2ACASSentDateTime = ZDateTime.Today.AddHours(-11);

			var shipment2 = consol.Shipments.AddNew();
			PopulateShipment(shipment2, "081002", 14, 27, "goods2");
			AddLog(shipment2, Events.InterchangeSent, shipment2ACASSentDateTime, $"MST={DocumentNames.AdvancedCargoReport}|LOC=US");

			Factory.Save();

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals(consol.JK_UniqueConsignRef, acas.ConsolNumber);
			AssertEquals("081-474747474", acas.WayBillNumber);
			AssertEquals("AUSYD", acas.PortOfOrigin.Code);
			AssertEquals("USJFK", acas.PortOfFirstArrival.Code);
			AssertEquals("USLAX", acas.PortOfDestination.Code);
			AssertEquals(26M, acas.Weight.Value);
			AssertEquals("KG", acas.Weight.Unit.Code);
			AssertEquals(52, acas.TotalNoOfPacks);
			AssertEquals("Carrier", acas.Carrier.CompanyName);
			AssertEquals("Mel", acas.Carrier.City);
			AssertEquals("SendingForwarder", acas.BookingParty.CompanyName);
			AssertEquals("Nanjing", acas.BookingParty.City);

			var acasShipment1 = acas.Shipments.First(s => s.ShipmentNumber == shipment1.JS_UniqueConsignRef);
			AssertEquals("081001", acasShipment1.WayBillNumber);
			AssertEquals("goods1", acasShipment1.GoodsDescription);
			AssertEquals("AUSYD", acasShipment1.PortOfOrigin.Code);
			AssertEquals("USLAX", acasShipment1.PortOfDestination.Code);
			AssertEquals(12M, acasShipment1.Weight.Value);
			AssertEquals("KG", acasShipment1.Weight.Unit.Code);
			AssertEquals(25, acasShipment1.TotalNoOfPacks);
			AssertEquals(ZDateTime.Empty, acasShipment1.ACASEventDate);

			var acasShipment2 = acas.Shipments.First(s => s.ShipmentNumber == shipment2.JS_UniqueConsignRef);
			AssertEquals("081002", acasShipment2.WayBillNumber);
			AssertEquals("goods2", acasShipment2.GoodsDescription);
			AssertEquals("AUSYD", acasShipment2.PortOfOrigin.Code);
			AssertEquals("USLAX", acasShipment2.PortOfDestination.Code);
			AssertEquals(14M, acasShipment2.Weight.Value);
			AssertEquals("KG", acasShipment2.Weight.Unit.Code);
			AssertEquals(27, acasShipment2.TotalNoOfPacks);
			AssertEquals(shipment2ACASSentDateTime, acasShipment2.ACASEventDate);
		}

		public void TestMAWBValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "");

			var builder = new ACASHouseChecklistBuilder(consol);
			var acas = builder.Build();
			AssertEquals("", acas.WayBillNumber);
			AssertHasMessageError(acas.WayBillNumberInfo, "MAWB must be entered to use Advance Air Cargo Report.");

			consol.JK_MasterBillNum = "001001";
			acas = builder.Build();
			AssertEquals("001-001", acas.WayBillNumber);
			AssertHasMessageError(acas.WayBillNumberInfo, "The MAWB should contain 11 digits.");

			consol.JK_MasterBillNum = "12345678911";
			acas = builder.Build();
			AssertEquals("123-45678911", acas.WayBillNumber);
			AssertHasMessageError(acas.WayBillNumberInfo, "Invalid check digit. The last digit should be '6'");

			consol.JK_MasterBillNum = "12345678916";
			acas = builder.Build();
			AssertEquals("123-45678916", acas.WayBillNumber);
			AssertNoNotifications(acas.WayBillNumberInfo);
		}

		#region SendersAcasCode

		const string errorMessage = "This code is required for ACAS messaging. Raise an eRequest to register your interest. Once provided by WTG, enter the code against the Branch or Company Organization Proxy > Config > Registration Numbers/Codes tab using Type = US ACA.";

		public void TestSendersAcasCode_CodeExistsInBranchAndCompany()
		{
			var consol = PrepareTestDataForSendersAcasCodeTests();
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "BBB", Core.Constants.CountryCodes.UnitedStates);
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "CCC", Core.Constants.CountryCodes.UnitedStates);

			AssertACACodeExists("Precondition: Current branch's proxy has ACA code.", GlbBranch.CurrentBranch.OrgProxy, true, "BBB");
			AssertACACodeExists("Precondition: Current company's proxy has ACA code.", GlbCompany.CurrentCompany.OrgProxy, true, "CCC");

			var acas = new ACASHouseChecklistBuilder(consol).Build();

			AssertEquals("SendersAcasCode should have valid value.", "BBB", acas.SendersAcasCode);
			AssertNoMessageError("SendersAcasCode should not have error message.", acas.SendersAcasCodeInfo, errorMessage);
		}

		public void TestSendersAcasCode_CodeExistsInBranchOnly()
		{
			var consol = PrepareTestDataForSendersAcasCodeTests();
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "BBB", Core.Constants.CountryCodes.UnitedStates);

			AssertACACodeExists("Precondition: Current branch's proxy has ACA code.", GlbBranch.CurrentBranch.OrgProxy, true, "BBB");
			AssertACACodeExists("Precondition: Current company's proxy does not have ACA code.", GlbCompany.CurrentCompany.OrgProxy, false);

			var acas = new ACASHouseChecklistBuilder(consol).Build();

			AssertEquals("SendersAcasCode should have valid value.", "BBB", acas.SendersAcasCode);
			AssertNoMessageError("SendersAcasCode should not have error message.", acas.SendersAcasCodeInfo, errorMessage);
		}

		public void TestSendersAcasCode_CodeExistsInCompanyOnly_FallbackCase()
		{
			var consol = PrepareTestDataForSendersAcasCodeTests();
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "CCC", Core.Constants.CountryCodes.UnitedStates);

			AssertACACodeExists("Precondition: Current branch's proxy does not have ACA code.", GlbBranch.CurrentBranch.OrgProxy, false);
			AssertACACodeExists("Precondition: Current company's proxy has ACA code.", GlbCompany.CurrentCompany.OrgProxy, true, "CCC");

			var acas = new ACASHouseChecklistBuilder(consol).Build();

			AssertEquals("SendersAcasCode should have valid value.", "CCC", acas.SendersAcasCode);
			AssertNoMessageError("SendersAcasCode should not have error message.", acas.SendersAcasCodeInfo, errorMessage);
		}

		public void TestSendersAcasCode_CodeNotExist()
		{
			var consol = PrepareTestDataForSendersAcasCodeTests();

			AssertACACodeExists("Precondition: Current branch's proxy does not have ACA code.", GlbBranch.CurrentBranch.OrgProxy, false);
			AssertACACodeExists("Precondition: Current company's proxy does not have ACA code.", GlbCompany.CurrentCompany.OrgProxy, false);

			var acas = new ACASHouseChecklistBuilder(consol).Build();

			AssertEquals("SendersAcasCode should have empty value.", ZString.Empty, acas.SendersAcasCode);
			AssertHasMessageError("SendersAcasCode should have error message.", acas.SendersAcasCodeInfo, errorMessage);
		}

		ForwardingConsol PrepareTestDataForSendersAcasCodeTests()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "");
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.Factory.New<OrgHeader>().PK;

			AssertNotNull("Precondition: Proxy of current branch is not null.", GlbBranch.CurrentBranch.OrgProxy);
			AssertACACodeExists("Precondition: Current branch's proxy has ACA code.", GlbBranch.CurrentBranch.OrgProxy, false);
			AssertNotNull("Precondition: Proxy of current company is not null.", GlbCompany.CurrentCompany.OrgProxy);
			AssertACACodeExists("Precondition: Current company's proxy has ACA code.", GlbCompany.CurrentCompany.OrgProxy, false);

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertHasMessageError("SendersAcasCode should have error message.", acas.SendersAcasCodeInfo, errorMessage);

			return consol;
		}

		void AssertACACodeExists(string message, OrgHeader orgHeader, bool testForExists, string number = null)
		{
			CombineAssertions(message, () =>
			{
				var code = orgHeader.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == OrgCusCode.USACodeTypes.ACASOriginatorCode && c.OK_RN_NKCodeCountry.StartsWith(Core.Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase));
				if (testForExists)
				{
					AssertNotNull("Proxy organization has an ACA number.", code);
					AssertEquals($"Proxy organization has an ACA number. {number}.", number, code.OK_CustomsRegNo.ToString());
				}
				else
				{
					AssertNull("Proxy organization does not have an ACA number.", code);
				}
			});
		}

		#endregion

		#region TestACASEventDate

		public void TestACASEventDate()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081474747474");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "081001", 12, 25, "goods1");

			string expectedErrorMessageNotSent = "The ACAS House Checklist message can only be sent when all House Bills have been reported to ACAS.";
			string expectedErrorMessageAwaitingForwarding = "The ACAS Shipment Report message is awaiting forwarding by eHub.";

			TestACASEventDate("Date is empty as no ACAS logs", consol, shipment, ZDateTime.Empty, expectedErrorMessageNotSent);

			AddLog(shipment, Events.InterchangeSent, ZDateTime.Today.AddHours(-11), $"MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			TestACASEventDate("ACAS message has been sent", consol, shipment, ZDateTime.Today.AddHours(-11), ZString.Empty);

			AddLog(shipment, Events.InterchangeRejected, ZDateTime.Today.AddHours(-10), $"MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			TestACASEventDate("The message has been rejected", consol, shipment, ZDateTime.Today.AddHours(-10), expectedErrorMessageNotSent);

			AddLog(shipment, Events.InterchangeSent, ZDateTime.Today.AddHours(-9), $"MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			TestACASEventDate("ACAS message has been resent", consol, shipment, ZDateTime.Today.AddHours(-9), ZString.Empty);

			AddLog(shipment, Events.MessageSent, ZDateTime.Today.AddHours(-8), $"MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			TestACASEventDate("Has Message Sent event", consol, shipment, ZDateTime.Today.AddHours(-8), expectedErrorMessageAwaitingForwarding);
		}

		void TestACASEventDate(ZString message, ForwardingConsol consol, ForwardingShipment shipment, ZDateTime expectedACASEventDate, ZString expectedNotification)
		{
			var builder = new ACASHouseChecklistBuilder(consol);
			var acas = builder.Build();
			var acasShipment = acas.Shipments.First();

			AssertEquals(message, expectedACASEventDate, acasShipment.ACASEventDate);

			if (expectedNotification.IsEmpty)
			{
				AssertNoNotifications(acasShipment.DisplayInformationInfo);
			}
			else
			{
				AssertHasMessageError(acasShipment.DisplayInformationInfo, expectedNotification);
			}
		}

		#endregion

		#region Acas State

		public void TestAcasState_NoStatus()
		{
			var consol = Factory.New<ForwardingConsol>();

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: None", AcasHouseChecklistMessageState.None, acas.State);
			AssertEquals("The ACAS House Checklist Message has not been sent.", acas.DisplayInformation);
		}

		public void TestAcasState_OriginalSent()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.MessageSent, ZDateTime.Today.AddHours(-11), $"MST={DocumentNames.AdvancedManifest}|LOC=US");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Original Sent", AcasHouseChecklistMessageState.OriginalSent, acas.State);
			AssertEquals("DisplayInformation", "The message has been sent to ACAS.", acas.DisplayInformation);
		}

		public void TestAcasState_OriginalForwarded()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.InterchangeSent, ZDateTime.Today.AddHours(-11), $"MST={DocumentNames.AdvancedManifest}|LOC=US");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Original Forwarded", AcasHouseChecklistMessageState.OriginalForwarded, acas.State);
			AssertEquals("DisplayInformation", "The message has been routed on to the recipient CBP (US Customs and Border Protection).", acas.DisplayInformation);
		}

		public void TestAcasState_eHubRejected()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.InterchangeRejected, ZDateTime.Today.AddHours(-11), $"DEP=WiseTechGlobal|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=Some Reason from eHub");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Rejected by eHub", AcasHouseChecklistMessageState.RejectedByEHub, acas.State);
			AssertEquals("DisplayInformation", "Message rejected by eHub ('Some Reason from eHub').", acas.DisplayInformation);
		}

		public void TestAcasState_CBPRejected()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.MessageRejected, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=Some Reason from CBP");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Rejected by CBP", AcasHouseChecklistMessageState.RejectedByCBP, acas.State);
			AssertEquals("DisplayInformation", "Message rejected by CBP ('Some Reason from CBP').", acas.DisplayInformation);
		}

		public void TestAcasState_MessagePendingProcessing()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.MessagePendingProcessing, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=SR");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Message Pending Processing", AcasHouseChecklistMessageState.MessagePendingProcessing, acas.State);
			AssertEquals("DisplayInformation", "Message Pending Processing by Customs because Security Filing Received - Assessment in Progress.", acas.DisplayInformation);
		}

		public void TestAcasState_ClearanceCompleted()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.ClearanceCompleted, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=SF");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Clearance Completed", AcasHouseChecklistMessageState.ClearanceCompleted, acas.State);
			AssertEquals("DisplayInformation", "Clearance Completed by Customs because Security Filing Received - Assessment Complete.", acas.DisplayInformation);
		}

		public void TestAcasState_HoldInPlace_6H()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.Held, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=6H");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Hold In Place", AcasHouseChecklistMessageState.HoldInPlace, acas.State);
			AssertEquals("DisplayInformation", "Shipment Report Held by Customs because Do Not Load (DNL) Hold.", acas.DisplayInformation);
		}

		public void TestAcasState_HoldInPlace_6J()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.Held, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=6J");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Hold In Place", AcasHouseChecklistMessageState.HoldInPlace, acas.State);
			AssertEquals("DisplayInformation", "Held by Customs because Do Not Load Hold Currently In Place.", acas.DisplayInformation);
		}

		public void TestAcasState_HoldRemoved_6I()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.ClearedHold, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=6I");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Hold Removed", AcasHouseChecklistMessageState.HoldRemoved, acas.State);
			AssertEquals("DisplayInformation", "Cleared Hold by Customs because Do Not Load Hold Removed.", acas.DisplayInformation);
		}

		public void TestAcasState_HoldInPlace_7H()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.Held, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=7H");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Hold In Place", AcasHouseChecklistMessageState.HoldInPlace, acas.State);
			AssertEquals("DisplayInformation", "Held by Customs because Selectee Data Issue Hold.", acas.DisplayInformation);
		}

		public void TestAcasState_HoldInPlace_7J()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.Held, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=7J");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Hold In Place", AcasHouseChecklistMessageState.HoldInPlace, acas.State);
			AssertEquals("DisplayInformation", @"Held by Customs because Selectee Data Issue Hold Currently In Place.", acas.DisplayInformation);
		}

		public void TestAcasState_HoldRemoved_7I()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.ClearedHold, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=7I");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Hold Removed", AcasHouseChecklistMessageState.HoldRemoved, acas.State);
			AssertEquals("DisplayInformation", @"Cleared Hold by Customs because Selectee Data Issue Hold Removed.", acas.DisplayInformation);
		}

		public void TestAcasState_HoldInPlace_8H()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.Held, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=8H");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Hold In Place", AcasHouseChecklistMessageState.HoldInPlace, acas.State);
			AssertEquals("DisplayInformation", @"Held by Customs because Selectee Screening (or Verification) Required Hold.", acas.DisplayInformation);
		}

		public void TestAcasState_HoldInPlace_8J()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.Held, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=8J");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Hold In Place", AcasHouseChecklistMessageState.HoldInPlace, acas.State);
			AssertEquals("DisplayInformation", @"Held by Customs because Selectee Screening (or Verification) Required Hold Currently In Place.", acas.DisplayInformation);
		}

		public void TestAcasState_HoldRemoved_8I()
		{
			var consol = Factory.New<ForwardingConsol>();
			AddLog(consol, Events.ClearedHold, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedManifest}|LOC=US|RES=8I");

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			AssertEquals("State: Hold Removed", AcasHouseChecklistMessageState.HoldRemoved, acas.State);
			AssertEquals("DisplayInformation", @"Cleared Hold by Customs because Selectee Screening (or Verification) Required Hold Removed.", acas.DisplayInformation);
		}

		#endregion

		#region Shipment Acas State

		public void TestShipmentAcasState_None()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: None", AcasState.None, acasShipment.State);
			AssertEquals("The ACAS Shipment Report has not been sent.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "The ACAS House Checklist message can only be sent when all House Bills have been reported to ACAS.");
		}

		public void TestShipmentAcasState_OriginalSent_NotForwardedToCBP()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			AddLog(shipment, Events.MessageSent, ZDateTime.Today.AddHours(-11), $"MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: Original Sent", AcasState.OriginalSent, acasShipment.State);
			AssertEquals("Await response - Original submitted to eHub.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "The ACAS Shipment Report message is awaiting forwarding by eHub.");
		}

		public void TestShipmentAcasState_OriginalSent_ForwardedToCBP()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			AddLog(shipment, Events.InterchangeSent, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: Original Sent", AcasState.OriginalSent, acasShipment.State);
			AssertEquals("Await response - Original sent to CBP.", acasShipment.DisplayInformation);
			AssertNoMessageErrors(acasShipment.DisplayInformationInfo);
		}

		public void TestShipmentAcasState_AmendmentSent_NotForwardedToCBP()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			AddLog(shipment, Events.InterchangeSent, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			AddLog(shipment, Events.MessageSent, ZDateTime.Today.AddHours(-10), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: Amendment Sent", AcasState.AmendmentSent, acasShipment.State);
			AssertEquals("Await response - Amendment submitted to eHub.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "The ACAS Shipment Report message is awaiting forwarding by eHub.");
		}

		public void TestShipmentAcasState_AmendmentSent_ForwardedToCBP()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			AddLog(shipment, Events.InterchangeSent, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			AddLog(shipment, Events.MessageSent, ZDateTime.Today.AddHours(-10), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			AddLog(shipment, Events.InterchangeSent, ZDateTime.Today.AddHours(-9), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: Amendment Sent", AcasState.AmendmentSent, acasShipment.State);
			AssertEquals("Await response - Amendment sent to CBP.", acasShipment.DisplayInformation);
			AssertNoMessageErrors(acasShipment.DisplayInformationInfo);
		}

		public void TestShipmentAcasState_AcknowledgementSent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			AddLog(shipment, Events.Held, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US|RES=8H");
			AddLog(shipment, Events.MessageSent, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: Acknowledgement Sent", AcasState.AcknowledgementSent, acasShipment.State);
			AssertEquals("Await response - Acknowledgement sent to CBP.", acasShipment.DisplayInformation);
			AssertNoMessageErrors(acasShipment.DisplayInformationInfo);
		}

		public void TestShipmentAcasState_AcknowledgementRequired()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S001";

			AddLog(shipment, Events.Held, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US|RES=7H");
			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: Acknowledgement Required", AcasState.AcknowledgementRequired, acasShipment.State);
			AssertEquals("Acknowledgement required - \"On Hold\" with CBP.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "Shipment S001 is currently on hold with CBP.");
		}

		public void TestShipmentAcasState_AssessmentOngoing()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			AddLog(shipment, Events.MessagePendingProcessing, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: Assessment Ongoing", AcasState.AssessmentOngoing, acasShipment.State);
			AssertEquals("Await response - CBP risk assessment ongoing.", acasShipment.DisplayInformation);
			AssertNoMessageErrors(acasShipment.DisplayInformationInfo);
		}

		public void TestShipmentAcasState_AssessmentComplete()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			AddLog(shipment, Events.ClearanceCompleted, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US");
			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: Assessment Complete", AcasState.AssessmentComplete, acasShipment.State);
			AssertEquals("Approved to be uplifted/loaded.", acasShipment.DisplayInformation);
			AssertNoMessageErrors(acasShipment.DisplayInformationInfo);
		}

		public void TestShipmentAcasState_HoldInPlace()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S001";

			AddLog(shipment, Events.Held, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US|RES=6J");
			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: Hold In Place", AcasState.HoldInPlace, acasShipment.State);
			AssertEquals("Await response - \"Hold Currently in Place\" with CBP.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "Shipment S001 is currently on hold with CBP.");
		}

		public void TestShipmentAcasState_HoldRemoved()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			AddLog(shipment, Events.ClearedHold, ZDateTime.Today.AddHours(-11), $"DEP=Customs|MST={DocumentNames.AdvancedCargoReport}|LOC=US|RES=6I");
			var acas = new ACASHouseChecklistBuilder(consol).Build();

			var acasShipment = acas.Shipments.First();
			AssertEquals("State: Hold Removed", AcasState.HoldRemoved, acasShipment.State);
			AssertEquals("Approved - \"Hold\" status removed by CBP.", acasShipment.DisplayInformation);
			AssertNoMessageErrors("No message error", acasShipment.DisplayInformationInfo);
		}

		#endregion

		#region HVLV

		public void TestPopulateHVLVShipmentACASStateDetails_WhenAllConsignmentsHaveEmptyHVC_ACASMessageStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "HVL";
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();
			Assert("precondition", consignment.HVC_ACASMessageStatus.IsEmpty);

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			var acasShipment = acas.Shipments.First();

			AssertEquals("The ACAS Shipment Report has not been sent.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "The ACAS House Checklist message can only be sent when all House Bills have been reported to ACAS.");
		}

		public void TestPopulateHVLVShipmentACASStateDetails_WhenConsignmentHasOIJHVC_InterchangeStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "HVL";
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.OriginalInterchangeRejected;
			Factory.Save();

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			var acasShipment = acas.Shipments.First();

			AssertEquals("Not all ACAS Reports have been sent.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "The ACAS House Checklist message can only be sent when all House Bills have been reported to ACAS.");
		}

		public void TestPopulateHVLVShipmentACASStateDetails_WhenConsignmentHasOSTHVC_ACASMessageStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "HVL";
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			Factory.Save();

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			var acasShipment = acas.Shipments.First();

			AssertEquals("DisplayInformation", "Await response - Original submitted to eHub.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "The ACAS Shipment Report messages are awaiting forwarding by eHub.");
		}

		public void TestPopulateHVLVShipmentACASStateDetails_WhenConsignmentHasCBPHOLDResponse()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "HVL";
			shipment.JS_UniqueConsignRef = "S001";
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.OriginalInterchangeSent;
			Factory.Save();

			var holdStatuses = new[] { ACASActions.Code.DoNotLoadHold,
							ACASActions.Code.SelecteeDataIssueHold,
							ACASActions.Code.SelecteeScreeningOrVerificationRequiredHold,
							ACASActions.Code.DoNotLoadHoldCurrentlyInPlace,
							ACASActions.Code.SelecteeDataIssueHoldCurrentlyInPlace,
							ACASActions.Code.SelecteeScreeningOrVerificationRequiredHoldCurrentlyInPlace };

			foreach (string acasStatus in holdStatuses)
			{
				consignment.HVC_ACASStatus = acasStatus;
				Factory.Save();

				var acas = new ACASHouseChecklistBuilder(consol).Build();
				var acasShipment = acas.Shipments.First();
				AssertEquals("Await response - \"Hold Currently in Place\" with CBP.", acasShipment.DisplayInformation);
				AssertHasMessageError(acasShipment.DisplayInformationInfo, "Shipment S001 is currently on hold with CBP.");
			}
		}

		public void TestPopulateHVLVShipmentACASStateDetails_WhenConsignmentHasOISHVC_ACASInterchangeStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "HVL";
			shipment.JS_UniqueConsignRef = "S001";
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			consignment.HVC_ACASStatus = ACASActions.Code.SecurityFilingAssessmentComplete;
			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.OriginalInterchangeSent;
			Factory.Save();

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			var acasShipment = acas.Shipments.First();

			AssertEquals("Approved to be uplifted/loaded.", acasShipment.DisplayInformation);
		}

		public void TestPopulateHVLVShipmentACASStateDetails_WhenConsignmentACASStatusIsEmpty()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "HVL";
			shipment.JS_UniqueConsignRef = "S001";
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			consignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.OriginalInterchangeSent;
			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			consignment.HVC_ACASStatus = ZString.Empty;
			Factory.Save();

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			var acasShipment = acas.Shipments.First();

			AssertEquals("Original sent to CBP.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "Await response - CBP risk assessment ongoing.");
		}

		public void TestPopulateHVLVShipmentACASStateDetails_MultipleStatusesPriority()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "HVL";
			shipment.JS_UniqueConsignRef = "S001";

			var emptyMessageStatusConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			emptyMessageStatusConsignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item1 = emptyMessageStatusConsignment.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			Factory.Save();

			var acas = new ACASHouseChecklistBuilder(consol).Build();
			var acasShipment = acas.Shipments.First();

			AssertEquals("The ACAS Shipment Report has not been sent.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "The ACAS House Checklist message can only be sent when all House Bills have been reported to ACAS.");

			var oijConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			oijConsignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item2 = oijConsignment.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			oijConsignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			oijConsignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.OriginalInterchangeRejected;
			Factory.Save();

			var ostConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			ostConsignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item3 = ostConsignment.Items.AddNew();
			item3.HVI_JS_LoadedOnShipment = shipment.PK;
			ostConsignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			Factory.Save();

			var cbpHoldConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			cbpHoldConsignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item4 = cbpHoldConsignment.Items.AddNew();
			item4.HVI_JS_LoadedOnShipment = shipment.PK;
			cbpHoldConsignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			cbpHoldConsignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.OriginalInterchangeSent;
			cbpHoldConsignment.HVC_ACASStatus = ACASActions.Code.DoNotLoadHoldCurrentlyInPlace;
			Factory.Save();

			var emptyACASStatusConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			emptyACASStatusConsignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item5 = emptyACASStatusConsignment.Items.AddNew();
			item5.HVI_JS_LoadedOnShipment = shipment.PK;
			emptyACASStatusConsignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			emptyACASStatusConsignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.OriginalInterchangeSent;
			Factory.Save();

			var notHoldConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			notHoldConsignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item6 = notHoldConsignment.Items.AddNew();
			item6.HVI_JS_LoadedOnShipment = shipment.PK;
			notHoldConsignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			notHoldConsignment.HVC_ACASInterchangeStatus = HVLVACASInterchangeStatusList.Codes.OriginalInterchangeSent;
			notHoldConsignment.HVC_ACASStatus = ACASActions.Code.SecurityFilingAssessmentComplete;
			Factory.Save();

			acas = new ACASHouseChecklistBuilder(consol).Build();
			acasShipment = acas.Shipments.First();

			AssertEquals("Not all ACAS Reports have been sent.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "The ACAS House Checklist message can only be sent when all House Bills have been reported to ACAS.");

			oijConsignment.HVC_JS_ManifestedOnShipment = Guid.Empty;
			item2.HVI_JS_LoadedOnShipment = Guid.Empty;
			Factory.Save();

			acas = new ACASHouseChecklistBuilder(consol).Build();
			acasShipment = acas.Shipments.First();

			AssertEquals("Await response - Original submitted to eHub.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "The ACAS Shipment Report messages are awaiting forwarding by eHub.");

			ostConsignment.HVC_JS_ManifestedOnShipment = Guid.Empty;
			item3.HVI_JS_LoadedOnShipment = Guid.Empty;
			Factory.Save();

			acas = new ACASHouseChecklistBuilder(consol).Build();
			acasShipment = acas.Shipments.First();

			AssertEquals("Await response - \"Hold Currently in Place\" with CBP.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "Shipment S001 is currently on hold with CBP.");

			cbpHoldConsignment.HVC_JS_ManifestedOnShipment = Guid.Empty;
			item4.HVI_JS_LoadedOnShipment = Guid.Empty;
			Factory.Save();

			acas = new ACASHouseChecklistBuilder(consol).Build();
			acasShipment = acas.Shipments.First();

			AssertEquals("Original sent to CBP.", acasShipment.DisplayInformation);
			AssertHasMessageError(acasShipment.DisplayInformationInfo, "Await response - CBP risk assessment ongoing.");

			emptyACASStatusConsignment.HVC_JS_ManifestedOnShipment = Guid.Empty;
			item5.HVI_JS_LoadedOnShipment = Guid.Empty;
			Factory.Save();

			acas = new ACASHouseChecklistBuilder(consol).Build();
			acasShipment = acas.Shipments.First();

			AssertEquals("Approved to be uplifted/loaded.", acasShipment.DisplayInformation);
		}

		#endregion

		#region Implementation

		void PopulateConsol(ForwardingConsol consol, ZString mawb)
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = mawb;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SendingForwarder";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJG";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "Xin";
			sendingForwarder.MainAddress.City = "Nanjing";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "USJFK";

			var transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_LegOrder = 2;
			transportLeg2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg2.JW_RL_NKLoadPort = "USJFK";
			transportLeg2.JW_RL_NKDiscPort = "USLAX";
		}

		void PopulateShipment(ForwardingShipment shipment, ZString hawb, ZDecimal weight, ZInt packs, ZString desc)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = hawb;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = desc;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = packs;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Consignor";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit52";
			shipper.MainAddress.Address2 = "Dorcus yamadai";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2017";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "USLAX";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = "US";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
		}

		void AddLog(IStmALogParent parent, Event eventType, ZDateTime eventDate, string parameters)
		{
			var paramList = new List<KeyValuePair<string, string>>();
			foreach (var paramPair in parameters.Split('|'))
			{
				var pair = paramPair.Split('=');
				paramList.Add(new KeyValuePair<string, string>(pair[0], pair[1]));
			}

			parent.Logs.AddNew(eventType, eventDate.ToOffset(), paramList.ToArray());

			Thread.Sleep(1);
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
		}

		#endregion
	}
}
