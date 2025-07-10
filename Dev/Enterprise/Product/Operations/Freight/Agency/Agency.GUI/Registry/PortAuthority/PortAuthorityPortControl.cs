using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	internal sealed partial class PortAuthorityPortControl : RegistryZUserControl
	{
		public PortAuthorityPortControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				((IBusiness)dataSource).SetCountedReadOnlyIncludingChildren(ReadOnlySet);
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			ReadOnlySet = readOnly;
			base.SetControlOrBusinessEntityReadOnly(readOnly);

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


