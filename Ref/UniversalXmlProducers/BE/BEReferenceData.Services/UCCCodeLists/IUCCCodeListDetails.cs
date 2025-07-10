using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public interface IUCCCodeListDetails
	{
		string Domain { get; }

		string CodeType { get; }

		string CodeListType { get; }

		string DataSource { get; }

		List<(string attributeName, string attributeValue)> AttributeValues { get; }
	}
}
