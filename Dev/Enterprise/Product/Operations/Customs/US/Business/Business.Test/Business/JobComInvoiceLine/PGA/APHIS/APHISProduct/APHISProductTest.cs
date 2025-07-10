using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using CommodityQualifier = Enterprise.Customs.US.Business.APHIS.CommodityQualifier;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISProduct))]
	public class APHISProductTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<APHISProduct>
	{
		public void TestAgeDetail()
		{
			var product = Header.Products.AddNew();
			product.US_Age = "1";
			AssertEquals("product.US_AgeRangeDescInfo.ReadOnly", false, product.US_AgeRangeDescInfo.ReadOnly);
			product.US_AgeRangeDesc = "10";
			product.US_Age = ZString.Empty;
			AssertEquals("product.US_AgeRangeDescInfo.ReadOnly", true, product.US_AgeRangeDescInfo.ReadOnly);
			AssertEquals("product.US_AgeRangeDesc", ZString.Empty, product.US_AgeRangeDesc);
		}

		public void DataIsDeletedOnSaving()
		{
			var header = InvoiceLine.APHISHeaders.AddNew();
			var product = header.Products.AddNew();
			product.US_Age = "A";
			Factory.Save();
			AssertEquals(false, product.IsDeleted);
			product.US_Age = ZString.Empty;
			Factory.Save();
			AssertEquals(true, product.IsDeleted);
			product = header.Products.AddNew();
			var identity = product.Identities.AddNew();
			identity.CY_Data = APHISItemIdentityNumberQualifierList.Codes.LAT;
			Factory.Save();
			AssertEquals(false, product.IsDeleted);
			product = header.Products.AddNew();
			Factory.Save();
			AssertEquals(true, product.IsDeleted);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var product = aphisHeader.Products.AddNew();
			ICusCodeDataTypeSupporter supporter = product;
			supporter.AssertType(typeof(APHISIdentity), CusCodeDataTypeList.Codes.APHISIdentity);
			supporter.AssertType(null, "ZZ!");

			var identity = product.Identities.AddNew();
			identity.CY_Data = "A";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<Customs.Business.CusCodeData>(identity.PK);
			AssertEquals(typeof(APHISIdentity), addInfo.GetType());
		}

		public void TestClone()
		{
			var aphisHeader = InvoiceLine.APHISHeaders.AddNew();
			aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var product = aphisHeader.Products.AddNew();
			product.US_Age = "A";
			var identity1 = product.Identities.AddNew();
			identity1.CY_Data = "A";
			var identity2 = product.Identities.AddNew();
			identity2.CY_Data = "Z";

			var clonedProduct = (APHISProduct)product.Clone();
			AssertEquals("clonedProduct.US_Age", "A", clonedProduct.US_Age);

			AssertEquals("clonedProduct.Identities.Count", 2, clonedProduct.Identities.Count);
			var clonedIdentity = clonedProduct.Identities[0];
			AssertEquals("clonedIdentity.CY_Data", "A", clonedIdentity.CY_Data);
			clonedIdentity = clonedProduct.Identities[1];
			AssertEquals("clonedIdentity.CY_Data", "Z", clonedIdentity.CY_Data);
		}

		public void TestIAPHISProductComponentMembers()
		{
			var product = Header.Products.AddNew();
			product.US_Age = "1";
			product.US_AgeRangeDesc = "1 DESC";
			product.US_BreedVariety = "2";
			product.US_Color = "3";
			product.US_Gender = "4";
			product.US_GeneralName = "5";
			product.US_GestationalAgeIfPregnant = "6";
			product.US_IsFertilizedPregnantGestating = "7";
			product.US_IsProtectedSpecies = "8";
			product.US_Origin = "9";
			product.US_SpecificName = "0";
			product.US_Type = "A";
			IAPHISProductComponent iProductComponent = product;
			AssertEquals("GeneralName", "5", iProductComponent.GeneralName);
			AssertEquals("Origin", "9", iProductComponent.Origin);
			AssertEquals("SpecificName", "0", iProductComponent.SpecificName);
			AssertCharacteristic(iProductComponent.Component, "A", CommodityQualifier.AnimalProductsAndByProductsList.Codes.SpeciesComposition, ZString.Empty);

			var characteristics = iProductComponent.Characteristics.ToArray();
			AssertEquals("characteristics.Length", 7, characteristics.Length);
			AssertCharacteristic(characteristics[0], "1", CommodityQualifier.LiveAnimalsList.Codes.Age, "1 DESC");
			AssertCharacteristic(characteristics[1], "2", CommodityQualifier.LiveAnimalsList.Codes.BreedVariety, ZString.Empty);
			AssertCharacteristic(characteristics[2], "3", CommodityQualifier.LiveAnimalsList.Codes.Color, ZString.Empty);
			AssertCharacteristic(characteristics[3], "4", CommodityQualifier.LiveAnimalsList.Codes.Gender, ZString.Empty);
			AssertCharacteristic(characteristics[4], "7", CommodityQualifier.LiveAnimalsList.Codes.FertilizedPregnantGestating, ZString.Empty);
			AssertCharacteristic(characteristics[5], "6", CommodityQualifier.LiveAnimalsList.Codes.GestationalAgeIfPregnant, ZString.Empty);
			AssertCharacteristic(characteristics[6], "8", CommodityQualifier.LiveAnimalsList.Codes.ProtectedSpecies, ZString.Empty);
		}

		#region Implementation

		void AssertCharacteristic(IAPHISCharacteristic characteristic, ZString qualifier, ZString code, ZString description)
		{
			AssertEquals("CommodityQualifierCode", code, characteristic.CommodityQualifierCode);
			AssertEquals("CommodityQualifierCode", qualifier, characteristic.CommodityCharacteristicQualifier);
			AssertEquals("CommodityCharacteristicDescription", description, characteristic.CommodityCharacteristicDescription);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			var product = aphisHeader.Products.AddNew();
			product.US_Age = "1";
			return product;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header.Products.AddNew();
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
