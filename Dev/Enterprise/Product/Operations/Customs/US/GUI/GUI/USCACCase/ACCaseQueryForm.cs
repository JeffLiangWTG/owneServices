using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ACCaseQueryForm : ZChildForm
	{
		public ACCaseQueryForm(ACEACCaseQuery query) : base(query)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return "Query AC/CVD Case (ACE)"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();

			if (!BusinessEntity.HasNotifications() || Globals.Message.Show("There are notifications. Are you sure you wish to continue?", "Query", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, System.Windows.Forms.DialogResult.Cancel) == System.Windows.Forms.DialogResult.OK)
			{
				new ACEACQueryMessageBuilder().Generate(BusinessEntity.Factory, (ACEACCaseQuery)BusinessEntity);

				try
				{
					BusinessEntity.Factory.Save();
				}
				catch (ZSaveException exc)
				{
					ZExceptionReporting.HandleSaveException(exc);
				}
				Close();
			}
		}

		void GiveUpButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
