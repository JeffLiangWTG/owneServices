using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public interface IReferenceDataParser
	{
		string DataSource { get; }

		DateTime PublicationDateTime { get; }

		RefDataRepoModelEntityType[] GetEntities();

		XmlWriterConfiguration GetXmlWriterConfiguration();
	}
}
