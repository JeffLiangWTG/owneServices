using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobComInvoiceHeader))]
	sealed class BaseJobComInvoiceHeaderBaseOnlyTest : BaseJobComInvoiceHeaderTest<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>
	{
		public void TestICustomsFileParent_GetDeclarationTypeFromInfo()
		{
			var invoice = (ICustomsFileParent)Factory.New<BaseJobComInvoiceHeader>();
			invoice.DeclarationTypeInfo.SetValueFromString("AAA");

			AssertEquals("Should get value from the DeclarationTypeInfo", "AAA", invoice.DeclarationType);
		}

		public void TestICustomsFileParent_BranchPk()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_GB = branch.PK;

			var customsFileParent = (ICustomsFileParent)invoice;

			AssertEquals("Should get value from JZ_GB.", invoice.JZ_GB, customsFileParent.BranchPk);
		}

		public void TestICustomsFileParent_IsLocked()
		{
			var invoice = (ICustomsFileParent)Factory.New<BaseJobComInvoiceHeader>();

			var log = invoice.Logs.AddNew(AutoEvents.UnlockForEdit);
			AssertEquals("Should not be locked as there is no active LCK log.", false, invoice.IsLocked);

			log = invoice.Logs.AddNew(AutoEvents.LockForEdit);
			AssertEquals("Should be locked as there is an active LCK log.", true, invoice.IsLocked);

			log.Cancel();
			AssertEquals("Should not be locked as there is no active LCK log.", false, invoice.IsLocked);
		}

		public void TestICustomsFileParent_LockFile()
		{
			var invoice = (ICustomsFileParent)Factory.New<BaseJobComInvoiceHeader>();

			var log1 = invoice.Logs.AddNew(AutoEvents.LockForEdit);
			var log2 = invoice.Logs.AddNew(AutoEvents.UnlockForEdit);
			var log3 = invoice.Logs.AddNew(AutoEvents.Acknowledged);

			invoice.LockFile(string.Empty);

			Assert("log1 - Should only cancel all existing LCK or UCK events.", log1.SL_IsCancelled);
			Assert("log2 - Should only cancel all existing LCK or UCK events.", log2.SL_IsCancelled);
			Assert("log3 - Should only cancel all existing LCK or UCK events.", !log3.SL_IsCancelled);

			var newLog = invoice.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
			AssertNotNull("Should add a new LCK event.", newLog);
		}

		public void TestICustomsFileParent_UnlockFile()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();

			var log1 = invoice.Logs.AddNew(AutoEvents.LockForEdit);
			var log2 = invoice.Logs.AddNew(AutoEvents.UnlockForEdit);
			var log3 = invoice.Logs.AddNew(AutoEvents.Acknowledged);

			var fileParent = (ICustomsFileParent)invoice;
			fileParent.UnlockFile(string.Empty);

			Assert("log1 - Should only cancel all existing LCK or UCK events.", log1.SL_IsCancelled);
			Assert("log2 - Should only cancel all existing LCK or UCK events.", log2.SL_IsCancelled);
			Assert("log3 - Should only cancel all existing LCK or UCK events.", !log3.SL_IsCancelled);

			var newLog = invoice.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit);
			AssertNotNull("Should add a new UCK event.", newLog);
		}

		public void TestEventsThatCannotBeAddedOfLogs()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var logs = invoice.Logs;

			var expectedEventCodes = new[] { AutoEvents.LockForEditCode, AutoEvents.UnlockForEditCode };
			var actualEventCodes = logs.EventsThatCannotBeAdded.Cast<Event>().Select(c => c.Code);

			AssertContainsExactElementsInAnyOrder(expectedEventCodes, actualEventCodes);
		}

		public void TestJZ_DataModel_SetOnSaving()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			AssertEquals(ZString.Empty, invoice.JZ_DataModel);
			invoice.OnSaving();
			AssertEquals("ER", invoice.JZ_DataModel);
		}

		public void TestJZ_DataModel_SetOnFactorySave()
		{
			AssertJZ_DataModel(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public void TestJZ_DataModel_Jurisdiction()
		{
			AssertJZ_DataModel(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.Australia);
			AssertJZ_DataModel(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland);
			AssertJZ_DataModel(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			AssertJZ_DataModel(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France);

			AssertJZ_DataModel(Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates);
			AssertJZ_DataModel(Core.Constants.CountryCodes.Liechtenstein, Core.Constants.CountryCodes.Switzerland);
			AssertJZ_DataModel(Core.Constants.CountryCodes.FrenchGuyana, Core.Constants.CountryCodes.France);
			AssertJZ_DataModel(Core.Constants.CountryCodes.Guadeloupe, Core.Constants.CountryCodes.France);
			AssertJZ_DataModel(Core.Constants.CountryCodes.Martinique, Core.Constants.CountryCodes.France);
			AssertJZ_DataModel(Core.Constants.CountryCodes.Mayotte, Core.Constants.CountryCodes.France);
			AssertJZ_DataModel(Core.Constants.CountryCodes.Reunion, Core.Constants.CountryCodes.France);
			AssertJZ_DataModel(Core.Constants.CountryCodes.SaintMartin, Core.Constants.CountryCodes.France);
			AssertJZ_DataModel(Core.Constants.CountryCodes.SaintBarthelemy, Core.Constants.CountryCodes.France);
		}

		void AssertJZ_DataModel(string currentCountry, string expectedJZ_DataModel)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(currentCountry))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				AssertEquals("Not set", ZString.Empty, invoice.JZ_DataModel);
				Factory.Save();
				AssertEquals("Set on saving", expectedJZ_DataModel, invoice.JZ_DataModel);
			}
		}

		public void TestJZ_DataModel_SetOnSaving_NotIsPersistent()
		{
			var invoice = declaration.Invoices.AddNew();
			new FakeDeclarationCreatorForInvoice(invoice);
			AssertEquals("Not set", ZString.Empty, invoice.JZ_DataModel);
			Factory.Save();
			AssertEquals("Set on saving", "ER", invoice.JZ_DataModel);
		}

		public void TestJZ_DataModel_ReportErrorWhenUpdated() =>
			DataModelTestHelper.RunDataModelTest_ReportErrorWhenUpdated<BaseJobComInvoiceHeader>(Factory);

		public void TestJZ_DataModel_CanSaveTwice() =>
			DataModelTestHelper.RunDataModelTest_CanSaveTwice<BaseJobComInvoiceHeader>(Factory);

		public void TestIDataModelSupporter()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			var cusSupportingInfoParent = (IDataModelSupporter)invoice;
			AssertEquals(ZString.Empty, cusSupportingInfoParent.DataModel);
			cusSupportingInfoParent.PopulateDataModelIfNeeded();
			AssertEquals("ER", cusSupportingInfoParent.DataModel);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert("Covered by BaseJobComInvoiceHeaderFetchStrategyTest", true);
		}

		public void TestEntryInstructionProcedures()
		{
			var declaration = Factory.New<CusEntryInstructionValidationTest.DeclarationForEritreaWithCEISupport>();
			var ceiA = declaration.CustomsEntryInstructions.AddNew();
			ceiA.CEI_Procedure = "2";
			var ceiB = declaration.CustomsEntryInstructions.AddNew();
			ceiB.CEI_Procedure = "3";
			var ceiC = declaration.CustomsEntryInstructions.AddNew();
			ceiC.CEI_Procedure = "1";
			var ceiD = declaration.CustomsEntryInstructions.AddNew();
			ceiD.CEI_Procedure = "3";
			var ceiE = declaration.CustomsEntryInstructions.AddNew();
			ceiE.CEI_Procedure = "4";
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_CEI = ceiA.PK;
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_CEI = ceiB.PK;
			var invoice1Line3 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line3.JI_CEI = ceiC.PK;
			var invoice1Line4 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line4.JI_CEI = ceiD.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_CEI = ceiE.PK;

			var entryInstructionProcedures = invoice1.EntryInstructionProcedures;
			AssertEquals("entryInstructionProcedures.Count", 3, entryInstructionProcedures.Count);
			AssertContainsExactElementsInAnyOrder("entryInstructionProcedures", new[] { "1", "2", "3" }, entryInstructionProcedures);
		}

		public void TestGetCusEntryInstructions()
		{
			var declaration = Factory.New<CusEntryInstructionValidationTest.DeclarationForEritreaWithCEISupport>();

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();

			var ceiA = declaration.CustomsEntryInstructions.AddNew();
			var ceiB = declaration.CustomsEntryInstructions.AddNew();
			var ceiC = declaration.CustomsEntryInstructions.AddNew();

			var line11 = invoice1.InvoiceLines.AddNew();
			var line12 = invoice1.InvoiceLines.AddNew();
			var line13 = invoice1.InvoiceLines.AddNew();
			var line14 = invoice1.InvoiceLines.AddNew();

			var line21 = invoice2.InvoiceLines.AddNew();
			var line22 = invoice2.InvoiceLines.AddNew();
			var line23 = invoice2.InvoiceLines.AddNew();
			var line24 = invoice2.InvoiceLines.AddNew();

			line11.JI_CEI = ceiA.PK;
			line12.JI_CEI = ceiB.PK;
			line13.JI_CEI = ceiA.PK;
			line14.JI_CEI = ZGuid.Empty;

			line21.JI_CEI = ceiC.PK;
			line22.JI_CEI = ceiA.PK;
			line23.JI_CEI = ceiA.PK;
			line24.JI_CEI = ZGuid.Invalid;

			AssertContainsExactElementsInAnyOrder(new[] { ceiA, ceiB }, invoice1.CusEntryInstructions.ToArray());
			AssertContainsExactElementsInAnyOrder(new[] { ceiC, ceiA }, invoice2.CusEntryInstructions.ToArray());
		}

		public void TestEffectiveUCR()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("EffectiveUCR", ZString.Empty, invoice.EffectiveUCR);
				declaration.JE_UCR = "DECUCR";
				AssertEquals("EffectiveUCR should come from Declaration", "DECUCR", invoice.EffectiveUCR);
				invoice.JZ_UCR = "INVUCR";
				AssertEquals("EffectiveUCR should come from Invoice", "INVUCR", invoice.EffectiveUCR);
			});
		}

		public void TestUpdatePartSyncManagerAndRefreshShouldNotBeCalledOnLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var buyer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, supplier.PK));

				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "TestPartNum";
				part.OP_Desc = "TESTPARTDESCRIPTION";
				part.OP_StockKeepingUnit = "UNT";

				var relation = part.RelatedOrganisations.AddNew();
				relation.OU_Relationship = "SUP";
				relation.OU_OH = supplier.PK;

				var invoice = Factory.New<BaseJobComInvoiceHeader>();
				new FakeDeclarationCreatorForInvoice(invoice);
				invoice.JZ_OH_Supplier = supplier.PK;
				invoice.JZ_OH_Buyer = buyer.PK;
				invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_OP = part.PK;
				var childLine = invoice.JobComInvoiceLines.AddNew();
				childLine.JI_ParentID = invoiceLine.PK;
				childLine.JI_ParentTableCode = invoiceLine.TablePrefix;
				Factory.Save();
				AssertEquals(2, invoice.JobComInvoiceLines.Count);
				AssertEquals(part.PK, invoiceLine.JI_OP);
				AssertEquals(invoiceLine.PK, childLine.JI_ParentID);
				AssertEquals(false, childLine.IsDeleted);
				var newFactory = new BusinessObjectFactory();
				var invoiceLoaded = newFactory.Load<BaseJobComInvoiceHeader>(invoice.PK);
				AssertEquals(2, invoiceLoaded.JobComInvoiceLines.Count);
				var invoiceLineLoaded = (BaseJobComInvoiceLine)invoiceLoaded.JobComInvoiceLines.FindByPK(invoiceLine.PK);
				AssertEquals(part.PK, invoiceLineLoaded.JI_OP);
				var childLineLoaded = (BaseJobComInvoiceLine)invoiceLoaded.JobComInvoiceLines.FindByPK(childLine.PK);
				AssertEquals(invoiceLineLoaded.PK, childLineLoaded.JI_ParentID);
				AssertEquals(false, childLineLoaded.IsDeleted);
			}
		}

		public void TestJI_OPIsSetWhenAttachedToDeclarationForASNInvoice()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.OP_StockKeepingUnit = "U!";
			part.RelatedOrganisations.AddOwner(consignee);

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PART1";
			AssertEquals("invoiceLine.JI_OP", ZGuid.Empty, invoiceLine.JI_OP);
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "PCE";
			Factory.Save();

			var invoiceLoaded = new BusinessObjectFactory().Load<BaseJobComInvoiceHeader>(invoice.PK);

			var declaration = invoiceLoaded.Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLoaded.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			declaration.Invoices.Add(invoiceLoaded);

			var invoiceLineLoaded = invoiceLoaded.InvoiceLines[0];
			AssertEquals("PART1", invoiceLineLoaded.JI_PartNo);
			AssertEquals(part.PK, invoiceLineLoaded.JI_OP);
			AssertEquals(100m, invoiceLineLoaded.JI_InvoiceQuantity);
			AssertEquals("U!", invoiceLineLoaded.JI_InvoiceUQ);
		}

		public void TestLocalCurrency()
		{
			var useBaseLogicCompany = Factory.New<GlbCompany>();
			useBaseLogicCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Afghanistan;
			useBaseLogicCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var useBaseLogicBranch = useBaseLogicCompany.Branches.AddNew();
			useBaseLogicBranch.GB_RL_NKHomePort = "NZAKL";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = useBaseLogicBranch.PK;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Should pick up declaration local currency", "AFN", invoice.LocalCurrencyCode);
			invoice.JZ_JE = ZGuid.Empty;
			invoice.JZ_GB = ZGuid.Empty;
			AssertEquals("Local currecny reverts to logged in company country currency", Core.Constants.CurrencyCodes.Eritrea, invoice.LocalCurrencyCode);
			invoice.JZ_GB = useBaseLogicBranch.PK;
			AssertEquals("Should pick up Invoice local currency", "AFN", invoice.LocalCurrencyCode);
		}

		public void TestDeclaration()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var invoice = declaration1.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(declaration1, invoiceLine.Declaration);
			var declaration2 = Factory.New<BaseJobDeclaration>();
			invoice.JZ_JE = declaration2.PK;
			AssertEquals(declaration2, invoiceLine.Declaration);
		}

		public void TestDecimalPlacesForTWD_CS00206956()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();

			invoice.JZ_InvoiceAmount = 1.55m;//as expressed in a commercial invoice
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;

			var decimalPlacesAttrib = (DecimalPlacesAttribute)invoice.GetType().GetProperty("JZ_InvoiceAmount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetCustomAttributes(typeof(DecimalPlacesAttribute), false)[0];
			AssertNull(decimalPlacesAttrib.DecimalPlacesMember);
			AssertEquals(2, decimalPlacesAttrib.DecimalPlaces);
		}

		public void TestHumanReadableName()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			AssertEquals("Invoice", invoice.HumanReadableName);
			invoice.JZ_InvoiceNumber = "INV1";
			AssertEquals("Invoice INV1", invoice.HumanReadableName);
			invoice.JZ_OH_Supplier = org1.PK;
			AssertEquals("Invoice INV1 (SUP:ORG1)", invoice.HumanReadableName);
			invoice.JZ_OH_Buyer = org2.PK;
			AssertEquals("Invoice INV1 (SUP:ORG1 IMP:ORG2)", invoice.HumanReadableName);
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("Invoice INV1 (IMP:ORG2)", invoice.HumanReadableName);
			invoice.JZ_InvoiceNumber = ZString.Empty;
			AssertEquals("Invoice (IMP:ORG2)", invoice.HumanReadableName);
			var declaration = Factory.New<BaseJobDeclaration>();
			invoice.JZ_JE = declaration.PK;
			AssertEquals("Invoice", invoice.HumanReadableName);
		}

		public void TestSetJZ_MessageTypeFromBuyerAndSupplier()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.MakeNonPersistent();
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.MainAddress.OA_RL_NKRelatedPortCode = "ERASA"; // Eritrea Assab... local
			var foreignClient = Factory.NewWithValidTestData<OrgHeader>();
			foreignClient.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			invoiceHeader.JZ_OH_Supplier = foreignClient.PK;
			invoiceHeader.JZ_OH_Buyer = localClient.PK;
			AssertEquals("IMP", invoiceHeader.JZ_MessageType);
			invoiceHeader.JZ_OH_Supplier = localClient.PK;
			invoiceHeader.JZ_OH_Buyer = foreignClient.PK;
			AssertEquals("EXP", invoiceHeader.JZ_MessageType);
		}

		public void TestICommonNonApportionedChargeProvider()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			ICommonNonApportionedChargeProvider<BaseInvoiceCharge> provider = invoice;
			AssertEquals(0, invoice.Charges.Count);
			var charge1 = provider.CreateNew();
			AssertEquals(1, invoice.Charges.Count);
			AssertEquals(charge1, invoice.Charges[0]);
			var charge2 = provider.CreateNew();
			AssertEquals(2, invoice.Charges.Count);
			AssertEquals(charge1, invoice.Charges[0]);
			AssertEquals(charge2, invoice.Charges[1]);
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge1.J7_Amount = ZDecimal.Zero;
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge2.J7_Amount = ZDecimal.Zero;
			AssertEquals(charge1, provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasInsurance));
			AssertEquals(charge2, provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasFreight));
			charge1.J7_Amount = 10m;
			AssertNull(provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasInsurance));
			AssertEquals(charge2, provider.GetChargeWithZeroAmount(CustomsChargeTypeList.Codes.OverseasFreight));
		}

		public void TestSupplierBuyerLink()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, declaration.CountryCode);
			var otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);
			var otherCountry2 = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry2.RN_Code));

			var consignee = OrgHeader.New(Factory);
			consignee.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
			var consignor = OrgHeader.New(Factory);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = consignor.PK;
			invoice.JZ_OH_Buyer = consignee.PK;
			AssertNull(invoice.SupplierBuyerLink);

			var link1 = consignee.SupplierLinks.AddNew(consignor);
			link1.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			var link2 = consignee.SupplierLinks.AddNew(consignor);
			link2.OL_RN_NKImporterCountry = otherCountry2.RN_Code;

			AssertEquals(link1, invoice.SupplierBuyerLink);

			declaration.JE_RL_NKFinalDestination = otherUnloco2.RL_Code;
			AssertEquals(link2, invoice.SupplierBuyerLink);

			declaration.JE_RL_NKFinalDestination = "AUSYD";
			link1.OL_RN_NKImporterCountry = "BF";
			var oldCompanyCountry = invoice.Branch.Company.GC_RN_NKCountryCode;
			invoice.Branch.Company.GC_RN_NKCountryCode = otherCountry2.RN_Code;
			AssertEquals(link2, invoice.SupplierBuyerLink);
			invoice.Branch.Company.GC_RN_NKCountryCode = oldCompanyCountry;
		}

		public void TestFOBCalculationWithPackingAdjustedFlag()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var pAC = invoice.Charges.AddNew();
			pAC.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
			pAC.J7_Amount = 2000m;
			pAC.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			pAC.J7_IsDutiable = true;
			pAC.J7_IsIncludedInITOT = false;

			var adjustedPAC = invoice.Charges.AddNew();
			adjustedPAC.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
			adjustedPAC.J7_Amount = 3000m;
			adjustedPAC.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			adjustedPAC.J7_IsDutiable = true;
			adjustedPAC.J7_AdjustedCharge = true;
			adjustedPAC.J7_IsIncludedInITOT = false;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 12000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 12000m, invoice.JZ_Calc_CIFAmount);

			pAC.J7_IsIncludedInITOT = true;
			adjustedPAC.J7_IsIncludedInITOT = true;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 12000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 12000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 12000m, invoice.JZ_Calc_CIFAmount);
		}

		public void TestFOBAndCIFAmountWithDiscount()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			AssertEquals("LineTotal", 1000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB Amount", 1000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF amount", 1000m, invoice.JZ_Calc_CIFAmount);

			var discount = invoice.Charges.AddNew();
			discount.J7_ChargeType = "DIS";
			discount.J7_Amount = 100m;
			discount.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			AssertEquals("LineTotal", 1100m, invoice.InvoiceLineTotal);
			AssertEquals("FOB Amount", 1000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF amount", 1000m, invoice.JZ_Calc_CIFAmount);

			discount.J7_IsIncludedInITOT = true;
			AssertEquals("LineTotal", 1000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB Amount", 1000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF amount", 1000m, invoice.JZ_Calc_CIFAmount);
		}

		public void TestLightValidationIsValid()
		{
			var declaration = Factory.New<DummyBaseJobDeclaration_BaseJobComInvoiceHeaderBaseOnlyTest>();
			declaration.HasNotificationsNotIncludingChildrenReturns = false;

			invoiceHeader.JZ_JE = declaration.PK;
			declaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			invoiceHeader.JZ_OH_Buyer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var declarationAddInfo = GetAddInfoAndPopulateWithData(declaration);
			var invoiceHeaderAddInfo = GetAddInfoAndPopulateWithData(invoiceHeader);

			AssertEquals("Declaration.IsValid initially", false, ((ILightValidationInternals)declaration).IsValid);
			AssertEquals("InvoiceHeader.IsValid initially", false, ((ILightValidationInternals)invoiceHeader).IsValid);
			if (declarationAddInfo is ILightValidationInternals)
			{
				AssertEquals("Declaration.AddInfo.IsValid initially", false, ((ILightValidationInternals)declarationAddInfo).IsValid);
			}
			if (invoiceHeaderAddInfo is ILightValidationInternals)
			{
				AssertEquals("InvoiceHeader.AddInfo.IsValid initially", false, ((ILightValidationInternals)invoiceHeaderAddInfo).IsValid);
			}

			declaration.LoadChildEditableObjects();
			declaration.RunPreSaveValidation();
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<DummyBaseJobDeclaration_BaseJobComInvoiceHeaderBaseOnlyTest>(declaration.PK);
			invoiceHeader = declaration.Invoices[0];

			declarationAddInfo = GetAddInfoAndPopulateWithData(declaration); // accessing the AddInfos may cause a MarkAsNeedingValidation to occur
			invoiceHeaderAddInfo = GetAddInfoAndPopulateWithData(invoiceHeader);
			AssertEquals("InvoiceHeader.HasChanges after save", false, invoiceHeader.HasChanges);
			AssertEquals("Declaration.IsValid after save", true, ((ILightValidationInternals)declaration).IsValid);
			AssertEquals("InvoiceHeader.IsValid after save", true, ((ILightValidationInternals)invoiceHeader).IsValid);
			if (declarationAddInfo is ILightValidationInternals)
			{
				AssertEquals("Declaration.AddInfo.IsValid after save", true, ((ILightValidationInternals)declarationAddInfo).IsValid);
			}
			if (invoiceHeaderAddInfo is ILightValidationInternals)
			{
				AssertEquals("InvoiceHeader.AddInfo.IsValid after save", true, ((ILightValidationInternals)invoiceHeaderAddInfo).IsValid);
			}
		}

		public void TestFOBCalculationWithDiscountAdjustedFlag()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_ExportDate = new ZDateTime(2004, 10, 30);
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 8000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var dIS = invoice.Charges.AddNew();
			dIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			dIS.J7_Amount = 2000m;
			dIS.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			dIS.J7_IsDutiable = true;
			dIS.J7_IsIncludedInITOT = false;

			var adjustedDIS = invoice.Charges.AddNew();
			adjustedDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			adjustedDIS.J7_Amount = 3000m;
			adjustedDIS.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			adjustedDIS.J7_AdjustedCharge = true;
			adjustedDIS.J7_IsIncludedInITOT = false;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 7000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 7000m, invoice.JZ_Calc_CIFAmount);

			dIS.J7_IsIncludedInITOT = true;
			adjustedDIS.J7_IsIncludedInITOT = true;

			AssertEquals("InvoiceLineTotal should only take charges without adjusted on", 8000m, invoice.InvoiceLineTotal);
			AssertEquals("FOB", 8000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("CIF", 8000m, invoice.JZ_Calc_CIFAmount);
		}

		public void TestEXWInvoiceAmountDoesGetCalculatedIfNotSet()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;

			var header = testDec.Invoices.AddNew();
			AssertEquals(0m, header.JZ_InvoiceAmount);

			var line = header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 100;
			AssertEquals(100m, header.JZ_InvoiceAmount);
		}

		public void TestAutoCreateOneInvoiceLineFromHeaderDetailsIfNoLinesAlreadyExist()
		{
			var invHeader = Factory.New<BaseJobComInvoiceHeader>();
			invHeader.JZ_InvoiceNumber = "inv123";
			invHeader.JZ_Weight = 50m;
			invHeader.JZ_Volume = 80m;
			invHeader.JZ_InvoiceAmount = 100m;

			AssertEquals("Precondition that there are no existing invoice lines", 0, invHeader.JobComInvoiceLines.Count);
			invHeader.AutoCreateOneInvoiceLineFromHeaderDetailsIfNoLinesAlreadyExist();
			AssertEquals("Check that there is now just 1 invoice line", 1, invHeader.JobComInvoiceLines.Count);

			var invLine = invHeader.JobComInvoiceLines[0];
			AssertEquals("Check that the new invoice line matchesits daddy: weight", invLine.JI_Weight, invHeader.JZ_Weight);
			AssertEquals("Check that the new invoice line matchesits daddy: amount", invLine.JI_LinePrice, invHeader.JZ_InvoiceAmount);
			AssertEquals("Check that the new invoice line matchesits daddy: serial number", invLine.InvoiceNumber, invHeader.JZ_InvoiceNumber);
			AssertEquals("Check that the new invoice line matchesits daddy: volume", invLine.JI_Volume, invHeader.JZ_Volume);
		}

		public void TestIRegistryAccessingSupporterMembers()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			IRegistryAccessingSupporter supporter = invoice;

			AssertEquals(true, invoice.IsAttachedToPersistentDeclaration);
			declaration.JE_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			declaration.JE_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), supporter.RegistryBranchPK);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			invoice.JZ_JE = ZGuid.Empty;
			AssertEquals(false, invoice.IsAttachedToPersistentDeclaration);
			invoice.JZ_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			invoice.JZ_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), supporter.RegistryBranchPK);

			invoice.JZ_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);
		}

		public void TestCloneGetsSameTypeAsCurrentInstantiation()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			DifferentlyTypedInvoiceHeader invoiceHeader = Factory.New<DifferentlyTypedInvoiceHeader>();

			IBusiness clonedResult = ((ITemplateCopyable)invoiceHeader).TemplateCopy();

			AssertEquals(typeof(DifferentlyTypedInvoiceHeader), clonedResult.GetType());
		}

		/// <summary>
		/// CS00041310
		/// </summary>
		public void TestIncoTermChangeCausesStackOverflowException()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 40000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.Charges.AddNew("OFT", 1000m, declaration.LocalCurrencyCode);

			for (int index = 0; index < 4000; index++)
			{
				var invoiceline = invoice.JobComInvoiceLines.AddNew();
				invoiceline.JI_LinePrice = 10m;
			}

			declaration.ResumeApportionment();
			AssertNoExceptionThrown(delegate
			{ invoice.JZ_IncoTerm = "CFR"; });
		}

		public void TestIncoTermChangeTriggersLineLevelChargeUpdate()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var oft1 = invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var oft2 = invoiceLine2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			var com = invoiceLine2.Charges.AddNew(CustomsChargeTypeList.Codes.Commission);

			AssertEquals("Is Included in invoice", true, oft1.J7_IsNotIncludedInInvoice);
			AssertEquals("Is Included in invoice", false, oft1.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("ReadOnly", true, oft1.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			AssertEquals("Is Included in invoice", true, oft2.J7_IsNotIncludedInInvoice);
			AssertEquals("Is Included in invoice", false, oft2.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("ReadOnly", true, oft2.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			com.J7_Calc_IsIncludedInInvoiceAmount = false;
			AssertEquals("Is Included in invoice", true, com.J7_IsNotIncludedInInvoice);
			AssertEquals("Is Included in invoice", false, com.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("ReadOnly", false, com.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			AssertEquals("Is Included in invoice", false, oft1.J7_IsNotIncludedInInvoice);
			AssertEquals("Is Included in invoice", true, oft1.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("ReadOnly", true, oft1.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			AssertEquals("Is Included in invoice", false, oft2.J7_IsNotIncludedInInvoice);
			AssertEquals("Is Included in invoice", true, oft2.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("ReadOnly", true, oft2.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

			AssertEquals("should not be affected by incoterm change", true, com.J7_IsNotIncludedInInvoice);
			AssertEquals("should not be affected by incoterm change", false, com.J7_Calc_IsIncludedInInvoiceAmount);
			AssertEquals("ReadOnly", false, com.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
		}

		public void TestFixedExchangeRate()
		{
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Belarus;
			invoice.JZ_InvoiceCurrExRateType = "FIX";
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", true, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterableInfo.ReadOnly", false, invoice.IsJZ_InvoiceCurrExRateUserEnterableInfo.ReadOnly);

			invoice.JZ_InvoiceCurrExRateType = ZString.Empty;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterableInfo.ReadOnly", false, invoice.IsJZ_InvoiceCurrExRateUserEnterableInfo.ReadOnly);

			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterableInfo.ReadOnly", true, invoice.IsJZ_InvoiceCurrExRateUserEnterableInfo.ReadOnly);
		}

		public void TestIDocumentSupportableImplementation()
		{
			IDocumentSupportable invoiceHeader = invoice;
			var documentSupporter = (JobComInvoiceHeaderDocumentSupporter)invoiceHeader.DocumentSupporter;
			AssertEquals("documentSupporter.InvoiceHeader", invoiceHeader, documentSupporter.InvoiceHeader);
		}

		public void TestTypeDecider()
		{
			AssertEquals("Type decider type", typeof(BaseJobComInvoiceHeaderTypeDecider), BaseJobComInvoiceHeader.TypeDecider.GetType());
		}

		public void TestIChargeApportioneeAndChargeHolder()
		{
			invoiceGroup.JobComInvoiceGroupHeaders.RemoveAndDeleteAll();
			var subGroup = invoiceGroup.JobComInvoiceGroupHeaders.AddNew();
			var invoice = subGroup.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = invoiceGroup.JobDeclaration.LocalCurrencyCode;
			invoice.JZ_Weight = 10m;
			invoice.JZ_WeightUQ = "kg";
			invoice.JZ_Volume = 3m;
			invoice.JZ_VolumeUQ = "m3";
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("IChargeApportionee.ApportionedCharges", invoice.GroupCharges, ((IChargeApportionee)invoice).ApportionedCharges);
			AssertEquals("GetBaseValueToApportionOn with Value", 10000m, ((IChargeApportionee)invoice).GetBaseValueToApportionOn(invoice.CurrencyConverter, ChargeDistributeByList.Codes.Value));
			AssertEquals("GetBaseValueToApportionOn with WEight", 10m, ((IChargeApportionee)invoice).GetBaseValueToApportionOn(invoice.CurrencyConverter, ChargeDistributeByList.Codes.Weight));
			AssertEquals("GetBaseValueToApportionOn with Volume", 3m, ((IChargeApportionee)invoice).GetBaseValueToApportionOn(invoice.CurrencyConverter, ChargeDistributeByList.Codes.Volume));

			var oFTKey = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true);
			var pACKey = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.PackingCost, true, true);

			AssertEquals("IChargeHolder.Charges", invoice.Charges, ((IChargeHolder)invoice).Charges);
			AssertEquals("IChargeHolder.CurrencyConverter", invoice.CurrencyConverter, ((IChargeHolder)invoice).CurrencyConverter);
			AssertEquals("IChargeHolder.AllApportionees", 1, ((IChargeHolder)invoice).AllApportionees.Length);
			AssertEquals("IChargeHolder.AllApportionees", invoiceLine, ((IChargeHolder)invoice).AllApportionees[0]);
			AssertEquals("IChargeHolder.ImmediateChargeHolderChildren", 1, ((IChargeHolder)invoice).ImmediateChargeHolderChildren.Length);
			AssertEquals("IChargeHolder.ImmediateChargeHolderChildren", invoiceLine, ((IChargeHolder)invoice).ImmediateChargeHolderChildren[0]);
			AssertEquals("IChargeHolder.ImmediateChargeHolderParent", subGroup, ((IChargeHolder)invoice).ImmediateChargeHolderParent);
		}

		public void TestIGroupInvoiceOrInvoice()
		{
			invoiceGroup.JobComInvoiceGroupHeaders.RemoveAndDeleteAll();
			var subGroup = invoiceGroup.JobComInvoiceGroupHeaders.AddNew();

			var invoice = subGroup.JobComInvoiceHeaders.AddNew();

			AssertEquals("GroupInvoiceOfInvoiceOrGroupInvoiceItself", subGroup, ((IGroupInvoiceOrInvoice)invoice).GroupInvoiceOfInvoiceOrGroupInvoiceItself);
			AssertEquals("ParentGroupInvoice", subGroup, ((IGroupInvoiceOrInvoice)invoice).ParentGroupInvoice);
			AssertEquals("IsGroupInvoice", false, ((IGroupInvoiceOrInvoice)invoice).IsGroupInvoice);
			AssertEquals("ChildGroupInvoices", 0, ((IGroupInvoiceOrInvoice)invoice).ChildGroupInvoices.Length);
			AssertEquals("ChildInvoices", 0, ((IGroupInvoiceOrInvoice)invoice).ChildInvoices.Length);

			((IGroupInvoiceOrInvoice)invoice).Move(subGroup, invoiceGroup);
			AssertEquals("JZ_JZ_GroupInvoiceFK is updated", invoiceGroup.PK, invoice.JZ_JZ_GroupInvoiceFK);
			AssertEquals("Sub Group does not have Invoice any more", false, subGroup.JobComInvoiceHeaders.Contains(invoice));
			AssertEquals("Invoice now belongs to TopGroup", true, invoiceGroup.JobComInvoiceHeaders.Contains(invoice));
		}

		public void TestICommonInvoiceImmmediateParent()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var invoiceGroupCharge = invoiceGroup.Charges.AddNew();
				invoiceGroupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				invoiceGroupCharge.J7_Amount = 10;
				invoiceGroupCharge.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoiceGroupCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;

				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invoice.JZ_InvoiceNumber = "TESTINVOICE1";
				invoice.JZ_InvoiceAmount = 10m;
				invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				var subGroup = invoiceGroup.JobComInvoiceGroupHeaders.AddNew();
				var subGroupCharge = subGroup.Charges.AddNew();
				subGroupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				subGroupCharge.J7_Amount = 10;
				subGroupCharge.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				subGroupCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;

				var invoice2 = subGroup.JobComInvoiceHeaders.AddNew();
				invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invoice2.JZ_InvoiceNumber = "TESTINVOICE2";
				invoice2.JZ_InvoiceAmount = 10m;
				invoice2.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

				AssertEquals("ICommonInvoice.ImmediateParent", invoiceGroup, ((ICommonInvoice)invoice).ImmediateCommonInvoiceParent);
				AssertEquals("ICommonInvoice.ImmediateParent", subGroup, ((ICommonInvoice)invoice2).ImmediateCommonInvoiceParent);
				AssertEquals("ICommonInvoice.UserFriendlyCode", "TESTINVOICE1", ((ICommonInvoice)invoice).UserFriendlyCode);
				AssertEquals("ICommonInvoice.UserFriendlyCode", "TESTINVOICE2", ((ICommonInvoice)invoice2).UserFriendlyCode);

				var charge1 = invoice.Charges.AddNew();
				charge1.J7_Amount = 1;

				var charge3 = invoice2.Charges.AddNew();
				charge3.J7_Amount = 1;

				declaration.ResumeApportionment();
				Factory.Save();
				AssertEquals("invoice.GroupCharges.Count", 1, invoice.GroupCharges.Count);
				var charge2 = invoice.GroupCharges[0];
				AssertEquals("invoice2.GroupCharges.Count", 1, invoice2.GroupCharges.Count);
				var charge4 = invoice2.GroupCharges[0];

				var factory2 = new BusinessObjectFactory();
				var invoice1Loaded = factory2.Load<BaseJobComInvoiceHeader>(invoice.PK);

				AssertEquals("AllCharges for Invoice1", true, ((ICommonInvoice)invoice1Loaded).AllCharges.Contains(charge1.PK));
				AssertEquals("AllCharges for Invoice1", true, ((ICommonInvoice)invoice1Loaded).AllCharges.Contains(charge2.PK));
				AssertEquals("AllCharges for Invoice1", false, ((ICommonInvoice)invoice1Loaded).AllCharges.Contains(charge3.PK));
				AssertEquals("AllCharges for Invoice1", false, ((ICommonInvoice)invoice1Loaded).AllCharges.Contains(charge4.PK));
			}
		}

		public void TestEffectiveValuationDate()
		{
			declaration.JE_ExportDate = new ZDateTime(2005, 1, 1);
			invoice.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 2);
			AssertEquals("Valuation date for invoice 1", invoice.JZ_ValuationDateOverride, invoice.EffectiveValuationDate);

			var invoice2 = declaration.Invoices.AddNew();
			AssertEquals("Valuation date is not overriden", ZDateTime.Empty, invoice2.JZ_ValuationDateOverride);
			AssertEquals("Valuation date for invoice2", new ZDateTime(2005, 1, 1), invoice2.EffectiveValuationDate);

			invoice2.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 3);
			AssertEquals("Valuation date for invoice1", new ZDateTime(2005, 1, 2), invoice.EffectiveValuationDate);
			AssertEquals("Valuation date for invoice2", new ZDateTime(2005, 1, 3), invoice2.EffectiveValuationDate);
		}

		public void TestCurrencyConverterDataProvider()
		{
			invoice.JZ_ValuationDateOverride = new ZDateTime(2005, 1, 2);
			AssertEquals("Date of valuation", new ZDateTime(2005, 1, 2), ((ICurrencyConverterDataProvider)invoice).DateOfValuation);
			AssertEquals("Rate type", ZArchitecture.Core.ExchangeRateType.Customs, ((ICurrencyConverterDataProvider)invoice).RateType);
			AssertEquals("MaximumDaysToFallback", BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack, ((ICurrencyConverterDataProvider)invoice).MaximumDaysToFallback);
			AssertEquals("ICurrencyConverterProvider.CurrencyConverter", invoice.CurrencyConverter, ((ICurrencyConverterProvider)invoice).CurrencyConverter);
		}

		public void TestGetForeignCurrencyProviders()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "KRSEL";

			var invoice = (BaseJobComInvoiceHeader)Factory.New<Integration.Customs.KR.IJobComInvoiceHeader>();
			invoice.JZ_GB = branch.PK;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;

			var invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew();
			invoiceLineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;

			var providers = invoice.GetCurrencyProvidersToRefreshExRatesFor();
			AssertEquals(3, providers.Count());
			Assert(providers.Contains(invoice));
			Assert(providers.Contains(invoiceCharge));
			Assert(providers.Contains(invoiceLineCharge));

			invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			AssertEquals(2, providers.Count());
			Assert(providers.Contains(invoice));
			Assert(!providers.Contains(invoiceCharge));

			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			AssertEquals(1, providers.Count());
			Assert(!providers.Contains(invoice));
			Assert(!providers.Contains(invoiceCharge));

			invoiceLineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			AssertEquals(0, providers.Count());
		}

		public void TestEffectiveExchangeRateForInvoiceCurr()
		{
			var foreignCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, invoice.JobDeclaration.LocalCurrencyCode));
			AssertNotNull("PreCondition:ForeignCurrency is there", foreignCurrency);

			TestCaseHelper.ClearTable(RefExchangeRate.Schema.TableName);

			var rate1 = foreignCurrency.ExchangeRates.AddNew();
			rate1.RE_ExRateType = "CUS";
			rate1.RE_StartDate = new ZDateTime(2000, 1, 1);
			rate1.RE_ExpiryDate = new ZDateTime(2000, 1, 1);
			rate1.RE_SellRate = 0.70m;

			invoice.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice.JZ_ValuationDateOverride = new ZDateTime(2000, 1, 2);
			AssertEquals("Fall back is up to 7 days", 0.70m, invoice.EffectiveExchangeRateForInvoiceCurr);

			var rate2 = foreignCurrency.ExchangeRates.AddNew();
			rate2.RE_ExRateType = "CUS";
			rate2.RE_StartDate = new ZDateTime(2000, 1, 2);
			rate2.RE_ExpiryDate = new ZDateTime(2000, 1, 2);
			rate2.RE_SellRate = 0.72m;
			AssertEquals("A new ex-rate is imported and shows the latest ex-rates", 0.72m, invoice.EffectiveExchangeRateForInvoiceCurr);
		}

		public void TestWeightApportionment()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			usd.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.5m);

			var nzd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.NewZealand);
			nzd.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.8m);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 10000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("Precondition: Weight Apportionment enabled", true, declaration.WeightApportionmentEnabled);

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoice1.JZ_InvoiceAmount = 1500m;
			AssertEquals("Invoice1 Weight", 10000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			invoice1.IsImportingData = true;
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_LinePrice = 100m;
			AssertEquals("No apportion while importing", 0m, invoice1Line1.JI_Weight);

			invoice1.IsImportingData = false;
			invoice1Line1.JI_LinePrice = 0m;
			invoice1Line1.JI_LinePrice = 100m;
			AssertEquals("Invoice1Line1 Weight", 10000m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line1.JI_WeightUQ);

			invoice1Line1.JI_WeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("Invoice1Line1 Weight", 10000m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Grams, invoice1Line1.JI_WeightUQ);

			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_WeightUQ = ZString.Empty;
			invoice1Line2.JI_LinePrice = 300m;
			AssertEquals("Invoice1Line1 Weight", 2500000m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Grams, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 7500m, invoice1Line2.JI_Weight);
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = nzd.RX_Code;
			AssertEquals("Invoice1 Weight", 10000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 2500000m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Grams, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 7500m, invoice1Line2.JI_Weight);
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);
			AssertEquals("Invoice2 Weight", ZDecimal.Zero, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice2.JZ_InvoiceAmount = 1600m;
			AssertEquals("Invoice1 Weight", 6000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 1500000m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Grams, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 4500m, invoice1Line2.JI_Weight);
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);
			AssertEquals("Invoice2 Weight", 4000m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice2.JZ_WeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("Invoice1 Weight", 6000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 1500000m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Grams, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 4500m, invoice1Line2.JI_Weight);
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);
			AssertEquals("Invoice2 Weight", 4000m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Grams, invoice2.JZ_WeightUQ);

			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_LinePrice = 200m;
			AssertEquals("Invoice1 Weight", 6000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 1500000m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Grams, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 4500m, invoice1Line2.JI_Weight);
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);
			AssertEquals("Invoice2 Weight", 4000m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Grams, invoice2.JZ_WeightUQ);
			AssertEquals("Invoice2Line1 Weight", 4000m, invoice2Line1.JI_Weight);
			AssertEquals("Invoice2Line1 WeightUQ", Core.Constants.Weight.Grams, invoice2Line1.JI_WeightUQ);

			invoice2Line1.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("Invoice2 Weight", 4000m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Grams, invoice2.JZ_WeightUQ);
			AssertEquals("Invoice2Line1 Weight", 4000m, invoice2Line1.JI_Weight);
			AssertEquals("Invoice2Line1 WeightUQ", Core.Constants.Weight.Tonnes, invoice2Line1.JI_WeightUQ);

			var invoice2Line2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line2.JI_WeightUQ = ZString.Empty;
			invoice2Line2.JI_LinePrice = ZDecimal.Zero;
			AssertEquals("Invoice2 Weight", 4000m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Grams, invoice2.JZ_WeightUQ);
			AssertEquals("Invoice2Line1 Weight", 0.004m, invoice2Line1.JI_Weight);
			AssertEquals("Invoice2Line1 WeightUQ", Core.Constants.Weight.Tonnes, invoice2Line1.JI_WeightUQ);
			AssertEquals("Invoice2Line2 Weight", ZDecimal.Zero, invoice2Line2.JI_Weight);
			AssertEquals("Invoice2Line2 WeightUQ", ZString.Empty, invoice2Line2.JI_WeightUQ);

			declaration.JE_TotalWeight = 1000m;
			invoice1.JZ_Weight = 800m;
			AssertEquals("Invoice1 Weight", 800m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 200000m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Grams, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 600m, invoice1Line2.JI_Weight);
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);

			invoice1Line1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice1Line2.JI_Weight = 400m;
			AssertEquals("Invoice1 Weight", 800m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 200000m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 400m, invoice1Line2.JI_Weight);
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);

			using (declaration.FilteredInvoiceLines.SuspendAdditionallyForImport())
			{
				invoice1.ApportionLineWeight(null);
				AssertEquals("Invoice1Line1 Weight", 200000m, invoice1Line1.JI_Weight);
			}

			invoice1.ApportionLineWeight(null);
			AssertEquals("Invoice1 Weight", 800m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 200m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 600m, invoice1Line2.JI_Weight);
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);

			invoice2.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			AssertEquals("Invoice1 Weight", 483.87m, invoice1.JZ_Weight.Round(2));
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 120.97m, invoice1Line1.JI_Weight.Round(2));
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 362.90m, invoice1Line2.JI_Weight.Round(2));
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);

			AssertEquals("Invoice2 Weight", 516129.03m, invoice2.JZ_Weight.Round(2));
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Grams, invoice2.JZ_WeightUQ);
			AssertEquals("Invoice2Line1 Weight", 0.516m, invoice2Line1.JI_Weight.Round(3));
			AssertEquals("Invoice2Line1 WeightUQ", Core.Constants.Weight.Tonnes, invoice2Line1.JI_WeightUQ);
			AssertEquals("Invoice2Line2 Weight", ZDecimal.Zero, invoice2Line2.JI_Weight);
			AssertEquals("Invoice2Line2 WeightUQ", Core.Constants.Weight.Grams, invoice2Line2.JI_WeightUQ);

			declaration.JE_AutoWeightApportion = false;
			invoice2.JZ_RX_NKInvoice_Currency = nzd.RX_Code;
			AssertEquals("Invoice1 Weight", 483.87m, invoice1.JZ_Weight.Round(2));
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 120.97m, invoice1Line1.JI_Weight.Round(2));
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 362.90m, invoice1Line2.JI_Weight.Round(2));
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);

			AssertEquals("Invoice2 Weight", 516129.03m, invoice2.JZ_Weight.Round(2));
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Grams, invoice2.JZ_WeightUQ);
			AssertEquals("Invoice2Line1 Weight", 0.516m, invoice2Line1.JI_Weight.Round(3));
			AssertEquals("Invoice2Line1 WeightUQ", Core.Constants.Weight.Tonnes, invoice2Line1.JI_WeightUQ);
			AssertEquals("Invoice2Line2 Weight", ZDecimal.Zero, invoice2Line2.JI_Weight);
			AssertEquals("Invoice2Line2 WeightUQ", Core.Constants.Weight.Grams, invoice2Line2.JI_WeightUQ);

			declaration.JE_AutoWeightApportion = true;
			AssertEquals("Invoice1 Weight", 600m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 150m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 450m, invoice1Line2.JI_Weight);
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);

			AssertEquals("Invoice2 Weight", 400000m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Grams, invoice2.JZ_WeightUQ);
			AssertEquals("Invoice2Line1 Weight", 0.4m, invoice2Line1.JI_Weight);
			AssertEquals("Invoice2Line1 WeightUQ", Core.Constants.Weight.Tonnes, invoice2Line1.JI_WeightUQ);
			AssertEquals("Invoice2Line2 Weight", ZDecimal.Zero, invoice2Line2.JI_Weight);
			AssertEquals("Invoice2Line2 WeightUQ", Core.Constants.Weight.Grams, invoice2Line2.JI_WeightUQ);

			var invoices = new ActiveBusinessObjectCollection<BaseJobComInvoiceHeader>(declaration, new ZQuery(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false));
			var invoice3 = (BaseJobComInvoiceHeader)((IBindingList)invoices).AddNew();
			invoice3.JZ_JE = declaration.PK;
			AssertEquals(false, declaration.Invoices.Contains(invoice3));
			invoice3.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoice3.JZ_WeightUQ = ZString.Empty;
			invoice3.JZ_Weight = ZDecimal.Zero;
			invoice3.JZ_InvoiceAmount = 2500m;

			AssertEquals("Invoice1 Weight", 300m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice1Line1 Weight", 75m, invoice1Line1.JI_Weight);
			AssertEquals("Invoice1Line1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line1.JI_WeightUQ);
			AssertEquals("Invoice1Line2 Weight", 225m, invoice1Line2.JI_Weight);
			AssertEquals("Invoice1Line2 WeightUQ", Core.Constants.Weight.Kilograms, invoice1Line2.JI_WeightUQ);

			AssertEquals("Invoice2 Weight", 200000m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Grams, invoice2.JZ_WeightUQ);
			AssertEquals("Invoice2Line1 Weight", 0.2m, invoice2Line1.JI_Weight);
			AssertEquals("Invoice2Line1 WeightUQ", Core.Constants.Weight.Tonnes, invoice2Line1.JI_WeightUQ);
			AssertEquals("Invoice2Line2 Weight", ZDecimal.Zero, invoice2Line2.JI_Weight);
			AssertEquals("Invoice2Line2 WeightUQ", Core.Constants.Weight.Grams, invoice2Line2.JI_WeightUQ);

			AssertEquals("Invoice3 Weight", 500m, invoice3.JZ_Weight);
			AssertEquals("Invoice3 WeightUQ", Core.Constants.Weight.Kilograms, invoice3.JZ_WeightUQ);

			var standAloneInvoice = Factory.New<BaseJobComInvoiceHeader>();
			standAloneInvoice.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			standAloneInvoice.JZ_WeightUQ = "KG";
			standAloneInvoice.JZ_Weight = 12050m;
			standAloneInvoice.JZ_InvoiceAmount = 2500m;

			var standAloneInvoiceLine1 = Factory.New<BaseJobComInvoiceLine>();
			standAloneInvoiceLine1.JI_LinePrice = 100m;
			standAloneInvoiceLine1.JI_JZ = standAloneInvoice.PK;
			FakeDeclarationCreatorForInvoice creator = new FakeDeclarationCreatorForInvoice(standAloneInvoice);
			AssertEquals("Invoice1Line1 Weight", 0m, standAloneInvoiceLine1.JI_Weight);
		}

		public void TestIWeightHolderMembers()
		{
			invoice.JZ_Weight = 120m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Pounds;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			IWeightHolder weightHolder = invoice;
			AssertEquals("TotalWeight", new ZWeight(120m, Core.Constants.Weight.Pounds), weightHolder.TotalWeight);
			AssertEquals("AllApportionees", 2, weightHolder.AllApportionees.Length);
			AssertCollectionContains(invoiceLine1, weightHolder.AllApportionees);
			AssertCollectionContains(invoiceLine2, weightHolder.AllApportionees);

			invoiceLine2.Delete();
			AssertEquals("AllApportionees", 1, weightHolder.AllApportionees.Length);
			AssertCollectionContains(invoiceLine1, weightHolder.AllApportionees);
			AssertCollectionNotContains(invoiceLine2, weightHolder.AllApportionees);
		}

		public void TestIWeightApportioneeMembers()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			usd.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.5m);
			invoice.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoice.JZ_InvoiceAmount = 2500m;
			invoice.JZ_Weight = 15m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Grams;

			IWeightApportionee weightApportionee = invoice;
			AssertEquals("Amount", 5000m, weightApportionee.Amount);

			AssertEquals("Weight", 15m, weightApportionee.Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Grams, weightApportionee.WeightUQ);

			weightApportionee.Weight = 20m;
			weightApportionee.WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Weight", 20m, invoice.JZ_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoice.JZ_WeightUQ);
		}

		public void TestInvoiceAmountForUserEnterableInvoiceCurrExRate()
		{
			SetupUserEnterableInvoiceCurrExRate();
			invoice.JZ_InvoiceAmount = 100m;

			var money = invoice.InvoiceAmount;
			AssertEquals("Amount", 200m, money.Amount);
			AssertEquals("Currency", invoice.JobDeclaration.LocalCurrencyCode, money.Currency.Code);

			RemoveUserEnterableInvoiceCurrExRate();

			money = invoice.InvoiceAmount;
			AssertEquals("Amount", 100m, money.Amount);
			AssertEquals("Currency", invoice.Invoice_Currency.RX_Code, money.Currency.Code);
		}

		public void TestDetachingInvoiceHeaderDoesNotClearJE_MessageType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice1.JZ_JE = ZGuid.Empty;
			Factory.Save();
			AssertEquals("JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
		}

		public void TestAutoAssignContainersToInvoiceLines()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invLine1 = invoice1.InvoiceLines.AddNew();
			var invLine2 = invoice1.InvoiceLines.AddNew();
			var invLine3 = invoice1.InvoiceLines.AddNew();

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT12456";

			AssertEquals("PreCondition: invoice line1 has 1 container", 1, invLine1.ContainersForInvoiceLinesForBindingOnly.Count);
			AssertEquals("PreCondition: invoice line2 has 1 container", 1, invLine2.ContainersForInvoiceLinesForBindingOnly.Count);
			AssertEquals("PreCondition: invoice line3 has 1 container", 1, invLine3.ContainersForInvoiceLinesForBindingOnly.Count);
			AssertEquals("PreCondition: invoice line1 first container not flagged as being for invoice", false, invLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			AssertEquals("PreCondition: invoice line2 first container not flagged as being for invoice", false, invLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			AssertEquals("PreCondition: invoice line3 first container not flagged as being for invoice", false, invLine3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			invoice1.AssignContainerToInvoiceLines(container.CO_ContainerNumber);
			AssertEquals("invoice line1 first container flagged as being for invoice", true, invLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			AssertEquals("invoice line2 first container flagged as being for invoice", true, invLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			AssertEquals("invoice line3 first container flagged as being for invoice", true, invLine3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "abcd98976";
			container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "xyz123";

			AssertEquals("PreCondition: invoice line1 has 3 containers", 3, invLine1.ContainersForInvoiceLinesForBindingOnly.Count);

			invoice1.AssignContainerToInvoiceLines(container.CO_ContainerNumber);
			AssertEquals("invoice line1 SECOND container flagged NOT set as being for invoice", false, invLine1.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine);
			AssertEquals("invoice line1 THIRD container flagged IS set as being for invoice", true, invLine1.ContainersForInvoiceLinesForBindingOnly[2].IsForInvoiceLine);
			AssertEquals("invoice line2 SECOND container flagged NOT set as being for invoice", false, invLine2.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine);
			AssertEquals("invoice line2 THIRD container flagged IS set as being for invoice", true, invLine2.ContainersForInvoiceLinesForBindingOnly[2].IsForInvoiceLine);
			AssertEquals("invoice line3 SECOND container flagged NOT set as being for invoice", false, invLine3.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine);
			AssertEquals("invoice line4 THIRD container flagged IS set as being for invoice", true, invLine3.ContainersForInvoiceLinesForBindingOnly[2].IsForInvoiceLine);
		}

		[ExpectNoExceptions]
		public void TestDeleteDoesNotThrowShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice1 = declaration.Invoices.AddNew();

			invoice1.Delete();
			invoice1.Delete();
		}

		[ExpectNoExceptions]
		public void TestJobDeclarationDoesNotThrowShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice1 = declaration.Invoices.AddNew();

			invoice1.Delete();
			_ = invoice1.JobDeclaration;
		}

		public void TestHeaderCDArchiveInfo()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			var helper = new TestHelper();
			declaration.JE_OH_Importer = helper.Buyer.PK;
			declaration.JE_OH_Supplier = helper.Supplier.PK;

			var invoiceImporter = Factory.NewWithValidTestData<OrgHeader>();
			var invoiceSupplier = Factory.NewWithValidTestData<OrgHeader>();
			invoice.JZ_OH_Buyer = invoiceImporter.PK;
			invoice.JZ_OH_Supplier = invoiceSupplier.PK;

			var customsContainer = declaration.CusContainers.AddNew();
			customsContainer.CO_ContainerNumber = "123";

			var customsContainer2 = declaration.CusContainers.AddNew();
			customsContainer2.CO_ContainerNumber = "APLU123001";

			declaration.JE_RL_NKFinalDestination = "52000";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			declaration.JE_VoyageFlightNo = "001Y";
			declaration.JE_HouseBill = "HouseBill1";
			declaration.JE_DeclarationReference = "B0001645";
			declaration.JE_MasterBill = "MasterBill1";
			declaration.JE_RL_NKOrigin = "NZAKL";
			declaration.JE_VesselName = "BUNGA DELIMA";

			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_OwnerRef = "Test Reference";
			declaration.DocsAndCartage.JP_OrderItemsAsString = "Order1, Order2";

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = "AAA";
			entry1.EntryNumber = "10000012";

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = "BBB";
			entry2.EntryNumber = "10000042";

			var job = new JobHeader.Loader(declaration).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoiceConsignee = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceConsignee.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoiceConsignee.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoiceConsignee.AH_OH = declaration.Importer.PK;
			invoiceConsignee.AH_TransactionNum = "00001001";
			invoiceConsignee.AH_RX_NKTransactionCurrency = declaration.LocalCurrencyCode;
			invoiceConsignee.AH_GB = GlbBranch.CurrentBranch.PK;
			invoiceConsignee.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			invoiceConsignee.AH_TransactionReference = "00001001";
			invoiceConsignee.AH_JH = job.PK;

			var headerInfo = ((ICDArchive)invoice).CDArchiveInfo;
			AssertEquals("Consignee", invoiceImporter.OH_Code, headerInfo.ConsigneeCode);
			AssertEquals("Consignor", invoiceSupplier.OH_Code, headerInfo.ConsignorCode);
			AssertEquals("Containers", "123, APLU123001", headerInfo.ContainerNumbers);
			AssertEquals("Destination", "52000", headerInfo.Destination);
			AssertEquals("EntryNumbers", "10000012, 10000012,10000042, 10000042", headerInfo.EntryNumber);
			AssertEquals("ETA", ZDateTime.Today, headerInfo.ETA);
			AssertEquals("ETD", ZDateTime.Today.AddDays(-1), headerInfo.ETD);
			AssertEquals("HouseBill", "HouseBill1", headerInfo.HouseBill);
			AssertEquals("InvoiceNumbers", "B0001645", headerInfo.InvoiceNumbers);
			AssertEquals("JobNumber", "B0001645", headerInfo.JobNumber);
			AssertEquals("MasterBill", "MasterBill1", headerInfo.MasterBill);
			AssertEquals("OrderNumbers", "Order1, Order2, Test Reference", headerInfo.OrderNumbers);
			AssertEquals("Origin", "NZAKL", headerInfo.Origin);
			AssertEquals("Vessel", "BUNGA DELIMA", headerInfo.Vessel);
			AssertEquals("VoyageFlight", "001Y", headerInfo.VoyageFlight);
		}

		public void TestProductAuditAction()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("ProductAuditAction", ProductAuditActions.Codes.NoAction, invoice.ProductAuditAction());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("ProductAuditAction", ProductAuditActions.Codes.NoAction, invoice.ProductAuditAction());
			CustomsDataRegistry.Instance.ImportProductAuditAction.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddWarningValidation);
			CustomsDataRegistry.Instance.ExportProductAuditAction.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation);
			AssertEquals("ProductAuditAction", ProductAuditActions.Codes.AddWarningValidation, invoice.ProductAuditAction());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("ProductAuditAction", ProductAuditActions.Codes.AddMessageErrorValidation, invoice.ProductAuditAction());

			// Export
			CustomsDataRegistry.Instance.ExportProductAuditAction.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.NoAction);
			var org1 = OrgHeader.New(Factory);
			var orgMiscServ = org1.MiscServ;
			AssertEquals("ProductAuditAction", ProductAuditActions.Codes.RegistryDefault, orgMiscServ.OM_EXValidationForUnauditedClassification);

			orgMiscServ.OM_EXValidationForUnauditedClassification = ProductAuditActions.Codes.NoAction;
			declaration.JE_OH_Supplier = org1.PK;
			AssertEquals(org1.PK, invoice.JZ_OH_Supplier_Effective);
			AssertEquals("ProductAuditAction", ProductAuditActions.Codes.NoAction, invoice.ProductAuditAction());

			CustomsDataRegistry.Instance.ExportProductAuditAction.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation);
			orgMiscServ.OM_EXValidationForUnauditedClassification = ProductAuditActions.Codes.AddWarningValidation;
			AssertEquals("ProductAuditAction", ProductAuditActions.Codes.AddWarningValidation, invoice.ProductAuditAction());

			// Import
			org1 = OrgHeader.New(Factory);
			orgMiscServ = org1.MiscServ;
			AssertEquals("ProductAuditAction", ProductAuditActions.Codes.RegistryDefault, orgMiscServ.OM_IMValidationForUnauditedClassification);
			orgMiscServ.OM_IMValidationForUnauditedClassification = ProductAuditActions.Codes.AddWarningValidation;
			invoice.JZ_OH_Buyer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(org1.PK, invoice.JZ_OH_Buyer_Effective);
			AssertEquals("ProductAuditAction", ProductAuditActions.Codes.AddWarningValidation, invoice.ProductAuditAction());

			CustomsDataRegistry.Instance.ImportProductAuditAction.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ProductAuditActions.Codes.AddMessageErrorValidation);
			orgMiscServ.OM_IMValidationForUnauditedClassification = ProductAuditActions.Codes.NoAction;
			AssertEquals("ProductAuditAction", ProductAuditActions.Codes.NoAction, invoice.ProductAuditAction());
		}

		public void TestRegisterInvoiceLinesToAdditionalDeclarations()
		{
			var declaration1 = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var groupHeader2 = Factory.New<JobComInvoiceGroupHeaderSupportAdditionalDeclarations>();
			groupHeader2.JZ_JE = declaration2.PK;
			groupHeader2.AttachToAdditionalDeclaration(declaration1);
			var invoice = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_JE = declaration2.PK;
			invoice.AttachToAdditionalDeclaration(declaration1);
			AssertCollectionContains(invoice.InvoiceLines, ((IBusiness)declaration1).Children);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			declaration1 = factory.Load<JobDeclarationSupportAdditionalInvoices>(declaration1.PK);
			declaration2 = factory.Load<BaseJobDeclaration>(declaration2.PK);
			invoice = factory.Load<JobComInvoiceHeaderSupportAdditionalDeclarations>(invoice.PK);
			invoiceLine = declaration2.InvoiceLines[0];
			Assert("Precondition: HasChanges", !declaration1.HasChanges);
			AssertCollectionContains(invoice.InvoiceLines, ((IBusiness)declaration1).Children);

			invoiceLine.JI_LinePrice = 100m;
			Assert("HasChanges", declaration1.HasChanges);

			invoice.DetachFromAdditionalDeclaration(declaration1);
			AssertCollectionNotContains(invoice.InvoiceLines, ((IBusiness)declaration1).Children);
		}

		public void TestSuspendRefreshAdditionalInvoice()
		{
			var declaration1 = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var groupHeader2 = Factory.New<JobComInvoiceGroupHeaderSupportAdditionalDeclarations>();
			groupHeader2.JZ_JE = declaration2.PK;
			groupHeader2.AttachToAdditionalDeclaration(declaration1);
			var invoice = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_JE = declaration2.PK;
			using (invoice.SuspendRefreshAdditionalInvoice())
			{
				invoice.AttachToAdditionalDeclaration(declaration1);
			}
			AssertNotEquals(invoice.InvoiceLines.Count, declaration1.InvoiceLines.Count);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			declaration1 = factory.New<JobDeclarationSupportAdditionalInvoices>();
			declaration2 = factory.New<BaseJobDeclaration>();
			invoice = factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			invoice.JZ_JE = declaration2.PK;
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.AttachToAdditionalDeclaration(declaration1);
			AssertEquals(invoice.InvoiceLines.Count, declaration1.InvoiceLines.Count);

			using (invoice.SuspendRefreshAdditionalInvoice())
			{
				invoice.DetachFromAdditionalDeclaration(declaration1);
			}
			AssertEquals(invoice.InvoiceLines.Count, declaration1.InvoiceLines.Count);
		}

		public void TestApplyWorkflowTemplates()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Commerical Invoice Task";
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			trigger.P9_Description = "Edit";

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";

			var invoice2 = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			invoice2.JZ_JE = ZGuid.Empty;
			invoice2.JZ_GB = ZGuid.Empty;
			invoice2.JZ_StandAloneInvoiceDirection = ZString.Empty;

			Factory.Save();

			AssertEquals("Should not apply templates for an Invoice on Declaration", 0, invoice1.WorkflowItems.Tasks.Count);
			AssertEquals("Should not apply templates for an Invoice on Declaration", 0, invoice1.WorkflowItems.Triggers.Count);
			AssertEquals("Should apply templates for a Standalone Invoice", 1, invoice2.WorkflowItems.Tasks.Count);
			AssertEquals("Should apply templates for a Standalone Invoice", 1, invoice2.WorkflowItems.Triggers.Count);
			AssertEquals("Should apply templates for a Standalone Invoice", 1, invoice2.WorkflowItems.Triggers[0].ProcessTaskNotifications.Count);
		}

		public void TestSuspendSettingOfSetterSuspender()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "DDP";
			invoice.JZ_RX_NKInvoice_Currency = "HK";

			var setterSuspender = invoice.SetterSuspender;

			using (setterSuspender.SuspendSetting(JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm))
			{
				invoice.JZ_IncoTerm = "FOB";

				Assert("It should suspend.", invoice.SetterSuspender.IsSetterSuspended(JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm));
				AssertEquals("JZ_IncoTerm unit setter should be suspended", "DDP", invoice.JZ_IncoTerm);
			}

			invoice.JZ_IncoTerm = "FOB";
			Assert("It should not suspend.", !invoice.SetterSuspender.IsSetterSuspended(JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm));
			AssertEquals("JZ_IncoTerm unit setter should not be suspended", "FOB", invoice.JZ_IncoTerm);

			using (setterSuspender.SuspendSetting(JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm))
			{
				invoice.JZ_IncoTerm = string.Empty;

				Assert("It should suspend.", invoice.SetterSuspender.IsSetterSuspended(JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm));
				AssertEquals("JZ_IncoTerm unit setter should be suspended", "FOB", invoice.JZ_IncoTerm);
			}

			invoice.JZ_IncoTerm = string.Empty;
			Assert("It should not suspend.", !invoice.SetterSuspender.IsSetterSuspended(JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm));
			AssertEquals("JZ_IncoTerm unit setter should not be suspended", string.Empty, invoice.JZ_IncoTerm);

			using (setterSuspender.SuspendSetting(JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency))
			{
				invoice.JZ_RX_NKInvoice_Currency = "USD";

				Assert("It should suspend.", invoice.SetterSuspender.IsSetterSuspended(JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency));
				AssertEquals("JZ_RX_NKInvoice_Currency unit setter should be suspended", "HK", invoice.JZ_RX_NKInvoice_Currency);
			}

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			Assert("It should not suspend.", !invoice.SetterSuspender.IsSetterSuspended(JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency));
			AssertEquals("JZ_RX_NKInvoice_Currency unit setter should not be suspended", "USD", invoice.JZ_RX_NKInvoice_Currency);

			using (setterSuspender.SuspendSetting(JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency))
			{
				invoice.JZ_RX_NKInvoice_Currency = string.Empty;

				Assert("It should suspend.", invoice.SetterSuspender.IsSetterSuspended(JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency));
				AssertEquals("JZ_RX_NKInvoice_Currency unit setter should be suspended", "USD", invoice.JZ_RX_NKInvoice_Currency);
			}

			invoice.JZ_RX_NKInvoice_Currency = string.Empty;
			Assert("It should not suspend.", !invoice.SetterSuspender.IsSetterSuspended(JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency));
			AssertEquals("JZ_RX_NKInvoice_Currency unit setter should not be suspended", string.Empty, invoice.JZ_RX_NKInvoice_Currency);
		}

		public void TestGetConsumptionTaxDescription()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				AssertEquals("TVA", BaseJobComInvoiceLine.ConsumptionTaxDescription);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				AssertEquals("VAT", BaseJobComInvoiceLine.ConsumptionTaxDescription);
			}
		}

		public void TestIsInvoiceUsedOverMultipleEntryInstructions()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			Assert(invoiceHeader.IsInvoiceUsedOverMultipleEntryInstructions);

			invoiceLine2.JI_CEI = entryInstruction1.PK;
			Assert(!invoiceHeader.IsInvoiceUsedOverMultipleEntryInstructions);
		}

		public void TestJZ_IncoTerm_IsProcessingOnAllTransactionsCommitted()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				AssertEquals("MandatoryChargesHandler was executed", 2, invoice.Charges.Count);

				var newFactory = new BusinessObjectFactoryIsProcessingOnAllTransactionsCommitted();
				var declaration2 = newFactory.New<BaseJobDeclaration>();
				var invoice2 = declaration2.Invoices.AddNew();
				newFactory.IsProcessingOnAllTransactionsCommittedExposed = true;
				invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
				AssertEquals("MandatoryChargesHandler wasn't executed", 0, invoice2.Charges.Count);
			});
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				var invoice = Factory.New<BaseJobComInvoiceHeader>();
				invoice.JZ_GB = ZGuid.Empty;
				AssertEquals("Branch is null", "ER", (invoice as ITypeDeciderContext).Country);

				invoice.JZ_GB = nzBranch.PK;
				AssertEquals("Branch isn't null", "NZ", (invoice as ITypeDeciderContext).Country);
			});
		}

		public void TestGetDefaultDistributeBy_Export()
		{
			using (Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Quantity))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("QTY", (invoice as IChargeHolder).GetDefaultDistributeBy());
			}
		}

		public void TestGetDefaultDistributeBy_Import()
		{
			using (Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Quantity))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("QTY", (invoice as IChargeHolder).GetDefaultDistributeBy());
			}
		}

		new BaseJobDeclaration declaration;
		BaseJobComInvoiceGroupHeader invoiceGroup;
		BaseJobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			invoiceGroup = declaration.JobComInvoiceGroupHeaders[0];
			invoice = invoiceGroup.JobComInvoiceHeaders.AddNew();
		}

		void RemoveUserEnterableInvoiceCurrExRate()
		{
			invoice.JZ_InvoiceCurrExRateType = ZString.Empty;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
		}

		void SetupUserEnterableInvoiceCurrExRate()
		{
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Belarus;
			invoice.JZ_InvoiceCurrExRateType = "FIX";
			invoice.JZ_InvoiceCurrExRate = 0.5m;
			AssertEquals("IsJZ_InvoiceCurrExRateUserEnterable", true, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
		}

		BusinessObject GetAddInfoAndPopulateWithData(BusinessObject addInfoParent)
		{
			BusinessObject result = null;
			if (ZCustomTypeDescriptor.GetProperties(addInfoParent.GetType())["AddInfo"] != null)
			{
				result = (BusinessObject)addInfoParent["AddInfo"];
				result.FillWithValidTestData(TestBusinessObjectKind.PopulateStrings, Array.Empty<PropertyDescriptor>());
			}
			return result;
		}

		sealed class DifferentlyTypedInvoiceHeader : BaseJobComInvoiceHeader
		{
			public DifferentlyTypedInvoiceHeader(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		sealed class BusinessObjectFactoryIsProcessingOnAllTransactionsCommitted : BusinessObjectFactory, IBusinessObjectFactoryInternals
		{
			public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
			{
				return new BusinessObjectFactoryIsProcessingOnAllTransactionsCommitted();
			}

			internal bool IsProcessingOnAllTransactionsCommittedExposed { get; set; }

			bool IBusinessObjectFactoryInternals.IsProcessingOnAllTransactionsCommitted => IsProcessingOnAllTransactionsCommittedExposed;
		}

		sealed class DummyBaseJobDeclaration_BaseJobComInvoiceHeaderBaseOnlyTest : BaseJobDeclaration
		{
			public DummyBaseJobDeclaration_BaseJobComInvoiceHeaderBaseOnlyTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool HasNotificationsNotIncludingChildrenReturns { get; set; }

			protected override bool HasNotificationsNotIncludingChildren(INotificationType notificationType) => HasNotificationsNotIncludingChildrenReturns;
		}
	}
}
