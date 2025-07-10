using System.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class BackdoorForSavingOnAmendmentForm : ZChildForm
	{
		public BackdoorForSavingOnAmendmentForm()
		{
		}

		public BackdoorForSavingOnAmendmentForm(DeferredAmendmentSavingOptions savingOptions) : base(savingOptions)
		{
			this.SavingOptions = savingOptions;
		}

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

		public override string FormHeading
		{
			get { return Res.GetString("0f3433cf-fd39-4046-bde4-fd3c1d34a54a", "Saving Options"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(TextLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public readonly DeferredAmendmentSavingOptions SavingOptions;

		void OKButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		void CancelButton2_Click(object sender, System.EventArgs e)
		{
			SavingOptions.IsCancelled = true;
			Close();
		}

		public static string SendAmendmentExplanation
		{
			get { return Res.GetString("fd345223-d001-46ec-ae79-cc727e42304f", "If you choose this option, the system will send an amendment now."); }
		}

		void SendAmendmentExplanationButton_Click(object sender, System.EventArgs e)
		{
			Globals.Message.ShowInformation(ExplanationForSendAmendmentOption);
		}

		public static string SaveWithoutEntryChangesExplanation
		{
			get { return Res.GetString("f791617a-8b0b-4cfa-aeaa-5ee64f3b9049", "If you believe you have not made any changes that are related to Customs Entries, then choose this option.\r\nYou will be able to save without sending an amendment message.\r\nAnd the system won't make any changes to Customs Entries including duty and tax calculations."); }
		}

		void SaveWithoutEntryChangesExplanationButton_Click(object sender, System.EventArgs e)
		{
			Globals.Message.ShowInformation(ExplanationForSaveWithoutEntryChanges);
		}

		public static string SaveWithEntryChangesExplanation
		{
			get { return Res.GetString("710fae77-b581-43b4-8daa-810995a2ea77", "If you have made changes that affect Customs Entries, please choose this option.\r\nThe system will take an amendment reason and lets you save without sending an amendment message.\r\nBut you will be able to see the changes on the screens and entry prints.\r\nYou can send an amendment later by clicking Brokerage > Send Amendment.\r\nMessage Status on the front page will show 'Amendment Not Lodged'."); }
		}

		void SaveWithEntryChangesExplanationButton_Click(object sender, System.EventArgs e)
		{
			Globals.Message.ShowInformation(ExplanationForSaveWithEntryChanges);
		}

		protected virtual string ExplanationForSendAmendmentOption
		{
			get { return SendAmendmentExplanation; }
		}

		protected virtual string ExplanationForSaveWithoutEntryChanges
		{
			get { return SaveWithoutEntryChangesExplanation; }
		}

		protected virtual string ExplanationForSaveWithEntryChanges
		{
			get { return SaveWithEntryChangesExplanation; }
		}
	}
}
