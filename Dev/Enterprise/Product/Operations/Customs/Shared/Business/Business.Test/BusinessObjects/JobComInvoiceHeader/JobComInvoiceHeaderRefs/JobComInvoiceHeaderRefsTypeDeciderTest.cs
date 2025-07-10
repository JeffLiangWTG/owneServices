using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class JobComInvoiceHeaderRefsTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestGetTypeForLoadForCN()
		{
			AssertLoadedTypeForCountry(Constants.CountryCodes.China, JobComInvoiceHeaderRefs.Constants.CTR, ObjectFactory.GetType<Integration.Customs.CN.IJobComInvoiceHeaderContract>());
		}

		public void TestGetTypeForLoadForCA()
		{
			AssertLoadedTypeForCountry(Constants.CountryCodes.Canada, JobComInvoiceHeaderRefs.Constants.CCN, ObjectFactory.GetType<Integration.Customs.CA.ICAJobComInvoiceHeaderCCNs>());
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var lineRefs = bizO as JobComInvoiceHeaderRefs;
			if (lineRefs != null)
			{
				lineRefs.InvoiceHeader.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var invoice = declaration.Invoices.AddNew();
			return invoice.InvoiceHeaderRefs.AddNew();
		}

		protected override Type BaseTypeDecidedType
		{
			get { return typeof(JobComInvoiceHeaderRefs); }
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Constants.CountryGuids.Brazil, BaseTypeDecidedType },
				{ Constants.CountryGuids.Taiwan, BaseTypeDecidedType },
				{ Constants.CountryGuids.India, BaseTypeDecidedType },
				{ Constants.CountryGuids._TemplateCountryName_, BaseTypeDecidedType }
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
				{ Constants.CountryCodes.Brazil, BaseTypeDecidedType },
				{ Constants.CountryCodes.Japan, BaseTypeDecidedType },
				{ Constants.CountryCodes.Taiwan, BaseTypeDecidedType },
				{ Constants.CountryCodes.India, BaseTypeDecidedType },
				{ Constants.CountryCodes._TemplateCountryName_, BaseTypeDecidedType }
			};
		}

		void AssertLoadedTypeForCountry(ZString countryCode, ZString refType, Type expectedType)
		{
			var branch = CustomsCountrySpecificTypeDeciderTestHelper.GetNewBranchForTesting(Factory, countryCode);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var invoiceheader = declaration.Invoices.AddNew();
			var reference = invoiceheader.InvoiceHeaderRefs.AddNew();
			reference.J2_ReferenceType = refType;

			var invoiceheader2 = declaration.Invoices.AddNew();
			var notRightRef = invoiceheader2.InvoiceHeaderRefs.AddNew();
			notRightRef.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			declaration.JE_GB = branch.PK;
			Factory.Save();

			var loadedBizO = new BusinessObjectFactory().Load(BaseTypeDecidedType, reference.PK);
			AssertNotNull("Loaded BizO should not be null", loadedBizO);
			AssertEquals("DecidedType for JobComInvoiceHeaderRefs", expectedType, loadedBizO.GetType());
			loadedBizO = new BusinessObjectFactory().Load(BaseTypeDecidedType, notRightRef.PK);
			AssertNotNull("Loaded BizO should not be null", loadedBizO);
			AssertEquals("DecidedType for CA JobComInvoiceHeaderRefs", ObjectFactory.GetType<Integration.Customs.Shared.IJobComInvoiceHeaderRefs>(), loadedBizO.GetType());
			loadedBizO = null;
		}
	}
}
