using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	sealed class InBondPlugInTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			using (var plugin = GetNewPlugIn(Shipment))
			{
				AssertEquals(Shipment, plugin.HostBusinessEntityInternal);
			}

			using (var plugin = GetNewPlugIn(Consol))
			{
				AssertEquals(Consol, plugin.HostBusinessEntityInternal);
			}
		}

		public void TestMenuItemWhenSecurityRightsDenied()
		{
			using (var plugin = GetNewPlugIn(Shipment))
			{
				plugin.SecurityCheckpointForMessagingMenuInternal.IsAllowed = true;
				var menuItem = ZArchitecture.GUI.ZMenuItemCollectionExtensions.FindByText(plugin.GetNewTopLevelMenuInternal().MenuItems, "The menu is disabled because you don't have the appropriate security rights: " + Env.Security.USInBondMessaging.DisplayTextPathToSecurityRight);
				AssertNull("In-Bond", menuItem);

				plugin.SecurityCheckpointForMessagingMenuInternal.IsAllowed = false;
				menuItem = ZArchitecture.GUI.ZMenuItemCollectionExtensions.FindByText(plugin.GetNewTopLevelMenuInternal().MenuItems, "The menu is disabled because you don't have the appropriate security rights: " + Env.Security.USInBondMessaging.DisplayTextPathToSecurityRight);
				AssertNotNull("In-Bond", menuItem);
			}
		}

		public void TestBondedWarehouseSecurityCheckOnShowPreSaveDialogs()
		{
			var expectedMessage = "You do not have security rights to save a Bonded Warehousing job. ";
			expectedMessage += Env.Security.USInBondEditBondedWarehouse.DisplayTextPathToSecurityRight;

			Env.Security.USInBondEditBondedWarehouse.IsAllowed = false;
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "SDF32";
			importer.MainAddress.OA_Address1 = "ADFSD";
			importer.CompanyData.OB_IMUsedBondedWhs = true;

			using (var plugIn = GetNewPlugIn(Shipment))
			{
				//not in database, no gui shown, no security check
				plugIn.CreateNewInBondInternal();
				var header = plugIn.InBond;
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				AssertEquals(false, header.HasAtLeastOneMovementWithWHSTransaction);
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				Env.Security.USInBondEditBondedWarehouse.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				//not in database, gui shown, no security check
				plugIn.OnGUIShown();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				//not in database, user control shown, security check
				plugIn.OnUserControlShown();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				header.Delete();
			}

			using (var plugIn = GetNewPlugIn(Shipment))
			{
				//not in database, menu item selected control shown, security check
				plugIn.CreateNewInBondInternal();
				var header = plugIn.InBond;
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				AssertEquals(false, header.HasAtLeastOneMovementWithWHSTransaction);
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				plugIn.OnGUIShown();

				if (plugIn.TopLevelMenu.MenuItems.Count > 0)
				{
					plugIn.TopLevelMenu.MenuItems[1].PerformSelect();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					plugIn.ShowPreSaveDialogs();
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				//saved, no changes, user control shown, no security check
				Env.Security.USInBondEditBondedWarehouse.IsAllowed = true;
				plugIn.Factory.Save();
				Env.Security.USInBondEditBondedWarehouse.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				//saved, haschanges, user control shown, security check
				header.BH_CarrierSCAC = "SCDK";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				plugIn.ShowPreSaveDialogs();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestName()
		{
			using (var plugin = GetNewPlugIn(Shipment))
			{
				AssertEquals("In-Bond", plugin.Name);
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var plugin = GetNewPlugIn(Shipment))
			{
				AssertEquals(Env.Licence.ImportBroker, plugin.LicenceCheckPointInternal);
			}
		}

		public void TestGetNewUserControl()
		{
			using (var plugin = GetNewPlugIn(Shipment))
			using (var control = plugin.GetNewUserControlInternal())
			{
				AssertType<USInBondUserControl>(control);
			}
		}

		public void TestGetBusinessEntityForPlugIn()
		{
			using (var plugin = GetNewPlugIn(Shipment))
			{
				AssertType<CusInBondHeader>(plugin.BusinessEntity);
			}
		}

		public void TestHasUserControl()
		{
			using (var plugin = GetNewPlugIn(Shipment))
			{
				AssertNotNull(plugin.UserControl);
			}
		}

		public void TestQueryUserToCreateInbond()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var plugin = GetNewPlugIn(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = false;
				Env.Security.USInBondNew.IsAllowed = false;
				plugin.Enabled = false;
				plugin.Enabled = true;
				plugin.TopLevelMenu.PerformClick();
				AssertNull("Are you sure you want to create an In-Bond Movement now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(Env.Security.USInBondNew.ErrorMessageForNotAllowed, plugin.PlugInNotDisplayedMessage);
			}

			using (var plugin = GetNewPlugIn(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = false;
				Env.Security.USInBondNew.IsAllowed = true;
				plugin.Enabled = false;
				plugin.Enabled = true;
				plugin.TopLevelMenu.PerformClick();
				AssertEquals("Are you sure you want to create a Movement now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(plugin.NotToCreateJobTextInternal, plugin.PlugInNotDisplayedMessage);
			}

			using (var plugin = GetNewPlugIn(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
				Env.Security.USInBondNew.IsAllowed = true;
				plugin.Enabled = false;
				plugin.Enabled = true;
				plugin.TopLevelMenu.PerformClick();
				AssertEquals("Are you sure you want to create a Movement now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(plugin.NotToCreateJobTextInternal, plugin.PlugInNotDisplayedMessage);
			}

			using (USCustomsDataRegistry.Instance.TransportModeForInBondCreationFromConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TransportModeForInBondCreationFromConsolList.Codes.AIR))
			using (var plugin = GetNewPlugIn(Consol))
			{
				Consol.JK_UniqueConsignRef = "C00000001";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
				Env.Security.USInBondNew.IsAllowed = true;
				plugin.Enabled = false;
				plugin.Enabled = true;
				plugin.TopLevelMenu.PerformClick();

				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Please click In-Bond tab and create In-Bond firstly.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLoadCorrectInBond()
		{
			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "S#@";
			otherCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "G#@";
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var shipment = Factory.New<ForwardingShipment>();
			var notInBond = Factory.New<CusInBondHeader>();
			notInBond.BH_ParentID = shipment.PK;
			notInBond.BH_ParentTableCode = shipment.TablePrefix;
			notInBond.BH_ApplicationCode = "Z!@";

			var inBondInDiffComp = Factory.New<CusInBondHeader>();
			inBondInDiffComp.BH_ParentID = shipment.PK;
			inBondInDiffComp.BH_ParentTableCode = shipment.TablePrefix;
			inBondInDiffComp.BH_GB = otherBranch.PK;

			var inBond = Factory.New<CusInBondHeader>();
			inBond.BH_ParentID = shipment.PK;
			inBond.BH_ParentTableCode = shipment.TablePrefix;

			Factory.Save();

			using (var plugin = GetNewPlugIn(shipment))
			{
				AssertEquals(inBond, plugin.InBond);
			}
		}

		public void TestMutexIsUsedInCreationgOfInBond()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var shipment = factory1.New<ForwardingShipment>();
			var declaratation = factory1.New<JobDeclaration>();
			declaratation.JE_JS = shipment.PK;
			declaratation.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaratation.JE_TransportMode = declaratation.TransportModeSeaCodeForTesting;
			declaratation.JE_VesselName = "ABC VESSEL";
			factory1.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var shipmentInFactory2 = factory2.Load<ForwardingShipment>(shipment.PK);

			using (var pluginFactory2 = GetNewPlugIn(shipmentInFactory2))
			using (var pluginFactory1 = GetNewPlugIn(shipment))
			{
				pluginFactory2.CreateInBond();
				AssertEquals("pluginFactory1.Mutex.IsLocked", true, pluginFactory1.Mutex.IsLocked);
				AssertEquals("pluginFactory2.Mutex.IsLocked", true, pluginFactory2.Mutex.IsLocked);
				var inBondFactory1 = pluginFactory1.InBond;
				AssertNull("inBondFactory1 should be null due to the mutex", inBondFactory1);
				var inBondFactory2 = pluginFactory2.InBond;
				AssertNotNull("inBondFactory2", inBondFactory2);
				factory2.Save();
				AssertEquals("pluginFactory1.Mutex.IsLocked", false, pluginFactory1.Mutex.IsLocked);
				AssertEquals("pluginFactory2.Mutex.IsLocked", false, pluginFactory2.Mutex.IsLocked);
				inBondFactory1 = pluginFactory1.InBond;
				AssertEquals("inBondFactory1 should be matched", inBondFactory2.PK, inBondFactory1.PK);
				AssertEquals("inBondFactory2", inBondFactory2, pluginFactory2.InBond);
				AssertEquals("inBondFactory1.BH_ImportConveyanceName", "ABC VESSEL", inBondFactory1.BH_ImportConveyanceName);
				AssertEquals("inBondFactory1.BH_ImportConveyanceNameInfo.ReadOnly", true, inBondFactory1.BH_ImportConveyanceNameInfo.ReadOnly);
				AssertEquals("inBondFactory2.BH_ImportConveyanceName", "ABC VESSEL", inBondFactory2.BH_ImportConveyanceName);
				AssertEquals("inBondFactory2.BH_ImportConveyanceNameInfo.ReadOnly", true, inBondFactory2.BH_ImportConveyanceNameInfo.ReadOnly);
			}
		}

		public void TestInBondIsExcludedFromParentMessageNotifications()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var provider = dec as IMessageNotificationsProvider;

			using (var plugin = new InBondPlugIn(dec))
			{
				plugin.CreateInBond();
				plugin.InBond.FillWithValidTestData();
				Assert(provider.ExcludedChildrenAndTheirDescendentsFromMessageNotifications.Any(x => x == plugin.InBond));
				Factory.Save();
			}

			using (var plugin = new InBondPlugIn(dec))
			{
				Assert(provider.ExcludedChildrenAndTheirDescendentsFromMessageNotifications.Any(x => x == plugin.InBond));
			}
		}

		public void TestPreventDuplicateMovementCreation()
		{
			const string notification = "Movement has already been created and linked to this Job.";

			// CusInBondHeader was created before CreateInBond was called.
			using var plugIn = new InBondPlugIn(Shipment);

			var result = plugIn.CreateInBond();

			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals(notification, plugIn.PlugInNotDisplayedMessage);
				AssertEquals(notification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
			});
		}

		public void TestBillsTabControlsWhenAMSHBREffectiveOnShipment()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "009008";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "MAS1";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;

			shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.JS_RL_NKOrigin = "AUMEL";
			Shipment.JS_RL_NKDestination = "USLAX";
			consol.Shipments.Add(Shipment);

			var container1 = CreateContainer("CONT1001", 1, "101", 8860);
			var packline1 = CreatePackline(6, "PLT", 120, "FR", "1", "340026560");
			packline1.SetContainer(consol, container1);

			var packline11 = CreatePackline(2, "DOZ", 51, "IT", "2", "1020304500");
			packline11.SetContainer(consol, container1);

			var container2 = CreateContainer("CONT1002", 1, "2056", 510);
			var packline2 = CreatePackline(4, "DOZ", 23, "BF", "3", "10203040");
			packline2.SetContainer(consol, container2);

			CreateContainer("", 3, "508", 2700);

			var packline3 = CreatePackline(8, "KEG", 3, "AU", "4", "");
			var packline4 = CreatePackline(9, "DOZ", 70, "", "5", "1010201030");

			var carrier0 = Factory.New<USCarrierCombined>();
			carrier0.UI_Code = "MAS1";
			carrier0.UI_ModeOfTransportation = US.Messaging.Business.TransportModeCodes.Codes.VesselContainer;

			using (var plugin = GetNewPlugIn(Shipment))
			{
				AssertEquals("In-Bond should be created and synchronized", true, plugin.CreateInBond());
				var inBondHeader = (CusInBondHeader)plugin.BusinessEntity;
				AssertEquals("11", inBondHeader.BH_ImportTransportMode);
				AssertEquals(1, inBondHeader.Bills.Count);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
				{
					using (USInBondForm form = new USInBondForm(inBondHeader))
					{
						var billsTab = form.Controls.Find("BillsTabPage", true);
						form.Show();
						var billsTabPage = billsTab[0] as ZTabPage;
						billsTabPage.Show();

						var inbondBillsUserControl = billsTabPage.Controls[0] as USInBondBillsUserControl;
						inbondBillsUserControl.Show();

						var issuerControl = inbondBillsUserControl.Controls.Find("B0_HouseBillIssuerCodeCodeFindBox", true)[0] as ZCodeFindBox;
						AssertEquals("Hide HouseBillIsserCodeFindBox", false, issuerControl.Visible);
						AssertEquals("Hide B0_HouseBillNumberTextBox", false, inbondBillsUserControl.B0_HouseBillNumberTextBox.Visible);
					}
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
				{
					using (USInBondForm form = new USInBondForm(inBondHeader))
					{
						var billsTab = form.Controls.Find("BillsTabPage", true);
						form.Show();
						var billsTabPage = billsTab[0] as ZTabPage;
						billsTabPage.Show();

						var inbondBillsUserControl = billsTabPage.Controls[0] as USInBondBillsUserControl;
						inbondBillsUserControl.Show();

						var issuerControl = inbondBillsUserControl.Controls.Find("B0_HouseBillIssuerCodeCodeFindBox", true)[0] as ZCodeFindBox;
						AssertEquals("Show HouseBillIsserCodeFindBox", true, issuerControl.Visible);
						AssertEquals("Show B0_HouseBillNumberTextBox", true, inbondBillsUserControl.B0_HouseBillNumberTextBox.Visible);
					}
				}
			}
		}

		public void TestBillsTabControlsWhenAMSHBREffectiveOnDeclaration()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "USCHI";
			importer.OH_Code = "IMP002";
			importer.OH_FullName = "Importer Two Co. Ltd";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.Import;
			declaration.US_IssueCode = "APU";
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "ABC VESSEL";

			using (var plugin = GetNewPlugIn(declaration))
			{
				plugin.CreateInBond();
				var inBondHeader = (CusInBondHeader)plugin.BusinessEntity;
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
				{
					using (USInBondForm form = new USInBondForm(inBondHeader))
					{
						var billsTab = form.Controls.Find("BillsTabPage", true);
						form.Show();
						var billsTabPage = billsTab[0] as ZTabPage;
						billsTabPage.Show();

						var inbondBillsUserControl = billsTabPage.Controls[0] as USInBondBillsUserControl;
						inbondBillsUserControl.Show();

						var issuerControl = inbondBillsUserControl.Controls.Find("B0_HouseBillIssuerCodeCodeFindBox", true)[0] as ZCodeFindBox;
						AssertEquals("Hide HouseBillIsserCodeFindBox", false, issuerControl.Visible);
						AssertEquals("Hide B0_HouseBillNumberTextBox", false, inbondBillsUserControl.B0_HouseBillNumberTextBox.Visible);
					}
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
				{
					using (USInBondForm form = new USInBondForm(inBondHeader))
					{
						var billsTab = form.Controls.Find("BillsTabPage", true);
						form.Show();
						var billsTabPage = billsTab[0] as ZTabPage;
						billsTabPage.Show();

						var inbondBillsUserControl = billsTabPage.Controls[0] as USInBondBillsUserControl;
						inbondBillsUserControl.Show();

						var issuerControl = inbondBillsUserControl.Controls.Find("B0_HouseBillIssuerCodeCodeFindBox", true)[0] as ZCodeFindBox;
						AssertEquals("Show HouseBillIsserCodeFindBox", true, issuerControl.Visible);
						AssertEquals("Show B0_HouseBillNumberTextBox", true, inbondBillsUserControl.B0_HouseBillNumberTextBox.Visible);
					}
				}
			}
		}

		public void TestCreateInBond()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "009008";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "MAS1";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;

			shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.JS_RL_NKOrigin = "AUMEL";
			Shipment.JS_RL_NKDestination = "USLAX";
			consol.Shipments.Add(Shipment);

			var container1 = CreateContainer("CONT1001", 1, "101", 8860);
			var packline1 = CreatePackline(6, "PLT", 120, "FR", "1", "340026560");
			packline1.SetContainer(consol, container1);

			var packline11 = CreatePackline(2, "DOZ", 51, "IT", "2", "1020304500");
			packline11.SetContainer(consol, container1);

			var container2 = CreateContainer("CONT1002", 1, "2056", 510);
			var packline2 = CreatePackline(4, "DOZ", 23, "BF", "3", "10203040");
			packline2.SetContainer(consol, container2);

			CreateContainer("", 3, "508", 2700);

			var packline3 = CreatePackline(8, "KEG", 3, "AU", "4", "");
			var packline4 = CreatePackline(9, "DOZ", 70, "", "5", "1010201030");

			var carrier0 = Factory.New<USCarrierCombined>();
			carrier0.UI_Code = "MAS1";
			carrier0.UI_ModeOfTransportation = US.Messaging.Business.TransportModeCodes.Codes.VesselContainer;

			using (var plugin = GetNewPlugIn(Shipment))
			{
				AssertEquals("In-Bond should be created and synchronized", true, plugin.CreateInBond());
				var businessEntity = (CusInBondHeader)plugin.BusinessEntity;
				AssertEquals("11", businessEntity.BH_ImportTransportMode);
				AssertEquals(1, businessEntity.Bills.Count);
				var bill = businessEntity.Bills[0];
				AssertEquals("MAS1", bill.B0_IssuerCode);
				AssertEquals("009008", bill.B0_MasterBillNumber);

				AssertEquals(1, businessEntity.MovementHeaders.Count);
				var moveHeader = businessEntity.MovementHeaders[0];
				AssertEquals(1, moveHeader.MovementDetails.Count);

				var moveDetail = moveHeader.MovementDetails[0];
				AssertEquals("MAS1", moveDetail.Bill.B0_IssuerCode);
				AssertEquals("009008", moveDetail.Bill.B0_MasterBillNumber);

				AssertEquals(3, moveDetail.Containers.Count);

				var inBondContainers = moveDetail.Containers.Find(x => x.BC_ContainerNum == "CONT1001");
				var inBondContainer1 = inBondContainers.First();

				AssertNotNull(inBondContainer1);
				AssertEquals("CONT1001", inBondContainer1.BC_ContainerNum);
				AssertEquals("101", inBondContainer1.BC_Seal1);
				AssertEquals(2, inBondContainer1.Commodities.Count);

				var commodities = inBondContainer1.Commodities.Find(x => x.BY_MarksAndNumbers == "1");
				var commodity = commodities.First();
				AssertEquals(6, commodity.BY_PieceCount);
				AssertEquals(120m, commodity.BY_GrossWeight);
				AssertEquals("340026560", commodity.BY_HarmonisedTariff);
				AssertEquals("FR", commodity.BY_RN_NKCountryOfOrigin);

				commodities = inBondContainer1.Commodities.Find(x => x.BY_MarksAndNumbers == "2");
				commodity = commodities.First();
				AssertEquals(2, commodity.BY_PieceCount);
				AssertEquals(51m, commodity.BY_GrossWeight);
				AssertEquals("1020304500", commodity.BY_HarmonisedTariff);
				AssertEquals("IT", commodity.BY_RN_NKCountryOfOrigin);

				inBondContainers = moveDetail.Containers.Find(x => x.BC_ContainerNum == "CONT1002");
				var inBondContainer2 = inBondContainers.First();
				AssertNotNull(inBondContainer2);
				AssertEquals("CONT1002", inBondContainer2.BC_ContainerNum);
				AssertEquals("2056", inBondContainer2.BC_Seal1);
				AssertEquals(1, inBondContainer2.Commodities.Count);

				commodities = inBondContainer2.Commodities.Find(x => x.BY_MarksAndNumbers == "3");
				commodity = commodities.First();
				AssertEquals(4, commodity.BY_PieceCount);
				AssertEquals(23m, commodity.BY_GrossWeight);
				AssertEquals("10203040", commodity.BY_HarmonisedTariff);
				AssertEquals("BF", commodity.BY_RN_NKCountryOfOrigin);

				inBondContainers = moveDetail.Containers.Find(x => x.BC_ContainerNum == "");
				var inBondContainer3 = inBondContainers.First();
				AssertNotNull(inBondContainer3);
				AssertEquals("", inBondContainer3.BC_ContainerNum);
				AssertEquals("508", inBondContainer3.BC_Seal1);
				AssertEquals(2, inBondContainer3.Commodities.Count);

				commodities = inBondContainer3.Commodities.Find(x => x.BY_MarksAndNumbers == "4");
				commodity = commodities.First();
				AssertEquals(8, commodity.BY_PieceCount);
				AssertEquals(3m, commodity.BY_GrossWeight);
				AssertEquals("", commodity.BY_HarmonisedTariff);
				AssertEquals("AU", commodity.BY_RN_NKCountryOfOrigin);

				commodities = inBondContainer3.Commodities.Find(x => x.BY_MarksAndNumbers == "5");
				commodity = commodities.First();
				AssertEquals(9, commodity.BY_PieceCount);
				AssertEquals(70m, commodity.BY_GrossWeight);
				AssertEquals("1010201030", commodity.BY_HarmonisedTariff);
				AssertEquals("", commodity.BY_RN_NKCountryOfOrigin);
			}
		}
		ForwardingConsol consol;

		public void TestCreateConsolInBond()
		{
			using (USCustomsDataRegistry.Instance.TransportModeForInBondCreationFromConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TransportModeForInBondCreationFromConsolList.Codes.AIR))
			using(var form = new ZForm(Consol))
			using (var plugin = GetNewPlugIn(Consol))
			{
				form.Show();
				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
				Env.Security.USInBondNew.IsAllowed = true;
				plugin.Enabled = false;
				plugin.Enabled = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((consolForm) =>
				{
					var form = ((CreateInBondForConsolForm)consolForm);
					form.DialogResult = DialogResult.OK;
				});
				plugin.TopLevelMenu.PerformClick();
				AssertNull(plugin.InBond);
			}
		}

		public void TestCreateConsolInBondWhenClickCancel()
		{
			using (USCustomsDataRegistry.Instance.TransportModeForInBondCreationFromConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TransportModeForInBondCreationFromConsolList.Codes.AIR))
			using (var plugin = GetNewPlugIn(Consol))
			{
				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
				Env.Security.USInBondNew.IsAllowed = true;
				plugin.Enabled = false;
				plugin.Enabled = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((consolForm) =>
				{
					var form = ((CreateInBondForConsolForm)consolForm);
					form.DialogResult = DialogResult.Cancel;
				});
				plugin.TopLevelMenu.PerformClick();
				AssertNull(plugin.InBond);
			}
		}

		ForwardingContainer CreateContainer(ZString number, ZShort count, ZString sealNum, ZDecimal weight)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = number;
			container.JC_ContainerCount = count;
			container.JC_SealNum = sealNum;
			container.JC_ContainerMode = "FCL";
			container.JC_GrossWeight = weight;
			container.JC_GrossWeightUQ = "KG";
			return container;
		}

		ForwardingPackLine CreatePackline(ZInt packCount, ZString type, ZDecimal weight, ZString countryOfOrigin, ZString marksNos, ZString tariff)
		{
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = packCount;
			packline.JL_F3_NKPackType = type;
			packline.JL_ActualWeight = weight;
			packline.JL_RN_NKOrigin = countryOfOrigin;
			packline.JL_MarksAndNumbers = marksNos;
			packline.JL_HarmonisedCode = tariff;
			return packline;
		}

		InBondPlugInForTest GetNewPlugIn(ICusInBondParent parent) => new InBondPlugInForTest(parent);

		ForwardingShipment GetNewshipment()
		{
			ForwardingShipment testShipment = Factory.New<ForwardingShipment>();
			CusInBondHeader testInBond = Factory.New<CusInBondHeader>();
			testInBond.BH_ParentID = testShipment.PK;
			testInBond.BH_ParentTableCode = testShipment.TablePrefix;
			return testShipment;
		}

		ForwardingShipment shipment;
		ForwardingShipment Shipment
		{
			get { return shipment ?? (shipment = GetNewshipment()); }
			set { shipment = value; }
		}

		ForwardingConsol GetNewConsol()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			return testConsol;
		}

		ForwardingConsol forwardingconsol;
		ForwardingConsol Consol
		{
			get { return forwardingconsol ?? (forwardingconsol = GetNewConsol()); }
			set { forwardingconsol = value; }
		}

		sealed class InBondPlugInForTest : InBondPlugIn
		{
			public InBondPlugInForTest(ICusInBondParent parent)
				: base(parent)
			{
			}

			internal ICusInBondParent HostBusinessEntityInternal => HostBusinessEntity;

			internal Security.SecurityCheckpoint SecurityCheckpointForMessagingMenuInternal => SecurityCheckpointForMessagingMenu;

			internal MenuItem GetNewTopLevelMenuInternal() => GetNewTopLevelMenu();

			internal void CreateNewInBondInternal() => CreateNewInBond();

			internal new Licensing.LicenceCheckpoint LicenceCheckPointInternal => LicenceCheckPoint;

			internal Control GetNewUserControlInternal() => GetNewUserControl();

			internal IBusiness GetBusinessEntityForPlugInInternal() => GetBusinessEntityForPlugIn();

			internal string NotToCreateJobTextInternal => NotToCreateJobText;
		}
	}
}
