using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.Testing
{
	class CusPermitHeaderTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestGetTypeForLoadForGuarantee()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.SouthAfrica);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			BaseCusPermitHeader permitHeader;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitHeader = GetNewBusinessObjectForLoadTest() as BaseCusPermitHeader;
				permitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedPermitHeader = otherFactory.Load<SharedCusPermitHeader>(permitHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.IBaseCusGuaranteeHeader>();

			AssertType("ZA guarantee should be of Customs.BaseCusGuaranteeHeader type", expectedType, loadedPermitHeader);
		}

		public void TestCorrectLoadUsingCommonCusPermitHeader()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.SouthAfrica);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			BaseCusPermitHeader permitHeader;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitHeader = GetNewBusinessObjectForLoadTest() as BaseCusPermitHeader;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedPermitHeader = otherFactory.Load<CommonCusPermitHeader>(permitHeader.PK);

			Assert(typeof(Integration.Customs.ZA.ICusPermitHeader).IsAssignableFrom(loadedPermitHeader.GetType()));
		}

		public void TestGetTypeForLoadForZA()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.SouthAfrica);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			BaseCusPermitHeader permitHeader;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitHeader = GetNewBusinessObjectForLoadTest() as BaseCusPermitHeader;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedPermitHeader = otherFactory.Load<BaseCusPermitHeader>(permitHeader.PK);

			Assert(typeof(Integration.Customs.ZA.ICusPermitHeader).IsAssignableFrom(loadedPermitHeader.GetType()));
		}
		public void TestGetTypeForLoadForUS()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.UnitedStates);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			BaseCusPermitHeader permitHeader;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitHeader = GetNewBusinessObjectForLoadTest() as BaseCusPermitHeader;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedPermitHeader = otherFactory.Load<BaseCusPermitHeader>(permitHeader.PK);

			Assert(typeof(Integration.Customs.US.ICusPermitHeader).IsAssignableFrom(loadedPermitHeader.GetType()));
		}
		public void TestGetTypeForLoadForCA()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.Canada);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			BaseCusPermitHeader permitHeader;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitHeader = GetNewBusinessObjectForLoadTest() as BaseCusPermitHeader;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedPermitHeader = otherFactory.Load<BaseCusPermitHeader>(permitHeader.PK);

			Assert(typeof(Integration.Customs.CA.ICusPermitHeader).IsAssignableFrom(loadedPermitHeader.GetType()));
		}
		public void TestGetTypeForLoadForNZ()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.NewZealand);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			BaseCusPermitHeader permitHeader;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitHeader = GetNewBusinessObjectForLoadTest() as BaseCusPermitHeader;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedPermitHeader = otherFactory.Load<SharedCusPermitHeader>(permitHeader.PK);

			Assert(typeof(Integration.Customs.IBaseCusPermitHeader).IsAssignableFrom(loadedPermitHeader.GetType()));

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitHeader = GetNewBusinessObjectForLoadTest() as BaseCusPermitHeader;
				permitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;

				Factory.Save();
			}

			otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			loadedPermitHeader = otherFactory.Load<SharedCusPermitHeader>(permitHeader.PK);

			Assert(typeof(Integration.Customs.IBaseCusGuaranteeHeader).IsAssignableFrom(loadedPermitHeader.GetType()));
		}

		public void TestGetTypeForLoadForCH()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.Switzerland);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			BaseCusPermitHeader permitHeader;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				permitHeader = GetNewBusinessObjectForLoadTest() as BaseCusPermitHeader;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedPermitHeader = otherFactory.Load<BaseCusPermitHeader>(permitHeader.PK);

			Assert(typeof(Integration.Customs.CH.ICusPermitHeader).IsAssignableFrom(loadedPermitHeader.GetType()));
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var permitHeader = bizO as BaseCusPermitHeader;
			if (permitHeader != null)
			{
				permitHeader.CPH_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest() => Factory.NewWithValidTestData<BaseCusPermitHeader>();

		protected override Type BaseTypeDecidedType => typeof(BaseCusPermitHeader);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusPermitHeader>() },
				{ Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryGuids.FrenchGuiana, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryGuids.Guadeloupe, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryGuids.Martinique, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryGuids.Mayotte, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryGuids.Reunion, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryGuids.SaintMartin, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryGuids.SaintBarthelemy, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusPermitHeader>() },
				{ Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusPermitHeader>() },
				{ Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusPermitHeader>() },
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
				{ Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusPermitHeader>() },
				{ Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryCodes.FrenchGuyana, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryCodes.Guadeloupe, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryCodes.Martinique, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryCodes.Mayotte, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryCodes.Reunion, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryCodes.SaintMartin, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryCodes.SaintBarthelemy, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.EU.ICusPermitHeader>() },
				{ Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusPermitHeader>() },
				{ Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusPermitHeader>() },
				{ Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusPermitHeader>() },
			};
		}
	}
}
