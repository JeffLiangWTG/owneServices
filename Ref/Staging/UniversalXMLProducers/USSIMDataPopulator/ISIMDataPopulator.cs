using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USSIMDataPopulator
{
	public interface ISIMDataPopulator
	{
		void Parse(string fullSaveFilePath);
		DateTime GetPublishedDateAndSaveExtractFile(string fileUrl, string downloadFilePath, string outputFilePath);
	}
}

