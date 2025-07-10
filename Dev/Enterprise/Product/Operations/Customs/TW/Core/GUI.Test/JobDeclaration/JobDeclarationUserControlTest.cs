using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	sealed class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestReloadEntryNumberSetDefaultEntryNumber()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var org2Code = orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress = orgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.JE_MessageType = "IMP";
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "G2";
			entryInstruction.CEI_CustomsOffice = "BA";
			entryInstruction.CEI_BoxNumber = "123";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 08, 05);
			declaration.EntryNumber = EntryNumberGenerator.New(declaration).GenerateEntryNumber();

			declaration.DefaultEntryNumber = "xxx";
			declaration.ReloadEntryNumber();
			CombineAssertions(() =>
			{
				AssertEquals("1st time reload should load from current EntryNumber", declaration.EntryNumber, declaration.DefaultEntryNumber);
				var nextNumber = EntryNumberGenerator.New(declaration).GenerateEntryNumber();
				declaration.EntryNumber = nextNumber;
				declaration.ReloadEntryNumber();
				AssertEquals("reload should load from current EntryNumber when not empty", nextNumber, declaration.DefaultEntryNumber);
				declaration.DefaultEntryNumber = ZString.Empty;
				declaration.ReloadEntryNumber();
				AssertEquals("reload should not set defult number to empty", nextNumber, declaration.DefaultEntryNumber);
			});
		}

		public void TestHandleDeclarationControlVisibilityChangedCore()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			{
				var control = new JobDeclarationUserControl { JobDeclaration = declaration };
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				var controlTWVesselArrivalRegTextBox = control.FindSingle<ZTextBox>(x => x.Name == "TWVesselArrivalRegTextBox");
				var controlTWSLDTextBox = control.FindSingle<ZTextBox>(x => x.Name == "TWSLDTextBox");
				Assert("Vessel Reg textbox should be visibility", controlTWVesselArrivalRegTextBox.Visible);
				Assert("SO No / Manifest textbox should be visibility", controlTWSLDTextBox.Visible);
				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				Assert("Vessel Reg textbox should not be visibility", !controlTWVesselArrivalRegTextBox.Visible);
				Assert("SO No / Manifest textbox should not be visibility", !controlTWSLDTextBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertVisibleForTransportMode(declaration, control);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertVisibleForTransportMode(declaration, control);
				declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertVisibleForTransportMode(declaration, control);
				declaration.JE_RL_NKOrigin = "TWZ99";
				var controlJE_Z99PortOfOriginTextBox = control.FindSingle<ZTextBox>(x => x.Name == "JE_Z99PortOfOriginTextBox");
				var controlJE_Z99FinalDestinationTextBox = control.FindSingle<ZTextBox>(x => x.Name == "JE_Z99FinalDestinationTextBox");
				Assert("DescriptionBox of OriginFindBox should not be visible", !control.OriginFindBox.ShowDescriptionBox);
				Assert("JE_Z99PortOfOriginTextBox should be visible", controlJE_Z99PortOfOriginTextBox.Visible);
				declaration.JE_RL_NKOrigin = "TWTPE";
				Assert("DescriptionBox of OriginFindBox should be visible", control.OriginFindBox.ShowDescriptionBox);
				Assert("JE_Z99PortOfOriginTextBox should not be visible", !controlJE_Z99PortOfOriginTextBox.Visible);
				declaration.JE_RL_NKFinalDestination = "TWZ99";
				Assert("DescriptionBox of FinalDestinationFindBox should not be visible", !control.FinalDestinationFindBox.ShowDescriptionBox);
				Assert("JE_Z99FinalDestinationTextBox should be visible", controlJE_Z99FinalDestinationTextBox.Visible);
				declaration.JE_RL_NKFinalDestination = "TWTPE";
				Assert("DescriptionBox of FinalDestinationFindBox should be visible", control.FinalDestinationFindBox.ShowDescriptionBox);
				Assert("JE_Z99FinalDestinationTextBox should not be visible", !controlJE_Z99FinalDestinationTextBox.Visible);
			}
		}

		void AssertVisibleForTransportMode(BaseJobDeclaration declaration, JobDeclarationUserControl control)
		{
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			Assert("Container Count should not be visible", !control.JE_ContainerCountCalcEdit.Visible);
			Assert("Units should not be visible", !control.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			Assert("Container Count should be visible", control.JE_ContainerCountCalcEdit.Visible);
			Assert("Units should not be visible", !control.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
		}

		public void TestSupplierOrganisationControl()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			{
				var control = new JobDeclarationUserControl();
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals(0, control.FindAll<ZOrganisationControlWithMiscellaneous>(x => x.Name == "SupplierOrganisationControl").Count());
			}
		}

		public void TestImporterOrganisationControl()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			{
				var control = new JobDeclarationUserControl();
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert("JobDeclarationUserControl should not contain ImporterOrganisationControl.", !control.FindAll<ZOrganisationControlWithMiscellaneous>(x => x.Name == "ImporterOrganisationControl").Any());
			}
		}

		public void TestNotifyOrganisationControlCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var captionResourceString = form.CustomsBrokerageUserControl.FindSingle<ZGuidFindBox>(c => c.Name == "NotifyOrganisationControl").CaptionResourceString;
				AssertEquals("Notify Party", captionResourceString.Caption);
				AssertEquals("Notify Party", captionResourceString.ShortCaption);
				AssertEquals("Notify Party", captionResourceString.MediumCaption);
			}
		}

		public void TestOrganisationsTabPageControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					jobDeclarationUserControl.RightTabControl.SelectedTab = jobDeclarationUserControl.OrganisationsTabPage;
					var consigneeOrganisationControl = jobDeclarationUserControl.OrganisationsTabPage.FindSingle<ConsignorOrConsigneeUserControl>(c => c.Name == "ConsigneeOrganisationControl");
					var consignorOrganisationControl = jobDeclarationUserControl.OrganisationsTabPage.FindSingle<ConsignorOrConsigneeUserControl>(c => c.Name == "ConsignorOrganisationControl");
					Assert("ConsigneeOrganisationControl is visible", consigneeOrganisationControl.Visible);
					Assert("ConsignorOrganisationControl is visible", consignorOrganisationControl.Visible);
				}
			}
		}

		public void TestBondedWarehouseVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.RightTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.OrganisationsTabPage;
				Application.DoEvents();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.BondedWarehouseDocAddressControl.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals(false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.BondedWarehouseDocAddressControl.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				AssertEquals(false, form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.BondedWarehouseDocAddressControl.Visible);
			}
		}

		public void TestCEI_PackageDescriptionVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				CustomsBrokerageUserControl brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				JobDeclarationUserControl jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				{
					if (jobDeclarationUserControl != null)
					{
						var controlTW_PackageDescriptionLongTextControl = jobDeclarationUserControl.FindSingle<LongTextControl>(x => x.Name == "CEI_PackageDescriptionLongTextControl");
						declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
						Assert(controlTW_PackageDescriptionLongTextControl.Visible);
						entryInstruction.CEI_IsCoPackaged = true;
						Assert(controlTW_PackageDescriptionLongTextControl.Visible);
						entryInstruction.CEI_IsCoPackaged = false;
						Assert(controlTW_PackageDescriptionLongTextControl.Visible);
					}
				}
			}
		}

		public void TestEntryDetailsTabPage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					var entryDetailsTabPage = jobDeclarationUserControl.FindSingle<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					var declarationOtherDetailsUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
					AssertEquals(jobDeclarationUserControl.RightTabControl.SelectedTab, entryDetailsTabPage);
					AssertEquals(jobDeclarationUserControl.RightTabControl.SelectedIndex, 0);
					AssertType(typeof(DeclarationOtherDetailsUserControl), declarationOtherDetailsUserControl);
				}
			}

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					var entryDetailsTabPage = jobDeclarationUserControl.FindSingleOrDefault<ZTabPage>(x => x.Name == "EntryDetailsTabPage");
					var declarationOtherDetailsUserControl = entryDetailsTabPage.FindSingle<DeclarationOtherDetailsUserControl>(x => x.Name == "TWDeclarationOtherDetailsUserControl");
					AssertEquals(jobDeclarationUserControl.RightTabControl.SelectedTab, entryDetailsTabPage);
					AssertEquals(jobDeclarationUserControl.RightTabControl.SelectedIndex, 0);
					AssertType(typeof(DeclarationOtherDetailsUserControl), declarationOtherDetailsUserControl);
				}
			}
		}

		public void TestCharacterCasing()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				var declarationControl = brokerageControl.DeclarationTabPage.FindSingle<JobDeclarationUserControl>(x => x.Name == "JobDeclarationUserControl");
				var longTextTextBox = declarationControl.FindSingle<LongTextControl>(c => c.Name == "CEI_PackageDescriptionLongTextControl");
				AssertNotNull(longTextTextBox);
				AssertEquals(CharacterCasing.Normal, longTextTextBox.CharacterCasing);
			}
		}

		public void TestControlsReadOnly()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					AssertEquals(true, jobDeclarationUserControl.FindSingle<ZDropEdit>(x => x.Name == "MessageStatusDropEdit").ReadOnly);
					AssertEquals(true, jobDeclarationUserControl.FindSingle<ZDropEdit>(x => x.Name == "EntryStatusDropEdit").ReadOnly);
				}
			}
		}

		public void TestIncoTermExplainButtonVisible()
		{
			using (var control = new JobDeclarationUserControl())
			{
				var incoTermExplainButton = control.FindSingleOrDefault<ZButton>(c => c.Name == "IncoTermExplainButton");
				AssertEquals("Do not display IncoTermExplainButton in TW Customs.", false, incoTermExplainButton.Visible);
			}
		}

		public void TestAllocateEntryNumberButton_Click()
		{
			var declaration = Declaration;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var cusHead1 = declaration.EntryHeader;
				var entryInstruction = declaration.CusEntryInstruction;
				entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 08, 05);
				cusHead1.CH_CEI_Instruction = entryInstruction.PK;
				declaration.EntryHeader.CH_Status = "";
				Factory.Save();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				declaration.Factory.SuspendValidation();
				jobDeclarationUserControl.FindSingle<ZButton>("AllocateEntryNumberButton").PerformClick();
				AssertEquals("Entry Number is allocated, and yet it was " + declaration.EntryNumber, "BBBF0912300001", declaration.EntryNumber);

				var factory2 = new BusinessObjectFactory();
				var declarationLoaded = factory2.Load<JobDeclaration>(declaration.PK);
				AssertEquals("BBBF0912300001", declarationLoaded.EntryNumber);
			}
		}

		public void TestButtonEnable()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CABF0945600030";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				var allocateEntryNumberButton = jobDeclarationUserControl.FindSingle<ZButton>("AllocateEntryNumberButton");
				var modifyEntryNumberButton = jobDeclarationUserControl.FindSingle<ZButton>("ModifyEntryNumberButton");
				CombineAssertions("Button Enabled", () =>
				{
					Assert("AllocateEntryNumberButton Enabled", allocateEntryNumberButton.Enabled);
					Assert("ModifyEntryNumberButton Enabled", modifyEntryNumberButton.Enabled);
				});
			}

			entryHeader.CusEntryNumber.CE_EntryStatus = "C1";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				var allocateEntryNumberButton = jobDeclarationUserControl.FindSingle<ZButton>("AllocateEntryNumberButton");
				var modifyEntryNumberButton = jobDeclarationUserControl.FindSingle<ZButton>("ModifyEntryNumberButton");
				CombineAssertions("Button Disabled", () =>
				{
					Assert("AllocateEntryNumberButton Disabled", !allocateEntryNumberButton.Enabled);
					Assert("ModifyEntryNumberButton Disabled", !modifyEntryNumberButton.Enabled);
				});
			}
		}

		public void TestModifyEntryNumberButton_Click()
		{
			var declaration = Declaration;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var cusHead1 = declaration.EntryHeader;
				var entryInstruction = declaration.CusEntryInstruction;
				entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 08, 05);
				cusHead1.CH_CEI_Instruction = entryInstruction.PK;
				declaration.EntryHeader.CH_Status = "";
				Factory.Save();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var dialog = (AllocateNumberForm)obj;
					var allocateNumber = (AllocateNumber)dialog.BusinessEntity;
					allocateNumber.Part5Number = "00002";
					dialog.FindSingle<ZButton>("OKButton").PerformClick();
				});
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				jobDeclarationUserControl.FindSingle<ZButton>("ModifyEntryNumberButton").PerformClick();
				AssertEquals("Entry Number is allocated, and yet it was " + declaration.EntryNumber, "BBBF0912300002", declaration.EntryNumber);
				Factory.Save();
				var factory2 = new BusinessObjectFactory();
				var declarationLoaded = factory2.Load<JobDeclaration>(declaration.PK);
				AssertEquals("BBBF0912300002", declarationLoaded.EntryNumber);
			}
		}

		public void TestSupportMultipleResourceStringData()
		{
			using (var form = new ZForm(Declaration))
			{
				var control = new JobDeclarationUserControl();
				control.JobDeclaration = Declaration;
				form.Controls.Add(control);
				form.Show();

				AssertSame(Declaration, ((ISupportMultipleResourceStringDataSupporter)control).SupportMultipleResourceStringData);
			}
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = "EXP";
					declaration.JE_GS_NKCusAgent = "TT";
					declaration.JE_CustomsProfile = "123-3";
					declaration.JE_CustomsOffice = "BF";
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
					declaration.ActiveEntryHeaders.AddNew();
					var entryInstruction = declaration.CusEntryInstruction;
					entryInstruction.CEI_CustomsOffice = "BB";
					entryInstruction.CEI_Style = "G1";
					entryInstruction.CEI_BoxNumber = "123";
					var invoice = declaration.Invoices.AddNew();
					var orgHeader = Factory.New<OrgHeader>();
					orgHeader.OH_Code = "Buyer TW";
					orgHeader.OH_FullName = "Buyer TW";
					invoice.JZ_OH_Supplier = orgHeader.PK;
					Factory.Save();
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
	}
}
