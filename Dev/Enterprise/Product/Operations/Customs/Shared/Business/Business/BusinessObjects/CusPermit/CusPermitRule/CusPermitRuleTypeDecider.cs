using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusPermitRuleTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.ICusPermitRule>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Integration.Customs.ZA.ICusPermitRule>(); }),
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.ICusPermitRule>(); }),
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseCusPermitRule);

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetPermitHeaderCountryCode(row, factory));

		protected ZString GetPermitHeaderCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var permitHeaderPK = (row != null) ? new ZGuid(row[BaseCusPermitRule.Schema.CPR_CPH_PermitHeader]) : ZGuid.Empty;
			var permitHeader = factory.Load<BaseCusPermitHeader>(permitHeaderPK);
			var permitCountryCode = (permitHeader != null) ? permitHeader.CPH_RN_NKCountryCode : ZString.Empty;
			return (!permitCountryCode.IsEmpty) ? permitCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
