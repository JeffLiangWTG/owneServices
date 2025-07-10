using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.Declaration;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsMiscOptionUserControl : BaseMiscOptionsUserControl
	{
		private MPIAccountDetailsUserControl mPIAccountDetailsUserControl;
		private readonly System.ComponentModel.Container components;

		public CustomsMiscOptionUserControl()
		{
			InitializeComponent();
		}

		#region Dispose
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
		#endregion

		#region JobDeclaration
		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}
		#endregion

		#region JobDeclaration_ControlVisibilityChanged
		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			bool isECIWriteOff = JobDeclaration.IsECIWriteoff;
			PaymentPartyDropEdit.Visible = !isECIWriteOff;
			MergeByDropEdit.Visible = !isECIWriteOff;
			mPIAccountDetailsUserControl.Visible = JobDeclaration.IsTSWDeclaration && JobDeclaration.IsImport;
		}
		#endregion

	}
}



