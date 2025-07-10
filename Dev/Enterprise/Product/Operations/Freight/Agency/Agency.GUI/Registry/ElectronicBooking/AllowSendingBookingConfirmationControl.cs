using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class AllowSendingBookingConfirmationControl : RegistryZUserControl
	{
		public AllowSendingBookingConfirmationControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (BindingSource.Current != null)
			{
				((IBusiness)BindingSource.Current).SetCountedReadOnlyIncludingChildren(ReadOnlySet);
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			ReadOnlySet = readOnly;

			base.SetControlOrBusinessEntityReadOnly(readOnly);

			this.PrincipalGrid.ReadOnly = readOnly;

			IBusiness current = BindingSource.Current as IBusiness;
			if (current != null)
			{
				if (readOnly)
				{
					current.IncrementReadOnlyIncludingChildren();
				}
				else
				{
					current.DecrementReadOnlyIncludingChildren();
				}
			}
		}

		bool ReadOnlySet;
	}
}
