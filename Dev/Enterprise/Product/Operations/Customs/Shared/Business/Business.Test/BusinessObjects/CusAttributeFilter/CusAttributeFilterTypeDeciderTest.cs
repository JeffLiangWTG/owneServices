using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusAttributeFilterTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		#region Overrides

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var attribute = bizO as CusAttributeFilter;
			if (attribute != null)
			{
				var partPivot = Factory.Load<BaseCusClassPartPivot>(attribute.BG_CI);

				if (partPivot != null)
				{
					((IBusinessObjectInternals)partPivot).Row[BaseCusClassPartPivot.Schema.CI_RN_NKCountry] = countryCode;
					partPivot.HasChanges = true;
					var classification = Factory.Load<BaseCusClassification>(partPivot.CI_CC);
					if (classification != null)
					{
						classification.CC_RN_NKCountryCode = countryCode;
					}
				}
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			var partPivot = Factory.New<BaseCusClassPartPivot>();
			partPivot.CI_OP = part.PK;
			partPivot.CI_CC = classification.PK;
			var attribute = (CusAttributeFilter)Factory.New(BaseTypeDecidedType);
			attribute.BG_CI = partPivot.PK;
			return attribute;
		}

		protected override Type BaseTypeDecidedType
		{
			get { return typeof(CusAttributeFilter); }
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusAttributeFilter>() }
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
				{ Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusAttributeFilter>() }
			};
		}

		#endregion
	}
}
