using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EventContextForm : ZChildForm
	{
		public EventContextForm()
		{
			InitializeComponent();
		}

		public EventContextForm(WorkflowEventContextBizo eventContextBizo)
			: base(eventContextBizo)
		{
			InitializeComponent();
		}

		void buttonOK_Click(object sender, EventArgs e)
		{
			if (ValidateAndSave() == ContinueWithSave.Yes)
			{
				Close();
			}
		}

		void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			// Do nothing here - we have non-persistent object
		}
	}
}
