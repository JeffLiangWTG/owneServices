using System;
using CargoWise.RefDbRepo.CAReferenceData.Business.CASIMAData;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	public static class CASIMADataRunner
	{
		public static void Run()
		{
			new CASIMADataProducer().QueryDataAndParseToXMLFile();
		}
	}
}
