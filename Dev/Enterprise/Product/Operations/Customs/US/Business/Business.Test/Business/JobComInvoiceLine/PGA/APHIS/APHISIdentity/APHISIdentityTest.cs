using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISIdentity))]
	public class APHISIdentityTest : Customs.Business.Testing.CusCodeDataTest<APHISIdentity>
	{
		public void TestSetDefaultValues()
		{
			var identity = Factory.New<APHISIdentity>();
			AssertEquals(CusCodeDataTypeList.Codes.APHISIdentity, identity.CY_Type);
		}

		public void TestCloned()
		{
			var identity = Product.Identities.AddNew();
			identity.CY_Code = "A";
			identity.UseMultipleNumbers = true;
			var range = identity.NumberRanges.AddNew();
			range.US_StartNumber = "N1";
			range.US_EndNumber = "N9";
			range = identity.NumberRanges.AddNew();
			range.US_StartNumber = "P1";
			range.US_EndNumber = "P9";
			var clonedIdentity = (APHISIdentity)identity.Clone();
			AssertEquals("CY_Code", "A", clonedIdentity.CY_Code);
			AssertEquals("UseMultipleNumbers", true, clonedIdentity.UseMultipleNumbers);
			AssertEquals("clonedIdentity.NumberRanges.Count", 2, clonedIdentity.NumberRanges.Count);
			var clonedRange = clonedIdentity.NumberRanges[0];
			AssertEquals("clonedRange.US_StartNumber", "N1", clonedRange.US_StartNumber);
			AssertEquals("clonedRange.US_EndNumber", "N9", clonedRange.US_EndNumber);
			clonedRange = clonedIdentity.NumberRanges[1];
			AssertEquals("clonedRange.US_StartNumber", "P1", clonedRange.US_StartNumber);
			AssertEquals("clonedRange.US_EndNumber", "P9", clonedRange.US_EndNumber);
		}

		public void TestSettingUseMultipleNumbers()
		{
			var identity = Product.Identities.AddNew();
			identity.UseMultipleNumbers = false;
			identity.CY_Data = "N1";
			var range = identity.NumberRanges.AddNew();
			identity.UseMultipleNumbers = true;
			AssertEquals("CY_Data", ZString.Empty, identity.CY_Data);
			AssertEquals("IsDeleted", false, range.IsDeleted);
			identity.CY_Data = "N1";
			identity.UseMultipleNumbers = false;
			AssertEquals("CY_Data", "N1", identity.CY_Data);
			AssertEquals("IsDeleted", true, range.IsDeleted);
		}

		public void TestChildrenAreDeleted()
		{
			var identity = Product.Identities.AddNew();
			var range = identity.NumberRanges.AddNew();
			identity.Delete();
			AssertEquals("range.IsDeleted", true, range.IsDeleted);
		}

		public void TestIAPHISIdentityMembers()
		{
			var identity = Product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			identity.UseMultipleNumbers = false;
			identity.CY_Data = "CN3234";
			var numberRange = identity.NumberRanges.AddNew();
			numberRange.US_StartNumber = "N01";
			numberRange.US_EndNumber = "N10";
			numberRange = identity.NumberRanges.AddNew();
			numberRange.US_StartNumber = "P01";
			numberRange.US_EndNumber = "P10";
			IAPHISIdentity iIdentity = identity;
			AssertEquals("IdentityType", APHISItemIdentityNumberQualifierList.Codes.LAT, iIdentity.IdentityType);
			var ranges = iIdentity.Numbers.ToArray();
			AssertEquals("ranges", 1, ranges.Length);
			var range = ranges[0];
			AssertEquals("StartNumber", "CN3234", range.StartNumber);
			AssertEquals("EndNumber", "", range.EndNumber);
			identity.UseMultipleNumbers = true;
			identity.CY_Data = "CN3234";
			ranges = iIdentity.Numbers.ToArray();
			AssertEquals("ranges", 2, ranges.Length);
			range = ranges[0];
			AssertEquals("StartNumber", "N01", range.StartNumber);
			AssertEquals("EndNumber", "N10", range.EndNumber);
			range = ranges[1];
			AssertEquals("StartNumber", "P01", range.StartNumber);
			AssertEquals("EndNumber", "P10", range.EndNumber);
		}

		public void TestParent()
		{
			var identity = Product.Identities.AddNew();
			AssertEquals(Product, identity.Parent);
		}

		public void TestLookupsAndValidation()
		{
			var identity = Factory.New<APHISIdentity>();
			AssertEquals(typeof(APHISIdentityLookups), identity.Lookups.GetType());
			AssertEquals(typeof(APHISIdentityValidation), identity.Validation.GetType());
		}

		public void TestUseMultipleNumbersReadOnly()
		{
			var identity = Product.Identities.AddNew();
			AssertEquals(true, identity.UseMultipleNumbersInfo.ReadOnly);

			var header = InvoiceLine.APHISHeaders.AddNew();
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = header.Products.AddNew();
			identity = product.Identities.AddNew();
			AssertEquals(false, identity.UseMultipleNumbersInfo.ReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var product = aphisHeader.Products.AddNew();
			var identity = product.Identities.AddNew();
			identity.CY_Code = APHISItemIdentityNumberQualifierList.Codes.LAT;
			return identity;
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		APHISHeader Header
		{
			get
			{
				if (aphisHeader == null)
				{
					aphisHeader = InvoiceLine.APHISHeaders.AddNew();
					aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
				}
				return aphisHeader;
			}
		}
		APHISHeader aphisHeader;

		APHISProduct Product
		{
			get { return product ?? (product = Header.Products.AddNew()); }
		}
		APHISProduct product;

		#endregion
	}
}
