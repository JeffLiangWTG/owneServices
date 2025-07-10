using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.Business
{
	public class CusPackageJobTypeDecider : CountrySpecificTypeDecider, ICusPackageJobTypeDecider
	{
		public CusPackageJobTypeDecider()
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
				yield return new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, () => ObjectFactory.GetType<Integration.Customs.TW.ICusPackageJob>());
			}
		}

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusPackageJob);

		protected ZString GetPackageJobCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var packingPK = row != null ? new ZGuid(row[CusPackageJob.Schema.KJ_ParentID]) : ZGuid.Invalid;
			var packing = packingPK.IsValid ? factory.Load<CusPackingList>(packingPK) : null;
			var countryCode = packing?.Declaration?.CountryCode ?? ZString.Empty;
			return !countryCode.IsEmpty ? countryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
