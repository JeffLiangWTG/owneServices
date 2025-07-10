
using CargoWise.Integration;

namespace Enterprise.Customs.NZ.Business
{
	class CodeDescriptionWithPrefix : ICodeDescription
	{
		public CodeDescriptionWithPrefix(ICodeDescription description, string prefix)
		{
			Description = $"{prefix} | {description.Description}";
			PK = description.PK;
			Code = description.Code;
		}

		public object PK { get; private set; }

		public string Code { get; private set; }

		public string Description { get; private set; }
	}
}
