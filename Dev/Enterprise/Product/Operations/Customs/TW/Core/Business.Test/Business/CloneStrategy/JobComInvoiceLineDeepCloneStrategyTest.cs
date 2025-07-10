using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobComInvoiceLineDeepCloneStrategyTest : Customs.Business.Testing.JobComInvoiceLineDeepCloneStrategyTest
	{
		protected override ZString AddInfoTestData => "CarType=11*Group=XXX";
		[ExpectNoExceptions]
		public void TestCloneInvoiceLineStmNotes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_DeclarationGoodsDescription = "XXX";
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines[0].JI_DeclarationGoodsDescription, NUnit.Framework.Is.EqualTo("XXX").Using(CustomComparers.TypeComparison));
			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines[0].JI_DeclarationGoodsDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCloneInvoiceLineTaxes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var tax1 = invoiceLine.Taxes.AddNew();
			tax1.JLT_Type = "AT";
			tax1.JLT_Tariff = "BREWED";
			tax1.JLT_BaseQuantity = 2m;
			tax1.JLT_MethodOfPayment = "CAS";
			var tax2 = invoiceLine.Taxes.AddNew();
			tax2.JLT_Type = "CT";
			tax2.JLT_Tariff = "OTHERBEVERAGE";
			tax2.JLT_MethodOfPayment = "CAS";
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines[0].Taxes.Count, NUnit.Framework.Is.EqualTo(2), "one tax");
			var clonedTax1 = clonedDec.InvoiceLines[0].Taxes[0];
			NUnit.Framework.Assert.That(clonedTax1.JLT_Type, NUnit.Framework.Is.EqualTo("AT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(clonedTax1.JLT_Tariff, NUnit.Framework.Is.EqualTo("BREWED").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(clonedTax1.JLT_BaseQuantity, NUnit.Framework.Is.EqualTo(2m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(clonedTax1.JLT_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));
			var clonedTax2 = clonedDec.InvoiceLines[0].Taxes[1];
			NUnit.Framework.Assert.That(clonedTax2.JLT_Type, NUnit.Framework.Is.EqualTo("CT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(clonedTax2.JLT_Tariff, NUnit.Framework.Is.EqualTo("OTHERBEVERAGE").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(clonedTax2.JLT_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));
			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines[0].Taxes.Count, NUnit.Framework.Is.EqualTo(0), "one tax");
		}

		[ExpectNoExceptions]
		public void TestCloneJI_Group()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Group = "ZZZZ";
			Factory.Save();
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines[0].JI_Group, NUnit.Framework.Is.EqualTo("ZZZZ").Using(CustomComparers.TypeComparison));
			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines[0].JI_AddInfo, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines[0].JI_NAddInfo, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public new void TestSetEntryInstructionForJobComInvoiceLine_ShouldNotThrowExceptionWhenEntryInstructionNotProvided()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var cloneStrategy1 = new JobComInvoiceLineDeepCloneStrategy(invoiceLine, Customs.Business.CloneType.TemplateCopy, invoice, new Dictionary<ZString, Dictionary<ZGuid, ZGuid>>());
			var cloneInvoiceLine1 = (JobComInvoiceLine)cloneStrategy1.Clone();
			NUnit.Framework.Assert.That(cloneInvoiceLine1.JI_CEI, NUnit.Framework.Is.EqualTo(entryInstruction.PK));
			var cloneStrategy2 = new JobComInvoiceLineDeepCloneStrategy(invoiceLine, Customs.Business.CloneType.TemplateCopy, invoice, new Dictionary<ZString, Dictionary<ZGuid, ZGuid>> { { JobDeclarationDeepCloneStrategy.CusEntryInstructionPKPairsKey, new Dictionary<ZGuid, ZGuid> { { entryInstruction.PK, entryInstruction.PK } } } });
			var cloneInvoiceLine2 = (JobComInvoiceLine)cloneStrategy2.Clone();
			NUnit.Framework.Assert.That(cloneInvoiceLine2.JI_CEI, NUnit.Framework.Is.EqualTo(entryInstruction.PK));
		}

		[ExpectNoExceptions]
		public void TestCloneAssignedJobComInvLineRefs()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var ref1 = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			ref1.JG_ReferenceNumber = "ABCD";
			var ref2 = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			ref2.JG_ReferenceNumber = "EFGH";
			Factory.Save();
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			var assignedNumbers = clonedDec.InvoiceLines[0].AssignedJobComInvLineRefsCollection;
			NUnit.Framework.Assert.That(assignedNumbers.Count, NUnit.Framework.Is.EqualTo(2), "AssignedJobComInvLineRefsCollection should have 2 items");
			NUnit.Framework.Assert.That(assignedNumbers.Cast<AssignedJobComInvLineRefs>().Count(x => x.JG_ReferenceNumber == "ABCD"), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(assignedNumbers.Cast<AssignedJobComInvLineRefs>().Count(x => x.JG_ReferenceNumber == "EFGH"), NUnit.Framework.Is.EqualTo(1));

			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			assignedNumbers = clonedDec2.InvoiceLines[0].AssignedJobComInvLineRefsCollection;
			NUnit.Framework.Assert.That(assignedNumbers.Count, NUnit.Framework.Is.EqualTo(0), "AssignedJobComInvLineRefsCollection should have 0 items");
		}

		[ExpectNoExceptions]
		public void TestCloneReservedFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var item1 = invoiceLine.ReservedFields.AddNew();
			item1.CY_Code = "1";
			item1.CY_Data = "abcd";
			var item2 = invoiceLine.ReservedFields.AddNew();
			item2.CY_Code = "2";
			item2.CY_Data = "efgh";
			Factory.Save();
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			var reservedFields = clonedDec.InvoiceLines[0].ReservedFields;
			NUnit.Framework.Assert.That(reservedFields.Count, NUnit.Framework.Is.EqualTo(2), "ReservedFields should have 2 items");
			NUnit.Framework.Assert.That(reservedFields.Cast<JobComInvoiceLineReservedField>().Count(x => x.CY_Code == "1" && x.CY_Data == "abcd"), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(reservedFields.Cast<JobComInvoiceLineReservedField>().Count(x => x.CY_Code == "2" && x.CY_Data == "efgh"), NUnit.Framework.Is.EqualTo(1));

			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			reservedFields = clonedDec2.InvoiceLines[0].ReservedFields;
			NUnit.Framework.Assert.That(reservedFields.Count, NUnit.Framework.Is.EqualTo(0), "ReservedFields should have 0 items");
		}

		[ExpectNoExceptions]
		public void TestClonePermitCusSupportingCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var item1 = invoiceLine.PermitCusSupportingCollection.AddNew();
			item1.CSI_ReferenceNumber = "1";
			item1.CSI_LineNo = 11;
			var item2 = invoiceLine.PermitCusSupportingCollection.AddNew();
			item2.CSI_ReferenceNumber = "2";
			item2.CSI_LineNo = 22;
			Factory.Save();
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			var permits = clonedDec.InvoiceLines[0].PermitCusSupportingCollection;
			NUnit.Framework.Assert.That(permits.Count, NUnit.Framework.Is.EqualTo(2), "PermitCusSupportingCollection should have 2 items");
			NUnit.Framework.Assert.That(permits.Cast<PermitCusSupporting>().Count(x => x.CSI_ReferenceNumber == "1" && x.CSI_LineNo == 11), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(permits.Cast<PermitCusSupporting>().Count(x => x.CSI_ReferenceNumber == "2" && x.CSI_LineNo == 22), NUnit.Framework.Is.EqualTo(1));

			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			permits = clonedDec2.InvoiceLines[0].PermitCusSupportingCollection;
			NUnit.Framework.Assert.That(permits.Count, NUnit.Framework.Is.EqualTo(0), "PermitCusSupportingCollection should have 0 items");
		}

		[ExpectNoExceptions]
		public void TestCloneExemptionOfControllingAgenciesCusSupportings()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var item1 = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			item1.CSI_ReferenceNumber = "1";
			var item2 = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			item2.CSI_ReferenceNumber = "2";
			Factory.Save();
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			var cusSupportings = clonedDec.InvoiceLines[0].ExemptionOfControllingAgenciesCusSupportings;
			NUnit.Framework.Assert.That(cusSupportings.Count, NUnit.Framework.Is.EqualTo(2), "ExemptionOfControllingAgenciesCusSupportings should have 2 items");
			NUnit.Framework.Assert.That(cusSupportings.Cast<ExemptionOfControllingAgenciesCusSupporting>().Count(x => x.CSI_ReferenceNumber == "1"), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(cusSupportings.Cast<ExemptionOfControllingAgenciesCusSupporting>().Count(x => x.CSI_ReferenceNumber == "2"), NUnit.Framework.Is.EqualTo(1));

			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			cusSupportings = clonedDec2.InvoiceLines[0].ExemptionOfControllingAgenciesCusSupportings;
			NUnit.Framework.Assert.That(cusSupportings.Count, NUnit.Framework.Is.EqualTo(0), "ExemptionOfControllingAgenciesCusSupportings should have 0 items");
		}

		[ExpectNoExceptions]
		public void TestCloneCertificateOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.CertificateOfOriginNumber = "1";
			invoiceLine.CertificateOfOriginNumberItemNumber = 11;
			Factory.Save();
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			var clonedInvoiceLine = clonedDec.InvoiceLines[0];
			NUnit.Framework.Assert.That(clonedInvoiceLine.CertificateOfOriginNumber, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(clonedInvoiceLine.CertificateOfOriginNumberItemNumber, NUnit.Framework.Is.EqualTo((ZShort)11).Using(CustomComparers.TypeComparison));

			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			clonedInvoiceLine = clonedDec2.InvoiceLines[0];
			NUnit.Framework.Assert.That(clonedInvoiceLine.CertificateOfOriginNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(clonedInvoiceLine.CertificateOfOriginNumberItemNumber, NUnit.Framework.Is.EqualTo(ZShort.Zero).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestClonePreviousBondedEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.PreviousBondedEntryNumber = "1";
			invoiceLine.PreviousBondedEntryLineNumber = 11;
			Factory.Save();
			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			var clonedInvoiceLine = clonedDec.InvoiceLines[0];
			NUnit.Framework.Assert.That(clonedInvoiceLine.PreviousBondedEntryNumber, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(clonedInvoiceLine.PreviousBondedEntryLineNumber, NUnit.Framework.Is.EqualTo((ZShort)11).Using(CustomComparers.TypeComparison));

			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy, Factory).Clone();
			NUnit.Framework.Assert.That(clonedDec2.Invoices.Count, NUnit.Framework.Is.EqualTo(1), "one invoice");
			NUnit.Framework.Assert.That(clonedDec2.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1), "one invoice line");
			clonedInvoiceLine = clonedDec2.InvoiceLines[0];
			NUnit.Framework.Assert.That(clonedInvoiceLine.PreviousBondedEntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(clonedInvoiceLine.PreviousBondedEntryLineNumber, NUnit.Framework.Is.EqualTo(ZShort.Zero).Using(CustomComparers.TypeComparison));
		}
	}
}
