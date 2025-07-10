using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupLink : AutoGlbGroupLink
	{
		public GlbGroupLink(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (Lookups.StaffMembershipTypesList.ContainsCode(MembershipTypeList.Codes.UDF))
			{
				GK_MembershipType = MembershipTypeList.Codes.UDF;
			}
			else if (Lookups.StaffMembershipTypesList.ContainsCode(MembershipTypeList.Codes.STF))
			{
				GK_MembershipType = MembershipTypeList.Codes.STF;
			}
		}

		#endregion

		#region Properties

		[List("Lookups.StaffMembershipTypesList")]
		public override ZString GK_MembershipType
		{
			get
			{
				return base.GK_MembershipType;
			}
			set
			{
				base.GK_MembershipType = value;
			}
		}

		public override ZPropertyInfo GK_GGInfo
		{
			get
			{
				ZPropertyInfo info = base.GK_GGInfo;
				if (this.Group != null && this.Group.GG_IsSales)
				{
					info.HumanReadableName = Res.GetString("709edd34-a04e-47d0-8318-2f734893b750", "Sales Team");
				}
				return info;
			}
		}

		public override ZGuid GK_GS
		{
			get => base.GK_GS;
			set
			{
				if (!base.GK_GS.IsEmpty && base.GK_GS != value && Staff != null)
				{
					Staff.GroupLinks.Remove(this);
				}
				base.GK_GS = value;
				if (!value.IsEmpty && Staff != null)
				{
					Staff.GroupLinks.Add(this);
				}
			}
		}

		#endregion

		#region Logging

		public override void OnSaving()
		{
			if (!IsDeleted && Staff != null)
			{
				if (Staff.IsInDatabase && !Staff.HasChanges)
				{
					Staff.Reload();
				}
				if (!Staff.GS_IsActive && !Group.GG_IsSales)
				{
					this.Delete();
				}
			}

			base.OnSaving();

			if (!IsDeleted && !IsInDatabase)
			{
				AddStaffGroupRelationshipLog(LoggingAction.Attach);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded && !IsInDatabase)
			{
				AddStaffGroupRelationshipLog(LoggingAction.Detach, false);
			}
		}

		public override void Delete()
		{
			Staff?.GroupLinks.Remove(this);
			base.Delete();
		}

		protected override void OnSavingForDelete()
		{
			AddDetachLog();
			base.OnSavingForDelete();
		}

		protected override void OnSavedForDeletedObject(bool saveSucceeded)
		{
			base.OnSavedForDeletedObject(saveSucceeded);

			if (!saveSucceeded && IsInDatabase)
			{
				using (ErrorReporter.SuppressErrorReporting())
				{
					AddStaffGroupRelationshipLog(LoggingAction.Attach, false);
				}
			}
		}

		void AddDetachLog()
		{
			using (ErrorReporter.SuppressErrorReporting())
			{
				var factory = new BusinessObjectFactory();
				var thisLink = factory.Load<GlbGroupLink>(PK);
				if (thisLink != null)
				{
					AddStaffGroupRelationshipLog(LoggingAction.Detach);
				}
			}
		}

		public override GlbStaff Staff
		{
			get
			{
				if (glbStaff == null)
				{
					glbStaff = base.Staff;
				}
				return glbStaff;
			}
		}
		GlbStaff glbStaff;

		public override GlbGroup Group
		{
			get
			{
				if (glbGroup == null)
				{
					glbGroup = base.Group;
				}
				return glbGroup;
			}
		}
		GlbGroup glbGroup;

		void SignalGroupNeedToBeADSynced(GlbGroup group)
		{
			if (group.IsADIntegrationEnabled)
			{
				// This is to signal the group has been changed so it will be picked by AD sync in the next run
				group.GG_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}
		}

		void AddStaffGroupRelationshipLog(LoggingAction action, bool shouldAddLog = true)
		{
			if (Staff != null && Group != null && !Group.IsAllStaffGroup)
			{
				var @event = GetEvent(action);
				Group.Logs.AddNew(@event, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Staff, Staff.GS_Code));
				Group.AddRelationshipLog(action, Staff.GS_Code, Staff.GS_FullName, shouldAddLog);
				SignalGroupNeedToBeADSynced(Group);

				Staff.AddRelationshipLog(action, Group.GG_Code, Group.GG_Desc, shouldAddLog);
				Staff.Logs.AddNew(@event, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Group, Group.GG_Code));
			}
		}

		static Event GetEvent(LoggingAction action)
		{
			return action switch
			{
				LoggingAction.Attach => Events.Attached,
				LoggingAction.Detach => Events.Detached,
				_ => throw new ArgumentException("Invalid action " + action, nameof(action))
			};
		}

		#endregion
	}
}
