using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineValidation))]
	class JobComInvoiceLineValidationBaseOnlyTest : JobComInvoiceLineValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

		protected override JobComInvoiceLineValidation GetValidation() => new JobComInvoiceLineValidation(invoiceLine);

		public void TestNonWesternEuropeanEntryLineDescriptionIsAllowed()
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

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			invoiceLine.Validation.ValidateAll();

			var error = invoiceLine.ExposedCusEntryLineErrorPropertyInfo.Notifications.FirstOrDefault(x => x.Type == NotificationType.Error && x.Message.Contains("Entry line description has non-Western European characters that are copied from"));
			AssertNull("There should be no non-Western European characters error preventing Save after Merge", error);
		}
	}
}
