using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocARInvoice))]
	internal class USDocARInvoiceTest : DocARInvoiceCommonTest
	{
		public void TestInvoiceTypeReferenceNumberHeadingForISF()
		{
			Invoice.AH_ConsolidatedInvoiceRef = ZString.Empty;
			AssertEquals("InvoiceTypeReferenceNumberHeading", ZString.Empty, ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);

			Invoice.AH_ConsolidatedInvoiceRef = "ISF0000120";
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "ISF0000120";
			JobHeader job = GetInvoiceJob(header, Invoice);
			AssertEquals("InvoiceTypeReferenceNumberHeading", "ISF JOB:", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeading);
			AssertEquals("InvoiceTypeReferenceNumberHeadingNoColon", "ISF Job", ARInvoiceWrapper.InvoiceTypeReferenceNumberHeadingNoColon);
			AssertEquals("InvoiceTypeReferenceNumber", "ISF0000120", ARInvoiceWrapper.InvoiceTypeReferenceNumber);
		}

		public void TestLinesForCusISFHeaderInvoice()
		{
			CusISFHeader isfHeader = Factory.New<CusISFHeader>();
			JobHeader isfJob = GetInvoiceJob(isfHeader, Invoice);
			isfJob.JH_JobNum = "00000000";
			((InvoicingBase)Invoice).Lines.AddNew();
			((InvoicingBase)Invoice).Lines[0].AL_JH = isfJob.PK;

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			var linesForCusISFHeaderInvoice = ((DocARInvoice)InvoiceWrapper).LinesForPeriodicInvoice;
			AssertEquals(1, linesForCusISFHeaderInvoice.Count);
			AssertEquals("00000000", linesForCusISFHeaderInvoice[0].JobHeader.JobNumber);
		}

		public void TestISFInvoiceLine()
		{
			CusISFHeader iSFHEader = Factory.New<CusISFHeader>();
			JobHeader job = GetInvoiceJob(iSFHEader, Invoice);
			((InvoicingBase)Invoice).Lines.AddNew();
			((InvoicingBase)Invoice).Lines[0].AL_JH = job.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals(1, ((DocARInvoice)InvoiceWrapper).ISFInvoiceLine.Count);
		}

		public new void TestRecipientTaxID_LoginCountryNotEqualRecipientCountryOfRegistration()
		{
			var storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				var header = Factory.New<OrgHeader>();
				header.OH_RL_NKClosestPort = "GBLON";
				header.CustomsCodes.RemoveAll();

				var taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				taxCode.OK_CodeType = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom);
				taxCode.OK_CustomsRegNo = "123456";

				ARInvoice.AH_OH = header.PK;
				InvoicingLineBase line2 = (InvoicingLineBase)ARInvoice.Lines.AddNew();
				AccTaxRate rate = base.Factory.LoadTop1<AccTaxRate>(new ZQuery());
				line2.AL_AT = rate.PK;

				InvoiceWrapper = DocARInvoice.New(ARInvoice, Factory);

				//In the Same Country
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				AssertEquals("Recipient Tax ID Heading for a United Kingdom Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a United Kingdom Company", "GB123456", InvoiceWrapper.RecipientTaxIDNumber);

				//In Another ENU
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Germany);
				header.ResetCodeForTaxRegistration_ForTestOnly();

				AssertEquals("Recipient Tax ID Heading for a German Company", "Client VAT ID No:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for another ENU Company", "GB123456", InvoiceWrapper.RecipientTaxIDNumber);

				//Out of ENU
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
				header.ResetCodeForTaxRegistration_ForTestOnly();

				AssertEquals("Recipient Tax ID Heading for a New Zealand Company", "Client GST #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Should be empty because New Zealand is not in ENU", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public new void TestRecipientTaxIDWithoutTax()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader header = Factory.New<OrgHeader>();
				header.OH_Code = "ORG1";
				header.MiscServ.OM_ARDontShowTaxOnDocs = ZBool.True;

				OrgCusCode taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
				taxCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Netherlands);
				taxCode.OK_CustomsRegNo = "123456";

				taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				taxCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Australia);
				taxCode.OK_CustomsRegNo = "654321";

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Netherlands);
				AssertEquals("PreCondition: Local VAT Code", "123456", header.RawTaxRegistrationNumber);
				AssertEquals("PreCondition: is not taxed", ZBool.True, header.MiscServ.OM_ARDontShowTaxOnDocs);

				ARInvoice.AH_OH = header.PK;

				InvoicingLineBase line2 = (InvoicingLineBase)ARInvoice.Lines.AddNew();
				AccTaxRate rate = base.Factory.LoadTop1<AccTaxRate>(new ZQuery());
				line2.AL_AT = rate.PK;

				InvoiceWrapper = DocARInvoice.New(ARInvoice, Factory);

				AssertEquals("Recipient Tax ID Heading when Current Company is Netherlands", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when Current Company is Netherlands", "NL123456", InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("654321", header.RawTaxRegistrationNumber);
				AssertEquals("Recipient Tax ID Heading when Current Company is Australia", ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when Current Company is Australia", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		protected override DocARBaseInvoice GetBaseInvoiceWrapper()
		{
			return DocARInvoice.New(base.Invoice, base.Factory);
		}

		protected override TransactionHeader GetWrappedInvoice()
		{
			return Factory.New<ARInvoice>();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocARInvoice.New(Invoice, Factory), };
		}

		protected DocARInvoice ARInvoiceWrapper
		{
			get { return (DocARInvoice)base.InvoiceWrapper; }
		}

		protected override void SetUp()
		{
			Invoice = (InvoicingBase)GetWrappedInvoice();
			base.SetUp();
		}

		ARInvoice ARInvoice
		{
			get { return Invoice as ARInvoice; }
		}
	}
}
