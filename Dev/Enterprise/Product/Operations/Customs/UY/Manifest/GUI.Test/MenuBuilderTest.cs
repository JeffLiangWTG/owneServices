using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.UY.Manifest.Business;
using Enterprise.Customs.UY.Manifest.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(DAETestingConstants))]

namespace Enterprise.Customs.UY.Manifest.GUI.Testing
{
	sealed class BuildMenuTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		IDisposable func;

		[TestDate(2020, 10, 02, 18, 00, 00)]
		public void TestSendManifest()
		{
			SetCertificate();

			PopulateManifestHeader();

			CreateAndPopulateHouseBill(ZString.Empty, ZString.Empty);
			CreateAndPopulateHouseBill(ASYCUDA.Business.MessageStatusCodeList.Codes.Error, ASYCUDA.Business.MessageStatusCodeList.Codes.Error);
			CreateAndPopulateHouseBill(ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted, ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted);
			CreateAndPopulateHouseBill(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, ZString.Empty);
			CreateAndPopulateMasterBill();

			CreateAndPopulatePack(header.Bills[0]);
			CreateAndPopulatePack(header.Bills[0]);
			CreateAndPopulatePack(header.Bills[1]);
			CreateAndPopulatePack(header.Bills[2]);
			CreateAndPopulatePack(header.Bills[3]);

			Factory.Save();

			AssertEquals(0, header.Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(1, menu.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Send Manifest").PerformClick();
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Messages.Count);
			AssertMessage(header.Messages[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword);
			AssertStatuses(true);
		}

		[TestDate(2020, 10, 02, 18, 00, 00)]
		public void TestSendBill_OneBillSelected()
		{
			SetCertificate();

			PopulateManifestHeader();
			CreateAndPopulateHouseBill(ZString.Empty, ZString.Empty);
			CreateAndPopulateHouseBill(ASYCUDA.Business.MessageStatusCodeList.Codes.Error, ASYCUDA.Business.MessageStatusCodeList.Codes.Error);
			CreateAndPopulateHouseBill(ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted, ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted);
			CreateAndPopulateHouseBill(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, ZString.Empty);
			CreateAndPopulateMasterBill();

			CreateAndPopulatePack(header.Bills[0]);

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
				});

				menu.MenuItems.FindByText("Send Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Messages.Count);
			AssertMessage(header.Messages[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword);
			AssertStatuses(false);
		}

		[TestDate(2020, 10, 02, 18, 00, 00)]
		public void TestDoNotSendManifestWithoutCertificate()
		{
			PopulateManifestHeader();
			CreateAndPopulateHouseBill(ZString.Empty, ZString.Empty);
			CreateAndPopulateMasterBill();
			CreateAndPopulatePack(header.Bills[0]);

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Company credentials have not been entered. You can enter the credentials from Maintain -> User Admin -> Companies. Picking the company, in the Brokerage Tab.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			AssertEquals(0, header.Messages.Count);
		}

		[TestDate(2020, 10, 02, 18, 00, 00)]
		public void TestDoNotSendManifestWithoutPassword()
		{
			PopulateManifestHeader();
			CreateAndPopulateHouseBill(ZString.Empty, ZString.Empty);
			CreateAndPopulateMasterBill();
			CreateAndPopulatePack(header.Bills[0]);

			var credential = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
			credential.GP_UserID = "USERNAME";
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			GlbCompany.CurrentCompany.Factory.Save();

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
						dialog.SelectAll();
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Company credentials have not been entered. You can enter the credentials from Maintain -> User Admin -> Companies. Picking the company, in the Brokerage Tab.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			AssertEquals(0, header.Messages.Count);
		}

		[TestDate(2020, 10, 02, 18, 00, 00)]
		public void TestDoNotSendManifestWithInvalidPassword()
		{
			SetCertificate();

			PopulateManifestHeader();
			CreateAndPopulateHouseBill(ZString.Empty, ZString.Empty);
			CreateAndPopulateMasterBill();
			CreateAndPopulatePack(header.Bills[0]);

			var credential = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;

			GlbCompany.CurrentCompany.Factory.Save();

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
						dialog.SelectAll();
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("It is necessary to review and update the certificate information in the Brokerage tab on the Company form before to send a new Manifest.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			AssertEquals(0, header.Messages.Count);
		}

		[TestDate(2021, 02, 08, 18, 00, 00)]
		public void TestDoNotSendAwaitingManifest()
		{
			SetCertificate();

			PopulateManifestHeader();
			CreateAndPopulateHouseBill("AWA", "SNT");
			CreateAndPopulateMasterBill();
			CreateAndPopulatePack(header.Bills[0]);

			header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
						dialog.SelectAll();
					});

					AssertNull(menu.MenuItems.FindByText("Send Manifest"));
				}
			}
			AssertEquals(0, header.Messages.Count);
		}

		[TestDate(2021, 02, 08, 18, 00, 00)]
		public void TestDoNotSendCancelledManifest()
		{
			SetCertificate();

			PopulateManifestHeader();
			CreateAndPopulateHouseBill("CAN", "CAN");
			CreateAndPopulateMasterBill();
			CreateAndPopulatePack(header.Bills[0]);

			header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Cancel;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
						dialog.SelectAll();
					});

					AssertNull(menu.MenuItems.FindByText("Send Manifest"));
				}
			}
			AssertEquals(0, header.Messages.Count);
		}

		[TestDate(2020, 10, 02, 18, 00, 00)]
		public void TestAskBeforeSendManifest()
		{
			SetCertificate();

			PopulateManifestHeader();
			CreateAndPopulateHouseBill(ZString.Empty, ZString.Empty);
			CreateAndPopulateMasterBill();

			Factory.Save();

			AssertEquals(0, header.Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					AssertEquals(1, menu.MenuItems.Count);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menu.MenuItems.FindByText("Send Manifest").PerformClick();

					AssertMultilineASCIIEquals("Popup message", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Number Type: The code you have selected is not in the list.
Bill Number: At least one Pack should be inserted
Gross Weight: You have not entered a Gross Weight.
Gross Weight Unit: You have not entered a Gross Weight Unit.
Quantity (on Bill): You have not entered a Quantity (on Bill).
Manifest UQ: You have not entered a Manifest UQ.
Customs Office: The code you have selected is not in the list.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[TestDate(2020, 12, 28, 18, 00, 00, 00)]
		public void TestCancelBill_OneBillSelected()
		{
			SetCertificate();

			PopulateManifestHeader();
			CreateAndPopulateHouseBill(ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted, ZString.Empty);
			CreateAndPopulateMasterBill();
			CreateAndPopulatePack(header.Bills[0]);
			header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
				});

				menu.MenuItems.FindByText("Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertMessage(header.Messages[0], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword);

			Assert("For CAN Registro Tipo should be B", header.Messages[0].EM_MessageText.Contains("<RegistroTipo>B</RegistroTipo>"));
		}

		[TestDate(2020, 12, 28, 18, 00, 00, 00)]
		public void TestAmendGeneralInformation()
		{
			SetInfoForAmend();
			header.Bills[0].ABL_Transshipment = true;

			header.Bills[0].Packs[0].APA_GoodsDescription = "Changed";

			Factory.Save();

			AssertEquals(1, header.Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Amend Manifest").PerformClick();
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(2, header.Messages.Count);
			AssertMessage(header.Messages[1], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword);

			Assert("For Amend if the descripton of a pack change", header.Messages[1].EM_MessageText.Contains("<MercaderiaDescripcion>Changed</MercaderiaDescripcion>"));
		}

		[TestDate(2020, 12, 28, 18, 00, 00, 00)]
		public void TestAmendWhenConsigneeChange()
		{
			SetInfoForAmend();
			header.Bills[0].ABL_ConsigneeRegNo = "211110890019";

			Factory.Save();

			AssertEquals(1, header.Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Amend Manifest").PerformClick();
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(2, header.Messages.Count);
			AssertMessage(header.Messages[1], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword);

			Assert("For Amend if the Consignee change", header.Messages[1].EM_MessageText.Contains("<ConsignatarioDocumento>211110890019</ConsignatarioDocumento>"));
		}

		[TestDate(2020, 12, 28, 18, 00, 00, 00)]
		public void TestAmendWhenGoodsChange()
		{
			SetInfoForAmend();

			header.Bills[0].Packs[0].APA_ArrivedQuantity = 720;

			Factory.Save();

			AssertEquals(1, header.Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Amend Manifest").PerformClick();
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(2, header.Messages.Count);
			AssertMessage(header.Messages[1], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword);

			Assert("For Amend if Goods Qty change", header.Messages[1].EM_MessageText.Contains("<BultoCantidad>720.000</BultoCantidad>"));
		}

		[TestDate(2020, 12, 28, 18, 00, 00, 00)]
		public void TestAmendNewPackAdded()
		{
			SetInfoForAmend();

			CreateAndPopulatePack(header.Bills[0]);

			Factory.Save();

			AssertEquals(1, header.Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Amend Manifest").PerformClick();
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(2, header.Messages.Count);
			AssertMessage(header.Messages[1], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword);

			Assert("For Amend if New Pack is added", header.Messages[1].EM_MessageText.Contains("<LineaNumero>1</LineaNumero>"));
			Assert("For Amend if New Pack is added", header.Messages[1].EM_MessageText.Contains("<LineaNumero>2</LineaNumero>"));
		}

		[TestDate(2020, 12, 28, 18, 00, 00, 00)]
		public void TestAmendGeneralInformationPlusGoods()
		{
			SetInfoForAmend();
			header.Bills[0].ABL_Transshipment = true;
			header.Bills[0].Packs[0].APA_ArrivedQuantity = 720;

			Factory.Save();

			AssertEquals(1, header.Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Amend Manifest").PerformClick();
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(2, header.Messages.Count);
			AssertMessage(header.Messages[1], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword);

			Assert("For Amend General Info Plus Goods", header.Messages[1].EM_MessageText.Contains("<BultoCantidad>720.000</BultoCantidad>"));
			Assert("For Amend General Info Plus Goods", header.Messages[1].EM_MessageText.Contains("<Trasbordo>S</Trasbordo>"));
		}

		[TestDate(2020, 12, 28, 18, 00, 00, 00)]
		public void TestAmendNewBillToSend()
		{
			SetInfoForAmend();

			CreateAndPopulateHouseBill("", "");
			CreateAndPopulatePack(header.Bills[1]);

			Factory.Save();

			AssertEquals(1, header.Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Amend Manifest").PerformClick();
				AssertEquals("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(2, header.Messages.Count);
			AssertMessage(header.Messages[1], Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword);

			Assert("For Amend New Bill Added", header.Messages[1].EM_MessageText.Contains("<ConocimientoNumeroSecuencial>5</ConocimientoNumeroSecuencial>"));
			Assert("For Amend New Bill Added", header.Messages[1].EM_MessageText.Contains("<ConocimientoNumeroSecuencial>2</ConocimientoNumeroSecuencial>"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			GlbCompany.CurrentCompany.GC_BusinessRegNo = "210413450015";
			GlbStaff.CurrentUser.GS_EmailAddress = "AC_ADUANAS@dhluy.com";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Uruguay);
			helper.CreateCusMapType(AsycudaBill.UYConstants.CountryMapType, "OUT", AsycudaBill.UYConstants.CountryMapType, true);
			helper.CreateCusMap(AsycudaBill.UYConstants.CountryMapType, Core.Constants.CountryCodes.Uruguay, "858", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Uruguay);

			func = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UYMAN, Core.Constants.CountryCodes.Uruguay, ZDateTime.Now, true);

			Factory.Save();
		}

		protected override void TearDown()
		{
			func.Dispose();
			base.TearDown();
		}

		void AssertMessage(EDIMessage message, GlbCompanyCredential credential)
		{
			CombineAssertions(() =>
			{
				AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.UYCustoms);
				AssertEquals(message.EM_ApplicationReference, header.AMA_JobReference);
				AssertEquals(message.EM_IsTestMessage, UYCustomsDataRegistry.Instance.IsUYTestingSystem);
				AssertEquals(message.EM_LinkUniqueID, header.PK);
				AssertEquals(message.EM_MessageOwner, credential.GP_UserID);
				AssertEquals(message.EM_MessageType, EDIInterchangeTypeList.Codes.UYCustoms);
				AssertEquals(message.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				AssertEquals(message.EM_Status, EDIMessage.Status.Queued);
				AssertEquals(message.EM_GP, credential.PK);
			});
		}

		void AssertStatuses(ZBool secondBillSent)
		{
			CombineAssertions(() =>
			{
				AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting, header.AMA_MessageStatus);
				AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, header.Bills[0].ABL_BillStatus);
				AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting, header.Bills[0].ABL_MessageStatus);
				if (secondBillSent)
				{
					AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, header.Bills[1].ABL_BillStatus);
					AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Awaiting, header.Bills[1].ABL_MessageStatus);
				}
				else
				{
					AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Error, header.Bills[1].ABL_BillStatus);
					AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Error, header.Bills[1].ABL_MessageStatus);
				}
				AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted, header.Bills[2].ABL_BillStatus);
				AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted, header.Bills[2].ABL_MessageStatus);
				AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, header.Bills[3].ABL_BillStatus);
				AssertEquals(ZString.Empty, header.Bills[3].ABL_MessageStatus);
			});
		}

		void PopulateManifestHeader()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "123456";
			org.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.CID, "213369370010");

			var orgAddress = org.MainAddress;
			orgAddress.Address1 = "1345";

			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = "RUT";
			orgCusCode.OK_CustomsRegNo = "214182520016";
			orgCusCode.OK_RN_NKCodeCountry = "UY";

			header.ShippingAgentOrgPK = org.PK;
			header.ShippingAgentOrg.Addresses.Add(orgAddress);
			header.AMA_OA_Carrier = orgAddress.PK;

			header.AMA_CustomsOffice = "2081";
			header.AMA_DateAtCustomsOffice = new ZDate(2019, 10, 31);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Voyage = "UC1103";

			Factory.Save();
		}

		void CreateAndPopulateHouseBill(ZString billStatus, ZString messageStatus)
		{
			AsycudaBill bill = header.Bills.AddNew();

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "UY";
			org.OH_FullName = "ConsigneeName";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "211110890017");

			orgAddress.OA_Address1 = "ConsigneeAddress1";
			orgAddress.OA_Address2 = "ConsigneeAddress2";
			orgAddress.OA_State = "ConsigneeState";
			orgAddress.OA_RN_NKCountryCode = "UY";
			bill.ABL_OA_Consignee = orgAddress.PK;

			org.OH_FullName = "THERMO ORION/THERMO ELECTRON";
			bill.ABL_OA_Shipper = orgAddress.PK;

			org.OH_FullName = "ELECO S.A.";
			orgAddress.OA_Address1 = "ROMAN GARCIA 1086";
			orgAddress.OA_Phone = "23046888";
			bill.ABL_OA_NotifyParty = orgAddress.PK;

			bill.ABL_BillNumber = "7YM7014";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_RL_NKOrigin = "USMIA";
			bill.ABL_RL_NKPortOfDischarge = "UYMVD";
			bill.ABL_RL_NKFinalDestination = "UYMVD";
			bill.ABL_Volume = 1.000m;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_Transshipment = false;
			bill.CustomsEntryNumber = "5";
			bill.CustomsEntryNumberType = "DNA";
			bill.ABL_BillStatus = billStatus;
			bill.ABL_MessageStatus = messageStatus;

			Factory.Save();
		}

		void CreateAndPopulateMasterBill()
		{
			AsycudaBill bill = (AsycudaBill)header.MasterBill;

			bill.ABL_BillNumber = "04507816955";
			bill.ABL_RL_NKPortOfLoading = "USMIA";
			bill.ABL_RL_NKPortOfDischarge = "UYMVD";

			Factory.Save();
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			AsycudaPack pack = bill.Packs.AddNew();

			pack.APA_Weight = 127.900m;
			pack.APA_WeightUQ = "KG";
			pack.APA_PackUQ = "BBK";
			pack.APA_PackQty = 1;
			pack.APA_MarksAndNumbers = "MarksAndNumbers";
			pack.APA_GoodsDescription = "DIAGNOSTIC LABORATORY";

			Factory.Save();
		}

		void SetCertificate()
		{
			var credential = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
			credential.GP_UserID = "USERNAME";
			credential.CurrentDecryptedPassword = "PASSWORD";
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			GlbCompany.CurrentCompany.Factory.Save();
		}

		UYMessage CreateEDIMessage(ZString messageText)
		{
			var message = Factory.New<UYMessage>();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UYCustoms;
			message.EM_MessageType = MessageTypes.Codes.UYC;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = messageText;

			Factory.Save();
			return message;
		}

		void SetInfoForAmend()
		{
			SetCertificate();

			PopulateManifestHeader();
			CreateAndPopulateHouseBill(ZString.Empty, ZString.Empty);
			CreateAndPopulateMasterBill();
			CreateAndPopulatePack(header.Bills[0]);

			var message = CreateEDIMessage(CreateFirstSend());
			message.EM_LinkedObject = header;

			header.AMA_MessageStatus = "ACP";
			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[0].ABL_MessageStatus = "ACP";
		}

		ZString CreateFirstSend()
		{
			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
				});

				menu.MenuItems.FindByText("Send Manifest").PerformClick();
			}

			header.Messages[0].EM_Status = EDIMessageStatusList.Codes.Sent;
			return header.Messages[0].EM_MessageText;
		}
	}
}
