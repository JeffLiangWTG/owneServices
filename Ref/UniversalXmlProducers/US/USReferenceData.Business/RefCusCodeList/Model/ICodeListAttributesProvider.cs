using System.Collections.Generic;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public interface ICodeListAttributesProvider
	{
		IEnumerable<ICodeListAttribute> Attributes { get; }
	}
}
