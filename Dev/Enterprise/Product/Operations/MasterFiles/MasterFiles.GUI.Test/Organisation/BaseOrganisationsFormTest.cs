using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Application = System.Windows.Forms.Application;
using User = Enterprise.ZArchitecture.Environment.User;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(BaseOrganisationsForm))]
	sealed class BaseOrganisationsFormTest : ZFormBasherTest
	{
		class BaseOrganisationsFormForTest : BaseOrganisationsForm
		{
			public BaseOrganisationsFormForTest(OrgHeader organisation)
				: base(organisation)
			{
			}

			public bool DuplicateDialogWasShown;
			protected override ContinueWithSave ShowDuplicationForm()
			{
				DuplicateDialogWasShown = true;
				return ContinueWithSave.Yes;
			}

			public bool SecurityLoginDialogWasShown;
			protected override ContinueWithSave ShowSecurityLoginForm()
			{
				SecurityLoginDialogWasShown = true;
				return base.ShowSecurityLoginForm();
			}
			public void HandleSaveExceptionForTest(Exception ex)
			{
				base.HandleSaveException(ex);
			}

			public new ContinueWithSave ValidateAndSave()
			{
				var result = base.ValidateAndSave();
				return result;
			}
		}

		public void TestScreeningLogsTabPage()
		{
			DataRegistry.Instance.UserEventTrackingEnterprise = true;

			OrgHeader header = Factory.New<OrgHeader>();
			using (BaseOrganisationsForm form = new BaseOrganisationsForm(header))
			{
				form.Show();
				ZTabControl tabControl = (ZTabControl)form.zLogsTabPage1.Controls[0].Controls[0];
				AssertEquals(3, tabControl.TabPages.Count);
				AssertEquals("Denied Party Screening Logs", tabControl.TabPages[2].Text);
				AssertEquals(typeof(StmEntityScreeningLogControl), tabControl.TabPages[2].Controls[0].GetType());
			}
		}

		[RequiresSTA]
		public void TestSwitchToDpsLogsTab()
		{
			var header = Factory.New<OrgHeader>();
			using (var form = new BaseOrganisationsForm(header))
			{
				form.Show();
				AssertNotEquals("Logs", form.OrganisationsTabControl.SelectedTab.Text);

				form.SwitchToDpsLogsTab();
				AssertEquals("Logs", form.OrganisationsTabControl.SelectedTab.Text);
				AssertEquals("Denied Party Screening Logs", ((ZTabControl)form.zLogsTabPage1.Controls[0].Controls[0]).SelectedTab.Text);
			}
		}

		public class ReadOnlyChecker
		{
			public delegate ZString TestControlDelegate(Control control);

			public void Test(string errorMessage, ZForm formForBashingWithAllTabPagesExposed, TestControlDelegate testControlBlock)
			{
				using (formForBashingWithAllTabPagesExposed)
				{
					TestControls(formForBashingWithAllTabPagesExposed, testControlBlock);
					string errors = errorList.ToStringWithNewLineBetweenAppends();
					if (errors != ZString.Empty)
					{
						throw new Exception(errorMessage + ":\r\n" + errors);
					}
				}
			}

			readonly ZStringBuilder errorList = new ZStringBuilder();

			void TestControls(Control control, TestControlDelegate testControlBlock)
			{
				ZString result = testControlBlock.Invoke(control);
				if (!result.IsEmpty)
				{
					errorList.Append(result);
				}

				foreach (Control c in control.Controls)
				{
					TestControls(c, testControlBlock);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestAllControlsAreReadOnly()
		{
			GlbCompany.CurrentCompany.SetCountry("US"); // to make sure TSA Known Shipper tab appears on address tab

			bool oldValue = Env.Security.OrgDetailsModify.IsAllowed;
			var initialUserContext = Env.CurrentUserContext;
			try
			{
				Env.Security.OrgDetailsModify.IsAllowed = false;

				BusinessObjectFactory staffFactory = new BusinessObjectFactory();
				GlbStaff newStaff = staffFactory.NewWithValidTestData<GlbStaff>();
				GlbSecurity securityRecord = staffFactory.New<GlbSecurity>();
				securityRecord.GU_SecurityRight = Env.Security.OrganisationModify.Code;
				securityRecord.GU_ItemGUID = Env.Security.OrganisationModify.ItemGuid;
				securityRecord.GU_SecurityItemIsAllowed = false;
				securityRecord.GU_GS = newStaff.PK;
				newStaff.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);
				staffFactory.Save();

				Env.SetUserContext(new UserContext(newStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				OrgHeader testHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
				OrgAddress testAddress = testHeader.MainAddress;
				testHeader.CompanyData.OB_IsCreditor = ZBool.True;
				testHeader.CompanyData.OB_IsDebtor = ZBool.True;
				testHeader.OH_IsConsignee = ZBool.True;
				testHeader.OH_IsConsignor = ZBool.True;
				testHeader.OH_IsTransportClient = ZBool.True;
				testHeader.OH_IsWarehouseClient = ZBool.True;
				testHeader.OH_IsShippingProvider = ZBool.True;
				testHeader.OH_IsForwarder = ZBool.True;
				testHeader.OH_IsBroker = ZBool.True;
				testHeader.OH_IsMiscFreightServices = ZBool.True;
				testHeader.OH_IsCompetitor = ZBool.True;
				testHeader.OH_IsSalesLead = ZBool.True;
				Factory.Save();

				Assert("PRECONDITION", testHeader.IsInDatabase);

				ZOrganisationsForm formForBashing = new ZOrganisationsForm(testHeader);
				formForBashing.Show();
				ExposeAllTabPages(formForBashing);

				string errorMessage = "The following controls are exposed on the form, but are not readonly when security is denied. If you have recently added these controls to the org form, you need to add your propertyinfo to the IReadOnlySecurity implementation on your business object.\r\n\r\nThe following controls were NOT readonly";
				new ReadOnlyChecker().Test(errorMessage, formForBashing, delegate(Control c)
				{
					ZString result = ZString.Empty;
					if (c.Name != "CodeBox" &&
						c.Name != "DateTextBox" &&
						c.Name != "ContactsFilterStringTextBox" &&
						(IsEditingControl(c) || c is ZGrid) &&
						c.Parent != null &&
						(c.Parent.Parent == null || c.Parent.Parent.Name != "ZStmALogUserControl") &&
						(c.Parent.Parent?.Parent == null || c.Parent.Parent.Parent.Name != "ZStmALogUserControl") &&
						(c.Parent.Parent == null || c.Parent.Parent.Name != "ZActivityLoggingUserControl") &&
						(c.Parent.Parent?.Parent?.Parent == null || c.Parent.Parent.Parent.Parent.Name != "ZStmNoteUserControl") &&
						#if !WINZOR
						!(c.Parent is ZRichTextBoxToolBar) &&
						!(c.Parent.Parent is ZRichTextBoxToolBar) &&
						#endif
						!(c is ZFilterStripDropEdit) &&
						c.GetParent<ZDateRangeControl>() == null &&
						!(c.Parent is AccountFeeControl) &&
						!(c.Parent.Parent is AccountFeeControl) &&
						!(c.Parent is ZFilterStrip) &&
						!(c.Parent.Parent is ZFilterStrip) &&
						!(c is ZDropEdit x && x.EditableInViewMode) &&
						!(c.Parent is ZDropEdit y && y.EditableInViewMode))
					{
						string badControlName = (c.Parent != null ? c.Parent.Name + "." : "") + c.Name;
						string bindingMember = c.GetBindingMember();

						if (!c.Visible)
						{
							ForceVisible(c);
						}
						if (!c.GetReadOnly() && !ControlInListOfControlsAllowedToBeEnabled(badControlName))
						{
							result = badControlName + " - BindingMember=" + bindingMember;
						}
					}
					return result;
				});
			}
			finally
			{
				Env.Security.OrgDetailsModify.IsAllowed = oldValue;
				Env.SetUserContext(initialUserContext);
			}
		}

		[RequiresSTA]
		public void TestContactsTabIsAllowed()
		{
			Env.Security.OrgContactView.IsAllowed = false;

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Demo Organisation";
			org.OH_Code = "DEMORG";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "111 Demo St";
			org.MainAddress.OA_City = "Demoville";
			org.OH_IsSalesLead = true;
			org.MainAddress.OA_PostCode = "12345";
			org.OH_RL_NKClosestPort = "FRPAR";

			using (BaseOrganisationsFormForTest form = new BaseOrganisationsFormForTest(org))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();

				var label = form.ContactsTabPage.Controls.Find("coveringLabel", false);
				Assert("Security label should be present", label.Length == 1);
			}

			Env.Security.OrgContactView.IsAllowed = true;
			using (BaseOrganisationsFormForTest form = new BaseOrganisationsFormForTest(org))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();

				var label = form.ContactsTabPage.Controls.Find("coveringLabel", false);
				Assert("Security label should not be present", label.Length == 0);
			}
		}

		static void ForceVisible(Control control)
		{
			control.Visible = true;
			if (control.Parent != null)
			{
				ForceVisible(control.Parent);
				TabControl tabControl = control.Parent as TabControl;
				if (tabControl != null)
				{
					TabPage tabPage = (TabPage)control;
					tabControl.SelectedTab = tabPage;
				}
			}
		}

		bool IsEditingControl(Control control)
		{
			if (!(control is ZUserControl))
			{
				return (control is IDataBoundControl) && (((IDataBoundControl)control).DataSourceType != null) && !IsWithinCompositeControl(control);
			}
			else
			{
				return TypeDescriptor.GetAttributes(control)[typeof(CompositeFieldControlAttribute)] != null;
			}
		}

		bool IsWithinCompositeControl(Control control)
		{
			Control current = control.Parent;
			while (current != null)
			{
				if (TypeDescriptor.GetAttributes(control)[typeof(CompositeFieldControlAttribute)] != null)
				{
					return true;
				}
				current = current.Parent;
			}
			return false;
		}

		bool ControlInListOfControlsAllowedToBeEnabled(string controlName)
		{
			return Array.Find(ListOfControlsAllowedToBeEnabled, delegate(string listValue)
			{ return listValue == controlName; }) != null;
		}

		readonly string[] ListOfControlsAllowedToBeEnabled = new string[]
		{
				"OpportunitiesFilterGroupBox.DateTypeDropEdit",

				"CallFilterGroupBox.SalesRepCodeFindBox",
				"CallFilterGroupBox.LocationCodeFindBox",
				"CallFilterGroupBox.DirectionDropEdit",
				"CallFilterGroupBox.DateNextCallToDateEdit",
				"CallFilterGroupBox.DateNextCallFromDateEdit",
				"CallFilterGroupBox.DateOfCallToDateEdit",
				"CallFilterGroupBox.DateOfCallFromDateEdit",
				"CallFilterGroupBox.ContactGuidFindBox",

				"DocumentSDFControl.CommonFieldsCheckBox",
				"DocumentSDFControl.DocumentTypeDropEdit",
				"DocumentSDFControl.FieldGrid",
				"DocumentSDFControl.FieldGrid",
				"NoteRichTextBox.NoteRichTextBox",
				"NoteRichTextBox.RichTextToolBar",
				"NoteRichTextBox.NoteTextBox",

				"EDIMappingLinkPanel.OO_LocalCodeBoundDropEditEVT",
				"EDIMappingLinkPanel.OO_LocalCodeBoundDropEditPKG",
				"EDIMappingLinkPanel.OO_LocalGuidBoundGuidFindBoxSER",
				"EDIMappingLinkPanel.OO_LocalGuidBoundGuidFindBoxZNE",
				"EDIMappingLinkPanel.OO_LocalGuidBoundGuidFindBoxCUR",
				"EDIMappingLinkPanel.OO_LocalGuidBoundGuidFindBoxPTC",
				"EDIMappingLinkPanel.OO_LocalGuidBoundGuidFindBoxORG",
				"EDIMappingLinkPanel.OO_LocalGuidBoundGuidFindBoxCNT",
				"EDIMappingLinkPanel.OO_LocalCodeBoundDropEdit",
				"EDIMappingLinkPanel.OO_LocalGuidBoundGuidFindBoxCOU",
				"EDIMappingLinkPanel.OO_LocalCodeBoundCodeFindBoxCHC",

				"ConfigurationDetailsGroupBox.OM_IMPaymentMethodDropEdit",
				"ConfigurationGroupBox.OM_IMPaymentMethodDropEdit",

				"SalesRepCodeFindBox.DescriptionBox",
				"zCodeFindBox1.DescriptionBox",

				"FilterGroupBox.GroupFindbox",
				"FilterGroupBox.StaffFindbox",
				"StaffFindbox.DescriptionBox",
				"FilterGroupBox.StatusDropEdit",
				"FilterGroupBox.TypeDropEdit",

				"AssignedToGroupBox.ContactGuidFindBox",
				"AssignedToGroupBox.GroupGuidFindBox",
				"AssignedToGroupBox.StaffCodeFindBox",
				"StaffCodeFindBox.DescriptionBox"
		};

		protected override Form GetFormToBashCore()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			return new BaseOrganisationsForm(organisation);
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}

		public override void TestMinimumSizeNotTooBig()
		{
			string formName;
			using (Form testForm = GetFormToBash())
			{
				formName = testForm.Name;

				int minScreenWidthSupported = ControlDpiScalingHelper.ScaleToCurrentDpiX(1200);
				int minScreenHeightSupported = ControlDpiScalingHelper.ScaleToCurrentDpiY(768);
				int typicalTaskbarHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(43);

				int maxSizeWidth = minScreenWidthSupported;
				int maxSizeHeight = minScreenHeightSupported - typicalTaskbarHeight;

				Assert("Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(), testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(), testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		public void TestCannotModifyAROrAPFlag()
		{
			OrgHeader company1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (BaseOrganisationsForm form = new BaseOrganisationsForm(company1))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.CannotModifyAROrAPFlag(null, new OrgCompanyData.CannotModifyAROrAPFlagEventArgs(LedgerTypes.AccountsPayable));
				ZString expectedMessage = $"You cannot change the {LedgerTypes.AccountsPayable} flag because active {LedgerTypes.AccountsPayable} transactions still exist for this Organization";
				AssertEquals("Should show error with message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.CannotModifyAROrAPFlag(null, new OrgCompanyData.CannotModifyAROrAPFlagEventArgs(LedgerTypes.AccountsReceivable));
				expectedMessage = $"You cannot change the {LedgerTypes.AccountsReceivable} flag because active {LedgerTypes.AccountsReceivable} transactions still exist for this Organization";
				AssertEquals("Should show error with message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOrgFormShowsDialogWhenNoOrgTypeSelected()
		{
			OrgHeader company1 = Factory.New<OrgHeader>();
			company1.OH_FullName = "BLAH";
			company1.OH_Code = "XXXXXX";
			company1.OH_RL_NKClosestPort = "AUSYD";
			company1.MainAddress.OA_Address1 = "Test Addy";
			company1.MainAddress.OA_City = "Test City";
			company1.MainAddress.OA_PostCode = "12345";

			using (BaseOrganisationsForm form = new BaseOrganisationsForm(company1))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();
				form.ButtonsUserControl.SaveButton.Enabled = true;
				form.ButtonsUserControl.SaveButton.PerformClick();

				string msg = Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Organisation Type Message", form.OrganisationTypeNotSelectedMessage, msg);
			}
		}

		public void TestOrgFormWithUnmatchedOrg_DisableRegistryItem()
		{
			var org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = false;

			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, org))
			{
				var company1 = Factory.New<OrgHeader>();
				company1.OH_FullName = "Demo Organisation";
				company1.OH_Code = "DEMORG";
				company1.OH_RL_NKClosestPort = "AUSYD";
				company1.MainAddress.OA_Address1 = "111 Demo St";
				company1.MainAddress.OA_City = "Demoville";
				company1.OH_IsSalesLead = true;
				company1.MainAddress.OA_PostCode = "12345";

				using (var form = new BaseOrganisationsFormForTest(company1))
				{
					form.DisplayMode = ODisplayMode.New;
					form.Show();
					Application.DoEvents();
					form.ButtonsUserControl.SaveButton.Enabled = true;
					form.ButtonsUserControl.SaveButton.PerformClick();

					Assert("The duplicate dialog was shown", form.DuplicateDialogWasShown);
					AssertNull("No errors shown", Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestOrgFormShowCreateTranslatedAddressForm()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "BLAH";
			org.OH_Code = "XXXXXX";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_Language = Constants.Languages.French;
			org.OH_IsSalesLead = true;

			var mainAddress = org.MainAddress;
			mainAddress.OA_Language = Constants.Languages.French;
			mainAddress.OA_Address1 = "Nîmes";
			mainAddress.OA_City = "Nîmes";
			mainAddress.OA_PostCode = "12345";

			using (BaseOrganisationsFormForTest form = new BaseOrganisationsFormForTest(org))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();
				form.ButtonsUserControl.SaveButton.Enabled = true;
				form.ButtonsUserControl.SaveButton.PerformClick();

				AssertEquals(typeof(CreateTranslatedAddressForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSwitchAddressInfo()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "BLAH";
			org.OH_Code = "XXXXXX";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_Language = Constants.Languages.French;
			org.OH_IsSalesLead = true;

			var mainAddress = org.MainAddress;
			mainAddress.OA_Language = Constants.Languages.French;
			mainAddress.OA_Address1 = "Nîmes1";
			mainAddress.OA_Address2 = "Nîmes2";
			mainAddress.OA_City = "Nîmes";
			mainAddress.OA_PostCode = "12345";

			var englishMainAddress = org.CreateEnglishEquivalentAddress(mainAddress);
			var addressMapper = new AddressMapper(mainAddress, englishMainAddress);
			var address = addressMapper.Address1;
			addressMapper.Address1 = addressMapper.Address2;
			addressMapper.Address2 = address;

			var orgCopy = Factory.NewWithValidTestData<OrgHeader>();
			orgCopy.OH_FullName = "BLAH";
			orgCopy.OH_Code = "XXXXXX";
			orgCopy.OH_RL_NKClosestPort = "AUSYD";
			orgCopy.OH_Language = Constants.Languages.French;
			orgCopy.OH_IsSalesLead = true;

			var mainAddressCopy = orgCopy.MainAddress;
			mainAddressCopy.OA_Language = Constants.Languages.French;
			mainAddressCopy.OA_Address1 = "Nîmes1";
			mainAddressCopy.OA_Address2 = "Nîmes2";
			mainAddressCopy.OA_City = "Nîmes";
			mainAddressCopy.OA_PostCode = "12345";

			var translatedAddressCopy = orgCopy.CreateEnglishEquivalentAddress(mainAddressCopy);
			using (var form = new BaseOrganisationsFormForTest(org))
			{
				form.SwitchAddressInfo(addressMapper, englishMainAddress, mainAddress);
				AssertEquals(translatedAddressCopy.OTA_Language, mainAddress.Language);
				AssertEquals(translatedAddressCopy.OTA_Address1, mainAddress.Address1);
				AssertEquals(translatedAddressCopy.OTA_Address2, mainAddress.Address2);
				AssertEquals(translatedAddressCopy.OTA_City, mainAddress.City);
				AssertEquals(translatedAddressCopy.OTA_PostCode, mainAddress.Postcode);
				AssertEquals(translatedAddressCopy.CountryCodeISO2, mainAddress.OA_RN_NKCountryCode);

				englishMainAddress = mainAddress.TranslatedAddresses[0];
				AssertEquals(englishMainAddress.OTA_Language, mainAddressCopy.Language);
				AssertEquals(englishMainAddress.OTA_Address1, mainAddressCopy.Address1);
				AssertEquals(englishMainAddress.OTA_Address2, mainAddressCopy.Address2);
				AssertEquals(englishMainAddress.OTA_City, mainAddressCopy.City);
				AssertEquals(englishMainAddress.OTA_PostCode, mainAddressCopy.Postcode);
				AssertEquals(englishMainAddress.CountryCodeISO2, mainAddressCopy.OA_RN_NKCountryCode);
			}
		}

		public void TestOrgFormNoDuplicate()
		{
			OrgHeader company1 = Factory.New<OrgHeader>();
			company1.OH_FullName = "BLAH";
			company1.OH_Code = "XXXXXX";
			company1.OH_RL_NKClosestPort = "AUSYD";
			company1.MainAddress.OA_Address1 = "Test Addy";
			company1.MainAddress.OA_City = "Test City";
			company1.OH_IsSalesLead = true;
			company1.MainAddress.OA_PostCode = "12345";

			using (BaseOrganisationsFormForTest form = new BaseOrganisationsFormForTest(company1))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();
				form.ButtonsUserControl.SaveButton.Enabled = true;
				form.ButtonsUserControl.SaveButton.PerformClick();

				Assert("The Duplicate dialog was NOT shown", !form.DuplicateDialogWasShown);
				AssertNull("No errors shown", Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestGlobalAccountOrgCanSaveWithoutOrgDetailsNewAllowCreationOutsideLoginCountry()
		{
			var companyFR = Factory.NewWithValidTestData<GlbCompany>();
			companyFR.GC_Code = "AAA";
			companyFR.GC_Name = "AAA Company";
			companyFR.GC_RN_NKCountryCode = "FR";
			var branchFR = companyFR.Branches.AddNew();
			branchFR.GB_Code = "ABC";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TTT";
			staff.GS_LoginName = "testUser";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG1";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_IsGlobalAccount = true;
			org.OH_IsShippingProvider = true;
			org.OH_FullName = "GlobalAccountOrg";

			var address = org.MainAddress;
			address.OA_RL_NKRelatedPortCode = "USCHI";
			address.OA_City = "LITTLE NECK";
			address.OA_State = "NY";
			address.OA_Address1 = "25310 NORTHERN BLVD";
			address.OA_PostCode = "11362";

			Factory.Save();

			using (Env.SetTemporaryUserContext("testUser", branchFR.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Security.OrgDetailsNewAllowCreationOutsideLoginCountry.IsAllowed = false;
				Assert(!org.RequiresSecurityOverrideToUpdateCountry);

				using (var form = new BaseOrganisationsFormForTest(org))
				{
					form.Show();
					form.FireValidateAllForTest();
					Assert(!org.HasErrors);

					form.ValidateAndSave();
					Assert(!form.SecurityLoginDialogWasShown);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestOrgFormShowsDialogWhenRequiresSecurityOverrideToUpdateCountry()
		{
			Env.Security.OrgDetailsNewAllowCreationOutsideLoginCountry.IsAllowed = false;

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Demo Organisation";
			org.OH_Code = "DEMORG";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "111 Demo St";
			org.MainAddress.OA_City = "Demoville";
			org.OH_IsSalesLead = true;
			org.MainAddress.OA_PostCode = "12345";
			org.OH_RL_NKClosestPort = "FRPAR";

			EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

			using (BaseOrganisationsFormForTest form1 = new BaseOrganisationsFormForTest(org))
			{
				form1.DisplayMode = ODisplayMode.New;
				form1.Show();
				Application.DoEvents();

				//Current user cannot create an organisation with a country outsite its login company.
				//A security popup will be displayed so he can provide login info from a user with this right.
				//We will provide valid information and check that upon success a log entry was created.
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ShowDialogsInTest = true;

				var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
				AssertEquals(true, loginController.ValidateUserLoginAndPassword(User.SupportUserName, User.MasterPassword).LoginValidated);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object form)
				{
					var loginForm = (DocumentLoginForm)form;

					loginForm.Shown += delegate
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

						var loginBO = (SecurityLogin)loginForm.BusinessEntity;
						loginBO.Login = User.SupportUserName;
						loginBO.Password = User.MasterPassword;
						loginForm.PrintButton.PerformClick();
						ZFormModaliser.ResultToReturnFromShowDialog = loginForm.DialogResult; // ZFormModaliser overrides our button click dialog results
						};
				});

				form1.ButtonsUserControl.SaveButton.Enabled = true;
				form1.ButtonsUserControl.SaveButton.PerformClick();

				Assert("The SecurityLogin dialog was shown", form1.SecurityLoginDialogWasShown);
				AssertNull("No errors shown", Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, org.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "Organization country security override granted by user CWSupport")).Length);
			}
		}

		public void TestGetSecurityLoginEventArgs()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new BaseOrganisationsFormForTest(organization))
			{
				var args = form.GetSecurityLoginEventArgs();
				AssertEquals(true, args.HideApprovalRequestButton);
				AssertEquals("You do not have sufficient security rights to create an organization for a country/region outside your current login country/region.", args.MessageToShowWhenNotAllowed);
				AssertContains("Your local administrator or user(s) within your company that have the appropriate rights can override this setting by entering their login credentials.\r\nDo you wish to continue?", args.LoginPromptMessage);
			}
		}

		public void TestRatingAndCompanyTariffNotShown()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			using (BaseOrganisationsFormForTest form = new BaseOrganisationsFormForTest(org))
			{
				form.Show();
				MainDetailsUserControl details = (MainDetailsUserControl)form.OrganisationsTabControl.TabPages[0].Controls[0];
				details.DetailsTabControl.SelectedIndex = 3; // Auto-Rating And CompanyTariffs
				AssertEquals("PRE: tab page index is correct", "AutoRatingAndCompanyTariffTabPage", details.DetailsTabControl.SelectedTab.Name);
				details.DetailsTabControl.SelectedIndex = 0;

				AssertDetailsTabControlVisibilityForOrgType(org, form, details, org.OH_IsDebtorInfo);
				AssertDetailsTabControlVisibilityForOrgType(org, form, details, org.OH_IsConsigneeInfo);
				AssertDetailsTabControlVisibilityForOrgType(org, form, details, org.OH_IsConsignorInfo);
				AssertDetailsTabControlVisibilityForOrgType(org, form, details, org.OH_IsSalesLeadInfo);
				AssertDetailsTabControlVisibilityForOrgType(org, form, details, org.OH_IsShippingProviderInfo);
			}
		}

		public void TestRatingAndCompanyTariffControlVisibilityWhenOrgTypeChanges()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			using (BaseOrganisationsFormForTest form = new BaseOrganisationsFormForTest(org))
			{
				form.Show();
				MainDetailsUserControl detailsControl = (MainDetailsUserControl)form.OrganisationsTabControl.TabPages["DetailsTabPage"].Controls["DetailsControl"];
				detailsControl.DetailsTabControl.SelectedIndex = 3;
				AssertEquals("Selected tab page is correct", "AutoRatingAndCompanyTariffTabPage", detailsControl.DetailsTabControl.SelectedTab.Name);
				Assert("Not available label is visible", detailsControl.AutoRatingAndCompanyTariffNotAvailableLabel.Visible);
				Assert("User control is not visible", !detailsControl.organisationRatingUserControl1.Visible);

				AssertRatingAndCompanyTariffVisibility(org.OH_IsDebtorInfo, detailsControl);
				AssertRatingAndCompanyTariffVisibility(org.OH_IsConsignorInfo, detailsControl);
				AssertRatingAndCompanyTariffVisibility(org.OH_IsConsigneeInfo, detailsControl);
				AssertRatingAndCompanyTariffVisibility(org.OH_IsSalesLeadInfo, detailsControl);
				AssertRatingAndCompanyTariffVisibility(org.OH_IsShippingProviderInfo, detailsControl);
			}
		}

		public void TestDefaultTariffLevelText()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsConsignor = true;
			using (var form = new BaseOrganisationsFormForTest(org))
			{
				form.Show();
				var detailsControl = (MainDetailsUserControl)form.OrganisationsTabControl.TabPages["DetailsTabPage"].Controls["DetailsControl"];
				detailsControl.DetailsTabControl.SelectedIndex = 3;
				AssertEquals("Selected tab page is correct", "AutoRatingAndCompanyTariffTabPage", detailsControl.DetailsTabControl.SelectedTab.Name);

				Assert("Default Company Tariff is shown", detailsControl.organisationRatingUserControl1.CompanyTariffUserControl.DefaultTariffLevel.Visible);

				org.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

				Assert("Default Company Tariff is hidden", !detailsControl.organisationRatingUserControl1.CompanyTariffUserControl.DefaultTariffLevel.Visible);
			}
		}

		public void TestRegenerateCodeWhenOrgTypesChangeIsTrue()
		{
			OrgHeader organization = Factory.New<OrgHeader>();
			AssertEquals("RegenerateCodeWhenOrgTypesChange", false, organization.RegenerateCodeWhenOrgTypesChange);
			using (BaseOrganisationsFormForTest form = new BaseOrganisationsFormForTest(organization))
			{
				AssertEquals("RegenerateCodeWhenOrgTypesChange", true, organization.RegenerateCodeWhenOrgTypesChange);
			}
		}

		void AssertRatingAndCompanyTariffVisibility(ZPropertyInfo orgType, MainDetailsUserControl detailsControl)
		{
			Assert("Not available label is visible", detailsControl.AutoRatingAndCompanyTariffNotAvailableLabel.Visible);
			Assert("User control is not visible", !detailsControl.organisationRatingUserControl1.Visible);

			orgType.Value = ZBool.True;
			Assert("Not available label is not visible", !detailsControl.AutoRatingAndCompanyTariffNotAvailableLabel.Visible);
			Assert("User control is visible", detailsControl.organisationRatingUserControl1.Visible);

			orgType.Value = ZBool.False;
			Assert("Not available label is visible", detailsControl.AutoRatingAndCompanyTariffNotAvailableLabel.Visible);
			Assert("User control is not visible", !detailsControl.organisationRatingUserControl1.Visible);
		}

		void AssertDetailsTabControlVisibilityForOrgType(OrgHeader org, BaseOrganisationsFormForTest form, MainDetailsUserControl details, ZPropertyInfo orgType)
		{
			details.DetailsTabControl.SelectedIndex = 3; // Auto-Rating And CompanyTariffs
			Assert("Not Available Label is visible", details.AutoRatingAndCompanyTariffNotAvailableLabel.Visible);

			details.DetailsTabControl.SelectedIndex = 0;
			orgType.Value = ZBool.True;
			details.DetailsTabControl.SelectedIndex = 3; // Auto-Rating And CompanyTariffs
			Assert("Not Available Label is NOT visible", !details.AutoRatingAndCompanyTariffNotAvailableLabel.Visible);
			details.DetailsTabControl.SelectedIndex = 0;
			orgType.Value = ZBool.False;
		}

		public void TestRegistrationNumbersSecurity_ShouldRequire()
		{
			using (var form = new BaseOrganisationsFormForTest(Factory.New<OrgHeader>()))
			{
				form.Show();
				var control = (MainDetailsUserControl)form.OrganisationsTabControl.TabPages["DetailsTabPage"].Controls["DetailsControl"];
				Assert(control.IsModifyConfigFinancialRegistrationNumbersSecurity);
				Assert(control.IsNewConfigModifyFinancialRegistrationNosSecurity);
			}
		}

		[RequiresSTA]
		public void TestHandlingOH_CodeDuplicated()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "TEST TESTOrganization";
			org1.OH_RL_NKClosestPort = "AUSYD";
			org1.MainAddress.OA_Address1 = "111 Demo St";
			org1.MainAddress.OA_City = "Demoville";
			org1.OH_IsSalesLead = true;
			org1.MainAddress.OA_PostCode = "12345";
			org1.OH_Code = "TESTESSYG";

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TESTESSYD";
			factory2.Save();

			using (var form = new BaseOrganisationsFormForTest(org1))
			{
				form.Show();
				Application.DoEvents();

				form.ButtonsUserControl.SaveButton.Enabled = true;
				form.ButtonsUserControl.SaveButton.PerformClick();

				org1.OH_Code = "TESTESSYD";
				org1.IsCodeDuplicatedCheckDisabledForOnce = true;
				form.ButtonsUserControl.SaveButton.Enabled = true;
				form.ButtonsUserControl.SaveButton.PerformClick();//I have to press save button twice here because the first time the factory will call BusinessObject.OnSaving() for twice due to some other persistent fields changed.

				AssertEquals("OH_Code of org1 should have been modified", "TESTESSYD1", org1.OH_Code);
			}
		}

		[RequiresSTA]
		public void TestHideOldDeduplicationForm()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();

			void MapDuplicatedOrgValues(OrgHeader x)
			{
				x.OH_FullName = "TEST TESTOrganization";
				x.OH_RL_NKClosestPort = "AUSYD";
				x.MainAddress.OA_Address1 = "111 Demo St";
				x.MainAddress.OA_City = "Demoville";
				x.MainAddress.OA_State = "NSW";
				x.OH_IsSalesLead = true;
				x.MainAddress.OA_PostCode = "12345";
				x.OH_Code = "TESTESSYG";
			}

			MapDuplicatedOrgValues(org1);

			Factory.Save();

			foreach (var enableDeduplication in new[] { true, false })
			{
				OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
				MapDuplicatedOrgValues(org2);

				using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableDeduplication))
				using (var form = new BaseOrganisationsFormForTest(org2))
				{
					form.Show();

					form.ButtonsUserControl.SaveButton.Enabled = true;
					form.ButtonsUserControl.SaveButton.PerformClick();

					if (enableDeduplication)
					{
						Assert("Should not show form", !form.DuplicateDialogWasShown);
					}
					else
					{
						Assert("Should show form", form.DuplicateDialogWasShown);
					}
				}
			}
		}

		public void TestSave_WithNoOrganizationTypeTicked_WithProductivityWiseModeEnabled_ShouldNotShowError_AndShouldSave()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			MasterFilesTestHelper.FillWithValidTestDataSoFormSaveWorks(org);

			using (var form = new BaseOrganisationsForm(org))
			{
				form.Show();
				Application.DoEvents();

				org.OH_IsDebtor = false;
				org.OH_IsCreditor = false;
				org.OH_IsConsignor = false;
				org.OH_IsConsignee = false;
				org.OH_IsWarehouseClient = false;
				org.OH_IsShippingProvider = false;
				org.OH_IsForwarder = false;
				org.OH_IsSalesLead = false;
				org.OH_IsCompetitor = false;
				org.OH_IsMiscFreightServices = false;
				form.FireSaveButton();

				AssertNoErrors(org);
				AssertEquals(@"You must select an Organization Type from the right-side of the Organization Details Screen. 
EG: Consignee/Consignor, Receivables/Payables etc....", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The organization should not have been successfully saved. SAD!", true, org.HasChanges);
			}

			UnitTestUserNotification.Instance.ClearMessages();
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			using (var form = new BaseOrganisationsForm(org))
			{
				form.Show();
				Application.DoEvents();

				org.OH_FullName = "Ricky Ticky Tacky";
				form.FireSaveButton();

				AssertNoErrors(org);
				AssertEquals("There should be no error relating to organization types when ProductivityWise mode is enabled. SAD!", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("The organization should have been successfully saved. SAD!", false, org.HasChanges);
			}
		}

		#region Test Not Wipe Warning Info For Address

		public void TestNotWipeWarningInfoForAddressWhenAddressTabOpened()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_FullName = "TEST ORG 1";
			organization.OH_RL_NKClosestPort = "AUSYD";
			organization.OH_IsConsignee = true;
			var mainAddress = organization.MainAddress;
			mainAddress.OA_Address1 = "main address1";
			mainAddress.OA_Address2 = "main address2";
			mainAddress.OA_City = "Sydney";
			mainAddress.OA_PostCode = "botany";
			mainAddress.OA_State = "NSW";
			mainAddress.OA_RN_NKCountryCode = "AU";
			Factory.Save();

			var newFactory = new BusinessObjectFactory()
			{
				RefreshEnabled = false
			};
			var orgInNewFactory = newFactory.Load<OrgHeader>(organization.PK);

			using (var form = new BaseOrganisationsFormForTest(orgInNewFactory))
			{
				form.Show();
				mainAddress.OA_Address1 = "new main address1";
				Factory.Save();

				orgInNewFactory.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "new additional address info";
				form.FireSaveButton();
				AssertAddressWarningInfo(orgInNewFactory.MainAddress);

				((IBusinessObjectInternals)orgInNewFactory.MainAddress).Validate(orgInNewFactory.MainAddress.OA_Address1Info); // Mock address tab is open and address 1 label call  CopyCaptionToPropertyHumanReadableName
				AssertAddressWarningInfo(orgInNewFactory.MainAddress);

				orgInNewFactory.MainAddress.OA_Address1 = "other address";
				AssertAddressWarningInfo(orgInNewFactory.MainAddress, false);
			}
		}

		void AssertAddressWarningInfo(OrgAddress address, bool hasWarning = true)
		{
			if (hasWarning)
			{
				CombineAssertions(() =>
				{
					Assert(address.IsValidationSuspended);
					AssertEquals(1, address.OA_Address1Info.Notifications.Count());
					AssertContains("has changed this field.\r\nYours: 'main address1', Theirs: 'new main address1'", address.OA_Address1Info.Notifications.First().Message);
				});
			}
			else
			{
				CombineAssertions(() =>
				{
					Assert(!address.IsValidationSuspended);
					AssertEquals(0, address.OA_Address1Info.Notifications.Count());
				});
			}
		}

		#endregion

		[DeveloperOnlyTest]
		public void TestCopyAddressForAnalysis()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var mainAddress = organization.MainAddress;
			mainAddress.OA_Address1 = "main address1";
			mainAddress.OA_Address2 = "main address2";
			mainAddress.OA_City = "Sydney";
			mainAddress.OA_PostCode = "botany";
			mainAddress.OA_State = "NSW";
			mainAddress.OA_RN_NKCountryCode = "AU";

			organization.Addresses.OfType<OrgAddress>().Where(x => x.Address1 != mainAddress.OA_Address1).ToList().ForEach(organization.Addresses.RemoveAndDelete);
			var orgAddress = organization.Addresses.AddNew();
			orgAddress.OA_Address1 = "org address1";
			orgAddress.OA_Address2 = "org address2";
			orgAddress.OA_City = "Murburne";
			orgAddress.OA_PostCode = "M1234";
			orgAddress.OA_State = "cwa";
			orgAddress.OA_RN_NKCountryCode = "AU";

			var expectContent = new string[2]
			{
					@"InputAddress1: main address1
InputAddress2: main address2
InputCity: Sydney
InputPostcode: botany
InputState: NSW
InputCountryCode: AU",
				@"InputAddress1: org address1
InputAddress2: org address2
InputCity: Murburne
InputPostcode: M1234
InputState: cwa
InputCountryCode: AU"
			};
			Factory.Save();

			using (var form = new BaseOrganisationsFormForTest(organization))
			{
				form.Show();
				var actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var item = actionsMenuItem.MenuItems.FindByName("Copy Address For Analysis");
				AssertNotNull("Check menu item exists", item);

				var tabControl = form.OrganisationsTabControl;
				tabControl.SelectedTab = form.DetailsTabPage;
				form.DetailsTabPage.Show();
				SafeClipboard.Clear();
				item.PerformClick();
				AssertMultilineASCIIEquals(expectContent[0], SafeClipboard.GetText());

				tabControl.SelectedTab = form.AddressesTabPage;
				form.AddressesTabPage.Show();
				var addressGrid = form.AddressesPageControl2.OrgAddressBoundGrid;
				addressGrid.SelectSingleElementByPK(orgAddress.PK);
				SafeClipboard.Clear();
				item.PerformClick();
				AssertMultilineASCIIEquals(expectContent[1], SafeClipboard.GetText());

				tabControl.SelectedTab = form.ContactsTabPage;
				form.ContactsTabPage.Show();
				SafeClipboard.Clear();
				item.PerformClick();
				AssertMultilineASCIIEquals(expectContent[0], SafeClipboard.GetText());
			}
		}

		[TestDate(2020, 02, 20, 13, 12, 00)]
		public void TestHandleSaveConcurrencyExceptionWhenRecordHasChangedShowWarningMessage()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_FullName = "TEST ORG 1";
			organization.OH_RL_NKClosestPort = "AUSYD";
			organization.MainAddress.OA_Address1 = "Main Address1";
			organization.MainAddress.OA_ValidationStatus = "VAD";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = organization.PK;
			address.OA_Address1 = "Other Address1";

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var orgNewFactory = newFactory.Load<OrgHeader>(organization.PK);
			orgNewFactory.MainAddress.OA_Address1 = "42 BANANA ST";
			orgNewFactory.MainAddress.OA_ValidationStatus = "MAN";

			var addressNewFactory = newFactory.Load<OrgAddress>(address.PK);
			addressNewFactory.OA_Address1 = "72 O'RIORDAN ST";

			using (var form = new BaseOrganisationsFormForTest(organization))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				try
				{
					address.Delete();
					Factory.Save();
					newFactory.Save();
				}
				catch (ZSaveConcurrencyException ex)
				{
					form.HandleSaveExceptionForTest(ex);
				}
			}

			var warningMessage = @"While you have been working with this form, another user has made changes.

The system will now try to combine your changes with those of the other user.
After you click 'OK', the form will merge your changes with changes made by other user.

However, the fields will have warning messages explaining the other user's changes.
Please review the form carefully before clicking the 'Save' button again.

The following objects have changes and will be merged:
Address (Main Address1) (CargoWise Support @ 20 Feb 2020 13:12:00)
	Address 1
	Validation Status

The following objects have been deleted:
Address (Other Address1)
Organization (TESORGSYD) (pending delete)";

			AssertMultilineASCIIEquals("Concurrency Information Message", warningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCannotModifyAROrAPFlag_IsChangingByWorkflowTriggerShouldHaveException()
		{
			OrgHeader company1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (BaseOrganisationsForm form = new BaseOrganisationsForm(company1))
			{
				using ((company1 as IRegisterStatusChangeContext).TemporarilySetStatusChangedByTriggerEvent(null))
				{
					AssertExceptionThrown<ZCannotSaveException>("Can not save excepetion", "You cannot change the AP flag because active AP transactions still exist for this Organization", () => form.CannotModifyAROrAPFlag(null, new OrgCompanyData.CannotModifyAROrAPFlagEventArgs(LedgerTypes.AccountsPayable)));
					AssertExceptionThrown<ZCannotSaveException>("Can not save excepetion", "You cannot change the AR flag because active AR transactions still exist for this Organization", () => form.CannotModifyAROrAPFlag(null, new OrgCompanyData.CannotModifyAROrAPFlagEventArgs(LedgerTypes.AccountsReceivable)));
				}
			}
		}
	}
}
