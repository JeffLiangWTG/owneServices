using System;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		[ExpectNoExceptions]
		public void TestCalculateDeclarationIncotermOnMergeWhenInvoiceIncotermIsAboveFOBAll()
		{
			TestCalculateDeclarationIncotermOnMergeWhenInvoiceIncotermIsAboveFOB("CIF");
			TestCalculateDeclarationIncotermOnMergeWhenInvoiceIncotermIsAboveFOB("C&I");
			TestCalculateDeclarationIncotermOnMergeWhenInvoiceIncotermIsAboveFOB("CFR");
		}

		[ExpectNoExceptions]
		void TestCalculateDeclarationIncotermOnMergeWhenInvoiceIncotermIsAboveFOB(string incoterm)
		{
			var factory = NewFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "TBK0641-0";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CusEntryInstruction;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 1m;
			invoice.JZ_IncoTerm = incoterm;

			var charges = invoice.Charges;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_LinePrice = 1m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("FOB").Using(CustomComparers.TypeComparison));

			var insurance = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 1m, Core.Constants.CurrencyCodes.Taiwan);
			insurance.J7_IsIncludedInITOT = true;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("C&I").Using(CustomComparers.TypeComparison));

			var freight = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 1m, Core.Constants.CurrencyCodes.Taiwan);
			freight.J7_IsIncludedInITOT = true;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("CIF").Using(CustomComparers.TypeComparison));

			insurance.J7_IsIncludedInITOT = false;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("CFR").Using(CustomComparers.TypeComparison));

			freight.J7_IsIncludedInITOT = false;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("FOB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCalculateDeclarationIncotermOnMergeWhenInvoiceIncotermIsBelowOrEqualToFOB()
		{
			var factory = NewFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "TBK0461-0";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CusEntryInstruction;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 1m;
			invoice.JZ_IncoTerm = "EXW";

			var charges = invoice.Charges;
			var insurance = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 1m, Core.Constants.CurrencyCodes.Taiwan);
			insurance.J7_IsIncludedInITOT = true;
			var freight = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 1m, Core.Constants.CurrencyCodes.Taiwan);
			freight.J7_IsIncludedInITOT = true;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_LinePrice = 1m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("EXW").Using(CustomComparers.TypeComparison));

			invoice.JZ_IncoTerm = "FAS";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("FAS").Using(CustomComparers.TypeComparison));

			invoice.JZ_IncoTerm = "FOB";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("FOB").Using(CustomComparers.TypeComparison));

			insurance.J7_IsIncludedInITOT = false;
			freight.J7_IsIncludedInITOT = false;

			invoice.JZ_IncoTerm = "FOB";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("FOB").Using(CustomComparers.TypeComparison));

			invoice.JZ_IncoTerm = "FAS";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("FAS").Using(CustomComparers.TypeComparison));

			invoice.JZ_IncoTerm = "FOB";
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("FOB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDefaultDeclarationIncotermOnMerged_TakeOnlyFromFirstInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_IncoTerm = "EXW";
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_InvoiceQuantity = 20;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("EXW").Using(CustomComparers.TypeComparison));
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice2.JZ_IncoTerm = "FOB";
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2000m;
			invoiceLine2.JI_InvoiceQuantity = 30;
			invoice1.JZ_IncoTerm = "FAS";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			entryHeader = declaration.CustomsEntryHeaders[0];
			NUnit.Framework.Assert.That(entryHeader.CH_DeclarationIncoterm, NUnit.Framework.Is.EqualTo("FAS").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestRequiresMerge()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			var mergeManager = new MergeManager(declaration);
			NUnit.Framework.Assert.That(mergeManager.RequiresMerge, NUnit.Framework.Is.EqualTo(false));

			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			mergeManager = new MergeManager(declaration);
			NUnit.Framework.Assert.That(mergeManager.RequiresMerge, NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public override void TestSupportsAutoMerge()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			var mergeManager = new MergeManager(declaration);
			NUnit.Framework.Assert.That(mergeManager.SupportsAutoMerge, NUnit.Framework.Is.EqualTo(false));

			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			mergeManager = new MergeManager(declaration);
			NUnit.Framework.Assert.That(mergeManager.SupportsAutoMerge, NUnit.Framework.Is.EqualTo(true));
		}

		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.CustomsEntryHeaders.AddNew();
			return declaration;
		}
	}
}
