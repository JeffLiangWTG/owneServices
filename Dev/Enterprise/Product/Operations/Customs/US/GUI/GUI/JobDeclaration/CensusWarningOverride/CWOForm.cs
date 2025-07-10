using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	partial class CWOForm : ZChildForm
	{
		public enum Action { None, Save, Send }

		public CWOForm(EntryCensusWarningOverrideCollection coll, ZString entryNumber) : base(coll)
		{
			this.entryNumber = entryNumber;
			this.coll = coll;
		}

		readonly ZString entryNumber;
		readonly EntryCensusWarningOverrideCollection coll;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			censusWarningOverrideUserControl1.SetAdditionalGridLayoutKey("CWOForm");
		}

		public override string FormCaption
		{
			get { return "Census Warning Overrides - " + entryNumber; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		internal Action ActionChosenByUsers;

		void CancelButton1_Click(object sender, EventArgs e)
		{
			ActionChosenByUsers = Action.None;
			this.Close();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			RunValidationsAndCheck(Action.Save);
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			RunValidationsAndCheck(Action.Send);
		}

		void RunValidationsAndCheck(Action actionToPerformIfOK)
		{
			coll.RunPreSaveValidation();

			if (coll.HasErrors())
			{
				Globals.Message.ShowError("Please fix the errors first");
			}
			else if (!coll.HasMessageErrors() || Globals.Message.Show("There are message errors on this form. Do you wish to continue?", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				ActionChosenByUsers = actionToPerformIfOK;
				Close();
			}
		}

		void DefaultButton_Click(object sender, EventArgs e)
		{
			var existingCount = coll.Count;

			coll.DefaultCustomsCWOs();

			if (coll.Count == existingCount)
			{
				Globals.Message.ShowInformation("Finished. No new Census Warning exists.");
			}
		}
	}
}
