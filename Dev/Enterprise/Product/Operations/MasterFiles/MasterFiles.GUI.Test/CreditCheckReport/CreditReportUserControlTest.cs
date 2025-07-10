using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Test;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using WTG.CreditCheck;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class CreditReportUserControlTest : TestCaseWithFactory
	{
		public void TestSaveIdentifiers_NotSilent_WithConcurrencyIssues()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_RL_NKClosestPort = "AUSYD";
			header.OH_Code = "MYORGSYD";
			header.Factory.Save();

			var headerInAnotherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<OrgHeader>(header.PK);

			using (var form = new ZOrganisationsForm(header))
			using (var control = new CreditReportUserControl())
			{
				var mockService = new Mock<ISupportCreditCheckService>();
				mockService.Setup(service => service.GetHttpClientHandler(It.IsAny<string>())).Returns(new MockHttpMessageHandler());
				mockService.Setup(service => service.IsProductionSystem).Returns(control.ServiceWrapper.IsProductionSystem);
				mockService.Setup(service => service.EndpointAddresses).Returns(control.ServiceWrapper.EndpointAddresses);
				control.ServiceWrapper = mockService.Object;
				form.Controls.Add(control);

				form.Show();

				AssertEquals(true, headerInAnotherFactory.SaveIdentifiers(
					new List<Identifier>
					{
						new Identifier
						{
							ID = "987654321",
							Type = IdentifierType.ACN
						}
					}, false));

				headerInAnotherFactory.Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals(false, control.SaveIdentifiers(
					new List<Identifier>
					{
						new Identifier
						{
							ID = "123456789",
							Type = IdentifierType.ACN
						}
					}, false));

				AssertContains(@"Probably before you confirm the organization, another user has already saved same type identifier(s) for this organization.
Please check the existing identifier(s) under: Details -> Config -> Registration Numbers / Codes after refreshing the organization form.", UnitTestUserNotification.Instance.LastMessage.Text);

				var configTabControl = form.DetailsControl.ConfigUserControl.ConfigTabControl;
				AssertEquals("should jump to RegistrationCodesTabPage", configTabControl.Controls?.OfType<ZTabPage>().Single(c => c.Name == "RegistrationCodesTabPage"), configTabControl.SelectedTab);
			}
		}

		public void TestSaveIdentifiers_Silent_Successful()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.Factory.Save();

			AssertEquals(0, header.CustomsCodes.Count);

			using (var form = new ZOrganisationsForm(header))
			using (var control = new CreditReportUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, header.SaveIdentifiers(
					new List<Identifier>
					{
						new Identifier
						{
							ID = "987654321",
							Type = IdentifierType.DUNS
						}
					}, true));
			}

			header = new BusinessObjectFactory().Load<OrgHeader>(header.PK);

			AssertEquals(1, header.CustomsCodes.Count);
			AssertEquals("987654321", header.CustomsCodes.Cast<OrgCusCode>().Single().OK_CustomsRegNo);
			AssertEquals("DUN", header.CustomsCodes.Cast<OrgCusCode>().Single().OK_CodeType);
		}

		public void TestSaveIdentifiers_Silent_Fail()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.Factory.Save();

			AssertEquals(0, header.CustomsCodes.Count);

			using (var form = new ZOrganisationsForm(header))
			using (var control = new CreditReportUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, header.SaveIdentifiers(
					new List<Identifier>
					{
					}, true));
			}

			header = new BusinessObjectFactory().Load<OrgHeader>(header.PK);
			AssertEquals(0, header.CustomsCodes.Count);
		}

		public void TestShowTermsAndAgreement()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new ZForm(org))
			{
				using (var control = new CreditReportUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(org, "");
					var terms = new CreditTermsAcknowledgementCheckerTest.CreditReportTermsAndConditionForTest();
					var acknowledgeChecker = new TermsAcknowledgementChecker(terms, control.ParentForm);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					var result = acknowledgeChecker.CheckTermAcknowledged();
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, result.Result);
				}
			}
		}

		public void TestIsProductionSystem()
		{
			using (var control = new CreditReportUserControl())
			{
				Assert("Pre-condition: Should be Test system", !Env.Instance.IsProductionSystem);
				AssertEquals(false, control.ServiceWrapper.IsProductionSystem);

				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
				Assert("Should be Production system now", Env.Instance.IsProductionSystem);
				AssertEquals(true, control.ServiceWrapper.IsProductionSystem);

				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
				AssertEquals(false, control.ServiceWrapper.IsProductionSystem);

				using (OrganisationsDataRegistry.Instance.EnableRealCreditCheckServiceInTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals(true, control.ServiceWrapper.IsProductionSystem);
				}
			}
		}

		#region Credit Reports

		public void TestCreditReportUserControl_HasCreditCheckUserControl()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZForm())
			using (var control = new CreditReportUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(1, control.Controls.OfType<CreditCheckUserControl>().Count());
			}
		}

		public void TestGetAvailableCreditReportTypes()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var creditReportItem = new CreditReportItem();
			creditReportItem.CountryEnabledForCompany = true;
			creditReportItem.CountryEnabledForOrganisation = true;
			creditReportItem.CommercialBureauEnquiryEnabled = true;
			creditReportItem.FailureRiskEnabled = true;
			creditReportItem.ComprehensiveReportEnabled = false;
			creditReportItem.LatePaymentRiskEnabled = false;

			var creditReportItemCollection = new CreditReportItemCollection();
			creditReportItemCollection.Add(creditReportItem);
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			using (var form = new ZForm(org))
			using (var detailsControl = new CreditReportUserControl())
			{
				var mockService = new Mock<ISupportCreditCheckService>();
				mockService.Setup(service => service.GetHttpClientHandler(It.IsAny<string>())).Returns(new MockHttpMessageHandler());
				mockService.Setup(service => service.IsProductionSystem).Returns(detailsControl.ServiceWrapper.IsProductionSystem);
				mockService.Setup(service => service.EndpointAddresses).Returns(detailsControl.ServiceWrapper.EndpointAddresses);
				detailsControl.ServiceWrapper = mockService.Object;
				form.Controls.Add(detailsControl);
				form.Show();

				AssertContainsExactElementsInAnyOrder(new[] { CreditReportType.FailureRisk, CreditReportType.CommercialBureauEnquiry }, detailsControl.AvailableCreditReportTypes);
			}
		}

		public void TestSupportCreditCheck()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			AssertEquals(org.HasChanges, false);

			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)"https://www.baidu.com", Bool = true });

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new ZForm(org))
			using (var detailsControl = new CreditReportUserControl())
			{
				form.Controls.Add(detailsControl);
				form.Show();

				var supportCreditCheck = detailsControl;

				CombineAssertions(() =>
				{
					AssertEquals(false, supportCreditCheck.Header.HasChanges);
					AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, supportCreditCheck.EnterpriseCode);
					AssertEquals(org.PK.ToGuid(), supportCreditCheck.EntityPk);
					AssertEquals("https://www.baidu.com", supportCreditCheck.ServiceWrapper.EndpointAddresses.FirstOrDefault(x => x.IsMain).Address);
					AssertEquals(false, supportCreditCheck.SaveIdentifiers(null, false));

					org.OH_FullName = "New Name";
					AssertEquals(true, supportCreditCheck.Header.HasChanges);
				});
			}
		}

		public void TestSupportCreditCheckSecurityCheckpoints()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)"https://www.baidu.com", Bool = true });

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new ZForm(org))
			using (var control = new CreditReportUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var supportCreditCheck = control;

				CombineAssertions(() =>
				{
					AssertEquals("CreditReportUserControl should have 8 security checkpoints", 8, supportCreditCheck.SecurityCheckpoints.Count);

					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.ComprehensiveReport, true, Env.Security.OrganisationCreditReportsGetReportComprehensiveReport, true);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.ComprehensiveReport, true, Env.Security.OrganisationCreditReportsGetReportComprehensiveReport, false);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.ComprehensiveReport, false, Env.Security.OrganisationCreditReportsRenewReportComprehensiveReport, true);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.ComprehensiveReport, false, Env.Security.OrganisationCreditReportsRenewReportComprehensiveReport, false);

					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.FailureRisk, true, Env.Security.OrganisationCreditReportsGetReportFailureRisk, true);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.FailureRisk, true, Env.Security.OrganisationCreditReportsGetReportFailureRisk, false);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.FailureRisk, false, Env.Security.OrganisationCreditReportsRenewReportFailureRisk, true);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.FailureRisk, false, Env.Security.OrganisationCreditReportsRenewReportFailureRisk, false);

					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.LatePaymentRisk, true, Env.Security.OrganisationCreditReportsGetReportLatePaymentRisk, true);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.LatePaymentRisk, true, Env.Security.OrganisationCreditReportsGetReportLatePaymentRisk, false);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.LatePaymentRisk, false, Env.Security.OrganisationCreditReportsRenewReportLatePaymentRisk, true);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.LatePaymentRisk, false, Env.Security.OrganisationCreditReportsRenewReportLatePaymentRisk, false);

					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.CommercialBureauEnquiry, true, Env.Security.OrganisationCreditReportsGetReportCommercialBureauEnquiry, true);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.CommercialBureauEnquiry, true, Env.Security.OrganisationCreditReportsGetReportCommercialBureauEnquiry, false);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.CommercialBureauEnquiry, false, Env.Security.OrganisationCreditReportsRenewReportCommercialBureauEnquiry, true);
					AssertSecurityCheckpoint(supportCreditCheck, CreditReportType.CommercialBureauEnquiry, false, Env.Security.OrganisationCreditReportsRenewReportCommercialBureauEnquiry, false);
				});
			}
		}

		void AssertSecurityCheckpoint(CreditReportUserControl creditCheck, CreditReportType reportType, bool isGet, SecurityCheckpoint coreSecurityCheckpoint, bool isSecurityCheckpointAllowed)
		{
			var originalValue = coreSecurityCheckpoint.IsAllowed;

			try
			{
				coreSecurityCheckpoint.IsAllowed = isSecurityCheckpointAllowed;

				if (creditCheck.SecurityCheckpoints.TryGetValue((reportType, isGet), out var creditCheckSecurityCheckpoint))
				{
					AssertEquals("CreditReportUserControl SecurityItem has correct IsAllowed", coreSecurityCheckpoint.IsAllowed, creditCheckSecurityCheckpoint.IsAllowed);
					AssertEquals("CreditReportUserControl SecurityItem has correct ErrorMessageForNotAllowed", coreSecurityCheckpoint.ErrorMessageForNotAllowed, creditCheckSecurityCheckpoint.ErrorMessageForNotAllowed);
				}
				else
				{
					Assert("Checkpoint exists in SecurityCheckpoints", false);
				}
			}
			finally
			{
				coreSecurityCheckpoint.IsAllowed = originalValue;
			}
		}

		public void TestIdentifiers()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)"https://www.baidu.com", Bool = true });

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new ZForm(org))
			using (var control = new CreditReportUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var supportCreditCheck = control;

				AssertEquals(0, supportCreditCheck.Identifiers.Count());

				var abnCusCode = org.CustomsCodes.AddNew();
				abnCusCode.OK_CustomsRegNo = "123";
				abnCusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
				abnCusCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.China;
				AssertEquals(0, supportCreditCheck.Identifiers.Count());

				abnCusCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
				AssertEquals(1, supportCreditCheck.Identifiers.Count());
				AssertEquals("123", supportCreditCheck.Identifiers.Single(x => x.Type == IdentifierType.ABN).ID);

				var acnCusCode = org.CustomsCodes.AddNew();
				acnCusCode.OK_CustomsRegNo = "234";
				acnCusCode.OK_CodeType = OrgCusCode.MozambiqueCodeTypes.GCR;
				acnCusCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.China;
				AssertEquals(1, supportCreditCheck.Identifiers.Count());
				AssertEquals(null, supportCreditCheck.Identifiers.SingleOrDefault(x => x.Type == IdentifierType.ACN));

				acnCusCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
				AssertEquals(2, supportCreditCheck.Identifiers.Count());
				AssertEquals("234", supportCreditCheck.Identifiers.Single(x => x.Type == IdentifierType.ACN).ID);

				var nzbnCusCode = org.CustomsCodes.AddNew();
				nzbnCusCode.OK_CustomsRegNo = "456";
				nzbnCusCode.OK_CodeType = OrgCusCode.CodeTypes.CompanyNumber;
				nzbnCusCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.China;
				AssertEquals(2, supportCreditCheck.Identifiers.Count());
				AssertEquals(null, supportCreditCheck.Identifiers.SingleOrDefault(x => x.Type == IdentifierType.NZBN));

				nzbnCusCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.NewZealand;
				AssertEquals(3, supportCreditCheck.Identifiers.Count());
				AssertEquals("456", supportCreditCheck.Identifiers.Single(x => x.Type == IdentifierType.NZBN).ID);

				var ncnCusCode = org.CustomsCodes.AddNew();
				ncnCusCode.OK_CustomsRegNo = "678";
				ncnCusCode.OK_CodeType = OrgCusCode.SamoaCodeTypes.GST;
				ncnCusCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.China;
				AssertEquals(3, supportCreditCheck.Identifiers.Count());
				AssertEquals(null, supportCreditCheck.Identifiers.SingleOrDefault(x => x.Type == IdentifierType.NCN));

				ncnCusCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.NewZealand;
				AssertEquals(4, supportCreditCheck.Identifiers.Count());
				AssertEquals("678", supportCreditCheck.Identifiers.Single(x => x.Type == IdentifierType.NCN).ID);

				var dunCusCode = org.CustomsCodes.AddNew();
				dunCusCode.OK_CustomsRegNo = "789";
				dunCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				AssertEquals(5, supportCreditCheck.Identifiers.Count());
				AssertEquals("789", supportCreditCheck.Identifiers.SingleOrDefault(x => x.Type == IdentifierType.DUNS).ID);
			}
		}

		#endregion
	}
}
