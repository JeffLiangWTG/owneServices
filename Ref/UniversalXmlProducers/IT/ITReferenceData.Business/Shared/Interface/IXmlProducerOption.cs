using System;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public interface IXmlProducerOption
	{
		string DataSourceName { get; }
		string FileName { get; }
		DateTime PublicationDateTime { get; }
	}
}
