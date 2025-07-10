using System.Linq;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class Coot1CertificateOfOriginTypeRefCusCodeListAttributeParser : RefCusCodeListAttributeParser
	{
		public Coot1CertificateOfOriginTypeRefCusCodeListAttributeParser(string code)
		{
			this.code = code;
		}

		readonly string code;

		protected override RefCusCodeListAttributeParserConfig[] GetConfigsCore()
		{
			var configCodes = GetConfigCodes();
			return configCodes.Select(c => new CertificateOfOriginTypePreferencesRefCusCodeListAttributeParserConfig(c)).ToArray();
		}

		string[] GetConfigCodes()
		{
			switch (code)
			{
				case Constants.CodeList.WK:
					return new[] { Constants.AttributeCodeList.GEN, Constants.AttributeCodeList.WTO, Constants.AttributeCodeList.TEM };

				case Constants.CodeList.GS:
					return new[] { Constants.AttributeCodeList.GSP, Constants.AttributeCodeList.LDC };

				default:
					return new[] { Constants.AttributeCodeList.EPA };
			}
		}
	}
}
