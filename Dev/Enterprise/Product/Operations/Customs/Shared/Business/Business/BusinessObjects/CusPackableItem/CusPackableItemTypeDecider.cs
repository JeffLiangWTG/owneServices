using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackableItemTypeDecider : CountrySpecificTypeDecider
	{
		public CusPackableItemTypeDecider()
		{
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetPackageJobCountryCode(row, factory));
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore
		{
			get
			{
				yield return new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, () => ObjectFactory.GetType<Integration.Customs.TW.ICusPackableItem>());
			}
		}

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusPackableItem);

		protected ZString GetPackageJobCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var packingPK = row != null ? new ZGuid(row[CusPackableItem.Schema.CUI_CUL]) : ZGuid.Invalid;
			var packing = packingPK.IsValid ? factory.Load<CusPackingList>(packingPK) : null;
			var countryCode = packing?.Declaration?.CountryCode ?? ZString.Empty;
			return !countryCode.IsEmpty ? countryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
