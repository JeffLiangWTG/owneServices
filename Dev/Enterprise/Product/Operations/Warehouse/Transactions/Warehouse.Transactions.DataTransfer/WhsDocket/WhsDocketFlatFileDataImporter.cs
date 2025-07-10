using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract class WhsDocketFlatFileDataImporter<TBusinessObject> : FlatFileDataImporter
		where TBusinessObject : WhsDocket
	{
		protected override IValueObject CreateXsd()
		{
			return new Xsd.WhsDockets();
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			bool result;
			var xsdDockets = (Xsd.WhsDockets)xSD;
			var collection = GetNewDocketCollection();

			var importContext = new ValueObjectImportContext(FactoryProvider, notifications);
			Adapter.ImportFromValueObject(collection, xsdDockets, importContext);
			result = !((NotificationBuffer)notifications).HasErrors;

			return result;
		}

		WhsDocketValueObjectDataAdapter<TBusinessObject> Adapter => GetDataAdapter();

		protected abstract WhsDocketCollection GetNewDocketCollection();
		protected abstract WhsDocketValueObjectDataAdapter<TBusinessObject> GetDataAdapter();
	}
}
