using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusEntryLineTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestTypeDeciderForNZ()
		{
			var nZBranch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, Constants.CountryCodes.NewZealand);
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, nZBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "123234112";
				var entryLine = entryHeader.MergedLines.AddNew();
				Factory.Save();

				var secondFactory = new BusinessObjectFactory();
				var loadedEntryLine = secondFactory.Load<CusEntryLine>(entryLine.PK);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.NZ.ICusEntryLine>(), loadedEntryLine.GetType());

				entryHeader = (CusEntryHeader)Factory.New<Integration.Customs.NZ.IECIWriteOffCusEntryHeader>();
				entryHeader.CH_JE = declaration.PK;
				entryHeader.EntryNumber = "89034998";

				entryLine = entryHeader.MergedLines.AddNew();
				Factory.Save();

				loadedEntryLine = secondFactory.Load<CusEntryLine>(entryLine.PK);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.NZ.ICusEntryLine>(), loadedEntryLine.GetType());

				entryHeader = (CusEntryHeader)Factory.New<Integration.Customs.NZ.ICompletionCusEntryHeader>();
				entryHeader.CH_JE = declaration.PK;
				entryHeader.EntryNumber = "38929459";
				entryLine = entryHeader.MergedLines.AddNew();
				Factory.Save();

				loadedEntryLine = secondFactory.Load<CusEntryLine>(entryLine.PK);
				AssertEquals(ObjectFactory.GetType<Integration.Customs.NZ.ICusEntryLine>(), loadedEntryLine.GetType());
			}
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cusEntryLine = bizO as CusEntryLine;
			if (cusEntryLine != null)
			{
				cusEntryLine.Header.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return entryHeader.MergedLines.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(CusEntryLine);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Constants.CountryGuids.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusEntryLine>() },
				{ Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusEntryLine>() },
				{ Constants.CountryGuids.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryLine>() },
				{ Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusEntryLine>() },
				{ Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusEntryLine>() },
				{ Constants.CountryGuids.China, ObjectFactory.GetType<Integration.Customs.CN.ICusEntryLine>() },
				{ Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusEntryLine>() },
				{ Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryLine>() },
				{ Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusEntryLine>() },
				{ Constants.CountryGuids.Israel, ObjectFactory.GetType<Integration.Customs.IL.ICusEntryLine>() },
				{ Constants.CountryGuids.India, ObjectFactory.GetType<Integration.Customs.IN.ICusEntryLine>() },
				{ Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryLine>() },
				{ Constants.CountryGuids.Japan, ObjectFactory.GetType<Integration.Customs.JP.ICusEntryLine>() },
				{ Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.ICusEntryLine>() },
				{ Constants.CountryGuids.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryLine>() },
				{ Constants.CountryGuids.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.ICusEntryLine>() },
				{ Constants.CountryGuids.Mexico, ObjectFactory.GetType<Integration.Customs.MX.ICusEntryLine>() },
				{ Constants.CountryGuids.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryLine>() },
				{ Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryLine>() },
				{ Constants.CountryGuids.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.ICusEntryLine>() },
				{ Constants.CountryGuids.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusEntryLine>() },
				{ Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusEntryLine>() },
				{ Constants.CountryGuids.Singapore, ObjectFactory.GetType<Integration.Customs.SG.ICusEntryLine>() },
				{ Constants.CountryGuids.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryLine>() },
				{ Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryLine>() },
				{ Constants.CountryGuids.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryLine>() },
				{ Constants.CountryGuids.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusEntryLine>() },
				{ Constants.CountryGuids.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusEntryLine>() },
				{ Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryLine>() },
				{ Constants.CountryGuids.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.ICusEntryLine>() },
				{ Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryLine>() },
				{ Constants.CountryGuids.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusEntryLine>() },
				{ Constants.CountryGuids._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.ICusEntryLine>() },
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
				{ Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusEntryLine>() },
				{ Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusEntryLine>() },
				{ Constants.CountryCodes.Botswana, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryLine>() },
				{ Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusEntryLine>() },
				{ Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusEntryLine>() },
				{ Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.ICusEntryLine>() },
				{ Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusEntryLine>() },
				{ Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryLine>() },
				{ Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusEntryLine>() },
				{ Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.IL.ICusEntryLine>() },
				{ Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.ICusEntryLine>() },
				{ Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryLine>() },
				{ Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.ICusEntryLine>() },
				{ Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusEntryLine>() },
				{ Constants.CountryCodes.Lesotho, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryLine>() },
				{ Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.ICusEntryLine>() },
				{ Constants.CountryCodes.Namibia, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryLine>() },
				{ Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryLine>() },
				{ Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.ICusEntryLine>() },
				{ Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusEntryLine>() },
				{ Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusEntryLine>() },
				{ Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.ICusEntryLine>() },
				{ Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryLine>() },
				{ Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryLine>() },
				{ Constants.CountryCodes.Swaziland, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryLine>() },
				{ Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusEntryLine>() },
				{ Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusEntryLine>() },
				{ Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryLine>() },
				{ Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.ICusEntryLine>() },
				{ Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryLine>() },
				{ Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.ICusEntryLine>() },
				{ Constants.CountryCodes._TemplateCountryName_, ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.ICusEntryLine>() },
			};
		}
	}
}
