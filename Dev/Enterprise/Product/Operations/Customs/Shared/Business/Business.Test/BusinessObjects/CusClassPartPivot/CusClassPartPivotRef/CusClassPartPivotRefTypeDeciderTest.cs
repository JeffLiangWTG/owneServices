using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusClassPartPivotRefTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public override void TestGetTypeForLoad()
		{
			base.TestGetTypeForLoad();

			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var cusClassPartPivotRef = pivot.CusClassPartPivotRefs.AddNew();
			var typeDecider = new CusClassPartPivotRefTypeDecider();
			AssertEquals(typeof(CusClassPartPivotRef), typeDecider.GetTypeForLoad(((INeedRow)cusClassPartPivotRef).Row, Factory));
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var pivotRefs = bizO as CusClassPartPivotRef;
			if (pivotRefs != null)
			{
				((IBusinessObjectInternals)pivotRefs.CusClassPartPivot).Row[BaseCusClassPartPivot.Schema.CI_RN_NKCountry] = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "111";
			var partPivot = part.PivotsForBinding.AddNew();
			var partPivotRef = partPivot.CusClassPartPivotRefs.AddNew();
			partPivotRef.CIR_ReferenceType = "AGA";
			partPivotRef.CIR_ReferenceNumber = "REF";
			return partPivotRef;
		}

		protected override Type BaseTypeDecidedType => typeof(CusClassPartPivotRef);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Brazil, BaseTypeDecidedType },
				{ Core.Constants.CountryGuids._TemplateCountryName_, BaseTypeDecidedType }
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Brazil, BaseTypeDecidedType },
				{ Core.Constants.CountryCodes.Japan, BaseTypeDecidedType },
				{ Core.Constants.CountryCodes._TemplateCountryName_, BaseTypeDecidedType }
			};
		}
	}
}
