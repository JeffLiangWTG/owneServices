using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class EntrySummaryQueryForm : ZChildForm
	{
		public EntrySummaryQueryForm(EntrySummaryQueryBizObj bizObj)
			: base(bizObj)
		{
			this.bizObj = bizObj;
		}

		readonly EntrySummaryQueryBizObj bizObj;

		public override string FormCaption
		{
			get { return "Entry Summary Query"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			if (ZZCustomsFunctionality.IsNewEntrySummaryQueryEffective)
			{
				InitializeNewComponent();
			}
		}

		public void SendButton_Click(object sender, System.EventArgs e)
		{
			bizObj.RunPreSaveValidation();

			if (!bizObj.HasMessageErrors ||
				Globals.Message.Show("There are message errors. Are you sure you wish to continue?", "Entry Summary Query", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				bizObj.SendMessage = true;
				Close();
			}
		}

		public void GiveUpButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
