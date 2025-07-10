using System.Linq;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
	sealed class CusEntryHeaderDocumentSupporterTest : Customs.Business.Testing.CusEntryHeaderDocumentSupportTest
	{
		[ExpectNoExceptions]
		public void TestGetBODocDataProviders()
		{
			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			var providers = entryHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.ApplicationAndCertificateDocument), null);
			NUnit.Framework.Assert.That(providers.Length, NUnit.Framework.Is.EqualTo(2), "Provider for ApplicationAndCertificateDocument");
			var inv = entryHeader.InvoiceLines.FirstOrDefault(x => !x.JI_Description.IsEmpty && x.JI_Description.Contains("CHASSIS NO", System.StringComparison.OrdinalIgnoreCase));
			inv.JI_Description = @"1). YEAR: 2016 CAR TYPE: WAGON DOOR: 5
2). BRAND: VOLVO MODEL: XC60 D4
3). DISPLACEMENT: 1969 C.C. CYLINDER: 4 SEAT: 5
4). LEFT SIDE STEERING: YES
5). TRANSMISSION: AUTO (A8) (WITH AUTO SHIFT LOCK)
6). ENGINE TYPE: DIESEL
7). STANDARD EQUIPMENT WITH EGR & CATALYST CONVERTER: YES
8).NON CFC REFRIGERANT SYSTEM (R134A)";
			providers = entryHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.ApplicationAndCertificateDocument), null);
			NUnit.Framework.Assert.That(providers.Length, NUnit.Framework.Is.EqualTo(2), "Provider for ApplicationAndCertificateDocument");
			providers = entryHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.N5167), null);
			NUnit.Framework.Assert.That(providers.Length, NUnit.Framework.Is.EqualTo(1), "Provider for N5167");
			providers = entryHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.ImportCustomsDeclarationDocument), null);
			NUnit.Framework.Assert.That(providers.Length, NUnit.Framework.Is.EqualTo(1), "Provider for ImportCustomsDeclarationDocument");
		}

		[ExpectNoExceptions]
		public void TestGetApplicationAndCertificateDocumentFilterValue()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoice0 = declaration.Invoices.AddNew();
			var line0 = invoice0.InvoiceLines.AddNew() as JobComInvoiceLine;
			line0.JI_CEI = entryInstruction.PK;
			line0.JI_Description = @"1";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			line0.JI_CL = entryLine1.PK;
			entryLine1.CL_LineNumber = 1;
			var chassis1 = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis1.JG_ReferenceNumber = "123456";
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGGDSCTRY), NUnit.Framework.Is.EqualTo("IMPVHCTW"), "Filtervalue for MSGGDSCTRY");
			entryHeader.ResetTotalsAndCachedValues();
			chassis1.JG_ReferenceNumber = "";
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGGDSCTRY), NUnit.Framework.Is.EqualTo("IMPTW"), "Filtervalue for MSGGDSCTRY");
			entryHeader.ResetTotalsAndCachedValues();
			chassis1.JG_ReferenceNumber = "123456";
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGGDSCTRY), NUnit.Framework.Is.EqualTo("EXPVHCTW"), "Filtervalue for MSGGDSCTRY");
			entryHeader.ResetTotalsAndCachedValues();
			chassis1.JG_ReferenceNumber = "";
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGGDSCTRY), NUnit.Framework.Is.EqualTo("EXPTW"), "Filtervalue for MSGGDSCTRY");
		}

		[ExpectNoExceptions]
		public override void TestGetDocBusinessObjects()
		{
			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			var result = entryHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			NUnit.Framework.Assert.That(result[0], NUnit.Framework.Is.EqualTo(default(Enterprise.DocumentEngineCore.DocWrappers.DocumentWrapper)));
		}

		[ExpectNoExceptions]
		public void TestApplicationAndCertificateDocumentIsSupported()
		{
			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter.IsDataContextSupported(new DataContextValue(CusEntryHeaderDocumentSupporter.ApplicationAndCertificateDocument)), NUnit.Framework.Is.EqualTo(true));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var orgHeader0 = Factory.New<OrgHeader>();
			orgHeader0.OH_Code = "TESTORG1";
			declaration.JE_OH_Supplier = orgHeader0.PK;
			var invoice0 = declaration.Invoices.AddNew();
			invoice0.InvoiceLines.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();
			var line0 = invoice0.InvoiceLines.AddNew() as JobComInvoiceLine;
			line0.JI_CEI = entryInst.PK;
			line0.JI_Description = @"1). YEAR: 2016 CAR TYPE: WAGON DOOR: 5
2). BRAND: VOLVO MODEL: XC60 D4
3). DISPLACEMENT: 1969 C.C. CYLINDER: 4 SEAT: 5
4). LEFT SIDE STEERING: YES
5). TRANSMISSION: AUTO (A8) (WITH AUTO SHIFT LOCK)
6). ENGINE TYPE: DIESEL
7). STANDARD EQUIPMENT WITH EGR & CATALYST CONVERTER: YES
8).NON CFC REFRIGERANT SYSTEM (R134A)
9). CHASSIS NO: YV1DZA8BDG2911087; YV1DZA8BDG2910741; YVIMV29H0G2337910";
			new LineMerger(declaration).DoMerge();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var chassis1 = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis1.JG_ReferenceNumber = "123456";
			var chassis2 = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis2.JG_ReferenceNumber = "123456";
			var chassis3 = line0.ChassisJobComInvLineRefsCollection.AddNew();
			chassis3.JG_ReferenceNumber = "789101";
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_EntryStatus = ClearanceStatusCodeList.Codes.C3M;
			entryHeader.EntryNumber = "AAB80860000002";
			Factory.Save();
			return declaration.ActiveEntryHeaders[0];
		}

		[ExpectNoExceptions]
		public void TestN5167IsSupported()
		{
			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter.IsDataContextSupported(new DataContextValue(CusEntryHeaderDocumentSupporter.N5167)), NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestGetFilterValueMSGBKRCTYMOD()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD), NUnit.Framework.Is.EqualTo(declaration.JE_MessageType + customsCountry + declaration.JE_TransportMode), "Filtervalue for MSGBKRCTYMOD");
		}

		[ExpectNoExceptions]
		public void TestGetFilterValueMSGBKRCTY()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			NUnit.Framework.Assert.That(entryHeader.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY), NUnit.Framework.Is.EqualTo(declaration.JE_MessageType + customsCountry), "Filtervalue for MSGBKRCTY");
		}
	}
}
