using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class CodeDescriptionList : Dictionary<string, string>
	{
		public string GetCodeFromDescription(string description)
		{
			return this.FirstOrDefault(x => x.Value == description).Key ?? string.Empty;
		}
	}
}
