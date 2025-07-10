using System.Collections.Generic;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Dictionaries.DictionaryPuesc
{
	public interface IRefCusCodeListMapper
	{
		IReadOnlyList<RefCusCodeList> MapDictionaryElementsToRefData(Dictionaries dictionaries, XmlTextReader xmlDoc, DictionaryData dictionaryData);
	}
}
