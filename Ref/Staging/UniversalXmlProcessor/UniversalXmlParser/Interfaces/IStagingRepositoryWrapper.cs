using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXmlParser.Models;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Interfaces
{
	public interface IStagingRepositoryWrapper
	{
		string CreateRecord(string entityType, string xmlContent, int xmlReaderLineNumber, Guid sourceId);
		bool CheckBulkInsert { get; }
		IEnumerable<XmlSchemaEntityRelationshipMap> SchemaRelationshipMapList { get; set; }
		Task<bool> ExecuteBulkInsertAsync();
		bool IsDataElement(string entityTypeName);
		void SetEntityTypeMetaData(IList<EntityTypeMetaData> entityTypeMetaDataCollection);
		Action BulkInsertExecuting { get; set; }
	}
}
