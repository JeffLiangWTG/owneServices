using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWMessageSendingValidationTest : TestCaseWithFactory
	{
		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCheckCalculationValuesWhenImport()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 28.57m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var decl = Factory.New<JobDeclaration>();
			decl.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			decl.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			decl.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = decl.CusEntryInstruction;

			var invoice = decl.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = -16372m;

			var charges = invoice.Charges;
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, -2300m, Core.Constants.CurrencyCodes.UnitedStates);
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, -50m, Core.Constants.CurrencyCodes.UnitedStates);
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, -18700.00m, Core.Constants.CurrencyCodes.UnitedStates);
			var dde = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, -2839.02, Core.Constants.CurrencyCodes.UnitedStates);
			dde.J7_IsIncludedInITOT = true;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "8419.20.00.00-5";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "IL";
			invoiceLine1.JI_Procedure = "50";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = -7738.2m;
			invoiceLine1.JI_VatPymntMthd = "CAS";
			invoiceLine1.JI_DtyPymntMthd = "CAS";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "8419.90.20.00-6";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_CountryOfOrigin = "IL";
			invoiceLine2.JI_Procedure = "50";
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = -145.8m;
			invoiceLine2.JI_VatPymntMthd = "DEF";
			invoiceLine2.JI_DtyPymntMthd = "DEF";

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Tariff = "8419.20.00.00-5";
			invoiceLine3.JI_PrimaryPreference = "PR1";
			invoiceLine3.JI_CountryOfOrigin = "IL";
			invoiceLine3.JI_Procedure = "50";
			invoiceLine3.JI_InvoiceQuantity = 1;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = -7738.2m;
			invoiceLine3.JI_VatPymntMthd = "CAS";
			invoiceLine3.JI_DtyPymntMthd = "DEF";

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_Tariff = "4819.10.00.00-1";
			invoiceLine4.JI_PrimaryPreference = "PR1";
			invoiceLine4.JI_CountryOfOrigin = "IL";
			invoiceLine4.JI_Procedure = "50";
			invoiceLine4.JI_InvoiceQuantity = 1;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = -108m;
			invoiceLine4.JI_VatPymntMthd = "CAS";
			invoiceLine4.JI_DtyPymntMthd = "CAS";

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = entryInstruction.PK;
			invoiceLine5.JI_Tariff = "84199020006";
			invoiceLine5.JI_PrimaryPreference = "PR1";
			invoiceLine5.JI_CountryOfOrigin = "IL";
			invoiceLine5.JI_Procedure = "50";
			invoiceLine5.JI_InvoiceQuantity = 1;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = -145.8m;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_CEI = entryInstruction.PK;
			invoiceLine6.JI_Tariff = "84199020006";
			invoiceLine6.JI_PrimaryPreference = "PR1";
			invoiceLine6.JI_CountryOfOrigin = "IL";
			invoiceLine6.JI_Procedure = "50";
			invoiceLine6.JI_InvoiceQuantity = 2;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = -160m;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "84199020006";
			invoiceLine7.JI_PrimaryPreference = "PR1";
			invoiceLine7.JI_CountryOfOrigin = "IL";
			invoiceLine7.JI_Procedure = "50";
			invoiceLine7.JI_InvoiceQuantity = 2;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = -88m;

			decl.ResumeApportionment();
			_ = decl.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var entryHeader = decl.EntryHeader;
			entryHeader.MergedLines.Cast<CusEntryLine>().ToList().ForEach(x => x.CL_ValueForVAT = -10000m);

			var charge1 = entryHeader.Charges.AddNew();
			charge1.C1_ChargeAmount = -1250.9m;
			charge1.C1_MethodOfPayment = "CAS";

			var charge2 = entryHeader.Charges.AddNew();
			charge2.C1_ChargeAmount = -1000m;
			charge2.C1_MethodOfPayment = "DEF";

			var testWrapper = new JobDeclarationMessageSendingObjectParent(decl, "ICD");
			var validation = TWMessageSendingValidation.New(testWrapper, null, false);
			var notificationsAsString = validation.CheckBusinessObjectLevelValidation().NotificationsAsString();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("FOB (17) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Freight (18) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Insurance (19) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Additions (20) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Deductions (21) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("CIF (22) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("CIF (TWD) (22) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Business Tax Base cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Total Tax Amount (Cash) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Total Tax Amount (Non-Cash) cannot be negative."));
			});

			charges.Cast<InvoiceCharge>().ToList().ForEach(x => x.J7_Amount = 10m);
			invoice.JZ_InvoiceAmount = 9000m;
			decl.ResumeApportionment();
			_ = decl.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			entryHeader = decl.EntryHeader;
			entryHeader.MergedLines.Cast<CusEntryLine>().ToList().ForEach(x => x.CL_ValueForVAT = 1000m);

			_ = entryHeader.DutyTaxFeeCharges.ShouldRebuildElements();
			charge1 = entryHeader.Charges.AddNew();
			charge1.C1_ChargeAmount = 1250.9m;
			charge1.C1_MethodOfPayment = "CAS";

			charge2 = entryHeader.Charges.AddNew();
			charge2.C1_ChargeAmount = 1000m;
			charge2.C1_MethodOfPayment = "DEF";

			testWrapper = new JobDeclarationMessageSendingObjectParent(decl, "ICD");
			validation = TWMessageSendingValidation.New(testWrapper, null, false);
			notificationsAsString = validation.CheckBusinessObjectLevelValidation().NotificationsAsString();
			NUnit.Framework.Assert.That(notificationsAsString, NUnit.Framework.Is.EqualTo(ZString.Empty), "Should be empty.");
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestCheckCalculationValuesWhenExport()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 28.57m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			Factory.Save();

			var decl = Factory.New<JobDeclaration>();
			decl.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			decl.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			decl.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = decl.CusEntryInstruction;

			var invoice = decl.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = -16372m;

			var charges = invoice.Charges;
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, -2300m, Core.Constants.CurrencyCodes.UnitedStates);
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, -50m, Core.Constants.CurrencyCodes.UnitedStates);
			_ = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, -18700.00m, Core.Constants.CurrencyCodes.UnitedStates);
			var ded = charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, -2839.02, Core.Constants.CurrencyCodes.UnitedStates);
			ded.J7_IsIncludedInITOT = true;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "8419.20.00.00-5";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "IL";
			invoiceLine1.JI_Procedure = "50";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = -7738.2m;
			invoiceLine1.JI_VatPymntMthd = "CAS";
			invoiceLine1.JI_DtyPymntMthd = "CAS";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "8419.90.20.00-6";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_CountryOfOrigin = "IL";
			invoiceLine2.JI_Procedure = "50";
			invoiceLine2.JI_InvoiceQuantity = 1;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = -145.8m;
			invoiceLine2.JI_VatPymntMthd = "DEF";
			invoiceLine2.JI_DtyPymntMthd = "DEF";

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Tariff = "8419.20.00.00-5";
			invoiceLine3.JI_PrimaryPreference = "PR1";
			invoiceLine3.JI_CountryOfOrigin = "IL";
			invoiceLine3.JI_Procedure = "50";
			invoiceLine3.JI_InvoiceQuantity = 1;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = -7738.2m;
			invoiceLine3.JI_VatPymntMthd = "CAS";
			invoiceLine3.JI_DtyPymntMthd = "DEF";

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_Tariff = "4819.10.00.00-1";
			invoiceLine4.JI_PrimaryPreference = "PR1";
			invoiceLine4.JI_CountryOfOrigin = "IL";
			invoiceLine4.JI_Procedure = "50";
			invoiceLine4.JI_InvoiceQuantity = 1;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = -108m;
			invoiceLine4.JI_VatPymntMthd = "CAS";
			invoiceLine4.JI_DtyPymntMthd = "CAS";

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = entryInstruction.PK;
			invoiceLine5.JI_Tariff = "84199020006";
			invoiceLine5.JI_PrimaryPreference = "PR1";
			invoiceLine5.JI_CountryOfOrigin = "IL";
			invoiceLine5.JI_Procedure = "50";
			invoiceLine5.JI_InvoiceQuantity = 1;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = -145.8m;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_CEI = entryInstruction.PK;
			invoiceLine6.JI_Tariff = "84199020006";
			invoiceLine6.JI_PrimaryPreference = "PR1";
			invoiceLine6.JI_CountryOfOrigin = "IL";
			invoiceLine6.JI_Procedure = "50";
			invoiceLine6.JI_InvoiceQuantity = 2;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = -160m;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "84199020006";
			invoiceLine7.JI_PrimaryPreference = "PR1";
			invoiceLine7.JI_CountryOfOrigin = "IL";
			invoiceLine7.JI_Procedure = "50";
			invoiceLine7.JI_InvoiceQuantity = 2;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = -88m;

			decl.ResumeApportionment();
			_ = decl.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var entryHeader = decl.EntryHeader;
			entryHeader.MergedLines.Cast<CusEntryLine>().ToList().ForEach(x => x.CL_ValueForVAT = -10000m);

			var charge1 = entryHeader.Charges.AddNew();
			charge1.C1_ChargeAmount = -1250.9m;
			charge1.C1_MethodOfPayment = "CAS";

			var charge2 = entryHeader.Charges.AddNew();
			charge2.C1_ChargeAmount = -1000m;
			charge2.C1_MethodOfPayment = "DEF";

			var testWrapper = new JobDeclarationMessageSendingObjectParent(decl, "ECD");
			var validation = TWMessageSendingValidation.New(testWrapper, null, false);
			var notificationsAsString = validation.CheckBusinessObjectLevelValidation().NotificationsAsString();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Total Inv. Amt. (16) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Freight (17) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Insurance (18) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Additions (19) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Deductions (20) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("FOB (21) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Business Tax Base cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Total Tax Amount (Cash) cannot be negative."));
				NUnit.Framework.Assert.That(notificationsAsString.ToString(), NUnit.Framework.Does.Contain("Total Tax Amount (Non-Cash) cannot be negative."));
			});

			charges.Cast<InvoiceCharge>().ToList().ForEach(x => x.J7_Amount = 10m);
			invoice.JZ_InvoiceAmount = 9000m;
			decl.ResumeApportionment();
			_ = decl.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			entryHeader = decl.EntryHeader;
			entryHeader.MergedLines.Cast<CusEntryLine>().ToList().ForEach(x => x.CL_ValueForVAT = 1000m);

			_ = entryHeader.DutyTaxFeeCharges.ShouldRebuildElements();
			charge1 = entryHeader.Charges.AddNew();
			charge1.C1_ChargeAmount = 1250.9m;
			charge1.C1_MethodOfPayment = "CAS";

			charge2 = entryHeader.Charges.AddNew();
			charge2.C1_ChargeAmount = 1000m;
			charge2.C1_MethodOfPayment = "DEF";

			testWrapper = new JobDeclarationMessageSendingObjectParent(decl, "ICD");
			validation = TWMessageSendingValidation.New(testWrapper, null, false);
			notificationsAsString = validation.CheckBusinessObjectLevelValidation().NotificationsAsString();
			NUnit.Framework.Assert.That(notificationsAsString, NUnit.Framework.Is.EqualTo(ZString.Empty), "Should be empty.");
		}

		[ExpectNoExceptions]
		public void TestCheckBusinessObjectLevelValidationCore()
		{
			declaration.MockValidationMessage = (ZPropertyInfo x) =>
			{
				x.AddMessageError("MessageType Add Message Error");
			}

			;
			validation.CheckBusinessObjectLevelValidation(notifier);
			NUnit.Framework.Assert.That(notifier.ContinueWithActionMessage, NUnit.Framework.Does.Contain("It is likely that your message(s) will be rejected by Customs, as they have the following message errors"));
			NUnit.Framework.Assert.That(notifier.ContinueWithActionMessage, NUnit.Framework.Does.Not.Contain("Do you want to send the message(s) despite these errors?"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclarationForTestSendingObject>();
			validation = TWMessageSendingValidation.New(declaration, null);
			notifier = new SendsMessagesToCustomsShutterUpperer(false);
		}

		JobDeclarationForTestSendingObject declaration;
		TWMessageSendingValidation validation;
		SendsMessagesToCustomsShutterUpperer notifier;
	}
}
