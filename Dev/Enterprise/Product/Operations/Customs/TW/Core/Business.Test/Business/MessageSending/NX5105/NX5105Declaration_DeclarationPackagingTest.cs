using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105Declaration_DeclarationPackagingTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDeclaration_DeclarationPackaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			entryInstruction.CEI_PackageDescription = "";
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.PackagingMaterialDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Declaration.Packaging.PackagingMaterialDescription should be");
			entryInstruction.CEI_PackageDescription = "PKG Descr";
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.PackagingMaterialDescription, NUnit.Framework.Is.EqualTo("PKG Descr").Using(CustomComparers.TypeComparison), "Declaration.Packaging.PackagingMaterialDescription should be");
			entryInstruction.CEI_IsCoPackaged = false;
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.Combination, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Declaration.Packaging.Combination should be");
			entryInstruction.CEI_IsCoPackaged = true;
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.Combination, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "Declaration.Packaging.Combination should be");
			declaration.JE_TotalNoOfPacksPackType = "";
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.TypeCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Declaration.Packaging.TypeCode should be");
			declaration.JE_TotalNoOfPacksPackType = "BAG";
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.TypeCode, NUnit.Framework.Is.EqualTo("BAG").Using(CustomComparers.TypeComparison), "Declaration.Packaging.TypeCode should be");
		}

		[ExpectNoExceptions]
		public void TestMarksNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentNotes = shipment.Notes.VisibleNotes;
			var noteD = shipmentNotes.AddNew();
			noteD.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			noteD.ST_NoteContextModuleCaption = "D - Customs/Declarations";
			noteD.ST_NoteText = "D Marks & Numbers";
			var noteA = shipmentNotes.AddNew();
			noteA.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			noteA.ST_NoteContextModuleCaption = "A - All";
			noteA.ST_NoteText = "A Marks & Numbers";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceHeader1.JZ_MarksAndNumbers = "MARKS AND NUMBER 1";
			invoiceHeader2.JZ_MarksAndNumbers = "MARKS AND NUMBER 1";
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.MarksNumbers, NUnit.Framework.Is.EqualTo("D Marks & Numbers").Using(CustomComparers.TypeComparison), "Declaration.Packaging.MarksNumbers should be");
			shipmentNotes.Remove(noteD);
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.MarksNumbers, NUnit.Framework.Is.EqualTo("MARKS AND NUMBER 1").Using(CustomComparers.TypeComparison), "Declaration.Packaging.MarksNumbers should be");
			invoiceHeader2.JZ_MarksAndNumbers = "MARKS AND NUMBER 2";
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.MarksNumbers, NUnit.Framework.Is.EqualTo("MARKS AND NUMBER 1\r\nMARKS AND NUMBER 2").Using(CustomComparers.TypeComparison), "Declaration.Packaging.MarksNumbers should be");
			invoiceHeader1.JZ_MarksAndNumbers = "";
			invoiceHeader2.JZ_MarksAndNumbers = "";
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.MarksNumbers, NUnit.Framework.Is.EqualTo("A Marks & Numbers").Using(CustomComparers.TypeComparison), "Declaration.Packaging.MarksNumbers should be");
			shipmentNotes.Remove(noteA);
			NUnit.Framework.Assert.That(messageSendingObject.Packaging.MarksNumbers, NUnit.Framework.Is.EqualTo("N/M").Using(CustomComparers.TypeComparison), "Declaration.Packaging.MarksNumbers should be");
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				new NX5105Declaration_DeclarationPackaging(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var entryHeader = Factory.New<CusEntryHeader>();
				var entryInstruction = Factory.New<CusEntryInstruction>();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				new NX5105Declaration_DeclarationPackaging(entryHeader);
			}

			);
		}
	}
}
