using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public interface IUCCExportCodeListDetail
	{
		string Domain { get; }

		string CodeType { get; }

		string CodeListType { get; }

		string DataSource { get; }

		string ExtraType { get; }

		IReadOnlyList<(string attributeName, string attributeValue)> AttributeValues { get; }

		string XmlDataItemForCode { get; } 
	}
}
