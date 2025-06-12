namespace CargoWise.eServices.USCustoms.InboundService
{
	partial class ProjectInstaller
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.InboundServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.InboundServiceInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// InboundServiceProcessInstaller
			// 
			this.InboundServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;
			this.InboundServiceProcessInstaller.Password = null;
			this.InboundServiceProcessInstaller.Username = null;
			// 
			// InboundServiceInstaller
			// 
			this.InboundServiceInstaller.ServiceName = "InboundService";
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.InboundServiceProcessInstaller,
            this.InboundServiceInstaller});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller InboundServiceProcessInstaller;
		private System.ServiceProcess.ServiceInstaller InboundServiceInstaller;
	}
}