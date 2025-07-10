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
	public class CusGuaranteeRuleTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore
		{
			get { yield break; }
		}
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type type = null;
			var permitCountryCode = GetGuaranteeHeaderCountryCode(row, factory);

			if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(permitCountryCode)))
			{
				var decider = (CusGuaranteeRuleTypeDecider)TypeDecider.GetTypeDeciderFromType(ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeRule>());
				type = decider.GetTypeForCountryCode(permitCountryCode);
			}
			else
			{
				type = DefaultTypeForUnsupportedCountry;
			}
			return type;
		}

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeRule>();
		protected override Type DefaultTypeForUnsupportedCountry => ObjectFactory.GetType<Integration.Customs.ICusGuaranteeRule>();

		protected ZString GetGuaranteeHeaderCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var guaranteeHeaderPk = (row != null) ? new ZGuid(row[CusGuaranteeRule.Schema.CPR_CPH_PermitHeader]) : ZGuid.Empty;
			if (!guaranteeHeaderPk.IsEmpty)
			{
				var guaranteeHeader = factory.Load<BaseCusGuaranteeHeader>(guaranteeHeaderPk);
				if (guaranteeHeader != null && !guaranteeHeader.CPH_RN_NKCountryCode.IsEmpty)
				{
					countryCode = guaranteeHeader.CPH_RN_NKCountryCode;
				}
			}
			return countryCode;
		}
	}
}
