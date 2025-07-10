using System;

namespace CargoWise.RefDbRepo.CAReferenceData.Business
{
	public interface IProducer
	{
		string OutPutFilePath { get; }
		string FunctionCode { get; }
		string WorkingFileOrDirectory { get; }
		DateTime PublicationTime { get; }
		void QueryDataAndParseToXMLFile();
	}
}
