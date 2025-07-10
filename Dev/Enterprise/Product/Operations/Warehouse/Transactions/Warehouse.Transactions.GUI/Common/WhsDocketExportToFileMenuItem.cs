using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public abstract class WhsDocketExportToFileMenuItem<TDocket, TValueObject> : WhsDocketExportMenuItem<TDocket, TValueObject>
		where TDocket : WhsDocket
		where TValueObject : IValueObject
	{
		protected WhsDocketExportToFileMenuItem(TDocket docket, WhsValueObjectDataAdapter<TDocket, TValueObject> adapter)
			: base(docket, adapter)
		{
			this.Caption = ResString.GetMultilingualString("19cb02e3-2f0b-49cb-8c59-df1aba0dee3f", "Store to File");
		}

		protected WhsDocketExportToFileMenuItem(TDocket docket)
			: this(docket, null)
		{
		}
	}
}
