using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbStaff)]
	public class GlbStaffManyToManyCollection : ManyToManyBusinessObjectCollection
	{
		public GlbStaffManyToManyCollection(GlbGroup parent)
			: base(parent)
		{
			this.parent = parent;
			if (parent.IsControlledByScim)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		readonly GlbGroup parent;

		#region Relationship

		public GlbStaff this[int index]
		{
			get { return (GlbStaff)Elements[index]; }
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(GlbGroupLink); }
		}

		#endregion

		#region Adding Staff

		public new GlbStaff AddNew()
		{
			return (GlbStaff)base.AddNew();
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			var staff = (GlbStaff)businessObject;
			staff.CurrentGroupLink = (GlbGroupLink)GetRelationshipBusinessObject(businessObject);
			staff.ValidationSuspendedForStaff = true;
			OnStaffAdded?.Invoke(staff, new EventArgs());
		}

		public EventHandler OnStaffAdded;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region Removing Staff

		public event EventHandler AttemptToDeleteFromAllUsers;
		public event EventHandler AttemptToDeleteFromDatabaseAccess;
		public event EventHandler AttemptToDeleteFromSCIM;

		public override void Remove(BusinessObject elementToRemove)
		{
			var staff = (GlbStaff)elementToRemove;
			if ((!parent.IsAllStaffGroup && !parent.IsFixedDatabaseAccessGroup && !parent.IsScimMappedGroup) || !staff.GS_IsActive || this.IsRefreshingByDataRefreshBus)
			{
				base.Remove(elementToRemove);
			}
			else if (parent.IsFixedDatabaseAccessGroup && AttemptToDeleteFromDatabaseAccess != null)
			{
				AttemptToDeleteFromDatabaseAccess(this, EventArgs.Empty);
			}
			else if (parent.IsAllStaffGroup && AttemptToDeleteFromAllUsers != null)
			{
				AttemptToDeleteFromAllUsers(this, EventArgs.Empty);
			}
			else if (parent.IsScimMappedGroup && AttemptToDeleteFromSCIM != null)
			{
				AttemptToDeleteFromSCIM(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
