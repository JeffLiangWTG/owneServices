using System.Collections.Generic;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public interface ICodeListAttribute
	{
		string Name { get; }
		string Value { get; }
	}
}
