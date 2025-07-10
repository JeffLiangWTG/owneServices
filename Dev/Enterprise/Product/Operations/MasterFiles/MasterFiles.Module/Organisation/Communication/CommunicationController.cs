using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class CommunicationController : ZController, ICommunicationController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public CommunicationController()
		{
		}

		#region Standard Controller Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Communication; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Communication; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgSalesCall); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CommunicationForm((OrgSalesCall)businessEntity);
		}

		#endregion

		#region BusinessEntity

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var communication = (OrgSalesCall)base.GetNewBusinessEntityInLocalFactory();

			if (CreateNewWithParentFormBizObjDefaults)
			{
				ApplyNewRelatedCommunicationDefaults(communication);
			}

			return communication;
		}

		#endregion

		#region Showing Forms

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			if (!RunAdditionalShowFormChecks(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowViewForm(sourceEntity);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (!RunAdditionalShowFormChecks(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowEditForm(sourceEntity);
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (!RunAdditionalShowFormChecks(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowDeleteForm(sourceEntity);
		}

		static bool RunAdditionalShowFormChecks(BusinessObject sourceEntity)
		{
			var communication = sourceEntity as OrgSalesCall;
			if (IsLinkedToRestrictedOpportunity(communication))
			{
				return false;
			}
			if (communication != null && !Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.IsAllowed)
			{
				return CheckLoginStaffMatches(communication);
			}

			return true;
		}

		static bool CheckLoginStaffMatches(OrgSalesCall communication)
		{
			if (communication.OQ_GS_NKSalesRep != GlbStaff.CurrentUser.GS_Code && !communication.AdditionalAttendeesStaff.Cast<OrgSalesCallAdditionalAttendee>().Any(staff => staff.O6_AttendeeID == GlbStaff.CurrentUser.PK))
			{
				Globals.Message.ShowError(
						ResString.GetMultilingualString("86199003-752e-4320-91a1-eadc1704b39e", @"You do not have the appropriate security rights to view {0}. You are only allowed to view communication where you are the staff coordinator or an attendee.

If you require access to this function please ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{1}",
								communication.HumanReadableName,
								Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.DisplayTextPathToSecurityRight),
						ResString.GetMultilingualString("ae126ead-6226-4d62-949a-af825f38a035", "Access Denied: {0}", Env.Security.CommunicationManagerViewWithoutBeingRelatedStaff.DisplayText)
					);

				return false;
			}

			return true;
		}

		static bool IsLinkedToRestrictedOpportunity(OrgSalesCall communication)
		{
			var link = communication?.RelatedActivityLinkCollection.Cast<RelatedActivityLink>().FirstOrDefault(a => a.ToActivityTableCode == CrmOpportunitySchema.Constants.Prefix);
			if (link?.ToActivity is ICrmOpportunity opp && opp.IsRestrictedForCurrentUser())
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("a7f8b8a6-df2c-4e1f-a321-f93d6baa0048", "You do not have the appropriate security rights to view this record. It is linked to a record to which you do not have access."));
				return true;
			}
			return false;
		}

		#endregion

		#region ICommunicationController

		public bool CreateNewWithParentFormBizObjDefaults
		{
			get;
			set;
		}

		#endregion

		#region ApplyNewRelatedCommunicationDefaults

		void ApplyNewRelatedCommunicationDefaults(OrgSalesCall communication)
		{
			var activeForm = GetActiveForm();
			if (activeForm != null)
			{
				var activeFormEntity = activeForm.BusinessEntity;

				var newRelatedCommunicationArgs = new NewRelatedCommunicationArgs(communication, AsRelatableActivities(activeFormEntity));
				var formWithCustomDefaults = activeForm as IHasCustomNewRelatedCommunicationHandling;
				if (formWithCustomDefaults != null)
				{
					formWithCustomDefaults.OnShowingNewFormForRelated(newRelatedCommunicationArgs);
				}

				ApplyNewRelatedCommunicationDefaults(activeForm, communication);

				foreach (var activityToMakeParent in newRelatedCommunicationArgs.ActivitiesToMakeParent.Where(activity => !communication.RelatedParentActivityPivotCollection.Activities.Any(existingParent => existingParent.PK == activity.PK)))
				{
					communication.RelatedParentActivityPivotCollection.AddActivity(activityToMakeParent);
				}
			}
		}

#if DEBUG
		public System.Windows.Forms.Form ActiveFormOverrideForTesting { get; set; }
#endif

		ZForm GetActiveForm()
		{
			var activeForm = ZForm.ActiveForm as ZForm;
#if DEBUG
			if (ActiveFormOverrideForTesting != null)
			{
				activeForm = ActiveFormOverrideForTesting as ZForm;
			}
#endif

			if (activeForm != null)
			{
				if (activeForm is RelatedCommunicationForm || activeForm is SalesRelationPopupForm)
				{
					var activeFormOwner = activeForm.Owner as ZForm;
					if (activeFormOwner != null)
					{
						activeForm = activeFormOwner;
					}
				}
			}

			return activeForm;
		}

		void ApplyNewRelatedCommunicationDefaults(ZForm activeForm, OrgSalesCall newCommunication)
		{
			var businessEntity = activeForm.BusinessEntity;
			if (businessEntity != null)
			{
				var controllerId = activeForm.ControllerID;
				if (controllerId != null)
				{
					Action<OrgSalesCall, IBusiness> applyDefaultsAction;
					if (applyRelatedCommunicationDefaultActions.TryGetValue(controllerId, out applyDefaultsAction))
					{
						applyDefaultsAction(newCommunication, businessEntity);
					}
				}
			}
		}

		static IEnumerable<IRelatableActivity> AsRelatableActivities(IBusiness activeFormEntity)
		{
			var activeFormBizObj = activeFormEntity as BusinessObject;

			if (activeFormBizObj == null || !activeFormBizObj.IsInDatabase)
			{
				yield break;
			}

			var relatableActivity = activeFormBizObj as IRelatableActivity;

			if (relatableActivity != null)
			{
				yield return relatableActivity;
			}
		}

		readonly IDictionary<ControllerID, Action<OrgSalesCall, IBusiness>> applyRelatedCommunicationDefaultActions = new Dictionary<ControllerID, Action<OrgSalesCall, IBusiness>>
		{
			{ ControllerIDs.Communication,      ApplyCommunicationDefaults },
			{ ControllerIDs.Organisation,       ApplyOrganisationOrClientIntelligenceDefaults },
			{ ControllerIDs.ClientIntelligence, ApplyOrganisationOrClientIntelligenceDefaults },
			{ ControllerIDs.Opportunity,        ApplyOpportunityDefaults },
			{ ControllerIDs.SalesEnquiry,       ApplySalesEnquiryDefaults },
			{ ControllerIDs.ClientRates,        ApplyClientRatesOrQuotationsDefaults },
			{ ControllerIDs.Quotations,         ApplyClientRatesOrQuotationsDefaults },
			{ ControllerIDs.QuotedBookings,     ApplyQuotedBookingsDefaults },
			{ ControllerIDs.Project,            ApplyProjectDefaults }
		};

		static void ApplyCommunicationDefaults(OrgSalesCall communication, IBusiness businessEntity)
		{
			var previousCommunication = businessEntity as OrgSalesCall;
			if (previousCommunication != null)
			{
				communication.OQ_OH = previousCommunication.OQ_OH;
			}
		}

		static void ApplyOrganisationOrClientIntelligenceDefaults(OrgSalesCall communication, IBusiness businessEntity)
		{
			var org = businessEntity as OrgHeader;
			if (org != null)
			{
				communication.OQ_OH = org.PK;
			}
		}

		static void ApplyOpportunityDefaults(OrgSalesCall communication, IBusiness businessEntity)
		{
			var opportunity = businessEntity as OrgOpportunity;
			if (opportunity != null)
			{
				communication.OQ_OH = opportunity.P8_OH;
			}
		}

		static void ApplyProjectDefaults(OrgSalesCall communication, IBusiness businessEntity)
		{
			var project = businessEntity as IProject;
			if (project != null)
			{
				communication.OQ_OH = project.ClientOrganisationPK;
				communication.OQ_OC = project.WKP_OC_Contact;
			}
		}

		static void ApplySalesEnquiryDefaults(OrgSalesCall communication, IBusiness businessEntity)
		{
			var inquiry = businessEntity as SalesEnquiry;
			if (inquiry != null)
			{
				communication.OQ_OH = inquiry.O1_OH_ConvertedToQualifiedLead;
				if (inquiry.IsInDatabase && !communication.OQ_OH.IsValid && inquiry.O1_OH_ConvertedToQualifiedLeadInfo.OriginalValue.IsEmpty)
				{
					communication.LinkedInquiry = inquiry;
				}
			}
		}

		static void ApplyClientRatesOrQuotationsDefaults(OrgSalesCall communication, IBusiness businessEntity)
		{
			var rating = businessEntity as IRatingHeader;
			if (rating != null)
			{
				communication.OQ_OH = rating.TH_OH;
			}
		}

		static void ApplyQuotedBookingsDefaults(OrgSalesCall communication, IBusiness businessEntity)
		{
			var quotedBooking = businessEntity as IQuotedBooking;
			if (quotedBooking != null)
			{
				communication.OQ_OH = quotedBooking.ClientPK;
			}
		}

		#endregion

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CommunicationManagerNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CommunicationManagerView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CommunicationManagerEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CommunicationManagerDelete; }
		}

		#endregion

		#region CRM Security

		readonly CommunicationCRMSecurityProvider SecurityProvider = new CommunicationCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgSalesCall, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgSalesCall, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgSalesCall, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion

	}
}
