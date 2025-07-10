using System.Threading.Tasks;
using WiseTechAcademy.UpdateNotesContract;

namespace CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.CmdLine
{
	public class ProgramService(IXmlGenerator xmlGenerator)
	{
		public IXmlGenerator XmlGenerator => xmlGenerator;

		public async Task Run(UpdateRunType runType)
		{
			var collection = await xmlGenerator.RunGenerator(runType);
			xmlGenerator.ManageXmlCreation(collection, runType);
		}
	}
}
