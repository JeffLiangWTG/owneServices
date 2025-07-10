using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public sealed class XmlProducerOption : IXmlProducerOption
	{
		public XmlProducerOption(IDateTimeProvider dateTimeProvider, IPublicationTimeLoader publicationTimeLoader)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.publicationTimeLoader = Argument.NotNull(publicationTimeLoader, nameof(publicationTimeLoader));
		}

		readonly IDateTimeProvider dateTimeProvider;
		readonly IPublicationTimeLoader publicationTimeLoader;

		string IXmlProducerOption.DataSourceName => DataSourceName;

		string IXmlProducerOption.FileName => $"{DataSourceName}_{dateTimeProvider.Now:yyyyMMddHHmmss}.xml";

		DateTime IXmlProducerOption.PublicationDateTime => publicationTimeLoader.GetDateTime();

		const string DataSourceName = "IT Taric AIDA Supporting Documents";
	}
}
