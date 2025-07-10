using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using WiseTechAcademy.UpdateNotesContract;

namespace CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.CmdLine
{
	public interface IXmlGenerator : IDisposable
	{
		Task<IEnumerable<UpdateNoteData>> RunGenerator(UpdateRunType runType);
		Task<string> CreateToken();
		Task<string> CallApi(string jsonBody);
		void ManageXmlCreation(IEnumerable<UpdateNoteData> collection, UpdateRunType updateType);
		void CreateXml(IEnumerable<UpdateNoteData> collection, UpdateType updateType, DateTime pubTime);
		List<RefGlbReleaseNote> GetReleaseNotesFromUpdateNotes(IEnumerable<UpdateNoteData> collection, UpdateType updateType);
		void ExportToXml(XmlWriter xmlWriter, IEnumerable<RefGlbReleaseNote> collection, string filePath);
		XmlWriterConfiguration GetXmlWriterConfiguration();
		HttpClient CreateClient();
	}
}
