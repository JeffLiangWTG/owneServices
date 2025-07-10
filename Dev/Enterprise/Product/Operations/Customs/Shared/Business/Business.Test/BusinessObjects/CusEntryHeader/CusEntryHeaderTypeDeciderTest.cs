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
	class CusEntryHeaderTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cusEntryHeader = bizO as CusEntryHeader;
			if (cusEntryHeader != null)
			{
				cusEntryHeader.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			return declaration.CustomsEntryHeaders.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(CusEntryHeader);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusEntryHeader>() },
				{ Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusEntryHeader>() },
				{ Constants.CountryGuids.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeader>() },
				{ Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusEntryHeader>() },
				{ Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusEntryHeader>() },
				{ Constants.CountryGuids.China, ObjectFactory.GetType<Integration.Customs.CN.ICusEntryHeader>() },
				{ Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusEntryHeader>() },
				{ Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryHeader>() },
				{ Constants.CountryGuids.HongKong, typeof(CusEntryHeader) },
				{ Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusEntryHeader>() },
				{ Constants.CountryGuids.Israel, ObjectFactory.GetType<Integration.Customs.IL.ICusEntryHeader>() },
				{ Constants.CountryGuids.India, ObjectFactory.GetType<Integration.Customs.IN.ICusEntryHeader>() },
				{ Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryHeader>() },
				{ Constants.CountryGuids.Japan, ObjectFactory.GetType<Integration.Customs.JP.ICusEntryHeader>() },
				{ Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.ICusEntryHeader>() },
				{ Constants.CountryGuids.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeader>() },
				{ Constants.CountryGuids.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.ICusEntryHeader>() },
				{ Constants.CountryGuids.Mexico, ObjectFactory.GetType<Integration.Customs.MX.ICusEntryHeader>() },
				{ Constants.CountryGuids.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeader>() },
				{ Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryHeader>() },
				{ Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IFormalEntryCusEntryHeader>() },
				{ Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusEntryHeader>() },
				{ Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusEntryHeader>() },
				{ Constants.CountryGuids.Singapore, ObjectFactory.GetType<Integration.Customs.SG.ICusEntryHeader>() },
				{ Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryHeader>() },
				{ Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryHeader>() },
				{ Constants.CountryGuids.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeader>() },
				{ Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusEntryHeader>() },
				{ Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusEntryHeader>() },
				{ Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryHeader>() },
				{ Constants.CountryGuids.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.ICusEntryHeader>() },
				{ Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryHeader>() },
				{ Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusEntryHeader>() },
				{ Constants.CountryGuids._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.ICusEntryHeader>() },
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
				{ Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusEntryHeader>() },
				{ Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusEntryHeader>() },
				{ Constants.CountryCodes.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeader>() },
				{ Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusEntryHeader>() },
				{ Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusEntryHeader>() },
				{ Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.ICusEntryHeader>() },
				{ Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusEntryHeader>() },
				{ Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryHeader>() },
				{ Constants.CountryCodes.HongKong, typeof(CusEntryHeader) },
				{ Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusEntryHeader>() },
				{ Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.ICusEntryHeader>() },
				{ Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryHeader>() },
				{ Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.ICusEntryHeader>() },
				{ Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusEntryHeader>() },
				{ Constants.CountryCodes.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeader>() },
				{ Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.ICusEntryHeader>() },
				{ Constants.CountryCodes.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeader>() },
				{ Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryHeader>() },
				{ Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IFormalEntryCusEntryHeader>() },
				{ Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusEntryHeader>() },
				{ Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusEntryHeader>() },
				{ Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.ICusEntryHeader>() },
				{ Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryHeader>() },
				{ Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryHeader>() },
				{ Constants.CountryCodes.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryHeader>() },
				{ Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusEntryHeader>() },
				{ Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusEntryHeader>() },
				{ Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryHeader>() },
				{ Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.ICusEntryHeader>() },
				{ Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryHeader>() },
				{ Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusEntryHeader>() },
				{ Constants.CountryCodes._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.ICusEntryHeader>() },
			};
		}

		public void TestGetTypeForLoadForNZ()
		{
			var nZBranch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.NewZealand);
			Factory.Save();

			CusEntryHeader entryHeader;
			CusEntryHeader formalEntryHeader;
			CusEntryHeader completionEntryHeader;
			CusEntryHeader primaryIndustriesEntryHeader;
			CusEntryHeader originalEntryHeader;
			CusEntryHeader eCIWriteOffEntryHeader;
			CusEntryHeader eCIWriteOffManifestEntryHeader;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, nZBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				entryHeader = (CusEntryHeader)GetNewBusinessObjectForLoadTest();
				entryHeader.CH_MessageType = ZString.Empty;

				formalEntryHeader = (CusEntryHeader)GetNewBusinessObjectForLoadTest();
				formalEntryHeader.CH_MessageType = CusEntryHeader.EntryHeaderTypes.NZ.FormalEntry;

				completionEntryHeader = (CusEntryHeader)GetNewBusinessObjectForLoadTest();
				completionEntryHeader.CH_MessageType = CusEntryHeader.EntryHeaderTypes.NZ.Completion;

				primaryIndustriesEntryHeader = (CusEntryHeader)GetNewBusinessObjectForLoadTest();
				primaryIndustriesEntryHeader.CH_MessageType = CusEntryHeader.EntryHeaderTypes.NZ.PrimaryIndustries;

				originalEntryHeader = (CusEntryHeader)GetNewBusinessObjectForLoadTest();
				originalEntryHeader.CH_MessageType = CusEntryHeader.EntryHeaderTypes.NZ.Original;

				eCIWriteOffEntryHeader = (CusEntryHeader)GetNewBusinessObjectForLoadTest();
				eCIWriteOffEntryHeader.CH_MessageType = CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOff;

				eCIWriteOffManifestEntryHeader = (CusEntryHeader)GetNewBusinessObjectForLoadTest();
				eCIWriteOffManifestEntryHeader.CH_MessageType = CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest;

				Factory.Save();
			}

			var loadedBizO = new BusinessObjectFactory().Load(BaseTypeDecidedType, entryHeader.PK);
			AssertNotNull("Loaded BizO should not be null", loadedBizO);
			AssertEquals("DecidedType for NZ CusEntryHeader", ObjectFactory.GetType<Integration.Customs.NZ.IFormalEntryCusEntryHeader>(), loadedBizO.GetType());
			loadedBizO = null;

			loadedBizO = new BusinessObjectFactory().Load(BaseTypeDecidedType, formalEntryHeader.PK);
			AssertNotNull("Loaded BizO should not be null", loadedBizO);
			AssertEquals("DecidedType for NZ CusEntryHeader", ObjectFactory.GetType<Integration.Customs.NZ.IFormalEntryCusEntryHeader>(), loadedBizO.GetType());
			loadedBizO = null;

			loadedBizO = new BusinessObjectFactory().Load(BaseTypeDecidedType, completionEntryHeader.PK);
			AssertNotNull("Loaded BizO should not be null", loadedBizO);
			AssertEquals("DecidedType for NZ CusEntryHeader", ObjectFactory.GetType<Integration.Customs.NZ.ICompletionCusEntryHeader>(), loadedBizO.GetType());
			loadedBizO = null;

			loadedBizO = new BusinessObjectFactory().Load(BaseTypeDecidedType, primaryIndustriesEntryHeader.PK);
			AssertNotNull("Loaded BizO should not be null", loadedBizO);
			AssertEquals("DecidedType for NZ CusEntryHeader", ObjectFactory.GetType<Integration.Customs.NZ.IPrimaryIndustriesCusEntryHeader>(), loadedBizO.GetType());
			loadedBizO = null;

			loadedBizO = new BusinessObjectFactory().Load(BaseTypeDecidedType, originalEntryHeader.PK);
			AssertNotNull("Loaded BizO should not be null", loadedBizO);
			AssertEquals("DecidedType for NZ CusEntryHeader", ObjectFactory.GetType<Integration.Customs.NZ.IOriginalCusEntryHeader>(), loadedBizO.GetType());
			loadedBizO = null;

			loadedBizO = new BusinessObjectFactory().Load(BaseTypeDecidedType, eCIWriteOffEntryHeader.PK);
			AssertNotNull("Loaded BizO should not be null", loadedBizO);
			AssertEquals("DecidedType for NZ CusEntryHeader", ObjectFactory.GetType<Integration.Customs.NZ.IECIWriteOffCusEntryHeader>(), loadedBizO.GetType());
			loadedBizO = null;

			loadedBizO = new BusinessObjectFactory().Load(BaseTypeDecidedType, eCIWriteOffManifestEntryHeader.PK);
			AssertNotNull("Loaded BizO should not be null", loadedBizO);
			AssertEquals("DecidedType for NZ CusEntryHeader", ObjectFactory.GetType<Integration.Customs.NZ.IECIWriteOffManifestingCusEntryHeader>(), loadedBizO.GetType());
			loadedBizO = null;
		}

		public void TestGetTypeForLoadForEU()
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.Germany);
			branch.GB_OH_OrgProxy = EnvProxy.Instance.CurrentCompany.OrganisationPK;
			Factory.Save();

			CusEntryHeader entryHeader;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				entryHeader = (CusEntryHeader)GetNewBusinessObjectForLoadTest();
				entryHeader.CH_MessageType = ZString.Empty;
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedHeader = otherFactory.Load<CusEntryHeader>(entryHeader.PK);
			var loadedDeclaration = otherFactory.Load<BaseJobDeclaration>(entryHeader.CH_JE);

			Assert(typeof(Integration.Customs.EU.ICusEntryHeader).IsAssignableFrom(loadedHeader.GetType()));
			Assert(typeof(Integration.Customs.EU.IJobDeclaration).IsAssignableFrom(loadedDeclaration.GetType()));
		}
	}
}
