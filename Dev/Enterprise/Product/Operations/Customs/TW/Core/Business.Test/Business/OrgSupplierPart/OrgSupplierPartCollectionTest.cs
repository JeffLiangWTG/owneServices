using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartCollection))]
	sealed class OrgSupplierPartCollectionTest : Customs.Business.Testing.OrgSupplierPartCollectionTest
	{
		public void TestAddPivotWithAdditionalLineDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.Both;
			cusClass.CC_TariffNum = "0121323122";
			invoiceLine.JI_Tariff = "1234567890";
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Set Tariff Code for new pivot", "1234567890", pivot.CI_TariffNum);
			invoiceLine.JI_CC = cusClass.PK;
			pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Set Class. Lookup for new pivot", cusClass.PK, pivot.CI_CC);
			invoiceLine.JI_NDescription = "CI_NDescription test";
			invoiceLine.JI_Description = "CI_Description test";
			invoiceLine.JI_InvoiceUQ = "PKG";
			invoiceLine.JI_Compositions = "CI_Compositions test";
			invoiceLine.JI_CustomsOwnerPartNo = "CI_CustomsOwnerPartNo test";
			invoiceLine.JI_CustomsSupplierPartNo = "CI_CustomsSupplierPartNo test";
			invoiceLine.JI_TariffAdditionalCode = "XX1";
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_AlcoholPercentage = 2m;
			invoiceLine.JI_EnteredUnitPrice = 3.333333m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceLine.JI_Procedure = "AX";
			var permitCusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
			permitCusSupporting.CSI_ReferenceNumber = "X1";
			permitCusSupporting.CSI_LineNo = 1;
			permitCusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
			permitCusSupporting.CSI_ReferenceNumber = "X2";
			permitCusSupporting.CSI_LineNo = 2;
			var assignedJobComInvLineRef = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedJobComInvLineRef.JG_ReferenceNumber = "XX1";
			assignedJobComInvLineRef = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			assignedJobComInvLineRef.JG_ReferenceNumber = "XX2";
			pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("CI_NDescription for new pivot", "CI_NDescription test", pivot.CI_NDescription);
			AssertEquals("CI_Description for new pivot", "CI_Description test", pivot.CI_Description);
			AssertEquals("JI_InvoiceUQ for new pivot", "PKG", pivot.CI_PartPivotUOM);
			AssertEquals("CI_Compositions for new pivot", "CI_Compositions test", pivot.CI_Compositions);
			AssertEquals("CI_CustomsOwnerPartNo for new pivot", "CI_CustomsOwnerPartNo test", pivot.CI_CustomsOwnerPartNo);
			AssertEquals("CI_CustomsSupplierPartNo for new pivot", "CI_CustomsSupplierPartNo test", pivot.CI_CustomsSupplierPartNo);
			AssertEquals("CI_TariffAdditionalCode for new pivot", "XX1", pivot.CI_TariffAdditionalCode);
			AssertEquals("CI_RN_NKCountryOfOrigin for new pivot", "CN", pivot.CI_RN_NKCountryOfOrigin);
			AssertEquals("JI_AlcoholPercentage for new pivot", 2m, pivot.CI_AlcoholPercentage);
			AssertEquals("TW_Price for new pivot", 3.333333m, pivot.CI_Price);
			AssertEquals("TW_PriceCurr for new pivot", "USD", pivot.CI_PriceCurr);
			AssertEquals("TW_DutyTreatment for new pivot", "AX", pivot.CI_DutyTreatment);
			AssertEquals("TW_ModeOfStatistics for new pivot", ZString.Empty, pivot.CI_ModeOfStatistics);
			var productPermitCusSupportingCollection = pivot.ProductPermitCusSupportingCollection.Cast<ProductPermitCusSupporting>();
			AssertEquals("ProductPermitCusSupportingCollection for new pivot", 2, productPermitCusSupportingCollection.Count());
			Assert(productPermitCusSupportingCollection.Any(x => x.CSI_ReferenceNumber == "X1" && x.CSI_LineNo == 1));
			Assert(productPermitCusSupportingCollection.Any(x => x.CSI_ReferenceNumber == "X2" && x.CSI_LineNo == 2));
			var assignedCusClassPartPivotRefCollection = pivot.AssignedCusClassPartPivotRefCollection.Cast<AssignedCusClassPartPivotRef>();
			AssertEquals("AssignedCusClassPartPivotRefCollection for new pivot", 2, assignedCusClassPartPivotRefCollection.Count());
			Assert(assignedCusClassPartPivotRefCollection.Any(x => x.CIR_ReferenceNumber == "XX1"));
			Assert(assignedCusClassPartPivotRefCollection.Any(x => x.CIR_ReferenceNumber == "XX2"));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CC = cusClass.PK;
			invoiceLine2.JI_Procedure = "FA";
			invoiceLine2.JI_TariffAdditionalCode = "XX1";
			invoiceLine2.JI_AlcoholPercentage = 2m;
			collection = new OrgSupplierPartCollection(Factory, invoiceLine2, true);
			pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("CI_DutyTreatment for new pivot", ZString.Empty, pivot.CI_DutyTreatment);
			AssertEquals("CI_ModeOfStatistics for new pivot", "FA", pivot.CI_ModeOfStatistics);
			AssertEquals("CI_TariffAdditionalCode for new pivot", ZString.Empty, pivot.CI_TariffAdditionalCode);
			AssertEquals("CI_AlcoholPercentage for new pivot", 0m, pivot.CI_AlcoholPercentage);
		}

		public void TestAddPivotWithAdditionalLineDetailsForCarInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8734567890";
			invoiceLine.JI_CarType = "A";
			invoiceLine.JI_Transmission = "B";
			invoiceLine.JI_EngineType = "C";
			invoiceLine.JI_LHD = "D";
			invoiceLine.JI_HasCatalystConverter = "E";
			invoiceLine.JI_EquipmentPrintMode = "F";
			invoiceLine.JI_CarCondition = "G";
			invoiceLine.JI_ModelYear = 99;
			invoiceLine.JI_Displacement = "98";
			invoiceLine.JI_NumberOfDoor = 9;
			invoiceLine.JI_Seats = 96;
			invoiceLine.JI_Cylinders = 95;
			invoiceLine.JI_Gears = 94;
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var pivot = collection.AddNew().PivotsForBinding[0];
			CombineAssertions("Set values for new pivot when IsCarRelatedTariff", () =>
			{
				AssertEquals(ZBool.True, pivot.IsCarRelatedTariff);
				AssertEquals("A", pivot.CI_CarType);
				AssertEquals("B", pivot.CI_Transmission);
				AssertEquals("C", pivot.CI_EngineType);
				AssertEquals("D", pivot.CI_LHD);
				AssertEquals("E", pivot.CI_HasCatalystConverter);
				AssertEquals("F", pivot.CI_EquipmentPrintMode);
				AssertEquals("G", pivot.CI_CarCondition);
				AssertEquals(new ZShort(99), pivot.CI_ModelYear);
				AssertEquals("98", pivot.CI_Displacement);
				AssertEquals(new ZShort(9), pivot.CI_NumberOfDoor);
				AssertEquals(new ZShort(96), pivot.CI_Seats);
				AssertEquals(new ZShort(95), pivot.CI_Cylinders);
				AssertEquals(new ZShort(94), pivot.CI_Gears);
			}

			);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			pivot = collection.AddNew().PivotsForBinding[0];
			CombineAssertions("No set values for new pivot when non IsCarRelatedTariff", () =>
			{
				AssertEquals(ZBool.False, pivot.IsCarRelatedTariff);
				AssertEquals(ZString.Empty, pivot.CI_CarType);
				AssertEquals(ZString.Empty, pivot.CI_Transmission);
				AssertEquals(ZString.Empty, pivot.CI_EngineType);
				AssertEquals(ZString.Empty, pivot.CI_LHD);
				AssertEquals(ZString.Empty, pivot.CI_HasCatalystConverter);
				AssertEquals(ZString.Empty, pivot.CI_EquipmentPrintMode);
				AssertEquals(ZString.Empty, pivot.CI_CarCondition);
				AssertEquals(ZShort.Zero, pivot.CI_ModelYear);
				AssertEquals(ZString.Empty, pivot.CI_Displacement);
				AssertEquals(ZShort.Zero, pivot.CI_NumberOfDoor);
				AssertEquals(ZShort.Zero, pivot.CI_Seats);
				AssertEquals(ZShort.Zero, pivot.CI_Cylinders);
				AssertEquals(ZShort.Zero, pivot.CI_Gears);
			}

			);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddAdditionalLineDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();
			var decDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			var doc1 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"), MessageConstants.DocumentTypes.TDM);
			invoiceLine.TrademarkStorageDocsGuid = doc1.UniqueKey;
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			var part = collection.AddNew();
			var docs = part.DocManagerInfo().AllEDocs.Cast<IeDoc>();
			AssertEquals(1, docs.Count());
			Assert(docs.Any(x => x.FileName == "TestBitmap.bmp" && x.DocType == MessageConstants.DocumentTypes.TDM && x.ImageData.Length == doc1.ImageData.Length));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new OrgSupplierPartCollection(Factory);
	}
}
