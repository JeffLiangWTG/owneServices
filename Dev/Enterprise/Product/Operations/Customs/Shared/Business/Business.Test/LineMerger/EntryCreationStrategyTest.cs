using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class EntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestGetKeyForInvoiceLine()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header = dec.Invoices.AddNew();
			var mockLine = Factory.NewMoq<BaseJobComInvoiceLine>();
			mockLine.Setup(m => m.IsExtendedCommercialDescriptionEnabled).Returns(false);
			var line = mockLine.Object;
			mockLine.Setup(m => m.JI_JZ).Returns(header.PK);
			mockLine.Setup(m => m.JI_Tariff).Returns(new ZString("3333333333"));

			dec.JE_MergeBy = ZString.Empty;
			var entryCreationStrategyMock = new Mock<EntryCreationStrategy>(dec) { CallBase = true };
			var entryCreationStrategy = entryCreationStrategyMock.Object;
			Assert(!entryCreationStrategy.GetKeyForLine(line).Contains(new ZString("3333333333")));
			entryCreationStrategyMock.Protected().Setup<ZString>("GetMergeBy").Returns(OrgConstants.MergeInvoiceLines.Tariff);
			Assert(entryCreationStrategy.GetKeyForLine(line).Contains(new ZString("3333333333")));
		}

		public void TestMergeByTariffAndDescriptionContainsDescription()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			BaseJobComInvoiceHeader header = dec.Invoices.AddNew();
			var mockLine = Factory.NewMoq<BaseJobComInvoiceLine>();
			mockLine.Setup(m => m.IsExtendedCommercialDescriptionEnabled).Returns(false);
			BaseJobComInvoiceLine line = mockLine.Object;
			line.JI_JZ = header.PK;
			line.JI_Description = "HELLO MUM";
			line.JI_ExtraInfoForClassification = "BYE MUM";
			EntryCreationStrategy entryCreationStrategy = new EntryCreationStrategy(dec);
			Assert(!entryCreationStrategy.GetKeyForLine(line).Contains((ZString)"HELLO MUM"));
			Assert(!entryCreationStrategy.GetKeyForLine(line).Contains((ZString)"BYE MUM"));
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			Assert(entryCreationStrategy.GetKeyForLine(line).Contains((ZString)"HELLO MUM"));
			Assert(!entryCreationStrategy.GetKeyForLine(line).Contains((ZString)"BYE MUM"));

			mockLine.Reset();
			mockLine.Setup(m => m.IsExtendedCommercialDescriptionEnabled).Returns(true);
			entryCreationStrategy = new EntryCreationStrategy(dec);
			Assert(entryCreationStrategy.GetKeyForLine(line).Contains((ZString)"HELLO MUM"));
			Assert(entryCreationStrategy.GetKeyForLine(line).Contains((ZString)"BYE MUM"));
		}

		public virtual void TestGetKeyForHeader()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ExportDate = new ZDateTime(2004, 12, 12);
			BaseJobComInvoiceHeader invHead1 = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invLine1 = invHead1.JobComInvoiceLines.AddNew();

			LineMerger lineMerger = new LineMerger(declaration);
			EntryCreationStrategy entryCreationStrategy = new EntryCreationStrategy(declaration);
			Assert("Header Valuation Date As String", entryCreationStrategy.GetKeyForHeader(invLine1).Contains(declaration.JE_ExportDate));
			AssertEquals("Entry Instruction not in header", true, declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);

			var entryManager = new EntryManager(declaration, entryCreationStrategy);
			var entryLine = entryManager.GetOrCreateEntryLine(invLine1);
			AssertEquals(ZGuid.Empty, entryLine.Header.CH_CEI_Instruction);
		}

		public void TestGetKeyForHeaderWithEntryInstruction()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var provider = declaration.CustomsEntryInstructionProvider;
			if (provider != null && !provider.IsNoEntryInstruction)
			{
				var entryInstr = provider.CustomsEntryInstructions.AddNew();
				var invHead1 = declaration.Invoices.AddNew();
				var invLine1 = invHead1.JobComInvoiceLines.AddNew();
				invLine1.JI_CEI = entryInstr.PK;

				var lineMerger = new LineMerger(declaration);
				var entryCreationStrategy = new EntryCreationStrategy(declaration);
				AssertEquals("Entry Instruction in header", false, provider.IsNoEntryInstruction);
				Assert("Entry Instruction pk", entryCreationStrategy.GetKeyForHeader(invLine1).Contains(entryInstr.PK));

				var entryManager = new EntryManager(declaration, entryCreationStrategy);
				var entryLine = entryManager.GetOrCreateEntryLine(invLine1);
				AssertEquals(entryInstr.PK, entryLine.Header.CH_CEI_Instruction);
			}
			else
			{
				Assert("Entry instruction is not supported", true);
			}
		}

		[TestDate(2018, 9, 25, 13, 30, 0)]
		public void TestNoExcessiveDatabaseAccess()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TESTPART";
			part.OP_Desc = "TESTPART";

			var declaration = Factory.New<BaseJobDeclaration>();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMPORTER";
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			for (int i = 0; i < 5; i++)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "TESTPART";
				AssertEquals("(pre-condition)", ZGuid.Empty, invoiceLine.JI_OP);
			}

			Factory.Save();

			var strategy = new EntryCreationStrategy(declaration);
			using (AssertDbHitsForAllFactories(ignoreUnspecified: true, expectedHitCounts: new Dictionary<string, int>
			{
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 }
			}))
			{
				var cleanFactory = new ReadOnlyBusinessObjectFactory();
				for (int i = 0; i < 5; i++)
				{
					var invoiceLine = invoice.InvoiceLines[i];
					strategy.HasMergeKeyChangeSinceLastSaving(invoiceLine, cleanFactory);
				}
			}
		}

		public void TestHasMergeKeyChangeSinceLastSaving_ShouldReturnTrueIfNoDeclarationAttached()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			Factory.Save();

			var cleanFactory = new ReadOnlyBusinessObjectFactory();
			var declaration = Factory.New<BaseJobDeclaration>();
			invoiceHeader.JZ_JE = declaration.PK;
			var strategy = new EntryCreationStrategy(declaration);
			var hasChanged = strategy.HasMergeKeyChangeSinceLastSaving(invoiceLine, cleanFactory);
			AssertEquals("InvoiceHeader has no declaration attached, we consider it as 'changed'.", true, hasChanged);

			Factory.Save();
			strategy = new EntryCreationStrategy(declaration);
			hasChanged = strategy.HasMergeKeyChangeSinceLastSaving(invoiceLine, cleanFactory);
			AssertEquals("InvoiceHeader has declaration attached.", false, hasChanged);
		}

		public virtual void TestIsEntryHeaderValidToBeReused()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			declaration.InvoiceLines[0].JI_CL = entryLine.PK;
			var strategy = new EntryCreationStrategy(declaration);

			var result = strategy.IsEntryHeaderValidToBeReused(entry, invoiceLine);
			Assert(result);

			var declaration2 = Factory.New<BaseJobDeclaration>();
			entry.CH_JE = declaration2.PK;
			result = strategy.IsEntryHeaderValidToBeReused(entry, invoiceLine);
			Assert(!result);
		}

		public virtual void TestCanCreateEntryLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var strategy = new EntryCreationStrategy(declaration);
			AssertEquals(true, strategy.CanCreateEntryLine(entryHeader, invoiceLine));
		}
	}
}
