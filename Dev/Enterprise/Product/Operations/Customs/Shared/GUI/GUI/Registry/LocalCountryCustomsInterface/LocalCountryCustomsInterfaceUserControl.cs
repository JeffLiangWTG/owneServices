using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DataRegistry.GUI
{
	public partial class LocalCountryCustomsInterfaceUserControl : RegistryZUserControl
	{
		public LocalCountryCustomsInterfaceUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			SubmissionTypeDropEdit.ReadOnly = readOnly;
			InterfaceTypeDropEdit.ReadOnly = readOnly;
			RecipientIDTextBox.ReadOnly = readOnly;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var localCountryCustomsInterface = BoundBusinessObject as LocalCountryCustomsInterface;
			ABMInterfaceActivatedLabel.Visible = localCountryCustomsInterface?.IsABMInterfaceActivated ?? false;
		}
	}
}
