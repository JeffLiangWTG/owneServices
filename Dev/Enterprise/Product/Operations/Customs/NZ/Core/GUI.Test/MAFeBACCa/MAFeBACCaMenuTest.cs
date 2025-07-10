using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa.Testing
{
	class MAFeBACCaMenuTest : TestCaseWithFactory
	{
		public void TestResetToOriginal()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			mafMessaging.ZX_ConsignmentNumber = "SDFLKJSD";
			mafMessaging.ZX_ReceiptNumber = "AFHIJKJL";
			mafMessaging.ZX_MessagingStatus = "ZXY";
			var message = mafMessaging.Messages.AddNew();
			message.EM_Status = NZMMessage.Status.Queued;
			message.EM_ReceiveTransmit = NZMMessage.Direction.Transmit;
			var menu = new MAFeBACCaMenu(mafMessaging);
			var menuItem = menu.MenuItems.FindByText(MAFeBACCaMenu.MenuNames.ResetToOriginal);
			AssertNotNull("Reset to Original menu item", menuItem);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			menuItem.PerformClick();
			AssertEquals("Show Question", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(MAFeBACCaMenu.MessageTextResetToOriginal));
			AssertEquals("Show Confirmation", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(MAFeBACCaMenu.MessageSuccessResetToOriginal));
			AssertEquals("mafMessaging.ZX_ConsignmentNumber", "SDFLKJSD", mafMessaging.ZX_ConsignmentNumber);
			AssertEquals("mafMessaging.ZX_ReceiptNumber", "AFHIJKJL", mafMessaging.ZX_ReceiptNumber);
			AssertEquals("mafMessaging.ZX_MessagingStatus", "ZXY", mafMessaging.ZX_MessagingStatus);
			AssertEquals("message.EM_Status", NZMMessage.Status.Queued, message.EM_Status);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			menuItem.PerformClick();
			AssertEquals("Show Question", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(MAFeBACCaMenu.MessageTextResetToOriginal));
			AssertEquals("Show Confirmation", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(MAFeBACCaMenu.MessageSuccessResetToOriginal));
			AssertEquals("mafMessaging.ZX_ConsignmentNumber", "", mafMessaging.ZX_ConsignmentNumber);
			AssertEquals("mafMessaging.ZX_ReceiptNumber", "", mafMessaging.ZX_ReceiptNumber);
			AssertEquals("mafMessaging.ZX_MessagingStatus", "", mafMessaging.ZX_MessagingStatus);
			AssertEquals("message.EM_Status", NZMMessage.Status.Cancelled, message.EM_Status);
		}

		public void TestMenuShowsErrorOnError()
		{
			var menu = new MAFeBACCaMenu(TestDataBuilder.GetMAFMessaging(Factory.New<ForwardingConsol>()));
			var menuItem = menu.MenuItems.FindByText(MAFeBACCaMenu.MenuNames.SendToMpi);
			AssertNotNull("Send to MPI menu item", menuItem);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menuItem.PerformClick();
			Assert("LastMessage.WasError", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestMenuShowsMessageQueuedWhenAllIsOk()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "BX");
			var declaration = Factory.New<JobDeclaration>();
			var builder = new TestDataBuilder(declaration);
			builder.PopulateDeclarationThatPassesValidation();
			var consol = Factory.New<ForwardingConsol>();
			builder.PopulateConsolThatPassesValidation(consol);
			declaration.JE_JS = consol.Shipments[0].PK;
			Factory.Save();
			TestHelper.SetupMessagingEnvironment();
			var menu = new MAFeBACCaMenu(TestDataBuilder.GetMAFMessaging(consol));
			var menuItem = menu.MenuItems.FindByText(MAFeBACCaMenu.MenuNames.SendToMpi);
			AssertNotNull("Send to MPI menu item", menuItem);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			try
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menuItem.PerformClick();
				Assert("LastMessage.WasInformation", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Last Message should say it's been Queued.", "IPI message queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.None;
			}
		}

		[ExpectNoExceptions]
		public void TestSendConsoleBACCaNowGeneratesIPIMessage()
		{
			SetupCurrentUser();
			var consol = eBAACAConsol();
			using (var form = new ZForm(consol))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.PlugIns.Add(ControllerIDs.Customs.NZ.MAFeBACCaConsolPlugIn);
				form.Show();
				var plugIn = (MAFeBACCaPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.NZ.MAFeBACCaConsolPlugIn);
				plugIn.Enabled = true;
				var sendMenuItem = plugIn.TopLevelMenu.MenuItems.FindByText(MAFeBACCaMenu.MenuNames.SendToMpi, true);
				AssertNotNull("Send to MPI menu item", sendMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
			}
		}

		#region Implementation
		internal ForwardingConsol eBAACAConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08100544272";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_UnpackDepotAddress = GetDepot().MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = GetImporter().MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = GetExporter().MainAddress.PK;
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = false;
			transport.JW_VoyageFlight = "QF11";
			transport.JW_ETA = new DateTime(2020, 08, 21);
			transport.JW_ATA = new DateTime(2020, 08, 21);
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			var shipment1 = consol.Shipments.AddNew();
			shipment1.ConsigneeNameOrPK = GetImporter().OH_Code;
			shipment1.ConsignorNameOrPK = GetExporter().OH_Code;
			shipment1.JS_HouseBill = "G85928";
			shipment1.JS_OuterPacks = 50;
			shipment1.JS_F3_NKPackType = "CTN";
			shipment1.JS_RL_NKDestination = "NZAKL";
			shipment1.JS_ActualWeight = 780;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.ConsigneeNameOrPK = GetImporter().OH_Code;
			shipment2.ConsignorNameOrPK = GetExporter().OH_Code;
			shipment2.JS_HouseBill = "H69238";
			shipment2.JS_OuterPacks = 10;
			shipment2.JS_F3_NKPackType = "BOX";
			shipment2.JS_RL_NKDestination = "NZAKL";
			shipment2.JS_ActualWeight = 500;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			var shipment3 = consol.Shipments.AddNew();
			shipment3.ConsigneeNameOrPK = GetImporter().OH_Code;
			shipment3.ConsignorNameOrPK = GetExporter().OH_Code;
			shipment3.JS_HouseBill = "G05827-17E";
			shipment3.JS_OuterPacks = 1;
			shipment3.JS_F3_NKPackType = "PLT";
			shipment3.JS_RL_NKDestination = "NZAKL";
			shipment3.JS_ActualWeight = 210;
			shipment3.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			var packLine1 = consol.RelatedPackLines.AddNew();
			packLine1.JL_PackageCount = 50;
			packLine1.JL_Description = "Magazines";
			packLine1.JL_F3_NKPackType = "CTN";
			packLine1.JL_JS = shipment1.PK;
			var packLine2 = consol.RelatedPackLines.AddNew();
			packLine2.JL_PackageCount = 10;
			packLine1.JL_Description = "Books";
			packLine2.JL_F3_NKPackType = "BOX";
			packLine2.JL_JS = shipment2.PK;
			var packLine3 = consol.RelatedPackLines.AddNew();
			packLine3.JL_PackageCount = 1;
			packLine3.JL_Description = "Newsprint";
			packLine3.JL_F3_NKPackType = "PLT";
			packLine3.JL_JS = shipment3.PK;
			Factory.Save();
			return consol;
		}

		internal void SetupCurrentUser()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			currentCompany.GC_Name = "CARGOWISE BROKERS";
			var branch = GlbBranch.CurrentBranch;
			branch.GB_GC = currentCompany.PK;
			branch.GB_BranchName = "CARGOWISE BROKERS ZZZ";
			branch.GB_Address1 = "72 BROKER STREET";
			branch.GB_Address2 = "BROKERS TOWER";
			branch.GB_City = "BROKERVILLE";
			branch.GB_RL_NKHomePort = "NZAKL";
			branch.GB_PostCode = "2015";
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009915B");
			Env.Registry.MailboxEmailAddress = "cargowiseone@brokers.cargowise.com";
			var currentUser = GlbStaff.CurrentUser;
			currentUser.GS_FullName = "Sidney allaballah Broker";
			currentUser.GS_EmailAddress = "broker@brokers.cargowise.com";
			currentUser.GS_FaxNum = "99887766";
			currentUser.GS_WorkPhone = "44556677";
		}

		OrgHeader GetExporter()
		{
			return GetNewOrganisation("SUPPLIER PTY LTD", "98 SUPPLIER CIRCUIT", "SUPPLIERHAVEN", "SUPPLIER HILL", "2234", "AUMEL", OrgCusCode.CodeTypes.SupplierCode, "00998877W", "Major", "Exporter", "major.exporter@exporter.com.au", "12345678", "876543211");
		}

		OrgHeader GetImporter()
		{
			var importer = GetNewOrganisation("IMPORTER INCORPORATED", "77 IMPORTER AVENUE", "IMPORTER SPIRE", "IMPORTERVILLE", "0232", "NZNPE", OrgCusCode.CodeTypes.CustomsClientCode, "00112233K", "Test", "Importer", "importer@importer.co.nz", "11111111", "22222222");
			importer.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.MAFCoverSheetQE, "AB123");
			return importer;
		}

		OrgHeader GetDepot()
		{
			return GetNewOrganisation("TRANSITIONAL FACILITY", "45 TRANSITIONAL ROAD", "FACILITY TOPS", "TRANSITIONAL", "4389", "NZWPW", OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "TF1");
		}

		OrgHeader GetNewOrganisation(string organisationName, string addressLine1, string addressLine2, string city, string postalCode, string unloco, string customsCodeType, string customsCode)
		{
			return GetNewOrganisation(organisationName, addressLine1, addressLine2, city, postalCode, unloco, customsCodeType, customsCode, null, null, null, null, null);
		}

		internal OrgHeader GetNewOrganisation(string organisationName, string addressLine1, string addressLine2, string city, string postalCode, string unloco, string customsCodeType, string customsCode, string contactFirstName, string contactLastName, string email, string fax, string phone)
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = organisationName;
			OrgAddress address = organisation.MainAddress;
			address.OA_Address1 = addressLine1;
			address.OA_Address2 = addressLine2;
			address.OA_City = city;
			address.OA_RL_NKRelatedPortCode = unloco;
			address.OA_PostCode = postalCode;
			if (contactFirstName != null)
			{
				OrgContact contact = organisation.Contacts.AddNew();
				contact.OC_ContactName = contactFirstName + " ABDULLAH " + contactLastName;
				contact.OC_Email = email;
				contact.OC_Fax = fax;
				contact.OC_Phone = phone;
				OrgDocument document = contact.Documents.AddNew();
				document.OD_DocumentGroup = "ALL";
				document.OD_DefaultContact = true;
			}

			if (customsCodeType != null)
			{
				organisation.CustomsCodes.AddNew(customsCodeType, customsCode);
			}

			if (organisation.OH_Code.IsEmpty)
			{
				organisation.OH_Code = organisation.OH_FullName.Left(5).Trim() + address.OA_RL_NKRelatedPortCode;
			}

			return organisation;
		}

		public static MAFMessagingBO GetMAFMessaging(ForwardingConsol consol)
		{
			return new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(consol));
		}
		#endregion
	}
}
