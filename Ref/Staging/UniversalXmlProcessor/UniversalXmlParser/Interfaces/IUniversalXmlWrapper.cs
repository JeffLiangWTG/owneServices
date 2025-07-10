using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Interfaces
{
	public interface IUniversalXmlWrapper
	{
		string UpdateType { get; set; }
		string InclusiveEndDate { get; set; }
		DateTime? PublicationTime { get; set; }
		string DataSource { get; set; }
		string ErrorMessage { get; set; }
		string SchemaXml { get; set; }
		string Metadata { get; set; }
		void AddUnknownElements(string xmlContent);
		bool IsValid(out string errorMessage);
		bool HasErrors();
		List<string> UnknownElements { get; }
		string ErrorXml { get; set; }
		string DependencyXml { get; set; }
	}
}
