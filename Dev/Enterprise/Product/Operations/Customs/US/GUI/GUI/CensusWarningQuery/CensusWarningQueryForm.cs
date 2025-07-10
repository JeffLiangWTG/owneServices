using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class CensusWarningQueryForm : ZChildForm
	{
		public enum Action { None, Send }

		public CensusWarningQueryForm(CensusWarningQuery messageData)
			: base(messageData)
		{
		}

		public new CensusWarningQuery BusinessEntity
		{
			get { return (CensusWarningQuery)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "Census Warning Query"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public Action ActionChosenByUsers;

		void SendButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();

			if (BusinessEntity.HasErrors)
			{
				Globals.Message.ShowError("Please fix the errors first");
			}
			else
			{
				ActionChosenByUsers = Action.Send;
				new CensusWarningQueryMessageBuilder(BusinessEntity).PopulateMessage();
				Close();
			}
		}

		void GiveUpButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
