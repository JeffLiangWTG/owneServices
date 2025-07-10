using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackingListTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetPackingListCountryCode(row, factory));
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.TW.ICusPackingList>(); }),
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusPackingList);

		protected ZString GetPackingListCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var declarationPK = (row != null) ? new ZGuid(row[CusPackingList.Schema.CUL_JE]) : ZGuid.Invalid;
			var packingDec = (declarationPK.IsValid) ? factory.Load<BaseJobDeclaration>(declarationPK) : null;
			var countryCode = packingDec?.CountryCode ?? ZString.Empty;
			return !countryCode.IsEmpty ? countryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
