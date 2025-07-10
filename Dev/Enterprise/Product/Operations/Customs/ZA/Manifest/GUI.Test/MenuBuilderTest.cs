using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.MessagingProcess;
using Enterprise.Customs.ZA.Manifest.Business;
using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Environment.Testing.UserContextTest;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ZA.Manifest.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestCancelOptionWhenCarnPresent()
		{
			AssertCancelOption(header => header.CARN = "123456");
		}

		public void TestAmendOptionWhenCarnPresent()
		{
			AssertAmendOption(header => header.CARN = "123456");
		}

		public void TestCancelOptionWhenRegistartionNumberPresent()
		{
			AssertCancelOption(header => header.RegistrationNumber = "REGNO1234");
		}

		public void TestAmendOptionWhenRegistrationNumberPresent()
		{
			AssertAmendOption(header => header.RegistrationNumber = "REGNO1234");
		}

		void AssertCancelOption(Action<AsycudaManifestHeader> putMainfestHeaderIntoAStateThatAllowsAModificationMessage)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, "ZA", "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList("SB", C.RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara Point Cruz Seaport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "SEA", "SBHIR");

			var zaJHB = helper.CreateNewOrGetExistingCusCodeList("ZA", C.RefCusCodeListTypes.Codes.CustomsOffice, "JHB", "JOHANNESBURG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(zaJHB.PK, "Code", "10");

			ASYCUDA.Business.Testing.AsycudaManifestHeaderTestHelper.EnsureOrCreateManifestTypesDataInZZ(Core.Constants.CountryCodes.SouthAfrica, Factory);
			Factory.Save();

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("AGT", "11111111", "ZA");
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SBHIR";
			header.AMA_RL_NKPortOfDischarge = "ZAJNB";
			header.AMA_JobReference = "I have changes now";
			header.AMA_E_ARV = ZDateTime.Now;
			header.AMA_TransportMode = "ROA";
			header.AMA_RN_NKCountry = "ZA";
			header.AMA_CustomsOffice = "Y";
			header.AMA_ManifestType = "RFM";
			var bill = header.Bills.AddNew();
			bill.ABL_BillIssuer = "X";
			putMainfestHeaderIntoAStateThatAllowsAModificationMessage(header);
			AssertEquals("Pre-Req - cancel is allowed", true, header.MessageStatusProvider.AllowCancellationMessage(header));
			Factory.Save();

			using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

					AssertEquals("&Amend Manifest", menu.MenuItems[0].Text);
					AssertEquals("&Cancel Manifest", menu.MenuItems[1].Text);
					menu.MenuItems[1].PerformClick();

					AssertEquals(1, header.Messages.Count);
					var message = ((IEDIMessageCollectionProvider)header).Messages.LastOutgoingMessage;
					AssertContains("Cancellation message text\r\n" + message.EM_MessageText, "+1'DTM+137:", message.EM_MessageText);
				}
			}
		}

		void AssertAmendOption(Action<AsycudaManifestHeader> putMainfestCountryIntoAStateThatAllowsAModificationMessage)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", C.RefCusCodeListTypes.Codes.ManifestCountry, "ZA", "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList("SB", C.RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara Point Cruz Seaport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "SEA", "SBHIR");

			var zaJHB = helper.CreateNewOrGetExistingCusCodeList("ZA", C.RefCusCodeListTypes.Codes.CustomsOffice, "JHB", "JOHANNESBURG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(zaJHB.PK, "Code", "10");

			ASYCUDA.Business.Testing.AsycudaManifestHeaderTestHelper.EnsureOrCreateManifestTypesDataInZZ(Core.Constants.CountryCodes.SouthAfrica, Factory);
			Factory.Save();

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("AGT", "11111111", "ZA");
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SBHIR";
			header.AMA_RL_NKPortOfDischarge = "ZAJNB";
			header.AMA_JobReference = "I have changes now";
			header.AMA_E_ARV = ZDateTime.Now;
			header.AMA_TransportMode = "ROA";
			header.AMA_RN_NKCountry = "ZA";
			header.AMA_CustomsOffice = "Y";
			header.AMA_ManifestType = "RFM";
			var bill = header.Bills.AddNew();
			bill.ABL_BillIssuer = "X";
			putMainfestCountryIntoAStateThatAllowsAModificationMessage(header);
			AssertEquals("Pre-Req - amend is allowed", true, header.MessageStatusProvider.AllowModificationMessage(header));
			Factory.Save();

			using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

					AssertEquals("&Amend Manifest", menu.MenuItems[0].Text);
					AssertEquals("&Cancel Manifest", menu.MenuItems[1].Text);
					menu.MenuItems[0].PerformClick();

					AssertEquals(1, header.Messages.Count);
					var message1 = ((IEDIMessageCollectionProvider)header).Messages.LastOutgoingMessage;
					AssertContains("Amend message text\r\n" + message1.EM_MessageText, "+4'DTM+137:", message1.EM_MessageText);
				}
			}
		}

		public void TestFeatureControlProtectionOfMenuItemForZAManifestSending()
		{
			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			var scenarios = new List<bool>()
			{ false, true };
			Action<bool> testAction = isFeatureEnabled =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.ZAManifestCaseNumbers,
					Core.Constants.CountryCodes.SouthAfrica, ZDate.Today, isFeatureEnabled))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
					header.AMA_RN_NKCountry = countryCode;
					var bill = header.Bills.AddNew();

					using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
					{
						using (var form = new ZForm(header))
						{
							form.Menu.MenuItems.Add(menu);
							form.Show();
							menu.OnPopup(EventArgs.Empty);

							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

							AssertEquals("Send &Manifest", menu.MenuItems[0].Text);

							var assertMsg = "For ZA Manifests, is the sending of Supporting Documents feature enabled?";
							AssertEquals(assertMsg, isFeatureEnabled, menu.MenuItems.ToList<MenuItem>().Any(x => x.Text == "Send Supporting &Documents"));
						}
					}
				}
			};
			scenarios.ForEach(testAction);
		}

		public void TestRefreshUcrAndLrnMenuVisibility()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				AssertRefreshUcrAndLrnMenuVisibility("111", false, false);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				AssertRefreshUcrAndLrnMenuVisibility("222", false, false);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				AssertRefreshUcrAndLrnMenuVisibility("333", true, true);
			}
		}

		void AssertRefreshUcrAndLrnMenuVisibility(string reference, bool shouldbeVisible, bool attachConsol)
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = reference;

			if (attachConsol)
			{
				var consol = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingConsol>();
				manifestHeader.SetParent(consol);
			}

			Factory.Save();

			using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(manifestHeader))
			{
				using (var form = new ZForm(manifestHeader))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					if (shouldbeVisible)
					{
						AssertNotNull(menu.MenuItems[0].MenuItems.FindByText("Refresh UCR and LRN data"));
					}
					else
					{
						AssertNull(menu.MenuItems[0].MenuItems.FindByText("Refresh UCR and LRN data"));
					}
				}
			}
		}

		public void TestRefreshUcrAndLrnOnZAMasterManifest()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var shipment1 = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
				shipment1.JS_HouseBill = "SHIP111";
				var declaration1 = Factory.New<ZA.Business.JobDeclaration>();
				declaration1.JE_MasterBill = "MBL111";
				var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
				entryHeader1.CH_BGMReference = "LRN1";
				var entryHeader11 = declaration1.ActiveEntryHeaders.AddNew();
				entryHeader11.CH_BGMReference = "LRN11";
				var entryInstruction1 = declaration1.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_DateForDuty = ZDateTime.BrettsBirthday.AddDays(1);
				entryInstruction1.CEI_UCROverride = "UCR1";
				var entryInstruction11 = declaration1.CustomsEntryInstructions.AddNew();
				entryInstruction11.CEI_DateForDuty = ZDateTime.BrettsBirthday;
				entryInstruction11.CEI_UCROverride = "UCR11";
				declaration1.JE_JS = shipment1.PK;

				var consol1 = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingConsol>();
				consol1.Shipments.Add(shipment1);

				var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
				manifestHeader.AMA_ManifestType = nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.RFM);
				var bill1 = manifestHeader.Bills.AddNew();
				bill1.ABL_BillNumber = "MBL111";
				manifestHeader.AMA_ParentId = consol1.PK;
				manifestHeader.AMA_ParentTableCode = ZArchitecture.Schema.JobConsolSchema.Constants.Prefix;

				AssertEquals("Pre-req", "", bill1.ABL_UCRNumber);
				AssertEquals("LRN count", 0, bill1.CustomsEntryNumbers.OfType<ABLEntryNum>().Count());

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(manifestHeader))
				{
					using (var form = new ZForm(manifestHeader))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						menu.MenuItems[0].MenuItems.FindByText("Refresh UCR and LRN data").PerformClick();
					}
				}

				Factory.Save();

				AssertEquals("Bill 'MBL111': UCR has been updated from '' to 'UCR11'.\nBill 'MBL111': LRN 'LRN1' has been added.\nBill 'MBL111': LRN 'LRN11' has been added.\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("UCR updated from Master bill", "UCR11", bill1.ABL_UCRNumber);
				AssertEquals("Two new LRNs added", 2, bill1.CustomsEntryNumbers.OfType<ABLEntryNum>().Count());
				var cenList = bill1.CustomsEntryNumbers.OfType<ABLEntryNum>().Select(e => (string)e.CE_EntryNum);
				AssertContainsExactElementsInAnyOrder(new List<string> { "LRN1", "LRN11" }, cenList);
			}
		}

		public void TestRefreshUcrAndLrnOnZAManifest()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var shipment1 = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
				shipment1.JS_HouseBill = "SHIP1";
				var declaration1 = Factory.New<ZA.Business.JobDeclaration>();
				declaration1.JE_HouseBill = "HBL1";
				var entryHeader1 = declaration1.ActiveEntryHeaders.AddNew();
				entryHeader1.CH_BGMReference = "LRN1";
				var entryHeader11 = declaration1.ActiveEntryHeaders.AddNew();
				entryHeader11.CH_BGMReference = "LRN11";
				var entryInstruction1 = declaration1.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_DateForDuty = ZDateTime.BrettsBirthday.AddDays(1);
				entryInstruction1.CEI_UCROverride = "UCR1";
				var entryInstruction11 = declaration1.CustomsEntryInstructions.AddNew();
				entryInstruction11.CEI_DateForDuty = ZDateTime.BrettsBirthday;
				entryInstruction11.CEI_UCROverride = "UCR11";
				declaration1.JE_JS = shipment1.PK;

				var shipment2 = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
				shipment2.JS_HouseBill = "SHIP2";
				var declaration2 = Factory.New<ZA.Business.JobDeclaration>();
				declaration2.JE_HouseBill = "HBL2";
				var entryHeader2 = declaration2.ActiveEntryHeaders.AddNew();
				entryHeader2.CH_BGMReference = "LRN2";
				var entryInstruction2 = declaration2.CustomsEntryInstructions.AddNew();
				declaration2.JE_JS = shipment2.PK;

				var shipment3 = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
				shipment2.JS_HouseBill = "SHIP3";

				var shipment4 = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
				shipment4.JS_HouseBill = "SHIP4";
				var declaration4 = Factory.New<ZA.Business.JobDeclaration>();
				declaration4.JE_HouseBill = "HBL4";
				var entryHeader4 = declaration4.ActiveEntryHeaders.AddNew();
				entryHeader4.CH_BGMReference = "LRN4";
				var entryInstruction4 = declaration4.CustomsEntryInstructions.AddNew();
				entryInstruction4.CEI_UCROverride = "UCR4";
				declaration4.JE_JS = shipment4.PK;

				var consol1 = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingConsol>();
				consol1.Shipments.Add(shipment1);
				consol1.Shipments.Add(shipment2);
				consol1.Shipments.Add(shipment3);
				consol1.Shipments.Add(shipment4);

				var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
				manifestHeader.AMA_ManifestType = nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.RFM); //Supports multiple CusEntryNums
				AssertEquals(true, manifestHeader.SupportMultipleCustomsNumbers);
				var bill1 = manifestHeader.Bills.AddNew();
				bill1.ABL_BillNumber = "HBL1";
				bill1.ABL_UCRNumber = "A";

				var lrn1 = bill1.CustomsEntryNumbers.AddNew();
				lrn1.CE_EntryType = ZaLRNTypes.Codes.AFM;
				lrn1.CE_EntryNum = "LRN1";
				var lrn111 = bill1.CustomsEntryNumbers.AddNew();
				lrn111.CE_EntryType = ZaLRNTypes.Codes.ABT;
				lrn111.CE_EntryNum = "LRN111";

				var bill2 = manifestHeader.Bills.AddNew();
				bill2.ABL_BillNumber = "B";
				bill2.ABL_UCRNumber = "B";
				var bill3 = manifestHeader.Bills.AddNew();
				bill3.ABL_BillNumber = "C";
				bill3.ABL_UCRNumber = "C";
				var bill4 = manifestHeader.Bills.AddNew();
				bill4.ABL_BillNumber = "HBL4";
				bill4.ABL_UCRNumber = "D";

				manifestHeader.AMA_ParentId = consol1.PK;
				manifestHeader.AMA_ParentTableCode = ZArchitecture.Schema.JobConsolSchema.Constants.Prefix;

				AssertEquals("Pre-req", "A", bill1.ABL_UCRNumber);
				AssertEquals("Pre-req", "B", bill2.ABL_UCRNumber);
				AssertEquals("Pre-req", "C", bill3.ABL_UCRNumber);
				AssertEquals("Pre-req", "D", bill4.ABL_UCRNumber);
				AssertEquals("2 existing LRNs on Bill1", 2, bill1.CustomsEntryNumbers.Count);
				AssertEquals("No LRNs on Bill2", 0, bill2.CustomsEntryNumbers.Count);
				AssertEquals("No LRNs on Bill3", 0, bill3.CustomsEntryNumbers.Count);
				AssertEquals("No LRNs on Bill4", 0, bill4.CustomsEntryNumbers.Count);

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(manifestHeader))
				{
					using (var form = new ZForm(manifestHeader))
					{
						form.Menu.MenuItems.Add(menu);
						form.Show();
						menu.OnPopup(EventArgs.Empty);
						menu.MenuItems[0].MenuItems.FindByText("Refresh UCR and LRN data").PerformClick();
					}
				}

				Factory.Save();

				AssertEquals("Bill 'HBL1': UCR has been updated from 'A' to 'UCR11'.\nBill 'HBL1': LRN 'LRN11' has been added.\nBill 'HBL4': UCR has been updated from 'D' to 'UCR4'.\nBill 'HBL4': LRN 'LRN4' has been added.\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("UCR updated to declaration's first entry instruction UCR Override ordered by assesment date", "UCR11", bill1.ABL_UCRNumber);
				AssertEquals("UCR not changed", "B", bill2.ABL_UCRNumber);
				AssertEquals("UCR not changed", "C", bill3.ABL_UCRNumber);
				AssertEquals("UCR updated", "UCR4", bill4.ABL_UCRNumber);
				AssertEquals("One LRN added, one a duplicate", 3, bill1.CustomsEntryNumbers.Count);
				AssertEquals(0, bill2.CustomsEntryNumbers.Count);
				AssertEquals(0, bill3.CustomsEntryNumbers.Count);
				AssertEquals("One LRN added", 1, bill4.CustomsEntryNumbers.Count);

				var cenList1 = bill1.CustomsEntryNumbers.OfType<ABLEntryNum>().Select(x => (string)x.CE_EntryNum);
				Assert(cenList1.ContainsSameElementsInAnyOrder(new List<string> { "LRN1", "LRN11", "LRN111" }));
				var cenList4 = bill4.CustomsEntryNumbers.OfType<ABLEntryNum>().Select(x => (string)x.CE_EntryNum);
				Assert(cenList4.ContainsSameElementsInAnyOrder(new List<string> { "LRN4" }));
			}
		}

		public void TestSendManifestLevelPOCMenuItem()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			header.AMA_ManifestType = nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.RFM);

			AssertPOCMenuItem(header);
		}

		public void TestSendBillLevelPOCMenuItem()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			header.AMA_ManifestType = nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.ALH);

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "B0001";
			bill1.ABL_BillIssuer = "Billy";

			AssertPOCMenuItem(header);
		}

		public void AssertPOCMenuItem(AsycudaManifestHeader header)
		{
			var user = new UserForTest();
			user.IsDeveloper = true;
			var context = new UserContextForTest(user, Env.CurrentCompany);

			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, true);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();

					Func<MenuItem> getPOCMenuItem = () =>
					{
						menu.OnPopup(EventArgs.Empty);
						var pocMenu = menu.MenuItems.FindByText("Send &Manifest POC");

						return pocMenu;
					};

					using (Env.SetTemporaryUserContext(context))
					{
						using (MessagingPOCHelper.TemporarilyEnablePOC(enabled: false))
						{
							var pocMenu = getPOCMenuItem();
							AssertNotNull("Send Manifest POC menu item not found", pocMenu);
							AssertEquals("developer", true, pocMenu.Visible);

							user.IsDeveloper = false;
							pocMenu = getPOCMenuItem();
							AssertNull("non-developer - no menu", pocMenu);
						}

						using (MessagingPOCHelper.TemporarilyEnablePOC(enabled: true))
						{
							var pocMenu = getPOCMenuItem();
							AssertNotNull("Send Manifest POC menu item not found", pocMenu);
							AssertEquals("FUNCS enabled", true, pocMenu.Visible);
						}
					}
				}
			}
		}

		public void TestSendManifestLevelMessagePOC()
		{
			AssertMessagesSentOnClick(false);
		}

		public void TestSendBillLevelMessagePOC()
		{
			AssertMessagesSentOnClick(true);
		}

		void AssertMessagesSentOnClick(bool isBillLevel)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (MessagingPOCHelper.TemporarilyEnablePOC(enabled: true))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
				header.AMA_ManifestType = isBillLevel ? nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.ALH) : nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.RFM);
				header.AMA_RL_NKPortOfLoading = "GBLON";
				header.AMA_RL_NKPortOfDischarge = "ZADUR";

				var bill1 = header.Bills.AddNew();
				bill1.ABL_BillNumber = "B0001";
				bill1.ABL_BillIssuer = "Billy";

				using (var menu = new ASYCUDA.GUI.Testing.AsycudaMenuForTest(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();

					menu.OnPopup(EventArgs.Empty);
					var pocMenu = menu.MenuItems.FindByText("Send &Manifest POC");
					AssertNotNull("Send Manifest POC menu item not found", pocMenu);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save?

					if (isBillLevel)
					{
						ZFormModaliser.ShowDialogsInTest = true;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // SendDialog
						ZFormModaliser.SetDelegateToCallOnFormShown((dialog) =>
						{
							if (dialog is ASYCUDA.GUI.AsycudaItemSelectionDialog selectionDlg)
							{
								selectionDlg.SelectAll();
							}
						});
					}

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Validation errors
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					pocMenu.PerformClick();

					AssertContains("Msg should be sent", "Message(s) queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}
	}
}
