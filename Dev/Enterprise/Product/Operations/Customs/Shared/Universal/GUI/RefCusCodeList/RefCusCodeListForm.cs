using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCusCodeListForm : ZTemplateForm
	{
		public RefCusCodeListForm(ZZRefCusCodeListCombined dataSource)
			: base(dataSource)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeTransporModeCheckBoxes();
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		public override string FormCaption
		{
			get { return BusinessEntity?.HumanReadableName ?? Res.GetString("RefCusCodeListForm|FormCaption", "Global Tariffs"); }
		}

		protected override void SaveToRecentItems()
		{
		}

		protected new ZZRefCusCodeListCombined BusinessEntity
		{
			get { return base.BusinessEntity as ZZRefCusCodeListCombined; }
		}

		public void InitializeTransporModeCheckBoxes()
		{
			if (!this.IsDesignMode())
			{
				CreateTransportModeCheckBoxes(this.CodeTransportModesPanel, "", ZZRefCusCodeListCombined.GetTransportModePropertyName);
				CreateTransportModeCheckBoxes(this.AttrTransportModesPanel, (NoResString)"Attributes", ZZRefCusCodeListAttributeCombined.GetTransportModePropertyName);
			}
		}

		void CreateTransportModeCheckBoxes(ZPanel panel, string dataMember, Func<ZString, string> getTransportModePropertyName)
		{
			panel.Controls.Clear();

			int i = 0;
			foreach (CodeDescriptionPair transportMode in RefTransportModesHelper.GetList(BusinessEntity?.Factory ?? new BusinessObjectFactory()))
			{
				var propertyName = getTransportModePropertyName(transportMode.Code);
				var transportModeCheckBox = new ZCheckBox();
				transportModeCheckBox.Text = transportMode.Code;
#if DEBUG
				System.ComponentModel.TypeDescriptor.AddAttributes(transportModeCheckBox, new SuppressFormsLocalizedTestAttribute());
#endif
				transportModeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint((i % 3) * 94, (i / 3) * 16, true);
				transportModeCheckBox.TabIndex = i;
				transportModeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
				panel.Controls.Add(transportModeCheckBox);
				this.BindingSource.SetBindingMember(transportModeCheckBox, string.Format(CultureInfo.CurrentCulture, "{0}.{1}", dataMember, propertyName));

				i++;
			}
		}
	}
}
