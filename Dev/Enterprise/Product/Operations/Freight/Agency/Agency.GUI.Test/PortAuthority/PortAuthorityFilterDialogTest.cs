using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class PortAuthorityFilterDialogTest : BaseAgencyTest
	{
		public void TestSendPath_OriginalSuccess()
		{
			ZDateTime now = ZDateTime.Now;
			SetPortAuthoritySettings(HomePort);
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();
			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			Voyage.JV_VoyageFlight = "x42";
			Voyage.JV_OH_Line = NewCarrier().PK;
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			sailing.Origin.JA_E_DEP = now.AddDays(1);
			sailing.Destination.JB_E_ARV = now.AddDays(2);
			NewSendableShipment(sailing);
			Factory.Save();
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(Filter))
			{
				dialog.Show();
				Filter.Direction = Constants.PortDirection.Load;
				Filter.Port = HomePort;
				Filter.MessageType = PortMessageTypeList.Codes.Original;
				dialog.PerformClickSend();
				AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), Array.ConvertAll(Filter.Issues.ToArray<PortMessageIssue>(), (i) => i.Text.ToString()));
				AssertEquals("Question Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should have sent a message", 1, sailing.Origin.Messages.Count);
			}
		}

		public void TestSendPath_ReplaceSuccess()
		{
			ZDateTime now = ZDateTime.Now;
			SetPortAuthoritySettings(HomePort);
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();
			Voyage.JV_VoyageFlight = "x42";
			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			Voyage.JV_OH_Line = NewCarrier().PK;
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			sailing.Origin.JA_E_DEP = now.AddDays(1);
			sailing.Destination.JB_E_ARV = now.AddDays(2);
			NewSendableShipment(sailing);
			Factory.Save();
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(Filter))
			{
				dialog.Show();
				Filter.Direction = Constants.PortDirection.Load;
				Filter.Port = HomePort;
				Filter.MessageType = PortMessageTypeList.Codes.Replace;
				dialog.PerformClickSend();
				AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), Array.ConvertAll(Filter.Issues.ToArray<PortMessageIssue>(), (i) => i.Text.ToString()));
				AssertEquals("Question Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should have sent a message", 1, sailing.Origin.Messages.Count);
			}
		}

		public void TestSendPath_CancellationSuccess()
		{
			ZDateTime now = ZDateTime.Now;
			SetPortAuthoritySettings(HomePort);
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();
			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			Voyage.JV_VoyageFlight = "x42";
			Voyage.JV_OH_Line = NewCarrier().PK;
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			sailing.Origin.JA_E_DEP = now.AddDays(1);
			sailing.Destination.JB_E_ARV = now.AddDays(2);
			NewSendableShipment(sailing);
			Factory.Save();
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(Filter))
			{
				dialog.Show();
				Filter.Direction = Constants.PortDirection.Load;
				Filter.Port = HomePort;
				Filter.MessageType = PortMessageTypeList.Codes.Cancellation;
				dialog.PerformClickSend();
				AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), Array.ConvertAll(Filter.Issues.ToArray<PortMessageIssue>(), (i) => i.Text.ToString()));
				AssertEquals("Question Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should have sent a message", 1, sailing.Origin.Messages.Count);
			}
		}

		[TestDate(2009, 5, 17, 13, 15, 32)]
		public void TestSendPath_3rdPartySuccess()
		{
			const string expectedMessageText = "UNA:+.? '" + "UNB+UNOA:1+SENDERID+RECIPIENTID+090517:1315+090517131532'" + "UNH+090517131532+IFCSUM:D:94B:UN:AU11'" + "BGM+785+090517131532+9+AB'" + "DTM+137:200905171315:203'" + "CNT+10:1'" + "CNT+16:0'" + "TDT+20+666++++++9168233:::BANOWATI'" + "LOC+9+AUBNE'" + "CNI+1'" + "LOC+11+SGSIN'" + "LOC+28+SGSIN'" + "LOC+88+AUBNE'" + "RFF+BM:RANDOM BILL'" + "NAD+CZ++: 1::: '" + "GID+1+1:PK'" + "FTX+AAA+++GOODS DESCRIPTION'" + "MEA+AAE+AAB+KGM:0'" + "MEA+AAE+AAW+MTQ:0'" + "PCI++RANDOM MARKS'" + "UNT+19+090517131532'" + "UNZ+1+090517131532'" + "";
			ZDateTime now = ZDateTime.Now;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			SetPortAuthoritySettings(HomePort);
			var vessel = RefVessel.LookupVesselByName("BANOWATI", Factory).First();
			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			Voyage.JV_VoyageFlight = "666";
			Voyage.JV_OH_Line = NewCarrier().PK;
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			sailing.Origin.JA_E_DEP = now.AddDays(1);
			sailing.Destination.JB_E_ARV = now.AddDays(2);
			NewSendableShipment(sailing);
			Factory.Save();
			Filter.DeliverTo3rdParty = true;
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(Filter))
			{
				dialog.Show();
				Filter.Direction = Constants.PortDirection.Load;
				Filter.Port = HomePort;
				Filter.MessageType = PortMessageTypeList.Codes.Original;
				Filter.Version = PortAuthorityVersionList.Codes.V11;
				Filter.SenderId = "SenderID";
				Filter.RecipientId = "RecipientID";
				Filter.EmailAddress = "bob@freadnet.org";
				dialog.PerformClickSend();
				AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), Array.ConvertAll(Filter.Issues.ToArray<PortMessageIssue>(), (i) => i.Text.ToString()));
				AssertEquals("Question Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should not have sent a message", 0, sailing.Origin.Messages.Count);
				AssertEquals("Should have generated an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef def = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Subject", "Manifest from: SenderID for: BANOWATI 666", def.Subject);
				AssertEquals("Body", "", def.Body);
				AssertEquals("Recipients.Count", 1, def.Recipients.Count);
				AssertEquals("Recipients[0]", "bob@freadnet.org", def.Recipients[0].Email);
				AttachmentDef att = def.Attachments[0];
				AssertEquals("Attachments.Count", 1, def.Attachments.Count);
				AssertEquals("Attachments[0].FileName", "message.edi", att.DisplayName);
				AssertMultilineASCIIEquals("Attachments[0].Data", expectedMessageText.Replace("'", "'\r\n"), Encoding.UTF8.GetString(att.Data).Replace("'", "'\r\n"));
			}
		}

		public void TestSendPath_ValidationErrors()
		{
			const string error = "Error There are errors - can't save.";
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(Filter))
			{
				dialog.Show();
				Filter.Direction = Constants.PortDirection.Load;
				Filter.Port = AlternateHomePort;
				dialog.PerformClickSend();
				AssertEquals("Should have shown the validation errors dialog.", error, UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should not have sent a message", 0, sailing.Origin.Messages.Count);
			}
		}

		public void TestSendPath_SenderNotifications()
		{
			SetPortAuthoritySettings(HomePort);
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			NewShipment(sailing, CreatePrincipal(), false, true);
			Factory.Save();
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(Filter))
			{
				dialog.Show();
				Filter.Direction = Constants.PortDirection.Load;
				Filter.Port = HomePort;
				dialog.PerformClickSend();
				AssertEquals("Question Encountered 1 error(s) while attempting to collect the required information. Please correct the error(s) and try again.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should not have sent a message", 0, sailing.Origin.Messages.Count);
			}
		}

		public void TestSendPath_SaveException()
		{
			ZDateTime now = ZDateTime.Now;
			SetPortAuthoritySettings(HomePort);
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();
			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			Voyage.JV_VoyageFlight = "x42";
			Voyage.JV_OH_Line = NewCarrier().PK;
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			sailing.Origin.JA_E_DEP = now.AddDays(1);
			sailing.Destination.JB_E_ARV = now.AddDays(2);
			NewSendableShipment(sailing);
			Factory.Save();
			// add broken VoyageOrigin to trigger save exception.
			Factory.New<VoyageOrigin>().JA_JV = ZGuid.NewZGuid();
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(Filter))
			{
				dialog.Show();
				Filter.Direction = Constants.PortDirection.Load;
				Filter.Port = HomePort;
				try
				{
					dialog.PerformClickSend();
				}
				catch (ZSaveException)
				{
				}

				AssertEquals("Show dialog approperate to the specific save exception.", "Error The JobVoyOrigin cannot be inserted/updated, requires a reference to a valid JobVoyage.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should not have sent a message", 0, sailing.Origin.Messages.Count);
			}
		}

		public void TestSendPath_Empty_Yes()
		{
			SetPortAuthoritySettings("AUBNE");
			JobSailing sailing = FindOrCreateSailing(Voyage, "AUBNE", OverseasPort);
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(Filter))
			{
				dialog.Show();
				Filter.Direction = Constants.PortDirection.Load;
				Filter.Port = "AUBNE";
				Filter.MessageType = PortMessageTypeList.Codes.Original;
				using (Filter.GetValidationSuspender())
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					dialog.PerformClickSend();
					AssertEquals("Question There are no shipments on this voyage loading in AUBNE. Are you sure you want to send an empty manifest?", UnitTestUserNotification.Instance.PreviousMessages[1].ToString());
					AssertEquals("Question Message successfully created and queued for delivery.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should have sent a message", 1, sailing.Origin.Messages.Count);
				}
			}
		}

		public void TestSendPath_Empty_No()
		{
			SetPortAuthoritySettings("AUBNE");
			JobSailing sailing = FindOrCreateSailing(Voyage, OverseasPort, "AUBNE");
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(Filter))
			{
				dialog.Show();
				Filter.Direction = Constants.PortDirection.Discharge;
				Filter.Port = "AUBNE";
				Filter.MessageType = PortMessageTypeList.Codes.Original;
				using (Filter.GetValidationSuspender())
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					dialog.PerformClickSend();
					AssertEquals("Question There are no shipments on this voyage discharging in AUBNE. Are you sure you want to send an empty manifest?", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals("Should have sent a message", 0, sailing.Origin.Messages.Count);
				}
			}
		}

		public void TestIssueDetails()
		{
			SetPortAuthoritySettings("AUBNE");
			FindOrCreateSailing(Voyage, OverseasPort, "AUBNE");
			PortMessageIssue issue1 = Filter.Issues.AddNew(Voyage.PK, "JV", "Issue1", "Detail 1");
			PortMessageIssue issue2 = Filter.Issues.AddNew(Voyage.PK, "JV", "Issue2", "Detail 2");
			PortMessageIssue issue3 = Filter.Issues.AddNew(Voyage.PK, "JV", "Issue3", "Detail 3");
			using (PortAuthorityFilterDialog dialog = new PortAuthorityFilterDialog(Filter))
			{
				dialog.Show();
				Application.DoEvents();
				SelectIssue(dialog, issue2);
				AssertEquals(issue2, dialog.CurrentIssue);
				SelectIssue(dialog, issue1);
				AssertEquals(issue1, dialog.CurrentIssue);
				SelectIssue(dialog, issue3);
				AssertEquals(issue3, dialog.CurrentIssue);
			}
		}

		public void TestShowDialog_Show()
		{
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object formOrDialog)
			{
				if (formOrDialog is PortAuthorityFilterDialog)
				{
					AssertEquals("form does not check messaging licence", false, Env.Licence.ShippingManagerPortAuthorityMessaging.IsLoggedIn);
				}
			});
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;
			Voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			Voyage.JV_VoyageFlight = "234N";
			Voyage.JV_OH_Line = NewCarrier().PK;
			TestVessel1.RV_LloydsNumber = "1234";
			FindOrCreateSailing(Voyage, HomePort, OverseasPort).Origin.JA_E_DEP = ZDateTime.Now;
			Factory.Save();
			Voyage.RunPreSaveValidation();
			AssertNoErrors("precondition:", Voyage);
			AssertNoMessageErrors("precondition", Voyage);
			AssertEquals("should not be logged in yet", false, Env.Licence.ShippingManagerPortAuthorityMessaging.IsLoggedIn);
			PortAuthorityFilterDialog.ShowDialog(Voyage);
			AssertEquals("should not be logged in anymore", false, Env.Licence.ShippingManagerPortAuthorityMessaging.IsLoggedIn);
			AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertType(typeof(PortAuthorityFilterDialog), ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestShowDialog_Errors()
		{
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;
			Voyage.JV_VoyageType = Constants.VoyageType.MainVoyage;
			Voyage.JV_RV_NKVessel = "";
			FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			Factory.Save();
			PortAuthorityFilterDialog.ShowDialog(Voyage);
			AssertEquals("Question This schedule has errors, please fix and try again.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestShowDialog_Unsaved()
		{
			Env.Security.SailingSchedulePortMessaging.IsAllowed = true;
			FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			PortAuthorityFilterDialog.ShowDialog(Voyage);
			AssertEquals("Question This schedule has changes, please save and try again.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestShowDialog_ShouldBeEitherMainVoyageOrSlotVoyageWithoutRelevantMainOne()
		{
			Voyage.JV_VoyageType = Constants.VoyageType.SlotVoyage;
			Voyage.JV_VoyageFlight = "HELLO";
			FindOrCreateSailing(Voyage, HomePort, OverseasPort).Origin.JA_E_DEP = ZDateTime.Now;
			Factory.Save();
			Action<bool> assertMainVoyageCheck = (shouldPass) =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				PortAuthorityFilterDialog.ShowDialog(Voyage);
				AssertEquals(shouldPass ? "Question This schedule has errors, please fix and try again." : "Information You have Main and Slot schedules for this vessel. Please send Port Authority message from the Main schedule.", UnitTestUserNotification.Instance.LastMessage.ToString());
				Voyage.Sailings.RemoveAndDeleteAll();
			};
			assertMainVoyageCheck(true);
			var anotherVoyage = Factory.New<JobVoyage>();
			anotherVoyage.JV_VoyageType = Constants.VoyageType.SlotVoyage;
			anotherVoyage.JV_VoyageFlight = "HELLO";
			FindOrCreateSailing(anotherVoyage, HomePort, OverseasPort).Origin.JA_E_DEP = ZDateTime.Now;
			Factory.Save();
			assertMainVoyageCheck(true);
			anotherVoyage.JV_VoyageType = Constants.VoyageType.MainVoyage;
			Factory.Save();
			assertMainVoyageCheck(false);
			Voyage.JV_VoyageType = Constants.VoyageType.MainVoyage;
			Factory.Save();
			assertMainVoyageCheck(true);
		}

		public void TestLogUsagePerConsigments()
		{
			SetPortAuthoritySettings("AUSYD");
			var principal1 = CreatePrincipal();
			var principal2 = CreatePrincipal();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			Factory.Save();
			ZDateTime today = ZDateTime.Today;
			var vessel = RefVessel.LookupVesselByName("TAIKO", Factory).First();
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "001";
			voyage.JV_OH_Line = NewCarrier().PK;
			JobSailing sailing = FindOrCreateSailing(voyage, "AUSYD", "NZAKL");
			sailing.Origin.JA_E_DEP = today.AddDays(1);
			sailing.Destination.JB_E_ARV = today.AddDays(2);
			var billOfLading1 = Factory.New<BillOfLading>();
			billOfLading1.JS_JX = sailing.PK;
			billOfLading1.JS_OH_DeliveryAgent = principal1.PK;
			billOfLading1.JS_RL_NKOrigin = sailing.JX_JA_RL_NKPortOfLoading;
			billOfLading1.JS_RL_NKDestination = sailing.JX_JB_RL_NKPortOfDischarge;
			billOfLading1.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			billOfLading1.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			billOfLading1.JS_HouseBill = "BILL001";
			billOfLading1.JS_GoodsDescription = "frozen ducks";
			billOfLading1.OuterPackLines[0].JL_MarksAndNumbers = "MARKS & NUMS";
			billOfLading1.OuterPackLines[0].JL_DetailedDescription = "frozen ducks";
			var billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_JX = sailing.PK;
			billOfLading2.JS_OH_DeliveryAgent = principal2.PK;
			billOfLading2.JS_RL_NKOrigin = sailing.JX_JA_RL_NKPortOfLoading;
			billOfLading2.JS_RL_NKDestination = sailing.JX_JB_RL_NKPortOfDischarge;
			billOfLading2.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			billOfLading2.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			billOfLading2.JS_HouseBill = "BILL002";
			billOfLading2.JS_GoodsDescription = "mushrooms";
			billOfLading2.OuterPackLines[0].JL_MarksAndNumbers = "MARKS & NUMS";
			billOfLading2.OuterPackLines[0].JL_DetailedDescription = "mushrooms";
			Factory.Save();
			var filter = new PortAuthority(voyage);
			using (var dialog = new PortAuthorityFilterDialog(filter))
			{
				dialog.Show();
				filter.Direction = Constants.PortDirection.Load;
				filter.Port = "AUSYD";
				filter.MessageType = PortMessageTypeList.Codes.Original;
				var logFilter = new ZDBOnlyQuery(typeof(StmActivityLog));
				logFilter.AddToFilter(StmActivityLogSchema.S7_FormCaption, Env.Licence.ShippingManagerPortAuthorityMessagingPerTransaction.Name);
				var existingLogCount = Factory.GetDatabaseCount(typeof(StmActivityLog), logFilter);
				dialog.PerformClickSend();
				AssertEquals("Prerequisite - should have sent a message", 1, sailing.Origin.Messages.Count);
				AssertEquals("created 2 logs, one for each sent bill of lading", 2, Factory.GetDatabaseCount(typeof(StmActivityLog), logFilter) - existingLogCount);
			}
		}

		#region Implementation

		void NewSendableShipment(JobSailing sailing)
		{
			var principal = CreatePrincipal();
			Factory.Save();
			Consignor.OH_IsConsignor = true;
			Consignee.OH_IsConsignee = true;
			AgencyShipment shipment = NewShipment(sailing, principal, false, true);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.PK;
			shipment.JS_HouseBill = "Random Bill";
			shipment.JS_GoodsDescription = "Random Crap";
			shipment.OuterPackLines[0].JL_MarksAndNumbers = "Random Marks";
			shipment.OuterPackLines[0].JL_DetailedDescription = "Goods Description";
			AssertNoErrors(shipment);
			AssertNoMessageErrors(shipment);
		}

		OrgHeader CreatePrincipal()
		{
			var principal = GlbBranch.CurrentBranch.OrgProxy;
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			SetAcosCode(principal, "BLAT");
			principal.Factory.Save();
			return principal;
		}

		void SelectIssue(PortAuthorityFilterDialog dialog, PortMessageIssue issue)
		{
			object control = typeof(PortAuthorityFilterDialog).InvokeMember("filterControl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, dialog, null);
			ZGrid grid = (ZGrid)typeof(PortAuthorityFilterControl).InvokeMember("issuesGrid", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, control, null);
			CurrencyManager manager = grid.ListManager;
			manager.Position = manager.List.IndexOf(issue);
		}

		JobVoyage Voyage
		{
			get
			{
				return voyage ?? (voyage = Factory.New<JobVoyage>());
			}
		}

		JobVoyage voyage;

		PortAuthority Filter
		{
			get
			{
				return filter ?? (filter = new PortAuthority(Voyage));
			}
		}

		PortAuthority filter;

		OrgHeader Consignor
		{
			get
			{
				return consignor ?? (consignor = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader consignor;

		OrgHeader Consignee
		{
			get
			{
				return consignee ?? (consignee = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader consignee;

		#endregion
	}
}
