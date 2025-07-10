using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business.Testing;

sealed class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
{
	protected override ZString TariffCode => "0000.00.00.00K";

	protected override ZString TariffCode2 => "0000.00.00.01K";

	protected override ZString TariffDescription => ZString.Empty;

	protected override ZString TariffDescription2 => ZString.Empty;

	protected override Type DeclarationTypeForTest => typeof(JobDeclaration);

	protected override BaseJobDeclaration CreateBaseJobDeclarationForMerge()
	{
		var result = base.CreateBaseJobDeclarationForMerge();
		result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		return result;
	}
}
