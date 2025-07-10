using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusMiscRequestLineTypeDecider : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var requestLine = (CusMiscRequestLine)bizO;
			requestLine.RequestHeader.Branch.Company.GC_RN_NKCountryCode = countryCode;
		}
		protected override Type BaseTypeDecidedType => typeof(CusMiscRequestLine);
		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var header = Factory.NewWithValidTestData<CusMiscRequestHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			header.CMR_MessageType = "5GW";//db constraint
			var line = header.RequestLines.AddNew();
			line.CML_EntryNumber = "1";
			line.CML_EntryType = "EXP";
			return line;
		}
		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();
		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusMiscRequestLine>() },
			};
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.ICusMiscRequestLine>() },
			};
		}
	}
}
