using CargoWise.EntityFramework;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISIdentityCollection))]
	public class APHISIdentityCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<APHISIdentity>
	{
		public void TestAllowNewAndIsClearedWhenNot()
		{
			AssertAllowNewAndIsClearedWhenNot("", false);
			AssertAllowNewAndIsClearedWhenNot("%", false);
			var list = new APHISCategoryTypeCodeList();
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.LiveAnimals);
			AssertAllowNewAndIsClearedWhenNot(APHISCategoryTypeCodeList.Codes.LiveAnimals, true);
			foreach (ICodeDescription pair in list)
			{
				AssertAllowNewAndIsClearedWhenNot(pair.Code, false);
			}
		}

		#region Implementation

		void AssertAllowNewAndIsClearedWhenNot(string categoryType, bool isAllowNew)
		{
			Header.US_CategoryType = categoryType;
			var product = Header.Products.AddNew();
			var identities = product.Identities;
			var identity = identities.AddNew();
			AssertEquals("AllowNew", isAllowNew, identities.AllowNew);
		}

		protected override Customs.Business.CusCodeDataCollection<APHISIdentity> GetCusCodeDataCollection()
		{
			return new APHISIdentityCollection(Product);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<APHISIdentity>();
			result.Parent = Product;
			return result;
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
			get
			{
				if (product == null)
				{
					product = Header.Products.AddNew();
				}
				return product;
			}
		}
		APHISProduct product;

		#endregion
	}
}
