using System;

namespace Enterprise.Customs.NL.GUI
{
	public partial class EntryLineAdditionalDataUserControl : EU.GUI.EntryLineAdditionalDataUserControl
	{
		public EntryLineAdditionalDataUserControl()
		{
			InitializeComponent();
		}

		protected override Type GetDutyAndTaxDetailsUserControlType() => typeof(EntryLineTaxAndFeeUserControl);
	}
}
