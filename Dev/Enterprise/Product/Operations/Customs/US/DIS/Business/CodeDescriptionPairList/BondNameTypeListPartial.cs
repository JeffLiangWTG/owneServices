using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business
{
	partial class BondNameTypeList
	{
		public static BondNameType GetBondNameType(string code)
		{
			var result = BondNameType.None;

			if (!string.IsNullOrEmpty(code))
			{
				switch (code)
				{
					case BondNameTypeList.Codes.SingleBond:
						result = BondNameType.Single;
						break;
					case BondNameTypeList.Codes.ISFBond:
						result = BondNameType.ISFBond;
						break;
					default:
						result = BondNameType.Other;
						break;
				}
			}
			return result;
		}

		public static string GetCodeFrom(BondNameType nameType)
		{
			var result = "";

			if (nameType != BondNameType.None)
			{
				switch (nameType)
				{
					case BondNameType.Single:
						result = Codes.SingleBond;
						break;
					case BondNameType.ISFBond:
						result = Codes.ISFBond;
						break;
					default:
						result = Codes.Other;
						break;
				}
			}
			return result;
		}
	}
}
