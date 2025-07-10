using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(MiscOptionsUserControl))]
	sealed class MiscOptionsUserControlTest : ImportCustomsUserControlBasherAbstractTest
	{
		public void TestProperPositionOfBondControls()
		{
			var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			Factory.Save();
			using (var frm = new JobDeclarationForm(declaration))
			{
				frm.Show();
				Application.DoEvents();
				frm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = frm.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var ctr = (MiscOptionsUserControl)frm.CustomsBrokerageUserControl.MiscOptionsTabPage.Controls.Find("MiscOptionsUserControl", true)[0];
				var groupBox = ctr.Controls.Find("ACEBondGroupBox", true)[0];
				var waiverCodeDropEdit = ctr.Controls.Find("WaiverCodeDropEdit", true)[0];
				var bondButton = ctr.Controls.Find("ACEBondButton", true)[0];
				var xOfWaiverCodeDropEdit = waiverCodeDropEdit.Location.X;
				var yOfWaiverCodeDropEdit = waiverCodeDropEdit.Location.Y;
				var xOfBondButton = bondButton.Location.X;
				var bondTypeCodes = new BondTypeList().GetAllCodes().ToList();
				bondTypeCodes.Insert(0, string.Empty);
				CombineAssertions(() =>
				{
					foreach (var code in bondTypeCodes)
					{
						declaration.US_BondType = code;
						Application.DoEvents();
						var lastUpperControl = groupBox.Controls
							.Cast<Control>()
							.Where(c => c != waiverCodeDropEdit && c.Visible && c.Location.X == xOfWaiverCodeDropEdit && c.Location.Y < yOfWaiverCodeDropEdit)
							.OrderBy(c => c.Location.Y)
							.Last();
						var message = $"The position of WaiverCodeDropEdit should be proper when the upper control is {lastUpperControl.Name} and the bond type is {code}.";
						var gap = (code == BondTypeList.Codes.SingleTransactionBond) ? 4 : 6;
						var expectedLocation = ControlDpiScalingHelper.NewScaledPoint(xOfWaiverCodeDropEdit, lastUpperControl.Location.Y + lastUpperControl.Height + gap);
						AssertEquals(message, expectedLocation, waiverCodeDropEdit.Location);
						var buttonY = waiverCodeDropEdit.Location.Y - 1;
						if (code == BondTypeList.Codes.SingleTransactionBond)
						{
							var nextControl = groupBox.Controls
								.Cast<Control>()
								.Where(c => c != waiverCodeDropEdit && c.Visible && c.Location.X == xOfWaiverCodeDropEdit && c.Location.Y > yOfWaiverCodeDropEdit)
								.OrderBy(c => c.Location.Y)
								.First();
							buttonY = nextControl.Location.Y - 1;
						}

						message = $"The position of ACEBondButton should be proper when the upper control is {lastUpperControl.Name} and the bond type is {code}.";
						expectedLocation = ControlDpiScalingHelper.NewScaledPoint(xOfBondButton, buttonY);
						AssertEquals(message, expectedLocation, bondButton.Location);
					}
				});
			}
		}

		public void TestSendEBondRequest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "Insurance Agent");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.InsuranceAgent, "IAA", "UnitedStates Code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			DeclarationTestHelper.SetProcessingDistrictPortCode("SQMZ");
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetBRecordOfficeCode("HZ");
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "Test User";
			staff.GS_LoginName = "TST";
			staff.GS_WorkPhone = "+610451111221";
			staff.GS_EmailAddress = "TestUser@anc.mail";
			Factory.Save();
			using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				var security = new Security.SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
				security.ImportMessaging.IsAllowed = true;
				security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				using (Env.SetTemporarySecurityInstanceForTest(security))
				{
					var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory);
					declaration.US_InsuranceAgent = "IAA";
					declaration.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.AcceptedByCBP;
					declaration.US_BondProducerAccNo = "TestUser";
					var companyWrapper = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany);
					var extPassword = companyWrapper.PasswordCollection.AddNew();
					extPassword.GP_MailBoxID = "IAA";
					extPassword.GP_UserID = "TST";
					extPassword.GP_CurrentPassword = "012345";
					var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
					Factory.Save();
					using (var frm = new JobDeclarationForm(declaration))
					{
						frm.Show();
						Application.DoEvents();
						frm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = frm.CustomsBrokerageUserControl.MiscOptionsTabPage;
						var ctr = (MiscOptionsUserControl)frm.CustomsBrokerageUserControl.MiscOptionsTabPage.Controls.Find("MiscOptionsUserControl", true)[0];
						UnitTestUserNotification.Instance.ClearMessages();
						UnitTestUserNotification.Instance.AddYesAnswer();
						UnitTestUserNotification.Instance.AddYesAnswer();
						var button = (ZButton)ctr.Controls.Find("SendEBondRequestButton", true)[0];
						button.PerformClick();
						var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
						query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.USeBond);
						var message = Factory.LoadTop1<Enterprise.Messaging.Business.EDIMessage>(query);
						var interchange = message.Interchange;
						AssertEquals(ApplicationCodeList.Codes.USeBond, message.EM_ApplicationCode);
						AssertEquals(ApplicationCodeList.Codes.USeBond, interchange.EI_ApplicationCode);
					}
				}
			}
		}

		public void TestFDALabelAndControlVisibility()
		{
			using (MiscOptionsUserControl control = new MiscOptionsUserControl())
			{
				AssertEquals("FDACCNStateDropEdit.Visible", false, control.FDACCNStateDropEdit.Visible);
				AssertEquals("FDACCNCountryCodeFindBox.Visible", true, control.FDACCNCountryCodeFindBox.Visible);
			}
		}

		public void TestFDACCNStateDropEditVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				MiscOptionsUserControl userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals("FDACCNStateDropEdit should be shown", true, userControl.FDACCNStateDropEdit.Visible);
				AssertEquals("FDACCNCountryCodeFindBox should not be visible", false, userControl.FDACCNCountryCodeFindBox.Visible);
			}
		}

		public void TestConsolidatedJobNumberVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals("US_ConsolidatedJobNumberFindBox should be shown", true, userControl.US_ConsolidatedJobNumberFindBox.Visible);
			}
		}

		public void TestServiceLevelVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals("JE_RS_NKServiceLevelBoundFindBox should be shown", true, userControl.JE_RS_NKServiceLevelBoundFindBox.Visible);
				AssertEquals("JE_RS_NKServiceLevelBoundFindBox should be shown", false, userControl.AESTIRJE_RS_NKServiceLevelBoundFindBox.Visible);
			}
		}

		public void TestServiceLevelVisibilityForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals("JE_RS_NKServiceLevelBoundFindBox should be shown", false, userControl.JE_RS_NKServiceLevelBoundFindBox.Visible);
				AssertEquals("JE_RS_NKServiceLevelBoundFindBox should be shown", true, userControl.AESTIRJE_RS_NKServiceLevelBoundFindBox.Visible);
			}
		}

		public void TestControlVisibliityForConsolidatedMonthlyFiling()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_MonthlyFiling = true;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals("PayableMPFCalcEdit", true, userControl.PayableMPFCalcEdit.Visible);
				AssertEquals("EstEnteredValueCalcEdit", false, userControl.EstEnteredValueCalcEdit.Visible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_MonthlyFiling = false;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("PayableMPFCalcEdit", false, userControl.PayableMPFCalcEdit.Visible);
				AssertEquals("EstEnteredValueCalcEdit", true, userControl.EstEnteredValueCalcEdit.Visible);
			}
		}

		public void TestACEControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_FDACANType = FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				Assert(userControl.BondGroupBox.Visible);
				Assert(!userControl.ACEBondGroupBox.Visible);
				Assert(!userControl.ACCVDBondGroupBox.Visible);
				Assert(userControl.AnticipatedPortOfCrossingCodeFindBox.Visible);
				Assert(!userControl.PGAInspectionDateEdit.Visible);
				Assert(!userControl.PGAInspectionFirmsFindBox.Visible);
				Assert(!userControl.FSISInspectionCodeFindBox.Visible);
				Assert(!userControl.InspectionPortFindBox.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				Assert(!userControl.BondGroupBox.Visible);
				Assert(userControl.ACEBondGroupBox.Visible);
				Assert(userControl.ACCVDBondGroupBox.Visible);
				Assert("Superseding is only visible when bond type is continuous", !userControl.SupersedingCheckBox.Visible);
				Assert(!userControl.AnticipatedPortOfCrossingCodeFindBox.Visible);
				Assert(userControl.PGAInspectionDateEdit.Visible);
				Assert(userControl.PGAInspectionFirmsFindBox.Visible);
				Assert(userControl.FSISInspectionCodeFindBox.Visible);
				Assert(userControl.InspectionPortFindBox.Visible);
				declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
				Assert("Superseding is only visible when bond type is continuous", userControl.SupersedingCheckBox.Visible);
				Assert("Bond amount should be invisible", !userControl.ACEBondAmountCalcEdit.Visible);
				Assert(!userControl.ACEBondDesignationCodeDropEdit.Visible);
				declaration.US_BondType2 = BondTypeList.Codes.SingleTransactionBond;
				Assert("Bond amount 2 should be invisible", userControl.ACEBondAmount2CalcEdit.Visible);
				declaration.US_BondType2 = BondTypeList.Codes.ContinuousBond;
				Assert("Bond amount 2 should be visible", !userControl.ACEBondAmount2CalcEdit.Visible);
				declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
				Assert("Superseding is only visible when bond type is continuous", !userControl.SupersedingCheckBox.Visible);
				Assert("Bond amount should be visible", userControl.ACEBondAmountCalcEdit.Visible);
				Assert(userControl.ACEBondDesignationCodeDropEdit.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl.ACEBondButton.PerformClick();
				AssertEquals(MiscOptionsUserControl.RefreshBondDetailsWarning, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFTZControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals(false, userControl.BondGroupBox.Visible);
				AssertEquals(true, userControl.ACEBondGroupBox.Visible);
				AssertEquals(true, userControl.ACCVDBondGroupBox.Visible);
				AssertEquals(true, userControl.GoodsFromFTZCodeFindBox.Visible);
				AssertEquals(true, userControl.US_ConsolidatedJobNumberFindBox.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals(false, userControl.BondGroupBox.Visible);
				AssertEquals(false, userControl.ACEBondGroupBox.Visible);
				AssertEquals(false, userControl.ACCVDBondGroupBox.Visible);
				AssertEquals(false, userControl.EntryGroupBox.Visible);
				AssertEquals(false, userControl.RemoteFilingGroupBox.Visible);
				AssertEquals(false, userControl.ReConGroupBox.Visible);
				AssertEquals(false, userControl.PPQForm368Box13Button.Visible);
				AssertEquals(false, userControl.PaymentsGroupBox.Visible);
				AssertEquals(false, userControl.GoodsFromFTZCodeFindBox.Visible);
				AssertEquals(false, userControl.US_ConsolidatedJobNumberFindBox.Visible);
			}
		}

		public void TestPPQControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals(false, userControl.PPQForm368Box13Button.Visible);
			}

			declaration.US_PPQForm368Box13A = "ABC";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals(true, userControl.PPQForm368Box13Button.Visible);
			}
		}

		public void TestDomesticCargoControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals(false, userControl.DomesticCargoCheckBox.Visible);
			}

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals(true, userControl.DomesticCargoCheckBox.Visible);
			}
		}

		public void TestFDAStateAndCountryCodeFieldsAreInSameLocation()
		{
			using (MiscOptionsUserControl control = new MiscOptionsUserControl())
			{
				AssertEquals("FDACCNStateDropEdit and FDACCNCountryCodeFindBox are the same field from user perspective - must be in the same location on this form", control.FDACCNStateDropEdit.Location, control.FDACCNCountryCodeFindBox.Location);
			}
		}

		public void TestBondAndCalculationCodeVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				var insuranceAgentDropEdit = userControl.Controls.Find("InsuranceAgentDropEdit", true)[0];
				var insuranceDispositionDropEdit = userControl.Controls.Find("InsuranceDispositionTextBox", true)[0];
				var sendEBondRequestButton = (ZButton)userControl.Controls.Find("SendEBondRequestButton", true)[0];
				AssertEquals("US_BondAmount should be hidden", false, userControl.BondAmountCalcEdit.Visible);
				AssertEquals("US_BondCalcCode should be hidden", false, userControl.BondCalcCodeDropEdit.Visible);
				AssertEquals("US_BondProducerAccNo should be hidden", false, userControl.BondProducerAccNoTextBox.Visible);
				AssertEquals("US_InsuranceAgent should be hidden", false, insuranceAgentDropEdit.Visible);
				AssertEquals("US_InsuranceDisposition should be hidden", false, insuranceDispositionDropEdit.Visible);
				AssertEquals("SendEBondRequestButton should be hidden", false, sendEBondRequestButton.Visible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				AssertEquals("US_BondAmount should be hidden", false, userControl.BondAmountCalcEdit.Visible);
				AssertEquals("US_BondCalcCode should be hidden", false, userControl.BondCalcCodeDropEdit.Visible);
				AssertEquals("US_BondProducerAccNo should be hidden", false, userControl.BondProducerAccNoTextBox.Visible);
				AssertEquals("US_InsuranceAgent should be hidden", false, insuranceAgentDropEdit.Visible);
				AssertEquals("US_InsuranceDisposition should be hidden", false, insuranceDispositionDropEdit.Visible);
				AssertEquals("SendEBondRequestButton should be hidden", false, sendEBondRequestButton.Visible);
				declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
				AssertEquals("US_BondAmount should be visible for Single Transaction Bond", true, userControl.BondAmountCalcEdit.Visible);
				AssertEquals("US_BondCalcCode should be visible for Single Transaction Bond", true, userControl.BondCalcCodeDropEdit.Visible);
				AssertEquals("US_BondProducerAccNo should be visible for Single Transaction Bond", true, userControl.BondProducerAccNoTextBox.Visible);
				AssertEquals("ACS US_InsuranceAgent should be hidden", false, insuranceAgentDropEdit.Visible);
				AssertEquals("ACS US_InsuranceDisposition should be hidden", false, insuranceDispositionDropEdit.Visible);
				AssertEquals("ACS SendEBondRequestButton should be hidden", false, sendEBondRequestButton.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				AssertEquals("ACE US_BondAmount should be visible for Single Transaction Bond", true, userControl.ACEBondAmountCalcEdit.Visible);
				AssertEquals("ACE US_BondCalcCode should visible for Single Transaction Bond", true, userControl.ACESEBCalcCodeDropEdit.Visible);
				AssertEquals("ACE US_BondProducerAccNo should be visible for Single Transaction Bond", true, userControl.ACEBondProducerAccNoTextBox.Visible);
				AssertEquals("ACE US_InsuranceAgent should be only visible for Single Transaction Bond", true, insuranceAgentDropEdit.Visible);
				AssertEquals("ACE US_InsuranceDisposition should be only visible for Single Transaction Bond", true, insuranceDispositionDropEdit.Visible);
				AssertEquals("ACE US_InsuranceDisposition should be read only for Single Transaction Bond", true, insuranceDispositionDropEdit.GetReadOnly());
				AssertEquals("ACE SendEBondRequestButton should be only visible for Single Transaction Bond", true, sendEBondRequestButton.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Factory.Save();
				sendEBondRequestButton.PerformClick();
				AssertEquals(MiscOptionsUserControl.SendEBondRequestWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
				AssertEquals("ACE US_BondAmount should be hidden", false, userControl.ACEBondAmountCalcEdit.Visible);
				AssertEquals("ACE US_BondCalcCode should be hidden", false, userControl.ACESEBCalcCodeDropEdit.Visible);
				AssertEquals("ACE US_BondProducerAccNo should be visible for Continuous Bond", true, userControl.ACEBondProducerAccNoTextBox.Visible);
				AssertEquals("ACE US_InsuranceAgent should be hidden", false, insuranceAgentDropEdit.Visible);
				AssertEquals("ACE US_InsuranceDisposition should be hidden", false, insuranceDispositionDropEdit.Visible);
				AssertEquals("ACE SendEBondRequestButton should be hidden", false, sendEBondRequestButton.Visible);
			}
		}

		public void TestEntryDateElectionCodeAndDateVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals(false, userControl.EntryDateElectionCodeDropEdit.Visible);
				AssertEquals(false, userControl.PresentationDateEdit.Visible);
			}

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var userControl = (MiscOptionsUserControl)form.CustomsBrokerageUserControl.MiscOptions;
				AssertEquals(true, userControl.EntryDateElectionCodeDropEdit.Visible);
				AssertEquals(true, userControl.PresentationDateEdit.Visible);
			}
		}

		protected override Type UserControlToBashType => typeof(MiscOptionsUserControl);
	}
}
