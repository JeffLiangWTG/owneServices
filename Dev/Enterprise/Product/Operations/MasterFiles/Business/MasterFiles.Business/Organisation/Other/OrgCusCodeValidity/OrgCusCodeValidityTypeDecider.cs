using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCusCodeValidityTypeDecider : CountrySpecificTypeDecider
	{
		public OrgCusCodeValidityTypeDecider()
		{
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var orgCusCodePK = new ZGuid(row[OrgCusCodeValidity.Schema.OCV_OK_OrgCusCode]);
			var orgCusCode = factory.Load<OrgCusCode>(orgCusCodePK);
			var countryCode = orgCusCode?.OK_RN_NKCodeCountry ?? ZString.Empty;
			return GetTypeForCountryCode(countryCode);
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgCusCodeValidity>(); })
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(OrgCusCodeValidity);
	}
}
