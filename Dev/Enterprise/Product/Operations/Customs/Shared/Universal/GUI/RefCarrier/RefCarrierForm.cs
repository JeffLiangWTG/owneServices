using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCarrierForm : ZTemplateForm
	{
		public RefCarrierForm(ZZRefCarrierCombined dataSource)
			: base(dataSource)
		{ }

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeTransporModeCheckBoxes();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		public override string FormCaption => BusinessEntity?.HumanReadableName ?? Res.GetString("RefCarrierForm|FormCaption", "Global Tariffs");

		protected override void SaveToRecentItems()
		{ }

		protected new ZZRefCarrierCombined BusinessEntity => base.BusinessEntity as ZZRefCarrierCombined;

		public void InitializeTransporModeCheckBoxes()
		{
			if (!this.IsDesignMode())
			{
				CreateTransportModeCheckBoxes(this.CodeTransportModesPanel, "", ZZRefCarrierCombined.GetTransportModePropertyName);
			}
		}

		void CreateTransportModeCheckBoxes(ZPanel panel, string dataMember, Func<ZString, string> getTransportModePropertyName)
		{
			panel.Controls.Clear();

			int i = 0;
			foreach (CodeDescriptionPair transportMode in RefCarrierHelper.GetTransportModesList(BusinessEntity?.Factory ?? new BusinessObjectFactory()))
			{
				var propertyName = getTransportModePropertyName(transportMode.Code);
				var transportModeCheckBox = new ZCheckBox();
				transportModeCheckBox.Text = transportMode.Code;

#if DEBUG
				System.ComponentModel.TypeDescriptor.AddAttributes(transportModeCheckBox, new SuppressFormsLocalizedTestAttribute());
#endif

				transportModeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint((i % 4) * 70, (i / 4) * 23, true);
				transportModeCheckBox.TabIndex = i++;
				transportModeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
				panel.Controls.Add(transportModeCheckBox);
				this.BindingSource.SetBindingMember(transportModeCheckBox, string.Format(CultureInfo.CurrentCulture, "{0}.{1}", dataMember, propertyName));
			}
		}
	}
}
