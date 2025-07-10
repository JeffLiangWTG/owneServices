using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISProductCollection))]
	public class APHISProductCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNewAndIsClearedWhenNot()
		{
			AssertAllowNewAndIsClearedWhenNot("", false);
			AssertAllowNewAndIsClearedWhenNot("%", false);
			var list = new APHISCategoryTypeCodeList();
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.LiveAnimals);
			AssertAllowNewAndIsClearedWhenNot(APHISCategoryTypeCodeList.Codes.LiveAnimals, true);
			list.RemoveCode(APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts);
			AssertAllowNewAndIsClearedWhenNot(APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts, true);
			foreach (ICodeDescription pair in list)
			{
				AssertAllowNewAndIsClearedWhenNot(pair.Code, false);
			}

			aphisHeader.US_CategoryType = APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts;
			aphisHeader.US_CategoryCode = APHIS.ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForResearchAndEvaluation;
			Assert(!aphisHeader.Products.AllowNew);

			aphisHeader.US_CategoryCode = APHIS.ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForSaleAndDistribution;
			Assert(!aphisHeader.Products.AllowNew);

			aphisHeader.US_CategoryCode = APHIS.ArticleCategory.AnimalProductsAndByProductsList.Codes.OrganismsAndVectors;
			Assert(!aphisHeader.Products.AllowNew);

			aphisHeader.US_CategoryCode = APHIS.ArticleCategory.AnimalProductsAndByProductsList.Codes.LaboratoryMammals;
			Assert(!aphisHeader.Products.AllowNew);

			aphisHeader.US_CategoryCode = APHIS.ArticleCategory.AnimalProductsAndByProductsList.Codes.Insects;
			Assert(!aphisHeader.Products.AllowNew);

			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.APQ;
			AssertAllowNewAndIsClearedWhenNot("", true);
			AssertAllowNewAndIsClearedWhenNot("%", true);
			AssertAllowNewAndIsClearedWhenNot(APHISCategoryTypeCodeList.Codes.LiveAnimals, true);
		}

		#region Implementation

		void AssertAllowNewAndIsClearedWhenNot(string categoryType, bool isAllowNew)
		{
			Header.US_CategoryType = "!";
			var products = Header.Products;
			var product = products.AddNew();
			Header.US_CategoryType = categoryType;
			AssertEquals("AllowNew", isAllowNew, products.AllowNew);
			AssertEquals("product.IsDeleted", true, product.IsDeleted);
			product = products.AddNew();
			Header.US_CategoryType = categoryType;
			AssertEquals("AllowNew", isAllowNew, products.AllowNew);
			AssertEquals("product.IsDeleted", false, product.IsDeleted);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APHISProductCollection(Header);
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

		#endregion
	}
}
