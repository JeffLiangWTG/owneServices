using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class PortAuthoritySettingsControl : RegistryZUserControl
	{
		public PortAuthoritySettingsControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			PortAuthoritySettings settings = (PortAuthoritySettings)CurrentDataItem;
			if (settings != null)
			{
				settings.SetCountedReadOnlyIncludingChildren(ReadOnlySet);
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			ReadOnlySet = readOnly;
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			IBusiness current = BindingSource.Current as IBusiness;
			if (current != null)
			{
				current.SetCountedReadOnlyIncludingChildren(readOnly);
			}
		}

		bool ReadOnlySet;
	}
}


