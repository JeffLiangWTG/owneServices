using System;
using System.Linq;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class Coot3CertificateOfOriginTypeRefCusCodeListAttributeParser : RefCusCodeListAttributeParser
	{
		public Coot3CertificateOfOriginTypeRefCusCodeListAttributeParser(string code)
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
				case Constants.CodeList.G:
				case Constants.CodeList.R:
				case Constants.CodeList.S:
				case Constants.CodeList.N:
					return new[] { Constants.AttributeCodeList.GEN, Constants.AttributeCodeList.WTO, Constants.AttributeCodeList.TEM };
				case Constants.CodeList.A:
				case Constants.CodeList.J:
				case Constants.CodeList.B:
				case Constants.CodeList.P:
				case Constants.CodeList.C:
				case Constants.CodeList.T:
				case Constants.CodeList.M:
					return new[] { Constants.AttributeCodeList.GSP, Constants.AttributeCodeList.LDC };
			}

			if (code.Length == 1 && code[0] <= '7' && code[0] >= '1')
			{
				return new[] { Constants.AttributeCodeList.EPA };
			}

			return Array.Empty<string>();
		}
	}
}
