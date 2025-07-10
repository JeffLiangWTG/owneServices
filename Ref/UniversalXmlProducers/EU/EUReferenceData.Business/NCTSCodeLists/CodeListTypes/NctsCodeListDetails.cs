using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public abstract class NctsCodeListDetails
	{
		public virtual string ExtraType => null;
		public virtual IReadOnlyList<(string attributeName, string attributeValue)> AttributeValues => new List<(string, string)> { };
	}
}
