using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvLineRefsTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public override void TestGetTypeForLoad()
		{
			base.TestGetTypeForLoad();

			var noneTWDeclaration = Factory.New<BaseJobDeclaration>();
			var noneInvoiceLine = noneTWDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var jobComInvLineRefs2 = noneInvoiceLine.InvoiceLineRefs.AddNew();
			var typeDecider = new JobComInvLineRefsTypeDecider();
			AssertEquals(typeof(JobComInvLineRefs), typeDecider.GetTypeForLoad(((INeedRow)jobComInvLineRefs2).Row, Factory));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var twDeclaration = Factory.New<BaseJobDeclaration>();
				var twInvoiceLine = twDeclaration.Invoices.AddNew().InvoiceLines.AddNew();

				var comInvLineRefs = (IJobComInvLineRefsTypeSupporter)twInvoiceLine;
				var jobComInvLineRefsType = comInvLineRefs.GetJobComInvLineRefsTypes()[Common.TW.JobComInvLineRefsType.Codes.AssignedNumber];
				var jobComInvLineRefs = (JobComInvLineRefs)Factory.New(jobComInvLineRefsType);
				jobComInvLineRefs.JG_JI = twInvoiceLine.PK;

				AssertEquals(jobComInvLineRefsType, typeDecider.GetTypeForLoad(((INeedRow)jobComInvLineRefs).Row, Factory));
			}
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var lineRefs = bizO as JobComInvLineRefs;
			if (lineRefs != null)
			{
				lineRefs.InvoiceLine.InvoiceHeader.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.InvoiceLineRefs.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(JobComInvLineRefs);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Brazil, BaseTypeDecidedType },
				{ Core.Constants.CountryGuids.India, BaseTypeDecidedType },
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
				{ Core.Constants.CountryCodes.India, BaseTypeDecidedType },
				{ Core.Constants.CountryCodes._TemplateCountryName_, BaseTypeDecidedType }
			};
		}
	}
}
