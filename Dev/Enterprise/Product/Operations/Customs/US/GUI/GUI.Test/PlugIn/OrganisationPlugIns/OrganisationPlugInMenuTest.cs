using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.US.GUI
{
	sealed class OrganisationPlugInMenuTest : TestCaseWithFactory
	{
		public void TestAddImporterNumberFireSaveButtonGetsCalled()
		{
			Env.Security.OrgConfigModifyFinancialRegistrationNumbers.IsAllowed = true;
			using (var form = new MasterFiles.GUI.BaseOrganisationsForm(organisation))
			{
				var mockMenu = new Mock<OrganisationPlugInMenu>(organisationWrapper, new Action(() => { }));
				using (OrganisationPlugInMenu menu = mockMenu.Object)
				{
					form.Menu = new MainMenu(new MenuItem[] { menu });
					SetUpErrorDataAgainstOrganisation();
					OrgAddressMessageData messageData = new OrgAddressMessageData(organisationWrapper);
					mockMenu
						.Protected()
						.Setup<OrgAddressMessageData>("GetOrgAddressMessageData")
						.Returns(messageData);
					messageData.US_RN_NKCountry1 = "MX"; //this should cause an error
					using (OrgAddressMessageDataForm messageDataForm = new OrgAddressMessageDataForm(messageData))
					{
						messageDataForm.IsOKToSendMessage = true;
						mockMenu
							.Protected()
							.Setup<OrgAddressMessageDataForm>("GetOrgAddressMessageDataForm", ItExpr.IsAny<OrgAddressMessageData>())
							.Returns(messageDataForm);
						menu.sendADDMessageToCustomsMenu.PerformClick();
						AssertEquals(OrganisationPlugInMenu.SaveOrganisationAdvice, UnitTestUserNotification.Instance.LastMessage.Text);
						Factory.Save();
						menu.sendADDMessageToCustomsMenu.PerformClick();
						AssertEquals("HasError in Organisation", true, organisation.HasErrors);
						AssertEquals("FireSaveButton() got called to check if there is an error there", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(messageDataForm, ZFormModaliser.LastFormShownDialogForTest);
						ZFormModaliser.LastFormShownDialogForTest = null;
						ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
					}
				}
				mockMenu.VerifyAll();
			}
		}

		public void TestSendQueryImporterBondMessage()
		{
			organisation.OH_IsConsignee = false;
			using (OrganisationPlugIn plugIn = new OrganisationPlugIn(organisation))
			using (var form = new MasterFiles.GUI.BaseOrganisationsForm(organisation))
			{
				var mainMenu = (OrganisationPlugInMenu)plugIn.TopLevelMenu;
				var mockMenu = new Mock<OrganisationPlugInMenu>(organisationWrapper, mainMenu.licenceLogIn) { CallBase = true };
				using (OrganisationPlugInMenu menu = mockMenu.Object)
				{
					form.Menu = new MainMenu(new MenuItem[] { menu });
					organisation.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABCDEFGH");
					menu.queryImporterBondMenu.PerformClick();
					AssertEquals(OrganisationPlugInMenu.QueryCannotBeSentBecauseNotConsignee, UnitTestUserNotification.Instance.LastMessage.Text);
					organisation.OH_IsConsignee = true;
					menu.queryImporterBondMenu.PerformClick();
					AssertEquals(OrganisationPlugInMenu.SaveOrganisationAdvice, UnitTestUserNotification.Instance.LastMessage.Text);
					Factory.Save();
					menu.queryImporterBondMenu.PerformClick();
					AssertEquals(OrganisationPlugInMenu.QueryImporterBond_NoIRSNumber, UnitTestUserNotification.Instance.LastMessage.Text);
					OrgCusCode cusCode = organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567-NN");
					ZStringBuilder builder = new ZStringBuilder(string.Format(OrganisationPlugInMenu.QueryImporterBond_NoValidIRSNumber, "Employer Identification Number"));
					builder.Append(EmployerIdentificationNumberValidator.EINNumberRightFormat);
					ZString errorMessage = builder.ToStringWithNewLineBetweenAppends();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Factory.Save();
					menu.queryImporterBondMenu.PerformClick();
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					mockMenu.VerifyAll();
					cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
					cusCode.OK_CustomsRegNo = "09ABCD-123-45";
					builder = new ZStringBuilder(string.Format(OrganisationPlugInMenu.QueryImporterBond_NoValidIRSNumber, "CBP Assigned Number"));
					builder.Append(CBPAssignedNumberValidator.CBPAssignedNumberRightFormat);
					errorMessage = builder.ToStringWithNewLineBetweenAppends();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Factory.Save();
					menu.queryImporterBondMenu.PerformClick();
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					mockMenu.VerifyAll();
					Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
					cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
					cusCode.OK_CustomsRegNo = "123-45-67-89";
					builder = new ZStringBuilder();
					builder.Append(SocialSecurityNumberValidator.DoesNotHavePermissionToSendSSNErrorMessage);
					errorMessage = builder.ToStringWithNewLineBetweenAppends();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Factory.Save();
					menu.queryImporterBondMenu.PerformClick();
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					mockMenu.VerifyAll();
					Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
					builder = new ZStringBuilder(string.Format(OrganisationPlugInMenu.QueryImporterBond_NoValidIRSNumber, "Social Security Number"));
					builder.Append(SocialSecurityNumberValidator.SocialSecurityNumberRightFormat);
					errorMessage = builder.ToStringWithNewLineBetweenAppends();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Factory.Save();
					menu.queryImporterBondMenu.PerformClick();
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					mockMenu
						.Protected()
						.Verify<QueryImporterBondForm>("GetQueryImporterBondForm", Times.Never(), ItExpr.IsAny<ZString>());
					cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
					cusCode.OK_CustomsRegNo = CorrectEINForTest;
					SetUpValidDataForADDMessageAgainstOrganisation();
					organisation.OH_RL_NKClosestPort = "USCHI";
					mockMenu.Reset();
					using (QueryImporterBondForm queryImporterBondForm = new QueryImporterBondForm(CorrectEINForTest))
					{
						queryImporterBondForm.IsOKToSendMessage = true;
						mockMenu
							.Protected()
							.Setup<QueryImporterBondForm>("GetQueryImporterBondForm", ItExpr.IsAny<ZString>())
							.Returns(queryImporterBondForm);
						Factory.Save();
						Assert(!Env.Licence.ImportBroker.IsLoggedIn);
						menu.queryImporterBondMenu.PerformClick();
						Assert(Env.Licence.ImportBroker.IsLoggedIn);
						AssertEquals(ZString.Format(OrganisationPlugInMenu.QueryImporterBond_MessageSent, CorrectEINForTest), UnitTestUserNotification.Instance.LastMessage.Text);
						mockMenu.VerifyAll();
					}

					cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
					cusCode.OK_CustomsRegNo = CorrectCBPForTest;
					mockMenu.Reset();
					using (QueryImporterBondForm queryImporterBondForm = new QueryImporterBondForm(CorrectCBPForTest))
					{
						queryImporterBondForm.IsOKToSendMessage = true;
						mockMenu
							.Protected()
							.Setup<QueryImporterBondForm>("GetQueryImporterBondForm", ItExpr.IsAny<ZString>())
							.Returns(queryImporterBondForm);
						Factory.Save();
						menu.queryImporterBondMenu.PerformClick();
						AssertEquals(ZString.Format(OrganisationPlugInMenu.QueryImporterBond_MessageSent, CorrectCBPForTest), UnitTestUserNotification.Instance.LastMessage.Text);
						mockMenu.VerifyAll();
					}

					cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
					cusCode.OK_CustomsRegNo = CorrectSSNForTest;
					mockMenu.Reset();
					using (QueryImporterBondForm queryImporterBondForm = new QueryImporterBondForm(CorrectSSNForTest))
					{
						queryImporterBondForm.IsOKToSendMessage = true;
						mockMenu
							.Protected()
							.Setup<QueryImporterBondForm>("GetQueryImporterBondForm", ItExpr.IsAny<ZString>())
							.Returns(queryImporterBondForm);
						Factory.Save();
						menu.queryImporterBondMenu.PerformClick();
						AssertEquals(ZString.Format(OrganisationPlugInMenu.QueryImporterBond_MessageSent, CorrectSSNForTest), UnitTestUserNotification.Instance.LastMessage.Text);
						mockMenu.VerifyAll();
					}

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
					cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
					Factory.Save();
					menu.queryImporterBondMenu.PerformClick();
					AssertNotEquals(OrganisationPlugInMenu.QueryImporterBond_NoIRSNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestQueryImporterBondFireSaveButtonGetsCalled()
		{
			using (var form = new MasterFiles.GUI.BaseOrganisationsForm(organisation))
			{
				var mockMenu = new Mock<OrganisationPlugInMenu>(organisationWrapper, new Action(() => { }));
				using (OrganisationPlugInMenu menu = mockMenu.Object)
				{
					form.Menu = new MainMenu(new MenuItem[] { menu });
					OrgCusCode cusCode = organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567-NN");
					cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
					cusCode.OK_CustomsRegNo = CorrectEINForTest;
					using (QueryImporterBondForm queryImporterBondForm = new QueryImporterBondForm(CorrectEINForTest))
					{
						organisation.OH_RL_NKClosestPort = "";
						queryImporterBondForm.IsOKToSendMessage = true;
						mockMenu
							.Protected()
							.Setup<QueryImporterBondForm>("GetQueryImporterBondForm", ItExpr.IsAny<ZString>())
							.Returns(queryImporterBondForm);
						menu.queryImporterBondMenu.PerformClick();
						AssertEquals(OrganisationPlugInMenu.SaveOrganisationAdvice, UnitTestUserNotification.Instance.LastMessage.Text);
						Factory.Save();
						menu.queryImporterBondMenu.PerformClick();
						AssertEquals("HasError in Organisation", true, organisation.HasErrors);
						AssertEquals("FireSaveButton() got called to check if there is an error there", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
						ZFormModaliser.LastFormShownDialogForTest = null;
						ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
						SetUpValidDataForADDMessageAgainstOrganisation();
						queryImporterBondForm.IsOKToSendMessage = true;
						mockMenu
							.Protected()
							.Setup<QueryImporterBondForm>("GetQueryImporterBondForm", ItExpr.IsAny<ZString>())
							.Returns(queryImporterBondForm);
						Factory.Save();
						menu.queryImporterBondMenu.PerformClick();
						AssertEquals(ZString.Format(OrganisationPlugInMenu.QueryImporterBond_MessageSent, CorrectEINForTest), UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestAddImporterNumberWhenEverythingIsOK()
		{
			organisation.Factory.Save();
			Env.Security.OrgConfigModifyFinancialRegistrationNumbers.IsAllowed = false;
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			using (var form = new MasterFiles.GUI.BaseOrganisationsForm(organisation))
			{
				var mockMenu = new Mock<OrganisationPlugInMenu>(organisationWrapper, new Action(() => { }));
				using (OrganisationPlugInMenu menu = mockMenu.Object)
				{
					form.Menu = new MainMenu(new MenuItem[] { menu });
					SetUpValidDataForADDMessageAgainstOrganisation();
					var messageData = new OrgAddressMessageData(organisationWrapper);
					messageData.US_ImporterNumber = "123-12-1234";
					mockMenu
						.Protected()
						.Setup<OrgAddressMessageData>("GetOrgAddressMessageData")
						.Returns(messageData);
					messageData.US_ImporterName = "Changed";
					using (OrgAddressMessageDataForm messageDataForm = new OrgAddressMessageDataForm(messageData))
					{
						messageDataForm.IsOKToSendMessage = true;
						mockMenu
							.Protected()
							.Setup<OrgAddressMessageDataForm>("GetOrgAddressMessageDataForm", ItExpr.IsAny<OrgAddressMessageData>())
							.Returns(messageDataForm);
						menu.sendADDMessageToCustomsMenu.PerformClick();
						AssertEquals(OrganisationPlugInMenu.HaveNotSecurityRightsForImporterAdd, UnitTestUserNotification.Instance.LastMessage.Text);
						Env.Security.OrgConfigModifyFinancialRegistrationNumbers.IsAllowed = true;
						menu.sendADDMessageToCustomsMenu.PerformClick();
						AssertEquals(OrganisationPlugInMenu.SaveOrganisationAdvice, UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						Factory.Save();
						menu.sendADDMessageToCustomsMenu.PerformClick();
						AssertEquals(SocialSecurityNumberValidator.DoesNotHavePermissionToSendSSNErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						messageData.US_ImporterNumber = "12-1234567AB";
						Factory.Save();
						menu.sendADDMessageToCustomsMenu.PerformClick();
						AssertEquals("should NOT have updated organisation.OH_FullName", "TEST ACCOUNT", organisation.OH_FullName);
						AssertEquals("One message has been generated", 1, organisationWrapper.Messages.Count);
						AssertEquals(messageDataForm, ZFormModaliser.LastFormShownDialogForTest);
						ZFormModaliser.LastFormShownDialogForTest = null;
						ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
					}
				}
			}
		}

		public void TestAddManufacturerFireSaveButtonGetsCalled()
		{
			using (var form = new MasterFiles.GUI.BaseOrganisationsForm(organisation))
			{
				var mockMenu = new Mock<OrganisationPlugInMenu>(organisationWrapper, new Action(() => { }));
				using (OrganisationPlugInMenu menu = mockMenu.Object)
				{
					form.Menu = new MainMenu(new MenuItem[] { menu });
					var messageData = new ManufacturerAddMessageData(organisationWrapper);
					mockMenu
						.Protected()
						.Setup<ManufacturerAddMessageData>("GetManufacturerAddMessageData")
						.Returns(messageData);
					organisation.OH_FullName = ""; //this should cause an error
					using (ManufacturerAddForm messageDataForm = new ManufacturerAddForm(messageData))
					{
						messageDataForm.Result = ManufacturerAddFormResult.SendAddMessage;
						mockMenu
							.Protected()
							.Setup<ManufacturerAddForm>("GetManufacturerAddForm", ItExpr.IsAny<ManufacturerAddMessageData>())
							.Returns(messageDataForm);
						menu.addManufacturerIdentifierMenu.PerformClick();
						AssertEquals("HasError in Organisation", false, organisation.HasErrors);
						organisation.OH_IsConsignor = true;
						menu.addManufacturerIdentifierMenu.PerformClick();
						AssertEquals("HasError in Organisation", false, organisation.HasErrors);
						AssertEquals(OrganisationPlugInMenu.SaveOrganisationAdvice, UnitTestUserNotification.Instance.LastMessage.Text);
						ZFormModaliser.LastFormShownDialogForTest = null;
						ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
					}
				}
			}
		}

		public void TestAddManufacturerWhenEverythingIsOK()
		{
			organisation.OH_IsConsignor = false;
			using (var form = new MasterFiles.GUI.BaseOrganisationsForm(organisation))
			{
				var mockMenu = new Mock<OrganisationPlugInMenu>(organisationWrapper, new Action(() => { }));
				using (OrganisationPlugInMenu menu = mockMenu.Object)
				{
					form.Menu = new MainMenu(new MenuItem[] { menu });
					SetUpValidDataForADDMessageAgainstOrganisation();
					var messageData = new ManufacturerAddMessageData(organisationWrapper);
					mockMenu
						.Protected()
						.Setup<ManufacturerAddMessageData>("GetManufacturerAddMessageData")
						.Returns(messageData);
					using (var messageDataForm = new ManufacturerAddForm(messageData))
					{
						messageDataForm.Result = ManufacturerAddFormResult.SendAddMessage;
						mockMenu
							.Protected()
							.Setup<ManufacturerAddForm>("GetManufacturerAddForm", ItExpr.IsAny<ManufacturerAddMessageData>())
							.Returns(messageDataForm);
						menu.addManufacturerIdentifierMenu.PerformClick();
						AssertEquals(OrganisationPlugInMenu.RequestCannotBeSentBecauseNotSupplier, UnitTestUserNotification.Instance.LastMessage.Text);
						organisation.OH_IsConsignor = true;
						menu.addManufacturerIdentifierMenu.PerformClick();
						AssertEquals(OrganisationPlugInMenu.SaveOrganisationAdvice, UnitTestUserNotification.Instance.LastMessage.Text);
						Factory.Save();
						menu.addManufacturerIdentifierMenu.PerformClick();
						AssertEquals("One message has been generated", 1, organisationWrapper.Messages.Count);
						AssertEquals(messageDataForm, ZFormModaliser.LastFormShownDialogForTest);
						ZFormModaliser.LastFormShownDialogForTest = null;
						ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
					}
				}
			}
		}

		public void TestGlobalBusinessIdentifierFireSaveButtonGetsCalled()
		{
			using (var form = new MasterFiles.GUI.BaseOrganisationsForm(organisation))
			{
				var mockMenu = new Mock<OrganisationPlugInMenu>(organisationWrapper, new Action(() => { }));
				using (OrganisationPlugInMenu menu = mockMenu.Object)
				{
					form.Menu = new MainMenu(new MenuItem[] { menu });

					var messageData = new GlobalBusinessIdentifierData(organisationWrapper);
					mockMenu.Protected().Setup<GlobalBusinessIdentifierData>("GetGlobalBusinessIdentifierData").Returns(messageData);

					using (var messageDataForm = new GlobalBusinessIdentifierForm(messageData))
					{
						messageDataForm.MessageType = GlobalBusinessIdentifierMessageType.Original;
						mockMenu.Protected().Setup<GlobalBusinessIdentifierForm>("GetGlobalBusinessIdentifierForm", ItExpr.IsAny<GlobalBusinessIdentifierData>()).Returns(messageDataForm);
						menu.globalBusinessIdentifierAddDeleteUpdateMenu.PerformClick();
						AssertEquals(OrganisationPlugInMenu.SaveOrganisationAdvice, UnitTestUserNotification.Instance.LastMessage.Text);
						Factory.Save();
						menu.globalBusinessIdentifierAddDeleteUpdateMenu.PerformClick();
						AssertEquals(messageDataForm, ZFormModaliser.LastFormShownDialogForTest);
						ZFormModaliser.LastFormShownDialogForTest = null;
						ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.OH_IsConsignee = true;
			organisationWrapper = OrgHeaderWrapper.New(organisation);
		}
		OrgHeader organisation;
		OrgHeaderWrapper organisationWrapper;

		void SetUpErrorDataAgainstOrganisation()
		{
			SetUpValidDataForADDMessageAgainstOrganisation();
			organisation.OH_FullName = "";
		}

		void SetUpValidDataForADDMessageAgainstOrganisation()
		{
			organisation.OH_FullName = "TEST ACCOUNT";
			organisation.MainAddress.OA_Address1 = "111 Main Road";
			organisation.MainAddress.OA_City = "Chicago";
			organisation.MainAddress.OA_PostCode = "60010";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			organisation.MainAddress.OA_State = "IL";
			organisationWrapper.ZO_ImporterType = ImporterTypeList.Codes.Corporation;
			var cusCode = organisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, GlbCompany.CurrentCompany.Country);
			if (cusCode == null)
			{
				organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567AB");
			}
		}

		const string CorrectEINForTest = "12-1234567NN";
		const string CorrectCBPForTest = "09ABCD-12345";
		const string CorrectSSNForTest = "123-45-6789";
	}
}
