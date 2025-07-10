using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.CusReconBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusReconEntryTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GetCountryCodeForReconDeclaration(row, factory);
			return GetTypeForCountryCode(countryCode);
		}

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusReconEntry);

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusReconEntry>),
			new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusReconEntry>)
		};

		static ZString GetCountryCodeForReconDeclaration(DataRow row, BusinessObjectFactory factory)
		{
			var branchPK = (row != null) ? new ZGuid(row[AutoCusReconEntry.Schema.CRE_GB_Branch]) : ZGuid.Invalid;
			var branch = branchPK.IsValid ? factory.Load<GlbBranch>(branchPK) : null;
			return branch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
