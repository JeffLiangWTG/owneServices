namespace Enterprise.Customs.US.Business
{
	partial class CanadaProvinceTerritoryCodes
	{
		public static bool IsCanadianProvince(string code)
		{
			return code == Codes.XA ||
				code == Codes.XB ||
				code == Codes.XC ||
				code == Codes.XM ||
				code == Codes.XN ||
				code == Codes.XO ||
				code == Codes.XP ||
				code == Codes.XQ ||
				code == Codes.XS ||
				code == Codes.XT ||
				code == Codes.XV ||
				code == Codes.XW ||
				code == Codes.XY;
		}

		public static bool IsCanadianSoftwoodLumberRegion(string code)
		{
			return code == Codes.XA ||
				code == Codes.XC ||
				code == Codes.XD ||
				code == Codes.XE ||
				code == Codes.XM ||
				code == Codes.XO ||
				code == Codes.XS ||
				code == Codes.XQ;
		}

		public static string GetCodeFromNormalStateCode(string code)
		{
			switch (code)
			{
				case CanadaStatesList.Codes.AB:
					return CanadaProvinceTerritoryCodes.Codes.XA;
				case CanadaStatesList.Codes.NB:
					return CanadaProvinceTerritoryCodes.Codes.XB;
				case CanadaStatesList.Codes.BC:
					return CanadaProvinceTerritoryCodes.Codes.XC;
				case CanadaStatesList.Codes.MB:
					return CanadaProvinceTerritoryCodes.Codes.XM;
				case CanadaStatesList.Codes.NS:
					return CanadaProvinceTerritoryCodes.Codes.XN;
				case CanadaStatesList.Codes.ON:
					return CanadaProvinceTerritoryCodes.Codes.XO;
				case CanadaStatesList.Codes.PE:
					return CanadaProvinceTerritoryCodes.Codes.XP;
				case CanadaStatesList.Codes.QC:
					return CanadaProvinceTerritoryCodes.Codes.XQ;
				case CanadaStatesList.Codes.SK:
					return CanadaProvinceTerritoryCodes.Codes.XS;
				case CanadaStatesList.Codes.NT:
					return CanadaProvinceTerritoryCodes.Codes.XT;
				case CanadaStatesList.Codes.NU:
					return CanadaProvinceTerritoryCodes.Codes.XV;
				case CanadaStatesList.Codes.NL:
					return CanadaProvinceTerritoryCodes.Codes.XW;
				case CanadaStatesList.Codes.YT:
					return CanadaProvinceTerritoryCodes.Codes.XY;
				default:
					return "";
			}
		}
	}
}
