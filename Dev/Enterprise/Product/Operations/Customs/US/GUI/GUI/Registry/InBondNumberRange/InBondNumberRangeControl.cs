using System;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class InBondNumberRangeControl : RegistryZUserControl
	{
		public InBondNumberRangeControl()
		{
			InitializeComponent();
		}

		public new InBondNumberRange CurrentDataItem
		{
			get { return (InBondNumberRange)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetBusinessEntityReadOnly();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			StartNumberTextBox.ReadOnly = readOnly;
			LastNumberTextBox.ReadOnly = readOnly;
			SetBusinessEntityReadOnly();
		}

		void SetBusinessEntityReadOnly()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.ReadOnly = ReadOnly;
			}
		}
	}
}
