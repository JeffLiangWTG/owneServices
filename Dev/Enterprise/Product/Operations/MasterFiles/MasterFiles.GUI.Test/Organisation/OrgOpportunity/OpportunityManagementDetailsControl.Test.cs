using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class OpportunityManagementDetailsControlTest : TestCaseWithFactory
	{
		[TestedType(typeof(OpportunityManagementDetailsControlForm))]
		public class Test : ZFormBasherTest
		{
			[RequiresSTA]
			public void TestP8_RX_EstimatedValueCurrency_ChangeShouldPromptUserToUpdateExchangeRate()
			{
				var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
				opportunity.P8_RX_NKEstimatedValueCurrency = "AUD";

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					var anotherTextBox = new ZTextBox();
					form.Controls.Add(anotherTextBox);
					form.Show();

					var unitFindBox = form.FindSingle<ZCodeFindBox>("UnitFindBox");
					unitFindBox.Focus();
					unitFindBox.CodeBox.Text = "USD";
					anotherTextBox.Focus();
					using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
					{
						AssertType(typeof(UpdateOpportunityDateForExchangeRateForm), lastShownForm);

						KeySender.PostKeyDown(lastShownForm, Keys.Escape);
						Application.DoEvents();

						AssertEquals("Should revert to previous currency on escape", "AUD", opportunity.P8_RX_NKEstimatedValueCurrency);
					}

					unitFindBox.Focus();
					unitFindBox.CodeBox.Text = "NZD";
					anotherTextBox.Focus();
					using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
					{
						AssertType(typeof(UpdateOpportunityDateForExchangeRateForm), lastShownForm);

						lastShownForm.Close();

						AssertEquals("Should not revert to previous currency if not escaped", "NZD", opportunity.P8_RX_NKEstimatedValueCurrency);
					}

					ZFormModaliser.LastFormShownForTest = null;
					unitFindBox.Focus();
					unitFindBox.CodeBox.Text = "";
					anotherTextBox.Focus();
					using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
					{
						AssertNull("Should not show form if currency is cleared", lastShownForm);
					}

					unitFindBox.Focus();
					unitFindBox.CodeBox.Text = "NZD";
					anotherTextBox.Focus();
					using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
					{
						AssertType(typeof(UpdateOpportunityDateForExchangeRateForm), lastShownForm);

						lastShownForm.Close();

						AssertEquals("Should not revert to previous currency if not escaped", "NZD", opportunity.P8_RX_NKEstimatedValueCurrency);
					}

					//should not show form during a transaction
					opportunity.P8_RX_NKEstimatedValueCurrency = "";
					Factory.Save();
					Factory.Saving += (factory) =>
					{
						AssertEquals(true, factory.IsInTransaction);
						opportunity.P8_RX_NKEstimatedValueCurrency = "AUD";
					};
					ZFormModaliser.LastFormShownForTest = null;
					AssertEquals("", opportunity.P8_RX_NKEstimatedValueCurrency);
					Factory.Save();
					AssertEquals("AUD", opportunity.P8_RX_NKEstimatedValueCurrency);
					AssertNull(ZFormModaliser.LastFormShownForTest);
				}
			}

			[RequiresSTA]
			public void TestP8_Status_ChangeResultingInMakingOpportunityNonEffective_WithApprovedAgreements()
			{
				OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OpportunityCommissionChangeUtils.GetOpportunityStatusCollection());

				var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
				opportunity.P8_Status = "EFF";
				var approvedAgreement = opportunity.ApprovedCommissionAgreements.AddNew();
				approvedAgreement.FillWithValidTestData();
				var unapprovedAgreement = opportunity.CommissionAgreements.AddNew();
				unapprovedAgreement.FillWithValidTestData();
				Factory.Save();

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					opportunity.P8_Status = "NON";

					AssertEquals(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals(string.Format(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaptionText, "EFF", "NON"), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should have reverted to previous status", "EFF", opportunity.P8_Status);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					opportunity.P8_Status = "NON";

					AssertEquals(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals(string.Format(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaptionText, "EFF", "NON"), UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("NON", opportunity.P8_Status);
					AssertEquals(true, approvedAgreement.HasDraft);
					AssertEquals(true, approvedAgreement.Draft.IsReversed);
					AssertEquals(true, unapprovedAgreement.IsReversed);

					var logs = approvedAgreement.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.Events.StatusChangeCode).Select(x => x.SL_Reference).ToArray();
					AssertArrayEqualsByElements(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeLogs, logs);

					logs = unapprovedAgreement.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.Events.StatusChangeCode).Select(x => x.SL_Reference).ToArray();
					AssertArrayEqualsByElements(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeLogs, logs);
				}
			}

			[RequiresSTA]
			public void TestP8_Status_ChangeResultingInMakingOpportunityNonEffective_WithOnlyFirstDraftAgreements()
			{
				OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OpportunityCommissionChangeUtils.GetOpportunityStatusCollection());

				var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
				opportunity.P8_Status = "EFF";
				var unapprovedAgreement = opportunity.CommissionAgreements.AddNew();
				unapprovedAgreement.FillWithValidTestData();
				Factory.Save();

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();

					opportunity.P8_Status = "NON";

					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals(false, unapprovedAgreement.IsReversed);
				}
			}

			[RequiresSTA]
			public void TestP8_Status_SetTradeStatus()
			{
				var forwardingProduct = Factory.LoadTop1<IOrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "SHP"));

				OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetOpportunityStatusCollection());

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
				opportunity.P8_OH = org.PK;
				opportunity.P8_Status = "AAA";

				var tradeLane1 = org.SalesCollection.AddNew();
				tradeLane1.OW_MP_Product = forwardingProduct.Identifier;
				var detail1 = tradeLane1.TradeDetails.AddNew();

				var tradeLane2 = org.SalesCollection.AddNew();
				tradeLane2.OW_MP_Product = forwardingProduct.Identifier;
				var detail2 = tradeLane2.TradeDetails.AddNew();

				opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
				opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);
				opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail1);
				opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail2);

				Factory.Save();

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					opportunity.P8_Status = "CCC";

					using (var lastShownForm = ZFormModaliser.LastFormShownDialogForTest)
					{
						AssertEquals("", lastShownForm.Text);
						lastShownForm.Close();
					}
					ZFormModaliser.LastFormShownDialogForTest = null;
					AssertEquals("CCC", opportunity.P8_Status);
				}
			}

			[RequiresSTA]
			public void TestP8_Status_SetTradeStatusCancel()
			{
				var forwardingProduct = Factory.LoadTop1<IOrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "SHP"));

				OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetOpportunityStatusCollection());

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
				opportunity.P8_OH = org.PK;
				opportunity.P8_Status = "AAA";

				var tradeLane1 = org.SalesCollection.AddNew();
				tradeLane1.OW_MP_Product = forwardingProduct.Identifier;
				var detail1 = tradeLane1.TradeDetails.AddNew();

				var tradeLane2 = org.SalesCollection.AddNew();
				tradeLane2.OW_MP_Product = forwardingProduct.Identifier;
				var detail2 = tradeLane2.TradeDetails.AddNew();

				opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
				opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);
				opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail1);
				opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail2);

				Factory.Save();

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					opportunity.P8_Status = "CCC";

					using (var lastShownForm = ZFormModaliser.LastFormShownDialogForTest)
					{
						AssertEquals("", lastShownForm.Text);
						lastShownForm.Close();
					}
					ZFormModaliser.LastFormShownDialogForTest = null;
					AssertEquals("AAA", opportunity.P8_Status);
				}
			}

			[RequiresSTA]
			public void TestP8_Status_NotInDatabase()
			{
				var forwardingProduct = Factory.LoadTop1<IOrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "SHP"));

				OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetOpportunityStatusCollection());

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
				opportunity.P8_OH = org.PK;

				var tradeLane1 = org.SalesCollection.AddNew();
				tradeLane1.OW_MP_Product = forwardingProduct.Identifier;
				var detail1 = tradeLane1.TradeDetails.AddNew();

				var tradeLane2 = org.SalesCollection.AddNew();
				tradeLane2.OW_MP_Product = forwardingProduct.Identifier;
				var detail2 = tradeLane2.TradeDetails.AddNew();

				opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
				opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);
				opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail1);
				opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail2);

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();
					AssertEquals(false, opportunity.IsInDatabase);
					AssertEquals("", opportunity.P8_Status);

					var tradeDetailCommitmentUpdaterGUIManager = new TradeDetailCommitmentUpdaterGUIManagerForTest();
					using (ObjectFactory.Substitute<ITradeDetailCommitmentUpdaterGUIManager>(tradeDetailCommitmentUpdaterGUIManager))
					{
						opportunity.P8_Status = "BBB";
						AssertEquals(tradeDetailCommitmentUpdaterGUIManager.OrgOpportunities.OfType<OrgOpportunity>().Single().PK, opportunity.PK);
					}
				}
			}

			[RequiresSTA]
			public void TestCreateCommunicationOnOpportunityContactCall()
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_Phone = "02 12345678";
				var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
				opportunity.P8_OC = contact.PK;

				var phoneDiallerForTest = new TestPhoneDialler();
				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					var contactPhoneDiallerUserControl = form.FindSingle<ContactPhoneDiallerUserControl>("ContactPhoneDiallerUserControl");
					contactPhoneDiallerUserControl.PhoneDiallerOverrideForTest = phoneDiallerForTest;
					form.Show();

					OrganisationsDataRegistry.Instance.CreateCommunicationOnOpportunityContactCall.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					contactPhoneDiallerUserControl.CallButton.PerformClick();
					AssertNull(contactPhoneDiallerUserControl.LastCommunicationControllerForTesting);

					OrganisationsDataRegistry.Instance.CreateCommunicationOnOpportunityContactCall.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					contactPhoneDiallerUserControl.CallButton.PerformClick();
					AssertNotNull(contactPhoneDiallerUserControl.LastCommunicationControllerForTesting.LastShownForm);
					using (var communicationForm = (ZForm)contactPhoneDiallerUserControl.LastCommunicationControllerForTesting.LastShownForm)
					{
						var communication = (OrgSalesCall)communicationForm.BusinessEntity;
						AssertEquals(contact.PK, communication.OQ_OC);
					}
				}
			}

			[RequiresSTA]
			public void TestSourceDetails()
			{
				var collection = new CodeDescriptionBoolRelatedItemCollection()
				{
					{ "MRK", (NoResString)"Marketing Campaign", true, "CL2" },
					{ "MRB", (NoResString)"Old Marketing Campaign", false, "CL2" }
				};

				OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				var header = Factory.NewWithValidTestData<OrgHeader>();
				var opportunity = header.SalesOpportunities.AddNew();
				Factory.Save();
				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();
					Application.DoEvents();
					var sourceDetailsDropEdit = form.FindSingle<ZDropEdit>("SourceDetailsDropEdit");
					var sourceDetailsTextBox = form.FindSingle<ZTextBox>("SourceDetailsTextBox");
					Assert("drop down should not be visible by default", !sourceDetailsDropEdit.Visible);
					Assert("text box should be visible by default", sourceDetailsTextBox.Visible);

					form.DetailsControl.BusinessEntity_Exposed.P8_Source = "MRK";
					Application.DoEvents();
					Assert("drop down should be visible if reg item found", sourceDetailsDropEdit.Visible);
					Assert("text box should not be visible if reg item found", !sourceDetailsTextBox.Visible);

					form.DetailsControl.BusinessEntity_Exposed.P8_Source = "";
					Application.DoEvents();
					Assert("drop down should not be visible if no reg item found", !sourceDetailsDropEdit.Visible);
					Assert("text box should be visible if no reg item found", sourceDetailsTextBox.Visible);
				}
			}

			[RequiresSTA]
			public void TestDisableCloseCertaintyTrackbar()
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var opportunity = header.SalesOpportunities.AddNew();
				Factory.Save();
				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.DisplayMode = ODisplayMode.ReadOnly;
					form.Show();
					var closeCertaintyTrackBar = form.FindSingle<KTrackBar>("CloseCertaintyTrackBar");
					AssertEquals(false, closeCertaintyTrackBar.Enabled);
				}

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					var closeCertaintyTrackBar = form.FindSingle<KTrackBar>("CloseCertaintyTrackBar");
					AssertEquals(true, closeCertaintyTrackBar.Enabled);
				}
			}

			[ExpectNoExceptions]
			[RequiresSTA]
			public void TestFormLoadWhenCloseCertaintyNotInAcceptedRange()
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var opportunity = header.SalesOpportunities.AddNew();
				opportunity.P8_CloseCertainty = 150;
				Factory.Save();

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();
					var closeCertaintyTrackBar = form.FindSingle<KTrackBar>("CloseCertaintyTrackBar");
					AssertEquals(0, closeCertaintyTrackBar.Value);
				}
			}

			[RequiresSTA]
			public void TestCreatedFromInquiryLabel()
			{
				var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var opportunity = header.SalesOpportunities.AddNew();
				opportunity.P8_O1_Enquiry = inquiry.PK;
				Factory.Save();

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();

					Env.Security.InquiryManagerEdit.IsAllowed = false;
					Env.Security.InquiryManagerView.IsAllowed = false;

					UnitTestUserNotification.Instance.AddOKAnswer();
					form.DetailsControl.CreatedFromInquiryLabel_Click_Exposed(null, EventArgs.Empty);
					AssertStartsWith("Should show error", "You do not have the appropriate security rights to run this function", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull(form.DetailsControl.InquiryController_Exposed.LastShownForm);

					Env.Security.InquiryManagerView.IsAllowed = true;
					form.DetailsControl.CreatedFromInquiryLabel_Click_Exposed(null, EventArgs.Empty);
					var createdFromInquiryLabel = form.FindSingle<ZLabel>("CreatedFromInquiryLabel");
					AssertEquals("View Inquiry", ((ZForm)form.DetailsControl.InquiryController_Exposed.LastShownForm).Text);
					((ZForm)form.DetailsControl.InquiryController_Exposed.LastShownForm).Close();

					Env.Security.InquiryManagerEdit.IsAllowed = true;
					form.DetailsControl.CreatedFromInquiryLabel_Click_Exposed(null, EventArgs.Empty);
					AssertEquals("Edit Inquiry", ((ZForm)form.DetailsControl.InquiryController_Exposed.LastShownForm).Text);
					((ZForm)form.DetailsControl.InquiryController_Exposed.LastShownForm).Close();

					if (form.DetailsControl.InquiryController_Exposed.LastShownForm != null)
					{
						((ZForm)form.DetailsControl.InquiryController_Exposed.LastShownForm).Dispose();
					}
				}
			}

			[TestDate(2020, 2, 27, 14, 25, 10)]
			[RequiresSTA]
			public void TestUpdateDeliveryStatusControls()
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				var contactNonDeliveryReport = Factory.NewWithValidTestData<OrgContact>();
				contactNonDeliveryReport.OC_ContactName = "Sm";
				contactNonDeliveryReport.OC_Email = "x@x.com";
				contactNonDeliveryReport.IsNDR = true;
				contactNonDeliveryReport.EmailAddress.GI_DeliveryStatus = "NDR";
				contactNonDeliveryReport.EmailAddress.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow;

				var header = Factory.NewWithValidTestData<OrgHeader>();
				var opportunity = header.SalesOpportunities.AddNew();

				Factory.Save();

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();

					var deliveryStatusLabel = form.FindSingle<ZLabel>("DeliveryStatusLabel");
					var deliveryStatusButton = form.FindSingle<ZButton>("DeliveryStatusButton");
					var opportunityTabControl = form.FindSingle<ZTemplateTabControl>("OpportunityTabControl");
					var height = opportunityTabControl.Height;

					AssertEquals("Delivery status label should be invisible", false, deliveryStatusLabel.Visible);
					AssertEquals("Delivery status button should be invisible", false, deliveryStatusButton.Visible);

					opportunity.P8_OC = contactNonDeliveryReport.PK;

					AssertEquals("Contact Main Email address is invalid", "Non-Delivery Receipt: 28-Feb-20 00:25", deliveryStatusLabel.Text);
					AssertEquals("Label ForeColor should be warning color", ZArchitecture.GUI.Notifications.NotificationColorScheme.GetFontColor(NotificationType.Warning), deliveryStatusLabel.ForeColor);
					AssertEquals("Delivery status label should be visible", true, deliveryStatusLabel.Visible);
					AssertEquals("Delivery status button should be visible", true, deliveryStatusButton.Visible);
					AssertImageEquals("Should be warning icon", Icons.GetIcon(IconTypes.Warning).ToBitmap(), deliveryStatusButton.BackgroundImage);
					AssertEquals("OpportunityTabControl should be adjusted narrower", height - ControlDpiScalingHelper.ScaleToCurrentDpiY(20), opportunityTabControl.Height);

					opportunity.P8_OC = contact.PK;

					AssertEquals("Delivery status label should be invisible", false, deliveryStatusLabel.Visible);
					AssertEquals("Delivery status button should be invisible", false, deliveryStatusButton.Visible);
					AssertEquals("OpportunityTabControl should be adjusted back to normal", height, opportunityTabControl.Height);
				}
			}

			[TestDate(2023, 5, 15, 12, 51, 00)]
			[RequiresSTA]
			public void TestP8_OH_P8_OC_ValueChanged()
			{
				var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();

				var orgContact = Factory.NewWithValidTestData<OrgContact>();
				var orgContactNonDeliveryReport = Factory.NewWithValidTestData<OrgContact>();
				var orgContactOrg2 = Factory.NewWithValidTestData<OrgContact>();

				var emailAddress = Factory.NewWithValidTestData<GlbEmailAddress>();
				emailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
				emailAddress.GI_DeliveryReportTimeUtc = ZDateTime.Today;

				orgContactNonDeliveryReport.OC_Email = emailAddress.GI_EmailAddress;
				org.Contacts.Add(orgContact);
				org.Contacts.Add(orgContactNonDeliveryReport);

				opportunity.P8_OH = org.PK;
				opportunity.P8_OC = orgContactNonDeliveryReport.PK;

				Factory.Save();

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();

					var deliveryStatusLabel = form.FindSingle<ZLabel>("DeliveryStatusLabel");
					var deliveryStatusButton = form.FindSingle<ZButton>("DeliveryStatusButton");

					AssertEquals("Non-Delivery Receipt: 15-May-23 10:00", deliveryStatusLabel.Text);
					Assert(deliveryStatusLabel.Visible);
					Assert(deliveryStatusButton.Visible);

					opportunity.P8_OH = org2.PK;
					opportunity.P8_OC = orgContactOrg2.PK;

					AssertEquals(string.Empty, deliveryStatusLabel.Text);
					Assert(!deliveryStatusLabel.Visible);
					Assert(!deliveryStatusButton.Visible);

					opportunity.P8_OH = org.PK;
					opportunity.P8_OC = orgContactNonDeliveryReport.PK;

					AssertEquals("Non-Delivery Receipt: 15-May-23 10:00", deliveryStatusLabel.Text);
					Assert(deliveryStatusLabel.Visible);
					Assert(deliveryStatusButton.Visible);

					opportunity.P8_OC = orgContact.PK;

					AssertEquals(string.Empty, deliveryStatusLabel.Text);
					Assert(!deliveryStatusLabel.Visible);
					Assert(!deliveryStatusButton.Visible);
				}
			}

			[RequiresSTA]
			public void TestCustomLabels()
			{
				using (var form = new OpportunityManagementDetailsControlForm(Factory.NewWithValidTestData<OrgHeader>().SalesOpportunities.AddNew()))
				{
					form.Show();
					AssertEquals(form.FindSingle<Control>("RentalMultiplierCalcEdit").GetExtension<HintExtension>().Description, OrganisationsDataRegistry.Instance.PotentialLabel.Value);
					AssertEquals(form.FindSingle<Control>("DiscountCalcEdit").GetExtension<HintExtension>().Description, OrganisationsDataRegistry.Instance.CurrentLabel.Value);
				}
			}

			[RequiresSTA]
			public void TestInputBoxMaxLength()
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var opportunity = header.SalesOpportunities.AddNew();
				opportunity.P8_CloseCertainty = 150;
				Factory.Save();

				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					form.Show();
					var rentalMultiplierCalcEdit = form.FindSingle<ZCalcEdit>("RentalMultiplierCalcEdit");
					var discountCalcEdit = form.FindSingle<ZCalcEdit>("DiscountCalcEdit");
					AssertEquals(999999999999m, rentalMultiplierCalcEdit.MaxValue);
					AssertEquals(999999999999m, discountCalcEdit.MaxValue);
				}
			}

			[RequiresSTA]
			public void TestCommissionAgreementTabLicenceCheckpoint()
			{
				var opportunity = Factory.New<OrgOpportunity>();
				using (var form = new OpportunityManagementDetailsControlForm(opportunity))
				{
					var commissionAgreementTabPage = form.FindSingle<ZTabPage>("CommissionAgreementTabPage");
					AssertEquals(Env.Licence.CommissionManager, commissionAgreementTabPage.LicenceCheckpoint);
				}
			}

			protected override Form GetFormToBashCore()
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var opportunity = header.SalesOpportunities.AddNew();
				Factory.Save();

				var form = new OpportunityManagementDetailsControlForm(opportunity);
				form.ControllerID = ControllerIDs.Opportunity;
				return form;
			}

			OpportunityStatusCollection GetOpportunityStatusCollection()
			{
				return new OpportunityStatusCollection
			{
				{ "AAA", (NoResString)"Desc A", false, true, true, OpportunityTradeStatus.Codes.Active },
				{ "BBB", (NoResString)"Desc B", false, true, true, OpportunityTradeStatus.Codes.Unsuccessful },
				{ "CCC", (NoResString)"Desc C", false, true, true, OpportunityTradeStatus.Codes.Successful }
			};
			}

			class OpportunityManagementDetailsControlForm : ZForm
			{
				public OpportunityManagementDetailsControlForm(OrgOpportunity opportunity)
					: base(opportunity)
				{
				}

				protected override void InitializeComponent()
				{
					base.InitializeComponent();

					Size = ControlDpiScalingHelper.NewScaledSize(1200, 768, true);

					Controls.Add(DetailsControl);
					BindingSource.SetBindingMember(DetailsControl, ".");
					CaptionRenderingEnabled = true;
				}

				internal OpportunityManagementDetailsControlForTest DetailsControl = new OpportunityManagementDetailsControlForTest();
			}

			class TradeDetailCommitmentUpdaterGUIManagerForTest : ITradeDetailCommitmentUpdaterGUIManager
			{
				public ZDialogResult ShowForm(IOrgOpportunity opportunity, EventArgs e)
				{
					OrgOpportunities.Add(opportunity);
					return ZDialogResult.OK;
				}

				public List<IOrgOpportunity> OrgOpportunities = new List<IOrgOpportunity>();
			}

			class OpportunityManagementDetailsControlForTest : OpportunityManagementDetailsControl
			{
				public OrgOpportunity BusinessEntity_Exposed => BusinessEntity;
				public ZController InquiryController_Exposed => InquiryController;

				public void CreatedFromInquiryLabel_Click_Exposed(object sender, EventArgs e)
				{
					CreatedFromInquiryLabel_Click(sender, e);
				}
			}
		}
	}
}
