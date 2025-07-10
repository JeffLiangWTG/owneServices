using System.Text;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	public static class CAGSTCodeRunner
	{
		public static void Run()
		{
			var errorBuilder = new StringBuilder();

			var producer = new CARMGSTCodeProducer(new WebServiceCaller(errorBuilder), errorBuilder);
			producer.QueryDataAndParseToXMLFile();
			if (errorBuilder.Length > 0)
			{
				Program.PrintErrorMessage(errorBuilder.ToString());
			}
		}
	}
}
