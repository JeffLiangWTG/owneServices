using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class JobDecRefsTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => Array.Empty<CountrySpecificType>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(JobDecRefs);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GetCountryCode(row, factory);
			return GetTypeForCountryCode(countryCode);
		}

		ZString GetCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var declarationPK = (row != null) ? new ZGuid(row[JobDecRefs.Schema.J3_JE]) : ZGuid.Invalid;
			var declaration = declarationPK.IsValid ? factory.Load<BaseJobDeclaration>(declarationPK) : null;
			return declaration != null ? declaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
