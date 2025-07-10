using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class LineMergerTest : EU.Business.Testing.LineMergerTest
	{
		public void TestPerformCountrySpecificOperationAfterMergeAfterCalculateDuty_CL_Description()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "220860990000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Muhtevası 2 litreyi geçen kaplarda olanlar");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "TRF";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.JI_Description = "English Description";
			var expected = new string('ç', CusEntryLineSchema.CL_Description.MaxLength);
			var input = $" {expected} 123 ";
			invoiceLine.JI_Description = "English Description";
			invoiceLine.JI_NDescription = input;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("After merge", expected, invoiceLine.CusEntryLine.CL_Description);
		}

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new Declaration.LineMerger((JobDeclaration)declaration);

		protected override Type GetSupportingDocumentType() => typeof(SupportingDocument);

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override Type[] ExpectedEntryCreationStrategiesType => new Type[1] { typeof(Declaration.EntryCreationStrategy) };
	}
}
