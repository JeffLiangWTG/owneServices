using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbGroup)]
	public class GlbGroupManyToManyCollection : ManyToManyBusinessObjectCollection<GlbGroup, GlbStaff>
	{
		public GlbGroupManyToManyCollection(GlbStaff parent) : base(parent)
		{
			this.Staff = parent;
			if (parent.IsControlledByScim)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		public GlbGroupManyToManyCollection(GlbStaff parent, ZQuery filter) : base(parent, filter)
		{
			this.Staff = parent;
			if (parent.IsControlledByScim)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		internal readonly GlbStaff Staff;

		#region Implementation

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			GlbGroup group = (GlbGroup)businessObject;
			group.CurrentGroupLink = (GlbGroupLink)GetRelationshipBusinessObject(group);
			Staff.MarkAsNeedingValidation();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(GlbGroupLink); }
		}

		#endregion

		#region Remove

		public override void Remove(BusinessObject elementToRemove)
		{
			Remove(elementToRemove, false);
		}

		internal void Remove(BusinessObject elementToRemove, bool allowToRemoveAllGroup)
		{
			var group = (GlbGroup)elementToRemove;
			if ((!group.IsAllStaffGroup && !group.IsFixedDatabaseAccessGroup && !group.IsScimMappedGroup) || !Staff.GS_IsActive || allowToRemoveAllGroup || IsRefreshingByDataRefreshBus)
			{
				base.Remove(elementToRemove);
				Staff.MarkAsNeedingValidation();
			}
			else if (group.IsFixedDatabaseAccessGroup)
			{
				OnAttemptToDeleteFromDatabaseAccess();
			}
			else if (group.IsScimMappedGroup)
			{
				OnAttemptToDeleteFromSCIM();
			}
			else
			{
				OnAttemptToDeleteFromAllUsers();
			}
		}

		#region Attempt To Delete From All Users Event

		public event EventHandler AttemptToDeleteFromAllUsers;

		public void OnAttemptToDeleteFromAllUsers()
		{
			if (AttemptToDeleteFromAllUsers != null)
			{
				AttemptToDeleteFromAllUsers(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Attempt To Delete From Database Access Event

		public event EventHandler AttemptToDeleteFromDatabaseAccess;

		public void OnAttemptToDeleteFromDatabaseAccess()
		{
			if (AttemptToDeleteFromDatabaseAccess != null)
			{
				AttemptToDeleteFromDatabaseAccess(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Attempt To Delete From SCIM Event

		public event EventHandler AttemptToDeleteFromSCIM;

		public void OnAttemptToDeleteFromSCIM()
		{
			if (AttemptToDeleteFromSCIM != null)
			{
				AttemptToDeleteFromSCIM(this, EventArgs.Empty);
			}
		}

		#endregion

		#endregion
	}

	[ModuleID(ModuleId.GlbGroup)]
	public class GroupCollectionView : BusinessObjectCollectionView<GlbGroup>
	{
		public GroupCollectionView(GlbGroupManyToManyCollection collection, bool isSales, bool activeOnly)
			: base(collection)
		{
			this.isSales = isSales;
			this.activeOnly = activeOnly;
			Rebuild();
		}

		readonly ZBool isSales;
		readonly bool activeOnly;

		public override void Remove(BusinessObject elementToRemove)
		{
			Remove(elementToRemove, false);
		}

		public void Remove(BusinessObject elementToRemove, bool allowToRemoveAllGroup)
		{
			var group = (GlbGroup)elementToRemove;
			if ((!group.IsAllStaffGroup && !group.IsFixedDatabaseAccessGroup && !group.IsScimMappedGroup) || !CollectionToFilter.Staff.GS_IsActive || allowToRemoveAllGroup || CollectionToFilter.IsRefreshingByDataRefreshBus)
			{
				var groupLink = CollectionToFilter.Staff.GroupLinks.Find(gl => gl.GK_GG == group.PK);
				if (groupLink != null)
				{
					CollectionToFilter.Staff.GroupLinks.Remove(groupLink);
				}
				base.Remove(elementToRemove);
			}
			else if (group.IsFixedDatabaseAccessGroup)
			{
				CollectionToFilter.OnAttemptToDeleteFromDatabaseAccess();
			}
			else if (group.IsScimMappedGroup)
			{
				CollectionToFilter.OnAttemptToDeleteFromSCIM();
			}
			else
			{
				CollectionToFilter.OnAttemptToDeleteFromAllUsers();
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var group = (GlbGroup)element;
			return group.GG_IsSales == isSales
				&& (!activeOnly || group.GG_IsActive);
		}

		public new GlbGroupManyToManyCollection CollectionToFilter
		{
			get { return (GlbGroupManyToManyCollection)base.CollectionToFilter; }
		}

		public event EventHandler AttemptToDeleteFromAllUsers
		{
			add { CollectionToFilter.AttemptToDeleteFromAllUsers += value; }
			remove { CollectionToFilter.AttemptToDeleteFromAllUsers -= value; }
		}

		public event EventHandler AttemptToDeleteFromDatabaseAccess
		{
			add { CollectionToFilter.AttemptToDeleteFromDatabaseAccess += value; }
			remove { CollectionToFilter.AttemptToDeleteFromDatabaseAccess -= value; }
		}

		public event EventHandler AttemptToDeleteFromSCIM
		{
			add { CollectionToFilter.AttemptToDeleteFromSCIM += value; }
			remove { CollectionToFilter.AttemptToDeleteFromSCIM -= value; }
		}

		/// <summary>
		/// Does this collection of groups contain any of the group codes given in the provided list?
		/// </summary>
		public bool ContainsAnyCode(params string[] codes)
		{
			bool result = false;
			List<string> codeList = new List<string>(codes);
			foreach (GlbGroup group in this)
			{
				if (codeList.Contains(group.GG_Code))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		#region Group Changed Event

		public event EventHandler GroupChanged;

		public void OnGroupChanged()
		{
			if (GroupChanged != null)
			{
				GroupChanged(this, EventArgs.Empty);
			}
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			if (!IsLoading)
			{
				OnGroupChanged();
			}
		}

		#endregion
	}
}
