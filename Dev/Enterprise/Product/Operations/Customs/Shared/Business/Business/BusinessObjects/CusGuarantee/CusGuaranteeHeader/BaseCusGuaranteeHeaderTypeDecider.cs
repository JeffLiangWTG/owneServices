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
	public class BaseCusGuaranteeHeaderTypeDecider : CountrySpecificTypeDecider, Integration.Customs.ICusGuaranteeHeaderTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore
		{
			get { yield break; }
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type type = null;
			var permitCountryCode = GetGuaranteeHeaderCountryCode(row);

			if (permitCountryCode == Core.Constants.CountryCodes.Switzerland)
			{
				type = DefaultTypeForEuCountry;
			}
			else if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(permitCountryCode)))
			{
				var decider = (BaseCusGuaranteeHeaderTypeDecider)TypeDecider.GetTypeDeciderFromType(ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>());
				type = decider.GetTypeForCountryCode(permitCountryCode);
			}
			else if (ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(permitCountryCode))
			{
				type = ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusGuaranteeHeader>();
			}
			else
			{
				type = DefaultTypeForUnsupportedCountry;
			}
			return type;
		}

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>();
		protected override Type DefaultTypeForUnsupportedCountry => ObjectFactory.GetType<Integration.Customs.IBaseCusGuaranteeHeader>();

		protected ZString GetGuaranteeHeaderCountryCode(DataRow row)
		{
			var permitCountryCode = (row != null) ? new ZString(row[BaseCusPermitHeader.Schema.CPH_RN_NKCountryCode]) : ZString.Empty;
			return (!permitCountryCode.IsEmpty) ? permitCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
