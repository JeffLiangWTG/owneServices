using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CommunicationController))]
	sealed class CommunicationControllerTest : ZControllerBasherTest
	{
		#region Showing Forms

		#region TestShowForm_ChecksViewWithoutBeingRelatedStaffCheckpoint

		public void TestShowViewForm_ChecksViewWithoutBeingRelatedStaffCheckpoint()
		{
			TestShowForm_ChecksViewWithoutBeingRelatedStaffCheckpoint((controller, bizObj) => controller.ShowViewForm(bizObj));
		}

		public void TestShowEditForm_ChecksViewWithoutBeingRelatedStaffCheckpoint()
		{
			TestShowForm_ChecksViewWithoutBeingRelatedStaffCheckpoint((controller, bizObj) => controller.ShowEditForm(bizObj));
		}

		public void TestShowDeleteForm_ChecksViewWithoutBeingRelatedStaffCheckpoint()
		{
			TestShowForm_ChecksViewWithoutBeingRelatedStaffCheckpoint((controller, bizObj) => controller.ShowDeleteForm(bizObj));
		}

		public void TestShowCopyForm_ChecksViewWithoutBeingRelatedStaffCheckpoint()
		{
			var controller = new CommunicationController();
			AssertExceptionThrown("Can not copy communication, don't need to check for this checkpoint. If this changes at a later point. Update Controller to check for this.", typeof(ModuleTemplateCopyNotSupportedException), () =>
			{
				controller.ShowTemplateCopyForm(Factory.New<OrgSalesCall>());
			});
		}

		void TestShowForm_ChecksViewWithoutBeingRelatedStaffCheckpoint(Action<CommunicationController, BusinessObject> showFormDelegate)
		{
			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();

			var currentUserAsSalesRep = Factory.NewWithValidTestData<OrgSalesCall>();
			currentUserAsSalesRep.OQ_GS_NKSalesRep = GlbStaff.CurrentUser.GS_Code;

			var differentStaffSalesRep = Factory.NewWithValidTestData<OrgSalesCall>();
			differentStaffSalesRep.OQ_GS_NKSalesRep = otherStaff.GS_Code;

			var currentUserAsAdditionalStaff = Factory.NewWithValidTestData<OrgSalesCall>();
			currentUserAsAdditionalStaff.OQ_GS_NKSalesRep = otherStaff.GS_Code;
			var additionalStaff = currentUserAsAdditionalStaff.AdditionalAttendeesStaff.AddNew();
			additionalStaff.O6_AttendeeTableCode = GlbStaffSchema.Constants.Prefix;
			additionalStaff.O6_AttendeeID = GlbStaff.CurrentUser.PK;

			Factory.Save();

			Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.IsAllowed = true;
			AssertShowFormAllowed(showFormDelegate, currentUserAsSalesRep, true, null, null);
			AssertShowFormAllowed(showFormDelegate, currentUserAsAdditionalStaff, true, null, null);
			AssertShowFormAllowed(showFormDelegate, differentStaffSalesRep, true, null, null);

			Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.IsAllowed = false;
			AssertShowFormAllowed(showFormDelegate, currentUserAsSalesRep, true, null, null);
			AssertShowFormAllowed(showFormDelegate, currentUserAsAdditionalStaff, true, null, null);
			AssertShowFormAllowed(showFormDelegate, differentStaffSalesRep,
				false,
				@"You do not have the appropriate security rights to view Communication (CM00001001). You are only allowed to view communication where you are the staff coordinator or an attendee.

If you require access to this function please ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.DisplayTextPathToSecurityRight,
				"Access Denied: " + Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.DisplayText);
		}

		#endregion

		#region TestShowForm_CheckRestrictedOpportunity

		public void TestShowViewForm_CheckRestrictedOpportunity()
		{
			TestShowForm_CheckRestrictedOpportunity((controller, bizObj) => controller.ShowViewForm(bizObj));
		}

		public void TestShowEditForm_CheckRestrictedOpportunity()
		{
			TestShowForm_CheckRestrictedOpportunity((controller, bizObj) => controller.ShowEditForm(bizObj));
		}

		public void TestShowDeleteForm_CheckRestrictedOpportunity()
		{
			TestShowForm_CheckRestrictedOpportunity((controller, bizObj) => controller.ShowDeleteForm(bizObj));
		}

		void TestShowForm_CheckRestrictedOpportunity(Action<CommunicationController, BusinessObject> showFormDelegate)
		{
			var restrictedUser = Factory.New<GlbStaff>();
			restrictedUser.GS_Code = "UR1";
			restrictedUser.GS_LoginName = "User1";

			var authorizedUser = Factory.New<GlbStaff>();
			authorizedUser.GS_Code = "UR2";
			authorizedUser.GS_LoginName = "User2";

			var opportunity = Factory.NewWithValidTestData(ObjectFactory.GetType<ICrmOpportunity>());
			opportunity[CrmOpportunitySchema.COP_IsRestricted] = true;

			var restriction = Factory.New<EntityStaffRestriction>();
			restriction.ESR_GS_NKStaff = authorizedUser.GS_Code;
			restriction.ESR_ParentTableCode = CrmOpportunitySchema.Constants.Prefix;
			restriction.ESR_ParentID = opportunity.PK;

			var communication = Factory.New<OrgSalesCall>();
			var pivot = communication.RelatedChildActivityPivotCollection.AddNew();
			pivot.RAP_ChildActivityID = opportunity.PK;
			pivot.RAP_ChildActivityTableCode = CrmOpportunitySchema.Constants.Prefix;

			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(authorizedUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				Env.Security.CommunicationManagerCRMSecurity.DisableCRMSecurityForTesting(true);
				Env.Security.CommunicationManagerView.IsAllowed = true;
				Env.Security.CommunicationManagerDelete.IsAllowed = true;
				Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.IsAllowed = true;
				AssertShowFormAllowed(showFormDelegate, communication, expectedIsAllowed: true, expectedLastMessage: null, expectedLastCaption: null);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(restrictedUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				Env.Security.CommunicationManagerCRMSecurity.DisableCRMSecurityForTesting(true);
				Env.Security.CommunicationManagerView.IsAllowed = true;
				Env.Security.CommunicationManagerDelete.IsAllowed = true;
				Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.IsAllowed = true;
				AssertShowFormAllowed(showFormDelegate, communication, false, (NoResString)"You do not have the appropriate security rights to view this record. It is linked to a record to which you do not have access.", null);
			}
		}

		#endregion

		void AssertShowFormAllowed(Action<CommunicationController, BusinessObject> showFormDelegate, BusinessObject sourceEntity, bool expectedIsAllowed, string expectedLastMessage, string expectedLastCaption)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var controller = new CommunicationController();
			showFormDelegate(controller, sourceEntity);

			CombineAssertions(() =>
			{
				using (var lastShownForm = controller.LastShownForm)
				{
					if (expectedIsAllowed)
					{
						AssertNotNull("LastShownForm", lastShownForm);
					}
					else
					{
						AssertNull("LastShownForm", lastShownForm);
					}
				}

				AssertMultilineASCIIEquals("LastMessage.Text", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertMultilineASCIIEquals("LastMessage.Caption", expectedLastCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
			});
		}

		#endregion

		#region ApplyNewRelatedCommunicationDefaults

		public void TestShowNewFormDefaults_Dummy()
		{
			using (var form = (ZForm)ZControllerFactory.Create(DummyControllerIDs.Dummy).ShowNewForm())
			{
				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					AssertEquals(ZGuid.Empty, communication.OQ_OH);
					AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), communication.RelatedParentActivityPivotCollection.Activities);
				}
			}
		}

		public void TestShowNewFormDefaults_Communication()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var previousCommunication = Factory.NewWithValidTestData<OrgSalesCall>();
			previousCommunication.OQ_OH = org.PK;
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.Communication).ShowEditForm(previousCommunication))
			{
				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					AssertEquals(org.PK, communication.OQ_OH);
					AssertCollectionContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == previousCommunication.PK);
				}
			}
		}

		public void TestShowNewFormDefaults_Organisation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.Organisation).ShowEditForm(org))
			{
				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					AssertEquals(org.PK, communication.OQ_OH);
					AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), communication.RelatedParentActivityPivotCollection.Activities);
				}
			}
		}

		public void TestShowNewFormDefaults_ClientIntelligence()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.ClientIntelligence).ShowEditForm(org))
			{
				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					AssertEquals(org.PK, communication.OQ_OH);
					AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), communication.RelatedParentActivityPivotCollection.Activities);
				}
			}
		}

		public void TestShowNewFormDefaults_Opportunity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.Opportunity).ShowEditForm(opportunity))
			{
				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					AssertEquals(org.PK, communication.OQ_OH);
					AssertCollectionContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == opportunity.PK);
				}
			}
		}

		public void TestShowNewFormDefaults_SalesEnquiry()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowEditForm(inquiry))
			{
				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					CombineAssertions("Should not link to inquiry if has client intelligence", () =>
					{
						AssertEquals(org.PK, communication.OQ_OH);
						AssertNull("LinkedInquiry", communication.LinkedInquiry);
						AssertCollectionContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == inquiry.PK);
					});
				}
			}

			inquiry.O1_OH_ConvertedToQualifiedLead = ZGuid.Empty;
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowEditForm(inquiry))
			{
				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					CombineAssertions("Should have linked to inquiry", () =>
					{
						AssertEquals(ZGuid.Empty, communication.OQ_OH);
						AssertNotNull("LinkedInquiry", communication.LinkedInquiry);
						AssertEquals("LinkedInquiry.PK", inquiry.PK, communication.LinkedInquiry.PK);
						AssertCollectionContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == inquiry.PK);
					});
				}
			}

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowNewForm())
			{
				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					CombineAssertions("Should not link to inquiry if inquiry not saved", () =>
					{
						AssertEquals(ZGuid.Empty, communication.OQ_OH);
						AssertNull("LinkedInquiry", communication.LinkedInquiry);
						AssertCollectionNotContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == inquiry.PK);
					});
				}
			}
		}

		public void TestShowNewFormDefaults_ClientRates()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.ClientRates).ShowNewForm())
			{
				var quotation = (IRatingHeader)form.BusinessEntity;
				quotation.TH_OH = org.PK;
				form.BusinessEntity.Factory.Save();

				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					AssertEquals(org.PK, communication.OQ_OH);
					AssertCollectionContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == ((BusinessObject)quotation).PK);
				}
			}
		}

		public void TestShowNewFormDefaults_Quotations()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.Quotations).ShowNewForm())
			{
				var quotation = (IRatingHeader)form.BusinessEntity;
				quotation.TH_OH = org.PK;
				form.BusinessEntity.Factory.Save();

				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					AssertEquals(org.PK, communication.OQ_OH);
					AssertCollectionContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == ((BusinessObject)quotation).PK);
				}
			}
		}

		public void TestShowNewFormDefaults_QuotedBookings()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.ClientPK = org.PK;
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.QuotedBookings).ShowEditForm((BusinessObject)quotedBooking))
			{
				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					AssertEquals(org.PK, communication.OQ_OH);
					AssertCollectionContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == ((BusinessObject)quotedBooking).PK);
				}
			}
		}

		public void TestShowNewFormDefaults_Cartage()
		{
			var cartage = (BusinessObject)Factory.New<ICommonCartage>();
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.Cartage).ShowEditForm(cartage))
			{
				form.Activate();

				using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = form }.ShowNewForm())
				{
					var communication = (OrgSalesCall)communicationForm.BusinessEntity;
					AssertCollectionContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == cartage.PK);
				}
			}
		}

		public void TestShowNewFormDefaults_FromRelatedCommunicationForm()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			Factory.Save();

			using (var parentForm = (ZForm)ZControllerFactory.Create(ControllerIDs.Opportunity).ShowEditForm(opportunity))
			{
				using (var relatedCommunicationForm = new RelatedCommunicationForm(opportunity.RelatedCommunicationCollection))
				{
					ZFormModaliser.Show(relatedCommunicationForm, parentForm);

					relatedCommunicationForm.Activate();

					using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = relatedCommunicationForm }.ShowNewForm())
					{
						var communication = (OrgSalesCall)communicationForm.BusinessEntity;
						AssertEquals(org.PK, communication.OQ_OH);
						AssertCollectionContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == opportunity.PK);
					}
				}
			}
		}

		public void TestShowNewFormDefaults_FromSalesRelationsForm()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			Factory.Save();

			using (var parentForm = (ZForm)ZControllerFactory.Create(ControllerIDs.Opportunity).ShowEditForm(opportunity))
			{
				using (var salesRelationForm = new SalesRelationPopupForm(opportunity.SalesRelationModel))
				{
					ZFormModaliser.Show(salesRelationForm, parentForm);

					salesRelationForm.Activate();

					using (var communicationForm = (ZForm)new CommunicationController() { CreateNewWithParentFormBizObjDefaults = true, ActiveFormOverrideForTesting = salesRelationForm }.ShowNewForm())
					{
						var communication = (OrgSalesCall)communicationForm.BusinessEntity;
						AssertEquals(org.PK, communication.OQ_OH);
						AssertCollectionContains(communication.RelatedParentActivityPivotCollection.Activities, activity => activity.PK == opportunity.PK);
					}
				}
			}
		}

		#endregion

		public void TestCRMSecurityCheckpoints()
		{
			var bizObj = Factory.NewWithValidTestData<OrgSalesCall>();
			bizObj.OQ_GS_NKSalesRep = "";
			CRMSecurityProviderTest<OrgSalesCall>.AssertController(new CommunicationController(), bizObj, Env.Security.CommunicationManagerCRMSecurity);
		}

		public void TestUrlsCanBeOpenedByAnyCompany()
		{
			Assert("Communication hyperlinks should not be restricted to the current company", !Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(OrgSalesCall);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Communication;
		}

		#endregion
	}
}
