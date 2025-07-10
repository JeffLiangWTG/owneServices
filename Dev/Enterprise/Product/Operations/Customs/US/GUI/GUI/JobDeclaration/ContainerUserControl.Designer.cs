
namespace Enterprise.Customs.US.GUI
{
	partial class ContainerUserControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}


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
			this.BaseAllPanel.SuspendLayout();
			this.BaseContainerPanel.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.CusContainer)(null)).CO_ContainerNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusContainer)(null)).CO_ContainerNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.CusContainer)(null)).CO_SealInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusContainer)(null)).CO_Seal)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusContainer)(null)).Lookups.ContainerTypeCollection)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.CusContainer)(null)).CO_RC)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.CusContainer)(null)).CO_RCInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusContainer)(null)).Lookups.CO_FCL_LCL_NCT_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.CusContainer)(null)).CO_FCL_LCL_AIRInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusContainer)(null)).CO_FCL_LCL_AIR)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusContainer)(null)).CO_Weight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.CusContainer)(null)).CO_WeightInfo)));
			// 
			// ContainerUserControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.CusContainer";
			this.Name = "ContainerUserControl";
			this.BaseAllPanel.ResumeLayout(false);
			this.BaseContainerPanel.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion


	}
}
