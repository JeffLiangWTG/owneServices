using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	internal sealed partial class ContainerTranshipmentIndicatorControl : RegistryZUserControl
	{
		public ContainerTranshipmentIndicatorControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			IBusiness current = dataSource as IBusiness;
			if (current != null)
			{
				current.SetCountedReadOnlyIncludingChildren(ReadOnlySet);
			}
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


