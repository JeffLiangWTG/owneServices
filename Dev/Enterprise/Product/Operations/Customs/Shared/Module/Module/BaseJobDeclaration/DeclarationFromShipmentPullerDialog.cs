using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Module
{
	public partial class DeclarationFromShipmentPullerDialog : ZArchitecture.GUI.ZChildForm
	{
		public DeclarationFromShipmentPullerDialog()
		{
		}

		public override string FormHeading
		{
			get { return Text; }
		}

		public DeclarationFromShipmentPullerDialog(Business.DeclarationFromShipmentPuller puller)
			: base(puller)
		{
		}

		public new Business.DeclarationFromShipmentPuller BusinessEntity
		{
			get { return (Business.DeclarationFromShipmentPuller)base.BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.InitializeComponent();
		}

		void CreateButton_Click(object sender, System.EventArgs e)
		{
			if (BusinessEntity.HasErrors)
			{
				fCreateClicked = false;
				Globals.Message.ShowError(FixAllErrors);
			}
			else
			{
				fCreateClicked = true;
				Close();
			}
		}

		internal static string FixAllErrors
		{
			get { return Res.GetString("ae4a8744-5756-4039-973f-513ca16a0ce5", "Please fix all errors on the form before creating a brokerage job."); }
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		bool fCreateClicked;
		public bool CreateClicked
		{
			get { return fCreateClicked; }
		}

#if DEBUG
		public void SetCreateClickedForTesting(bool value)
		{
			fCreateClicked = value;
		}
#endif
	}
}
