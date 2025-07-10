using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class WhsWorkOrderAssemblyConfirmationUserControl : ZUserControl
	{
		public WhsWorkOrderAssemblyConfirmationUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is WhsReceive receive)
			{
				var customProperties = ReceiveFromWorkOrderCustomPropertiesProvider.GetCustomPropertiesForAssembledReceive();
				new ZGridCustomColumnsInitializerWithPropertyContainer(AssemblyGrid, receive.Lines, customProperties, groupName: null, isVisible: true)
					.AddCustomColumns(customProperties.CustomProperties);
			}

			base.SetDataBinding(dataSource, dataMember);
		}
	}
}
