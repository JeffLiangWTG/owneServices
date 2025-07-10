using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class DummyBizOWithNotifier : DummyEnterpriseBusinessObject, IBizOChangesEmailNotification, IWebUserEditableNoteSupport
	{
		public DummyBizOWithNotifier(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Notifier = new BusinessObjectChangesEmailNotifier(this);
		}

		public static DummyBizOWithNotifier New(BusinessObjectFactory factory, GlbBranch branch, OrgContact contact, GuidRegistryItem emailGroup)
		{
			DummyBizOWithNotifier bizO = factory.New<DummyBizOWithNotifier>();
			bizO.EventBranch = branch;
			bizO.LoggedInContact = contact;
			bizO.EmailGroupRegistryItem = emailGroup;
			return bizO;
		}

		public BusinessObjectChangesEmailNotifier Notifier;

		public bool NonHasChangesAffectingProperty { get; set; }

		#region IWebEmailNotification Members

		public ZString Number
		{
			get { return "123"; }
		}

		public OrgContact LoggedInContact { get; set; }

		public ZBool IsCancelled
		{
			get { return fCancelled; }
			set { fCancelled = value; HasChanges = true; }
		}
		ZBool fCancelled;

		public GlbBranch EventBranch { get; set; }

		public GlbBranch EventBranchSetterForTest { set { EventBranch = value; } }

		public GuidRegistryItem EmailGroupRegistryItem { get; set; }

		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state) { }

		public PropertyChangeInfo[] GetPropertiesForEmailReporting()
		{
			return new[]
				{
						new PropertyChangeInfo((NoResString)"Human Readable Name", "Original Value", "Updated Value"),
						new PropertyChangeInfo((NoResString)"* Human Readable Name *", "* Original Value *", "* Updated Value *") ,
						new PropertyChangeInfo((NoResString)"Incorrect symbols for HTML", "5 <> 4", "6 <> 4")
					};
		}

		public ControllerID ControllerForEnterpriseUrl
		{
			get { return fÑontrollerForEnterpriseSetterForTest; }
		}

		public OrgHeader RelatedOrgDummy;
		OrgHeader IBizOChangesEmailNotification.RelatedOrg { get { return RelatedOrgDummy; } }

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			ZString staffNK = staffAssignments.GetStaffAssignment(role.Code,
				OrgStaffAssignmentsCollection.Direction.Export, OrgStaffAssignmentsCollection.AirSea.Sea);
			GlbStaff staff = staffAssignments.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
			if (staff != null)
			{
				return staff.PK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		public CodeDescriptionBoolRegistryItem StaffRolesToNotifyDummy;
		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify { get { return StaffRolesToNotifyDummy; } }

		public CodePairRegistryItem notificationSendingRuleSetter = WebDataRegistry.Instance.BookingNotificationOptions;
		CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return notificationSendingRuleSetter; }
		}

		public ControllerID fÑontrollerForEnterpriseSetterForTest = DummyControllerIDs.Dummy;

		#endregion

		#region IWebUserEditableNoteSupport Members

		public IStmNoteParent NotesParentBO
		{
			get { return this; }
		}

		public WebUserEditableNote UserEditableNoteHelper
		{
			get { return fUserEditableNoteHelper ?? (fUserEditableNoteHelper = new WebUserEditableNote(this, PredefinedNoteTypes.Instance.SpecialInstructions)); }
		}
		WebUserEditableNote fUserEditableNoteHelper;

		#endregion
	}
}
