using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.SG.Access.Business
{
	public class SGPackQuantityAndUnitConverter
	{
		public (ZDecimal Quantity, ZString UnitOfQuantity) GetCustomsPackDetails(BusinessObjectFactory factory, UnitConverter unitConverter, TariffView tariff, ZLong packLineQuantity, ZString packLineUnitOfQuantity)
		{
			var quantity = (ZDecimal)packLineQuantity;
			var customsUnitOfQuantityCodeList = factory.GetCachedValue<UnitOfQuantityCodeList>();
			var unitOfQuantity = customsUnitOfQuantityCodeList.ContainsCode(packLineUnitOfQuantity) ? packLineUnitOfQuantity : (ZString)UnitOfQuantityCodeList.Codes.NMB;
			var tariffUnitOfQuantity = tariff?.ZZ1_ZZ8_UQ1 ?? ZString.Empty;
			var unitOfQuantityToConvertTo = !tariffUnitOfQuantity.IsEmpty ? tariffUnitOfQuantity : packLineUnitOfQuantity;
			unitOfQuantityToConvertTo = customsUnitOfQuantityCodeList.ContainsCode(unitOfQuantityToConvertTo) ? unitOfQuantityToConvertTo : ConvertUnitOfQuantityUsingRefPack(factory, customsUnitOfQuantityCodeList, packLineUnitOfQuantity);
			if (unitConverter.Convertible(packLineUnitOfQuantity, unitOfQuantityToConvertTo))
			{
				quantity = unitConverter.Convert((ZDecimal)packLineQuantity, packLineUnitOfQuantity, unitOfQuantityToConvertTo);
				unitOfQuantity = unitOfQuantityToConvertTo;
			}
			else if (!tariffUnitOfQuantity.IsEmpty && customsUnitOfQuantityCodeList.ContainsCode(tariffUnitOfQuantity))
			{
				quantity = 0;
				unitOfQuantity = tariffUnitOfQuantity;
			}
			return (quantity, unitOfQuantity);
		}

		ZString ConvertUnitOfQuantityUsingRefPack(BusinessObjectFactory factory, UnitOfQuantityCodeList customsUnitOfQuantityCodeList, ZString packUQ)
		{
			if (!UnitOfQuantityKeys.TryGetValue(packUQ, out var result))
			{
				var refPacks = CusRefPacksHelper.LoadFilteredRefPacks(factory, Core.Constants.CountryCodes.Singapore, RPTypeList.Codes.GlobalManifestLine, packUQ);
				foreach (var refPack in refPacks)
				{
					if (customsUnitOfQuantityCodeList.ContainsCode(refPack.RP_CustomsPack))
					{
						result = refPack.RP_CustomsPack;
						break;
					}
				}
				UnitOfQuantityKeys[packUQ] = !result.IsEmpty ? result : (ZString)UnitOfQuantityCodeList.Codes.NMB;
			}
			return result;
		}

		Dictionary<string, ZString> UnitOfQuantityKeys => unitOfQuantityKeys ?? (unitOfQuantityKeys = new Dictionary<string, ZString>());

		Dictionary<string, ZString> unitOfQuantityKeys;
	}
}
