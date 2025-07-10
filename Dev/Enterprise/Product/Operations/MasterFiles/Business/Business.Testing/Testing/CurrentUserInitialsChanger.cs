using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class CurrentUserInitialsChanger : IDisposable
	{
		CurrentUserInitialsChanger(string newInitials)
		{
			OldUserInitials = GlbStaff.CurrentUser.GS_Code;
			GlbStaff.CurrentUser.GS_Code = newInitials;
		}

		public static CurrentUserInitialsChanger ChangeCurrentUserInitials(string newInitials)
		{
			return new CurrentUserInitialsChanger(newInitials);
		}

		public void Dispose()
		{
			GlbStaff.CurrentUser.GS_Code = OldUserInitials;
		}

		readonly string OldUserInitials;
	}
}
