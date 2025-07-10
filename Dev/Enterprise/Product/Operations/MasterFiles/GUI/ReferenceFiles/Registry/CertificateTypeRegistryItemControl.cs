using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CertificateTypeRegistryItemControl : RegistryZUserControl
	{
		public CertificateTypeRegistryItemControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			IBusiness current = dataSource as IBusiness;
			if (current != null)
			{
				current.SetCountedReadOnlyIncludingChildren(readOnlySet);
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			readOnlySet = readOnly;
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

		bool readOnlySet;
	}
}
