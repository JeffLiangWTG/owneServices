using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public abstract class WhsDocketExportToEmailMenuItem<TDocket, TValueObject> : WhsDocketExportMenuItem<TDocket, TValueObject>
		where TDocket : WhsDocket
		where TValueObject : IValueObject
	{
		protected WhsDocketExportToEmailMenuItem(TDocket docket, WhsValueObjectDataAdapter<TDocket, TValueObject> adapter)
			: base(docket, adapter)
		{
			this.Caption = ResString.GetMultilingualString("23697a02-e862-44b7-a2ab-37679df712fe", "Send Email");
		}
	}
}
