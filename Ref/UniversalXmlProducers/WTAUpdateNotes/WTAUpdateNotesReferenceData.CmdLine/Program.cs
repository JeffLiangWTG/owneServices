using System;
using System.Threading.Tasks;
using WiseTechAcademy.UpdateNotesContract;

namespace CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.CmdLine
{
	public static class Program
	{
		public async static Task Main(string[] args)
		{
			if (args.Length != 1)
			{
				throw new ArgumentException("There should be a single argument that is 'full' or 'partial'.");
			}

			var runType = args[0];

			if (!Enum.TryParse(runType, ignoreCase:true, out UpdateRunType updateType))
			{
				throw new ArgumentException("Argument should be 'full' or 'partial'.");
			}

			var programService = new ProgramService(XmlGenerator.instance);
			await programService.Run(updateType);
		}
	}
}
