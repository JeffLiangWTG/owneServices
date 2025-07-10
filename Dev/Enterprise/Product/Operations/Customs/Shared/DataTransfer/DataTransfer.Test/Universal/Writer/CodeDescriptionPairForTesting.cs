using CargoWise.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	public sealed class CodeDescriptionPairForTesting : ICodeDescription
	{
		CodeDescriptionPairForTesting(string code, string description)
		{
			Code = code;
			Description = description;
		}

		public static ICodeDescription New(string code, string description, int maxLength = 0)
		{
			if (!string.IsNullOrEmpty(description) && maxLength > 0 && description.Length > maxLength)
			{
				description = description.Substring(0, maxLength);
			}

			return new CodeDescriptionPairForTesting(code, description);
		}

		public object PK { get; private set; }

		public string Code { get; private set; }

		public string Description { get; private set; }
	}
}
