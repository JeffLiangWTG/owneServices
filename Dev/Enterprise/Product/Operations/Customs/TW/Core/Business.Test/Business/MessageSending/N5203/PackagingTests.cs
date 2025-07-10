using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PackagingTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMarksNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentNotes = shipment.Notes.VisibleNotes;
			var noteD = shipmentNotes.AddNew();
			noteD.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			noteD.ST_NoteContextModule = nameof(StmNoteContextModule.D);
			noteD.ST_NoteText = "D Marks & Numbers";
			var noteA = shipmentNotes.AddNew();
			noteA.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			noteA.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			noteA.ST_NoteText = "A Marks & Numbers";
			entryHeader.Declaration.JE_JS = shipment.PK;
			var invoice1 = testHelper.CreateInvoiceLineForN5203(entryHeader).InvoiceHeader;
			var invoice2 = testHelper.CreateInvoiceLineForN5203(entryHeader).InvoiceHeader;
			invoice1.JZ_MarksAndNumbers = "XXX1";
			invoice2.JZ_MarksAndNumbers = "XXX2";
			NUnit.Framework.Assert.That(DeclarationPackaging.MarksNumbers, NUnit.Framework.Is.EqualTo("D Marks & Numbers").Using(CustomComparers.TypeComparison));
			shipmentNotes.Remove(noteD);
			NUnit.Framework.Assert.That(DeclarationPackaging.MarksNumbers, NUnit.Framework.Is.EqualTo("XXX1\r\nXXX2").Using(CustomComparers.TypeComparison));
			invoice1.JZ_MarksAndNumbers = ZString.Empty;
			invoice2.JZ_MarksAndNumbers = ZString.Empty;
			NUnit.Framework.Assert.That(DeclarationPackaging.MarksNumbers, NUnit.Framework.Is.EqualTo("A Marks & Numbers").Using(CustomComparers.TypeComparison));
			shipmentNotes.Remove(noteA);
			NUnit.Framework.Assert.That(DeclarationPackaging.MarksNumbers, NUnit.Framework.Is.EqualTo("N/M").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackagingMaterialDescription()
		{
			EntryInstruction.CEI_PackageDescription = "XXX";
			NUnit.Framework.Assert.That(DeclarationPackaging.PackagingMaterialDescription, NUnit.Framework.Is.EqualTo("XXX").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCombination()
		{
			NUnit.Framework.Assert.That(DeclarationPackaging.Combination, NUnit.Framework.Is.EqualTo(ZString.Empty));
			EntryInstruction.CEI_IsCoPackaged = true;
			NUnit.Framework.Assert.That(DeclarationPackaging.Combination, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			entryHeader.Declaration.JE_TotalNoOfPacksPackType = "KG";
			NUnit.Framework.Assert.That(DeclarationPackaging.TypeCode, NUnit.Framework.Is.EqualTo("KG").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
		}

		TestTWCreator testHelper;
		CusEntryHeader entryHeader;
		IDeclarationPackaging DeclarationPackaging => new Packaging(entryHeader);
		CusEntryInstruction EntryInstruction => entryHeader.EntryInstruction;
	}
}
