using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// Summary description for ClassAInvoiceForm.
	/// </summary>
	public partial class LockReleaseComplianceBookForm : ZForm
	{
		public LockReleaseComplianceBookForm()
		{
			InitializeComponent();
		}

		public LockReleaseComplianceBookForm(LockReleaseComplianceBook lockRelease) : base(lockRelease)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, SaveButton);

			CaptionResourceString = ((LockReleaseComplianceBook)this.DataSource).IsLock ?
				Res.GetData("367ff00a-da29-463a-b0ab-0bd17ab4abb6", "Lock Counter Compliance Book") :
				Res.GetData("08f9866f-4051-431c-9480-8857ff6b4e6d", "Release Counter Compliance Book");

			SaveButton.CaptionResourceString = ((LockReleaseComplianceBook)this.DataSource).IsLock ?
				Res.GetData("D672C648-C8A8-4b90-B5FB-6984DADC8982", "Lock") :
				Res.GetData("4F55066F-DC5B-4aba-9BB9-679096821724", "Release");
		}

		public override string FormVerb
		{
			get
			{
				return string.Empty;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (base.ValidateAndSave() == ContinueWithSave.Yes)
			{
				var message = ((LockReleaseComplianceBook)this.DataSource).IsLock ?
				   Res.GetString("cf436510-f99f-4bcd-861b-93b3ceecdbf9", "Lock successfully") :
				   Res.GetString("2bdd6d21-ebfc-435b-ad6c-752306041cd3", "Release successfully");

				Globals.Message.Show(message);
				return ContinueWithSave.Yes;
			}

			return ContinueWithSave.No;
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
		}
	}
}


