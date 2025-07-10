using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusPermitHeaderTypeDecider : CountrySpecificTypeDecider, Integration.Customs.ICusPermitHeaderTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.ICusPermitHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Integration.Customs.ZA.ICusPermitHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, delegate { return ObjectFactory.GetType<Integration.Customs.CH.ICusPermitHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, delegate { return ObjectFactory.GetType<Integration.Customs.GB.ICusPermitHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.ICusPermitHeader>(); }),
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseCusPermitHeader);

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var applicationCode = (row != null) ? new ZString(row[BaseCusPermitHeader.Schema.CPH_ApplicationCode]) : ZString.Empty;
			var permitCountryCode = GetPermitHeaderCountryCode(row);

			Type type = null;

			switch (applicationCode)
			{
				case CusPermitHeaderApplicationCodeList.Codes.Guarantee:
					type = BaseCusGuaranteeHeader.TypeDecider.GetTypeForLoad(row, factory);
					break;

				case CusPermitHeaderApplicationCodeList.Codes.Permit:
				default:
					if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(permitCountryCode)))
					{
						var decider = (CusPermitHeaderTypeDecider)TypeDecider.GetTypeDeciderFromType(ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>());
						type = decider.GetTypeForCountryCode(permitCountryCode);
					}
					break;
			}

			return type ?? GetTypeForCountryCode(permitCountryCode);
		}

		protected ZString GetPermitHeaderCountryCode(System.Data.DataRow row)
		{
			var permitCountryCode = (row != null) ? new ZString(row[BaseCusPermitHeader.Schema.CPH_RN_NKCountryCode]) : ZString.Empty;
			return (!permitCountryCode.IsEmpty) ? permitCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>();
	}
}
