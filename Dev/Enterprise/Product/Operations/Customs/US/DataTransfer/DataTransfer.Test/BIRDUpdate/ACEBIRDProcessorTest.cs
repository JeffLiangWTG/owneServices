using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	sealed class ACEBIRDProcessorTest : TestCaseWithFactory
	{
		public const string TestResource = "Enterprise.Customs.US.DataTransfer.Testing.BIRDUpdate.TestFiles.ACE.";
		[TestDate(2008, 1, 1)]
		public void TestCannotImportACSCertifiedCargoRelease()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "ACSCertifiedCargoRelease.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "ACSCertifiedCargoRelease.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			var query = Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "80022510", Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_EntryFilerCode, "SV9"));
			var declarations = Factory.Load<JobDeclaration>(query);
			AssertEquals(0, declarations.Length);
			AssertContains("Cannot process data that is certified for ACS Cargo Release.", notifications.ToUniqueMessageListString());
		}

		[TestDate(2008, 1, 1)]
		public void TestImportCottonFee()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "CottonFee.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "CottonFee.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			var query = Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "81020307", Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_EntryFilerCode, "SV9"));
			var declarations = Factory.Load<JobDeclaration>(query);
			AssertEquals(1, declarations.Length);
			var declaration = declarations[0];
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(3, invoice.JobComInvoiceLines.Count);
			var invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToArray();
			AssertInvoiceLineCotton(invoiceLines.First(x => x.JI_Tariff == "6105100010"), "6105100010", YesNoDefaultList.Codes.No, System.Array.Empty<ZString>());
			AssertInvoiceLineCotton(invoiceLines.First(x => x.JI_Tariff == "6105100011"), "6105100011", ZString.Empty, System.Array.Empty<ZString>());
			AssertInvoiceLineCotton(invoiceLines.First(x => x.JI_Tariff == "6105100012"), "6105100012", ZString.Empty, new ZString[] { "123456789" });
		}

		[TestDate(2008, 1, 1)]
		public void TestImportWatchRepair()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "WatchRepair.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "WatchRepair.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			var query = Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "B0041270", Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_EntryFilerCode, "SV9"));
			var declarations = Factory.Load<JobDeclaration>(query);
			AssertEquals(1, declarations.Length);
			var declaration = declarations[0];
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			var invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToList();
			AssertEquals(4, invoiceLines.Count);
			var parentLine = invoiceLines.First(x => x.JI_ParentID.IsEmpty && x.US_IsParent);
			invoiceLines.Remove(parentLine);
			AssertInvoiceLine(parentLine, "9101118010", "98130020", ZGuid.Empty, ZGuid.Empty, true);
			var invoiceLine = invoiceLines.First(x => x.US_SupTariff == "9813000520");
			invoiceLines.Remove(invoiceLine);
			AssertInvoiceLine(invoiceLine, "9101118040", "9813000520", parentLine.PK, ZGuid.Empty, false);
			invoiceLine = invoiceLines[0].JI_Tariff == "9101118020" ? invoiceLines[0] : invoiceLines[1];
			invoiceLines.Remove(invoiceLine);
			AssertInvoiceLine(invoiceLine, "9101118020", "", parentLine.PK, ZGuid.Empty, false);
			AssertInvoiceLine(invoiceLines[0], "9101118030", "", parentLine.PK, ZGuid.Empty, false);
		}

		void AssertInvoiceLineCotton(JobComInvoiceLine invoiceLine, ZString tariff, ZString cottonFeeExempt, ZString[] expecteExemptionCertificates)
		{
			AssertEquals("invoiceLine.JI_Tariff", tariff, invoiceLine.JI_Tariff);
			AssertEquals("invoiceLine.US_CottonFeeExempt", cottonFeeExempt, invoiceLine.US_CottonFeeExempt);
			AssertContainsExactElementsInAnyOrder(expecteExemptionCertificates, invoiceLine.LicenceAndPermits.GetElementsHaving(LicencePermitTypeList.Codes._22).Select(x => x.CY_Data));
		}

		[TestDate(2008, 1, 1)]
		public void TestImportFDA()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "FDAAndFCC.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "FDAAndFCC.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			var query = Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "81019838", Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_EntryFilerCode, "SV9"));
			var declarations = Factory.Load<JobDeclaration>(query);
			AssertEquals(1, declarations.Length);
			var declaration = declarations[0];
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(1, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines[0];
			AssertEquals("JI_Tariff", "8527214040", invoiceLine.JI_Tariff);
			AssertEquals("US_FDAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_FDAIndicator);
			AssertEquals("invoiceLine.ACE_FDALines.Count", 1, invoiceLine.ACE_FDALines.Count);
			var fda = invoiceLine.ACE_FDALines[0];
			AssertEquals("fda.US_ProgramCode", "RAD", fda.US_ProgramCode);
		}

		[TestDate(2008, 1, 1)]
		public void TestImportMultiLinesWithFDA()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "MultiLinesWithFDA.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "MultiLinesWithFDA.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			var query = Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "81019424", Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_EntryFilerCode, "SV9"));
			var declarations = Factory.Load<JobDeclaration>(query);
			AssertEquals(1, declarations.Length);
			var declaration = declarations[0];
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(4, invoice.JobComInvoiceLines.Count);
			var invoiceLines = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().ToArray();
			var invoiceLine1 = invoiceLines.First(x => x.JI_Tariff == "1902194000" && x.US_FDAIndicator.IsEmpty);
			AssertInvoiceLine(invoiceLine1, "1902194000", ZString.Empty, ZGuid.Empty, ZGuid.Empty, true);
			AssertEquals("invoiceLine1.US_SetInd", SecondarySpecProgIndicatorList.Codes.X, invoiceLine1.US_SetInd);
			AssertEquals("invoiceLine1.US_FDAIndicator", ZString.Empty, invoiceLine1.US_FDAIndicator);
			AssertEquals("invoiceLine1.ACE_FDALines.Count", 0, invoiceLine1.ACE_FDALines.Count);
			var invoiceLine2 = invoiceLines.First(x => x.JI_Tariff == "1902194000" && OGAIndicatorList.IsToBeDeclared(x.US_FDAIndicator));
			AssertInvoiceLine(invoiceLine2, "1902194000", ZString.Empty, invoiceLine1.PK, ZGuid.Empty, false);
			AssertEquals("invoiceLine2.US_SetInd", SecondarySpecProgIndicatorList.Codes.V, invoiceLine2.US_SetInd);
			AssertEquals("invoiceLine2.US_FDAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine2.US_FDAIndicator);
			AssertEquals("invoiceLine2.ACE_FDALines.Count", 1, invoiceLine2.ACE_FDALines.Count);
			AssertEquals("invoiceLine2.ACE_FDALines[0].US_ProgramCode", "FOO", invoiceLine2.ACE_FDALines[0].US_ProgramCode);
			var invoiceLine3 = invoiceLines.First(x => x.JI_Tariff == "0712311000");
			AssertInvoiceLine(invoiceLine3, "0712311000", ZString.Empty, invoiceLine1.PK, ZGuid.Empty, false);
			AssertEquals("invoiceLine3.US_SetInd", SecondarySpecProgIndicatorList.Codes.V, invoiceLine3.US_SetInd);
			AssertEquals("invoiceLine3.US_FDAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine3.US_FDAIndicator);
			AssertEquals("invoiceLine3.ACE_FDALines.Count", 1, invoiceLine3.ACE_FDALines.Count);
			AssertEquals("invoiceLine3.ACE_FDALines[0].US_ProgramCode", "FOO", invoiceLine3.ACE_FDALines[0].US_ProgramCode);
			var invoiceLine4 = invoiceLines.First(x => x.JI_Tariff == "2002908020");
			AssertInvoiceLine(invoiceLine4, "2002908020", ZString.Empty, invoiceLine1.PK, ZGuid.Empty, false);
			AssertEquals("invoiceLine4.US_SetInd", SecondarySpecProgIndicatorList.Codes.V, invoiceLine4.US_SetInd);
			AssertEquals("invoiceLine4.US_FDAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine4.US_FDAIndicator);
			AssertEquals("invoiceLine4.ACE_FDALines.Count", 1, invoiceLine4.ACE_FDALines.Count);
			AssertEquals("invoiceLine4.ACE_FDALines[0].US_ProgramCode", "FOO", invoiceLine4.ACE_FDALines[0].US_ProgramCode);
		}

		[TestDate(2008, 1, 1)]
		public void TestImportNHTSA()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "NHTSA.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "NHTSA.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			var query = Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "81019515", Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_EntryFilerCode, "SV9"));
			var declarations = Factory.Load<JobDeclaration>(query);
			AssertEquals(1, declarations.Length);
			var declaration = declarations[0];
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(1, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines[0];
			AssertInvoiceLine(invoiceLine, "2843300000", ZString.Empty, ZGuid.Empty, ZGuid.Empty, false);
			AssertEquals("invoiceLine.US_NHTSAIndicator", OGAIndicatorList.Codes.Declared, invoiceLine.US_NHTSAIndicator);
			AssertEquals("invoiceLine.NHTSALines.Count", 1, invoiceLine.NHTSALines.Count);
			AssertEquals("invoiceLine.NHTSALines[0].US_NHTProgramCode", "MVS", invoiceLine.NHTSALines[0].US_NHTProgramCode);
		}

		[TestDate(2008, 1, 1)]
		public void TestImportXAndVLines()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "XAndVLines.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "XAndVLines.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			var query = Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "80027576", Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_EntryFilerCode, "SV9"));
			var declarations = Factory.Load<JobDeclaration>(query);
			AssertEquals(1, declarations.Length);
			var declaration = declarations[0];
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(3, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines.OfType<JobComInvoiceLine>().First(x => x.US_IsParent);
			AssertInvoiceLine(invoiceLine, "8708292500", ZString.Empty, ZGuid.Empty, ZGuid.Empty, true);
			AssertEquals("invoiceLine.US_SetInd", SecondarySpecProgIndicatorList.Codes.X, invoiceLine.US_SetInd);
			var childLines = invoiceLine.ChildLines.ToArray();
			AssertEquals("childLines", 2, childLines.Length);
			var childLine1 = childLines[0];
			var childLine2 = childLines[1];
			if (childLine2.JI_Tariff == "8708292500")
			{
				childLine2 = childLines[0];
				childLine1 = childLines[1];
			}

			AssertInvoiceLine(childLine1, "8708292500", ZString.Empty, invoiceLine.PK, ZGuid.Empty, false);
			AssertEquals("childLine1.US_SetInd", SecondarySpecProgIndicatorList.Codes.V, childLine1.US_SetInd);
			AssertInvoiceLine(childLine2, "7210490091", ZString.Empty, invoiceLine.PK, ZGuid.Empty, false);
			AssertEquals("childLine2.US_SetInd", SecondarySpecProgIndicatorList.Codes.V, childLine2.US_SetInd);
		}

		[TestDate(2008, 1, 1)]
		public void TestImportParentAndSecondaryLinesWith98()
		{
			var notifications = new NotificationCollection();
			var importer = new BIRDDataImporter(Factory);
			var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(TestResource + "ParentAndSecondaryLinesWith98.txt"));
			AssertNoExceptionThrown(delegate
			{
				importer.ImportData(reader, "ParentAndSecondaryLinesWith98.txt", notifications, SourceInfo.EmptySourceInfo);
			});
			var query = Customs.Business.EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "80022510", Core.Constants.CountryCodes.UnitedStates);
			query.AddToFilter(new GenAddOnColumnQueryHelper(typeof(JobDeclaration)).GetQueryOnGenAddOnColumn(JobDeclaration.Schema.US_EntryFilerCode, "SV9"));
			var declarations = Factory.Load<JobDeclaration>(query);
			AssertEquals(1, declarations.Length);
			var declaration = declarations[0];
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			var invoiceLine = invoice.JobComInvoiceLines[0];
			if (invoiceLine.IsChildLine)
			{
				invoiceLine = invoice.JobComInvoiceLines[1];
			}

			AssertInvoiceLine(invoiceLine, "9404902000", "9801001010", ZGuid.Empty, ZGuid.Empty, true);
			var secondaryTariffLines = invoiceLine.SecondaryTariffLines.ToArray();
			AssertEquals("SecondaryTariffLines", 1, secondaryTariffLines.Length);
			AssertInvoiceLine(secondaryTariffLines[0], "3401111000", "9801001010", invoiceLine.PK, ZGuid.Empty, false);
		}

		void AssertInvoiceLine(JobComInvoiceLine invoiceLine, ZString tariff, ZString supTariff, ZGuid parentPK, ZGuid parentProductPK, bool isParent)
		{
			AssertEquals("JI_Tariff", tariff, invoiceLine.JI_Tariff);
			AssertEquals("US_SupTariff", supTariff, invoiceLine.US_SupTariff);
			AssertEquals("JI_ParentID", parentPK, invoiceLine.JI_ParentID);
			AssertEquals("US_JI_ParentProduct", parentProductPK, invoiceLine.US_JI_ParentProduct);
			AssertEquals("US_IsParent", isParent, invoiceLine.US_IsParent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCEXPCHI";
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EntryFilerCode, "XJ5");
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			org.EDICommunicationsModes.AddNew().EK_Module = EDICommunicationsMode.Modules.US_BIRD;
			Factory.Save();
		}
	}
}
