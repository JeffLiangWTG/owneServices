using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.Testing
{
	class CusPermitRuleTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestGetTypeForLoadForUS()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.UnitedStates);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			BaseCusPermitRule permitRule;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitRule = GetNewBusinessObjectForLoadTest() as BaseCusPermitRule;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedPermitRule = otherFactory.Load<BaseCusPermitRule>(permitRule.PK);

			Assert(typeof(Integration.Customs.US.ICusPermitRule).IsAssignableFrom(loadedPermitRule.GetType()));
		}

		public void TestGetTypeForLoadForCA()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.Canada);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			BaseCusPermitRule permitRule;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitRule = GetNewBusinessObjectForLoadTest() as BaseCusPermitRule;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedPermitRule = otherFactory.Load<BaseCusPermitRule>(permitRule.PK);

			Assert(typeof(Integration.Customs.CA.ICusPermitRule).IsAssignableFrom(loadedPermitRule.GetType()));
		}

		public void TestGetTypeForLoadForZA()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.SouthAfrica);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			BaseCusPermitRule permitRule;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitRule = GetNewBusinessObjectForLoadTest() as BaseCusPermitRule;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedPermitRule = otherFactory.Load<BaseCusPermitRule>(permitRule.PK);

			Assert(typeof(Integration.Customs.ZA.ICusPermitRule).IsAssignableFrom(loadedPermitRule.GetType()));
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var permitRule = bizO as BaseCusPermitRule;
			if (permitRule != null)
			{
				var permitHeader = permitRule.PermitHeader;
				if (permitHeader != null)
				{
					permitHeader.CPH_RN_NKCountryCode = countryCode;
				}
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			return helper.CreatePermitRule(permitHeader, "TAR", "1", "2");
		}

		protected override Type BaseTypeDecidedType => typeof(BaseCusPermitRule);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusPermitRule>() },
				{ Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusPermitRule>() },
				{ Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusPermitRule>() },
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
				{ Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusPermitRule>() },
				{ Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusPermitRule>() },
				{ Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusPermitRule>() },
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new PermitTestDataHelper(Factory);
		}

		PermitTestDataHelper helper;
	}
}
