using CargoWise.RefDbRepo.CAReferenceData.Business.CASurtax;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	public static class CASurtaxRunner
	{
		public static void Run()
		{
			new CASurtaxProducer().QueryDataAndParseToXMLFile();
		}
	}
}
