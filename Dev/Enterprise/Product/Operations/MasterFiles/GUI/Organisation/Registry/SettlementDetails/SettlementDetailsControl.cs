using System;
using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class SettlementDetailsControl : RegistryZUserControl
	{
		public SettlementDetailsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TermsGrid.ReadOnly = readOnly;
			TreatDisbursementsAsStandardValueBoundCalcEdit.ReadOnly = readOnly;
		}

		void ARPaymentCycleGroupBox_Enter(object sender, EventArgs e)
		{
		}

		void SettlementDetailsControl_Load(object sender, EventArgs e)
		{
		}
	}
}
