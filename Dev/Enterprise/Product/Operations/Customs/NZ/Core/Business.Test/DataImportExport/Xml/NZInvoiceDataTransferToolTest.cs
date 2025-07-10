using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.NZ.Business.Data.Testing
{
	class NZInvoiceDataTransferToolTest
	{
		public NZInvoiceDataTransferToolTest(IValueObjectDataAdapter invoiceDataAdapter, GetInvoiceHeaderDelegate getInvoiceHeader, BusinessObjectFactory factory)
		{
			InvoiceDataAdapter = invoiceDataAdapter;
			GetInvoiceHeader = getInvoiceHeader;
			this.factory = factory;
		}

		public readonly GetInvoiceHeaderDelegate GetInvoiceHeader;
		public readonly IValueObjectDataAdapter InvoiceDataAdapter;
		public delegate BaseJobComInvoiceHeader GetInvoiceHeaderDelegate();
		readonly BusinessObjectFactory factory;

		public void TestImportInvoiceHeaderAdditionalInfo()
		{
			JobComInvoiceHeader invHeader = (JobComInvoiceHeader)GetInvoiceHeader();

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.AdditionalCustomsInformationCollection addCusInfos = xmlInvoiceHeader.AddCustomsDetails;
			Xsd.AdditionalCustomsInformation addCusInfo = addCusInfos.AddNew();

			addCusInfo.CustomsDetailType = "PreferentialCountryGroup";
			addCusInfo.CustomsDetailValue = "LLD";
			TestCaseWithFactory.AssertEquals("PreCondition: JZ_PreferentialCountryGroup", "", invHeader.JZ_PreferentialCountryGroup);

			ValueObjectImportContext context = new ValueObjectImportContext(factory, new NotificationBuffer());
			InvoiceDataAdapter.ImportFromValueObject(invHeader, xmlInvoiceHeader, context);

			TestCaseWithFactory.AssertEquals("LLD", invHeader.JZ_PreferentialCountryGroup);

			addCusInfo.CustomsDetailType = "IsZeroRatedDuty";
			addCusInfo.CustomsDetailValue = "Y";
			TestCaseWithFactory.AssertEquals("PreCondition: JZ_IsZeroRatedDuty", ZBool.False, new ZBool(invHeader.JZ_IsZeroRatedDuty));

			InvoiceDataAdapter.ImportFromValueObject(invHeader, xmlInvoiceHeader, context);
			TestCaseWithFactory.AssertEquals(ZBool.True, new ZBool(invHeader.JZ_IsZeroRatedDuty));
		}

		public void TestImportInvoiceLinesAdditionalInfo()
		{
			JobComInvoiceHeader invHeader = (JobComInvoiceHeader)GetInvoiceHeader();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.AdditionalCustomsInformationCollection addCusInfos = new Xsd.AdditionalCustomsInformationCollection();
			Xsd.AdditionalCustomsInformation addCusInfo = addCusInfos.AddNew();
			xmlInvoiceHeader.InvoiceLines.AddNew().LineClassification.AddCustomsDetails = addCusInfos;

			addCusInfo.CustomsDetailType = "ConcessionCode";
			addCusInfo.CustomsDetailValue = "UT";
			TestCaseWithFactory.AssertEquals("PreCondition: ZN_ConcessionCode", "", invLine.JI_ConcessionCode);

			ValueObjectImportContext context = new ValueObjectImportContext(factory, new NotificationBuffer());
			InvoiceDataAdapter.ImportFromValueObject(invHeader, xmlInvoiceHeader, context);
			TestCaseWithFactory.AssertEquals("UT", invLine.JI_ConcessionCode);

			addCusInfo.CustomsDetailType = "AntiDumpingDuty";
			addCusInfo.CustomsDetailValue = "123";
			TestCaseWithFactory.AssertEquals("PreCondition: ZN_AntiDumpingDuty", (ZDecimal)0, invLine.JI_AntiDumpingDutyAmount);

			InvoiceDataAdapter.ImportFromValueObject(invHeader, xmlInvoiceHeader, context);
			TestCaseWithFactory.AssertEquals((ZDecimal)123, invLine.JI_AntiDumpingDutyAmount);
		}

		public void TestGetXmlInvoiceHeaderAdditionalInfo()
		{
			JobComInvoiceHeader invHeader = (JobComInvoiceHeader)GetInvoiceHeader();

			((IAddInfoManager)invHeader).AddInfo.UpdateAddInfoFromString("PreferentialCountryGroup=LLD*OriginRegion=SPACE*IsZeroRatedDuty=Y");

			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)InvoiceDataAdapter.ExportToValueObject(invHeader, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.AdditionalCustomsInformationCollection addCusInfos = xmlInvoiceHeader.AddCustomsDetails;

			TestCaseWithFactory.Assert("PreferentialCountryGroup should be in collection", ItemInXmlAddInfoCollection(addCusInfos, "PreferentialCountryGroup"));
			TestCaseWithFactory.Assert("OriginRegion should be in collection", ItemInXmlAddInfoCollection(addCusInfos, "OriginRegion"));
			TestCaseWithFactory.Assert("IsZeroRatedDuty should be in collection", ItemInXmlAddInfoCollection(addCusInfos, "IsZeroRatedDuty"));

			TestCaseWithFactory.AssertEquals("PreferentialCountryGroup Value", "LLD", GetXmlAddInfoValue(addCusInfos, "PreferentialCountryGroup"));
			TestCaseWithFactory.AssertEquals("OriginRegion Value", "SPACE", GetXmlAddInfoValue(addCusInfos, "OriginRegion"));
			TestCaseWithFactory.AssertEquals("IsZeroRatedDuty Value", "Y", GetXmlAddInfoValue(addCusInfos, "IsZeroRatedDuty"));
		}

		public void TestExportInvoiceLinesAdditionalInfo()
		{
			JobComInvoiceHeader invHeader = (JobComInvoiceHeader)GetInvoiceHeader();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();

			((IAddInfoManager)invLine).AddInfo.UpdateAddInfoFromString("AntiDumpingDuty=1*ProhibitedCodes=2*OtherInfos=hellotest^keepontesting=yes");
			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)InvoiceDataAdapter.ExportToValueObject(invHeader, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.AdditionalCustomsInformationCollection addCusInfos = xmlInvoiceHeader.InvoiceLines[0].LineClassification.AddCustomsDetails;

			TestCaseWithFactory.Assert("AntiDumpingDuty should be in collection", ItemInXmlAddInfoCollection(addCusInfos, "AntiDumpingDuty"));
			TestCaseWithFactory.Assert("ProhibitedCodes should be in collection", ItemInXmlAddInfoCollection(addCusInfos, "ProhibitedCodes"));
			TestCaseWithFactory.Assert("OtherInfos should be in collection", ItemInXmlAddInfoCollection(addCusInfos, "OtherInfos"));

			TestCaseWithFactory.AssertEquals("AntiDumpingDuty Value", "1", GetXmlAddInfoValue(addCusInfos, "AntiDumpingDuty"));
			TestCaseWithFactory.AssertEquals("ProhibitedCodes Value", "2", GetXmlAddInfoValue(addCusInfos, "ProhibitedCodes"));
			TestCaseWithFactory.AssertEquals("OtherInfos Value", "hellotest^keepontesting=yes", GetXmlAddInfoValue(addCusInfos, "OtherInfos"));
		}

		public void TestImportNZPreferenceCode()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)GetInvoiceHeader();

			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			xmlInvoiceLine.LineClassification = new Xsd.InvoiceLineLineClassification();
			xmlInvoiceLine.LineClassification.Preference = "N";

			ValueObjectImportContext context = new ValueObjectImportContext(factory, new NotificationBuffer());
			InvoiceDataAdapter.ImportFromValueObject(invoiceHeader, xmlInvoiceHeader, context);

			TestCaseWithFactory.AssertEquals("Should set the preference code", "N", invoiceHeader.JobComInvoiceLines[0].JI_QualifiesForPreferentialDuty);
		}

		public void TestExportNZPreferenceCode()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)GetInvoiceHeader();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_QualifiesForPreferentialDuty = "N";

			Xsd.InvoiceHeader xmlInvoiceHeader = (Xsd.InvoiceHeader)InvoiceDataAdapter.ExportToValueObject(invoiceHeader, new ValueObjectExportContext(new NotificationBuffer()));

			TestCaseWithFactory.AssertEquals("Should have imported the single invoice line", 1, xmlInvoiceHeader.InvoiceLines.Count);

			TestCaseWithFactory.AssertEquals("Should set the preference code", "N", xmlInvoiceHeader.InvoiceLines[0].LineClassification.Preference);
		}

		public void TestSetInvoiceLineCharges()
		{
			Xsd.InvoiceHeader xmlInvoiceHeader = new Xsd.InvoiceHeader();
			xmlInvoiceHeader.InvoiceLines = new Xsd.InvoiceLineCollection();
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines.AddNew();

			Xsd.InvoiceChargeCollection charges = new Xsd.InvoiceChargeCollection();
			Xsd.InvoiceCharge charge = charges.AddNew();
			charge.ChargeType = "PAC";
			charge.ChargeValue = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(10), factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD"));
			charge.DutyApplies = Xsd.TrueFalse.@true;
			charge.DutyAppliesSpecified = true;
			charge.GstApplies = Xsd.TrueFalse.@true;
			charge.GstAppliesSpecified = true;

			charge = charges.AddNew();
			charge.ChargeType = "GHS";
			charge.ChargeValue = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(15), factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD"));
			charge.DutyApplies = Xsd.TrueFalse.@false;
			charge.DutyAppliesSpecified = true;
			charge.GstApplies = Xsd.TrueFalse.@false;
			charge.GstAppliesSpecified = true;

			charge = charges.AddNew();
			charge.ChargeType = "COM";
			charge.ChargeValue = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(15), factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD"));
			charge.DutyApplies = Xsd.TrueFalse.@false;
			charge.DutyAppliesSpecified = false;
			charge.GstApplies = Xsd.TrueFalse.@false;
			charge.GstAppliesSpecified = false;

			xmlInvoiceLine.Charges = charges;

			BaseJobComInvoiceHeader invHead = (JobComInvoiceHeader)GetInvoiceHeader();
			BaseJobComInvoiceLine invLine = invHead.JobComInvoiceLines.AddNew();

			ValueObjectImportContext context = new ValueObjectImportContext(factory, new NotificationBuffer());
			InvoiceDataAdapter.ImportFromValueObject(invHead, xmlInvoiceHeader, context);

			TestCaseWithFactory.AssertEquals(3, invLine.Charges.Count);

			TestCaseWithFactory.AssertEquals("PAC", invLine.Charges[0].J7_ChargeType);
			TestCaseWithFactory.AssertEquals((ZDecimal)10, invLine.Charges[0].J7_Amount);
			TestCaseWithFactory.AssertEquals("AUD", invLine.Charges[0].J7_RX_NKCurrency);
			TestCaseWithFactory.Assert(invLine.Charges[0].J7_IsDutiable);
			TestCaseWithFactory.Assert(invLine.Charges[0].J7_IsGSTApplicable);

			TestCaseWithFactory.AssertEquals("GHS", invLine.Charges[1].J7_ChargeType);
			TestCaseWithFactory.AssertEquals((ZDecimal)15, invLine.Charges[1].J7_Amount);
			TestCaseWithFactory.AssertEquals("NZD", invLine.Charges[1].J7_RX_NKCurrency);
			TestCaseWithFactory.Assert(!invLine.Charges[1].J7_IsDutiable);
			TestCaseWithFactory.Assert(!invLine.Charges[1].J7_IsGSTApplicable);//IsIncludedInITOT cannot be set by users as it is readonly on forms
		}

		protected bool ItemInXmlAddInfoCollection(Xsd.AdditionalCustomsInformationCollection addCusInfos, string itemType)
		{
			return Enterprise.Customs.DataTransfer.Testing.AddInfoDataTransferToolTest.ItemInXmlAddInfoCollection(addCusInfos, itemType);
		}

		protected string GetXmlAddInfoValue(Xsd.AdditionalCustomsInformationCollection addCusInfos, string itemType)
		{
			return Enterprise.Customs.DataTransfer.Testing.AddInfoDataTransferToolTest.GetXmlAddInfoValue(addCusInfos, itemType);
		}
	}
}
