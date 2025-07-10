using System;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Business;

namespace CargoWise.RefDbRepo.NLReferenceData.CmdLine
{
	class ECCNCodeListProgram
	{
		public static void Run()
		{
			var errorCollector = new StringBuilder();

			var processManagerECCNCodeList = new ECCNCodeListProcessManager(errorCollector);
			processManagerECCNCodeList.RunBuilder();

			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred: \r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
