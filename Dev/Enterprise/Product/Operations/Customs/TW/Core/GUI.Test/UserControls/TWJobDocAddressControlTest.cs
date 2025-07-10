using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.TWJobDocAddressTest;

namespace Enterprise.Customs.TW.GUI.Testing
{
	public abstract class TWJobDocAddressControlAbstractTest<T> : TestCaseWithFactory
		where T : TWJobDocAddressControl
	{
		protected abstract T CreateTWJobDocAddressControl();

		public void TestLocalAddressDetailLabelDisplayDefaultTextWhenMissingLocalAddress_OrgHeaderChanged()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			header.OH_RL_NKClosestPort = "TW";
			header.OH_IsConsignee = true;
			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = "EN";
			mainAddress.UnrestrictedAdditionalAddressInformation = "addinfo address";
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_City = "APPLE CITY";
			mainAddress.Postcode = "12345";
			mainAddress.OA_State = "TPE";
			mainAddress.OA_Phone = "+1 (273) 5495200";
			mainAddress.OA_Email = "001@xx.com";
			var declaration = Factory.NewWithValidTestData<JobDeclarationForJobDocAddressTest>();
			var docAddress = declaration.JobDocAddress;
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				docAddress.OrganisationPK = header.PK;
				AssertEquals("The selected address does not have a traditional Chinese translated address.", control.LocalAddressDetailLabel.Text);
			}

			var zhTWtranslatedAddress1 = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = "ZH-TW";
			zhTWtranslatedAddress1.UnrestrictedAdditionalAddressInformation = "附加信息2";
			zhTWtranslatedAddress1.OTA_CompanyName = "綠晃科技股份有限公司";
			zhTWtranslatedAddress1.OTA_Address1 = "臺北加工出口區園東街6號";
			zhTWtranslatedAddress1.OTA_Address2 = string.Empty;
			zhTWtranslatedAddress1.OTA_City = "臺北巿";
			zhTWtranslatedAddress1.OTA_PostCode = "90093";
			zhTWtranslatedAddress1.OTA_State = "TPE";
			zhTWtranslatedAddress1.ClosestPort = "TW";
			docAddress.OrganisationPK = header.PK;
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				var detailsTabControl = control.FindSingle<ZTabControl>("DetailsTabControl");
				detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("LocalAddressTabPage");
				Application.DoEvents();
				var expected = @"綠晃科技股份有限公司
臺北加工出口區園東街6號
附加信息2
臺北巿  TPE  台灣  90093";
				AssertEquals(expected, control.LocalAddressDetailLabel.Text);
			}
		}

		public void TestLocalAddressDetailLabelDisplayDefaultTextWhenMissingLocalAddress_E2_AddressChanged()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			header.OH_RL_NKClosestPort = "TW";
			header.OH_IsConsignee = true;
			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = "EN";
			mainAddress.UnrestrictedAdditionalAddressInformation = "addinfo address";
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_City = "APPLE CITY";
			mainAddress.Postcode = "12345";
			mainAddress.OA_State = "TPE";
			mainAddress.OA_Phone = "+1 (273) 5495200";
			mainAddress.OA_Email = "001@xx.com";
			var zhTWtranslatedAddress = mainAddress.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress.OTA_Language = "ZH-TW";
			zhTWtranslatedAddress.UnrestrictedAdditionalAddressInformation = "附加信息2";
			zhTWtranslatedAddress.OTA_CompanyName = "綠晃科技股份有限公司";
			zhTWtranslatedAddress.OTA_Address1 = "臺北加工出口區園東街6號";
			zhTWtranslatedAddress.OTA_Address2 = string.Empty;
			zhTWtranslatedAddress.OTA_City = "臺北巿";
			zhTWtranslatedAddress.OTA_PostCode = "90093";
			zhTWtranslatedAddress.OTA_State = "TPE";
			zhTWtranslatedAddress.ClosestPort = "TW";
			var address2 = header.Addresses.AddNew();
			address2.OA_Language = "EN";
			address2.OA_Address1 = "address2 line 1";
			address2.OA_Address2 = "address2 line 2";
			address2.OA_RN_NKCountryCode = "TW";
			var declaration = Factory.NewWithValidTestData<JobDeclarationForJobDocAddressTest>();
			var docAddress = declaration.JobDocAddress;
			docAddress.OrganisationPK = header.PK;
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				var detailsTabControl = control.FindSingle<ZTabControl>("DetailsTabControl");
				detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("LocalAddressTabPage");
				Application.DoEvents();
				var expected = @"綠晃科技股份有限公司
臺北加工出口區園東街6號
附加信息2
臺北巿  TPE  台灣  90093";
				AssertEquals(expected, control.LocalAddressDetailLabel.Text);
			}

			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				docAddress.E2_OA_Address = address2.PK;
				AssertEquals("The selected address does not have a traditional Chinese translated address.", control.LocalAddressDetailLabel.Text);
			}
		}

		public void TestDefaultContact()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var declaration = Factory.NewWithValidTestData<JobDeclarationForJobDocAddressTest>();
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var docAddress = declaration.JobDocAddress;
				AssertEquals("", docAddress.E2_Contact);
				docAddress.OrganisationPK = header.PK;
				AssertEquals("Contact1", docAddress.E2_Contact);
			}
		}

		public void TestControlVisible()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var declaration = Factory.NewWithValidTestData<JobDeclarationForJobDocAddressTest>();
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				AssertControlVisible(declaration, control, true);
				AssertControlVisible(declaration, control, false);
			}
		}

		void AssertControlVisible(JobDeclarationForJobDocAddressTest declaration, TWJobDocAddressControl control, bool isAddressOverride)
		{
			declaration.JobDocAddress.E2_AddressOverride = isAddressOverride;
			Application.DoEvents();
			AssertEquals(!isAddressOverride, control.FindSingle<ZPanel>("OrganisationPanel").Visible);
			var detailsTabControl = control.FindSingle<ZTabControl>("DetailsTabControl");
			detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("AddressTabPage");
			CombineAssertions(() =>
			{
				Application.DoEvents();
				AssertEquals(isAddressOverride, control.FindSingle<ZPanel>("EditAddressPanel").Visible);
				AssertEquals(!isAddressOverride, control.FindSingle<ZPanel>("AddressPanel").Visible);
				detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("LocalAddressTabPage");
				Application.DoEvents();
				AssertEquals(isAddressOverride, control.FindSingle<ZPanel>("EditLocalAddressPanel").Visible);
				AssertEquals(!isAddressOverride, control.FindSingle<ZLabel>("LocalAddressDetailLabel").Visible);
				detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("ContactTabPage");
				Application.DoEvents();
				AssertEquals(isAddressOverride, control.FindSingle<ZPanel>("EditContactPanel").Visible);
				AssertEquals(!isAddressOverride, control.FindSingle<ZPanel>("ContactPanel").Visible);
				detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("CodeTabPage");
				Application.DoEvents();
			});
		}

		public void TestTPCCodeControlVisible()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var declaration = Factory.NewWithValidTestData<JobDeclarationForJobDocAddressTest>();
			declaration.JE_MessageType = "IMP";
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				AssertTPCCodeControlVisible(declaration, control, "EXP", DocAddressType.SupplierDocumentaryAddress, true);
				AssertTPCCodeControlVisible(declaration, control, "IMP", DocAddressType.SupplierDocumentaryAddress, false);
				AssertTPCCodeControlVisible(declaration, control, "EXP", DocAddressType.ImporterDocumentaryAddress, false);
				AssertTPCCodeControlVisible(declaration, control, "IMP", DocAddressType.ImporterDocumentaryAddress, true);
				AssertTPCCodeControlVisible(declaration, control, "EXP", DocAddressType.LocalProcessorAddress, false);
				AssertTPCCodeControlVisible(declaration, control, "IMP", DocAddressType.LocalProcessorAddress, false);
			}
		}

		void AssertTPCCodeControlVisible(JobDeclarationForJobDocAddressTest declaration, TWJobDocAddressControl control, ZString messageType, DocAddressType addressType, bool expectedVisible)
		{
			declaration.JobDocAddress.DocAddressType = addressType;
			declaration.JE_MessageType = messageType;
			var detailsTabControl = control.FindSingle<ZTabControl>("DetailsTabControl");
			detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("CodeTabPage");
			CombineAssertions(() =>
			{
				AssertEquals(expectedVisible, control.FindSingle<ZTextBox>("TPCCodeTextBox").Visible);
				AssertEquals(expectedVisible, control.FindSingle<ZDropEdit>("TPCCodeTypeDropEdit").Visible);
			});
		}

		public void TestAEOCodeControlVisible()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var declaration = Factory.NewWithValidTestData<JobDeclarationForJobDocAddressTest>();
			declaration.JE_MessageType = "IMP";
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				AssertAEOCodeControlVisible(declaration, control, "EXP", DocAddressType.SupplierDocumentaryAddress, true);
				AssertAEOCodeControlVisible(declaration, control, "IMP", DocAddressType.SupplierDocumentaryAddress, true);
				AssertAEOCodeControlVisible(declaration, control, "EXP", DocAddressType.ImporterDocumentaryAddress, true);
				AssertAEOCodeControlVisible(declaration, control, "IMP", DocAddressType.ImporterDocumentaryAddress, true);
				AssertAEOCodeControlVisible(declaration, control, "EXP", DocAddressType.LocalProcessorAddress, false);
				AssertAEOCodeControlVisible(declaration, control, "IMP", DocAddressType.LocalProcessorAddress, false);
				AssertAEOCodeControlVisible(declaration, control, "EXP", DocAddressType.Manufacturer, false);
				AssertAEOCodeControlVisible(declaration, control, "IMP", DocAddressType.Manufacturer, false);
			}
		}

		void AssertAEOCodeControlVisible(JobDeclarationForJobDocAddressTest declaration, TWJobDocAddressControl control, ZString messageType, DocAddressType addressType, bool expectedVisible)
		{
			declaration.JobDocAddress.DocAddressType = addressType;
			declaration.JE_MessageType = messageType;
			var detailsTabControl = control.FindSingle<ZTabControl>("DetailsTabControl");
			detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("CodeTabPage");
			CombineAssertions(() =>
			{
				AssertEquals(expectedVisible, control.FindSingle<ZTextBox>("AEOCodeTextBox").Visible);
				AssertEquals(expectedVisible, control.FindSingle<ZDropEdit>("AEOCodeTypeDropEdit").Visible);
			});
		}

		public void TestCBPCodeControlVisible()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			AssertCBPCodeControlVisible(DocAddressType.SupplierDocumentaryAddress, true);
			AssertCBPCodeControlVisible(DocAddressType.ImporterDocumentaryAddress, true);
			AssertCBPCodeControlVisible(DocAddressType.LocalProcessorAddress, false);
		}

		void AssertCBPCodeControlVisible(DocAddressType addressType, bool expectedVisible)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForJobDocAddressTest>();
			declaration.JE_MessageType = "IMP";
			var jobDocAddress = declaration.JobDocAddress;
			jobDocAddress.DocAddressType = addressType;
			jobDocAddress.E2_AddressOverride = true;
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				var detailsTabControl = control.FindSingle<ZTabControl>("DetailsTabControl");
				detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("CodeTabPage");
				CombineAssertions(() =>
				{
					AssertEquals(expectedVisible, control.FindSingle<ZTextBox>("CBPCodeTextBox").Visible);
					AssertEquals(expectedVisible, control.FindSingle<ZDropEdit>("CBPCodeTypeDropEdit").Visible);
				});
			}
		}

		public void TestFRICodeControlVisible()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			AssertFRICodeControlVisible(DocAddressType.SupplierDocumentaryAddress, false);
			AssertFRICodeControlVisible(DocAddressType.ImporterDocumentaryAddress, false);
			AssertFRICodeControlVisible(DocAddressType.LocalProcessorAddress, false);
			AssertFRICodeControlVisible(DocAddressType.Manufacturer, true);
		}

		void AssertFRICodeControlVisible(DocAddressType addressType, bool expectedVisible)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForJobDocAddressTest>();
			declaration.JE_MessageType = "IMP";
			var jobDocAddress = declaration.JobDocAddress;
			jobDocAddress.DocAddressType = addressType;
			jobDocAddress.E2_AddressOverride = true;
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				var detailsTabControl = control.FindSingle<ZTabControl>("DetailsTabControl");
				detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("CodeTabPage");
				CombineAssertions(() =>
				{
					AssertEquals(expectedVisible, control.FindSingle<ZTextBox>("FRICodeTextBox").Visible);
					AssertEquals(expectedVisible, control.FindSingle<ZDropEdit>("FRICodeTypeDropEdit").Visible);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenDeleteRowFromInvoiceHeaderGrid()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			header.OH_RL_NKClosestPort = "TW";
			header.OH_IsConsignee = true;
			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = "EN";
			mainAddress.UnrestrictedAdditionalAddressInformation = "addinfo address";
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_City = "APPLE CITY";
			mainAddress.Postcode = "12345";
			mainAddress.OA_State = "TPE";
			mainAddress.OA_Phone = "+1 (273) 5495200";
			mainAddress.OA_Email = "001@xx.com";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoices = declaration.Invoices;
			var invoice = invoices.AddNew();
			invoice.SupplierDocumentaryAddress.OrganisationPK = header.PK;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting)
				{
					brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
					var invoiceTabControl = brokerageControl.InvoicesTabPage.FindSingle<ZTabControl>("InvoiceTabControl");
					var organizationsTabPage = invoiceTabControl.FindSingle<ZTabPage>("OrganizationsTabPage");
					invoiceTabControl.SelectedTab = organizationsTabPage;
					invoices.Delete(invoice);
				}
			}
		}

		public void TestCodeBoxesVisible()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			var declaration = Factory.NewWithValidTestData<JobDeclarationForJobDocAddressTest>();
			declaration.JE_MessageType = "IMP";
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				control.CodeBoxesVisible = false;
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();

				var detailsTabControl = control.FindSingle<ZTabControl>("DetailsTabControl");
				detailsTabControl.SelectedTab = control.FindSingle<ZTabPage>("CodeTabPage");
				CombineAssertions("controls should be invisible", () =>
				{
					Assert("TPCCodeTextBox", !control.FindSingle<ZTextBox>("TPCCodeTextBox").Visible);
					Assert("TPCCodeTypeDropEdit", !control.FindSingle<ZDropEdit>("TPCCodeTypeDropEdit").Visible);
					Assert("AEOCodeTextBox", !control.FindSingle<ZTextBox>("AEOCodeTextBox").Visible);
					Assert("AEOCodeTypeDropEdit", !control.FindSingle<ZDropEdit>("AEOCodeTypeDropEdit").Visible);
					Assert("CBPCodeTextBox", !control.FindSingle<ZTextBox>("CBPCodeTextBox").Visible);
					Assert("CBPCodeTypeDropEdit", !control.FindSingle<ZDropEdit>("CBPCodeTypeDropEdit").Visible);
					Assert("FRICodeTextBox", !control.FindSingle<ZTextBox>("FRICodeTextBox").Visible);
					Assert("FRICodeTypeDropEdit", !control.FindSingle<ZDropEdit>("FRICodeTypeDropEdit").Visible);
				});
			}
		}

		public void TestSupportMultipleResourceStringData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForJobDocAddressTest>();
			using (var form = new ZForm(declaration))
			{
				var control = CreateTWJobDocAddressControl();
				control.CodeBoxesVisible = false;
				control.SetDataBinding(declaration, ".JobDocAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();

				AssertSame(declaration.JobDocAddress, ((ISupportMultipleResourceStringDataSupporter)control).SupportMultipleResourceStringData);
			}
		}
	}

	sealed class TWJobDocAddressControlTest : TWJobDocAddressControlAbstractTest<TWJobDocAddressControl>
	{
		protected override TWJobDocAddressControl CreateTWJobDocAddressControl()
		{
			return new TWJobDocAddressControl();
		}
	}
}
