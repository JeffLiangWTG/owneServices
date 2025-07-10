using System.Collections.Generic;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingCartage : CommonCartage,
		IWebDocumentsWithUploadSupport,
		IBizOChangesEmailNotification,
		IUpdatableMilestoneEventsProvider,
		IMilestonesProvider,
		ITrackingEventsProvider,
		IEventReferenceProvider
	{
		public TrackingCartage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public abstract new class Schema : CommonCartage.Schema
		{
			public const string LocalClientAddressDetailed = "LocalClientAddressDetailed";
		}

		#endregion

		public static TrackingCartage FromPKFilteredBySiteUser(BusinessObjectFactory factory, ZGuid pK, TrackingSiteUser siteUser)
		{
			TrackingCartage result = null;
			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				result = factory.Load<TrackingCartage>(pK);
			}
			return result;
		}

		#region Properties

		public ZString LocalClientAddressDetailed
		{
			get
			{
				ZString result = ZString.Empty;
				if (LocalClientAddress != null)
				{
					result = ((IOrgAddress)LocalClientAddress).Address;
				}
				return result;
			}
		}

		public ZPropertyInfo LocalClientAddressDetailedInfo
		{
			get { return GetZPropertyInfo(Schema.LocalClientAddressDetailed); }
		}

		public FilteredCartageLegsCollection FilteredCartageLegs
		{
			get
			{
				if (fFilteredCartageLegs == null)
				{
					fFilteredCartageLegs = new FilteredCartageLegsCollection(CartageLegs, LoggedInContact);
				}
				return fFilteredCartageLegs;
			}
		}
		FilteredCartageLegsCollection fFilteredCartageLegs;

		#region Test
#if DEBUG
		public void NullFilteredCartageLegsCollection()
		{
			fFilteredCartageLegs = null;
		}
#endif
		#endregion

		#endregion

		#region TrackingEvents

		public StmALogCollection TrackingEvents
		{
			get { return this.GetTrackingEvents(SiteUser); }
		}

		public bool CanViewTrackingEvents
		{
			get { return SiteUser?.CanViewEvents ?? false; }
		}

		TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance != null ? WebEnv.AppInstance.SiteUser as TrackingSiteUser : null; }
		}

		#endregion

		#region Milestones

		public void ReloadMilestones()
		{
			fMilestones = null;
		}

		public TrackingMilestoneCollection Milestones
		{
			get { return fMilestones ?? (fMilestones = new TrackingMilestoneCollection(this)); }
		}
		TrackingMilestoneCollection fMilestones;

		public TrackingMilestoneCollection EditableMilestones
		{
			get { return editableMilestones ?? (editableMilestones = new TrackingMilestoneCollection(this, true)); }
		}
		TrackingMilestoneCollection editableMilestones;

		#endregion

		#region IUpdatableMilestoneEventsProvider

		public List<string> UpdatableMilestoneEventCodes
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					return (new UpdateableMilestoneEventsHelper(SiteUser)).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.CartageMilestoneEventUpdates.Value, WebParties);
				}
				return new List<string>();
			}
		}

		#endregion

		#region IEventReferenceProvider

		public string EventReference
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					return (new EventReferenceHelper(WebEnv.AppInstance.SiteUser as TrackingSiteUser)).GetEventReferences(WebParties);
				}
				return string.Empty;
			}
		}

		#endregion

		#region WebParties

		WebPartyTypeOrgPairCollection WebParties
		{
			get
			{
				webParties = new WebPartyTypeOrgPairCollection();
				if (FirstDocAddress != null)
				{
					if (FirstDocAddress.DocAddressType == DocAddressType.LocalCartageCFS)
					{
						webParties.Add(WebPartyType.CFS, FirstDocAddress);
					}
					if (FirstDocAddress.DocAddressType == DocAddressType.LocalCartageCTO)
					{
						webParties.Add(WebPartyType.CTO, FirstDocAddress);
					}
					if (FirstDocAddress.DocAddressType == DocAddressType.LocalCartageImporter)
					{
						webParties.Add(WebPartyType.Consignee, FirstDocAddress);
					}
					if (FirstDocAddress.DocAddressType == DocAddressType.LocalCartageExporter)
					{
						webParties.Add(WebPartyType.Shipper, FirstDocAddress);
					}
				}
				if (SecondDocAddress != null)
				{
					if (SecondDocAddress.DocAddressType == DocAddressType.LocalCartageCFS)
					{
						webParties.Add(WebPartyType.CFS, SecondDocAddress);
					}
					if (SecondDocAddress.DocAddressType == DocAddressType.LocalCartageCTO)
					{
						webParties.Add(WebPartyType.CTO, SecondDocAddress);
					}
					if (SecondDocAddress.DocAddressType == DocAddressType.LocalCartageImporter)
					{
						webParties.Add(WebPartyType.Consignee, SecondDocAddress);
					}
					if (SecondDocAddress.DocAddressType == DocAddressType.LocalCartageExporter)
					{
						webParties.Add(WebPartyType.Shipper, SecondDocAddress);
					}
				}
				webParties.Add(WebPartyType.LocalClient, LocalClient);

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		#endregion

		#region LoggedInContact

		public OrgContact LoggedInContact
		{
			get { return fLoggedInContact ?? WebEnv.CurrentUser as OrgContact; }
			set { fLoggedInContact = value; }
		}
		protected OrgContact fLoggedInContact;

		#endregion

		#region IWebDocumentsSupport Members

		public DocumentSupport DocumentHelper
		{
			get
			{
				if (fDocumentHelper == null)
				{
					fDocumentHelper = new DocumentSupport(this);
				}
				return fDocumentHelper;
			}
		}
		DocumentSupport fDocumentHelper;

		public ZGuid DocParentPK
		{
			get { return PK; }
		}

		#endregion

		#region IWebDocumentsWithUploadSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return ((IDocManagerSupport)this).DocManagerInfo;
			}
		}

		public DocumentUploadSupport DocumentUploadHelper
		{
			get
			{
				if (documentUploadHelper == null)
				{
					documentUploadHelper = new DocumentUploadSupport(Factory);
				}
				return documentUploadHelper;
			}
		}

		DocumentUploadSupport documentUploadHelper;

		public void ResetDocumentHelper()
		{
			fDocumentHelper = null;
		}

		public List<ZGuid> DocRelatedPKs
		{ get { return new List<ZGuid>(); } }

		#endregion

		#region IBizOChangesEmailNotification Members

		public ZString Number
		{
			get { return this.JJ_ConsignmentID; }
		}

		public new ZBool IsCancelled
		{
			get { return this.JJ_IsCancelled; }
		}

		public GlbBranch EventBranch
		{
			get
			{
				GlbBranch result = Factory.Load<GlbBranch>(this.JJ_GB);
				return result ?? GlbBranch.FindControllingBranchWithFallBackToAnyCompany(((IBizOChangesEmailNotification)this).RelatedOrg);
			}
		}

		public ZArchitecture.Environment.GuidRegistryItem EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.TrackingCartageNotificationEmailGroup; }
		}

		public void AddPropertiesForEmailReporting(DataState state)
		{
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
		}

		PropertyChangeInfoCollection PropertiesForEmailReporting
		{
			get
			{
				if (fPropertiesForEmailReporting == null)
				{
					fPropertiesForEmailReporting = new PropertyChangeInfoCollection();
				}

				return fPropertiesForEmailReporting;
			}
		}
		PropertyChangeInfoCollection fPropertiesForEmailReporting;

		public PropertyChangeInfo[] GetPropertiesForEmailReporting()
		{
			return PropertiesForEmailReporting.GetValuesAsArray();
		}

		public ControllerID ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.Cartage; }
		}

		public OrgHeader RelatedOrg
		{
			get
			{
				return this.LocalClient;
			}
		}

		public ZGuid GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			ZString staffNk = staffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsLookups.ContainerYardServices);
			GlbStaff staff = staffAssignments.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNk);
			if (staff != null)
			{
				return staff.PK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		public CodeDescriptionBoolRegistryItem StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.TrackingCartageNotificationStaffRoles; }
		}

		public ZArchitecture.Environment.CodePairRegistryItem NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.TrackingCartageNotificationOptions; }
		}

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("1900c2b2-f561-4146-94b8-2913eee09bef", "Transport Job {0}", JJ_ConsignmentID);
			}
		}
	}
}
