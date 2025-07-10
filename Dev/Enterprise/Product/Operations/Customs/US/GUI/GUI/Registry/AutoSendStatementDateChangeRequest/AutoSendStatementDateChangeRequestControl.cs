using System;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public partial class AutoSendStatementDateChangeRequestControl : RegistryZUserControl
	{
		public AutoSendStatementDateChangeRequestControl()
		{
			InitializeComponent();
		}

		public new AutoSendStatementDateChangeRequest CurrentDataItem
		{
			get { return (AutoSendStatementDateChangeRequest)base.CurrentDataItem; }
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
