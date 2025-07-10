using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GroupFormForTestCollection
	{
		public GroupFormForTestCollection(GlbGroup group)
		{
			fGroup = group;
			AllUserEventFired = false;
			DatabaseAccessEventFired = false;
			group.Staff.AttemptToDeleteFromAllUsers += new EventHandler(OnAllUserEventFired);
			group.Staff.AttemptToDeleteFromDatabaseAccess += new EventHandler(OnDatabaseAccessEventFired);
			group.Staff.AttemptToDeleteFromSCIM += OnAttemptToDeleteFromSCIM;
		}

		readonly GlbGroup fGroup;

		public bool AllUserEventFired;
		public bool DatabaseAccessEventFired;
		public bool SCIMEventFired;

		public void Dispose()
		{
			fGroup.Staff.AttemptToDeleteFromAllUsers -= new EventHandler(OnAllUserEventFired);
			fGroup.Staff.AttemptToDeleteFromDatabaseAccess -= new EventHandler(OnDatabaseAccessEventFired);
			fGroup.Staff.AttemptToDeleteFromSCIM -= new EventHandler(OnAttemptToDeleteFromSCIM);
		}

		void OnAllUserEventFired(object sender, EventArgs e)
		{
			AllUserEventFired = true;
		}

		void OnDatabaseAccessEventFired(object sender, EventArgs e)
		{
			DatabaseAccessEventFired = true;
		}

		void OnAttemptToDeleteFromSCIM(object sender, EventArgs e)
		{
			SCIMEventFired = true;
		}
	}
}
