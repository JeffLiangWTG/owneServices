using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCusCodeValidityTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override Type BaseTypeDecidedType => typeof(OrgCusCodeValidity);

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CustomsRegNo = "213";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "I have events";
			org.OH_RL_NKClosestPort = "AU";
			org.CustomsCodes.Add(orgCusCode);
			Factory.Save();

			var validity = Factory.New<OrgCusCodeValidity>();
			validity.OCV_OK_OrgCusCode = orgCusCode.PK;
			return validity;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgCusCodeValidity>() }
			};
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			SetCountryCode(countryCode);
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgCusCodeValidity>() }
			};
		}
	}
}
