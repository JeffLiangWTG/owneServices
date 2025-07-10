using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public static class N5101HHelpers
	{
		public static ZString RegNoTypeToTypeCode(ZString regNoType)
		{
			var result = ZString.Empty;
			switch (regNoType)
			{
				case OrgCusCode.CodeTypes.VATCode:
					result = TW.Business.PartyIdentifierCodeList.Codes._58;
					break;
				case OrgCusCode.CodeTypes.PassportID:
					result = TW.Business.PartyIdentifierCodeList.Codes._53;
					break;
				case OrgCusCode.TaiwanCodeTypes.PID:
					result = TW.Business.PartyIdentifierCodeList.Codes._174;
					break;
			}
			return result;
		}

		public static IEnumerable<ZString> GetSeals(this AsycudaContainer container)
		{
			var seal1 = container.ACN_Seal1;
			var seal2 = container.ACN_Seal2;
			var seal3 = container.ACN_Seal3;

			if (!seal1.IsEmpty)
			{
				yield return seal1;
			}

			if (!seal2.IsEmpty)
			{
				yield return seal2;
			}

			if (!seal3.IsEmpty)
			{
				yield return seal3;
			}
		}
	}
}
