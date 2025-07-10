using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseSupplementaryCode))]
	sealed class BaseSupplementaryCodeTest : CusCodeDataWithOrderAbstractTest<BaseSupplementaryCode>
	{
		public void TestCY_Code_Attributes() => CombineAssertions(() =>
			AssertEntity<BaseSupplementaryCode>()
				.HasProperty(s => s.CY_Code)
				.WithCaption("Supplementary Code")
				.WithAttribute<MaxLengthAttribute>(m => m.MaxLength == 35)
		);

		public void TestProvider()
		{
			var parent = Factory.New<JobDeclarationWithSupplementaryCodeSupport>();
			var code = Loader.LoadOrCreate<BaseSupplementaryCode, JobDeclarationWithSupplementaryCodeSupport>(parent, 1);
			AssertNotNull("Provider", code.Provider);

			var codeWithNoParent = Factory.New<BaseSupplementaryCode>();
			AssertNotNull("Even though SupplementaryCode has not parent, Provider should never be null", codeWithNoParent.Provider);
		}

		public void TestValidation()
		{
			var code = Factory.New<BaseSupplementaryCode>();
			AssertType<BaseSupplementaryCodeValidation>(code.Validation);
		}

		public void TestLookups()
		{
			var code = Factory.New<BaseSupplementaryCode>();
			AssertType<BaseSupplementaryCodeLookups>(code.Lookups);
		}

		public void TestPropertyChangedNotifier()
		{
			var code = Factory.New<BaseSupplementaryCode>();
			AssertType<BaseSupplementaryCodePropertyChangedNotifier>(code.PropertyChangedNotifier);
		}

		protected override string ExpectedCusCodeDataType => BaseCusCodeDataTypeList.Codes.SupplementaryCode;

		protected override IEnumerable<BaseSupplementaryCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var parent = factory.New<JobDeclarationWithSupplementaryCodeSupport>();
			var codeLoader = new BaseSupplementaryCode.Loader(factory);
			var code = codeLoader.LoadOrCreate<BaseSupplementaryCodeForTest, JobDeclarationWithSupplementaryCodeSupport>(parent, 1);
			code.CY_Code = "CD1";
			yield return code;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var code = factory.NewWithValidTestData<BaseSupplementaryCode>();
			code.CY_Code = BaseSupplementaryCode.CodeDataType;
			return code;
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, BaseSupplementaryCode bizObj)
		{
			factory.Load<JobDeclarationWithSupplementaryCodeSupport>(bizObj.CY_ParentID);
		}

		BaseSupplementaryCode.Loader Loader => loader ?? new BaseSupplementaryCode.Loader(Factory);
		readonly BaseSupplementaryCode.Loader loader;
	}

	public sealed class JobDeclarationWithSupplementaryCodeSupport : BaseJobDeclaration,
			ISupplementaryCodeSupporter,
			ICusCodeDataTypeSupporter,
			ICusSupportingInfoTypeSupporter
	{
		public JobDeclarationWithSupplementaryCodeSupport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		TariffView ICusCodeDataWithOrderSupporter.Tariff => null;

		IZZRateSelectionCriteria ICusCodeDataWithOrderSupporter.RateSelectionCriteria => Mock.Of<IZZRateSelectionCriteria>();

		ZString ICusCodeDataWithOrderSupporter.GetCountryCodeForCodeProvider() => GlbCompany.CurrentCompany.Country.Code;

		CodeDescriptionPairList ICusCodeDataWithOrderSupporter.CachedListOfAdditionalCodeDescriptions => new ();

		void ICusCodeDataWithOrderSupporter.OnCodesChanged() { }

		IEnumerable<BaseSupplementaryCode> ISupplementaryCodeSupporter.SupplementaryCodes => Array.Empty<BaseSupplementaryCode>();

		ZString ISupplementaryCodeSupporter.SupplementaryCodesFieldType => ZString.Empty;

		ICusCodeDataCollection<BaseSupplementaryCode> ISupplementaryCodeSupporter.AdditionalSupplementaryCodes => AdditionalSupplementaryCodes;
		public ICusCodeDataCollection<BaseSupplementaryCode> AdditionalSupplementaryCodes => additionalSupplementaryCodes ??= new CusCodeDataCollection<BaseSupplementaryCode>(this, "SUP");
		ICusCodeDataCollection<BaseSupplementaryCode> additionalSupplementaryCodes;

		ResourceStringData ISupplementaryCodeSupporter.SupplementaryCodeCaption => null;

		ZString ISupplementaryCodeSupporter.GetCountryCodeFromAdditionalCode(ZString additionalCode) => GlbCompany.CurrentCompany.Country.Code;

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>();

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(BaseCusCodeDataTypeList.Codes.SupplementaryCode, typeof(BaseSupplementaryCode));
			return result;
		}
	}

	sealed class BaseSupplementaryCodeForTest : BaseSupplementaryCode
	{
		public BaseSupplementaryCodeForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected internal override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobDeclarationWithSupplementaryCodeSupport)); }
		}
	}
}
