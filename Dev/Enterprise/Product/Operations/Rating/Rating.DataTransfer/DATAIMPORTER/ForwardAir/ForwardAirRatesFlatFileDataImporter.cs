
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.ForwardAir
{
	internal class ForwardAirRatesFlatFileDataImporter : FlatFileDataImporter
	{
		public ForwardAirRatesFlatFileDataImporter(RatingHeader ratingHeader)
			: base(ratingHeader)
		{
		}

		RatingHeader RatingHeader
		{
			get { return (RatingHeader)base.BusinessEntity; }
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.Rate();
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new ForwardAirRatesFlatFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new ForwardAirRatesFlatFileFormat(RatingHeader.Factory); }
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			Xsd.Rate rate = (Xsd.Rate)xSD;

			ForwardAirRatesValueObjectDataAdapter adapter = new ForwardAirRatesValueObjectDataAdapter();
			ValueObjectImportContext importContext = new ValueObjectImportContext(RatingHeader.Factory, notifications);
			adapter.ImportFromValueObject(RatingHeader, rate, importContext);

			return true;
		}
	}
}

