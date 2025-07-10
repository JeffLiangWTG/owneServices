using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging.CUSCAR;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public class CusCarSeal : ICusCarSeal
	{
		public CusCarSeal(BusinessObjectFactory factory, ZString sealNumber, ZString sealingPartyCode, ZString sealType)
		{
			this.factory = Argument.NotNull(factory, "factory cannot be null");
			this.sealNumber = sealNumber;
			this.sealingPartyCode = sealingPartyCode;
			this.sealType = sealType;
		}
		readonly BusinessObjectFactory factory;
		readonly ZString sealNumber;
		readonly ZString sealingPartyCode;
		readonly ZString sealType;

		ZString ICusCarSeal.SealNumber => sealNumber;
		ZString ICusCarSeal.SealingParty => GetCountryMappingForCode(RefCusMapTypeList.Codes.STYPE).GetValueSafe(sealingPartyCode);
		ZString ICusCarSeal.SealType => GetCountryMappingForCode(RefCusMapTypeList.Codes.MSELT).GetValueSafe(sealType);

		Dictionary<ZString, ZString> GetCountryMappingForCode(ZString codeType)
		{
			var mapping = ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(factory, Core.Constants.CountryCodes.SouthAfrica, codeType, ZDateTime.Now);
			if (mapping.Count == 0)
			{
				mapping = ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, codeType, ZDateTime.Now);
			}
			return mapping;
		}
	}
}
