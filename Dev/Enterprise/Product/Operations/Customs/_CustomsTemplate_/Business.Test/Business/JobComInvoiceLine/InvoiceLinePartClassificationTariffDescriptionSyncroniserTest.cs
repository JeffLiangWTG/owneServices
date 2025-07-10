using System;
using CargoWise.Types;

namespace Enterprise.Customs._CustomsTemplate_.Business.Testing
{
	class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		protected override ZString TariffCode => "0000.00.00.00K";

		protected override ZString TariffCode2 => "0000.00.00.01K";

		// to be overridden once the Tariff is setup for a new country
		protected override ZString TariffDescription => "TARIFF_DESCRIPTION";

		protected override ZString TariffDescription2 => "TARIFF_DESCRIPTION";

		protected override Type DeclarationTypeForTest => typeof(JobDeclaration);
	}
}
