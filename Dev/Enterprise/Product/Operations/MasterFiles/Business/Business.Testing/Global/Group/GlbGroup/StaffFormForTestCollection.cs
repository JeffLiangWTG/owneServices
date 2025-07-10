using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StaffFormForTestCollection : IDisposable
	{
		public StaffFormForTestCollection(GlbStaff staff)
		{
			fStaff = staff;
			AllUsersEventFired = false;
			GroupChangedEventFired = false;
			fStaff.Groups.AttemptToDeleteFromAllUsers += new EventHandler(OnAllUsersEventFired);
			fStaff.Groups.AttemptToDeleteFromDatabaseAccess += new EventHandler(OnDatabaseAccessEventFired);
			fStaff.Groups.AttemptToDeleteFromSCIM += OnAttemptToDeleteFromSCIM;
			fStaff.Groups.GroupChanged += new EventHandler(OnGroupChangedEventFired);
		}

		readonly GlbStaff fStaff;
		public bool AllUsersEventFired;
		public bool DatabaseAccessEventFired;
		public bool GroupChangedEventFired;
		public bool SCIMFired;

		public void Dispose()
		{
			fStaff.Groups.AttemptToDeleteFromAllUsers -= new EventHandler(OnAllUsersEventFired);
			fStaff.Groups.AttemptToDeleteFromDatabaseAccess -= new EventHandler(OnDatabaseAccessEventFired);
			fStaff.Groups.AttemptToDeleteFromSCIM -= new EventHandler(OnAttemptToDeleteFromSCIM);
			fStaff.Groups.GroupChanged -= new EventHandler(OnGroupChangedEventFired);
		}

		void OnAllUsersEventFired(object sender, EventArgs e)
		{
			AllUsersEventFired = true;
		}

		void OnDatabaseAccessEventFired(object sender, EventArgs e)
		{
			DatabaseAccessEventFired = true;
		}

		void OnAttemptToDeleteFromSCIM(object sender, EventArgs e)
		{
			SCIMFired = true;
		}

		void OnGroupChangedEventFired(object sender, EventArgs e)
		{
			GroupChangedEventFired = true;
		}
	}
}
