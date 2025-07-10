using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartDataLoad))]
	sealed class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
	{
		public void TestImportingCountrySpecificFields()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var classification = Factory.New<CusClassification>();
				classification.CC_LookupCode = "POOLCUE";
				classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
				Factory.Save();
				var dataLoad = new OrgSupplierPartDataLoad();
				using (TempFile tempFile = TempFile.New())
				{
					using (StreamWriter sw = new StreamWriter(tempFile.Filename))
					{
						sw.WriteLine("Code   ,Description,UQ,Supplier,ClassificationLookup,ClassificationType,AdditionalTariffPart,AdditionalTariffItemCode");
						sw.WriteLine("PoolCue,Pool Cue   ,NO,ABIGAS  ,PoolCue             ,HTI               ,4P1                 ,40101020");
					}

					dataLoad.ImportProductData(tempFile.Filename, false, false);
					AssertEquals("Records Created", 1, dataLoad.RunCounters.RecsCreated);
					OrgHeader supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
					OrgSupplierPart product = (OrgSupplierPart)new OrgSupplierPart.Loader(Factory, typeof(OrgSupplierPart)).Load("PoolCue", null, supplier);
					AssertNotNull("Product", product);
					var exportPivot = product.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty);
					var importPivot = product.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
					AssertNull("Export Pivot", exportPivot);
					AssertNotNull("Import pivot", importPivot);
					var tariffDetail = importPivot.CusLineTariffDetails.OfType<CusLineTariffDetail>().First();
					AssertEquals("4P1", tariffDetail.BZ_Type);
					AssertEquals("40101020", tariffDetail.BZ_Tariff);
				}
			}
		}

		public void TestProductImportRelationship_UnitPrice_MultipleRecordsWithInvalidData_InvalidUnitPrice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var client = Helper.CreateClient("C1");
				var product1 = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
				product1.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
				var product2 = (OrgSupplierPart)Helper.CreateProduct(client, "P2");
				product2.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
				Factory.Save();
				var dataLoad = new OrgSupplierPartDataLoad();
				using (var testFileName = TempFile.New())
				{
					using (var sw = new StreamWriter(testFileName.Filename))
					{
						sw.WriteLine("Code,UQ,OWNER,UnitPrice,UnitPriceCurrency");
						sw.WriteLine("P1, UNT, C1, 2, AUD");
						sw.WriteLine("P2, UNT, C1, -2, AUD");
						sw.Flush();
					}

					AssertEquals("Precondtion", 0, dataLoad.Log.Count);
					dataLoad.ImportProductData(testFileName.Filename, true, false);
				}

				AssertEquals("Precondtion", 5, dataLoad.Log.Count);
				AssertEquals("Products to Import = 2", dataLoad.Log[0]);
				AssertEquals("PART NO: P1 - Part has been UPDATED", dataLoad.Log[1]);
				AssertEquals("The Unit Price of -2 is invalid. Valid value range for unit price is between 0 and 922337203685477.58. The unit price will not be imported.", dataLoad.Log[2]);
				AssertEquals("PART NO: P2 - Part has been UPDATED", dataLoad.Log[3]);
				AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 2, Products excluded = 0\r\n", dataLoad.Log[4]);
				var relation1InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product1.PK)).Single();
				AssertEquals("Should update.", 2m, relation1InAnotherFactory.OU_UnitPrice);
				AssertEquals("Should update.", "AUD", relation1InAnotherFactory.OU_RX_NKUnitPriceCurrency);
				var relation2InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product2.PK)).Single();
				AssertEquals("Should not update.", 0m, relation2InAnotherFactory.OU_UnitPrice);
				AssertEquals("Should not update.", "", relation2InAnotherFactory.OU_RX_NKUnitPriceCurrency);
			}
		}

		public void TestProductImportRelationship_UnitPrice_MultipleRecordsWithInvalidData_InvalidCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var client = Helper.CreateClient("C1");
				var product1 = (OrgSupplierPart)Helper.CreateProduct(client, "P1");
				product1.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
				var product2 = (OrgSupplierPart)Helper.CreateProduct(client, "P2");
				product2.RelatedOrganisations.FindByOrganisationPKAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
				Factory.Save();
				var dataLoad = new OrgSupplierPartDataLoad();
				using (var testFileName = TempFile.New())
				{
					using (var sw = new StreamWriter(testFileName.Filename))
					{
						sw.WriteLine("Code,UQ,OWNER,UnitPrice,UnitPriceCurrency");
						sw.WriteLine("P1, UNT, C1, 2, AUD");
						sw.WriteLine("P2, UNT, C1, 2, XXX");
						sw.Flush();
					}

					AssertEquals("Precondtion", 0, dataLoad.Log.Count);
					dataLoad.ImportProductData(testFileName.Filename, true, false);
				}

				AssertEquals("Precondtion", 5, dataLoad.Log.Count);
				AssertEquals("Products to Import = 2", dataLoad.Log[0]);
				AssertEquals("PART NO: P1 - Part has been UPDATED", dataLoad.Log[1]);
				AssertEquals("The Unit Price is invalid because 'XXX' is not a valid currency. The unit price will not be imported. Please enter a valid currency to import a unit price.", dataLoad.Log[2]);
				AssertEquals("PART NO: P2 - Part has been UPDATED", dataLoad.Log[3]);
				AssertEquals("\r\nT O T A L : Products created = 0, Products updated = 2, Products excluded = 0\r\n", dataLoad.Log[4]);
				var relation1InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product1.PK)).Single();
				AssertEquals("Should update.", 2m, relation1InAnotherFactory.OU_UnitPrice);
				AssertEquals("Should update.", "AUD", relation1InAnotherFactory.OU_RX_NKUnitPriceCurrency);
				var relation2InAnotherFactory = new BusinessObjectFactory().Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_OP, product2.PK)).Single();
				AssertEquals("Should not update.", 0m, relation2InAnotherFactory.OU_UnitPrice);
				AssertEquals("Should not update.", "", relation2InAnotherFactory.OU_RX_NKUnitPriceCurrency);
			}
		}

		protected override OrgSupplierPartDataLoad GetNewDataLoader() => new OrgSupplierPartDataLoad();

		IWhsTransactionTestHelper Helper => helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory));
		IWhsTransactionTestHelper helper;
	}
}
