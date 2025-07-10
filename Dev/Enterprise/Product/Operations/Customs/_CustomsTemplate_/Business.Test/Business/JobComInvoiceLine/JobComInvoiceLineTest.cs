using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Customs.Business.IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes._TemplateCountryName_, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestPartType()
		{
			var factory2 = new BusinessObjectFactory();
			var importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<Integration.Customs.AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			var customsTemplate_Company = Factory.New<GlbCompany>();
			customsTemplate_Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes._TemplateCountryName_;
			var customsTemplate_Branch = customsTemplate_Company.Branches.AddNew();
			customsTemplate_Branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes._TemplateCountryName_)).RL_Code;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = customsTemplate_Branch.PK;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertType<OrgSupplierPart>("Product type gets changed depending on who is requesting", invoiceLine.Part);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			var factory3 = new BusinessObjectFactory();
			var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertType<OrgSupplierPart>("product type still the type", declarationLoaded.InvoiceLines[0].Part);
		}

		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<JobComInvoiceLine>("Update Customs.Business.BaseJobComInvoiceLine to include a decider for this class", Factory.New(typeof(Customs.Business.BaseJobComInvoiceLine)));
		}

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
	}
}
