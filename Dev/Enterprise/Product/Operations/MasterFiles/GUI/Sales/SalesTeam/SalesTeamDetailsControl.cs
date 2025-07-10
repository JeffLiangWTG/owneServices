using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesTeamDetailsControl : ZUserControl
	{
		public SalesTeamDetailsControl()
		{
			InitializeComponent();
		}

		#region Classes

		public class MembersModuleGrid : ZModuleButtonGrid
		{
			protected override bool IsAttachAllowed()
			{
				if (!Env.Security.SalesTeamsEdit.IsAllowed)
				{
					Env.Security.SalesTeamsEdit.ShowError();
					return false;
				}

				return true;
			}

			protected override bool IsDetachAllowed()
			{
				if (!Env.Security.SalesTeamsEdit.IsAllowed)
				{
					Env.Security.SalesTeamsEdit.ShowError();
					return false;
				}

				return true;
			}
		}

		#endregion
	}
}
