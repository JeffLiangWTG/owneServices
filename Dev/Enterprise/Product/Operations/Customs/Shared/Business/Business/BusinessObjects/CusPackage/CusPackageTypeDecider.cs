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
	public class CusPackageTypeDecider : CountrySpecificTypeDecider, ICusPackageTypeDecider
	{
		public CusPackageTypeDecider()
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
				yield return new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, () => ObjectFactory.GetType<Integration.Customs.TW.ICusPackage>());
			}
		}

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusPackage);

		protected ZString GetPackageJobCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var packageJobPK = row != null ? new ZGuid(row[CusPackage.Schema.KP_KJ_ParentPackageJob]) : ZGuid.Invalid;
			var packageJob = packageJobPK.IsValid ? factory.Load<CusPackageJob>(packageJobPK) : null;
			var countryCode = packageJob?.PackingList?.Declaration?.CountryCode ?? ZString.Empty;
			return !countryCode.IsEmpty ? countryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
