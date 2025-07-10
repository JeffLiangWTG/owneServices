using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa
{
	public partial class MAFeBACCaContainerUserControl : ZUserControl
	{
		public MAFeBACCaContainerUserControl(JobDeclaration jobDeclaration)
		{
			InitializeComponent();
			this.jobDeclaration = jobDeclaration;
			SetControlVisibility();
		}
		readonly JobDeclaration jobDeclaration;

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			SetControlVisibility();
		}

		#region SetControlVisibilityForTradeSingleWindow
		protected void SetControlVisibility()
		{
			MPINumberTextBox.Visible = jobDeclaration.IsTSWDeclaration;
			ContainerPackingAddressGroupBox.Visible = jobDeclaration.IsTSWDeclaration;
			if (jobDeclaration.IsTSWDeclaration)
			{
				ContainerTypeDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("MAFeBACCaContainerUserControl|76F46B5B-AD35-4F2C-9786-3164D32C741C", "Container Type", "TSW Container Type", "State the size and type of container using the list of Container Types specified by TSW.");
			}
			else
			{
				ContainerTypeDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("MAFeBACCaContainerUserControl|7b549a4e-8132-4753-8fa7-4d9af43b0995", "Container Type", "MPI Container Type", "The type of container using the list of Container Types specified by MPI.");
			}
			ContainerTypeDropEdit.GetExtension<ILabelCaptionRenderer>().Caption = ContainerTypeDropEdit.CaptionResourceString.Caption;
		}
		#endregion
	}
}
