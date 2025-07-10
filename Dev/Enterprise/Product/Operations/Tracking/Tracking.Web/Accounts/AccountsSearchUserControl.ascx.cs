using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Accounts
{
	/// <summary>
	///		Summary description for SessionSearchUserControl.
	/// </summary>
	public partial class AccountsSearchUserControl : BaseUserControl
	{
		protected override void OnInit(EventArgs e)
		{
			Zdropdownlist2.ShowEmptyItem = true;
			Zdropdownlist2.EmptyItemText = Res.GetString("eef192d9-9336-4f58-9bbd-ebde1d3aedec", "All companies");
			base.OnInit(e);
		}
	}
}
