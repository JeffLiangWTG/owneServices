using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeader))]
	sealed class DocJobComInvoiceHeaderTest : DocBaseJobComInvoiceHeaderAbstractTest<JobComInvoiceHeader, DocJobComInvoiceHeader>
	{
		#region Overrides

		public override void TestIncoTermDescription()
		{
			InvoiceHeaderInternal.JZ_IncoTerm = "EXW";
			Assert("Inco term should be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);

			InvoiceHeaderInternal.JZ_IncoTerm = "CIF";
			Assert("Inco term should be not be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);
		}

		public override void TestConversionFactorIsWrapped()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, InvoiceHeaderWrapperInternal.ConversionFactor);
		}

		public new void TestOSParty()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			OrgHeader importer = OrgHeader.New(Factory);
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_OH_Buyer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeCodeForExport;
			DocJobComInvoiceHeader docHeaderExport = CreateInvoiceHeaderWrapper((JobComInvoiceHeader)invoiceHeader);
			Enterprise.DocumentWrappers.DocBaseWrapper intermediateWrapper = (Enterprise.DocumentWrappers.DocBaseWrapper)docHeaderExport.OSParty.WrappedObject;
			AssertEquals("DocHeader.OSParty = Importer", importer.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);

			declaration.JE_MessageType = JobMessageTypeCodeForImport;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			DocJobComInvoiceHeader docHeaderImport = CreateInvoiceHeaderWrapper((JobComInvoiceHeader)invoiceHeader);
			intermediateWrapper = (Enterprise.DocumentWrappers.DocBaseWrapper)docHeaderImport.OSParty.WrappedObject;
			AssertEquals("DocHeader.OSParty = Supplier", supplier.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);
		}

		#endregion

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.UnitedStates; }
		}

		protected override DocJobComInvoiceHeader CreateInvoiceHeaderWrapper(JobComInvoiceHeader invoiceHeaderInternal)
		{
			return DocJobComInvoiceHeader.New(invoiceHeaderInternal, Factory);
		}

		#endregion
	}
}
