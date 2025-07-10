using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	sealed class NonInheritableCusEntryLineTest : CusEntryLineTest<CusEntryLine, BaseJobComInvoiceLine>
	{
		public void TestConfirmedFeesCollection()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			AssertNotNull(entryLine.ConfirmedFees);
		}

		public void TestIsInProcessOfMerging()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			CombineAssertions(() =>
			{
				declaration.IsMergeInProgress = true;
				AssertEquals("CusEntryLine.IsInProcessOfMerging should be true", true, entryLine.IsInProcessOfMerging);

				declaration.IsMergeInProgress = false;
				AssertEquals("CusEntryLine.IsInProcessOfMerging should be false", false, entryLine.IsInProcessOfMerging);
			});
		}

		public void TestDelete_AdditionalInvoiceLineLinks()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);

			var testDec = BaseJobDeclaration.New(Factory);
			var invLine = testDec.InvoiceLines.AddNew();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			AssertEquals("Line does not have a direct connection to Entry", ZGuid.Empty, invLine.JI_CL);
			AssertEquals("Link added through invoice line appears in Entry", 1, entryLine.AdditionalInvoiceLineLinks.Count);

			entryLine.Delete();
			AssertEquals("Link gets deleted from collection on invoice line when Entry is deleted", 0, invLine.AdditionalEntryLineLinks.Count);
		}

		[ExpectNoExceptions()]
		public void TestAssignDescriptionToCL_Description()
		{
			var entryLineMock = Factory.NewMoq<CusEntryLine>();
			entryLineMock.Protected().Setup<ZString>("DescriptionInternal").Returns(new ZString("A".PadLeft(CusEntryLineSchema.CL_Description.MaxLength + 1)));
			var entryLine = entryLineMock.Object;
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine.CL_CH = entryHeader.PK;

			entryLine.SetValuesAfterInvoiceLinesAreRebuilt();
		}

		public void TestEffectiveDescription()
		{
			var entryLineMock = Factory.NewMoq<CusEntryLine>();
			entryLineMock.Protected().Setup<ZString>("DescriptionInternal").Returns(new ZString("CalculatedDescription"));
			var entryLine = entryLineMock.Object;
			AssertEquals("EffectiveDescription", "CalculatedDescription", entryLine.EffectiveDescription);

			entryLine.CL_Description = "DatabaseDescription";
			AssertEquals("EffectiveDescription", "DatabaseDescription", entryLine.EffectiveDescription);
		}

		public void TestDelete_AdditionalInvoiceLineLinks_Concurrency()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);

			var factory1 = new BusinessObjectFactory();
			var dec11 = factory1.NewWithValidTestData<BaseJobDeclaration>();
			var invoice11 = dec11.Invoices.AddNew();
			var invoiceLine11 = invoice11.InvoiceLines.AddNew();
			var entry11 = dec11.CustomsEntryHeaders.AddNew();
			entry11.CH_MessageType = "AAA";
			var entry12 = dec11.CustomsEntryHeaders.AddNew();
			entry12.CH_MessageType = "BBB";
			var entryLine11 = entry11.MergedLines.AddNew();
			var entryLine12 = entry12.MergedLines.AddNew();
			var coll11 = invoiceLine11.AdditionalEntryLineLinks;
			var link11 = coll11.AddLinkIfNoneExists(entryLine11);
			var link12 = coll11.AddLinkIfNoneExists(entryLine12);
			factory1.Save();
			entryLine11.Reload();
			entryLine12.Reload();
			AssertEquals(2, coll11.Count);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dec21 = factory2.Load<BaseJobDeclaration>(dec11.PK);
			var coll21 = dec21.InvoiceLines.Cast<BaseJobComInvoiceLine>().First().AdditionalEntryLineLinks;
			AssertEquals(2, coll21.Count);
			AssertEquals(entryLine11.PK, coll21.GetEntryLineFor("AAA").First().PK);
			AssertEquals(entryLine12.PK, coll21.GetEntryLineFor("BBB").First().PK);

			dec21.ActiveEntryHeaders.Cast<CusEntryHeader>().First(x => x.CH_MessageType == "AAA").AllEntryLines.First().Delete();
			AssertSame("AAA deleted in factory 2", null, coll21.GetEntryLineFor("AAA").FirstOrDefault());
			AssertSame("AAA not deleted in factory 1", entryLine11, coll11.GetEntryLineFor("AAA").First());
			AssertEquals("Line in factory 2 still has a link", 1, coll21.Count);
			AssertEquals("Line in factory 1 has both links", 2, coll11.Count);

			factory2.Save();
			AssertSame("AAA not deleted in factory 1", entryLine11, coll11.GetEntryLineFor("AAA").First());
			AssertEquals("Count is as before", 1, coll21.Count);
			AssertEquals("Count is as before", 2, coll11.Count);

			entryLine11.CL_Description = "1";
			entryLine12.CL_Description = "2";
			var handler = new NotificationHandlerForTest();
			try
			{
				factory1.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}
			AssertMultilineASCIIEquals("ReportInformationMessage", @"While you have been working with this form, another user has made changes.
The system will now try to combine your changes with those of the other user.
After you click 'OK', the form will merge your changes with changes made by other user.
However, the fields will have warning messages explaining the other user's changes.
Please review the form carefully before clicking the 'Save' button again.
The following objects have been deleted:
CusEntryLine", handler.ReportInformationMessage.Trim());

			AssertSame("Entry Line is removed from collection in Factory 1", null, coll11.GetEntryLineFor("AAA").FirstOrDefault());
			AssertSame("Second entry line remains", entryLine12, coll11.GetEntryLineFor("BBB").First());
			AssertEquals(1, coll11.Count);
		}

		public void TestDutyAndGSTExcludeLandedCost()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var fee1 = entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100m);
			AssertEquals("entryLine.DutyAmount", 100m, entryLine.DutyAmount);
			fee1.CF_IsLandedCostOnly = true;
			AssertEquals("entryLine.DutyAmount", ZDecimal.Zero, entryLine.DutyAmount);
			var fee2 = entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 200m);
			AssertEquals("entryLine.GSTVATAmount", 200m, entryLine.GSTVATAmount);
			fee2.CF_IsLandedCostOnly = true;
			AssertEquals("entryLine.GSTVATAmount", ZDecimal.Zero, entryLine.GSTVATAmount);
			var fee3 = entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred, 300m);
			AssertEquals("entryLine.GSTVATDeferred", 300m, entryLine.GSTVATDeferred);
			fee3.CF_IsLandedCostOnly = true;
			AssertEquals("entryLine.GSTVATDeferred", ZDecimal.Zero, entryLine.GSTVATDeferred);

			fee2.CF_IsLandedCostOnly = false;
			var fee4 = entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.VAT, 400m);
			AssertEquals("entryLine.GSTVATAmount", 200m, entryLine.GSTVATAmount);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals("entryLine.GSTVATAmount", 400m, entryLine.GSTVATAmount);
		}

		public void TestMergeAndEntryLineInvoiceLines()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("one header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one invoice line for the EntryLine", 1, declaration.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			AssertEquals("one invoice line for the EntryLine", 1, declaration.CustomsEntryHeaders[0].MergedLines[1].InvoiceLines.Count);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("one header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one invoice line for the EntryLine", 1, declaration.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			AssertEquals("one invoice line for the EntryLine", 1, declaration.CustomsEntryHeaders[0].MergedLines[1].InvoiceLines.Count);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobDeclaration declarationLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);
			AssertEquals("one entry", 1, declarationLoaded.CustomsEntryHeaders.Count);
			AssertEquals("two lines", 2, declarationLoaded.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("one invoice line for the EntryLine", 1, declarationLoaded.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			AssertEquals("one invoice line for the EntryLine", 1, declarationLoaded.CustomsEntryHeaders[0].MergedLines[1].InvoiceLines.Count);

			declarationLoaded.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declarationLoaded.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("one header", 1, declarationLoaded.CustomsEntryHeaders.Count);
			AssertEquals("one entryline", 1, declarationLoaded.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Two invoice lines for the EntryLine", 2, declarationLoaded.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
			factory2.Save();

			factory2 = new BusinessObjectFactory();
			declarationLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);
			AssertEquals("one entry", 1, declarationLoaded.CustomsEntryHeaders.Count);
			AssertEquals("one line", 1, declarationLoaded.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("two invoice lines for the EntryLine", 2, declarationLoaded.CustomsEntryHeaders[0].MergedLines[0].InvoiceLines.Count);
		}

		public void TestIRegistryAccessingSupporterMembers()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			IRegistryAccessingSupporter supporter = entryLine;

			declaration.JE_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			declaration.JE_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), supporter.RegistryBranchPK);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			entry.CH_JE = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);
		}

		public void TestCurrencyConverter()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals(entryHeader.CurrencyConverter, entryLine.CurrencyConverter);
			entryLine.CL_CH = ZGuid.Invalid;
			AssertNotNull(entryLine.CurrencyConverter);
			AssertNotEquals("We Can Still Get Currency Converter", entryHeader.CurrencyConverter, entryLine.CurrencyConverter);
		}

		public void TestHasAnyProcedureWithSuspendedVat()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var procedure = GenerateProcedure("No", YesNoList.Codes.No, YesNoList.Codes.No, "No description", false);
			Factory.Save();

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			var invoiceline = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceline.JI_Procedure = "Ye";
			Assert("invoiceline HasAnyProcedureWithSuspendedVat = true", invoiceline.HasAnyProcedureWithSuspendedVat);

			var invoiceline2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceline2.JI_Procedure = "";
			Assert("invoiceline HasAnyProcedureWithSuspendedVat = false", !invoiceline2.HasAnyProcedureWithSuspendedVat);

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryLine entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			Assert(entryLine.HasAnyProcedureWithSuspendedVat);

			invoiceline.JI_Procedure = "";
			Assert(!invoiceline.HasAnyProcedureWithSuspendedVat);
			Assert(!invoiceline2.HasAnyProcedureWithSuspendedVat);
			Assert(!entryLine.HasAnyProcedureWithSuspendedVat);
		}

		RefCusProcedure GenerateProcedure(string procedureCode, string isGuaranteeConsumed, string isGuaranteeReleased, string description, ZBool isCalculeVAT)
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = procedureCode;
			procedure.ZZ6_IsGuaranteeConsumed = isGuaranteeConsumed;
			procedure.ZZ6_IsGuaranteeReleased = isGuaranteeReleased;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_Description = description;
			procedure.ZZ6_CalculateVAT = isCalculeVAT;
			return procedure;
		}
	}
}
