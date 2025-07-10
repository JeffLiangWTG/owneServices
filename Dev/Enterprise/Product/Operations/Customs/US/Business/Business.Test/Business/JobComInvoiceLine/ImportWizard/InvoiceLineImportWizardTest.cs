using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceLineImportWizard))]
	public class InvoiceLineImportWizardTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportManufacturerWithInvalidFileColumnIndex()
		{
			USCustomsDataRegistry.Instance.CreateMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "ORG";
			header.OH_FullName = "org name";
			header.MainAddress.OA_Address1 = "address1";
			header.MainAddress.CustomsCodes.AddNew("MID", "MIDCODE", "US");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();

			var collection = new InvoiceLineViewCollection(declaration);
			var wizard = new InvoiceLineImportWizardForTest();
			wizard.Mapping[0].AddFileColumnIndex(0);
			wizard.Mapping[1].AddFileColumnIndex(1);
			wizard.Mapping[2].AddFileColumnIndex(3);
			wizard.fakeFileContent.Add(new[] { "", "", "MIDCODE" });
			wizard.fakeFileContent.Add(new[] { "", "", "USTEMPCODE" });
			wizard.fakeFileContent.Add(new[] { "", "", "USTEMPCODE" });

			UnitTestUserNotification.Instance.AddYesAnswer();
			UnitTestUserNotification.Instance.AddYesAnswer();
			wizard.ImportIntoCollection(collection);

			var newHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "AUTO CREATED FROM MID"));
			AssertEquals("No OrgHeader created.", 0, newHeaders.Length);
		}

		public void TestImportManufacturer()
		{
			USCustomsDataRegistry.Instance.CreateMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "ORG";
			header.OH_FullName = "org name";
			header.MainAddress.OA_Address1 = "address1";
			header.MainAddress.CustomsCodes.AddNew("MID", "MIDCODE", "US");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();

			var collection = new InvoiceLineViewCollection(declaration);
			var wizard = new InvoiceLineImportWizardForTest();
			wizard.Mapping[0].AddFileColumnIndex(0);
			wizard.Mapping[1].AddFileColumnIndex(1);
			wizard.Mapping[2].AddFileColumnIndex(2);
			wizard.fakeFileContent.Add(new[] { "", "", "MIDCODE" });
			wizard.fakeFileContent.Add(new[] { "", "", "USTEMPCODE" });
			wizard.fakeFileContent.Add(new[] { "", "", "USTEMPCODE" });

			UnitTestUserNotification.Instance.AddYesAnswer();
			UnitTestUserNotification.Instance.AddYesAnswer();
			wizard.ImportIntoCollection(collection);

			var newHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "AUTO CREATED FROM MID"));
			AssertEquals("Should be only 1 OrgHeader created.", 1, newHeaders.Length);
			var newHeader = newHeaders[0];
			AssertImportResult(collection[0], header.PK, header.MainAddress.PK);
			AssertImportResult(collection[1], newHeader.PK, newHeader.MainAddress.PK);
		}

		public void TestUpdateDetailsForCombinedLineAfterImport()
		{
			var newFactory = new BusinessObjectFactory();
			var org1 = newFactory.LoadTop1<OrgHeader>(new ZQuery());
			var part = newFactory.New<OrgSupplierPart>();
			part.OP_PartNum = "Test1";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Owner);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9201000010";
			tariff.UE_Unit1 = "T";
			tariff.UE_Unit2 = "T";
			tariff.UE_Unit3 = "T";
			tariff.UE_ShortDescription = "What a lovely day!!";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			CusClassification classification = newFactory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = classification.PK;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_SupplementalTariff = "99038804";
			var attrib1 = pivot.Attributes1.AddNew();
			attrib1.BG_AttributeValue1 = "pivot1";

			var childPivot1 = pivot.Children.AddNew();
			childPivot1.CI_TariffNum = "9201000010";
			childPivot1.CI_SupplementalTariff = "99038801";
			childPivot1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			newFactory.Save();

			var childPivot2 = pivot.Children.AddNew();
			childPivot2.CI_SupplementalTariff = "99038802";
			childPivot2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();

			var collection = new InvoiceLineViewCollection(declaration);
			var wizard = new InvoiceLineImportWizardForTest();

			wizard.Mapping[0].AddFileColumnIndex(0);
			wizard.Mapping[1].AddFileColumnIndex(1);
			wizard.Mapping[2].AddFileColumnIndex(2);
			wizard.Mapping[3].AddFileColumnIndex(3);
			wizard.Mapping[4].AddFileColumnIndex(4);
			wizard.Mapping[5].AddFileColumnIndex(5);
			wizard.Mapping[6].AddFileColumnIndex(6);
			wizard.Mapping[7].AddFileColumnIndex(7);
			wizard.Mapping[8].AddFileColumnIndex(8);
			wizard.Mapping[9].AddFileColumnIndex(9);
			wizard.Mapping[10].AddFileColumnIndex(10);
			wizard.Mapping[11].AddFileColumnIndex(11);
			wizard.Mapping[12].AddFileColumnIndex(12);
			wizard.Mapping[13].AddFileColumnIndex(13);
			wizard.Mapping[14].AddFileColumnIndex(14);
			wizard.Mapping[15].AddFileColumnIndex(15);
			wizard.Mapping[16].AddFileColumnIndex(16);
			wizard.Mapping[17].AddFileColumnIndex(17);
			wizard.fakeFileContent.Add(new[] { "", "", "", "pivot1", "Test1", "1000", "KG", "2000", "KG", "204", "KG", "196", "KG", "180", "KG", "181", "KG", "182" });
			wizard.fakeFileContent.Add(new[] { "", "", "", "pivot1", "Test1", "1000", "KG", "2000", "KG", "204", "KG", "196", "", "180", "", "181", "", "182" });

			UnitTestUserNotification.Instance.AddYesAnswer();
			UnitTestUserNotification.Instance.AddYesAnswer();
			wizard.ImportIntoCollection(collection);
			AssertEquals(6, collection.Count);
			CombineAssertions(() =>
			{
				AssertImportResultAfterAddChild(collection[0], "", "9903.88.04", 0m, "", 0m, "", 0m, "", 0m, "", 0m, "", 0m, "", 0m);
				AssertImportResultAfterAddChild(collection[1], "9201.00.0010", "9903.88.01", 1000m, "KG", 2000m, "KG", 204m, "KG", 196m, "KG", 180m, "KG", 181m, "KG", 182m);
				AssertImportResultAfterAddChild(collection[2], "", "9903.88.02", 0m, "NO", 0m, "", 0m, "", 0m, "", 0m, "", 0m, "", 0m);
				AssertImportResultAfterAddChild(collection[3], "", "9903.88.04", 0m, "", 0m, "", 0m, "", 0m, "", 0m, "", 0m, "", 0m);
				AssertImportResultAfterAddChild(collection[4], "9201.00.0010", "9903.88.01", 1000m, "KG", 2000m, "KG", 204m, "KG", 196m, "T", 180m, "T", 181m, "T", 182m);
				AssertImportResultAfterAddChild(collection[5], "", "9903.88.02", 0m, "NO", 0m, "", 0m, "", 0m, "", 0m, "", 0m, "", 0m);
			});
		}

		void AssertImportResultAfterAddChild(JobComInvoiceLine invoiceLine, ZString tariff, ZString supTariff, ZDecimal linePrice, ZString invoiceUQ, ZDecimal invoiceQuantity, ZString weightUQ, ZDecimal weight, ZString netWeightUQ, ZDecimal netWeight, ZString firstCustomsQty, ZDecimal firstCustomsUQ, ZString secondCustomQty, ZDecimal secondCustomsUQ, ZString thirdCustomsQty, ZDecimal thirdCustomsUQ)
		{
			var prefix = invoiceLine.HumanReadableShortcutName;
			AssertEquals(prefix + "JI_FormattedTariff", tariff, invoiceLine.JI_FormattedTariff);
			AssertEquals(prefix + "SupTariffFormatted", supTariff, invoiceLine.SupTariffFormatted);
			AssertEquals(prefix + "JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
			AssertEquals(prefix + "JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
			AssertEquals(prefix + "JI_InvoiceQuantity", invoiceQuantity, invoiceLine.JI_InvoiceQuantity);
			AssertEquals(prefix + "JI_WeightUQ", weightUQ, invoiceLine.JI_WeightUQ);
			AssertEquals(prefix + "JI_Weight", weight, invoiceLine.JI_Weight);
			AssertEquals(prefix + "JI_NetWeight", netWeight, invoiceLine.JI_NetWeight);
			AssertEquals(prefix + "JI_NetWeightUQ", netWeightUQ, invoiceLine.JI_NetWeightUQ);
			AssertEquals(prefix + "JI_CustomsUnitQty", firstCustomsQty, invoiceLine.JI_CustomsUnitQty);
			AssertEquals(prefix + "JI_CustomsQuantity", firstCustomsUQ, invoiceLine.JI_CustomsQuantity);
			AssertEquals(prefix + "JI_CustomsSecondUnitQty", secondCustomQty, invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals(prefix + "JI_CustomsSecondQuantity)", secondCustomsUQ, invoiceLine.JI_CustomsSecondQuantity);
			AssertEquals(prefix + "JI_CustomsThirdUnitQty", thirdCustomsQty, invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(prefix + "JI_CustomsThirdQuantity", thirdCustomsUQ, invoiceLine.JI_CustomsThirdQuantity);
		}

		void AssertImportResult(JobComInvoiceLine invoiceLine, ZGuid manufacturerOrgPK, ZGuid manufacturerAddressPK)
		{
			AssertEquals(manufacturerOrgPK, invoiceLine.ManufacturerOrgPK);
			AssertEquals(manufacturerAddressPK, invoiceLine.JI_OA_ManufacturerAddress);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceLineImportWizardForTest();
		}

		public class InvoiceLineImportWizardForTest : InvoiceLineImportWizard
		{
			static readonly JobDeclaration declaration = new BusinessObjectFactory().New<JobDeclaration>();

			public InvoiceLineImportWizardForTest() : base(GetCollectionInfo(), GetSettingsStorage(), new FileMapperForTest())
			{
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
			}

			IImportCollectionInfo collectionIfo;
			public new IImportCollectionInfo CollectionInfo => collectionIfo ?? (collectionIfo = GetCollectionInfo());

			public List<string[]> fakeFileContent = new List<string[]>();

			public override List<string[]> LoadFile(int startingRow, int maximumRows, bool forceFileLoad = false)
			{
				return fakeFileContent;
			}

			static IImportCollectionInfo GetCollectionInfo()
			{
				return new ImportCollectionInfoImpl(new InvoiceLineViewCollection(declaration))
				{
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.ManufacturerOrgPK) { HeaderText = "Manufacturer" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLineSchema.Constants.JI_OA_ManufacturerAddress) { HeaderText = "Address" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.ManufacturerMID) { HeaderText = "Manufacturer MID" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_PartAttrib1) { HeaderText = "JI_PartAttrib1" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_PartNo) { HeaderText = "JI_PartNo" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_LinePrice) { HeaderText = "JI_LinePrice" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_InvoiceUQ) { HeaderText = "JI_InvoiceUQ" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_InvoiceQuantity) { HeaderText = "JI_InvoiceQuantity" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_WeightUQ) { HeaderText = "JI_WeightUQ" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_Weight) { HeaderText = "JI_Weight" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_NetWeightUQ) { HeaderText = "JI_NetWeightUQ" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_NetWeight) { HeaderText = "JI_NetWeight" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_CustomsUnitQty) { HeaderText = "JI_CustomsUnitQty" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_CustomsQuantity) { HeaderText = "JI_CustomsQuantity" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty) { HeaderText = "JI_CustomsSecondUnitQty" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_CustomsSecondQuantity) { HeaderText = "JI_CustomsSecondQuantity" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty) { HeaderText = "JI_CustomsThirdUnitQty" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_CustomsThirdQuantity) { HeaderText = "JI_CustomsThirdQuantity" },
				};
			}

			static ISettingsStorage GetSettingsStorage()
			{
				var mockery = new MockRepository(MockBehavior.Default);
				var settingsStorageStub = new Mock<ISettingsStorage>();
				settingsStorageStub.Setup(m => m.GetSavedSettings())
					.Returns(
					new string[] {
						JobComInvoiceLine.Schema.ManufacturerOrgPK,
						JobComInvoiceLineSchema.Constants.JI_OA_ManufacturerAddress,
						JobComInvoiceLine.Schema.ManufacturerMID,
						JobComInvoiceLine.Schema.JI_PartAttrib1,
						JobComInvoiceLine.Schema.JI_PartNo,
						JobComInvoiceLine.Schema.JI_LinePrice,
						JobComInvoiceLine.Schema.JI_InvoiceUQ,
						JobComInvoiceLine.Schema.JI_InvoiceQuantity,
						JobComInvoiceLine.Schema.JI_WeightUQ,
						JobComInvoiceLine.Schema.JI_Weight,
						JobComInvoiceLine.Schema.JI_NetWeightUQ,
						JobComInvoiceLine.Schema.JI_NetWeight,
						JobComInvoiceLine.Schema.JI_CustomsUnitQty,
						JobComInvoiceLine.Schema.JI_CustomsQuantity,
						JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
						JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
						JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
						JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
					});
				return settingsStorageStub.Object;
			}
		}
	}
}
