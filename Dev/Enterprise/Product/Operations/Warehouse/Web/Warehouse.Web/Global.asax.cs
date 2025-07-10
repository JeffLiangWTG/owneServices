using System;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Warehouse.Web
{
	public class Global : ZGlobal
	{
		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);

			Globals.IsUserInteractive = false; // to differentiate between WebTracker and RF
		}

		#region Page Constants

		public override string DefaultPage
		{
			get { return ApplicationRoot + "Default.aspx"; }
		}

		public override string LoginPage
		{
			get { return ApplicationRoot + "Login.aspx"; }
		}

		#endregion
	}
}
