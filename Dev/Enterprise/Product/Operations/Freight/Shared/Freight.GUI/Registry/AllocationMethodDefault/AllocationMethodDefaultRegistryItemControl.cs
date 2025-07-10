using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class AllocationMethodDefaultRegistryItemControl : RegistryZUserControl
	{
		public AllocationMethodDefaultRegistryItemControl()
		{
			InitializeComponent();
		}

		#region Implementation

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			IBusiness current = (IBusiness)CurrentDataItem;

			if (current != null)
			{
				current.SetCountedReadOnlyIncludingChildren(ReadOnlySet);
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			ReadOnlySet = readOnly;
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			rulesGrid.ReadOnly = readOnly; // because binding sux!

			IBusiness current = BindingSource.Current as IBusiness;
			if (current != null)
			{
				current.SetCountedReadOnlyIncludingChildren(ReadOnlySet);
			}
		}

		bool ReadOnlySet;

		#endregion
	}
}
