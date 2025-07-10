using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class JobDeclarationLookupsAbstractTest<T, TParent> : BusinessObjectLookupsTestCase
		where T : JobDeclarationLookups
		where TParent : BaseJobDeclaration
	{
		public virtual void TestEntryStatusList()
		{
			AssertEntryStatusList_IntegratedAndABMInterface();
			AssertEntryStatusList_DefaultFallBack();
			AssertEntryStatusList_StatusListInZZ();
		}

		protected virtual void AssertEntryStatusList_IntegratedAndABMInterface()
		{
			SetCustomsWareRegistry(true);
			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			CombineAssertions(() =>
			{
				if (jobDeclaration.IsDeclarationIntegrated)
				{
					if (IntegratedCountryHelper.CustomsWareInstallations(jobDeclaration.CountryCode))
					{
						ClearCache();
						AssertEquals("CustomsWare codes", EntryStatusListForCustomsWareCodes, lookups.EntryStatusList.CodesAsString);
					}
					else
					{
						ClearCache();
						AssertNotEquals("Not CustomsWare country", EntryStatusListForCustomsWareCodes, lookups.EntryStatusList.CodesAsString);
					}
					SetCustomsWareRegistry(false);
					ClearCache();
					AssertNotEquals("Not CustomsWare codes", EntryStatusListForCustomsWareCodes, lookups.EntryStatusList.CodesAsString);
				}
				else
				{
					Assert("Not Integrated declaration no idea what list is returned both may be empty or different", true);
				}
			});
		}

		void AssertEntryStatusList_DefaultFallBack()
		{
			CombineAssertions(() =>
			{
				SetCustomsWareRegistry(true);
				jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				ClearCache();
				AssertEquals("Default Fall Back not integrated", EntryStatusListForDefaultFallBackCodes, lookups.EntryStatusList.CodesAsString);
				SetCustomsWareRegistry(false);
				jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				ClearCache();
				AssertEquals("Default Fall Back not ABM Interfaced", EntryStatusListForDefaultFallBackCodes, lookups.EntryStatusList.CodesAsString);
			});
		}

		void AssertEntryStatusList_StatusListInZZ()
		{
			CombineAssertions(() =>
			{
				RunAssertionsForEntryStatusList("SIT, SIU", jobDeclaration, lookups);

				var mockDeclaration = Factory.NewMoq<TParent>();
				mockDeclaration.Protected().Setup<ZString>("DefaultDataGroupingCore").Returns(alternativeDataGrouping);
				var lookupsForNewDeclaration = (T)Activator.CreateInstance(typeof(T), mockDeclaration.Object);
				RunAssertionsForEntryStatusList("XYZ", mockDeclaration.Object, lookupsForNewDeclaration);
			});
		}

		void RunAssertionsForEntryStatusList(ZString expectedStatusListCodes, BaseJobDeclaration declarationForAssertions, T lookupsForAssertion)
		{
			SetCustomsWareRegistry(true);
			declarationForAssertions.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			ClearCache();
			using (SetUpStatusListInZZ(EntryStatusListCodeType))
			{
				AssertEquals("ZZ RefList not integrate", expectedStatusListCodes, lookupsForAssertion.EntryStatusList.CodesAsString);
			}

			SetCustomsWareRegistry(false);
			declarationForAssertions.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			ClearCache();
			using (SetUpStatusListInZZ(EntryStatusListCodeType))
			{
				AssertEquals($"ZZ RefList not ABM Interfaced, by {EntryStatusListCodeType}", expectedStatusListCodes, lookupsForAssertion.EntryStatusList.CodesAsString);
			}
			ClearCache();
			using (SetUpStatusListInZZ(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatusForInterface))
			{
				AssertEquals("ZZ RefList not ABM Interfaced, by CSTI", expectedStatusListCodes, lookupsForAssertion.EntryStatusList.CodesAsString);
			}
		}

		IDisposable SetUpStatusListInZZ(ZString codeType)
		{
			if (!codeType.IsEmpty)
			{
				var dataGrouping = jobDeclaration.CountryCode;
				var cusCodeHelper = new UniversalReferenceTestDataHelper(Factory);
				var cusCodeType = cusCodeHelper.CreateNewOrGetExistingCusCodeType(codeType, "Entry Status List Type");
				var sit = cusCodeHelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "SIT", "SIT DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var siu = cusCodeHelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "SIU", "SIU DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var inv = cusCodeHelper.CreateNewOrGetExistingCusCodeList("INV", codeType, "SNL", "SNL DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var abc = cusCodeHelper.CreateNewOrGetExistingCusCodeList(alternativeDataGrouping, codeType, "XYZ", "XYZ DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.Save();

				return new DisposableAction(() =>
				{
					sit.Delete();
					siu.Delete();
					inv.Delete();
					cusCodeType.Delete();
					Factory.Save();
				});
			}

			return new DisposableAction(() => { });
		}

		void SetCustomsWareRegistry(bool enabled)
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			CustomsDataRegistry.Instance.CustomsWareCompany.SetValue(companyPK, Guid.Empty, Guid.Empty, enabled ? "TST" : string.Empty);
			var customsWareRegistry = ObjectFactory.Get<Integration.Customs.CustomsWare.ICustomsWareRegistry>();
			customsWareRegistry.CustomsWareSiteID.SetValue(companyPK, Guid.Empty, Guid.Empty, enabled ? "TSTSITE" : string.Empty);
			customsWareRegistry.UserName.SetValue(companyPK, Guid.Empty, Guid.Empty, enabled ? "TSTUN" : string.Empty);
			customsWareRegistry.Password.SetValue(companyPK, Guid.Empty, Guid.Empty, enabled ? "TSTPW" : string.Empty);
		}

		protected virtual ZString EntryStatusListCodeType => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus;

		protected virtual ZString EntryStatusListForCustomsWareCodes => ZString.Empty;

		protected virtual ZString EntryStatusListForDefaultFallBackCodes => ZString.Empty;

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<TParent>();
			lookups = (T)Activator.CreateInstance(typeof(T), jobDeclaration);
		}
		protected TParent jobDeclaration;
		protected T lookups;
		readonly ZString alternativeDataGrouping = "ABC";

		void ClearCache()
		{
			Factory.ClearCachedValue<CodeDescriptionPairList>(GetEntryStatusListKey(EntryStatusListCodeType));
			Factory.ClearCachedValue<CodeDescriptionPairList>(GetEntryStatusListKey(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatusForInterface));
		}

		ZString GetEntryStatusListKey(string codeType) => "ZZRefCusCodeList_" + Universal.RefCusCodeListTypes.GetKey(jobDeclaration.CountryCode, codeType, ZDateTime.Today.Date, ZString.Empty);
	}
}
