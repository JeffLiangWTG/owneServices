using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class AdditionalLineTariffDetailDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestPopulateBusinessObject()
		{
			var logger = new TestErrorLogger();
			var currentCompanyHelper = new UniversalDataObjectReaderHelper(Factory, CurrentCompany.GC_RN_NKCountryCode, CurrentCompany.GC_RN_NKCountryCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = "BLT";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLineBase = invoice.InvoiceLines.AddNew();
				var invoiceLine = invoiceLineBase as Business.IAdditionalLineTariffDetailParent;
				var tariffDetail1 = invoiceLine.CusLineTariffDetails.AddNew();
				tariffDetail1.BZ_Type = "11A";
				var tariffDetail2 = invoiceLine.CusLineTariffDetails.AddNew();
				tariffDetail2.BZ_Type = "11A";
				tariffDetail2.BZ_Tariff = "Tariff2";
				tariffDetail2.BZ_Value = 51m;

				CombineAssertions(() =>
				{
					var input = new AdditionalLineTariffDetail();
					input.Type = new CodeDescriptionPair5Char() { Code = "11A" };
					input.Tariff = "Tariff2";
					input.Value = 52m;
					var reader = new AdditionalLineTariffDetailDataObjectReader(input, logger, Factory, currentCompanyHelper, invoiceLine);
					var result = reader.ReadIntoBusinessObject();
					AssertEquals(tariffDetail2.PK, result.PK);
					AssertEquals("11A", result.BZ_Type);
					AssertEquals("Tariff2", result.BZ_Tariff);
					AssertEquals(52m, result.BZ_Value);
				});
				CombineAssertions(() =>
				{
					var input = new AdditionalLineTariffDetail();
					input.Type = new CodeDescriptionPair5Char() { Code = "11A" };
					input.Tariff = "Tariff1";
					input.Value = 51m;
					var reader = new AdditionalLineTariffDetailDataObjectReader(input, logger, Factory, currentCompanyHelper, invoiceLine);
					var result = reader.ReadIntoBusinessObject();
					AssertEquals(tariffDetail1.PK, result.PK);
					AssertEquals("11A", result.BZ_Type);
					AssertEquals("Tariff1", result.BZ_Tariff);
					AssertEquals(51m, result.BZ_Value);
				});
				CombineAssertions(() =>
				{
					var input = new AdditionalLineTariffDetail();
					input.Type = new CodeDescriptionPair5Char() { Code = "11B" };
					input.Tariff = "Tariff2";
					input.Value = 49m;
					var reader = new AdditionalLineTariffDetailDataObjectReader(input, logger, Factory, currentCompanyHelper, invoiceLine);
					var result = reader.ReadIntoBusinessObject();
					AssertNotEquals(tariffDetail1.PK, result.PK);
					AssertNotEquals(tariffDetail2.PK, result.PK);
					AssertEquals("11B", result.BZ_Type);
					AssertEquals(invoiceLineBase.PK, result.BZ_ParentID);
					AssertEquals(3, invoiceLine.CusLineTariffDetails.Count);
					AssertEquals("Tariff2", result.BZ_Tariff);
					AssertEquals(49m, result.BZ_Value);
				});
				CombineAssertions(() =>
				{
					var input = new AdditionalLineTariffDetail();
					input.Type = new CodeDescriptionPair5Char() { Code = "11B11" };
					input.Tariff = "Tariff11B11";
					input.Value = 59m;
					var reader = new AdditionalLineTariffDetailDataObjectReader(input, logger, Factory, currentCompanyHelper, invoiceLine);
					var result = reader.ReadIntoBusinessObject();
					AssertNotEquals(tariffDetail1.PK, result.PK);
					AssertNotEquals(tariffDetail2.PK, result.PK);
					AssertEquals("11B11", result.BZ_Type);
					AssertEquals(invoiceLineBase.PK, result.BZ_ParentID);
					AssertEquals(4, invoiceLine.CusLineTariffDetails.Count);
					AssertEquals("Tariff11B11", result.BZ_Tariff);
					AssertEquals(59m, result.BZ_Value);
				});
				CombineAssertions(() =>
				{
					var input = new AdditionalLineTariffDetail();
					input.Tariff = "TariffNoType";
					input.Value = 69m;
					var reader = new AdditionalLineTariffDetailDataObjectReader(input, logger, Factory, currentCompanyHelper, invoiceLine);
					var result = reader.ReadIntoBusinessObject();
					AssertNotEquals(tariffDetail1.PK, result.PK);
					AssertNotEquals(tariffDetail2.PK, result.PK);
					AssertEquals("", result.BZ_Type);
					AssertEquals(invoiceLineBase.PK, result.BZ_ParentID);
					AssertEquals(5, invoiceLine.CusLineTariffDetails.Count);
					AssertEquals("TariffNoType", result.BZ_Tariff);
					AssertEquals(69m, result.BZ_Value);
				});
			}
		}

		public void TestReadBZ_NAddInfoFromAddInfoCollection()
		{
			var logger = new TestErrorLogger();
			var currentCompanyHelper = new UniversalDataObjectReaderHelper(Factory, CurrentCompany.GC_RN_NKCountryCode, CurrentCompany.GC_RN_NKCountryCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew() as Business.IAdditionalLineTariffDetailParent;

				var additionalLineTariffDetail = new AdditionalLineTariffDetail(DefaultDataObjectWriterStrategy.TestInstance);
				additionalLineTariffDetail.SetAddInfoCollection(() => new List<AddInfo>());
				additionalLineTariffDetail.AddInfoCollection.Add(new AddInfo() { Key = BRCusEntryInstructionSchema.Constants.CEI_LegalDocument.Substring(3), Value = "X" });
				additionalLineTariffDetail.AddInfoCollection.Add(new AddInfo() { Key = BRCusLineTariffDetailSchema.Constants.BZ_LegalActSubject.Substring(3), Value = "1" });

				var result = new AdditionalLineTariffDetailDataObjectReader(additionalLineTariffDetail, logger, Factory, currentCompanyHelper, invoiceLine).ReadIntoBusinessObject();
				AssertEquals("read BZ_NAddInfo", "LegalActSubject=1", result.BZ_NAddInfo);
			}
		}

		public GlbCompany CurrentCompany
		{
			get { return currentCompany ?? (currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK)); }
		}
		GlbCompany currentCompany;
	}
}
