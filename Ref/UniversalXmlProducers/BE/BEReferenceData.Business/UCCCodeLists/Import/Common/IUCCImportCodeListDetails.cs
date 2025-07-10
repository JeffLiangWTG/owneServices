using CargoWise.RefDbRepo.BEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public interface IUCCImportCodeListDetails : IUCCCodeListDetails
	{
		XmlWriterConfiguration XmlWriterConfiguration { get; }
	}
}
