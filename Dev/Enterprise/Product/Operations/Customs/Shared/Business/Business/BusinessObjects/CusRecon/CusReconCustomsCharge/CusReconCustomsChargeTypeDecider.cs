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
	public class CusReconCustomsChargeTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GetCountryCodeForReconDeclaration(row, factory);
			return GetTypeForCountryCode(countryCode);
		}

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusReconCustomsCharge);

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusReconCustomsCharge>)
		};

		static ZString GetCountryCodeForReconDeclaration(DataRow row, BusinessObjectFactory factory)
		{
			var cusReconEntryLinePK = (row != null) ? new ZGuid(row[AutoCusReconCustomsCharge.Schema.CRC_CRL_Line]) : ZGuid.Invalid;
			var cusReconEntryLine = cusReconEntryLinePK.IsValid ? factory.Load<CusReconEntryLine>(cusReconEntryLinePK) : null;
			var cusReconEntry = cusReconEntryLine?.CRL_CRE.IsValid ?? false ? factory.Load<CusReconEntry>(cusReconEntryLine.CRL_CRE) : null;
			return cusReconEntry?.Branch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
