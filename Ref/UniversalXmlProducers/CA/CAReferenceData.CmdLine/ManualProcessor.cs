using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.ManualProcessor;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	public static class ManualProcessor
	{
		public static void Run()
		{
			var errorBuilder = new StringBuilder();

			var producers = GetProducers(errorBuilder);
			Parallel.Invoke(producers.Select(x => new Action(() => x.QueryDataAndParseToXMLFile())).ToArray());

			if (errorBuilder.Length > 0)
			{
				Program.PrintErrorMessage(errorBuilder.ToString());
			}
		}

		static IEnumerable<IProducer> GetProducers(StringBuilder errorBuilder)
		{
			yield return new CACDocumentTypesProducer(errorBuilder);
		}
	}
}
