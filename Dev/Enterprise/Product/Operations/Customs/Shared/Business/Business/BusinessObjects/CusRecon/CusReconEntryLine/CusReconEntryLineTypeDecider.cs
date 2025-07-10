using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusReconEntryLineTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GetCountryCodeForReconDeclaration(row, factory);
			return GetTypeForCountryCode(countryCode);
		}

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusReconEntryLine);

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusReconEntryLine>),
			new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusReconEntryLine>)
		};

		static ZString GetCountryCodeForReconDeclaration(DataRow row, BusinessObjectFactory factory)
		{
			var cusReconEntryPK = (row != null) ? new ZGuid(row[AutoCusReconEntryLine.Schema.CRL_CRE]) : ZGuid.Invalid;
			var cusReconEntry = cusReconEntryPK.IsValid ? factory.Load<CusReconEntry>(cusReconEntryPK) : null;
			return cusReconEntry?.Branch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
