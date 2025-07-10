using System;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class DefaultStatementPrintDateControl : RegistryZUserControl
	{
		public DefaultStatementPrintDateControl()
		{
			InitializeComponent();
		}

		public new DefaultStatementPrintDate CurrentDataItem
		{
			get { return (DefaultStatementPrintDate)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetBusinessEntityReadOnly();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
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
