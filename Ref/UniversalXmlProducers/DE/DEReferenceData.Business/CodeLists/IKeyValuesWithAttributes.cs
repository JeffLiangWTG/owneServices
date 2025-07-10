using System.Collections.Generic;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public interface IKeyValuesWithAttributes : IKeyValues
	{
		IList<KeyValueAttribute> Attributes { get; }
	}
}
