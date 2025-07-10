using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	sealed class JobComInvoiceLineTest : EU.Business.Declaration.Testing.JobComInvoiceLineTest<JobComInvoiceLine>
	{
		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be SE", Core.Constants.CountryCodes.Sweden, InvoiceLine.CustomsCountryCode);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.Sweden, partDetails.CustomsCountryCode);
				AssertEquals(typeof(MasterFiles.OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestLookups_Import()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobComInvoiceLineLookups>(InvoiceLine.Lookups);
		}

		public void TestLookups_Export()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobComInvoiceLineLookups>(InvoiceLine.Lookups);
		}

		public void TestLookups_MiscellaneousCustoms()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobComInvoiceLineLookups>(InvoiceLine.Lookups);
		}

		public void TestValidation_Import()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobComInvoiceLineValidation>(InvoiceLine.Validation);
		}

		public void TestValidation_Export()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobComInvoiceLineValidation>(InvoiceLine.Validation);
		}

		public void TestValidation_MiscellaneousCustoms()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobComInvoiceLineValidation>(InvoiceLine.Validation);
		}

		public void TestAddInfo()
		{
			AssertType<AddInfoJobComInvoiceLine>(InvoiceLine.AddInfo);
		}

		protected override Type GetExpectedPartType() => typeof(MasterFiles.OrgSupplierPart);

		protected override Type GetExpectedEntryInstructionType() => typeof(CusEntryInstruction);

		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		new JobComInvoiceLine InvoiceLine => base.InvoiceLine;

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		}
	}
}
