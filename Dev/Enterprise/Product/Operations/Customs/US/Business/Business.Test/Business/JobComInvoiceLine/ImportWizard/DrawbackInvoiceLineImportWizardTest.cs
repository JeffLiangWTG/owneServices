using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackInvoiceLineImportWizardForTest))]
	class DrawbackInvoiceLineImportWizardTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateDetailsForCombinedLineAfterImport()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1020304050";
			tariff.UE_Unit1 = Core.Constants.Weight.Kilograms;
			tariff.UE_Unit2 = Core.Constants.Weight.Grams;
			tariff.UE_Unit3 = Core.Constants.Weight.Pounds;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var referencetDec = Factory.New<JobDeclaration>();
			referencetDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			referencetDec.US_EntryFilerCode = "MC2";
			var entryHeader = referencetDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "62963374";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "123456";
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "7895465";
			var decInvoice = referencetDec.Invoices.AddNew();
			var decInvoiceLine1 = decInvoice.InvoiceLines.AddNew();
			decInvoiceLine1.JI_CL = entryLine1.PK;
			var decInvoiceLine2 = decInvoice.InvoiceLines.AddNew();
			decInvoiceLine2.JI_CL = entryLine2.PK;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = declaration.Invoices.AddNew();
			var collection = new InvoiceLineViewCollection(declaration);
			var wizard = new DrawbackInvoiceLineImportWizardForTest();

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
			wizard.fakeFileContent.Add(new[]
			{
				"2", "1", "TS7054M MOUNTED BRAKE LININGS", "MC262963374", "2", "RN-111711-C", "100", "0.02",
				"0.01", "Y", "$5", "1020304050", "Y", "Y"
			});
			wizard.fakeFileContent.Add(new[]
			{
				"1", "10", "DESC 2", "MC262963374", "1", "INV2", "200", "0.04",
				"0.03", "N", "$500", "2030405060", "Y", "N"
			});

			UnitTestUserNotification.Instance.AddYesAnswer();
			UnitTestUserNotification.Instance.AddYesAnswer();
			wizard.ImportIntoCollection(collection);

			AssertEquals(2, collection.Count);
			CombineAssertions(() =>
			{
				AssertImportResultAfterAddChild(collection[0], 2, "1020304050", 1, "TS7054M MOUNTED BRAKE LININGS", "MC262963374", 2, "RN-111711-C", 100m,
				0.02m, 0.01m, ZBool.True, 5m, ZBool.True, ZBool.True);
				AssertImportResultAfterAddChild(collection[1], 1, "2030405060", 10m, "DESC 2", "MC262963374", 1, "INV2", ZDecimal.Zero,
				ZDecimal.Zero, ZDecimal.Zero, ZBool.False, ZDecimal.Zero, ZBool.True, ZBool.False);
			});
		}

		void AssertImportResultAfterAddChild(JobComInvoiceLine invoiceLine, ZShort lineNo, ZString tariff, ZDecimal invoiceQuantity, ZString description, ZString importEntryNo, ZInt importEntryLine, ZString impDecInvoiceNum, ZDecimal declaredVFD,
			ZDecimal declaredMPF, ZDecimal declaredHMF, ZBool amountOverriden, ZDecimal lineDuty, ZBool isForImportSection, ZBool isForExportSection)
		{
			var prefix = invoiceLine.HumanReadableShortcutName;
			AssertEquals(prefix + ".JI_LineNo", lineNo, invoiceLine.JI_LineNo);
			AssertEquals(prefix + ".JI_Tariff", tariff, invoiceLine.JI_Tariff);
			AssertEquals(prefix + ".JI_InvoiceQuantity", invoiceQuantity, invoiceLine.JI_InvoiceQuantity);
			AssertEquals(prefix + ".JI_Description", description, invoiceLine.JI_Description);
			AssertEquals(prefix + ".US_ImportEntryNo", importEntryNo, invoiceLine.US_ImportEntryNo);
			AssertEquals(prefix + ".US_DRWImportEntryLine", importEntryLine, invoiceLine.US_DRWImportEntryLine);
			AssertEquals(prefix + ".US_ImpDecInvoiceNum", impDecInvoiceNum, invoiceLine.US_ImpDecInvoiceNum);
			AssertEquals(prefix + ".DeclaredVFD", declaredVFD, invoiceLine.DeclaredVFD);
			AssertEquals(prefix + ".DeclaredMPF", declaredMPF, invoiceLine.DeclaredMPF);
			AssertEquals(prefix + ".DeclaredHMF", declaredHMF, invoiceLine.DeclaredHMF);
			AssertEquals(prefix + ".US_DRWClaimAmountOverriden_New", amountOverriden, invoiceLine.US_DRWClaimAmountOverriden_New);
			AssertEquals(prefix + ".LineDuty", lineDuty, invoiceLine.LineDuty);
			AssertEquals(prefix + ".US_DRWIsForImportSection", isForImportSection, invoiceLine.US_DRWIsForImportSection);
			AssertEquals(prefix + ".US_DRWIsForExportSection", isForExportSection, invoiceLine.US_DRWIsForExportSection);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DrawbackInvoiceLineImportWizardForTest();
		}

		public class DrawbackInvoiceLineImportWizardForTest : DrawbackInvoiceLineImportWizard
		{
			public DrawbackInvoiceLineImportWizardForTest()
				: base(GetCollectionInfo(), GetSettingsStorage(), new FileMapperForTest())
			{
			}

			public new IImportCollectionInfo CollectionInfo => collectionIfo ?? (collectionIfo = GetCollectionInfo());
			IImportCollectionInfo collectionIfo;

			public List<string[]> fakeFileContent = new List<string[]>();

			public override List<string[]> LoadFile(int startingRow, int maximumRows, bool forceFileLoad = false)
			{
				return fakeFileContent;
			}

			static IImportCollectionInfo GetCollectionInfo()
			{
				var factory = new BusinessObjectFactory();
				var declaration = factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				return new ImportCollectionInfoImpl(new InvoiceLineViewCollection(declaration))
				{
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_LineNo) { HeaderText = "LNO" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_InvoiceQuantity) { HeaderText = "Inv. Qty" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_Description) { HeaderText = "Goods Description" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_ImportEntryNo) { HeaderText = "Imp Entry No" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_DRWImportEntryLine) { HeaderText = "Imp Entry Line No" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_ImpDecInvoiceNum) { HeaderText = "Imp. Decl. Invoice Number" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.DeclaredVFD) { HeaderText = "Entered Value " },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.DeclaredMPF) { HeaderText = "MPF Claim" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.DeclaredHMF) { HeaderText = "HMF Claim" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_DRWClaimAmountOverriden_New) { HeaderText = "Override" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.LineDuty) { HeaderText = "Line Duty" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_Tariff) { HeaderText = "Tariff" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_DRWIsForImportSection) { HeaderText = "Imp Section" },
					new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_DRWIsForExportSection) { HeaderText = "Exp Section" }
				};
			}

			static ISettingsStorage GetSettingsStorage()
			{
				var settingsStorageStub = new Mock<ISettingsStorage>();
				settingsStorageStub.Setup(m => m.GetSavedSettings())
					.Returns(
					new string[] {
						JobComInvoiceLine.Schema.JI_LineNo,
						JobComInvoiceLine.Schema.JI_InvoiceQuantity,
						JobComInvoiceLine.Schema.JI_Description,
						JobComInvoiceLine.Schema.US_ImportEntryNo,
						JobComInvoiceLine.Schema.US_DRWImportEntryLine,
						JobComInvoiceLine.Schema.US_ImpDecInvoiceNum,
						JobComInvoiceLine.Schema.DeclaredVFD,
						JobComInvoiceLine.Schema.DeclaredMPF,
						JobComInvoiceLine.Schema.DeclaredHMF,
						JobComInvoiceLine.Schema.US_DRWClaimAmountOverriden_New,
						JobComInvoiceLine.Schema.LineDuty,
						JobComInvoiceLine.Schema.JI_Tariff,
						JobComInvoiceLine.Schema.US_DRWIsForImportSection,
						JobComInvoiceLine.Schema.US_DRWIsForExportSection
					});
				return settingsStorageStub.Object;
			}
		}
	}
}
