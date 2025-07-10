using CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries;

namespace CargoWise.RefDbRepo.PLReferenceData.CmdLine
{
	sealed class DictionariesProgram
	{
		public static bool Run() => DictionariesUniversalReferenceDataXmlGenerator.GenerateDictionariesUniversalReferenceData()
			& CusProcedureProgram.Run();
	}
}
